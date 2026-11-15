using MMF.Cooking;
using MMF.Data;
using MMF.Truck;
using MMF.UI;
using UnityEngine;
namespace MMF.Player
{
    /// <summary>Player-owned station interaction and carrying.</summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private FryingStation _station = null;
        [SerializeField] private FryingPanel _panel = null;
        [SerializeField] private TruckArea _truck = null;
        [SerializeField] private Vector3 _carryOffset = new Vector3(0f, 1.65f, 0f);
        private PlayerBackpack _backpack;
        private PlayerMovement _movement;
        private bool _carrying;
        private const float PlacementInset = 0.05f;
        public bool Cooking { get; private set; }
        public bool CanDrop => !Cooking && !_carrying;
        private void Awake()
        {
            _backpack = GetComponent<PlayerBackpack>(); _movement = GetComponent<PlayerMovement>();
        }
        /// <summary>Applies selected-slot station rules or exits cooking.</summary>
        public void Interact()
        {
            if (Cooking) { Cancel(); return; }
            if (_carrying || !_station.InRange(transform.position)) return;
            ItemDefinition selected = _backpack.ItemAt(_backpack.Model.SelectedSlot);
            if (selected != null && selected.FryingResult != null)
            {
                int count = _station.ItemsPerBatch;
                if (_backpack.Model.CountAt(_backpack.Model.SelectedSlot) < count) { _backpack.ShowFailure(); return; }
                for (int i = 0; i < count; i++) _backpack.TakeSelected();
                ReturnContents(true);
                _station.SetContents(selected, count);
                Cooking = true; _movement.SetBlocked(true);
                _panel.Open(_station.CreateSequence(), selected);
            }
            else if (selected == null && _station.Item != null) ReturnContents(false);
            else _backpack.ShowFailure();
        }
        private void ReturnContents(bool dropOverflow)
        {
            ItemDefinition item = _station.Item;
            int remaining = _station.Count;
            while (remaining > 0 && _backpack.TryAdd(item)) remaining--;
            if (dropOverflow)
                while (remaining > 0) { _backpack.DropItem(item); remaining--; }
            _station.SetContents(item, remaining);
            if (remaining > 0) _backpack.ShowFailure();
        }
        /// <summary>Exits cooking while retaining raw contents.</summary>
        public void Cancel()
        {
            if (!Cooking) return;
            Cooking = false; _panel.Close(); _movement.SetBlocked(false);
        }
        /// <summary>Forwards a frying key during this player session.</summary>
        public void Press(char letter) { if (Cooking) _panel.Press(letter); }
        private void Update()
        {
            if (!Cooking || !_panel.Finished) return;
            ItemDefinition result = _panel.Burnt ? _station.BurntItem : _station.Item.FryingResult;
            _station.SetContents(result, _station.Count);
            Cancel(); ReturnContents(false);
        }
        /// <summary>Carries the pan or places it on a valid interior cell.</summary>
        public void Lift()
        {
            if (Cooking) return;
            if (!_carrying)
            {
                if (!_station.InRange(transform.position)) return;
                _carrying = true; _station.SetCarried(true);
                _station.transform.SetParent(transform, false); _station.transform.localPosition = _carryOffset;
                return;
            }
            if (_movement == null) _movement = GetComponent<PlayerMovement>();
            Vector2 target = (Vector2)transform.position + _movement.LastMoveDirection;
            target = _truck.SnapCell(target, _station.Size);
            if (!_truck.ContainsFootprint(target, _station.Size)) return;
            Physics2D.SyncTransforms();
            var overlaps = Physics2D.OverlapBoxAll(target, _station.Size - Vector2.one * (2f * PlacementInset), 0f);
            for (int i = 0; i < overlaps.Length; i++)
                if (!overlaps[i].isTrigger && overlaps[i].enabled) return;
            _station.transform.SetParent(null, true); _station.transform.position = target;
            _station.SetCarried(false); _carrying = false;
        }
        private void OnDisable() { if (Cooking) Cancel(); }
    }
}
