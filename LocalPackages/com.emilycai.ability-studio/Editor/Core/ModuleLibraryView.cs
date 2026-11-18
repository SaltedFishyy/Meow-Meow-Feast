using System;
using System.Linq;
using AbilityStudio.Model;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbilityStudio.Editor
{
    /// <summary>Searchable module library operating on the window's shared skill.</summary>
    public sealed class ModuleLibraryView : VisualElement
    {
        /// <summary>Native editor drag payload identifying the module type.</summary>
        public const string DragTypeKey = "AbilityStudio.ModuleType";
        /// <summary>Source asset identity prevents drops into a different skill.</summary>
        public const string DragSkillKey = "AbilityStudio.ModuleSkill";
        /// <summary>Creates searchable, always-enabled module cards for dragging onto the board.</summary>
        public ModuleLibraryView(Func<SkillDefinition> currentSkill)
        {
            name = "module-library";
            style.width = 240; style.minWidth = 240; style.flexShrink = 0;
            style.paddingLeft = 12; style.paddingRight = 12; style.paddingTop = 12; style.paddingBottom = 12;
            style.backgroundColor = new Color(0.20f, 0.20f, 0.20f);
            style.borderRightWidth = 1; style.borderRightColor = new Color(0.13f, 0.13f, 0.13f);
            var heading = new Label("模块库"); heading.style.marginBottom = 8; Add(heading);
            var search = new ToolbarSearchField { name = "module-search" };
            search.style.width = Length.Percent(100); search.style.minWidth = 0;
            search.style.maxWidth = Length.Percent(100); search.style.flexShrink = 1;
            search.style.marginLeft = 0; search.style.marginRight = 0; search.style.marginBottom = 8;
            Add(search);
            Add(new Label("按住鼠标左键，将模块拖到画板。") { style = { whiteSpace = WhiteSpace.Normal } });
            var list = new ScrollView(ScrollViewMode.Vertical);
            list.style.flexGrow = 1; list.style.minWidth = 0;
            list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            Add(list);
            var expanded = new System.Collections.Generic.Dictionary<string, bool>();
            Foldout Group(string title, string id)
            {
                var group = new Foldout { name = id, text = title };
                expanded[id] = SessionState.GetBool("AbilityStudio.Library." + id, true);
                group.SetValueWithoutNotify(expanded[id]);
                group.style.marginTop = 16; group.style.marginBottom = 4;
                group.RegisterValueChangedCallback(evt =>
                {
                    if (evt.target != group || !string.IsNullOrEmpty(search.value)) return;
                    expanded[id] = evt.newValue; SessionState.SetBool("AbilityStudio.Library." + id, evt.newValue);
                });
                list.Add(group); return group;
            }
            var coreGroup = Group("技能基础", "category-core");
            var coreCard = new Button(() => { if (currentSkill() != null) Selection.activeObject = currentSkill(); })
                { text = "技能基础", name = "inspect-core", tooltip = "每个技能已有一个核心，点击查看设置。" };
            coreCard.style.height = 34; coreGroup.Add(coreCard);
            var releaseGroup = Group("释放方式", "category-release");
            var areaGroup = Group("技能范围", "category-area");
            var effectGroup = Group("技能效果", "category-effect");
            var responseGroup = Group("命中响应", "category-response");
            var feedbackGroup = Group("命中反馈", "category-feedback");
            foreach (var type in ModuleCatalog.Types)
            {
                var metadata = ModuleCatalog.Metadata(type);
                var row = new VisualElement(); row.style.marginTop = 12;
                var button = new VisualElement { name = "add-" + type.Name };
                button.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
                button.style.paddingTop = 10; button.style.paddingBottom = 10;
                button.style.paddingLeft = 10;
                button.style.paddingRight = 10;
                button.style.borderTopLeftRadius = button.style.borderTopRightRadius = 4;
                button.style.borderBottomLeftRadius = button.style.borderBottomRightRadius = 4;
                button.RegisterCallback<MouseEnterEvent>(_ => button.style.backgroundColor = new Color(0.34f, 0.34f, 0.34f));
                button.RegisterCallback<MouseLeaveEvent>(_ => button.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f));
                button.Add(new Label(metadata.DisplayName ?? type.Name) { pickingMode = PickingMode.Ignore });
                bool armed = false;
                Vector2 start = default;
                SkillDefinition source = null;
                button.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button != 0 || currentSkill() == null) return;
                    source = currentSkill(); start = evt.mousePosition; armed = true;
                    button.CaptureMouse(); evt.StopPropagation();
                });
                button.RegisterCallback<MouseMoveEvent>(evt =>
                {
                    if (!armed || (evt.pressedButtons & 1) == 0 || Vector2.Distance(start, evt.mousePosition) < 6) return;
                    armed = false; button.ReleaseMouse();
                    if (source == null || source != currentSkill()) return;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = Array.Empty<UnityEngine.Object>();
                    DragAndDrop.SetGenericData(DragTypeKey, type);
                    DragAndDrop.SetGenericData(DragSkillKey, source);
                    DragAndDrop.StartDrag(metadata.DisplayName ?? type.Name);
                    evt.StopPropagation();
                });
                button.RegisterCallback<MouseUpEvent>(_ => { armed = false; button.ReleaseMouse(); });
                button.RegisterCallback<MouseCaptureOutEvent>(_ => armed = false);
                var group = typeof(HitFeedbackEffect).IsAssignableFrom(type) ? feedbackGroup : typeof(ReleaseModule).IsAssignableFrom(type) ? releaseGroup : typeof(AttackModule).IsAssignableFrom(type) ? areaGroup : typeof(HitResponseModule).IsAssignableFrom(type) ? responseGroup : effectGroup;
                row.AddToClassList("module-row"); row.Add(button); group.Add(row);
                void Refresh()
                {
                    var skill = currentSkill();
                    button.tooltip = skill == null ? "请先选择技能资产。" :
                        "按住左键拖到画板，松开添加。";
                    row.style.display = !(skill != null && skill.IntegratedAttacks && typeof(ReleaseModule).IsAssignableFrom(type)) && (metadata.DisplayName ?? type.Name).IndexOf(search.value ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        ? DisplayStyle.Flex : DisplayStyle.None;
                }
                search.RegisterValueChangedCallback(_ => Refresh());
                row.schedule.Execute(Refresh).Every(200); Refresh();
            }
            void RefreshGroups()
            {
                coreGroup.style.display = "技能基础".IndexOf(search.value ?? "", StringComparison.OrdinalIgnoreCase) >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
                foreach (var group in new[] { releaseGroup, areaGroup, effectGroup, responseGroup, feedbackGroup })
                    group.style.display = group.Children().Any(row => row.ClassListContains("module-row") && row.style.display.value != DisplayStyle.None)
                        ? DisplayStyle.Flex : DisplayStyle.None;
                foreach (var group in new[] { coreGroup, releaseGroup, areaGroup, effectGroup, responseGroup, feedbackGroup })
                    group.SetValueWithoutNotify(!string.IsNullOrEmpty(search.value) || expanded[group.name]);
            }
            search.RegisterValueChangedCallback(_ => RefreshGroups());
            RefreshGroups();
            schedule.Execute(RefreshGroups).Every(200);
        }
    }
}
