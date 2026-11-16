using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Displays a reusable sector mesh briefly after an accepted weapon swing.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class SwingFlash : MonoBehaviour
    {
        private const int ArcSegments = 64;
        [SerializeField] private PlayerAttack _attack = null;
        [SerializeField] private MeshFilter _meshFilter = null;
        [SerializeField] private MeshRenderer _meshRenderer = null;
        [SerializeField] private Color _color = new Color(1f, 1f, 1f, 0.5f);
        private Mesh _mesh;
        private readonly Vector3[] _vertices = new Vector3[ArcSegments + 2];
        private readonly Color[] _colors = new Color[ArcSegments + 2];
        private readonly int[] _triangles = new int[ArcSegments * 3];
        private float _range = -1f;
        private float _angle = -1f;
        private Color _meshColor;
        private float _hideTime;

        private void Awake()
        {
            _mesh = new Mesh { name = "Swing Flash Sector" };
            _meshFilter.sharedMesh = _mesh;
            _meshRenderer.enabled = false;
            for (int i = 0; i < ArcSegments; i++)
            {
                int triangle = i * 3;
                _triangles[triangle] = 0;
                _triangles[triangle + 1] = i + 2;
                _triangles[triangle + 2] = i + 1;
            }
        }

        private void OnEnable()
        {
            _attack.Swung += Show;
        }

        private void OnDisable()
        {
            _attack.Swung -= Show;
            _meshRenderer.enabled = false;
        }

        private void Show(WeaponDefinition weapon, Vector2 direction)
        {
            if (direction.sqrMagnitude == 0f)
            {
                _meshRenderer.enabled = false;
                return;
            }
            if (_range != weapon.Range || _angle != weapon.Angle || _meshColor != _color)
                RebuildMesh(weapon.Range, weapon.Angle);
            transform.SetPositionAndRotation(_attack.transform.position,
                Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            _hideTime = Time.time + weapon.SwingFlashDuration;
            _meshRenderer.enabled = weapon.SwingFlashDuration > 0f;
        }

        private void Update()
        {
            if (_meshRenderer.enabled && Time.time >= _hideTime)
                _meshRenderer.enabled = false;
        }

        private void RebuildMesh(float range, float angle)
        {
            _range = range;
            _angle = angle;
            _meshColor = _color;
            for (int i = 0; i <= ArcSegments; i++)
            {
                float radians = (-angle * 0.5f + angle * i / ArcSegments) * Mathf.Deg2Rad;
                _vertices[i + 1] = new Vector3(Mathf.Cos(radians) * range, Mathf.Sin(radians) * range, 0f);
            }
            for (int i = 0; i < _colors.Length; i++) _colors[i] = _color;
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;
            _mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
