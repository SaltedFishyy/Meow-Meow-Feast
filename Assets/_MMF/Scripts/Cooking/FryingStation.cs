using MMF.Data;
using UnityEngine;
namespace MMF.Cooking
{
    /// <summary>Portable pan contents and Inspector frying rules.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class FryingStation : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _burntItem = null;
        [SerializeField] private SpriteRenderer _food = null;
        [SerializeField] private CookingTuning _tuning = null;
        private BoxCollider2D _collider;
        private ItemDefinition _displayedItem;
        private BoxCollider2D Collider
        {
            get
            {
                if (_collider == null) _collider = GetComponent<BoxCollider2D>();
                return _collider;
            }
        }
        public ItemDefinition BurntItem => _burntItem;
        public Vector2 Size => Vector2.Scale(Collider.size, transform.lossyScale);
        private void Awake() { _collider = GetComponent<BoxCollider2D>(); }
        /// <summary>Checks the configured player-to-station distance.</summary>
        public bool InRange(Vector2 player)
        {
            // Calculate from box geometry: disabled colliders have no usable bounds.
            Vector2 local = (Vector2)transform.InverseTransformPoint(player) - Collider.offset;
            Vector2 half = Collider.size * 0.5f;
            Vector2 closest = new Vector2(Mathf.Clamp(local.x, -half.x, half.x), Mathf.Clamp(local.y, -half.y, half.y));
            Vector2 world = transform.TransformPoint(closest + Collider.offset);
            return (player - world).sqrMagnitude <= _tuning.InteractionDistance * _tuning.InteractionDistance;
        }
        /// <summary>Updates the station's visible food marker.</summary>
        public void SetContents(ItemDefinition item)
        {
            if (_displayedItem == item) return;
            _displayedItem = item;
            _food.enabled = item != null;
            if (item != null) _food.color = item.Color;
        }
        /// <summary>Removes collision while carried.</summary>
        public void SetCarried(bool carried) { Collider.enabled = !carried; }
    }
}
