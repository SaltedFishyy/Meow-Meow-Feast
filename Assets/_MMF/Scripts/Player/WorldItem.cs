using System.Collections.Generic;
using MMF.Data;
using UnityEngine;
namespace MMF.Player
{
    /// <summary>A single ground item with pickup and expiry timers.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WorldItem : MonoBehaviour
    {
        private static readonly List<WorldItem> Items = new List<WorldItem>();
        public static IReadOnlyList<WorldItem> ActiveItems => Items;
        [SerializeField] private ItemDefinition _item = null;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField, Min(0f)] private float _pickupDelay;
        [SerializeField, Min(0f)] private float _lifetime = 60f;
        [SerializeField, Min(0.01f)] private float _size = 0.4f;
        private float _bornAt;
        public ItemDefinition Item => _item;
        public bool CanPickUp => Time.time >= _bornAt + _pickupDelay && Time.time < _bornAt + _lifetime;
        private void Awake() { _renderer = GetComponent<SpriteRenderer>(); }
        private void OnEnable() { _bornAt = Time.time; Items.Add(this); Refresh(); }
        private void OnDisable() { Items.Remove(this); }
        private void Update()
        {
            if (Time.time < _bornAt + _lifetime) return;
            gameObject.SetActive(false); Destroy(gameObject);
        }
        /// <summary>Configures a new drop and starts fresh timers.</summary>
        public void Initialize(ItemDefinition item, Sprite sprite, float delay, float lifetime, float size)
        {
            _item = item; _renderer.sprite = sprite; _pickupDelay = delay;
            _lifetime = lifetime; _size = size; _bornAt = Time.time; Refresh();
        }
        private void Refresh()
        {
            if (_renderer != null && _item != null) _renderer.color = _item.Color;
            transform.localScale = new Vector3(_size, _size, 1f);
        }
    }
}

