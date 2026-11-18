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
        private Vector3 _positionStart;
        private Vector3 _followPosition;
        private bool _positionTransition;
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
            Canvas.ForceUpdateCanvases();
            _map.RefreshLayout();
            ApplyBounds(_inside ? InsidePosition() : (_tuning.FixedOutside ? OutsideCenter() : _target.position));
            _followPosition = transform.position;
        }
        private void LateUpdate()
        {
            bool inside = _truck.Contains(_target.position);
            if (inside != _inside)
            {
                _inside = inside;
                _zoomStart = _camera.orthographicSize;
                _zoomElapsed = 0f;
                _positionStart = transform.position;
                _followPosition = transform.position;
                _velocity = Vector3.zero;
                _positionTransition = true;
            }
            _zoomElapsed += Time.deltaTime;
            float t = _tuning.ZoomDuration <= 0f ? 1f : Mathf.Clamp01(_zoomElapsed / _tuning.ZoomDuration);
            _camera.orthographicSize = Mathf.Lerp(_zoomStart, _inside ? _tuning.InsideSize : _tuning.OutsideSize, Mathf.SmoothStep(0f, 1f, t));
            Vector3 target = _target.position;
            target.z = transform.position.z;
            Vector3 position;
            if (_inside)
            {
                position = InsidePosition();
            }
            else if (_tuning.FixedOutside)
            {
                position = OutsideCenter();
                _followPosition = transform.position;
                _velocity = Vector3.zero;
            }
            else
            {
                Vector3 followStart = _positionTransition ? _followPosition : transform.position;
                // Keep the moving follow target independent of the transition blend.
                _followPosition = _tuning.FollowTime <= 0f ? target : Vector3.SmoothDamp(followStart, target, ref _velocity, _tuning.FollowTime);
                position = _followPosition;
            }
            if (_positionTransition)
            {
                position = Vector3.Lerp(_positionStart, position, Mathf.SmoothStep(0f, 1f, t));
                _positionTransition = t < 1f;
            }
            ApplyBounds(position);
        }
        private Vector3 InsidePosition()
        {
            Vector3 position = _truck.transform.position;
            position.y -= _camera.orthographicSize * _map.BackpackScreenFraction;
            position.z = transform.position.z;
            return position;
        }
        private Vector3 OutsideCenter()
        {
            Vector3 position = _map.WorldRect.center;
            position.z = transform.position.z;
            return position;
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
