using System;
using System.Collections.Generic;
using System.Reflection;
using MMF.Data;
using UnityEditor;
using UnityEditorInternal;
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
        private readonly Dictionary<ScriptableObject, SerializedObject> _serializedAssets = new Dictionary<ScriptableObject, SerializedObject>();
        private readonly Dictionary<string, ReorderableList> _lists = new Dictionary<string, ReorderableList>();
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
            _lists.Clear();
            foreach (var serialized in _serializedAssets.Values)
                serialized.Dispose();
            _serializedAssets.Clear();
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

        private void DrawAsset(ScriptableObject asset)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(asset.name, EditorStyles.boldLabel);
            if (!_serializedAssets.TryGetValue(asset, out var serialized))
            {
                serialized = new SerializedObject(asset);
                _serializedAssets.Add(asset, serialized);
            }
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
                    if (property.propertyType == SerializedPropertyType.Generic || property.isArray
                        && property.propertyType != SerializedPropertyType.String)
                    {
                        var height = GetFieldHeight(property, field?.FieldType);
                        DrawField(EditorGUILayout.GetControlRect(false, height), property, field,
                            field?.FieldType);
                    }
                    else
                        EditorGUILayout.PropertyField(property, GetLabel(property, field), true);
                }
                if (serialized.ApplyModifiedProperties())
                    Save(asset);
            }

            if (GUILayout.Button("恢复默认值") && EditorUtility.DisplayDialog("恢复默认值",
                "确定恢复「" + asset.name + "」的默认数值？物体引用和列表保持不变。", "恢复", "取消"))
                RestoreDefaults(asset);
            EditorGUILayout.EndVertical();
        }

        private static GUIContent GetLabel(SerializedProperty property, FieldInfo field)
        {
            return new GUIContent(field?.GetCustomAttribute<TuningLabelAttribute>()?.Label
                ?? property.displayName, field?.GetCustomAttribute<TooltipAttribute>()?.tooltip
                ?? property.tooltip);
        }

        private ReorderableList GetList(SerializedProperty property, Type type)
        {
            var key = property.serializedObject.targetObject.GetEntityId() + ":" + property.propertyPath;
            if (!_lists.TryGetValue(key, out var list))
            {
                list = new ReorderableList(property.serializedObject, property.Copy(), true, false, true, true);
                _lists.Add(key, list);
                var elementType = type?.IsArray == true ? type.GetElementType()
                    : type?.IsGenericType == true ? type.GetGenericArguments()[0] : null;
                list.elementHeightCallback = index => GetFieldHeight(
                    list.serializedProperty.GetArrayElementAtIndex(index), elementType)
                    + EditorGUIUtility.standardVerticalSpacing;
                list.drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.height = GetFieldHeight(
                        list.serializedProperty.GetArrayElementAtIndex(index), elementType);
                    DrawField(rect, list.serializedProperty.GetArrayElementAtIndex(index), null, elementType,
                        new GUIContent("元素 " + index));
                };
            }
            list.serializedProperty = property.Copy();
            return list;
        }

        private float GetFieldHeight(SerializedProperty property, Type type)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
                return EditorGUIUtility.singleLineHeight + (property.isExpanded
                    ? EditorGUIUtility.standardVerticalSpacing + GetList(property, type).GetHeight() : 0f);
            if (property.propertyType != SerializedPropertyType.Generic)
                return EditorGUI.GetPropertyHeight(property, true);
            var height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
                return height;
            using (var child = property.Copy())
            {
                var end = child.GetEndProperty();
                if (child.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(child, end))
                            break;
                        var field = FindField(type, child.name);
                        height += EditorGUIUtility.standardVerticalSpacing + GetFieldHeight(child, field?.FieldType);
                    }
                    while (child.NextVisible(false));
                }
            }
            return height;
        }

        private void DrawField(Rect rect, SerializedProperty property, FieldInfo field, Type type,
            GUIContent label = null)
        {
            label = label ?? GetLabel(property, field);
            var isList = property.isArray && property.propertyType != SerializedPropertyType.String;
            if (!isList && property.propertyType != SerializedPropertyType.Generic)
            {
                EditorGUI.PropertyField(rect, property, label, true);
                return;
            }
            var line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            if (isList)
                line.width -= 52f;
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);
            if (isList)
            {
                var sizeRect = new Rect(rect.xMax - 48f, rect.y, 48f, line.height);
                EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("Array.size"), GUIContent.none);
            }
            if (!property.isExpanded)
                return;
            line.y += line.height + EditorGUIUtility.standardVerticalSpacing;
            if (isList)
            {
                GetList(property, type).DoList(new Rect(rect.x, line.y, rect.width,
                    rect.height - line.height - EditorGUIUtility.standardVerticalSpacing));
                return;
            }
            EditorGUI.indentLevel++;
            using (var child = property.Copy())
            {
                var end = child.GetEndProperty();
                if (child.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(child, end))
                            break;
                        var childField = FindField(type, child.name);
                        line.height = GetFieldHeight(child, childField?.FieldType);
                        DrawField(line, child, childField, childField?.FieldType);
                        line.y += line.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (child.NextVisible(false));
                }
            }
            EditorGUI.indentLevel--;
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
