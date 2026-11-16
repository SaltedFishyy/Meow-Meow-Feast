using UnityEngine;
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
        public WeaponDefinition Weapon => _weapon;
        public bool NeverExpiresOnGround => _neverExpiresOnGround;
        public ItemDefinition FryingResult => _fryingResult;
        public string Id => _id;
        public string DisplayName => _displayName;
        public Color Color => _color;
    }
}
