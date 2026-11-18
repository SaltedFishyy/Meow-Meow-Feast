using AbilityStudio.Model;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbilityStudio.Editor
{
    /// <summary>Inspector companion to the composition board, editing the same asset.</summary>
    [CustomEditor(typeof(SkillDefinition))]
    public sealed class SkillDefinitionInspector : UnityEditor.Editor
    {
        /// <inheritdoc/>
        public override VisualElement CreateInspectorGUI()
        {
            var skill = (SkillDefinition)target;
            SkillAssetCommands.MigrateEndpoint(skill);
            var root = new VisualElement();
            string signature = null;
            void RefreshComposition()
            {
                if (skill == null) return;
                var current = skill.WeaponMode + ":" + skill.IntegratedAttacks + ":" + string.Join(",",skill.Attacks.Select(a=>a.ReleaseMode)) + ":" + string.Join(",", skill.Releases.Select(r => r?.ModuleId)) + ":" + string.Join(",", skill.Responses.Select(r => r?.ModuleId)) + ":" + string.Join(",", skill.Positions.Select(a => a?.ModuleId)) + ":" + string.Join(",", skill.Attacks.Select(a => a?.ModuleId)) + ":" + string.Join(",", skill.Effects.Select(effect => effect?.ModuleId));
                if (current == signature) return;
                signature = current;
                root.Clear();
                Build(root, skill);
            }
            RefreshComposition();
            root.schedule.Execute(RefreshComposition).Every(200);
            return root;
        }
        private static void Build(VisualElement root, SkillDefinition skill)
        {
            root.Add(new Button(() => SkillBoardWindow.OpenSkill(skill)) { text = "打开技能画板" });
            root.Add(new HelpBox("运行时可修改参数，再次施法即可测试。点击“保存技能”保留修改。", HelpBoxMessageType.Info));
            root.Add(SkillParameterFields.Create(skill, ""));
            for (int i = 0; i < skill.Positions.Count; i++)
            {
                if (skill.Positions[i] == null) continue;
                root.Add(new Label("位置 · " + ModuleCatalog.Metadata(skill.Positions[i].GetType()).DisplayName));
                root.Add(SkillParameterFields.Create(skill, "positions.Array.data[" + i + "]"));
            }
            foreach (var entry in ModuleCatalog.AttackEntries(skill))
            {
                if (entry.module == null) continue;
                root.Add(new Label("技能范围 · " + (ModuleCatalog.Metadata(entry.module.GetType())?.DisplayName ?? entry.module.GetType().Name)));
                root.Add(SkillParameterFields.Create(skill, entry.path));
            }
            if (!skill.Attacks.Any()) root.Add(new Button(() => SkillAssetCommands.SetProjectile(skill)) { text = "添加投射物" });
            for (int i = 0; !skill.IntegratedAttacks && i < skill.Releases.Count; i++)
            {
                if (skill.Releases[i] == null) continue;
                root.Add(new Label("释放方式 · " + ModuleCatalog.Metadata(skill.Releases[i].GetType()).DisplayName));
                root.Add(SkillParameterFields.Create(skill, "releases.Array.data[" + i + "]"));
            }
            for (int i = 0; i < skill.Responses.Count; i++)
            {
                if (skill.Responses[i] == null) continue;
                root.Add(new Label("命中响应 · " + ModuleCatalog.Metadata(skill.Responses[i].GetType()).DisplayName));
                root.Add(SkillParameterFields.Create(skill, "responses.Array.data[" + i + "]"));
            }
            bool hasPull = false;
            for (int i = 0; i < skill.Effects.Count; i++)
            {
                if (skill.Effects[i] == null) continue;
                hasPull |= skill.Effects[i] is PullEffect;
                root.Add(new Label((skill.Effects[i] is HitFeedbackEffect ? "命中反馈 · " : "技能效果 · ") + (ModuleCatalog.Metadata(skill.Effects[i].GetType())?.DisplayName ?? skill.Effects[i].GetType().Name)));
                root.Add(SkillParameterFields.Create(skill, "effects.Array.data[" + i + "]"));
            }
            if (!hasPull) root.Add(new Button(() => SkillAssetCommands.AddPull(skill)) { text = "添加拉拽效果" });
            root.Add(new Button(() => SkillAssetCommands.Save(skill)) { text = "保存技能" });
            var issues = new HelpBox("", HelpBoxMessageType.Warning); root.Add(issues);
            issues.schedule.Execute(() =>
            {
                if (skill == null) return;
                var errors = SkillValidator.Validate(skill);
                issues.text = string.Join("\n", errors);
                issues.style.display = errors.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }).Every(250);
        }
    }
}





