using UnityEngine;

namespace MMF.Core
{
    /// <summary>Keeps prototype ground and collider walls aligned with the fixed camera viewport.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PrototypeArena : MonoBehaviour
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private Transform _ground = null;
        [SerializeField] private BoxCollider2D _left = null;
        [SerializeField] private BoxCollider2D _right = null;
        [SerializeField] private BoxCollider2D _top = null;
        [SerializeField] private BoxCollider2D _bottom = null;
        [SerializeField, Min(0.1f)] private float _wallThickness = 1f;

        private float _lastAspect = -1f;
        private float _lastSize = -1f;

        private void OnEnable()
        {
            RefreshLayout();
        }

        private void Update()
        {
            if (_camera != null && (_camera.aspect != _lastAspect ||
                _camera.orthographicSize != _lastSize))
                RefreshLayout();
        }

        /// <summary>Fits the ground and four walls to the camera's visible XY rectangle.</summary>
        public void RefreshLayout()
        {
            if (_camera == null || _ground == null || _left == null ||
                _right == null || _top == null || _bottom == null)
                return;

            _lastAspect = _camera.aspect;
            _lastSize = _camera.orthographicSize;
            float halfWidth = _lastSize * _lastAspect;
            float halfThickness = _wallThickness * 0.5f;
            Vector2 center = _camera.transform.position;
            _ground.position = new Vector3(center.x, center.y, 0f);
            _ground.localScale = new Vector3(halfWidth * 2f, _lastSize * 2f, 1f);
            SetWall(_left, center + Vector2.left * (halfWidth + halfThickness),
                new Vector2(_wallThickness, _lastSize * 2f + _wallThickness * 2f));
            SetWall(_right, center + Vector2.right * (halfWidth + halfThickness),
                new Vector2(_wallThickness, _lastSize * 2f + _wallThickness * 2f));
            SetWall(_top, center + Vector2.up * (_lastSize + halfThickness),
                new Vector2(halfWidth * 2f, _wallThickness));
            SetWall(_bottom, center + Vector2.down * (_lastSize + halfThickness),
                new Vector2(halfWidth * 2f, _wallThickness));
        }

        private static void SetWall(BoxCollider2D wall, Vector2 position, Vector2 size)
        {
            wall.transform.position = position;
            wall.size = size;
        }
    }
}
