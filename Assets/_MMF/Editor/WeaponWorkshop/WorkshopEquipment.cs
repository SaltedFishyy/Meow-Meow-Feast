using AbilityStudio.Model;
using MMF.Data;
using UnityEditor;
using UnityEngine;
namespace MMF.Editor.WeaponWorkshop
{
    /// <summary>Creates and saves the shared main-scene loadout without editing scenes.</summary>
    public static class WorkshopEquipment
    {
        public const string AssetPath = "Assets/_MMF/Data/Resources/MMFWorkshopLoadout.asset";
        public static WorkshopLoadout Load() => AssetDatabase.LoadAssetAtPath<WorkshopLoadout>(AssetPath);
        public static bool IsEquipped(SkillDefinition weapon) => weapon != null && Load() != null && Load().PlayerOne == weapon;
        /// <summary>Persists equipment immediately and supports editor Undo.</summary>
        public static void Equip(SkillDefinition weapon)
        {
            var loadout = Load();
            if (loadout == null)
            {
                if (weapon == null) return;
                if (!AssetDatabase.IsValidFolder("Assets/_MMF/Data/Resources"))
                    AssetDatabase.CreateFolder("Assets/_MMF/Data", "Resources");
                loadout = ScriptableObject.CreateInstance<WorkshopLoadout>();
                AssetDatabase.CreateAsset(loadout, AssetPath);
            }
            Undo.RecordObject(loadout, weapon == null ? "卸下 P1 武器" : "装备给 P1");
            loadout.EquipPlayerOne(weapon);
            EditorUtility.SetDirty(loadout);
            AssetDatabase.SaveAssetIfDirty(loadout);
        }
    }
}
