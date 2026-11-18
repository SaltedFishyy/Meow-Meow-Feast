using MMF.Truck;
using UnityEngine;
using UnityEngine.UI;

namespace MMF.UI
{
    /// <summary>Displays the truck's remaining health on the HUD.</summary>
    [RequireComponent(typeof(Image))]
    public sealed class TruckHealthBar : MonoBehaviour
    {
        [SerializeField] private TruckHealth _health = null;
        [SerializeField] private Image _fill = null;
        [SerializeField] private Text _label = null;
        [SerializeField] private Vector2 _size = new Vector2(240f, 28f);
        [SerializeField] private float _topPadding = 46f;
        [SerializeField] private Color _backgroundColor = new Color(0.08f, 0.10f, 0.11f);
        [SerializeField] private Color _fillColor = new Color(0.12f, 0.43f, 0.24f);
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private int _fontSize = 18;

        private bool _started;

        private void Awake()
        {
            Image background = GetComponent<Image>();
            background.rectTransform.sizeDelta = _size;
            background.rectTransform.anchoredPosition = new Vector2(0f, -_topPadding);
            background.color = _backgroundColor;
            _fill.color = _fillColor;
            _label.color = _textColor;
            _label.fontSize = _fontSize;
        }

        private void Start()
        {
            _started = true;
            Refresh(_health.Current, _health.Max);
        }

        private void OnEnable()
        {
            _health.Changed += Refresh;
            if (_started)
                Refresh(_health.Current, _health.Max);
        }

        private void OnDisable()
        {
            _health.Changed -= Refresh;
        }

        private void Refresh(int current, int max)
        {
            float fraction = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            _fill.rectTransform.anchorMax = new Vector2(fraction, 1f);
            _label.text = $"{current}/{max}";
        }
    }
}
