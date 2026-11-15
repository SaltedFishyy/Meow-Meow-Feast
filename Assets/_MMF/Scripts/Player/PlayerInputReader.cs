using UnityEngine;
using UnityEngine.InputSystem;

namespace MMF.Player
{
    /// <summary>Reads local keyboard movement and forwards it to the movement component.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _controls = null;

        private InputActionAsset _controlsInstance;
        private InputAction _moveAction;
        private PlayerMovement _movement;

        /// <summary>The current local movement direction, limited to unit length.</summary>
        public Vector2 MoveInput { get; private set; }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            if (_controls == null)
            {
                Debug.LogError("PlayerInputReader requires the MMFControls input asset.", this);
                enabled = false;
                return;
            }

            // Each reader owns its action state; disabling one cannot disable another.
            _controlsInstance = Instantiate(_controls);
            _moveAction = _controlsInstance.FindAction("Gameplay/Move", true);
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
            _movement.SetMoveDirection(MoveInput);
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            MoveInput = Vector2.zero;
            _movement.SetMoveDirection(Vector2.zero);
        }

        private void OnDestroy()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMove;
                _moveAction.canceled -= OnMove;
            }

            if (_controlsInstance != null)
                Destroy(_controlsInstance);
        }
    }
}
