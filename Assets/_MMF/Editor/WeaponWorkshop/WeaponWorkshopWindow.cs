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

        private readonly System.Collections.Generic.List<SkillDefinition> _weapons = new();
        private ListView _weaponList;
        private Label _emptyMessage;
        private Toolbar _toolbar;
        private ScrollView _inspector;
        private SkillDefinition _selectedWeapon;

        private static string SelectionKey => "MMF.WeaponWorkshop.Selection:" + Application.dataPath;

        [MenuItem("MMF/" + WindowTitle)]
        private static void Open()
        {
            GetWindow<WeaponWorkshopWindow>(WindowTitle);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            _toolbar = new Toolbar();
            _toolbar.Add(new ToolbarButton(PreviewWeapon) { text = "试玩预览" });
            _toolbar.Add(new ToolbarButton(PingWeapon) { text = "在 Project 里选中" });
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
                makeItem = () => new Label(),
                bindItem = (element, index) => ((Label)element).text = _weapons[index].name
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

            _selectedWeapon = weapon;
            _inspector.Clear();
            if (weapon == null)
                return;

            EditorPrefs.SetString(SelectionKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(weapon)));
            _inspector.Add(new InspectorElement(weapon));
            _inspector.scrollOffset = Vector2.zero;
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
