using MMF.Cooking;
using MMF.Data;
using MMF.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MMF.UI
{
    /// <summary>Routes mouse drags and flip gestures for the visible pan.</summary>
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
        [SerializeField, Range(0f, 1f)] private float _minimumSwipeWidthRatio = 0.15f;

        private readonly Vector3[] _corners = new Vector3[4];
        private RectTransform _panRect;
        private RectTransform _foodRect;
        private RectTransform _iconRect;
        private RectTransform _iconParent;
        private Mouse _mouse;
        private ItemDefinition _sourceItem;
        private int _sourceSlot;
        private Vector2 _pressPoint;
        private Vector2 _previousPoint;
        private float _minimumSwipeDistance;
        private bool _startedOnFood;
        private bool _swipeHitFood;

        private bool CanDrag => isActiveAndEnabled && _input.isActiveAndEnabled &&
                                _panCanvas.isActiveAndEnabled && Application.isFocused;

        private void Awake()
        {
            _panRect = (RectTransform)_panCanvas.transform;
            _foodRect = _food.rectTransform;
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
                _pressPoint = _previousPoint = point;
                _minimumSwipeDistance = GetScreenRect(_panRect).width * _minimumSwipeWidthRatio;
                _startedOnFood = _food.enabled && RectTransformUtility.RectangleContainsScreenPoint(
                    _foodRect, point, _panCanvas.worldCamera);
                _swipeHitFood = _startedOnFood;
            }

            ItemDefinition item = slot < 0 ? _pan.Item : _backpack.ItemAt(slot);
            if (item == null) return true;

            _mouse = mouse;
            _sourceSlot = slot;
            _sourceItem = item;
            _iconRect.SetAsLastSibling();
            UpdateIcon(point, slot < 0 ? _food.color : item.Color);
            _icon.enabled = !onPan;
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
            bool fromPan = _sourceSlot < 0;
            if (fromPan)
            {
                if (!_swipeHitFood && _food.enabled)
                    _swipeHitFood = Intersects(_previousPoint, point, GetScreenRect(_foodRect));
                _previousPoint = point;
            }

            if (_mouse.leftButton.isPressed)
            {
                _icon.enabled = !fromPan || (_startedOnFood && !IsOverPan(point));
                if (_icon.enabled)
                    UpdateIcon(point, fromPan ? _food.color : item.Color);
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
                if (!_startedOnFood) return;

                if (_backpack.Model.CanAdd(item.Id) && _pan.TryTakeBack(out ItemDefinition taken))
                    _backpack.TryAdd(taken);
                else
                    _backpack.ShowFailure();
            }
            else if (fromPan && _swipeHitFood)
            {
                float distanceSquared = (point - _pressPoint).sqrMagnitude;
                if (distanceSquared > 0f && distanceSquared >= _minimumSwipeDistance * _minimumSwipeDistance)
                    FlipFood();
            }
        }

        private void FlipFood()
        {
            if (!_pan.TryFlip(out ItemDefinition cooked) || cooked == null || _backpack.TryAdd(cooked))
                return;

            _pan.PutBackCooked(cooked);
            _backpack.ShowFailure();
        }

        private Rect GetScreenRect(RectTransform rect)
        {
            rect.GetWorldCorners(_corners);
            Camera camera = _panCanvas.worldCamera;
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, _corners[0]);
            Vector2 max = min;
            for (int i = 1; i < _corners.Length; i++)
            {
                Vector2 corner = RectTransformUtility.WorldToScreenPoint(camera, _corners[i]);
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool Intersects(Vector2 start, Vector2 end, Rect rect)
        {
            Vector2 delta = end - start;
            float enter = 0f;
            float exit = 1f;
            return ClipAxis(start.x, delta.x, rect.xMin, rect.xMax, ref enter, ref exit) &&
                   ClipAxis(start.y, delta.y, rect.yMin, rect.yMax, ref enter, ref exit);
        }

        private static bool ClipAxis(float start, float delta, float min, float max, ref float enter, ref float exit)
        {
            if (delta == 0f) return start >= min && start <= max;

            float first = (min - start) / delta;
            float last = (max - start) / delta;
            enter = Mathf.Max(enter, Mathf.Min(first, last));
            exit = Mathf.Min(exit, Mathf.Max(first, last));
            return enter <= exit;
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
