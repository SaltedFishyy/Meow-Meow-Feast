using MMF.Data;
using MMF.Truck;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Suspends player operations while downed and revives the player inside the truck.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerDowned : MonoBehaviour
    {
        [SerializeField] private PlayerHealthTuning _tuning = null;
        [SerializeField] private TruckArea _truck = null;

        private PlayerHealth _health;
        private PlayerHitReaction _hitReaction;
        private PlayerMovement _movement;
        private Rigidbody2D _body;
        private PlayerInputReader _input;
        private PlayerInteractor _interactor;
        private PlayerThrow _throw;
        private PlayerBackpack _backpack;
        private double _reviveAt;
        private bool _inputWasEnabled;
        private bool _backpackWasEnabled;

        /// <summary>Seconds remaining on the active revival timer.</summary>
        public double RemainingSeconds => _health != null && _health.IsDown
            ? System.Math.Max(0d, _reviveAt - Time.timeAsDouble)
            : 0d;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            _hitReaction = GetComponent<PlayerHitReaction>();
            _movement = GetComponent<PlayerMovement>();
            _body = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInputReader>();
            _interactor = GetComponent<PlayerInteractor>();
            _throw = GetComponent<PlayerThrow>();
            _backpack = GetComponent<PlayerBackpack>();
        }

        private void OnEnable()
        {
            _health.Downed += OnDowned;
            _health.Revived += OnRevived;
        }

        private void OnDisable()
        {
            _health.Downed -= OnDowned;
            _health.Revived -= OnRevived;
        }

        private void OnDowned()
        {
            _reviveAt = Time.timeAsDouble + _tuning.DownSeconds;
            // Disabling the input map also calls the throw action's release callback.
            _throw.Cancel();
            _inputWasEnabled = _input.enabled;
            _input.enabled = false;
            _interactor.Cancel();
            _movement.SetBlocked(true);
            _backpackWasEnabled = _backpack.enabled;
            _backpack.enabled = false;
            _interactor.DropCarriedStation();
        }

        private void Update()
        {
            if (!_health.IsDown || Time.timeAsDouble < _reviveAt || _hitReaction.IsKnockingBack) return;

            _body.position = _truck.InteriorRect.center;
            _body.linearVelocity = Vector2.zero;
            _health.Revive();
        }

        private void OnRevived()
        {
            _movement.SetBlocked(false);
            _input.enabled = _inputWasEnabled;
            _backpack.enabled = _backpackWasEnabled;
        }
    }
}
