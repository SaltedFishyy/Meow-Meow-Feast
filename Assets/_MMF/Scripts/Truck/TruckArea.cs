using System;
using UnityEngine;

namespace MMF.Truck
{
    /// <summary>Placeholder truck geometry and player occupancy.</summary>
    public sealed class TruckArea : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(12f, 7f);
        [SerializeField, Min(0.1f)] private float _doorWidth = 2f;
        [SerializeField, Min(0.01f)] private float _wallThickness = 0.5f;
        [SerializeField] private Transform _floor = null;
        [SerializeField] private Transform[] _walls = new Transform[6];
        [SerializeField] private Transform _player = null;
        /// <summary>Raised when the tracked player enters or leaves the interior.</summary>
        public event Action<bool> OccupancyChanged;
        /// <summary>Current tracked player occupancy.</summary>
        public bool IsPlayerInside { get; private set; }
        /// <summary>Tests the position against the wall inner faces.</summary>
        public bool Contains(Vector2 position)
        {
            Vector2 local = position - (Vector2)transform.position;
            return Mathf.Abs(local.x) < _size.x * 0.5f - _wallThickness &&
                   Mathf.Abs(local.y) < _size.y * 0.5f - _wallThickness;
        }
        private void Start() { RefreshLayout(); IsPlayerInside = Contains(_player.position); }
        private void LateUpdate()
        {
            bool inside = Contains(_player.position);
            if (inside == IsPlayerInside) return;
            IsPlayerInside = inside;
            OccupancyChanged?.Invoke(inside);
        }
        /// <summary>Applies Inspector dimensions to floor and six wall segments.</summary>
        public void RefreshLayout()
        {
            _floor.localScale = new Vector3(_size.x, _size.y, 1f);
            float sideX = (_size.x - _wallThickness) * 0.5f;
            SetPart(_walls[0], new Vector2(-sideX, 0f), new Vector2(_wallThickness, _size.y));
            SetPart(_walls[1], new Vector2(sideX, 0f), new Vector2(_wallThickness, _size.y));
            float width = (_size.x - _doorWidth) * 0.5f;
            float x = (_size.x + _doorWidth) * 0.25f;
            float y = (_size.y - _wallThickness) * 0.5f;
            SetPart(_walls[2], new Vector2(-x, y), new Vector2(width, _wallThickness));
            SetPart(_walls[3], new Vector2(x, y), new Vector2(width, _wallThickness));
            SetPart(_walls[4], new Vector2(-x, -y), new Vector2(width, _wallThickness));
            SetPart(_walls[5], new Vector2(x, -y), new Vector2(width, _wallThickness));
        }
        private static void SetPart(Transform part, Vector2 position, Vector2 size)
        {
            part.localPosition = position;
            part.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
