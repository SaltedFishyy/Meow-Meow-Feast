using System;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Default health and smooth displacement adapter, independent of any skill.</summary>
    public sealed class CombatTarget : MonoBehaviour, ISkillTarget, IDamageable, IHealable, ITrackedDisplaceable, IStatusReceiver, IBleedReceiver, ISlowReceiver
    {
        [SerializeField] private int _team = 1;
        [SerializeField] private float _maxHp = 10;
        [SerializeField] private float _armor;
        [SerializeField] private float _radius = 0.6f;
        private Vector3 _spawn, _moveStart, _moveEnd;
        private float _moveTime, _moveDuration;
        private Action _displacementCompleted;
        private float _slowReduction;
        /// <inheritdoc/>
        public float SlowRemaining { get; private set; }
        /// <inheritdoc/>
        public bool IsSlowed => SlowRemaining > 0;
        /// <inheritdoc/>
        public float MovementSpeedMultiplier => IsSlowed ? 1 - _slowReduction / 100f : 1;
        /// <inheritdoc/>
        public void ApplySlow(float reductionPercent, float duration)
        {
            if (!float.IsFinite(reductionPercent) || reductionPercent <= 0 || reductionPercent > 100 || !float.IsFinite(duration) || duration <= 0) return;
            _slowReduction = reductionPercent; SlowRemaining = duration;
        }
        /// <summary>Applies voluntary movement with slow and stun; forced displacement takes priority.</summary>
        public void MoveVoluntarily(Vector3 worldDelta)
        {
            if (!IsAlive || IsStunned || IsMoving || !float.IsFinite(worldDelta.x) || !float.IsFinite(worldDelta.y) || !float.IsFinite(worldDelta.z)) return;
            worldDelta.z = 0;
            transform.position += worldDelta * MovementSpeedMultiplier;
        }
        private float _bleedRate, _bleedProgress, _bleedPending;
        private ISkillCaster _bleedSource;
        /// <inheritdoc/>
        public float BleedRemaining { get; private set; }
        /// <inheritdoc/>
        public bool IsBleeding => BleedRemaining > 0;
        /// <inheritdoc/>
        public void ApplyBleed(float damagePerSecond, float duration, ISkillCaster source)
        {
            if (!IsAlive || !float.IsFinite(damagePerSecond) || damagePerSecond <= 0 || !float.IsFinite(duration) || duration <= 0) return;
            if (!IsBleeding) { _bleedProgress = 0; _bleedPending = 0; }
            _bleedRate = damagePerSecond; BleedRemaining = duration; _bleedSource = source;
        }
        private void ClearBleed()
        { BleedRemaining = 0; _bleedProgress = 0; _bleedPending = 0; _bleedRate = 0; _bleedSource = null; }
        private void TickBleed(float deltaTime)
        {
            if (!IsAlive) { ClearBleed(); return; }
            float active = Mathf.Min(deltaTime, BleedRemaining);
            while (active > 0 && IsBleeding && IsAlive)
            {
                float step = Mathf.Min(active, 1 - _bleedProgress);
                _bleedProgress += step; _bleedPending += step * _bleedRate;
                BleedRemaining = Mathf.Max(0, BleedRemaining - step); active = Mathf.Max(0, active - step);
                if (_bleedProgress >= 0.999999f || BleedRemaining <= 0.000001f)
                {
                    float damage = _bleedPending * (100f / (100f + Mathf.Max(0, Armor)));
                    _bleedProgress = 0; _bleedPending = 0;
                    ApplyDamage(damage, _bleedSource);
                }
                if (BleedRemaining <= 0.000001f || !IsAlive) ClearBleed();
            }
        }
        /// <inheritdoc/>
        public float StunRemaining { get; private set; }
        /// <inheritdoc/>
        public bool IsStunned => StunRemaining > 0;
        /// <inheritdoc/>
        public void ApplyStun(float duration)
        {
            if (float.IsFinite(duration) && duration > 0) StunRemaining = duration;
        }
        /// <inheritdoc/>
        public Vector3 Position => transform.position;
        /// <inheritdoc/>
        public float Radius => _radius;
        /// <inheritdoc/>
        public int Team => _team;
        /// <inheritdoc/>
        public bool IsAlive => this != null && gameObject.activeInHierarchy && CurrentHp > 0;
        /// <inheritdoc/>
        public float CurrentHp { get; private set; }
        /// <inheritdoc/>
        public float MaxHp => _maxHp;
        /// <inheritdoc/>
        public float Armor => _armor;
        /// <summary>Whether a displacement is active.</summary>
        public bool IsMoving => _moveTime < _moveDuration;
        /// <summary>Final point of the active forced displacement, for presentation.</summary>
        public Vector3 DisplacementDestination => _moveEnd;
        /// <summary>Actual damage for feedback.</summary>
        public event Action<float> Damaged;
        private void Awake() { _spawn = Position; CurrentHp = _maxHp; }
        /// <inheritdoc/>
        public void ApplyDamage(float amount, ISkillCaster source)
        { float actual = Mathf.Min(CurrentHp, Mathf.Max(0, amount)); CurrentHp -= actual; Damaged?.Invoke(actual); }
        /// <summary>Sets the friendly-fire group for runtime-created actors.</summary>
        public void SetTeam(int team) => _team = team;
        /// <inheritdoc/>
        public void ApplyHealing(float amount, ISkillCaster source)
        {
            if (!IsAlive || !float.IsFinite(amount) || amount <= 0) return;
            CurrentHp += Mathf.Min(amount, Mathf.Max(0, MaxHp - CurrentHp));
        }
        /// <inheritdoc/>
        public void Displace(Vector3 worldDelta, float duration)
        { _displacementCompleted = null; _moveStart = Position; _moveEnd = Position + new Vector3(worldDelta.x, worldDelta.y, 0); _moveTime = 0; _moveDuration = Mathf.Max(0.001f, duration); }
        /// <inheritdoc/>
        public void DisplaceTracked(Vector3 worldDelta, float duration, Action completed)
        { Displace(worldDelta, duration); _displacementCompleted = completed; }
        /// <summary>Advances displacement explicitly, allowing deterministic tests.</summary>
        public void Tick(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
            StunRemaining = Mathf.Max(0, StunRemaining - deltaTime);
            SlowRemaining = Mathf.Max(0, SlowRemaining - deltaTime);
            TickBleed(deltaTime);
            if (!IsMoving) return;
            _moveTime = Mathf.Min(_moveDuration, _moveTime + deltaTime);
            transform.position = Vector3.Lerp(_moveStart, _moveEnd, Mathf.SmoothStep(0, 1, _moveTime / _moveDuration));
            if (!IsMoving) { var completed = _displacementCompleted; _displacementCompleted = null; if (IsAlive) completed?.Invoke(); }
        }
        /// <summary>Restores health and original placement.</summary>
        public void ResetTarget() { _displacementCompleted = null; CurrentHp = _maxHp; ClearBleed(); StunRemaining = 0; SlowRemaining = 0; _slowReduction = 0; _moveDuration = 0; _moveTime = 0; transform.position = _spawn; }
    }
}


















