using MMF.Data;
using UnityEngine;

namespace MMF.Truck
{
    /// <summary>Placeholder truck geometry and interior position checks.</summary>
    public sealed class TruckArea : MonoBehaviour
    {
        [SerializeField] private TruckTuning _tuning = null;
        [SerializeField, Min(0.01f)] private float _wallThickness = 0.5f;
        [SerializeField] private Transform _floor = null;
        [SerializeField] private Transform[] _walls = new Transform[6];
        [SerializeField] private Transform[] _doorBlockers = new Transform[2];
        private Vector2 _size;

        /// <summary>Tests the position against the wall inner faces.</summary>
        public bool Contains(Vector2 position)
        {
            Vector2 local = position - (Vector2)transform.position;
            return Mathf.Abs(local.x) < _size.x * 0.5f - _wallThickness &&
                   Mathf.Abs(local.y) < _size.y * 0.5f - _wallThickness;
        }
        /// <summary>Returns the center of the front (right) or rear (left) outer wall face.</summary>
        public Vector2 EndPoint(bool front)
        {
            return (Vector2)transform.position + Vector2.right * (front ? _size.x : -_size.x) * 0.5f;
        }
        private const float PlacementTolerance = 0.0001f;
        /// <summary>World rectangle bounded by the wall outer faces.</summary>
        public Rect ExteriorRect => new Rect((Vector2)transform.position - _size * 0.5f, _size);
        /// <summary>Interior rectangle bounded by the wall inner faces.</summary>
        public Rect InteriorRect => new Rect((Vector2)transform.position - _size * 0.5f + Vector2.one * _wallThickness,
            _size - Vector2.one * (2f * _wallThickness));
        /// <summary>Snaps to cell centers measured from the interior lower-left corner.</summary>
        public Vector2 SnapCell(Vector2 position, Vector2 cellSize)
        {
            Vector2 origin = InteriorRect.min + cellSize * 0.5f;
            Vector2 delta = position - origin;
            return origin + new Vector2(Mathf.Round(delta.x / cellSize.x) * cellSize.x,
                Mathf.Round(delta.y / cellSize.y) * cellSize.y);
        }
        /// <summary>Checks the complete footprint, allowing contact with inner walls.</summary>
        public bool ContainsFootprint(Vector2 center, Vector2 size)
        {
            Rect rect = InteriorRect;
            Vector2 min = center - size * 0.5f, max = center + size * 0.5f;
            return min.x >= rect.xMin - PlacementTolerance && min.y >= rect.yMin - PlacementTolerance &&
                max.x <= rect.xMax + PlacementTolerance && max.y <= rect.yMax + PlacementTolerance;
        }
        /// <summary>Returns the top-left complete cell in the interior grid.</summary>
        public Vector2 TopLeftCell(Vector2 cellSize)
        {
            Rect rect = InteriorRect;
            return rect.min + new Vector2(cellSize.x * 0.5f,
                (Mathf.Floor((rect.height + PlacementTolerance) / cellSize.y) - 0.5f) * cellSize.y);
        }
        private void Awake() { RefreshLayout(); }
        /// <summary>Applies configured dimensions to the floor, walls and door blockers.</summary>
        public void RefreshLayout()
        {
            // Keep the one-cell station clear of the player at the center and leave room to walk.
            float wallAllowance = 2f * _wallThickness;
            _size = new Vector2(Mathf.Max(_tuning.Length, 3f + wallAllowance),
                Mathf.Max(_tuning.Width, 2f + wallAllowance));
            float minimumDoorOpening = 1f + 2f * Physics2D.defaultContactOffset;
            float doorWidth = Mathf.Clamp(_tuning.DoorWidth, minimumDoorOpening, _size.x - wallAllowance);
            _floor.localScale = new Vector3(_size.x, _size.y, 1f);
            float sideX = (_size.x - _wallThickness) * 0.5f;
            SetPart(_walls[0], new Vector2(-sideX, 0f), new Vector2(_wallThickness, _size.y));
            SetPart(_walls[1], new Vector2(sideX, 0f), new Vector2(_wallThickness, _size.y));
            float width = (_size.x - doorWidth) * 0.5f;
            float x = (_size.x + doorWidth) * 0.25f;
            float y = (_size.y - _wallThickness) * 0.5f;
            SetPart(_walls[2], new Vector2(-x, y), new Vector2(width, _wallThickness));
            SetPart(_walls[3], new Vector2(x, y), new Vector2(width, _wallThickness));
            SetPart(_walls[4], new Vector2(-x, -y), new Vector2(width, _wallThickness));
            SetPart(_walls[5], new Vector2(x, -y), new Vector2(width, _wallThickness));
            for (int i = 0; i < _doorBlockers.Length; i++)
                if (_doorBlockers[i] != null)
                    SetPart(_doorBlockers[i], new Vector2(0f, i == 0 ? y : -y), new Vector2(doorWidth, _wallThickness));
        }
        private static void SetPart(Transform part, Vector2 position, Vector2 size)
        {
            part.localPosition = position;
            part.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
