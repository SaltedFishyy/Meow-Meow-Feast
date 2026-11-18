using MMF.Data;
using UnityEngine;

namespace MMF.Core
{
    /// <summary>Fixed map geometry with a backpack-safe lower collision boundary.</summary>
    public sealed class MapBounds : MonoBehaviour
    {
        [SerializeField] private MapTuning _tuning = null;
        [SerializeField, Min(0.1f)] private float _wallThickness = 1f;
        [SerializeField, Min(0f)] private float _bottomMargin = 0.15f;
        [SerializeField] private Transform _ground = null;
        [SerializeField] private BoxCollider2D _left = null;
        [SerializeField] private BoxCollider2D _right = null;
        [SerializeField] private BoxCollider2D _top = null;
        [SerializeField] private BoxCollider2D _bottom = null;
        [SerializeField] private Camera _camera = null;
        [SerializeField] private FollowCamera _follow = null;
        [SerializeField] private RectTransform _backpackRow = null;
        private readonly Vector3[] _corners = new Vector3[4];
        /// <summary>Ground rectangle in world coordinates.</summary>
        public Rect WorldRect
        {
            get
            {
                Vector2 size = new Vector2(_tuning.Width, _tuning.Height);
                return new Rect((Vector2)transform.position - size * 0.5f, size);
            }
        }
        /// <summary>Screen-height fraction below the top of the backpack row.</summary>
        public float BackpackScreenFraction { get; private set; }
        private void Start() { RefreshLayout(); }
        private void LateUpdate() { RefreshLayout(); }
        /// <summary>Updates ground and walls from map dimensions and actual screen UI height.</summary>
        public void RefreshLayout()
        {
            if (_ground == null || _bottom == null || _camera == null || _follow == null || _backpackRow == null) return;
            Rect rect = WorldRect;
            _ground.position = transform.position;
            _ground.localScale = new Vector3(rect.width, rect.height, 1f);
            _backpackRow.GetWorldCorners(_corners);
            float screenTop = RectTransformUtility.WorldToScreenPoint(null, _corners[1]).y;
            float fraction = Mathf.Clamp01((screenTop - _camera.pixelRect.yMin) / Mathf.Max(1f, _camera.pixelHeight));
            BackpackScreenFraction = fraction;
            float bottom = rect.yMin + 2f * _follow.OutsideSize * fraction + _bottomMargin;
            float half = _wallThickness * 0.5f;
            SetWall(_left, new Vector2(rect.xMin - half, rect.center.y), new Vector2(_wallThickness, rect.height + 2f * _wallThickness));
            SetWall(_right, new Vector2(rect.xMax + half, rect.center.y), new Vector2(_wallThickness, rect.height + 2f * _wallThickness));
            SetWall(_top, new Vector2(rect.center.x, rect.yMax + half), new Vector2(rect.width, _wallThickness));
            SetWall(_bottom, new Vector2(rect.center.x, bottom - half), new Vector2(rect.width, _wallThickness));
        }
        private static void SetWall(BoxCollider2D wall, Vector2 position, Vector2 size)
        {
            wall.transform.position = position;
            wall.size = size;
        }
    }
}
