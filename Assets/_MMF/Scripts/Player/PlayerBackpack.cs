using System;
using System.Collections.Generic;
using MMF.Data;
using UnityEngine;
namespace MMF.Player
{
    /// <summary>Connects inventory rules to ground items and the held marker.</summary>
    public sealed class PlayerBackpack : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _slotCount = 5;
        [SerializeField, Min(1)] private int _stackLimit = 3;
        [SerializeField, Min(0f)] private float _pickupRadius = 0.5f;
        [SerializeField, Min(0f)] private float _dropPickupDelay = 2f;
        [SerializeField, Min(0f)] private float _worldLifetime = 60f;
        [SerializeField, Min(0.01f)] private float _worldItemSize = 0.4f;
        [SerializeField] private Sprite _itemSprite = null;
        [SerializeField] private SpriteRenderer _heldIcon = null;
        [SerializeField] private bool _showHeldIcon = true;
        [SerializeField] private Vector3 _iconOffset = new Vector3(0f, 0.85f, 0f);
        [SerializeField, Min(0.01f)] private float _iconSize = 0.25f;
        [SerializeField] private Sprite _failureSprite = null;
        [SerializeField, Min(0f)] private float _failureSeconds = 1f;
        private float _failureUntil;
        private readonly Dictionary<string, ItemDefinition> _definitions = new Dictionary<string, ItemDefinition>();
        public BackpackModel Model { get; private set; }
        public event Action ContentsChanged;
        public event Action SelectionChanged;
        private void Awake()
        {
            Model = new BackpackModel(_slotCount, _stackLimit);
            Model.ContentsChanged += OnContentsChanged; Model.SelectionChanged += OnSelectionChanged;
            RefreshIcon();
        }
        public ItemDefinition ItemAt(int slot)
        {
            string id = Model.ItemAt(slot);
            return id != null && _definitions.TryGetValue(id, out ItemDefinition item) ? item : null;
        }
        public void SelectSlot(int slot) => Model.Select(slot);
        public void ScrollSlot(float direction)
        {
            if (direction != 0f) Model.Select(Model.SelectedSlot + (direction < 0f ? 1 : -1));
        }
        private void Update()
        {
            if (_failureUntil > 0f && Time.time >= _failureUntil) { _failureUntil = 0f; RefreshIcon(); }
            var items = WorldItem.ActiveItems;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                WorldItem world = items[i];
                if (!world.CanPickUp || world.Item == null ||
                    ((Vector2)(world.transform.position - transform.position)).sqrMagnitude > _pickupRadius * _pickupRadius) continue;
                if (!TryAdd(world.Item)) continue;
                world.gameObject.SetActive(false); Destroy(world.gameObject);
            }
        }
        /// <summary>Drops one item at the player position with a pickup cooldown.</summary>
        public void Drop()
        {
            ItemDefinition item = ItemAt(Model.SelectedSlot);
            if (item == null || !Model.TryDrop(out _)) return;
            DropItem(item);
        }
        /// <summary>Registers definitions before inventory change notifications.</summary>
        public bool TryAdd(ItemDefinition item)
        {
            if (item == null) return false;
            _definitions[item.Id] = item; return Model.TryAdd(item.Id);
        }
        /// <summary>Removes one item from the selected slot.</summary>
        public void TakeSelected() { Model.TryDrop(out _); }
        /// <summary>Temporarily replaces the held marker with a red failure mark.</summary>
        public void ShowFailure() { _failureUntil = Time.time + _failureSeconds; RefreshIcon(); }
        /// <summary>Creates a ground item with the same pickup delay as Q.</summary>
        public void DropItem(ItemDefinition item)
        {
            GameObject drop = new GameObject("Dropped " + item.name);
            drop.transform.position = transform.position;
            drop.AddComponent<SpriteRenderer>().sortingOrder = 1;
            drop.AddComponent<WorldItem>().Initialize(item, _itemSprite, _dropPickupDelay, _worldLifetime, _worldItemSize);
        }
        private void OnContentsChanged() { RefreshIcon(); ContentsChanged?.Invoke(); }
        private void OnSelectionChanged() { RefreshIcon(); SelectionChanged?.Invoke(); }
        private void RefreshIcon()
        {
            if (_heldIcon == null) return;
            ItemDefinition item = ItemAt(Model.SelectedSlot);
            _heldIcon.enabled = _showHeldIcon && item != null;
            _heldIcon.transform.localPosition = _iconOffset;
            _heldIcon.transform.localScale = Vector3.one * _iconSize;
            if (item != null) _heldIcon.color = item.Color;
            bool failure = Time.time < _failureUntil;
            _heldIcon.sprite = failure ? _failureSprite : _itemSprite;
            if (failure) { _heldIcon.enabled = true; _heldIcon.color = Color.red; }
        }
    }
}

