using System.Collections.Generic;
using AbilityStudio.Runtime;
using UnityEngine;
namespace MMF.Combat
{
    /// <summary>Shows equipped workshop projectiles and short area outlines in the main scene.</summary>
    public sealed class WorkshopWeaponView : MonoBehaviour
    {
        private SkillExecutor _executor;
        private readonly List<GameObject> _shots = new();
        private readonly List<(GameObject view, float expires)> _areas = new();
        private Sprite _sprite;
        private Material _lineMaterial;
        public void Bind(SkillExecutor executor)
        {
            _executor = executor;
            _sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * .5f, Texture2D.whiteTexture.width);
            _lineMaterial = new Material(Shader.Find("Sprites/Default"));
            executor.AreaCast += Circle; executor.RectangleCast += Rectangle; executor.ConeCast += Cone;
            executor.ResetPerformed += Clear;
        }
        private void LateUpdate()
        {
            if (_executor == null) return;
            while (_shots.Count < _executor.Projectiles.Count)
            {
                var go = new GameObject("工坊投射物"); go.transform.SetParent(transform, true);
                var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = _sprite;
                renderer.color = Color.cyan; renderer.sortingOrder = 20; _shots.Add(go);
            }
            for (int i = 0; i < _shots.Count; i++)
            {
                bool active = i < _executor.Projectiles.Count; _shots[i].SetActive(active);
                if (!active) continue;
                var shot = _executor.Projectiles[i]; _shots[i].transform.position = shot.Position;
                _shots[i].transform.localScale = new Vector3(shot.Radius * 2 / transform.lossyScale.x, shot.Radius * 2 / transform.lossyScale.y, 1);
            }
            for (int i = _areas.Count - 1; i >= 0; i--)
                if (Time.time >= _areas[i].expires) { Destroy(_areas[i].view); _areas.RemoveAt(i); }
        }
        private void Outline(Vector3[] points)
        {
            var go = new GameObject("工坊范围"); go.transform.SetParent(transform, true);
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = _lineMaterial;
            line.useWorldSpace = true; line.loop = true; line.widthMultiplier = .04f;
            line.startColor = line.endColor = Color.cyan; line.sortingOrder = 20;
            line.positionCount = points.Length; line.SetPositions(points);
            _areas.Add((go, Time.time + .15f));
        }
        private void Circle(Vector3 center, float radius)
        {
            var points = new Vector3[48];
            for (int i = 0; i < points.Length; i++)
            { float angle = i * Mathf.PI * 2 / points.Length; points[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius; }
            Outline(points);
        }
        private void Rectangle(Vector3 origin, Vector3 direction, float length, float width)
        {
            var points = new Vector3[4];
            for (int i = 0; i < 4; i++) points[i] = RectangleGeometry.Corner(origin, direction, length, width, i);
            Outline(points);
        }
        private void Cone(Vector3 origin, Vector3 direction, float range, float angle)
        {
            var points = new Vector3[34]; points[0] = origin;
            for (int i = 1; i < points.Length; i++) points[i] = ConeGeometry.ArcPoint(origin, direction, range, angle, (i - 1f) / (points.Length - 2));
            Outline(points);
        }
        private void Clear()
        {
            foreach (var shot in _shots) if (shot != null) shot.SetActive(false);
            foreach (var area in _areas) if (area.view != null) Destroy(area.view);
            _areas.Clear();
        }
        private void OnDestroy()
        {
            if (_executor != null)
            { _executor.AreaCast -= Circle; _executor.RectangleCast -= Rectangle; _executor.ConeCast -= Cone; _executor.ResetPerformed -= Clear; }
            Clear(); if (_sprite != null) Destroy(_sprite); if (_lineMaterial != null) Destroy(_lineMaterial);
        }
    }
}
