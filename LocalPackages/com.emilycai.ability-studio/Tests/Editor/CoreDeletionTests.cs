using System;
using System.Linq;
using AbilityStudio.Editor;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace AbilityStudio.Tests
{
    public sealed class CoreDeletionTests
    {
        private SkillDefinition _skill;
        private sealed class Clock : ISkillClock { public double Time => 0; }
        private sealed class Caster : ISkillCaster
        {
            public Vector3 Position => Vector3.zero;
            public Quaternion Rotation => Quaternion.identity;
            public int Team => 0;
        }
        [SetUp] public void Setup()
        {
            _skill = ScriptableObject.CreateInstance<SkillDefinition>();
            _skill.SetAttack(new ProjectileAttack { Size = .7f });
            var heal = new HealEffect { Amount = 7 }; _skill.AddEffect(heal);
            _skill.SetBinding(_skill.Attack.ModuleId, heal.ModuleId, BindingRules.Targets);
            _skill.SetBinding(SkillDefinition.CoreLayoutId, _skill.Attack.ModuleId, BindingRules.Position);
        }
        [TearDown] public void Cleanup() { Undo.ClearUndo(_skill); UnityEngine.Object.DestroyImmediate(_skill); }
        [Test] public void RemovalPreservesOtherNodesBindingsAndParametersAndSurvivesSerialization()
        {
            Assert.That(SkillAssetCommands.RemoveModules(_skill, new[] { SkillDefinition.CoreLayoutId }), Is.True);
            Assert.That(_skill.HasCore, Is.False);
            Assert.That(_skill.Effects.Count, Is.EqualTo(1));
            Assert.That(((ProjectileAttack)_skill.Attack).Size, Is.EqualTo(.7f));
            Assert.That(_skill.Bindings.Count, Is.EqualTo(1));
            Assert.That(_skill.BindingSource(_skill.Effects[0].ModuleId, BindingRules.Targets), Is.EqualTo(_skill.Attack.ModuleId));
            Assert.That(BindingRules.CanConnect(_skill, SkillDefinition.CoreLayoutId, _skill.Attack.ModuleId, BindingRules.Position), Is.False);
            var copy = ScriptableObject.CreateInstance<SkillDefinition>();
            try
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_skill), copy);
                Assert.That(copy.HasCore, Is.False); Assert.That(copy.Effects.Count, Is.EqualTo(1));
                Assert.That(copy.AddCore(), Is.True); Assert.That(copy.AddCore(), Is.False);
                Assert.That(copy.BindingSource(copy.Attack.ModuleId, BindingRules.Position), Is.Null);
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
        [Test] public void MissingCoreCannotCastAndRestoredCoreCanCast()
        {
            var executor = new SkillExecutor(new Clock(), new ListTargetProvider());
            _skill.RemoveModule(SkillDefinition.CoreLayoutId);
            Assert.That(executor.TryCast(_skill, new Caster(), Vector3.right), Is.EqualTo(CastResult.Invalid));
            Assert.That(SkillValidator.Validate(_skill).Any(x => x.Contains("缺少技能基础")), Is.True);
            _skill.AddCore();
            Assert.That(executor.TryCast(_skill, new Caster(), Vector3.right), Is.EqualTo(CastResult.Success));
        }
        [Test] public void DeleteAndAddSupportUndoRedo()
        {
            Undo.IncrementCurrentGroup(); SkillAssetCommands.RemoveModules(_skill, new[] { SkillDefinition.CoreLayoutId }); Undo.FlushUndoRecordObjects();
            Undo.PerformUndo(); Assert.That(_skill.HasCore, Is.True); Assert.That(_skill.Bindings.Count, Is.EqualTo(2));
            Undo.PerformRedo(); Assert.That(_skill.HasCore, Is.False);
            Undo.IncrementCurrentGroup(); SkillAssetCommands.AddCore(_skill); Undo.FlushUndoRecordObjects();
            Undo.PerformUndo(); Assert.That(_skill.HasCore, Is.False);
            Undo.PerformRedo(); Assert.That(_skill.HasCore, Is.True);
        }
        [Test] public void BoardDeletesRootAndLibraryOffersRestore()
        {
            var type = Type.GetType("AbilityStudio.Editor.Board.GraphView.CompositionBoardView, AbilityStudio.Editor.Board.GraphView", true);
            using var board = (IBoardView)Activator.CreateInstance(type);
            board.Bind(_skill); var graph = (GraphView)board.Element;
            var core = graph.nodes.First(n => (string)n.userData == SkillDefinition.CoreLayoutId);
            Assert.That((core.capabilities & Capabilities.Deletable) != 0, Is.True);
            graph.AddToSelection(core); board.DeleteSelectedModules();
            Assert.That(graph.nodes.Any(n => (string)n.userData == SkillDefinition.CoreLayoutId), Is.False);
            Assert.That(graph.nodes.Count(), Is.EqualTo(3));
            var library = new ModuleLibraryView(() => _skill);
            Assert.That(library.Q<Button>("inspect-core").text, Is.EqualTo("添加技能基础"));
            SkillAssetCommands.AddCore(_skill); board.Bind(_skill);
            Assert.That(graph.nodes.Count(n => (string)n.userData == SkillDefinition.CoreLayoutId), Is.EqualTo(1));
        }
    }
}
