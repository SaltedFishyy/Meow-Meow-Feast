using MMF.Truck;
using UnityEngine;

namespace MMF.Core
{
    /// <summary>Follows the interpolated player transform and bounds the current viewport.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target = null;
        [SerializeField] private MapBounds _map = null;
        [SerializeField] private TruckArea _truck = null;
        [SerializeField, Min(0.01f)] private float _insideSize = 5f;
        [SerializeField, Min(0.01f)] private float _outsideSize = 7f;
        [SerializeField, Min(0f)] private float _zoomDuration = 0.5f;
        [SerializeField, Min(0f)] private float _followTime = 0.2f;
        private Camera _camera;
        private Vector3 _velocity;
        private bool _inside;
        private float _zoomStart;
        private float _zoomElapsed;
        /// <summary>Outdoor half-height used for UI clearance.</summary>
        public float OutsideSize => _outsideSize;
        private void Awake() { _camera = GetComponent<Camera>(); }
        private void Start()
        {
            _inside = _truck.Contains(_target.position);
            _camera.orthographicSize = _inside ? _insideSize : _outsideSize;
            _zoomStart = _camera.orthographicSize;
            _zoomElapsed = _zoomDuration;
            ApplyBounds(_target.position);
        }
        private void LateUpdate()
        {
            bool inside = _truck.Contains(_target.position);
            if (inside != _inside)
            {
                _inside = inside;
                _zoomStart = _camera.orthographicSize;
                _zoomElapsed = 0f;
            }
            _zoomElapsed += Time.deltaTime;
            float t = _zoomDuration <= 0f ? 1f : Mathf.Clamp01(_zoomElapsed / _zoomDuration);
            _camera.orthographicSize = Mathf.Lerp(_zoomStart, _inside ? _insideSize : _outsideSize, Mathf.SmoothStep(0f, 1f, t));
            Vector3 target = _target.position;
            target.z = transform.position.z;
            Vector3 position = _followTime <= 0f ? target : Vector3.SmoothDamp(transform.position, target, ref _velocity, _followTime);
            ApplyBounds(position);
        }
        private void ApplyBounds(Vector3 position)
        {
            Rect rect = _map.WorldRect;
            // Fit extreme Game-view aspect ratios without exposing outside the map.
            float aspect = Mathf.Max(0.001f, _camera.aspect);
            _camera.orthographicSize = Mathf.Min(_camera.orthographicSize,
                Mathf.Min(rect.height * 0.5f, rect.width * 0.5f / aspect));
            float y = _camera.orthographicSize;
            float x = y * aspect;
            position.x = Mathf.Clamp(position.x, rect.xMin + x, rect.xMax - x);
            position.y = Mathf.Clamp(position.y, rect.yMin + y, rect.yMax - y);
            position.z = transform.position.z;
            transform.position = position;
        }
    }
}
