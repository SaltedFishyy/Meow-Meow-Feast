using System;
using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AbilityStudio.Editor
{
    /// <summary>Manually advances visual feedback entirely inside the preview scene.</summary>
    public sealed class PlaygroundPreviewFeedback : IDisposable
    {
        private readonly PlaygroundSimulation _world;
        private readonly Camera _camera;
        private readonly GameObject _staging;
        private readonly Dictionary<ISkillTarget, Transform> _bodies = new Dictionary<ISkillTarget, Transform>();
        private readonly Dictionary<Transform, (Quaternion rest, Quaternion from, Quaternion peak, float elapsed, float lean, float recover)> _recoils
            = new Dictionary<Transform, (Quaternion, Quaternion, Quaternion, float, float, float)>();
        private readonly List<(GameObject instance, ParticleSystem[] particles, float remaining)> _visuals
            = new List<(GameObject, ParticleSystem[], float)>();
        private readonly List<(float strength, float duration, float frequency, float elapsed)> _shakes
            = new List<(float, float, float, float)>();
        private Vector3 _offset;
        private bool _disposed;
        /// <summary>Feedback status, including audio events which are intentionally not played into the main game.</summary>
        public string LastMessage { get; private set; } = "";
        /// <summary>Number of isolated transient visual instances.</summary>
        public int ActiveVisuals => _visuals.Count;
        /// <summary>Creates an inactive staging root so prefab scripts never run during instantiation.</summary>
        public PlaygroundPreviewFeedback(PlaygroundSimulation world, Camera camera)
        {
            _world = world; _camera = camera;
            _staging = new GameObject("Inactive VFX staging") { hideFlags = HideFlags.HideAndDontSave };
            _staging.SetActive(false);
            SceneManager.MoveGameObjectToScene(_staging, world.Scene);
            _staging.transform.SetParent(world.Root.transform, false);
            world.Executor.FeedbackRequested += Present;
            world.Executor.ResetPerformed += Clear;
        }
        /// <summary>Associates a visual body, never a main-game target.</summary>
        public void RegisterBody(ISkillTarget target, Transform body) => _bodies[target] = body;
        private void Present(HitFeedbackRequest request)
        {
            if (request.Effect is CameraShakeFeedback shake)
            {
                if (_shakes.Count == 32) _shakes.RemoveAt(0);
                _shakes.Add((shake.Strength, shake.Duration, shake.Frequency, 0));
            }
            else if (request.Effect is RecoilFeedback recoil && _bodies.TryGetValue(request.Target, out var body) && body != null)
            {
                var rest = _recoils.TryGetValue(body, out var previous) ? previous.rest : body.localRotation;
                var away = request.Target.Position - request.Caster.Position;
                float sign = away.x >= 0 ? -1 : 1;
                _recoils[body] = (rest, body.localRotation, Quaternion.Euler(0, 0, recoil.Angle * sign) * rest, 0,
                    recoil.LeanDuration, recoil.RecoveryDuration);
            }
            else if (request.Effect is HitVfxFeedback vfx && vfx.Prefab != null)
            {
                if (_visuals.Count >= 64) return;
                var instance = Object.Instantiate(vfx.Prefab, _staging.transform, false);
                instance.SetActive(false);
                // Keep only visual components. No user gameplay scripts, audio, cameras or physics run in the preview.
                // Remove scripts first, including ones that require colliders or rigidbodies.
                foreach (var component in instance.GetComponentsInChildren<MonoBehaviour>(true))
                    if (component != null) Object.DestroyImmediate(component);
                foreach (var component in instance.GetComponentsInChildren<Component>(true))
                    if (component is Behaviour || component is Joint || component is Joint2D)
                        Object.DestroyImmediate(component);
                foreach (var component in instance.GetComponentsInChildren<Component>(true))
                    if (component is Collider || component is Collider2D || component is Rigidbody || component is Rigidbody2D)
                        Object.DestroyImmediate(component);
                foreach (var child in instance.GetComponentsInChildren<Transform>(true))
                    child.gameObject.hideFlags = HideFlags.HideAndDontSave;
                instance.transform.SetParent(_world.Root.transform, false);
                instance.transform.position = request.Position + vfx.Offset;
                instance.transform.localScale *= vfx.Scale;
                var particles = instance.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var particle in particles)
                {
                    var main = particle.main; main.playOnAwake = false;
                    particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                instance.SetActive(true);
                _visuals.Add((instance, particles, vfx.Lifetime));
            }
            if (request.Effect is HitAudioFeedback audio)
                LastMessage = audio.Clip == null ? "命中音效：未指定资源" : "音效触发：" + audio.Clip.name + "（仅提示，不混入游戏音频）";
            else
                LastMessage = "命中反馈：" + (ModuleCatalog.Metadata(request.Effect.GetType())?.DisplayName ?? request.Effect.GetType().Name);
        }
        /// <summary>Uses the preview delta, never Time.timeScale or a game camera.</summary>
        public void Tick(float delta)
        {
            if (_disposed || delta <= 0 || !float.IsFinite(delta)) return;
            RestoreCamera();
            for (int i = _shakes.Count - 1; i >= 0; i--)
            {
                var shake = _shakes[i]; shake.elapsed += delta;
                if (shake.elapsed >= shake.duration) { _shakes.RemoveAt(i); continue; }
                _shakes[i] = shake;
                float phase = shake.elapsed * shake.frequency * Mathf.PI * 2;
                float strength = shake.strength * (1 - shake.elapsed / shake.duration);
                _offset += new Vector3(Mathf.Sin(phase), Mathf.Sin(phase * 1.37f), 0) * strength;
            }
            if (_camera != null) _camera.transform.position += _offset;
            foreach (var body in new List<Transform>(_recoils.Keys))
            {
                var recoil = _recoils[body]; recoil.elapsed += delta;
                if (body == null) { _recoils.Remove(body); continue; }
                if (recoil.elapsed >= recoil.lean + recoil.recover)
                { body.localRotation = recoil.rest; _recoils.Remove(body); continue; }
                body.localRotation = recoil.elapsed < recoil.lean
                    ? Quaternion.Slerp(recoil.from, recoil.peak, Mathf.SmoothStep(0, 1, recoil.elapsed / recoil.lean))
                    : Quaternion.Slerp(recoil.peak, recoil.rest, Mathf.SmoothStep(0, 1, (recoil.elapsed - recoil.lean) / recoil.recover));
                _recoils[body] = recoil;
            }
            for (int i = _visuals.Count - 1; i >= 0; i--)
            {
                var visual = _visuals[i]; visual.remaining -= delta;
                if (visual.remaining <= 0 || visual.instance == null)
                { if (visual.instance != null) Object.DestroyImmediate(visual.instance); _visuals.RemoveAt(i); continue; }
                foreach (var particle in visual.particles) if (particle != null) particle.Simulate(delta, false, false);
                _visuals[i] = visual;
            }
        }
        private void RestoreCamera()
        { if (_camera != null) _camera.transform.position -= _offset; _offset = Vector3.zero; }
        /// <summary>Restores all poses and removes only this preview's temporary objects.</summary>
        public void Clear()
        {
            RestoreCamera(); _shakes.Clear();
            foreach (var pair in _recoils) if (pair.Key != null) pair.Key.localRotation = pair.Value.rest;
            _recoils.Clear();
            foreach (var visual in _visuals) if (visual.instance != null) Object.DestroyImmediate(visual.instance);
            _visuals.Clear(); LastMessage = "";
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _world.Executor.FeedbackRequested -= Present; _world.Executor.ResetPerformed -= Clear;
            Clear(); _bodies.Clear(); if (_staging != null) Object.DestroyImmediate(_staging);
        }
    }
}
