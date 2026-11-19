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
        private BoxCollider2D Collider
        {
            get
            {
                if (_collider == null) _collider = GetComponent<BoxCollider2D>();
                return _collider;
            }
        }
        /// <summary>Raised after the station stores and displays its contents.</summary>
        public event System.Action ContentsChanged;

        public ItemDefinition Item { get; private set; }
        public int Count { get; private set; }
        public int ItemsPerBatch => _tuning.ItemsPerBatch;
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
        /// <summary>Creates a session using this station tuning.</summary>
        public FryingSequence CreateSequence() => new FryingSequence(_tuning.LettersPerRound, _tuning.RoundSeconds, new System.Random());
        /// <summary>Stores a batch and refreshes its visible marker.</summary>
        public void SetContents(ItemDefinition item, int count)
        {
            Item = count > 0 ? item : null; Count = count;
            _food.enabled = Item != null;
            if (Item != null) _food.color = Item.Color;
            ContentsChanged?.Invoke();
        }
        /// <summary>Removes collision while carried.</summary>
        public void SetCarried(bool carried) { Collider.enabled = !carried; }
    }
}
