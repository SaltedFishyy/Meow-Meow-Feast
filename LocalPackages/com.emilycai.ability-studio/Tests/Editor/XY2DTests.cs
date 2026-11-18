using System.Collections.Generic;
using System.Linq;
using AbilityStudio.Editor;
using AbilityStudio.Model;
using AbilityStudio.Playground;
using AbilityStudio.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AbilityStudio.Tests
{
    public sealed class XY2DTests
    {
        private sealed class Caster : ISkillCaster
        {
            public Vector3 Position { get; set; } = new Vector3(0, 0, 7);
            public Quaternion Rotation { get; set; } = Quaternion.identity;
            public int Team => 0;
        }
        private sealed class Clock : ISkillClock { public double Time => 0; }
        private sealed class Target : ISkillTarget, IDamageable, IDisplaceable
        {
            public Vector3 Position { get; set; }
            public float Radius => .1f;
            public int Team { get; set; } = 1;
            public bool IsAlive => CurrentHp > 0;
            public float CurrentHp { get; private set; } = 100;
            public float MaxHp => 100;
            public float Armor => 0;
            public void ApplyDamage(float amount, ISkillCaster source) => CurrentHp -= amount;
            public void Displace(Vector3 delta, float duration) => Position += delta;
        }
        private readonly List<Object> _objects = new List<Object>();
        private SkillDefinition Skill(AttackModule attack)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>(); _objects.Add(skill);
            skill.SetAttack(attack); skill.BaseDamage = 10; skill.Cooldown = 0; skill.Distance = 20;
            return skill;
        }
        private GameObject Go(string name) { var go = new GameObject(name); _objects.Add(go); return go; }
        [TearDown] public void Cleanup()
        {
            foreach (var obj in _objects) if (obj != null) Object.DestroyImmediate(obj);
            _objects.Clear();
        }

        [TestCase(0, 0)] [TestCase(0, 90)] [TestCase(0, 225)]
        [TestCase(1, 0)] [TestCase(1, 90)] [TestCase(1, 225)]
        [TestCase(2, 0)] [TestCase(2, 90)] [TestCase(2, 225)]
        [TestCase(3, 0)] [TestCase(3, 90)] [TestCase(3, 225)]
        public void AttacksHitInXYAndIgnoreRenderingDepth(int shape, float angle)
        {
            AttackModule attack = shape == 0 ? new ProjectileAttack { Speed = 20, Range = 10, Size = .1f }
                : shape == 1 ? new ConeAttack { Range = 8, AngleDegrees = 60 }
                : shape == 2 ? new LineThrustAttack { Length = 8, Width = 1 }
                : (AttackModule)new CircleAttack { Radius = 1 };
            var skill = Skill(attack); var caster = new Caster();
            var direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
            var hit = new Target { Position = direction * 5 + Vector3.forward * -30 };
            var miss = new Target { Position = direction * 5 + new Vector3(-direction.y, direction.x, 0) * 6 };
            var friend = new Target { Position = hit.Position, Team = 0 };
            var targets = new ListTargetProvider(); targets.Add(hit); targets.Add(miss); targets.Add(friend);
            var executor = new SkillExecutor(new Clock(), targets);
            Assert.That(executor.TryCast(skill, caster, direction * 5 + Vector3.forward * 40), Is.EqualTo(CastResult.Success));
            executor.Tick(1);
            Assert.That(hit.CurrentHp, Is.EqualTo(90)); Assert.That(miss.CurrentHp, Is.EqualTo(100));
            Assert.That(friend.CurrentHp, Is.EqualTo(100));
        }

        [Test] public void EndpointAndLocalOffsetKeepCasterDepth()
        {
            var skill = Skill(new ProjectileAttack()); skill.Distance = 5;
            skill.CastOriginOffset = new Vector3(2, 0, 99);
            var caster = new Caster { Rotation = Quaternion.Euler(0, 0, 90) };
            Assert.That(Vector3.Distance(SkillPositions.CastOrigin(skill, caster), new Vector3(0, 2, 7)), Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(SkillPositions.Endpoint(skill, caster, new Vector3(0, 20, -90)), new Vector3(0, 7, 7)), Is.LessThan(.0001f));
            Assert.That(SkillPositions.Resolve(new MousePosition(), skill, caster, new Vector3(3, 9, -50)), Is.EqualTo(new Vector3(3, 9, 7)));
        }

        [Test] public void PullAndKnockbackMoveYWithoutChangingZ()
        {
            var caster = new Caster(); var target = new Target { Position = new Vector3(0, 5, -12) };
            var context = new EffectContext(caster, target, target.Position, Vector3.up * 8);
            new PullEffectHandler().Apply(new PullEffect { StopDistance = 1 }, context);
            Assert.That(target.Position, Is.EqualTo(new Vector3(0, 1, -12)));
            new KnockbackEffectHandler().Apply(new KnockbackEffect { Distance = 3 }, context);
            Assert.That(target.Position, Is.EqualTo(new Vector3(0, 4, -12)));
            target.Position = new Vector3(0, 0, -12); caster.Rotation = Quaternion.Euler(0, 0, 90);
            new KnockbackEffectHandler().Apply(new KnockbackEffect { Distance = 2 }, context);
            Assert.That(Vector3.Distance(target.Position, new Vector3(0, 2, -12)), Is.LessThan(.0001f));
        }

        [Test] public void PierceChainReturnFollowXYAndHitAgainOnReturn()
        {
            var skill = Skill(new ProjectileAttack { Speed = 10, Range = 10, Size = .1f });
            var pierce = new PierceResponse { Count = 1 }; var chain = new ChainResponse { Count = 1, Distance = 5 };
            var back = new ReturnResponse { SpeedMultiplier = 1 };
            foreach (var response in new HitResponseModule[] { pierce, chain, back })
            { skill.AddResponse(response); skill.SetBinding(skill.Attack.ModuleId, response.ModuleId, BindingRules.Projectile); }
            var a = new Target { Position = new Vector3(0, 2, 0) };
            var b = new Target { Position = new Vector3(0, 4, -4) };
            var c = new Target { Position = new Vector3(3, 4, 8) };
            var targets = new ListTargetProvider(); targets.Add(a); targets.Add(b); targets.Add(c);
            var executor = new SkillExecutor(new Clock(), targets); var hits = new List<ISkillTarget>();
            executor.Hit += (shot, target) => hits.Add(target);
            Assert.That(executor.TryCast(skill, new Caster(), Vector3.up * 8), Is.EqualTo(CastResult.Success));
            executor.Tick(10);
            Assert.That(hits.Take(3), Is.EqualTo(new ISkillTarget[] { a, b, c }));
            Assert.That(hits.Count(t => t == c), Is.EqualTo(2));
            Assert.That(executor.Projectiles, Is.Empty);
        }

        [Test] public void ChargedRectangleTipAndPreviewAgreeInXY()
        {
            var attack = new LineThrustAttack { Length = 8, MinLength = 2, Width = 2, TipLength = 1, ReleaseMode = 1 };
            var skill = Skill(attack); skill.IntegratedAttacks = true;
            attack.Settings.Duration = 2; attack.Settings.BaseDamage = 10;
            skill.SetBinding(SkillDefinition.CoreLayoutId, attack.ModuleId, BindingRules.Trigger, BindingRules.Normal);
            var multiplier = new DamageEffect { MultiplierPercent = 150 };
            attack.DamageMode = DamageMode.ConnectedDamage;
            skill.AddEffect(multiplier); skill.SetBinding(attack.ModuleId, multiplier.ModuleId, BindingRules.Targets, BindingRules.TipTargets);
            var tip = new Target { Position = new Vector3(0, 4.5f, 10) };
            var body = new Target { Position = new Vector3(0, 2, -10) };
            var targets = new ListTargetProvider(); targets.Add(tip); targets.Add(body);
            var executor = new SkillExecutor(new Clock(), targets); var session = new SkillReleaseSession(executor);
            Assert.That(session.Press(skill, new Caster(), Vector3.up * 10), Is.EqualTo(CastResult.Charging));
            session.Tick(1);
            Assert.That(SkillChargeGeometry.Length(skill, attack, session.ChargeRatio), Is.EqualTo(5));
            Assert.That(session.Release(Vector3.up * 10), Is.EqualTo(CastResult.Success));
            Assert.That(tip.CurrentHp, Is.EqualTo(85)); Assert.That(body.CurrentHp, Is.EqualTo(100));
            for (int i = 0; i < 4; i++)
                Assert.That(RectangleGeometry.Corner(new Vector3(0, 0, 7), Vector3.up, 5, 2, i).z, Is.EqualTo(7));
            Assert.That(ConeGeometry.ArcPoint(new Vector3(0, 0, 7), Vector3.up, 5, 60, .5f), Is.EqualTo(new Vector3(0, 5, 7)));
        }

        [Test] public void KeyboardMovementAndFacingStayInXY()
        {
            var player = Go("Player").AddComponent<PlaygroundPlayer>();
            var camera = Go("Camera").AddComponent<Camera>(); camera.transform.position = new Vector3(0, 0, -20);
            var host = Go("Host").AddComponent<SkillRuntimeHost>();
            player.Configure(Skill(new ProjectileAttack()), host, camera);
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W)); InputSystem.Update();
                player.TickInput(.5f);
                Assert.That(player.Position.y, Is.GreaterThan(0)); Assert.That(player.Position.z, Is.Zero);
                Assert.That(Vector3.Distance(player.transform.forward, Vector3.forward), Is.LessThan(.0001f));
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }

        [Test] public void RecoilOnlyRotatesVisualInScreenPlaneAndResets()
        {
            var root = Go("Target"); var pivot = Go("Visual"); pivot.transform.SetParent(root.transform);
            var recoil = pivot.AddComponent<PlaygroundRecoil>(); var feedback = new RecoilFeedback();
            recoil.Play(feedback, Vector3.right); recoil.Tick(feedback.LeanDuration);
            Assert.That(Quaternion.Angle(pivot.transform.localRotation, Quaternion.identity), Is.EqualTo(feedback.Angle).Within(.001f));
            Assert.That(Vector3.Distance(pivot.transform.forward, Vector3.forward), Is.LessThan(.0001f));
            Assert.That(root.transform.rotation, Is.EqualTo(Quaternion.identity));
            recoil.ResetPose(); Assert.That(pivot.transform.localRotation, Is.EqualTo(Quaternion.identity));
        }

        [Test] public void SceneGeneratorIs2DAndDoesNotOverwriteExistingScene()
        {
            PrototypeSceneCommands.CreateMissingAssets();
            Assert.That(PrototypeSceneCommands.PlaygroundPath, Does.Contain("Playground2D"));
            var before = System.IO.File.ReadAllBytes(PrototypeSceneCommands.PlaygroundPath);
            PrototypeSceneCommands.CreateMissingAssets();
            Assert.That(System.IO.File.ReadAllBytes(PrototypeSceneCommands.PlaygroundPath), Is.EqualTo(before));
            EditorSceneManager.OpenScene(PrototypeSceneCommands.PlaygroundPath);
            var camera = Object.FindAnyObjectByType<Camera>();
            Assert.That(camera.orthographic, Is.True); Assert.That(camera.transform.forward, Is.EqualTo(Vector3.forward));
            Assert.That(Object.FindObjectsByType<Collider>(), Is.Empty);
            var player = Object.FindAnyObjectByType<PlaygroundPlayer>();
            Assert.That(player.Position, Is.EqualTo(new Vector3(0, -5, 0)));
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test] public void TongsStillLoadsWithoutAssetConversion()
        {
            var path = "Assets/_MMF/Data/Weapons/Tongs.asset";
            var before = System.IO.File.ReadAllBytes(path);
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            Assert.That(skill, Is.Not.Null); Assert.That(skill.Attack, Is.TypeOf<ProjectileAttack>());
            Assert.That(skill.Effects.OfType<PullEffect>().Any(), Is.True);
            Assert.That(SkillValidator.Validate(skill), Is.Empty);
            Assert.That(System.IO.File.ReadAllBytes(path), Is.EqualTo(before));
        }
    }
}
