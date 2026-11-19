using MMF.Cooking;
using MMF.Data;
using UnityEngine;
using UnityEngine.UI;
using CookingState = MMF.Cooking.PanCookingModel.CookingState;

namespace MMF.UI
{
    /// <summary>Displays the station's pan and its cooking cues in world space.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class PanCookingView : MonoBehaviour
    {
        [SerializeField] private PanCooking _pan = null;
        [SerializeField] private FryingStation _station = null;
        [SerializeField] private Transform _player = null;
        [SerializeField] private CookingTuning _tuning = null;
        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private RectTransform _backpackRow = null;
        [SerializeField] private Image _food = null;
        [SerializeField] private RectTransform _progressRing = null;
        [SerializeField] private Image _progress = null;
        [SerializeField] private Text _flipHint = null;
        [SerializeField] private Image[] _smoke = null;
        [SerializeField, Min(0.01f)] private float _worldSize = 3f;
        [SerializeField] private float _height = 2f;
        [SerializeField, Min(0f)] private float _pulseFrequency = 3f;
        [SerializeField, Range(0f, 1f)] private float _pulseAmount = 0.15f;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _smokeColor = new Color(0.72f, 0.72f, 0.72f, 0.8f);
        [SerializeField, Min(0f)] private float _smokeCyclesPerSecond = 1.2f;
        [SerializeField] private Vector2 _smokeRise = new Vector2(0f, 100f);
        [SerializeField] private float _smokeSpacing = 22f;

        private RectTransform _foodRect;
        private RectTransform _canvasRect;
        private Camera _camera;
        private CookingState _state;
        private ItemDefinition _previousItem;
        private ItemDefinition _flippingItem;
        private bool _finalFlip;
        private float _flipStarted;

        private void Awake()
        {
            _foodRect = _food.rectTransform;
            _canvasRect = (RectTransform)_canvas.transform;
            _camera = _canvas.worldCamera;
        }

        private void Start()
        {
            _state = _pan.State;
            _previousItem = _pan.Item;
            _pan.StateChanged += OnStateChanged;
        }

        private void OnDestroy() => _pan.StateChanged -= OnStateChanged;

        private void OnStateChanged()
        {
            CookingState next = _pan.State;
            bool firstFlip = _state == CookingState.ReadyFirst && next == CookingState.CookingSecond;
            bool finalFlip = _state == CookingState.ReadySecond && next == CookingState.Empty;
            if (firstFlip || finalFlip)
            {
                _flippingItem = _previousItem;
                _finalFlip = finalFlip;
                _flipStarted = Time.time;
            }
            else if (next != CookingState.ReadySecond &&
                     !(next == CookingState.Holding && _finalFlip && _flippingItem != null))
            {
                _flippingItem = null;
            }

            _state = next;
            _previousItem = _pan.Item;
        }

        private void Update()
        {
            float flipPhase = 1f;
            if (_flippingItem != null && _tuning.FlipSeconds > 0f)
                flipPhase = Mathf.Clamp01((Time.time - _flipStarted) / _tuning.FlipSeconds);
            if (flipPhase >= 1f)
                _flippingItem = null;

            bool visible = _station.InRange(_player.position);
            if (_canvas.gameObject.activeSelf != visible)
                _canvas.gameObject.SetActive(visible);
            if (!visible)
                return;

            bool cooking = _state == CookingState.CookingFirst || _state == CookingState.CookingSecond;
            bool ready = _state == CookingState.ReadyFirst || _state == CookingState.ReadySecond;
            float readyElapsed = _pan.ReadyElapsed;
            float warningStart = Mathf.Max(0f, _pan.BurnSeconds - _tuning.BurnWarningSeconds);
            bool warning = ready && readyElapsed >= warningStart;

            if (_progressRing.gameObject.activeSelf != cooking)
                _progressRing.gameObject.SetActive(cooking);
            if (cooking)
                _progress.fillAmount = Mathf.Clamp01(_pan.SideElapsed / _pan.SideSeconds);

            _flipHint.enabled = ready;
            UpdateFood(flipPhase, ready, warning, readyElapsed);
            UpdateSmoke(warning, readyElapsed - warningStart);
        }

        private void LateUpdate()
        {
            if (!_canvas.gameObject.activeSelf)
                return;

            Rect viewport = _camera.pixelRect;
            float bottom = viewport.yMin;
            if (_backpackRow != null)
            {
                Vector3 rowTop = _backpackRow.TransformPoint(0f, _backpackRow.rect.yMax, 0f);
                bottom = Mathf.Max(bottom, RectTransformUtility.WorldToScreenPoint(null, rowTop).y);
            }

            float availableHeight = viewport.yMax - bottom;
            if (viewport.width <= 0f || availableHeight <= 0f)
                return;

            float pixelsPerWorldUnit = viewport.height / (2f * _camera.orthographicSize);
            float desiredSize = _worldSize * pixelsPerWorldUnit;
            float size = Mathf.Min(desiredSize, Mathf.Min(viewport.width, availableHeight));
            float half = size * 0.5f;
            Vector3 center = _camera.WorldToScreenPoint(transform.TransformPoint(0f, _height, 0f));
            center.x = Mathf.Clamp(center.x, viewport.xMin + half, viewport.xMax - half);
            center.y = Mathf.Clamp(center.y, bottom + half, viewport.yMax - half);

            _canvasRect.localScale = Vector3.one * (_worldSize / _canvasRect.rect.width * (size / desiredSize));
            _canvasRect.position = _camera.ScreenToWorldPoint(center);
        }


        private void UpdateFood(float flipPhase, bool ready, bool warning, float readyElapsed)
        {
            bool flipping = _flippingItem != null;
            ItemDefinition item = flipping ? _flippingItem : _pan.Item;
            _food.enabled = item != null;
            if (item == null)
                return;

            bool browned = flipping
                ? _finalFlip || flipPhase >= 0.5f
                : _state == CookingState.CookingSecond || _state == CookingState.ReadySecond;
            Color color = browned && item.FryingResult != null ? item.FryingResult.Color : item.Color;
            float pulse = ready ? 0.5f + 0.5f * Mathf.Sin(readyElapsed * _pulseFrequency * 2f * Mathf.PI) : 0f;
            float size = 1f + pulse * _pulseAmount;
            float height = flipping ? size * Mathf.Abs(1f - 2f * flipPhase) : size;
            _foodRect.localScale = new Vector3(size, height, 1f);
            _food.color = warning ? Color.Lerp(color, _warningColor, pulse) : color;
        }

        private void UpdateSmoke(bool warning, float elapsed)
        {
            for (int i = 0; i < _smoke.Length; i++)
            {
                Image particle = _smoke[i];
                particle.enabled = warning;
                if (!warning)
                    continue;

                float phase = Mathf.Repeat(elapsed * _smokeCyclesPerSecond + (float)i / _smoke.Length, 1f);
                float x = (i - (_smoke.Length - 1) * 0.5f) * _smokeSpacing;
                particle.rectTransform.anchoredPosition = new Vector2(x, 25f) + _smokeRise * phase;
                Color color = _smokeColor;
                color.a *= 1f - phase;
                particle.color = color;
            }
        }
    }
}
