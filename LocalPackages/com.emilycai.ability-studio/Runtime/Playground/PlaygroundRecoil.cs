using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Playground
{
    /// <summary>Tilts a dedicated visual pivot only; the target root and colliders stay untouched.</summary>
    public sealed class PlaygroundRecoil : MonoBehaviour
    {
        private Quaternion _rest, _from, _peak;
        private float _elapsed, _leanDuration, _recoveryDuration;
        /// <summary>Whether the visual is currently leaning or recovering.</summary>
        public bool IsActive { get; private set; }
        /// <summary>Retriggers smoothly from the current pose, preserving the original rest pose.</summary>
        public void Play(RecoilFeedback feedback, Vector3 away)
        {
            if (!isActiveAndEnabled) return;
            if (!IsActive) _rest = transform.localRotation;
            away.z = 0;
            if (away.sqrMagnitude < .000001f) away = Vector3.right;
            var localAway = transform.parent != null ? transform.parent.InverseTransformDirection(away.normalized) : away.normalized;
            // A 2D visual leans within XY; never rotate its collision root or tilt out of the screen.
            var axis = Vector3.forward * (localAway.x >= 0 ? -1 : 1);
            _from = transform.localRotation;
            _peak = Quaternion.AngleAxis(feedback.Angle, axis) * _rest;
            _leanDuration = feedback.LeanDuration; _recoveryDuration = feedback.RecoveryDuration;
            _elapsed = 0; IsActive = true;
        }
        /// <summary>Advances the visual animation using an explicit delta time.</summary>
        public void Tick(float deltaTime)
        {
            if (!IsActive || !float.IsFinite(deltaTime) || deltaTime < 0) return;
            _elapsed += deltaTime;
            if (_elapsed >= _leanDuration + _recoveryDuration) { ResetPose(); return; }
            if (_elapsed < _leanDuration)
                transform.localRotation = Quaternion.Slerp(_from, _peak, Mathf.SmoothStep(0, 1, _elapsed / _leanDuration));
            else
                transform.localRotation = Quaternion.Slerp(_peak, _rest, Mathf.SmoothStep(0, 1, (_elapsed - _leanDuration) / _recoveryDuration));
        }
        /// <summary>Immediately restores the original visual pose.</summary>
        public void ResetPose() { if (IsActive) transform.localRotation = _rest; IsActive = false; }
        private void OnDisable() => ResetPose();
    }
}
