using MMF.Data;
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
        [SerializeField] private CameraTuning _tuning = null;
        private Camera _camera;
        private Vector3 _velocity;
        private bool _inside;
        private float _zoomStart;
        private float _zoomElapsed;
        /// <summary>Outdoor half-height used for UI clearance.</summary>
        public float OutsideSize => _tuning.OutsideSize;
        private void Awake() { _camera = GetComponent<Camera>(); }
        private void Start()
        {
            _inside = _truck.Contains(_target.position);
            _camera.orthographicSize = _inside ? _tuning.InsideSize : _tuning.OutsideSize;
            _zoomStart = _camera.orthographicSize;
            _zoomElapsed = _tuning.ZoomDuration;
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
            float t = _tuning.ZoomDuration <= 0f ? 1f : Mathf.Clamp01(_zoomElapsed / _tuning.ZoomDuration);
            _camera.orthographicSize = Mathf.Lerp(_zoomStart, _inside ? _tuning.InsideSize : _tuning.OutsideSize, Mathf.SmoothStep(0f, 1f, t));
            Vector3 target = _target.position;
            target.z = transform.position.z;
            Vector3 position = _tuning.FollowTime <= 0f ? target : Vector3.SmoothDamp(transform.position, target, ref _velocity, _tuning.FollowTime);
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
