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
        [SerializeField] private PanCookingView _worldView = null;
        [SerializeField] private PanCookingView _rightView = null;
        [SerializeField] private Image _icon = null;
        [SerializeField, Range(0f, 1f)] private float _minimumSwipeWidthRatio = 0.15f;

        private readonly Vector3[] _corners = new Vector3[4];
        private PanCookingView _dragView;
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

        private bool CanDrag => isActiveAndEnabled && _input.isActiveAndEnabled && Application.isFocused;

        private PanCookingView VisibleView => _rightView != null && _rightView.IsVisible
            ? _rightView
            : _worldView != null && _worldView.IsVisible ? _worldView : null;

        private RectTransform HitRect => _dragView.HitRect;
        private RectTransform PanRect => _dragView.PanRect;
        private Image Food => _dragView.Food;
        private Camera EventCamera => _dragView.EventCamera;

        private void Awake()
        {
            _iconRect = _icon.rectTransform;
            _iconParent = (RectTransform)_iconRect.parent;
        }

        /// <summary>Consumes a press over the pan or a backpack slot while the pan is visible.</summary>
        public bool TryHandlePress(Mouse mouse)
        {
            PanCookingView view = VisibleView;
            if (!CanDrag || view == null) return false;

            Vector2 point = mouse.position.ReadValue();
            int slot = _bar.SlotAtScreenPoint(point);
            bool onPan = (_rightView == null || view != _rightView || slot < 0) &&
                RectTransformUtility.RectangleContainsScreenPoint(view.HitRect, point, view.EventCamera);
            if (!onPan && slot < 0) return false;

            CancelDrag();
            _dragView = view;
            _foodRect = Food.rectTransform;
            if (onPan)
            {
                slot = -1;
                _pressPoint = _previousPoint = point;
                _minimumSwipeDistance = GetScreenRect(PanRect).width * _minimumSwipeWidthRatio;
                _startedOnFood = Food.enabled && RectTransformUtility.RectangleContainsScreenPoint(
                    _foodRect, point, EventCamera);
                _swipeHitFood = _startedOnFood;
            }

            ItemDefinition item = slot < 0 ? _pan.Item : _backpack.ItemAt(slot);
            if (item == null) return true;

            _mouse = mouse;
            _sourceSlot = slot;
            _sourceItem = item;
            _iconRect.SetAsLastSibling();
            UpdateIcon(point, slot < 0 ? Food.color : item.Color);
            _icon.enabled = !onPan;
            return true;
        }

        /// <summary>Attempts to place one item from the selected slot while a pan view is visible.</summary>
        public void TryPlaceSelected()
        {
            if (!CanDrag || VisibleView == null) return;

            int slot = _backpack.Model.SelectedSlot;
            ItemDefinition item = _backpack.ItemAt(slot);
            if (item != null && _pan.TryPlace(item))
                _backpack.Model.TryTakeOne(slot, out _);
            else
                _backpack.ShowFailure();
        }

        private void LateUpdate()
        {
            if (_mouse == null) return;

            ItemDefinition item = _sourceSlot < 0 ? _pan.Item : _backpack.ItemAt(_sourceSlot);
            PanCookingView view = VisibleView;
            if (!CanDrag || view == null || view != _dragView || !_mouse.added || item == null || (_sourceSlot >= 0 && item != _sourceItem))
            {
                CancelDrag();
                return;
            }

            Vector2 point = _mouse.position.ReadValue();
            bool fromPan = _sourceSlot < 0;
            if (fromPan)
            {
                if (!_swipeHitFood && Food.enabled)
                    _swipeHitFood = Intersects(_previousPoint, point, GetScreenRect(_foodRect));
                _previousPoint = point;
            }

            if (_mouse.leftButton.isPressed)
            {
                bool overBackpack = _dragView != null && _dragView == _rightView && _bar.SlotAtScreenPoint(point) >= 0;
                _icon.enabled = !fromPan || (_startedOnFood && (overBackpack || !IsOverPan(point)));
                if (_icon.enabled)
                    UpdateIcon(point, fromPan ? Food.color : item.Color);
                return;
            }

            int slot = _sourceSlot;
            bool onPan = IsOverPan(point);
            CancelDrag();
            if (slot >= 0 && onPan)
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
            Camera camera = EventCamera;
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
            RectTransformUtility.RectangleContainsScreenPoint(HitRect, point, EventCamera);

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
            _dragView = null;
            // Scene teardown can destroy the icon before this component is disabled.
            if (_icon != null) _icon.enabled = false;
        }
    }
}
