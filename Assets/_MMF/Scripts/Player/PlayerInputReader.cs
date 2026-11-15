using UnityEngine;
using UnityEngine.InputSystem;

namespace MMF.Player
{
    /// <summary>Reads local movement and inventory input and forwards it to player components.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _controls = null;

        private InputActionAsset _controlsInstance;
        private InputAction _moveAction;
        private PlayerMovement _movement;
        private PlayerBackpack _backpack;
        private InputActionMap _gameplay;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _backpack = GetComponent<PlayerBackpack>();
            if (_controls == null)
            {
                Debug.LogError("PlayerInputReader requires the MMFControls input asset.", this);
                enabled = false;
                return;
            }

            // Each reader owns its action state; disabling one cannot disable another.
            _controlsInstance = Instantiate(_controls);
            _moveAction = _controlsInstance.FindAction("Gameplay/Move", true);
            _gameplay = _controlsInstance.FindActionMap("Gameplay", true);
            for (int i = 0; i < 5; i++)
            {
                int slot = i;
                _gameplay.FindAction("SelectSlot" + (i + 1), true).performed += context => _backpack?.SelectSlot(slot);
            }
            _gameplay.FindAction("ScrollSlot", true).performed += context => _backpack?.ScrollSlot(context.ReadValue<float>());
            _gameplay.FindAction("Drop", true).performed += context => _backpack?.Drop();
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }

        private void OnEnable()
        {
            _gameplay?.Enable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            _movement.SetMoveDirection(context.canceled
                ? Vector2.zero
                : Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f));
        }

        private void OnDisable()
        {
            _gameplay?.Disable();
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
