using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEngine;
namespace AbilityStudio.Playground
{
    /// <summary>Optional feedback presentation for the playground; gameplay may subscribe with its own adapters.</summary>
    public sealed class PlaygroundHitFeedback : MonoBehaviour
    {
        private SkillExecutor _executor;
        private Camera _camera;
        private Vector3 _cameraOffset;
        private readonly Dictionary<ISkillTarget, PlaygroundRecoil> _recoilViews = new Dictionary<ISkillTarget, PlaygroundRecoil>();
        /// <summary>Registers an explicitly separate model pivot for a preview target.</summary>
        public void RegisterRecoil(ISkillTarget target, PlaygroundRecoil view) => _recoilViews[target] = view;
        private readonly List<(GameObject instance, float remaining)> _instances = new List<(GameObject, float)>();
        private readonly List<(float strength, float duration, float frequency, float elapsed)> _shakes = new List<(float, float, float, float)>();
        /// <summary>Current transient visuals/audio objects.</summary>
        public int ActiveInstances => _instances.Count;
        /// <summary>Current camera offset, used by deterministic presentation checks.</summary>
        public Vector3 CameraOffset => _cameraOffset;
        /// <summary>Attaches this optional presenter to a runtime and camera.</summary>
        public void Bind(SkillExecutor executor, Camera camera)
        {
            Unbind(); _executor = executor; _camera = camera;
            if (isActiveAndEnabled) Subscribe();
        }
        private void Subscribe()
        {
            if (_executor == null) return;
            _executor.FeedbackRequested += Present;
            _executor.ResetPerformed += Clear;
        }
        private void Unsubscribe()
        {
            if (_executor == null) return;
            _executor.FeedbackRequested -= Present;
            _executor.ResetPerformed -= Clear;
        }
        private void Unbind() { Unsubscribe(); Clear(); _executor = null; _camera = null; }
        private void OnEnable() => Subscribe();
        private void OnDisable() { Unsubscribe(); Clear(); }
        private void OnDestroy() => Unbind();
        private void Present(HitFeedbackRequest request)
        {
            if (request.Effect is RecoilFeedback recoil)
            {
                if (_recoilViews.TryGetValue(request.Target, out var view) && view != null)
                {
                    var away = request.Target.Position - request.Caster.Position;
                    if (away.sqrMagnitude < .000001f) away = request.Caster.Rotation * Vector3.right;
                    view.Play(recoil, away);
                }
                return;
            }
            if (request.Effect is CameraShakeFeedback shake)
            {
                if (_camera != null && shake.Strength > 0)
                {
                    // Bounded overlap: retain at most 32 active shake requests.
                    if (_shakes.Count == 32) _shakes.RemoveAt(0);
                    _shakes.Add((shake.Strength, shake.Duration, shake.Frequency, 0));
                }
                return;
            }
            if (request.Effect is HitVfxFeedback vfx && vfx.Prefab != null)
            {
                var instance = Instantiate(vfx.Prefab, request.Position + vfx.Offset, Quaternion.identity);
                instance.name = "命中特效（预览）";
                instance.transform.localScale *= vfx.Scale;
                instance.SetActive(true);
                Track(instance, vfx.Lifetime);
            }
            else if (request.Effect is HitAudioFeedback audio && audio.Clip != null)
            {
                var instance = new GameObject("命中音效（预览）");
                instance.transform.position = request.Position;
                var source = instance.AddComponent<AudioSource>();
                source.playOnAwake = false; source.clip = audio.Clip;
                source.volume = audio.Volume; source.pitch = audio.Pitch;
                source.spatialBlend = 0; // Arena preview remains audible from its elevated camera.
                source.Play();
                Track(instance, audio.Clip.length / audio.Pitch);
            }
        }
        private void Track(GameObject instance, float lifetime)
        {
            if (_instances.Count == 128) { Destroy(_instances[0].instance); _instances.RemoveAt(0); }
            _instances.Add((instance, lifetime));
        }
        private void LateUpdate() => Tick(Time.deltaTime);
        /// <summary>Advances transient lifetimes and shake with explicit delta time.</summary>
        public void Tick(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) return;
            RestoreCamera();
            foreach (var view in _recoilViews.Values) if (view != null) view.Tick(deltaTime);
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                var entry = _instances[i]; entry.remaining -= deltaTime;
                if (entry.instance == null || entry.remaining <= 0) { if (entry.instance != null) Destroy(entry.instance); _instances.RemoveAt(i); }
                else _instances[i] = entry;
            }
            Vector2 offset = Vector2.zero;
            for (int i = _shakes.Count - 1; i >= 0; i--)
            {
                var shake = _shakes[i]; shake.elapsed += deltaTime;
                if (shake.elapsed >= shake.duration) { _shakes.RemoveAt(i); continue; }
                _shakes[i] = shake;
                float phase = shake.elapsed * shake.frequency * Mathf.PI * 2;
                float envelope = shake.strength * (1 - shake.elapsed / shake.duration);
                offset += new Vector2(Mathf.Sin(phase), Mathf.Sin(phase * 1.37f)) * envelope;
            }
            if (_camera != null)
            {
                _cameraOffset = _camera.transform.right * offset.x + _camera.transform.up * offset.y;
                _camera.transform.position += _cameraOffset;
            }
        }
        private void RestoreCamera()
        {
            if (_camera != null) _camera.transform.position -= _cameraOffset;
            _cameraOffset = Vector3.zero;
        }
        /// <summary>Stops audio, removes transient visuals and restores the camera immediately.</summary>
        public void Clear()
        {
            RestoreCamera(); _shakes.Clear();
            foreach (var view in _recoilViews.Values) if (view != null) view.ResetPose();
            foreach (var entry in _instances)
                if (entry.instance != null) { entry.instance.SetActive(false); Destroy(entry.instance); }
            _instances.Clear();
        }
    }
}
