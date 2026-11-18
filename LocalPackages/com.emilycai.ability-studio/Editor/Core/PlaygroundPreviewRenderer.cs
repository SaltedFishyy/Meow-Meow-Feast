using System;
using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AbilityStudio.Editor
{
    /// <summary>Disposable offscreen presentation for the isolated editor playground.</summary>
    public sealed class PlaygroundPreviewRenderer : IDisposable
    {
        private readonly PreviewRenderUtility _preview;
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<GameObject> _dummyBodies = new List<GameObject>(), _shots = new List<GameObject>();
        private readonly List<LineRenderer> _lines = new List<LineRenderer>();
        private readonly List<(Vector3[] points, double until)> _flashes = new List<(Vector3[], double)>();
        private readonly Material _blue, _orange, _cyan, _gray, _yellow, _red, _purple;
        private readonly GameObject _player;
        private bool _disposed;
        private readonly PlaygroundPreviewFeedback _feedback;
        /// <summary>Preview-only feedback presenter.</summary>
        public PlaygroundPreviewFeedback Feedback => _feedback;
        /// <summary>World advanced manually by the window.</summary>
        public PlaygroundSimulation Simulation { get; }
        /// <summary>Camera used for both rendering and pointer projection.</summary>
        public Camera Camera => _preview.camera;
        /// <summary>Creates only preview-scene objects and transient rendering resources.</summary>
        public PlaygroundPreviewRenderer()
        {
            _preview = new PreviewRenderUtility();
            try
            {
                Simulation = new PlaygroundSimulation(Camera.gameObject.scene);
                _feedback = new PlaygroundPreviewFeedback(Simulation, Camera);
                _blue = Material(new Color(.15f, .65f, 1)); _orange = Material(new Color(1, .4f, .12f));
                _cyan = Material(new Color(.12f, .9f, .8f)); _gray = Material(new Color(.2f, .27f, .32f));
                _yellow = Material(Color.yellow); _red = Material(new Color(.9f, .15f, .2f)); _purple = Material(new Color(.65f, .4f, 1));
                Body("Floor", PrimitiveType.Quad, new Vector3(0, 0, 1), new Vector3(28, 22, 1), Material(new Color(.065f, .1f, .13f)));
                for (int x = -14; x <= 14; x += 2) Body("Grid", PrimitiveType.Quad, new Vector3(x, 0, .9f), new Vector3(.025f, 22, 1), _gray);
                for (int y = -11; y <= 11; y += 2) Body("Grid", PrimitiveType.Quad, new Vector3(0, y, .9f), new Vector3(28, .025f, 1), _gray);
                _player = Body("Player", PrimitiveType.Quad, Vector3.zero, Vector3.one, _cyan);
                Camera.transform.position = new Vector3(0, 0, -20); Camera.transform.rotation = Quaternion.identity;
                Camera.orthographic = true; Camera.orthographicSize = 14; Camera.nearClipPlane = .1f; Camera.farClipPlane = 100;
                Camera.clearFlags = CameraClearFlags.SolidColor; Camera.backgroundColor = new Color(.025f, .04f, .06f); Camera.allowHDR = false;
                Simulation.Executor.AreaCast += (p, r) => Flash(Circle(p, r));
                Simulation.Executor.RectangleCast += (p, d, l, w) => Flash(Rectangle(p, d, l, w));
                Simulation.Executor.ConeCast += (p, d, r, a) => Flash(Cone(p, d, r, a));
                Simulation.Executor.Hit += (shot, target) => Flash(Circle(target.Position, target.Radius + .2f));
            }
            catch { Dispose(); throw; }
        }
        private Material Material(Color color)
        {
            var shader = Shader.Find(GraphicsSettings.currentRenderPipeline == null ? "Unlit/Color" : "Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("Playground preview currently supports Built-in or URP rendering.");
            var material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave, color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            _materials.Add(material); return material;
        }
        private GameObject Body(string name, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = name; go.hideFlags = HideFlags.HideAndDontSave;
            SceneManager.MoveGameObjectToScene(go, Simulation.Scene); go.transform.SetParent(Simulation.Root.transform);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
        private static Vector3[] Circle(Vector3 p, float radius)
        {
            var points = new Vector3[65];
            for (int i = 0; i < points.Length; i++) { float angle = i * Mathf.PI * 2 / 64; points[i] = p + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -.08f); }
            return points;
        }
        private static Vector3[] Rectangle(Vector3 p, Vector3 d, float length, float width)
        {
            var points = new Vector3[5];
            for (int i = 0; i < points.Length; i++) points[i] = RectangleGeometry.Corner(p, d, length, width, i % 4) + Vector3.back * .08f;
            return points;
        }
        private static Vector3[] Cone(Vector3 p, Vector3 d, float range, float angle)
        {
            var points = new Vector3[35]; points[0] = points[34] = p + Vector3.back * .08f;
            for (int i = 0; i <= 32; i++) points[i + 1] = ConeGeometry.ArcPoint(p, d, range, angle, i / 32f) + Vector3.back * .08f;
            return points;
        }
        private void Flash(Vector3[] points) => _flashes.Add((points, Simulation.Time + .3));
        /// <summary>Clears transient activation flashes when resetting or changing skills.</summary>
        public void ClearFlashes() { _flashes.Clear(); _feedback.Clear(); }
        private void Line(int index, Vector3[] points, Material material)
        {
            while (_lines.Count <= index)
            {
                var go = new GameObject("Skill outline") { hideFlags = HideFlags.HideAndDontSave };
                SceneManager.MoveGameObjectToScene(go, Simulation.Scene); go.transform.SetParent(Simulation.Root.transform);
                var line = go.AddComponent<LineRenderer>(); line.useWorldSpace = true; line.widthMultiplier = .065f;
                _lines.Add(line);
            }
            var item = _lines[index]; item.enabled = true; item.sharedMaterial = material;
            item.positionCount = points.Length; item.SetPositions(points);
        }
        /// <summary>Renders an offscreen texture without a Game view or Play Mode.</summary>
        public Texture Render(float width, float height)
        {
            var world = Simulation;
            _player.transform.position = world.Position;
            while (_dummyBodies.Count < world.Targets.Count) _dummyBodies.Add(Body("Dummy body", PrimitiveType.Quad, Vector3.zero, Vector3.one, _orange));
            for (int i = 0; i < world.Targets.Count; i++)
            {
                var target = world.Targets[i]; var body = _dummyBodies[i];
                body.transform.position = target.Position; body.SetActive(target.IsAlive);
                body.transform.localScale = new Vector3(target.Radius * 2, target.Radius * 2, 1);
                _feedback.RegisterBody(target, body.transform);
                body.GetComponent<Renderer>().sharedMaterial = target.IsStunned ? _yellow : target.IsBleeding ? _red : target.IsSlowed ? _purple : _orange;
            }
            while (_shots.Count < world.Executor.Projectiles.Count) _shots.Add(Body("Projectile", PrimitiveType.Quad, Vector3.zero, Vector3.one, _cyan));
            for (int i = 0; i < _shots.Count; i++)
            {
                bool active = i < world.Executor.Projectiles.Count; _shots[i].SetActive(active); if (!active) continue;
                var shot = world.Executor.Projectiles[i]; _shots[i].transform.position = shot.Position + Vector3.back * .2f;
                _shots[i].transform.localScale = Vector3.one * Mathf.Max(.15f, shot.Radius * 2);
            }
            int count = 0;
            Line(count++, new[] { world.Position + Vector3.back * .1f, world.Position + world.Rotation * Vector3.right * 1.5f + Vector3.back * .1f }, _cyan);
            if (world.Skill != null && !SkillValidator.HasErrors(world.Skill))
            {
                var direction = SkillPositions.Direction(world.Skill, world, world.Aim);
                foreach (var attack in world.Skill.AttacksFor(world.PreviewSlot))
                {
                    var p = SkillPositions.AttackPosition(world.Skill, attack, world, world.Aim);
                    if (attack is CircleAttack c) Line(count++, Circle(p, SkillChargeGeometry.Radius(world.Skill, c, world.ChargeRatio)), _blue);
                    else if (attack is LineThrustAttack r) Line(count++, Rectangle(p, direction, SkillChargeGeometry.Length(world.Skill, r, world.ChargeRatio), r.Width), _blue);
                    else if (attack is ConeAttack cone) Line(count++, Cone(p, direction, SkillChargeGeometry.Range(world.Skill, cone, world.ChargeRatio), cone.AngleDegrees), _blue);
                    else if (attack is ProjectileAttack shot) Line(count++, new[] { p + Vector3.back * .08f, p + direction * shot.Range + Vector3.back * .08f }, _blue);
                }
                var release = world.Skill.ReleaseFor(world.PreviewSlot);
                var endpoint = world.Skill.WeaponMode && release != null
                    ? SkillPositions.BranchEndpoint(world.Skill, release, world, world.Aim)
                    : SkillPositions.Endpoint(world.Skill, world, world.Aim);
                Line(count++, Circle(endpoint, .18f), _cyan);
                foreach (var rectangle in world.Skill.AttacksFor(world.PreviewSlot).OfType<LineThrustAttack>())
                {
                    float length = SkillChargeGeometry.Length(world.Skill, rectangle, world.ChargeRatio);
                    float tip = Mathf.Min(length, rectangle.TipLength);
                    var origin = SkillPositions.AttackPosition(world.Skill, rectangle, world, world.Aim);
                    if (tip > 0) Line(count++, Rectangle(SkillChargeGeometry.TipOrigin(origin, direction, length, tip), direction, tip, rectangle.Width), _yellow);
                }
            }
            _flashes.RemoveAll(f => f.until < world.Time);
            foreach (var flash in _flashes) Line(count++, flash.points, _orange);
            for (int i = count; i < _lines.Count; i++) _lines[i].enabled = false;
            Camera.aspect = width / height;
            Camera.orthographicSize = Mathf.Max(14, 16 / Camera.aspect);
            _preview.BeginPreview(new Rect(0, 0, width, height), GUIStyle.none);
            Texture texture;
            try { _preview.Render(true, false); }
            finally { texture = _preview.EndPreview(); }
            return texture;
        }
        /// <summary>Converts UI Toolkit top-left-origin viewport coordinates to a ground point.</summary>
        public bool GroundPoint(Vector2 local, Vector2 size, out Vector3 point)
        {
            point = default; if (size.x <= 0 || size.y <= 0) return false;
            var ray = Camera.ViewportPointToRay(new Vector3(local.x / size.x, 1 - local.y / size.y, 0));
            if (!new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out float distance)) return false;
            point = ray.GetPoint(distance); return true;
        }
        /// <summary>Closes the preview scene and releases textures, materials and objects.</summary>
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _feedback?.Dispose(); Simulation?.Dispose(); _preview?.Cleanup();
            foreach (var material in _materials) if (material != null) Object.DestroyImmediate(material);
            _materials.Clear();
        }
    }
}

