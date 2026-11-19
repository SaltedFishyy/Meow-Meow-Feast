using System;
using System.Collections.Generic;
using System.Linq;
using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Undo-aware asset authoring operations.</summary>
    public static class SkillAssetCommands
    {
        /// <summary>Upgrades graph ownership and wiring as one reversible edit.</summary>
        public static bool UpgradeGraph(SkillDefinition skill)
        {
            if (skill == null || skill.AreaGraph) return false;
            Undo.RegisterCompleteObjectUndo(skill, "升级释放连线与范围参数");
            SkillGraphMigration.Upgrade(skill); EditorUtility.SetDirty(skill); return true;
        }
        /// <summary>Adds a fresh independent module instance and optional position in one Undo step.</summary>
        public static bool AddModule(SkillDefinition skill, Type type, Vector2? position = null)
        {
            if (!ModuleCatalog.CanAdd(skill, type)) return false;
            UpgradeGraph(skill);
            var module = (SkillModule)Activator.CreateInstance(type);
            Undo.RegisterCompleteObjectUndo(skill, "添加 " + ModuleCatalog.Metadata(type).DisplayName);
            if (module is AttackModule attack) skill.AddAttack(attack);
            else if (module is PositionModule positionModule) skill.AddPosition(positionModule);
            else if (module is ReleaseModule release) skill.AddRelease(release);
            else if (module is HitResponseModule response) skill.AddResponse(response);
            else skill.AddEffect((EffectModule)module);
            if (position.HasValue) skill.SetBoardPosition(module.ModuleId, BoardGrid.Snap(position.Value));
            EditorUtility.SetDirty(skill);
            return true;
        }
        /// <summary>Restores the deleted core in one undoable edit.</summary>
        public static bool AddCore(SkillDefinition skill)
        {
            if (skill == null || skill.HasCore) return false;
            Undo.RegisterCompleteObjectUndo(skill, "添加技能基础");
            skill.AddCore(); EditorUtility.SetDirty(skill); return true;
        }
        /// <summary>Removes modules and their saved positions together as one undoable edit.</summary>
        public static bool RemoveModules(SkillDefinition skill, IEnumerable<string> moduleIds)
        {
            if (skill == null || moduleIds == null) return false;
            var requested = new HashSet<string>(moduleIds);
            var existing = skill.Modules.Where(e => e != null).Select(e => e.ModuleId).ToList();
            if (skill.HasCore) existing.Add(SkillDefinition.CoreLayoutId);
            // Modules includes attacks, effects and position sources.
            var removed = existing.Where(requested.Contains).ToArray();
            if (removed.Length == 0) return false;
            Undo.RegisterCompleteObjectUndo(skill, "删除技能模块");
            foreach (var id in removed) skill.RemoveModule(id);
            EditorUtility.SetDirty(skill);
            return true;
        }
        /// <summary>Connects or disconnects a typed input with Undo.</summary>
        public static bool SetBinding(SkillDefinition skill, string sourceId, string targetId, string input, string sourcePort = "")
        {
            if (skill == null || (sourceId != null && !BindingRules.CanConnect(skill, sourceId, targetId, input, sourcePort))) return false;
            if (skill.BindingSource(targetId, input) == sourceId && skill.BindingOutput(targetId, input) == (sourcePort ?? "")) return false;
            Undo.RegisterCompleteObjectUndo(skill, "连接技能模块");
            skill.SetBinding(sourceId, targetId, input, sourcePort); EditorUtility.SetDirty(skill); return true;
        }
        /// <summary>Edits a serialized parameter with Unity Undo and shared asset identity.</summary>
        public static void SetParameter(SkillDefinition skill, string path, object value)
        {
            using var serialized = new SerializedObject(skill);
            var property = serialized.FindProperty(path);
            if (property == null) throw new System.ArgumentException("Unknown skill parameter: " + path);
            switch (value)
            {
                case null when property.propertyType == SerializedPropertyType.ObjectReference: property.objectReferenceValue = null; break;
                case UnityEngine.Object reference when property.propertyType == SerializedPropertyType.ObjectReference: property.objectReferenceValue = reference; break;
                case float number: property.floatValue = number; break;
                case int number: property.intValue = number; break;
                case string text: property.stringValue = text; break;
                case Vector3 vector: property.vector3Value = vector; break;
                default: throw new System.ArgumentException("Unsupported parameter value");
            }
            serialized.ApplyModifiedProperties();
        }
        /// <summary>Records a presentation change independently from runtime parameters.</summary>
        public static void MoveModule(SkillDefinition skill, string moduleId, Vector2 position)
        {
            Undo.RecordObject(skill, "移动技能模块");
            skill.SetBoardPosition(moduleId, BoardGrid.Snap(position));
            EditorUtility.SetDirty(skill);
        }
        /// <summary>Adds the prototype's supported attack through a reusable authoring command.</summary>
        public static void SetProjectile(SkillDefinition skill)
        { AddModule(skill, typeof(ProjectileAttack)); }
        /// <summary>Adds a reusable Pull module.</summary>
        public static void AddPull(SkillDefinition skill)
        { AddModule(skill, typeof(PullEffect)); }
        /// <summary>Saves the existing asset without touching other dirty assets.</summary>
        public static void Save(SkillDefinition skill)
        { EditorUtility.SetDirty(skill); AssetDatabase.SaveAssetIfDirty(skill); }
        /// <summary>Creates an asset only at a previously unused path.</summary>
        public static void Create(SkillDefinition skill, string path)
        {
            if (System.IO.File.Exists(path)) throw new System.InvalidOperationException("Refusing to overwrite " + path);
            AssetDatabase.CreateAsset(skill, path); Undo.RegisterCreatedObjectUndo(skill, "创建技能"); Save(skill);
        }
    }
}




