using System;
using AbilityStudio.Editor;
using AbilityStudio.Model;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MMF.Editor.WeaponWorkshop
{
    /// <summary>Edits the Ability Studio weapons in the project's weapon folder.</summary>
    public sealed class WeaponWorkshopWindow : EditorWindow
    {
        private const string WeaponFolder = "Assets/_MMF/Data/Weapons";
        private const string WindowTitle = "武器工坊";
        private const double AutoSaveIntervalSeconds = 0.3;

        private readonly System.Collections.Generic.List<SkillDefinition> _weapons = new();
        private ListView _weaponList;
        private Label _emptyMessage;
        private Toolbar _toolbar;
        private ScrollView _inspector;
        private SkillDefinition _selectedWeapon;
        private double _nextAutoSaveTime;
        private VisualElement _equipmentRow;
        private Label _equipmentStatus;
        private Button _equipmentButton;
        private SkillDefinition _markedWeapon;

        private static string SelectionKey => "MMF.WeaponWorkshop.Selection:" + Application.dataPath;

        [MenuItem("MMF/" + WindowTitle)]
        private static void Open()
        {
            GetWindow<WeaponWorkshopWindow>(WindowTitle);
        }

        private void OnEnable()
        {
            _nextAutoSaveTime = EditorApplication.timeSinceStartup + AutoSaveIntervalSeconds;
            EditorApplication.update += CheckAutoSave;
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckAutoSave;
            SaveDirtyWeapons();
        }

        private void CheckAutoSave()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now < _nextAutoSaveTime)
                return;

            _nextAutoSaveTime = now + AutoSaveIntervalSeconds;
            SaveDirtyWeapons();
            RefreshEquipment();
        }

        private void SaveDirtyWeapons()
        {
            foreach (var weapon in _weapons)
            {
                if (weapon != null && EditorUtility.IsDirty(weapon)
                    && AssetDatabase.GetAssetPath(weapon).StartsWith(WeaponFolder + "/", StringComparison.Ordinal))
                    AssetDatabase.SaveAssetIfDirty(weapon);
            }
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            _toolbar = new Toolbar();
            _toolbar.Add(new ToolbarButton(PreviewWeapon) { text = "试玩预览" });
            _toolbar.Add(new ToolbarButton(PingWeapon) { text = "在 Project 里选中" });
            var autoSaveLabel = new Label("改了自动保存") { pickingMode = PickingMode.Ignore };
            autoSaveLabel.style.fontSize = 11;
            autoSaveLabel.style.alignSelf = Align.Center;
            _toolbar.Add(autoSaveLabel);
            rootVisualElement.Add(_toolbar);

            var split = new TwoPaneSplitView(0, 180, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            rootVisualElement.Add(split);

            var sidebar = new VisualElement();
            _emptyMessage = new Label("武器文件夹中没有武器。");
            _emptyMessage.style.whiteSpace = WhiteSpace.Normal;
            sidebar.Add(_emptyMessage);
            _weaponList = new ListView
            {
                itemsSource = _weapons,
                selectionType = SelectionType.Single,
                makeItem = () =>
                {
                    var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
                    var title = new Label { name = "weapon-name" }; title.style.flexGrow = 1;
                    row.Add(title); row.Add(new Label { name = "equipment-badge" }); return row;
                },
                bindItem = (element, index) =>
                {
                    element.Q<Label>("weapon-name").text = _weapons[index].name;
                    element.Q<Label>("equipment-badge").text = WorkshopEquipment.IsEquipped(_weapons[index]) ? "P1 已装备" : "";
                }
            };
            _weaponList.style.flexGrow = 1;
            _weaponList.selectionChanged += _ => ShowWeapon(_weaponList.selectedItem as SkillDefinition);
            sidebar.Add(_weaponList);
            split.Add(sidebar);

            _inspector = new ScrollView();
            split.Add(_inspector);
            _selectedWeapon = null;
            RefreshWeapons();
        }

        private void OnProjectChange()
        {
            if (_weaponList != null)
                RefreshWeapons();
        }

        private void RefreshWeapons()
        {
            string selectedGuid = EditorPrefs.GetString(SelectionKey);
            string[] guids = AssetDatabase.IsValidFolder(WeaponFolder)
                ? AssetDatabase.FindAssets("t:SkillDefinition", new[] { WeaponFolder })
                : Array.Empty<string>();
            string[] paths = Array.ConvertAll(guids, AssetDatabase.GUIDToAssetPath);
            Array.Sort(paths, StringComparer.Ordinal);

            _weapons.Clear();
            foreach (string path in paths)
            {
                var weapon = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
                if (weapon != null)
                    _weapons.Add(weapon);
            }

            int selectedIndex = _weapons.FindIndex(weapon =>
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(weapon)) == selectedGuid);
            if (selectedIndex < 0 && _weapons.Count > 0)
                selectedIndex = 0;

            _weaponList.SetSelectionWithoutNotify(selectedIndex < 0 ? Array.Empty<int>() : new[] { selectedIndex });
            _weaponList.RefreshItems();
            _emptyMessage.style.display = _weapons.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            ShowWeapon(selectedIndex < 0 ? null : _weapons[selectedIndex]);
        }

        private void ShowWeapon(SkillDefinition weapon)
        {
            _toolbar.SetEnabled(weapon != null);
            if (ReferenceEquals(_selectedWeapon, weapon))
                return;

            SaveDirtyWeapons();
            _selectedWeapon = weapon;
            _inspector.Clear();
            if (weapon == null)
                return;

            EditorPrefs.SetString(SelectionKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(weapon)));
            var inspector = new InspectorElement(weapon);
            _inspector.Add(inspector);
            _equipmentRow = new VisualElement { name = "main-scene-equipment" };
            _equipmentRow.style.flexDirection = FlexDirection.Row;
            _equipmentRow.style.alignItems = Align.Center;
            _equipmentRow.style.marginBottom = 8;
            _equipmentRow.Add(new Label("主场景装备") { style = { minWidth = 120 } });
            _equipmentStatus = new Label(); _equipmentStatus.style.flexGrow = 1;
            _equipmentRow.Add(_equipmentStatus);
            _equipmentButton = new Button(() =>
            {
                WorkshopEquipment.Equip(WorkshopEquipment.IsEquipped(_selectedWeapon) ? null : _selectedWeapon);
                RefreshEquipment();
            }) { name = "equip-p1" };
            _equipmentRow.Add(_equipmentButton);
            // The shared inspector rebuilds after graph edits; reinsert directly after its name field.
            inspector.schedule.Execute(() =>
            {
                var nameField = inspector.Q<TextField>("displayName");
                if (nameField != null && _equipmentRow.parent != nameField.parent)
                    nameField.parent.Insert(nameField.parent.IndexOf(nameField) + 1, _equipmentRow);
            }).Every(100);
            RefreshEquipment();
            _inspector.scrollOffset = Vector2.zero;
        }

        private void RefreshEquipment()
        {
            var loadout = WorkshopEquipment.Load();
            if (loadout != null && EditorUtility.IsDirty(loadout)) AssetDatabase.SaveAssetIfDirty(loadout);
            var current = loadout == null ? null : loadout.PlayerOne;
            if (_markedWeapon != current) { _markedWeapon = current; _weaponList?.RefreshItems(); }
            if (_equipmentStatus == null || _selectedWeapon == null) return;
            bool equipped = current == _selectedWeapon;
            _equipmentStatus.text = equipped ? "P1 已装备" : "未装备";
            _equipmentButton.text = equipped ? "卸下" : "装备给 P1";
            _equipmentButton.tooltip = equipped ? "卸下后恢复背包武器。" : "替换 P1 当前工坊武器，进入主场景后接管左键攻击。";
        }

        private void PreviewWeapon()
        {
            if (_selectedWeapon != null)
                PlaygroundWindow.OpenSkill(_selectedWeapon);
        }

        private void PingWeapon()
        {
            if (_selectedWeapon == null)
                return;

            Selection.activeObject = _selectedWeapon;
            EditorGUIUtility.PingObject(_selectedWeapon);
        }
    }
}
