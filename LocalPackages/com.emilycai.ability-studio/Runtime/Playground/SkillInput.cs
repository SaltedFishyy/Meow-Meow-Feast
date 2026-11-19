using System;
using AbilityStudio.Model;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
namespace AbilityStudio.Playground
{
    /// <summary>Shared authored button mapping for runtime and editor previews.</summary>
    public static class SkillInput
    {
        private static readonly System.Collections.Generic.Dictionary<SkillInputKey, Key> KeyboardKeys = CreateKeyboardKeys();
        private static System.Collections.Generic.Dictionary<SkillInputKey, Key> CreateKeyboardKeys()
        {
            var keys = new System.Collections.Generic.Dictionary<SkillInputKey, Key>();
            foreach (SkillInputKey key in Enum.GetValues(typeof(SkillInputKey)))
                if (Enum.TryParse<Key>(key.ToString(), out var keyboardKey)) keys[key] = keyboardKey;
            return keys;
        }
        public static string DisplayName(SkillInputKey key) => key == SkillInputKey.MouseLeft ? "鼠标左键" : key == SkillInputKey.MouseRight ? "鼠标右键" : key == SkillInputKey.MouseMiddle ? "鼠标中键" : key.ToString().Replace("Digit", "");
        public static ButtonControl Control(SkillInputKey key)
        {
            if (key == SkillInputKey.MouseLeft) return Mouse.current?.leftButton;
            if (key == SkillInputKey.MouseRight) return Mouse.current?.rightButton;
            if (key == SkillInputKey.MouseMiddle) return Mouse.current?.middleButton;
            return Keyboard.current != null && KeyboardKeys.TryGetValue(key, out var keyboardKey) ? Keyboard.current[keyboardKey] : null;
        }
        public static SkillInputKey? MouseKey(int button) => button == 0 ? SkillInputKey.MouseLeft : button == 1 ? SkillInputKey.MouseRight : button == 2 ? SkillInputKey.MouseMiddle : (SkillInputKey?)null;
        public static SkillInputKey? EditorKey(KeyCode key)
        {
            string name = key.ToString().Replace("Alpha", "Digit").Replace("Control", "Ctrl");
            if (key == KeyCode.Return) name = "Enter";
            return Enum.TryParse<SkillInputKey>(name, out var result) ? result : (SkillInputKey?)null;
        }
    }
}
