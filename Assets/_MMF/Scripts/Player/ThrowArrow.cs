using UnityEngine;

namespace MMF.Player
{
    /// <summary>Displays a reusable line and arrowhead for the current charge.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class ThrowArrow : MonoBehaviour
    {
        [SerializeField] private PlayerThrow _throw = null;
        [SerializeField] private MeshFilter _meshFilter = null;
        [SerializeField] private MeshRenderer _meshRenderer = null;
        [SerializeField, Min(0f)] private float _lineWidth = 0.06f;
        [SerializeField, Min(0f)] private float _headLength = 0.3f;
        [SerializeField, Min(0f)] private float _headWidth = 0.25f;
        [SerializeField] private Color _color = new Color(1f, 1f, 1f, 0.5f);
        private readonly Vector3[] _vertices = new Vector3[7];
        private readonly Color[] _colors = new Color[7];
        private readonly int[] _triangles = { 0, 1, 2, 0, 2, 3, 4, 5, 6 };
        private Mesh _mesh;
        private float _distance = -1f;
        private float _meshLineWidth;
        private float _meshHeadLength;
        private float _meshHeadWidth;
        private Color _meshColor;

        private void Awake()
        {
            _mesh = new Mesh { name = "Throw Arrow" };
            _meshFilter.sharedMesh = _mesh;
            _meshRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            _meshRenderer.enabled = _throw.Charging;
            if (!_throw.Charging) return;
            float distance = _throw.Distance;
            if (_distance != distance || _meshLineWidth != _lineWidth ||
                _meshHeadLength != _headLength || _meshHeadWidth != _headWidth || _meshColor != _color)
                RebuildMesh(distance);
            Vector2 direction = _throw.Direction;
            transform.SetPositionAndRotation(_throw.transform.position,
                Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        }

        private void RebuildMesh(float distance)
        {
            _distance = distance;
            _meshLineWidth = _lineWidth;
            _meshHeadLength = _headLength;
            _meshHeadWidth = _headWidth;
            _meshColor = _color;
            float stem = Mathf.Max(0f, distance - _headLength);
            float halfWidth = _lineWidth * 0.5f;
            _vertices[0] = new Vector3(0f, -halfWidth, 0f);
            _vertices[1] = new Vector3(0f, halfWidth, 0f);
            _vertices[2] = new Vector3(stem, halfWidth, 0f);
            _vertices[3] = new Vector3(stem, -halfWidth, 0f);
            _vertices[4] = new Vector3(stem, -_headWidth * 0.5f, 0f);
            _vertices[5] = new Vector3(stem, _headWidth * 0.5f, 0f);
            _vertices[6] = new Vector3(distance, 0f, 0f);
            for (int i = 0; i < _colors.Length; i++) _colors[i] = _color;
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;
            _mesh.RecalculateBounds();
        }

        private void OnDisable() => _meshRenderer.enabled = false;

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
