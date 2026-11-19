using MMF.Combat;
using MMF.UI;
using AbilityStudio.Model;
using AbilityStudio.Playground;
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

        [SerializeField] private PanDragInput _panDrag = null;

        private InputActionAsset _controlsInstance;
        private InputAction _moveAction;
        private PlayerMovement _movement;
        private PlayerBackpack _backpack;
        private PlayerInteractor _interactor;
        private PlayerAttack _attack;
        private PlayerThrow _throw;
        private Camera _camera;
        private InputActionMap _gameplay;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _backpack = GetComponent<PlayerBackpack>();
            _interactor = GetComponent<PlayerInteractor>();
            _attack = GetComponent<PlayerAttack>();
            _throw = GetComponent<PlayerThrow>();
            _camera = Camera.main;
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
            _gameplay.FindAction("PlaceInPan", true).performed += context => _panDrag?.TryPlaceSelected();
            _gameplay.FindAction("Drop", true).performed += context => { if (_interactor == null || _interactor.CanDrop) _backpack?.Drop(); };
            _gameplay.FindAction("Throw", true).performed += context =>
            {
                if (_attack != null && _attack.WorkshopOwnsRightMouse)
                {
                    if (TryGetMouseWorldPosition(out Vector2 aim)) _attack.PressWorkshopInput(SkillInputKey.MouseRight, aim);
                }
                else _throw?.TryStartCharge();
            };
            _gameplay.FindAction("Throw", true).canceled += context =>
            {
                // Disabling the map also sends canceled for a held button.
                if (!isActiveAndEnabled) { _attack?.CancelWorkshop(); return; }
                if (_attack != null && _attack.WorkshopOwnsRightMouse)
                {
                    if (TryGetMouseWorldPosition(out Vector2 aim)) _attack.ReleaseWorkshopInput(SkillInputKey.MouseRight, aim);
                }
                else _throw?.Release();
            };
            _gameplay.FindAction("Lift", true).performed += context =>
            {
                if (context.control.device is Mouse mouse && _panDrag != null && _panDrag.TryHandlePress(mouse)) return;
                if (_throw != null && _throw.Charging) { _throw.Cancel(); return; }
                if (_interactor == null || _interactor.Lift()) return;
                if (_attack != null && TryGetMouseWorldPosition(out Vector2 worldPosition))
                {
                    if (_attack.HasWorkshopWeapon) _attack.PressWorkshopInput(SkillInputKey.MouseLeft, worldPosition);
                    else _attack.TryAttack(worldPosition);
                }
            };
            _gameplay.FindAction("Lift", true).canceled += context =>
            {
                if (isActiveAndEnabled && TryGetMouseWorldPosition(out Vector2 aim)) _attack?.ReleaseWorkshopInput(SkillInputKey.MouseLeft, aim);
                else _attack?.CancelWorkshop();
            };
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }

        private void Update()
        {
            if (_attack != null && _attack.HasWorkshopWeapon && TryGetMouseWorldPosition(out Vector2 aim))
            {
                PollWorkshopKey(_attack.WorkshopPrimaryKey, aim);
                if (_attack.WorkshopSecondaryKey is SkillInputKey secondary) PollWorkshopKey(secondary, aim);
                if (_attack.WorkshopOwnsRightMouse && _throw != null && _throw.Charging) _throw.Cancel();
            }
            if (_throw != null && _throw.Charging && TryGetMouseWorldPosition(out Vector2 worldPosition))
                _throw.SetAim(worldPosition);
        }

        private void PollWorkshopKey(SkillInputKey key, Vector2 aim)
        {
            if (key == SkillInputKey.MouseLeft || key == SkillInputKey.MouseRight) return;
            var control = SkillInput.Control(key);
            if (control == null) return;
            if (control.wasPressedThisFrame) _attack.PressWorkshopInput(key, aim);
            if (control.wasReleasedThisFrame) _attack.ReleaseWorkshopInput(key, aim);
        }
        private bool TryGetMouseWorldPosition(out Vector2 worldPosition)
        {
            worldPosition = default;
            if (Mouse.current == null || _camera == null) return false;
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            worldPosition = _camera.ScreenToWorldPoint(new Vector3(
                screenPosition.x, screenPosition.y, transform.position.z - _camera.transform.position.z));
            return true;
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
            _attack?.CancelWorkshop();
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
