using System;
using System.Collections.Generic;
using System.Reflection;
using MMF.Data;
using UnityEditor;
using UnityEngine;

namespace MMF.Editor
{
    /// <summary>Discovers and persistently edits marked tuning assets.</summary>
    public sealed class TuningWindow : EditorWindow
    {
        private sealed class Group
        {
            public string Name;
            public int Order;
            public bool Expanded = true;
            public readonly List<ScriptableObject> Assets = new List<ScriptableObject>();
        }

        private readonly List<Group> _groups = new List<Group>();
        private Vector2 _scroll;

        [MenuItem("MMF/数值")]
        private static void Open()
        {
            GetWindow<TuningWindow>("数值");
        }

        private void OnEnable()
        {
            RefreshAssets();
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnProjectChange()
        {
            RefreshAssets();
            Repaint();
        }

        private void RefreshAssets()
        {
            var expanded = new Dictionary<string, bool>();
            foreach (var group in _groups)
                expanded[group.Name] = group.Expanded;
            _groups.Clear();

            foreach (var type in TypeCache.GetTypesWithAttribute<TuningGroupAttribute>())
            {
                if (type.IsAbstract || !typeof(ScriptableObject).IsAssignableFrom(type))
                    continue;

                var attribute = type.GetCustomAttribute<TuningGroupAttribute>();
                var group = _groups.Find(item => item.Name == attribute.Group);
                if (group == null)
                {
                    group = new Group { Name = attribute.Group, Order = attribute.Order };
                    if (expanded.TryGetValue(group.Name, out var isExpanded))
                        group.Expanded = isExpanded;
                    _groups.Add(group);
                }
                group.Order = Math.Min(group.Order, attribute.Order);

                foreach (var guid in AssetDatabase.FindAssets("t:" + type.Name))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                        AssetDatabase.GUIDToAssetPath(guid));
                    if (asset != null && asset.GetType() == type && !group.Assets.Contains(asset))
                        group.Assets.Add(asset);
                }
            }

            _groups.Sort((left, right) => left.Order != right.Order
                ? left.Order.CompareTo(right.Order)
                : string.CompareOrdinal(left.Name, right.Name));
            foreach (var group in _groups)
                group.Assets.Sort((left, right) => string.CompareOrdinal(
                    AssetDatabase.GetAssetPath(left), AssetDatabase.GetAssetPath(right)));
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = Mathf.Max(220f, position.width * 0.55f);
            EditorGUILayout.HelpBox("改了马上保存。Play 中改马上生效；标着'下次 Play 生效'的要重新 Play。", MessageType.Info);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var group in _groups)
            {
                group.Expanded = EditorGUILayout.Foldout(group.Expanded, group.Name, true);
                if (!group.Expanded)
                    continue;
                foreach (var asset in group.Assets)
                {
                    if (asset != null)
                        DrawAsset(asset);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawAsset(ScriptableObject asset)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(asset.name, EditorStyles.boldLabel);
            using (var serialized = new SerializedObject(asset))
            {
                serialized.Update();
                var property = serialized.GetIterator();
                var enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyPath == "m_Script")
                        continue;
                    var field = FindField(asset.GetType(), property.name);
                    var label = field?.GetCustomAttribute<TuningLabelAttribute>()?.Label
                        ?? property.displayName;
                    var tooltip = field?.GetCustomAttribute<TooltipAttribute>()?.tooltip
                        ?? property.tooltip;
                    EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), true);
                }
                if (serialized.ApplyModifiedProperties())
                    Save(asset);
            }

            if (GUILayout.Button("恢复默认值") && EditorUtility.DisplayDialog("恢复默认值",
                "确定恢复「" + asset.name + "」的默认数值？物体引用和列表保持不变。", "恢复", "取消"))
                RestoreDefaults(asset);
            EditorGUILayout.EndVertical();
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public
                    | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
                type = type.BaseType;
            }
            return null;
        }

        private static void RestoreDefaults(ScriptableObject asset)
        {
            var defaults = CreateInstance(asset.GetType());
            try
            {
                using (var source = new SerializedObject(defaults))
                using (var destination = new SerializedObject(asset))
                {
                    var property = source.GetIterator();
                    var enterChildren = true;
                    while (property.NextVisible(enterChildren))
                    {
                        enterChildren = false;
                        if (property.propertyPath == "m_Script" || property.isArray)
                            continue;
                        if (property.propertyType == SerializedPropertyType.Generic)
                        {
                            enterChildren = true;
                            continue;
                        }
                        if (IsSimpleValue(property.propertyType))
                            destination.CopyFromSerializedProperty(property);
                    }
                    if (destination.ApplyModifiedProperties())
                        Save(asset);
                }
            }
            finally
            {
                DestroyImmediate(defaults);
            }
        }

        private static bool IsSimpleValue(SerializedPropertyType type)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Character:
                    return true;
                default:
                    return false;
            }
        }

        private static void Save(ScriptableObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        private void OnUndoRedo()
        {
            foreach (var group in _groups)
            {
                foreach (var asset in group.Assets)
                {
                    if (asset != null && EditorUtility.IsDirty(asset))
                        AssetDatabase.SaveAssetIfDirty(asset);
                }
            }
            Repaint();
        }
    }
}
