using MMF.Combat;
using MMF.Data;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Applies knockback, flashes the player and manages monster collision pairs.</summary>
    [DefaultExecutionOrder(-110)]
    [DisallowMultipleComponent]
    public sealed class PlayerHitReaction : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _health;
        [SerializeField] private PlayerHealthTuning _tuning;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField, Min(0f)] private float _knockbackDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float _flashInterval = 0.08f;
        [SerializeField, Range(0f, 1f)] private float _dimAlpha = 0.3f;

        private const float ContactGap = 0.002f;
        private const float StepTimeTolerance = 0.000001f;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private ContactFilter2D _filter;
        private Vector2 _knockbackDirection;
        private float _knockbackSpeed;
        private float _knockbackRemaining;
        private bool _ownsMovement;
        private float _originalAlpha;
        private double _flashStarted;
        private bool _flashing;

        /// <summary>Whether player and monster collision pairs are suppressed.</summary>
        public bool PassesThroughMonsters { get; private set; }

        private void Awake()
        {
            _filter = new ContactFilter2D { useTriggers = false };
        }

        private void OnEnable()
        {
            _health.Hit += OnHit;
            _health.InvincibilityStarted += OnInvincibilityStarted;
        }

        private void OnDisable()
        {
            _health.Hit -= OnHit;
            _health.InvincibilityStarted -= OnInvincibilityStarted;
            EndKnockback();
            EndFlash();
            if (PassesThroughMonsters) SetMonsterCollisions(false);
        }

        private void OnHit(Vector2 direction)
        {
            _knockbackDirection = direction.normalized;
            float distance = _tuning.KnockbackDistance;
            if (distance <= 0f || _knockbackDirection == Vector2.zero)
            {
                EndKnockback();
                return;
            }
            _knockbackRemaining = Mathf.Max(_knockbackDuration, Time.fixedDeltaTime);
            _knockbackSpeed = distance / _knockbackRemaining;
            _ownsMovement = true;
            _movement.SetHitReactionControl(true);
            _body.linearVelocity = Vector2.zero;
        }

        private void OnInvincibilityStarted()
        {
            if (!_flashing) _originalAlpha = _renderer.color.a;
            _flashing = true;
            _flashStarted = Time.timeAsDouble;
            SetMonsterCollisions(true);
        }

        private void Update()
        {
            if (!_flashing) return;
            if (!_health.IsInvincible)
            {
                EndFlash();
                return;
            }

            int phase = (int)((Time.timeAsDouble - _flashStarted) / _flashInterval);
            SetAlpha((phase & 1) == 0 ? _originalAlpha * _dimAlpha : _originalAlpha);
        }

        private void FixedUpdate()
        {
            if (PassesThroughMonsters)
                SetMonsterCollisions(_health.IsInvincible || OverlapsMonster());

            if (!_ownsMovement) return;
            if (_knockbackRemaining <= StepTimeTolerance)
            {
                EndKnockback();
                return;
            }

            float stepTime = Mathf.Min(Time.fixedDeltaTime, _knockbackRemaining);
            float distance = _knockbackSpeed * stepTime;
            _knockbackRemaining = Mathf.Max(0f, _knockbackRemaining - Time.fixedDeltaTime);
            int count = _body.Cast(_knockbackDirection, _filter, _hits, distance + ContactGap);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (Physics2D.GetIgnoreCollision(_collider, hit.collider) ||
                    Vector2.Dot(hit.normal, _knockbackDirection) >= 0f) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - ContactGap));
            }
            _body.linearVelocity = _knockbackDirection * (distance / Time.fixedDeltaTime);
        }

        private bool OverlapsMonster()
        {
            foreach (Collider2D monster in Monster.ActiveColliders)
            {
                ColliderDistance2D separation = _collider.Distance(monster);
                if (separation.isValid && separation.isOverlapped) return true;
            }
            return false;
        }

        private void SetMonsterCollisions(bool ignore)
        {
            PassesThroughMonsters = ignore;
            foreach (Collider2D monster in Monster.ActiveColliders)
                if (Physics2D.GetIgnoreCollision(_collider, monster) != ignore)
                    Physics2D.IgnoreCollision(_collider, monster, ignore);
        }

        private void EndKnockback()
        {
            if (!_ownsMovement) return;
            _ownsMovement = false;
            _movement.SetHitReactionControl(false);
            _body.linearVelocity = Vector2.zero;
        }

        private void EndFlash()
        {
            if (!_flashing) return;
            _flashing = false;
            SetAlpha(_originalAlpha);
        }

        private void SetAlpha(float alpha)
        {
            Color color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;
        }
    }
}
