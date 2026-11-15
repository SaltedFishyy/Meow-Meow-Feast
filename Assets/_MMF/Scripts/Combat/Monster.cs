using System;
using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>A solid monster that walks straight toward its target and accepts weapon hits.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class Monster : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition _definition = null;
        [SerializeField] private Transform _target = null;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private ContactFilter2D _filter;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private int _health;
        private float _flashRemaining;
        private Vector2 _knockback;
        private const float ContactGap = 0.002f;

        /// <summary>Raised once after hiding a monster whose health reached zero.</summary>
        public event Action<Monster> Died;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _filter = new ContactFilter2D { useTriggers = false };
        }

        private void OnEnable()
        {
            _health = _definition.Health;
            _flashRemaining = 0f;
            _knockback = Vector2.zero;
            transform.localScale = Vector3.one * _definition.Size;
            _renderer.color = _definition.Color;
        }

        private void Update()
        {
            if (_flashRemaining <= 0f) return;
            _flashRemaining -= Time.deltaTime;
            if (_flashRemaining <= 0f) _renderer.color = _definition.Color;
        }

        private void FixedUpdate()
        {
            Vector2 displacement;
            if (_knockback.sqrMagnitude > 0f)
            {
                displacement = _knockback;
                _knockback = Vector2.zero;
            }
            else
                displacement = Vector2.ClampMagnitude((Vector2)_target.position - _body.position,
                    _definition.Speed * Time.fixedDeltaTime);
            float distance = displacement.magnitude;
            if (distance <= 0f) return;
            Vector2 direction = displacement / distance;
            int count = _body.Cast(direction, _filter, _hits, distance + ContactGap);
            for (int i = 0; i < count; i++)
                if (Vector2.Dot(_hits[i].normal, direction) < 0f)
                    distance = Mathf.Min(distance, Mathf.Max(0f, _hits[i].distance - ContactGap));
            _body.MovePosition(_body.position + direction * distance);
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
