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
        public ItemDefinition FryingResult => _fryingResult;
        public string Id => _id;
        public string DisplayName => _displayName;
        public Color Color => _color;
    }
}
