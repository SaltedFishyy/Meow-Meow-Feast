using System;
using System.Linq;
using AbilityStudio.Editor;
using AbilityStudio.Model;
using AbilityStudio.Playground;
using AbilityStudio.Runtime;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace AbilityStudio.Tests
{
    public sealed class ReleaseGraphTests
    {
        private SkillDefinition _skill;
        private sealed class Clock : ISkillClock { public double Time { get; set; } }
        private sealed class Caster : ISkillCaster
        { public Vector3 Position => Vector3.zero; public Quaternion Rotation => Quaternion.identity; public int Team => 0; }
        private sealed class Target : ISkillTarget, IDamageable
        {
            public Vector3 Position { get; set; } = Vector3.right * 3;
            public float Radius => .1f; public int Team => 1; public bool IsAlive => CurrentHp > 0;
            public float CurrentHp { get; private set; } = 100; public float MaxHp => 100; public float Armor => 0;
            public void ApplyDamage(float amount, ISkillCaster caster) => CurrentHp -= amount;
        }
        [SetUp] public void Setup() { _skill = ScriptableObject.CreateInstance<SkillDefinition>(); }
        [TearDown] public void Cleanup() { UnityEditor.Undo.ClearUndo(_skill); UnityEngine.Object.DestroyImmediate(_skill); }
        private ReleaseModule Upgrade(AttackModule attack)
        { _skill.SetAttack(attack); SkillGraphMigration.Upgrade(_skill); return _skill.ReleaseFor(SkillKind.NormalAttack); }
        [Test] public void LegacyUpgradeMovesParametersAndReversesReleaseConnectionOnce()
        {
            _skill.BaseDamage = 7; _skill.Cooldown = 3; _skill.Distance = 9; _skill.CastOriginOffset = new Vector3(2, 1, 0);
            var charge = new ChargeRelease { Duration = 2 }; _skill.AddRelease(charge);
            _skill.SetBinding(charge.ModuleId, SkillDefinition.CoreLayoutId, BindingRules.Release);
            var area = new CircleAttack { Radius = 3 };
            var release = Upgrade(area);
            Assert.That(release, Is.SameAs(charge)); Assert.That(_skill.AreaGraph, Is.True); Assert.That(_skill.DualAttack, Is.False);
            Assert.That(area.Settings.BaseDamage, Is.EqualTo(7)); Assert.That(area.Settings.Cooldown, Is.EqualTo(3));
            Assert.That(area.Settings.Distance, Is.EqualTo(9)); Assert.That(area.CastOriginOffset, Is.EqualTo(new Vector3(2, 1, 0)));
            Assert.That(_skill.BindingSource(charge.ModuleId, BindingRules.Trigger), Is.EqualTo(SkillDefinition.CoreLayoutId));
            Assert.That(_skill.BindingSource(area.ModuleId, BindingRules.Cast), Is.EqualTo(charge.ModuleId));
            Assert.That(_skill.BindingSource(SkillDefinition.CoreLayoutId, BindingRules.Release), Is.Null);
            area.Settings.BaseDamage = 17; Assert.That(SkillGraphMigration.Upgrade(_skill), Is.False); Assert.That(area.Settings.BaseDamage, Is.EqualTo(17));
        }
        [Test] public void IntegratedChargeUpgradePreservesChargeAndParameters()
        {
            var area = new LineThrustAttack { ReleaseMode = 1, Length = 6, MinLength = 2 };
            area.Settings.BaseDamage = 12; area.Settings.Cooldown = 4; area.Settings.Duration = 2;
            _skill.SetAttack(area); _skill.IntegratedAttacks = true;
            _skill.SetBinding(SkillDefinition.CoreLayoutId, area.ModuleId, BindingRules.Trigger, BindingRules.Normal);
            SkillGraphMigration.Upgrade(_skill);
            Assert.That(_skill.IntegratedAttacks, Is.False);
            Assert.That(_skill.ReleaseFor(SkillKind.NormalAttack), Is.TypeOf<ChargeRelease>());
            Assert.That(((ChargeRelease)_skill.ReleaseFor(SkillKind.NormalAttack)).Duration, Is.EqualTo(2));
            Assert.That(area.Settings.BaseDamage, Is.EqualTo(12));
            Assert.That(SkillChargeGeometry.Length(_skill, area, .5f), Is.EqualTo(4));
        }
        [Test] public void AreaDamageAndCooldownAreIndependentOfHiddenCoreAndReleaseValues()
        {
            var a = new CircleAttack { Radius = 1 }; var release = Upgrade(a);
            var b = new CircleAttack { Radius = 1 }; _skill.AddAttack(b); _skill.SetBinding(release.ModuleId, b.ModuleId, BindingRules.Cast);
            _skill.SetBinding(SkillDefinition.CoreLayoutId, a.ModuleId, BindingRules.Position);
            _skill.SetBinding(SkillDefinition.CoreLayoutId, b.ModuleId, BindingRules.Position);
            a.Settings.BaseDamage = 10; a.Settings.Cooldown = 5; b.Settings.BaseDamage = 20; b.Settings.Cooldown = 2;
            _skill.BaseDamage = 999; _skill.Cooldown = 999; release.BaseDamage = 999; release.Cooldown = 999;
            var target = new Target(); var provider = new ListTargetProvider(); provider.Add(target); var clock = new Clock();
            var executor = new SkillExecutor(clock, provider); var caster = new Caster();
            Assert.That(executor.TryCast(_skill, caster, target.Position), Is.EqualTo(CastResult.Success)); Assert.That(target.CurrentHp, Is.EqualTo(70));
            Assert.That(executor.TryCast(_skill, caster, target.Position), Is.EqualTo(CastResult.OnCooldown));
            clock.Time = 2; Assert.That(executor.TryCast(_skill, caster, target.Position), Is.EqualTo(CastResult.Success)); Assert.That(target.CurrentHp, Is.EqualTo(50));
            clock.Time = 5; Assert.That(executor.TryCast(_skill, caster, target.Position), Is.EqualTo(CastResult.Success)); Assert.That(target.CurrentHp, Is.EqualTo(20));
        }
        [Test] public void AreaOffsetAndMaximumDistanceDriveActualHitPosition()
        {
            var area = new CircleAttack { Radius = .5f }; Upgrade(area);
            _skill.SetBinding(SkillDefinition.CoreLayoutId, area.ModuleId, BindingRules.Position);
            area.CastOriginOffset = Vector3.right * 2; area.Settings.Distance = 2; area.Settings.BaseDamage = 6;
            _skill.CastOriginOffset = Vector3.up * 99; _skill.Distance = 99;
            var target = new Target { Position = Vector3.right * 4 }; var provider = new ListTargetProvider(); provider.Add(target);
            Assert.That(new SkillExecutor(new Clock(), provider).TryCast(_skill, new Caster(), Vector3.right * 20), Is.EqualTo(CastResult.Success));
            Assert.That(target.CurrentHp, Is.EqualTo(94));
        }
        [Test] public void ReleasePositionBindingMovesAreaOriginAndKeepsOffset()
        {
            var area = new LineThrustAttack(); Upgrade(area);
            area.CastOriginOffset = Vector3.up;
            var caster = new Caster(); var aim = Vector3.right * 4;
            Assert.That(SkillPositions.AreaOrigin(_skill, area, caster, aim), Is.EqualTo(Vector3.up));
            var mouse = new MousePosition(); _skill.AddPosition(mouse);
            Assert.That(BindingRules.CanConnect(_skill, mouse.ModuleId, area.ModuleId, BindingRules.Origin), Is.True);
            _skill.SetBinding(mouse.ModuleId, area.ModuleId, BindingRules.Origin);
            Assert.That(SkillPositions.AttackPosition(_skill, area, caster, aim), Is.EqualTo(aim + Vector3.up));
            var self = new SelfPosition(); _skill.AddPosition(self);
            _skill.SetBinding(self.ModuleId, area.ModuleId, BindingRules.Origin);
            Assert.That(SkillPositions.AttackPosition(_skill, area, caster, aim), Is.EqualTo(Vector3.up));
        }
        [Test] public void HiddenLegacyPositionCannotOverrideVisibleReleasePosition()
        {
            foreach (var area in new AttackModule[] { new ProjectileAttack(), new LineThrustAttack(), new ConeAttack() })
            {
                Upgrade(area);
                var caster = new Caster(); var aim = Vector3.right * 4;
                _skill.SetBinding(SkillDefinition.CoreLayoutId, area.ModuleId, BindingRules.Position);
                Assert.That(SkillPositions.AttackPosition(_skill, area, caster, aim), Is.EqualTo(caster.Position));
                var mouse = new MousePosition(); _skill.AddPosition(mouse);
                _skill.SetBinding(mouse.ModuleId, area.ModuleId, BindingRules.Origin);
                Assert.That(SkillPositions.AttackPosition(_skill, area, caster, aim), Is.EqualTo(aim));
            }
        }
        [Test] public void EveryAreaDefaultsToPlayerAndUsesExplicitReleasePosition()
        {
            foreach (var area in new AttackModule[] { new CircleAttack(), new ProjectileAttack(), new LineThrustAttack(), new ConeAttack() })
            {
                Upgrade(area); area.CastOriginOffset = Vector3.up;
                var caster = new Caster(); var aim = Vector3.right * 4;
                Assert.That(SkillPositions.AttackPosition(_skill, area, caster, aim), Is.EqualTo(Vector3.up));
                var mouse = new MousePosition(); _skill.AddPosition(mouse);
                _skill.SetBinding(mouse.ModuleId, area.ModuleId, BindingRules.Origin);
                Assert.That(SkillPositions.AttackPosition(_skill, area, caster, aim), Is.EqualTo(aim + Vector3.up));
            }
        }
        [Test] public void UnboundCircleHitsPlayerLocationInsteadOfMouseLocation()
        {
            var area = new CircleAttack { Radius = .5f }; Upgrade(area); area.Settings.BaseDamage = 10;
            var nearby = new Target { Position = Vector3.zero }; var distant = new Target { Position = Vector3.right * 4 };
            var provider = new ListTargetProvider(); provider.Add(nearby); provider.Add(distant);
            Assert.That(new SkillExecutor(new Clock(), provider).TryCast(_skill, new Caster(), distant.Position), Is.EqualTo(CastResult.Success));
            Assert.That(nearby.CurrentHp, Is.EqualTo(90)); Assert.That(distant.CurrentHp, Is.EqualTo(100));
        }
        [Test] public void AttackModeHidesSpecialWithoutDeletingItsWiring()
        {
            Upgrade(new CircleAttack()); _skill.AttackMode = 1;
            var release = new InstantRelease(); _skill.AddRelease(release);
            var special = new CircleAttack(); _skill.AddAttack(special);
            _skill.SetBinding(SkillDefinition.CoreLayoutId, release.ModuleId, BindingRules.Trigger, BindingRules.Special);
            _skill.SetBinding(release.ModuleId, special.ModuleId, BindingRules.Cast);
            _skill.AttackMode = 0; Assert.That(_skill.ReleaseFor(SkillKind.SpecialAttack), Is.Null);
            Assert.That(_skill.AttacksFor(SkillKind.SpecialAttack), Is.Empty);
            _skill.AttackMode = 1; Assert.That(_skill.ReleaseFor(SkillKind.SpecialAttack), Is.SameAs(release));
            Assert.That(_skill.NormalKey, Is.EqualTo(SkillInputKey.MouseLeft)); Assert.That(_skill.SpecialKey, Is.EqualTo(SkillInputKey.MouseRight));
            _skill.NormalKey = SkillInputKey.F; Assert.That(_skill.SlotForKey(SkillInputKey.F), Is.EqualTo(SkillKind.NormalAttack));
            _skill.SpecialKey = SkillInputKey.F; Assert.That(SkillValidator.HasErrors(_skill), Is.True);
        }
        [Test] public void BoardPortsAndFieldsMatchSerialReleaseLayout()
        {
            var release = Upgrade(new ProjectileAttack());
            var type = Type.GetType("AbilityStudio.Editor.Board.GraphView.CompositionBoardView, AbilityStudio.Editor.Board.GraphView", true);
            using var board = (IBoardView)Activator.CreateInstance(type); board.Bind(_skill);
            var graph = (GraphView)board.Element;
            Node Core() => graph.nodes.First(n => (string)n.userData == SkillDefinition.CoreLayoutId);
            Assert.That(Core().outputContainer.Children().Cast<Port>().Select(x => x.portName), Is.EqualTo(new[] { "释放方式", "技能终点" }));
            var releaseNode = graph.nodes.First(n => (string)n.userData == release.ModuleId);
            Assert.That(releaseNode.inputContainer.Children().Cast<Port>().Single().portName, Is.EqualTo("释放方式"));
            var areaNode = graph.nodes.First(n => (string)n.userData == _skill.Attack.ModuleId);
            Assert.That(areaNode.inputContainer.Children().Cast<Port>().Select(p => p.portName), Is.EqualTo(new[] { "释放", "释放位置" }));
            Assert.That(Core().Q<FloatField>("baseDamage"), Is.Null); Assert.That(Core().Q<DropdownField>("attackMode"), Is.Not.Null);
            Assert.That(areaNode.Q<FloatField>("attack.settings.baseDamage"), Is.Not.Null);
            Assert.That(areaNode.Q<Vector2Field>("attack.castOriginOffset"), Is.Not.Null);
            _skill.AttackMode = 1; board.Bind(_skill);
            Assert.That(Core().outputContainer.Children().Cast<Port>().Select(x => x.portName), Is.EqualTo(new[] { "普通攻击释放方式", "特殊攻击释放方式", "技能终点" }));
        }
        [Test] public void AuthoredKeysResolveToInputSystemControls()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>(); var mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                Assert.That(SkillInput.Control(SkillInputKey.F), Is.SameAs(keyboard.fKey));
                Assert.That(SkillInput.Control(SkillInputKey.MouseRight), Is.SameAs(mouse.rightButton));
                Assert.That(SkillInput.EditorKey(KeyCode.Alpha1), Is.EqualTo(SkillInputKey.Digit1));
            }
            finally { InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); }
        }
    }
}
