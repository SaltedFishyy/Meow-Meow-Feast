using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbilityStudio.Editor
{
    /// <summary>Shared parameter controls for the Inspector and composition board.</summary>
    public static class SkillParameterFields
    {
        /// <summary>Creates controls over a serialized subtree; all edits use asset commands.</summary>
        public static VisualElement Create(SkillDefinition skill, string prefix)
        {
            var root = new VisualElement();
            if (prefix == "")
            {
                Text(root, skill, "displayName", "名称");
                if (!skill.IntegratedAttacks)
                {
                    var merge = new Button(() => SkillAssetCommands.IntegrateAttacks(skill)) { text = "合并释放方式到攻击节点", tooltip = "每个攻击按键最多对应一个范围时可以合并；原连线和数值会保留。" };
                    merge.SetEnabled(skill.CanIntegrate); root.Add(merge);
                }
                if (!skill.WeaponMode)
                {
                root.Add(new Button(() => SkillAssetCommands.EnableWeaponMode(skill)) { text = "转换为双攻击（保留当前攻击）" });
                Choice(root, skill, "kind", "技能类型", "普通攻击", "特殊攻击");
                Number(root, skill, "cooldown", "冷却时间（秒）");
                Number(root, skill, "baseDamage", "基础伤害");
                Number(root, skill, "distance", "最大施法距离（米）");
                root.Q<FloatField>("distance").tooltip = "技能终点在范围内跟随鼠标，超出最大施法距离时停在边界。";
                }
                var field = new Vector2Field("施法偏移（XY）") { name = "castOriginOffset" };
                field.SetValueWithoutNotify((Vector2)skill.CastOriginOffset);
                field.RegisterValueChangedCallback(evt => SkillAssetCommands.SetParameter(skill, "castOriginOffset", new Vector3(evt.newValue.x, evt.newValue.y, 0)));
                field.schedule.Execute(() => { if (skill != null && !IsEditing(field)) field.SetValueWithoutNotify((Vector2)skill.CastOriginOffset); }).Every(150);
                root.Add(field);
            }
            else
            {
                using var serialized = new SerializedObject(skill);
                var module = serialized.FindProperty(prefix)?.managedReferenceValue;
                if (module is ReleaseModule && skill.WeaponMode)
                {
                    Number(root, skill, prefix + ".baseDamage", "基础伤害");
                    Number(root, skill, prefix + ".cooldown", "冷却时间（秒）");
                    Number(root, skill, prefix + ".distance", "最大施法距离（米）");
                }
                if (module is AttackModule integrated && skill.IntegratedAttacks)
                {
                    root.Add(new Label("攻击设置"));
                    Choice(root,skill,prefix+".releaseMode","释放方式","瞬发","蓄力");
                    Number(root,skill,prefix+".settings.baseDamage","基础伤害");
                    Number(root,skill,prefix+".settings.cooldown","冷却时间（秒）");
                    Number(root,skill,prefix+".settings.distance","最大施法距离（米）");
                    if (integrated.IsCharged) Number(root,skill,prefix+".settings.duration","蓄满时间（秒）");
                    root.Add(new Label("范围设置"));
                }
                bool inline = skill.IntegratedAttacks && module is AttackModule;
                bool charging = inline && ((AttackModule)module).IsCharged;
                if (module is AttackModule) Choice(root, skill, prefix + ".damageMode", "伤害结算", "默认基础伤害", "连接倍率节点");
                if (module is HitFeedbackEffect)
                {
                    Choice(root, skill, prefix + ".triggerMode", "触发方式", "每次命中", "每次攻击首次命中");
                    Number(root, skill, prefix + ".minInterval", "最短触发间隔（秒）");
                    if (module is RecoilFeedback)
                    {
                        Number(root, skill, prefix + ".angle", "后仰角度（度）");
                        Number(root, skill, prefix + ".leanDuration", "后仰时间（秒）");
                        Number(root, skill, prefix + ".recoveryDuration", "回正时间（秒）");
                    }
                    else if (module is CameraShakeFeedback)
                    {
                        Number(root, skill, prefix + ".strength", "震动强度（米）");
                        Number(root, skill, prefix + ".duration", "持续时间（秒）");
                        Number(root, skill, prefix + ".frequency", "震动频率（Hz）");
                    }
                    else if (module is HitVfxFeedback)
                    {
                        Asset(root, skill, prefix + ".prefab", "特效预制体", typeof(GameObject));
                        Number(root, skill, prefix + ".scale", "大小倍率");
                        Number(root, skill, prefix + ".lifetime", "存活时间（秒）");
                        var field = new Vector3Field("世界偏移") { name = prefix + ".offset" };
                        string feedbackId = ((HitVfxFeedback)module).ModuleId;
                        bool SameFeedback(SerializedObject so) => (so.FindProperty(prefix)?.managedReferenceValue as HitVfxFeedback)?.ModuleId == feedbackId;
                        field.SetValueWithoutNotify(((HitVfxFeedback)module).Offset);
                        field.RegisterValueChangedCallback(evt => { if (skill == null) return; using var so = new SerializedObject(skill); if (SameFeedback(so)) SkillAssetCommands.SetParameter(skill, prefix + ".offset", evt.newValue); });
                        field.schedule.Execute(() => { if (skill == null || IsEditing(field)) return; using var so = new SerializedObject(skill); if (SameFeedback(so)) field.SetValueWithoutNotify(so.FindProperty(prefix + ".offset").vector3Value); }).Every(150);
                        root.Add(field);
                        root.Add(new HelpBox("未指定预制体时不生成特效。请使用不带战斗逻辑的视觉预制体。", HelpBoxMessageType.Info));
                    }
                    else if (module is HitAudioFeedback)
                    {
                        Asset(root, skill, prefix + ".clip", "音效资源", typeof(AudioClip));
                        Number(root, skill, prefix + ".volume", "音量（0–1）");
                        Number(root, skill, prefix + ".pitch", "音调（0.1–3）");
                        root.Add(new HelpBox("未指定音效资源时不播放声音。", HelpBoxMessageType.Info));
                    }
                }
                else if (module is ChargeRelease) Number(root, skill, prefix + ".duration", "最长蓄力时间（秒）");
                else if (module is DamageEffect) Number(root, skill, prefix + ".multiplierPercent", "基础伤害倍率（%）");
                else if (module is PierceResponse) Integer(root, skill, prefix + ".count", "穿透次数");
                else if (module is ChainResponse)
                { Integer(root, skill, prefix + ".count", "连锁次数"); Number(root, skill, prefix + ".distance", "搜索距离（米）"); }
                else if (module is ReturnResponse) Number(root, skill, prefix + ".speedMultiplier", "返回速度倍率");
                else if (module is SkillEndpointPosition)
                    Number(root, skill, prefix + ".distance", "终点距离（米）");
                else if (module is SelfPosition) root.Add(new Label("施法时的玩家位置"));
                else if (module is MousePosition) root.Add(new Label("施法时的鼠标地面位置"));
                else if (module is ProjectileAttack)
                {
                    Number(root, skill, prefix + ".speed", "速度（米/秒）");
                    Number(root, skill, prefix + ".range", "距离（米）");
                    Number(root, skill, prefix + ".size", "半径（米）");
                }
                else if (module is ConeAttack)
                {
                    if (!inline || charging) Number(root, skill, prefix + ".minRange", inline ? "起始距离（米）" : "蓄力最短距离（米）", !inline);
                    Number(root, skill, prefix + ".range", inline ? (charging ? "蓄满距离（米）" : "距离（米）") : "固定／最远距离（米）");
                    Number(root, skill, prefix + ".angleDegrees", "角度（度）");
                }
                else if (module is LineThrustAttack)
                {
                    Number(root, skill, prefix + ".length", inline ? (charging ? "蓄满长度（米）" : "长度（米）") : "固定／最长长度（米）");
                    if (!inline || charging) Number(root, skill, prefix + ".minLength", inline ? "起始长度（米）" : "蓄力最短长度（米）", !inline);
                    Number(root, skill, prefix + ".tipLength", "前端区域长度（米）");
                    Number(root, skill, prefix + ".width", "宽度（米）");
                }
                else if (module is CircleAttack)
                {
                    Number(root, skill, prefix + ".radius", inline ? (charging ? "蓄满半径（米）" : "半径（米）") : "固定／最大半径（米）");
                    if (!inline || charging) Number(root, skill, prefix + ".minRadius", inline ? "起始半径（米）" : "蓄力最小半径（米）", !inline);
                }
                else if (module is SlowEffect)
                {
                    Number(root, skill, prefix + ".reductionPercent", "减速比例（%）");
                    Number(root, skill, prefix + ".duration", "减速时间（秒）");
                }
                else if (module is BleedEffect)
                {
                    Number(root, skill, prefix + ".damagePerSecond", "每秒流血伤害");
                    Number(root, skill, prefix + ".duration", "流血时间（秒）");
                }
                else if (module is StunEffect)
                    Number(root, skill, prefix + ".duration", "眩晕时间（秒）");
                else if (module is KnockbackEffect)
                    Number(root, skill, prefix + ".distance", "击退距离（米）");
                else if (module is PullEffect)
                {
                    Number(root, skill, prefix + ".stopDistance", "目的地间距（米）");
                    Number(root, skill, prefix + ".duration", "拉拽时间（秒）");
                }
            }
            return root;
        }
        private static void Asset(VisualElement root, SkillDefinition skill, string path, string label, System.Type type)
        {
            var field = new UnityEditor.UIElements.ObjectField(label) { name = path, objectType = type, allowSceneObjects = false };
            using var initial = new SerializedObject(skill);
            string ownerPath = path.Substring(0, path.LastIndexOf('.'));
            string ownerId = (initial.FindProperty(ownerPath)?.managedReferenceValue as SkillModule)?.ModuleId;
            bool SameOwner(SerializedObject so) => (so.FindProperty(ownerPath)?.managedReferenceValue as SkillModule)?.ModuleId == ownerId;
            void Refresh() { if (skill == null || IsEditing(field)) return; using var so = new SerializedObject(skill); if (SameOwner(so)) field.SetValueWithoutNotify(so.FindProperty(path).objectReferenceValue); }
            field.RegisterValueChangedCallback(evt => { if (skill == null) return; using var so = new SerializedObject(skill); if (SameOwner(so)) SkillAssetCommands.SetParameter(skill, path, evt.newValue); });
            Refresh(); field.schedule.Execute(Refresh).Every(150); root.Add(field);
        }
        private static void Choice(VisualElement root, SkillDefinition skill, string path, string label, params string[] choices)
        {
            var field = new DropdownField(label, new System.Collections.Generic.List<string>(choices), 0) { name = path };
            string OwnerId()
            {
                if (skill == null || !path.Contains(".")) return null;
                using var serialized = new SerializedObject(skill);
                return (serialized.FindProperty(path.Substring(0, path.LastIndexOf('.')))?.managedReferenceValue as SkillModule)?.ModuleId;
            }
            var ownerId = OwnerId();
            void Refresh()
            {
                if (skill == null || IsEditing(field) || OwnerId() != ownerId) return;
                using var serialized = new SerializedObject(skill); var property = serialized.FindProperty(path);
                if (property != null) field.SetValueWithoutNotify(choices[Mathf.Clamp(property.intValue, 0, choices.Length - 1)]);
            }
            Refresh(); field.RegisterValueChangedCallback(evt =>
            { if (skill != null && OwnerId() == ownerId) SkillAssetCommands.SetParameter(skill, path, System.Array.IndexOf(choices, evt.newValue)); });
            field.schedule.Execute(Refresh).Every(150); root.Add(field);
        }
        private static void Integer(VisualElement root, SkillDefinition skill, string path, string label)
        {
            var field = new IntegerField(label) { name = path, isDelayed = true };
            string OwnerId()
            {
                if (skill == null) return null;
                using var serialized = new SerializedObject(skill);
                return (serialized.FindProperty(path.Substring(0, path.LastIndexOf('.')))?.managedReferenceValue as SkillModule)?.ModuleId;
            }
            var ownerId = OwnerId();
            void Refresh()
            {
                if (skill == null || IsEditing(field) || OwnerId() != ownerId) return;
                using var serialized = new SerializedObject(skill);
                var property = serialized.FindProperty(path);
                if (property != null) field.SetValueWithoutNotify(property.intValue);
            }
            Refresh();
            field.RegisterValueChangedCallback(evt => { if (skill != null && OwnerId() == ownerId) SkillAssetCommands.SetParameter(skill, path, evt.newValue); });
            field.schedule.Execute(Refresh).Every(150); root.Add(field);
        }
        private static void Number(VisualElement root, SkillDefinition skill, string path, string label, bool chargeOnly = false)
        {
            var field = new FloatField(label) { name = path, isDelayed = true };
            string OwnerId()
            {
                if (skill == null || !path.Contains(".")) return null;
                using var serialized = new SerializedObject(skill);
                return (serialized.FindProperty(path.Substring(0, path.LastIndexOf('.')))?.managedReferenceValue as SkillModule)?.ModuleId;
            }
            var ownerId = OwnerId();
            bool CanEdit() => !chargeOnly || (skill != null && skill.FindModule(skill.BindingSource(ownerId, BindingRules.Charge)) is ChargeRelease && skill.BindingOutput(ownerId, BindingRules.Charge) == BindingRules.Charge);
            field.labelElement.enableRichText = true;
            void Refresh()
            {
                bool allowed = CanEdit();
                field.SetEnabled(allowed);
                field.label = allowed ? label : "<s>" + label + "</s>";
                field.tooltip = !allowed ? "连接蓄力节点的蓄力比例后可修改；断开连接会保留当前值。" : "";
                if (skill == null || (allowed && IsEditing(field)) || OwnerId() != ownerId) return;
                using var serialized = new SerializedObject(skill);
                var property = serialized.FindProperty(path);
                if (property != null) field.SetValueWithoutNotify(property.floatValue);
            }
            Refresh();
            field.RegisterValueChangedCallback(evt =>
            {
                if (skill != null && OwnerId() == ownerId && CanEdit()) SkillAssetCommands.SetParameter(skill, path, evt.newValue);
                else Refresh();
            });
            field.schedule.Execute(Refresh).Every(150);
            root.Add(field);
        }
        private static void Text(VisualElement root, SkillDefinition skill, string path, string label)
        {
            var field = new TextField(label) { name = path, isDelayed = true };
            field.SetValueWithoutNotify(skill.DisplayName);
            field.RegisterValueChangedCallback(evt => SkillAssetCommands.SetParameter(skill, path, evt.newValue));
            field.schedule.Execute(() => { if (skill != null && !IsEditing(field)) field.SetValueWithoutNotify(skill.DisplayName); }).Every(150);
            root.Add(field);
        }
        private static bool IsEditing(VisualElement field) =>
            field.focusController?.focusedElement is VisualElement focused && (focused == field || field.Contains(focused));
    }
}








