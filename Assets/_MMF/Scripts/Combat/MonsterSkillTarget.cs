using System;
using System.Collections.Generic;
using AbilityStudio.Runtime;
using UnityEngine;
namespace MMF.Combat
{
    /// <summary>Adapts workshop effects to the monster's existing health and collision-limited movement.</summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class MonsterSkillTarget : MonoBehaviour, ISkillTarget, IDamageable, ITrackedDisplaceable, IStatusReceiver, IBleedReceiver, ISlowReceiver
    {
        internal static readonly HashSet<MonsterSkillTarget> Active = new();
        private Monster _monster;
        private Collider2D _collider;
        private float _pendingDamage, _slowPercent, _bleedRate, _bleedProgress, _bleedPending;
        private ISkillCaster _bleedSource;
        private Vector3 _moveStart, _moveEnd;
        private float _moveElapsed, _moveDuration;
        private Action _completed;
        public Vector3 Position => transform.position;
        public float Radius => Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.y);
        public int Team => 1;
        public bool IsAlive => this != null && isActiveAndEnabled && _monster != null && _monster.Health > 0;
        public float CurrentHp => _monster.Health;
        public float MaxHp => _monster.MaxHealth;
        public float Armor => 0;
        public float StunRemaining { get; private set; }
        public bool IsStunned => StunRemaining > 0;
        public float SlowRemaining { get; private set; }
        public bool IsSlowed => SlowRemaining > 0;
        public float MovementSpeedMultiplier => IsSlowed ? 1 - _slowPercent / 100 : 1;
        public float BleedRemaining { get; private set; }
        public bool IsBleeding => BleedRemaining > 0;
        private void Awake() { _monster = GetComponent<Monster>(); _collider = GetComponent<Collider2D>(); }
        private void OnEnable() { _pendingDamage = 0; Active.Add(this); }
        private void OnDisable()
        {
            Active.Remove(this); StunRemaining = SlowRemaining = BleedRemaining = 0;
            _bleedProgress = _bleedPending = _moveDuration = 0; _completed = null; _bleedSource = null;
        }
        public void ApplyDamage(float amount, ISkillCaster source)
        {
            if (!IsAlive || !float.IsFinite(amount) || amount <= 0) return;
            _pendingDamage = Mathf.Min(CurrentHp, _pendingDamage + amount);
            int points = Mathf.FloorToInt(_pendingDamage); _pendingDamage -= points;
            _monster.TakeHit(points, source == null ? Vector2.zero : (Vector2)(Position - source.Position));
        }
        public void ApplyStun(float duration) { if (float.IsFinite(duration) && duration > 0) StunRemaining = duration; }
        public void ApplySlow(float reductionPercent, float duration)
        {
            if (!float.IsFinite(reductionPercent) || reductionPercent <= 0 || reductionPercent > 100 || !float.IsFinite(duration) || duration <= 0) return;
            _slowPercent = reductionPercent; SlowRemaining = duration;
        }
        public void ApplyBleed(float damagePerSecond, float duration, ISkillCaster source)
        {
            if (!IsAlive || !float.IsFinite(damagePerSecond) || damagePerSecond <= 0 || !float.IsFinite(duration) || duration <= 0) return;
            if (!IsBleeding) _bleedProgress = _bleedPending = 0;
            _bleedRate = damagePerSecond; BleedRemaining = duration; _bleedSource = source;
        }
        public void Displace(Vector3 delta, float duration) => DisplaceTracked(delta, duration, null);
        public void DisplaceTracked(Vector3 delta, float duration, Action completed)
        {
            _moveStart = Position; _moveEnd = Position + new Vector3(delta.x, delta.y, 0);
            _moveElapsed = 0; _moveDuration = Mathf.Max(.001f, duration); _completed = completed;
        }
        private void FixedUpdate()
        {
            if (!IsAlive) return;
            if (_moveDuration <= 0 && _completed != null) { var completed = _completed; _completed = null; completed(); }
            float dt = Time.fixedDeltaTime;
            StunRemaining = Mathf.Max(0, StunRemaining - dt); SlowRemaining = Mathf.Max(0, SlowRemaining - dt);
            if (IsBleeding)
            {
                float active = Mathf.Min(dt, BleedRemaining); _bleedProgress += active; _bleedPending += active * _bleedRate;
                BleedRemaining = Mathf.Max(0, BleedRemaining - active);
                if (_bleedProgress >= 1 || !IsBleeding)
                { float damage = _bleedPending; _bleedProgress = _bleedPending = 0; ApplyDamage(damage, _bleedSource); }
            }
            if (!IsAlive || _moveDuration <= 0) return;
            _moveElapsed = Mathf.Min(_moveDuration, _moveElapsed + dt);
            Vector3 desired = Vector3.Lerp(_moveStart, _moveEnd, Mathf.SmoothStep(0, 1, _moveElapsed / _moveDuration));
            _monster.Shove(desired - Position);
            if (_moveElapsed >= _moveDuration) _moveDuration = 0;
        }
    }
}
