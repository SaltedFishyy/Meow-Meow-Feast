using System.Collections.Generic;
using MMF.Cooking;
using MMF.Truck;
using UnityEngine;
namespace MMF.Player
{
    /// <summary>Player-owned station interaction and carrying.</summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private FryingStation _station = null;
        [SerializeField] private TruckArea _truck = null;
        [SerializeField] private SpriteRenderer _stationRenderer = null;
        [SerializeField] private SpriteRenderer _placementShadow = null;
        [SerializeField] private SpriteRenderer _placementFailure = null;
        [SerializeField, Range(0f, 1f)] private float _placementAlpha = 0.5f;
        [SerializeField] private Vector3 _carryOffset = new Vector3(0f, 1.65f, 0f);
        private PlayerMovement _movement;
        private bool _carrying;
        private Vector3 _stationPositionBeforeLift;
        private readonly List<Collider2D> _placementOverlaps = new List<Collider2D>(16);
        private const float PlacementInset = 0.05f;
        public bool CanDrop => !_carrying;
        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
        }
        /// <summary>Carries the pan or places it on a valid interior cell.</summary>
        public bool Lift()
        {
            if (!_carrying)
            {
                if (!_station.InRange(transform.position)) return false;
                _stationPositionBeforeLift = _station.transform.position;
                _carrying = true; _station.SetCarried(true);
                _station.transform.SetParent(transform, false); _station.transform.localPosition = _carryOffset;
                return true;
            }
            // EditMode callers may invoke Lift before Awake has run.
            if (_movement == null) _movement = GetComponent<PlayerMovement>();
            if (!TryGetPlacement(out Vector2 target)) return true;
            _station.transform.SetParent(null, true); _station.transform.position = target;
            _station.SetCarried(false); _carrying = false;
            HidePlacement();
            return true;
        }
        /// <summary>Sets down the station on this cell or returns it to its last position.</summary>
        public void DropCarriedStation()
        {
            if (!_carrying) return;
            bool valid = TryGetPlacement(transform.position, out Vector2 target);
            _station.transform.SetParent(null, true);
            _station.transform.position = valid ? (Vector3)target : _stationPositionBeforeLift;
            _station.SetCarried(false);
            _carrying = false;
            HidePlacement();
        }
        private bool TryGetPlacement(out Vector2 target) =>
            TryGetPlacement((Vector2)transform.position + _movement.LastMoveDirection, out target);

        private bool TryGetPlacement(Vector2 position, out Vector2 target)
        {
            target = _truck.SnapCell(position, _station.Size);
            if (!_truck.ContainsFootprint(target, _station.Size)) return false;
            Physics2D.SyncTransforms();
            Physics2D.OverlapBox(target, _station.Size - Vector2.one * (2f * PlacementInset), 0f,
                ContactFilter2D.noFilter, _placementOverlaps);
            for (int i = 0; i < _placementOverlaps.Count; i++)
                if (!_placementOverlaps[i].isTrigger && _placementOverlaps[i].enabled) return false;
            return true;
        }
        private void LateUpdate()
        {
            if (!_carrying) { HidePlacement(); return; }
            bool valid = TryGetPlacement(out Vector2 target);
            _placementShadow.transform.position = target;
            _placementShadow.transform.rotation = _stationRenderer.transform.rotation;
            _placementShadow.transform.localScale = _stationRenderer.transform.lossyScale;
            _placementShadow.sprite = _stationRenderer.sprite;
            _placementShadow.drawMode = _stationRenderer.drawMode;
            _placementShadow.size = _stationRenderer.size;
            _placementShadow.flipX = _stationRenderer.flipX;
            _placementShadow.flipY = _stationRenderer.flipY;
            Color color = _stationRenderer.color;
            color.a = _placementAlpha;
            _placementShadow.color = color;
            _placementShadow.enabled = true;
            _placementFailure.transform.position = target;
            _placementFailure.enabled = !valid;
        }
        private void HidePlacement()
        {
            if (_placementShadow != null) _placementShadow.enabled = false;
            if (_placementFailure != null) _placementFailure.enabled = false;
        }
        private void OnDisable() { HidePlacement(); }
    }
}
