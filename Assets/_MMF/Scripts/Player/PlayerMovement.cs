using UnityEngine;

namespace MMF.Player
{
    /// <summary>Moves a physics body using a direction supplied by any input source.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _maxSpeed = 5f;
        [SerializeField, Min(0f)] private float _accelerationTime = 0.1f;
        [SerializeField, Min(0f)] private float _decelerationTime = 0.1f;

        private Rigidbody2D _body;
        private Rigidbody2D Body
        {
            get
            {
                if (_body == null) _body = GetComponent<Rigidbody2D>();
                return _body;
            }
        }
        private Vector2 _moveDirection;
        private bool _blocked;
        private float _speedMultiplier = 1f;

        /// <summary>Current movement intent, or zero while movement is blocked.</summary>
        public Vector2 MoveDirection => _blocked ? Vector2.zero : _moveDirection;

        /// <summary>Unmodified maximum movement speed.</summary>
        public float MaxSpeed => _maxSpeed;

        /// <summary>Sets the target speed scale without changing acceleration or input.</summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0f, multiplier);
        }

        public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
        /// <summary>Immediately stops the body while cooking owns input.</summary>
        public void SetBlocked(bool blocked)
        {
            _blocked = blocked;
            if (blocked) Body.linearVelocity = Vector2.zero;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        /// <summary>Supplies movement intent, limited to unit length.</summary>
        public void SetMoveDirection(Vector2 direction)
        {
            _moveDirection = Vector2.ClampMagnitude(direction, 1f);
            if (_blocked) return;
            if (direction.sqrMagnitude > 0f)
                LastMoveDirection = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? new Vector2(Mathf.Sign(direction.x), 0f) : new Vector2(0f, Mathf.Sign(direction.y));
        }

        private void FixedUpdate()
        {
            if (_blocked) { _body.linearVelocity = Vector2.zero; return; }
            Vector2 targetVelocity = _moveDirection * (_maxSpeed * _speedMultiplier);
            float duration = _moveDirection.sqrMagnitude > 0f
                ? _accelerationTime : _decelerationTime;
            _body.linearVelocity = duration <= 0f
                ? targetVelocity
                : Vector2.MoveTowards(_body.linearVelocity, targetVelocity,
                    _maxSpeed / duration * Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            _moveDirection = Vector2.zero;
            Body.linearVelocity = Vector2.zero;
        }
    }
}
