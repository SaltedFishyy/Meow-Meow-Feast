using MMF.Cooking;
using MMF.Data;
using MMF.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MMF.UI
{
    /// <summary>Routes mouse drags between the backpack bar and the visible pan.</summary>
    [DefaultExecutionOrder(110)]
    public sealed class PanDragInput : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _input = null;
        [SerializeField] private PlayerBackpack _backpack = null;
        [SerializeField] private BackpackBar _bar = null;
        [SerializeField] private PanCooking _pan = null;
        [SerializeField] private Canvas _panCanvas = null;
        [SerializeField] private Image _food = null;
        [SerializeField] private Image _icon = null;

        private RectTransform _panRect;
        private RectTransform _iconRect;
        private RectTransform _iconParent;
        private Mouse _mouse;
        private ItemDefinition _sourceItem;
        private int _sourceSlot;

        private bool CanDrag => isActiveAndEnabled && _input.isActiveAndEnabled &&
                                _panCanvas.isActiveAndEnabled && Application.isFocused;

        private void Awake()
        {
            _panRect = (RectTransform)_panCanvas.transform;
            _iconRect = _icon.rectTransform;
            _iconParent = (RectTransform)_iconRect.parent;
        }

        /// <summary>Consumes a press over the pan or a backpack slot while the pan is visible.</summary>
        public bool TryHandlePress(Mouse mouse)
        {
            if (!CanDrag) return false;

            Vector2 point = mouse.position.ReadValue();
            int slot = _bar.SlotAtScreenPoint(point);
            bool onPan = IsOverPan(point);
            if (!onPan && slot < 0) return false;

            CancelDrag();
            if (onPan)
            {
                slot = -1;
                if (!_food.enabled || !RectTransformUtility.RectangleContainsScreenPoint(
                        _food.rectTransform, point, _panCanvas.worldCamera))
                    return true;
            }

            ItemDefinition item = slot < 0 ? _pan.Item : _backpack.ItemAt(slot);
            if (item == null) return true;

            _mouse = mouse;
            _sourceSlot = slot;
            _sourceItem = item;
            _iconRect.SetAsLastSibling();
            UpdateIcon(point, slot < 0 ? _food.color : item.Color);
            _icon.enabled = true;
            return true;
        }

        private void LateUpdate()
        {
            if (_mouse == null) return;

            ItemDefinition item = _sourceSlot < 0 ? _pan.Item : _backpack.ItemAt(_sourceSlot);
            if (!CanDrag || !_mouse.added || item == null || (_sourceSlot >= 0 && item != _sourceItem))
            {
                CancelDrag();
                return;
            }

            Vector2 point = _mouse.position.ReadValue();
            if (_mouse.leftButton.isPressed)
            {
                UpdateIcon(point, _sourceSlot < 0 ? _food.color : item.Color);
                return;
            }

            int slot = _sourceSlot;
            CancelDrag();
            if (slot >= 0 && IsOverPan(point))
            {
                if (_pan.TryPlace(item))
                    _backpack.Model.TryTakeOne(slot, out _);
                else
                    _backpack.ShowFailure();
            }
            else if (slot < 0 && _bar.SlotAtScreenPoint(point) >= 0)
            {
                if (_backpack.Model.CanAdd(item.Id) && _pan.TryTakeBack(out ItemDefinition taken))
                    _backpack.TryAdd(taken);
                else
                    _backpack.ShowFailure();
            }
        }

        private bool IsOverPan(Vector2 point) =>
            RectTransformUtility.RectangleContainsScreenPoint(_panRect, point, _panCanvas.worldCamera);

        private void UpdateIcon(Vector2 point, Color color)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_iconParent, point, null, out Vector2 position);
            _iconRect.localPosition = position;
            color.a = 0.6f;
            _icon.color = color;
        }

        private void OnDisable() => CancelDrag();

        private void CancelDrag()
        {
            _mouse = null;
            _sourceItem = null;
            // Scene teardown can destroy the icon before this component is disabled.
            if (_icon != null) _icon.enabled = false;
        }
    }
}
