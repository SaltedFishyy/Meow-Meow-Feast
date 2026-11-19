using System.Collections.Generic;
using AbilityStudio.Editor;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbilityStudio.Tests
{
    public sealed class HealingTests
    {
        private sealed class Clock : ISkillClock { public double Time => 0; }
        private sealed class Actor : ISkillCaster, ISkillTarget, IDamageable, IHealable
        {
            public Vector3 Position { get; set; }
            public Quaternion Rotation => Quaternion.identity;
            public int Team { get; set; }
            public float Radius => .1f;
            public bool IsAlive => CurrentHp > 0;
            public float CurrentHp { get; set; } = 20;
            public float MaxHp => 100;
            public float Armor => 100;
            public void ApplyDamage(float amount, ISkillCaster source) => CurrentHp -= amount;
            public void ApplyHealing(float amount, ISkillCaster source) => CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        }
        private readonly List<Object> _objects = new List<Object>();
        [TearDown] public void Cleanup() { foreach (var obj in _objects) Object.DestroyImmediate(obj); _objects.Clear(); }
        private SkillDefinition Skill(AttackModule attack, HealEffect heal)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>(); _objects.Add(skill);
            skill.SetAttack(attack); skill.BaseDamage = 40; skill.Cooldown = 0; skill.Distance = 20;
            attack.DamageMode = DamageMode.ConnectedDamage;
            skill.AddEffect(heal); skill.SetBinding(attack.ModuleId, heal.ModuleId, BindingRules.Targets);
            return skill;
        }
        [TestCase(0, 10, 30)] [TestCase(1, 50, 40)] [TestCase(2, 50, 70)]
        public void AreaHealsAlliesAndUnregisteredCasterOnly(int mode, float amount, float expected)
        {
            var skill = Skill(new CircleAttack { Radius = 3 }, new HealEffect { Mode = mode, Amount = amount });
            var self = new Actor(); var friend = new Actor { Position = Vector3.right };
            var enemy = new Actor { Team = 1 }; var outside = new Actor { Position = Vector3.right * 10 };
            var targets = new ListTargetProvider(); targets.Add(friend); targets.Add(enemy); targets.Add(outside);
            var executor = new SkillExecutor(new Clock(), targets);
            Assert.That(executor.TryCast(skill, self, Vector3.zero), Is.EqualTo(CastResult.Success));
            Assert.That(self.CurrentHp, Is.EqualTo(expected)); Assert.That(friend.CurrentHp, Is.EqualTo(expected));
            Assert.That(enemy.CurrentHp, Is.EqualTo(20)); Assert.That(outside.CurrentHp, Is.EqualTo(20));
        }
        [Test] public void MixedEffectsNeverDamageAlliesOrHealEnemies()
        {
            var skill = Skill(new CircleAttack { Radius = 3 }, new HealEffect { Amount = 10 });
            var damage = new DamageEffect(); skill.AddEffect(damage);
            skill.SetBinding(skill.Attack.ModuleId, damage.ModuleId, BindingRules.Targets);
            var self = new Actor(); var enemy = new Actor { Team = 1, CurrentHp = 70 };
            var targets = new ListTargetProvider(); targets.Add(self); targets.Add(enemy);
            Assert.That(new SkillExecutor(new Clock(), targets).TryCast(skill, self, Vector3.zero), Is.EqualTo(CastResult.Success));
            Assert.That(self.CurrentHp, Is.EqualTo(30)); Assert.That(enemy.CurrentHp, Is.EqualTo(50));
        }
        [Test] public void ProjectileSnapshotsHealingParameters()
        {
            var heal = new HealEffect { Amount = 10 };
            var skill = Skill(new ProjectileAttack { Speed = 10, Range = 10, Size = .1f }, heal);
            var self = new Actor(); var targets = new ListTargetProvider();
            var executor = new SkillExecutor(new Clock(), targets);
            Assert.That(executor.TryCast(skill, self, Vector3.right * 5), Is.EqualTo(CastResult.Success));
            heal.Amount = 99; executor.Tick(.1f);
            Assert.That(self.CurrentHp, Is.EqualTo(30));
        }
        [Test] public void CombatHealthCapsAndDoesNotReviveOrAcceptInvalidHealing()
        {
            var go = new GameObject("Health test"); _objects.Add(go);
            var target = go.AddComponent<CombatTarget>(); target.ResetTarget();
            target.ApplyDamage(5, null); target.ApplyHealing(float.NaN, null); target.ApplyHealing(-10, null);
            Assert.That(target.CurrentHp, Is.EqualTo(target.MaxHp - 5));
            target.ApplyHealing(float.MaxValue, null); Assert.That(target.CurrentHp, Is.EqualTo(target.MaxHp));
            target.ApplyDamage(target.MaxHp, null); target.ApplyHealing(10, null); Assert.That(target.CurrentHp, Is.Zero);
        }
        [Test] public void CatalogAndValidationSupportHealing()
        {
            Assert.That(ModuleCatalog.Types, Does.Contain(typeof(HealEffect)));
            var heal = new HealEffect { Amount = 10 }; var skill = Skill(new CircleAttack(), heal);
            Assert.That(SkillValidator.Validate(skill), Is.Empty);
            heal.Mode = 3; Assert.That(SkillValidator.HasErrors(skill), Is.True);
            heal.Mode = 0; heal.Amount = float.PositiveInfinity; Assert.That(SkillValidator.HasErrors(skill), Is.True);
        }
        [Test] public void DesignerDropdownUpdatesAndSerializesAllThreeModes()
        {
            var heal = new HealEffect { Amount = 25 }; var skill = Skill(new CircleAttack(), heal);
            var fields = SkillParameterFields.Create(skill, "effects.Array.data[0]");
            var window = ScriptableObject.CreateInstance<UnityEditor.EditorWindow>(); _objects.Add(window);
            window.Show(); window.rootVisualElement.Add(fields);
            var dropdown = fields.Q<DropdownField>("effects.Array.data[0].mode");
            Assert.That(dropdown.choices.Count, Is.EqualTo(3));
            for (int mode = 0; mode < 3; mode++)
            {
                dropdown.value = dropdown.choices[mode]; Assert.That(((HealEffect)skill.Effects[0]).Mode, Is.EqualTo(mode));
                var copy = ScriptableObject.CreateInstance<SkillDefinition>(); _objects.Add(copy);
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(skill), copy);
                Assert.That(((HealEffect)copy.Effects[0]).Mode, Is.EqualTo(mode));
                Assert.That(((HealEffect)copy.Effects[0]).Amount, Is.EqualTo(25));
            }
        }
        [TestCase(0)] [TestCase(1)]
        public void ConeAndRectangleHealInsideAlliesOnly(int shape)
        {
            AttackModule attack = shape == 0 ? (AttackModule)new ConeAttack { Range = 5, AngleDegrees = 60 }
                : new LineThrustAttack { Length = 5, Width = 2 };
            var skill = Skill(attack, new HealEffect { Amount = 10 });
            var self = new Actor(); var ally = new Actor { Position = Vector3.right * 3 };
            var outside = new Actor { Position = Vector3.up * 10 }; var targets = new ListTargetProvider();
            targets.Add(ally); targets.Add(outside);
            Assert.That(new SkillExecutor(new Clock(), targets).TryCast(skill, self, Vector3.right * 5), Is.EqualTo(CastResult.Success));
            Assert.That(ally.CurrentHp, Is.EqualTo(30)); Assert.That(outside.CurrentHp, Is.EqualTo(20));
        }
        [Test] public void PreviewCasterCanReceiveHealingAndReset()
        {
            using var world = new PlaygroundSimulation();
            var skill = Skill(new CircleAttack { Radius = 2 }, new HealEffect { Amount = 3 });
            world.Bind(skill); world.Health.ApplyDamage(5, world); world.SetAim(world.Position);
            Assert.That(world.Press(SkillKind.NormalAttack), Is.EqualTo(CastResult.Success));
            Assert.That(world.Health.CurrentHp, Is.EqualTo(world.MaxHp - 2));
            world.Reset(); Assert.That(world.Health.CurrentHp, Is.EqualTo(world.MaxHp));
        }
    }
}
