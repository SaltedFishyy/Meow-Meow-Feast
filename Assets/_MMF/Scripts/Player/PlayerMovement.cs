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
        private Vector2 _moveDirection;

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
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = _moveDirection * _maxSpeed;
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
            _body.linearVelocity = Vector2.zero;
        }
    }
}
