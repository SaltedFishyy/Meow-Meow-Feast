using System;
using System.Collections.Generic;
using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>A solid monster that walks straight toward its target and accepts weapon hits.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class Monster : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition _definition = null;
        private MonsterTargeting _targeting;
        private MonsterAttack _attack;
        private BoxCollider2D _collider;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private ContactFilter2D _filter;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private int _health;
        private MonsterSkillTarget _skillTarget;
        internal int Health => _health;
        internal int MaxHealth => _definition.Health;
        private float _flashRemaining;
        private Vector2 _knockback;
        private Vector2 _shove;
        private bool _shoved;
        private Vector2 _avoidanceDirection;
        private float _avoidanceRemaining;
        private static readonly HashSet<Collider2D> _activeColliders = new();

        internal static HashSet<Collider2D> ActiveColliders => _activeColliders;

        private const float ContactGap = 0.002f;

        /// <summary>Raised once after hiding a monster whose health reached zero.</summary>
        public event Action<Monster> Died;

        /// <summary>Current sidestep direction, or zero while waiting or not avoiding.</summary>
        public Vector2 AvoidanceDirection => _avoidanceRemaining > 0f ? _avoidanceDirection : Vector2.zero;

        internal bool IsHitFlashing => _flashRemaining > 0f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _targeting = GetComponent<MonsterTargeting>();
            _attack = GetComponent<MonsterAttack>();
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _filter = new ContactFilter2D { useTriggers = false };
            _skillTarget = GetComponent<MonsterSkillTarget>();
            if (_skillTarget == null) _skillTarget = gameObject.AddComponent<MonsterSkillTarget>();
        }

        private void OnEnable()
        {
            _activeColliders.Add(_collider);
            _health = _definition.Health;
            _flashRemaining = 0f;
            _knockback = Vector2.zero;
            _shove = Vector2.zero;
            _shoved = false;
            _avoidanceRemaining = 0f;
            transform.localScale = Vector3.one * _definition.Size;
            _renderer.color = _definition.Color;
        }

        private void OnDisable()
        {
            _activeColliders.Remove(_collider);
        }

        private bool IgnoresContact(Collider2D other) => Physics2D.GetIgnoreCollision(_collider, other);

        private void Update()
        {
            if (_flashRemaining <= 0f) return;
            _flashRemaining -= Time.deltaTime;
            if (_flashRemaining <= 0f) _renderer.color = _definition.Color;
        }

        private void FixedUpdate()
        {
            Vector2 target = _targeting.TargetPosition(_body.position);
            bool hasExternalMotion = _shoved || _knockback.sqrMagnitude > 0f;
            if (_skillTarget != null && _skillTarget.IsStunned && !hasExternalMotion) return;
            Vector2 attackDisplacement = Vector2.zero;
            bool attackControlsMovement = _attack != null && _attack.isActiveAndEnabled &&
                _attack.Step(_body.position, hasExternalMotion, out attackDisplacement);
            bool sliding = !hasExternalMotion && !attackControlsMovement;
            Vector2 displacement;
            if (_knockback.sqrMagnitude > 0f)
            {
                displacement = _knockback;
                _knockback = Vector2.zero;
            }
            else if (_shoved)
                displacement = _shove;
            else if (attackControlsMovement)
                displacement = attackDisplacement;
            else
                displacement = Vector2.ClampMagnitude(target - _body.position,
                    _definition.Speed * Time.fixedDeltaTime);
            if (!hasExternalMotion && _skillTarget != null) displacement *= _skillTarget.MovementSpeedMultiplier;
            _shoved = false;
            _shove = Vector2.zero;
            float distance = displacement.magnitude;
            if (distance <= 0f) return;
            Vector2 direction = displacement / distance;
            int count = _body.Cast(direction, _filter, _hits, distance + ContactGap);
            for (int i = 0; i < count; i++)
                if (!IgnoresContact(_hits[i].collider) && Vector2.Dot(_hits[i].normal, direction) < 0f)
                    distance = Mathf.Min(distance, Mathf.Max(0f, _hits[i].distance - ContactGap));
            if (sliding && TryAvoidMonster(count, direction, distance)) return;
            Vector2 movement = direction * distance;
            if (sliding)
            {
                Vector2 remaining = displacement - movement;
                for (int i = 0; i < count; i++)
                    if (!IgnoresContact(_hits[i].collider) && _hits[i].distance <= distance + ContactGap && Vector2.Dot(remaining, _hits[i].normal) < 0f)
                        remaining -= Vector2.Dot(remaining, _hits[i].normal) * _hits[i].normal;
                float slideDistance = remaining.magnitude;
                if (slideDistance > 0f)
                {
                    Vector2 slideDirection = remaining / slideDistance;
                    int slideCount = Physics2D.BoxCast((Vector2)_collider.bounds.center + movement,
                        _collider.bounds.size, _body.rotation, slideDirection, _filter, _hits,
                        slideDistance + ContactGap);
                    for (int i = 0; i < slideCount; i++)
                        if (_hits[i].rigidbody != _body && !IgnoresContact(_hits[i].collider) &&
                            Vector2.Dot(_hits[i].normal, slideDirection) < 0f)
                            slideDistance = Mathf.Min(slideDistance, Mathf.Max(0f, _hits[i].distance - ContactGap));
                    if (TryAvoidMonster(slideCount, slideDirection, slideDistance)) return;
                    movement += slideDirection * slideDistance;
                }
            }
            _avoidanceRemaining = 0f;
            movement = LimitDynamicContact(movement);
            if (attackControlsMovement && !hasExternalMotion &&
                movement.sqrMagnitude < attackDisplacement.sqrMagnitude - ContactGap * ContactGap)
                _attack.StopLunge();
            _body.MovePosition(_body.position + movement);
        }

        private bool TryAvoidMonster(int count, Vector2 direction, float distance)
        {
            if (!IsBlockedByMonster(count, direction, distance, out Monster blocker)) return false;
            Vector2 otherAvoidanceDirection = blocker.AvoidanceDirection;
            float step = _definition.Speed * Time.fixedDeltaTime;
            if (_avoidanceRemaining <= 0f)
            {
                Vector2 right = MonsterAvoidance.SelectDirection(direction, true, false);
                bool rightClear = AvoidanceDistance(right, step) >= step;
                bool leftClear = !rightClear && AvoidanceDistance(-right, step) >= step;
                _avoidanceDirection = MonsterAvoidance.SelectDirection(direction, rightClear, leftClear,
                    otherAvoidanceDirection);
                _avoidanceRemaining = _definition.AvoidanceDuration;
            }
            else
            {
                Vector2 right = MonsterAvoidance.SelectDirection(direction, true, false);
                // Handle the other monster starting its sidestep later in the same physics tick.
                if (Vector2.Dot(_avoidanceDirection, right) < 0f)
                    _avoidanceDirection = MonsterAvoidance.SelectDirection(direction, false, true,
                        otherAvoidanceDirection);
            }
            float avoidanceStep = _definition.Speed * Mathf.Min(Time.fixedDeltaTime, _avoidanceRemaining);
            float avoidanceDistance = AvoidanceDistance(_avoidanceDirection, avoidanceStep);
            _avoidanceRemaining = Mathf.Max(0f, _avoidanceRemaining - Time.fixedDeltaTime);
            if (avoidanceDistance <= 0f && _avoidanceDirection.sqrMagnitude > 0f) _avoidanceRemaining = 0f;
            _body.MovePosition(_body.position +
                LimitDynamicContact(_avoidanceDirection * avoidanceDistance));
            return true;
        }

        private Vector2 LimitDynamicContact(Vector2 movement)
        {
            float distance = movement.magnitude;
            if (distance <= 0f) return Vector2.zero;
            Vector2 direction = movement / distance;
            // Keep moving kinematic bodies outside both colliders' contact skins.
            float padding = Physics2D.defaultContactOffset * 2f + ContactGap;
            int count = Physics2D.BoxCast(_collider.bounds.center,
                (Vector2)_collider.bounds.size + Vector2.one * (padding * 2f),
                _body.rotation, direction, _filter, _hits, distance + ContactGap);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (hit.rigidbody == null || hit.rigidbody.bodyType != RigidbodyType2D.Dynamic ||
                    IgnoresContact(hit.collider)) continue;
                if (hit.distance <= 0f)
                {
                    // A player can enter the padding: still allow the monster to move away.
                    ColliderDistance2D separation = _collider.Distance(hit.collider);
                    Vector2 away = separation.pointA - separation.pointB;
                    if (separation.isOverlapped) away = -away;
                    if (!separation.isValid || Vector2.Dot(direction, away) <= 0f)
                        distance = 0f;
                }
                else if (Vector2.Dot(hit.normal, direction) < 0f)
                    distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - ContactGap));
            }
            return direction * distance;
        }

        private bool IsBlockedByMonster(int count, Vector2 direction, float distance, out Monster blocker)
        {
            blocker = null;
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].rigidbody == _body || IgnoresContact(_hits[i].collider) || _hits[i].distance > distance + ContactGap ||
                    Vector2.Dot(_hits[i].normal, direction) >= 0f) continue;
                if (!_hits[i].collider.TryGetComponent<Monster>(out Monster monster)) return false;
                blocker = monster;
            }
            return blocker != null;
        }

        private float AvoidanceDistance(Vector2 direction, float distance)
        {
            if (direction.sqrMagnitude <= 0f || distance <= 0f) return 0f;
            int count = _body.Cast(direction, _filter, _hits, distance + ContactGap);
            for (int i = 0; i < count; i++)
                if (!IgnoresContact(_hits[i].collider) && Vector2.Dot(_hits[i].normal, direction) < 0f)
                    distance = Mathf.Min(distance, Mathf.Max(0f, _hits[i].distance - ContactGap));
            return distance;
        }

        /// <summary>Queues collision-limited movement for this physics tick without damage or flashing.</summary>
        public void Shove(Vector2 displacement)
        {
            if (_health <= 0 || !isActiveAndEnabled) return;
            _shove = displacement;
            _shoved = true;
        }

        /// <summary>Applies positive damage and pushes along the supplied attack direction.</summary>
        public void TakeHit(int damage, Vector2 direction)
        {
            if (_health <= 0 || damage <= 0 || !isActiveAndEnabled) return;
            _health = damage >= _health ? 0 : _health - damage;
            _flashRemaining = _definition.FlashDuration;
            _renderer.color = _flashRemaining > 0f ? Color.white : _definition.Color;
            _knockback = direction.normalized * _definition.KnockbackDistance;
            if (_health > 0) return;
            gameObject.SetActive(false);
            Died?.Invoke(this);
        }
    }
}
