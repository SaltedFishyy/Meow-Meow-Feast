using System;
using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Stops a fed customer and fades its sprite before removal.</summary>
    public sealed class Customer : MonoBehaviour
    {
        [SerializeField] private CustomerTuning _tuning = null;
        [SerializeField] private MonsterDefinition _definition = null;
        private Monster _monster;
        private MonsterTargeting _targeting;
        private MonsterAttack _attack;
        private MonsterTruckRoutine _truckRoutine;
        private BoxCollider2D _collider;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private bool _fed;
        private float _elapsed;
        private Color _startColor;

        /// <summary>Raised when the fed customer is ready to be removed.</summary>
        public event Action<Monster> Purified;

        internal Monster Monster => _monster;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
            _targeting = GetComponent<MonsterTargeting>();
            _attack = GetComponent<MonsterAttack>();
            _truckRoutine = GetComponent<MonsterTruckRoutine>();
            _collider = GetComponent<BoxCollider2D>();
            _body = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Accepts food once and immediately stops motion, attacks and collisions.</summary>
        public bool TryFeed()
        {
            if (_fed || !_monster.isActiveAndEnabled) return false;

            _fed = true;
            _monster.enabled = false;
            _attack.enabled = false;
            _targeting.enabled = false;
            _truckRoutine.enabled = false;
            _collider.enabled = false;
            _body.simulated = false;
            _startColor = _definition.Color;
            _renderer.color = _startColor;
            return true;
        }

        private void Update()
        {
            if (!_fed) return;

            _elapsed += Time.deltaTime;
            float duration = _tuning.PurifySeconds;
            float progress = duration > 0f ? _elapsed / duration : 1f;
            _renderer.color = Color.Lerp(_startColor, Color.white, progress);
            if (progress < 1f) return;

            enabled = false;
            Purified?.Invoke(_monster);
        }
    }
}
