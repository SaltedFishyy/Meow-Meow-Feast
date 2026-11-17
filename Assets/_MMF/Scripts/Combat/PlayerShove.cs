using MMF.Data;
using MMF.Player;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Slows active movement while asking monsters ahead to move with the player.</summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement), typeof(Rigidbody2D))]
    public sealed class PlayerShove : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private PlayerTuning _tuning;
        [SerializeField, Min(0f)] private float _detectionDistance = 0.05f;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private ContactFilter2D _filter;

        private void Awake()
        {
            if (_movement == null) _movement = GetComponent<PlayerMovement>();
            if (_body == null) _body = GetComponent<Rigidbody2D>();
            _filter = new ContactFilter2D { useTriggers = false };
        }

        private void FixedUpdate()
        {
            _movement.SetSpeedMultiplier(1f);
            Vector2 intent = _movement.MoveDirection;
            if (intent.sqrMagnitude <= 0f) return;

            Vector2 displacement = intent * (_movement.MaxSpeed * _tuning.SpeedRatio * Time.fixedDeltaTime);
            Vector2 direction = intent.normalized;
            int count = _body.Cast(direction, _filter, _hits,
                displacement.magnitude + _detectionDistance);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (Vector2.Dot(hit.normal, direction) >= 0f) continue;
                if (!hit.collider.TryGetComponent<Monster>(out Monster monster)) continue;
                monster.Shove(displacement);
                _movement.SetSpeedMultiplier(_tuning.SpeedRatio);
            }
        }

        private void OnDisable()
        {
            if (_movement != null) _movement.SetSpeedMultiplier(1f);
        }
    }
}
