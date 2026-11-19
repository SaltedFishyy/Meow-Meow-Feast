using UnityEngine;
using AbilityStudio.Model;
namespace MMF.Data
{
    /// <summary>Shared item identity and placeholder appearance.</summary>
    [CreateAssetMenu(menuName = "MMF/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id = "";
        [SerializeField] private string _displayName = "";
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private ItemDefinition _fryingResult = null;
        [SerializeField] private bool _neverExpiresOnGround = false;
        [SerializeField] private WeaponDefinition _weapon = null;
        private SkillDefinition _workshopWeapon;
        public SkillDefinition WorkshopWeapon => _workshopWeapon;
        /// <summary>Inventory representation of the workshop weapon selected for this play session.</summary>
        public static ItemDefinition CreateWorkshopWeapon(SkillDefinition skill)
        {
            var item = CreateInstance<ItemDefinition>();
            item.name = skill.DisplayName; item.hideFlags = HideFlags.DontSave;
            item._id = "WorkshopWeapon:" + skill.Id;
            item._displayName = skill.DisplayName; item._color = Color.white;
            item._neverExpiresOnGround = true; item._workshopWeapon = skill;
            return item;
        }
        public WeaponDefinition Weapon => _weapon;
        public bool NeverExpiresOnGround => _neverExpiresOnGround;
        public ItemDefinition FryingResult => _fryingResult;
        public string Id => _id;
        public string DisplayName => _displayName;
        public Color Color => _color;
    }
}
