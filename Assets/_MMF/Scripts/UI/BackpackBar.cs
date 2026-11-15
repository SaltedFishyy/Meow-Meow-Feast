using MMF.Player;
using UnityEngine;
using UnityEngine.UI;
namespace MMF.UI
{
    /// <summary>Builds a centered screen-space inventory row and refreshes it on events.</summary>
    public sealed class BackpackBar : MonoBehaviour
    {
        [SerializeField] private PlayerBackpack _backpack = null;
        [SerializeField] private Font _font = null;
        [SerializeField] private RectTransform _row = null;
        [SerializeField] private Vector2 _slotSize = new Vector2(110f, 72f);
        [SerializeField] private float _spacing = 8f;
        [SerializeField] private Color _selectedColor = new Color(1f, 0.8f, 0.25f);
        [SerializeField] private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        private Image[] _backgrounds;
        private Image[] _swatches;
        private Text[] _labels;
        private void Start()
        {
            int count = _backpack.Model.SlotCount;
            _backgrounds = new Image[count]; _swatches = new Image[count]; _labels = new Text[count];
            _row.sizeDelta = new Vector2(count * (_slotSize.x + _spacing) - _spacing, _slotSize.y);
            for (int i = 0; i < count; i++)
            {
                RectTransform slot = Rect("Slot " + (i + 1), _row, _slotSize);
                slot.anchoredPosition = new Vector2((i - (count - 1) * 0.5f) * (_slotSize.x + _spacing), 0f);
                _backgrounds[i] = slot.gameObject.AddComponent<Image>();
                RectTransform swatch = Rect("Color", slot, new Vector2(18f, 18f));
                swatch.anchoredPosition = new Vector2(0f, 18f);
                _swatches[i] = swatch.gameObject.AddComponent<Image>();
                RectTransform label = Rect("Name and count", slot, new Vector2(_slotSize.x - 4f, 36f));
                label.anchoredPosition = new Vector2(0f, -15f);
                Text text = label.gameObject.AddComponent<Text>();
                text.font = _font; text.fontSize = 18; text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white; text.raycastTarget = false; _labels[i] = text;
            }
            _backpack.ContentsChanged += Refresh; _backpack.SelectionChanged += Refresh;
            Refresh();
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.sizeDelta = size; return rect;
        }
        private void Refresh()
        {
            for (int i = 0; i < _labels.Length; i++)
            {
                var item = _backpack.ItemAt(i);
                _backgrounds[i].color = i == _backpack.Model.SelectedSlot ? _selectedColor : _normalColor;
                _swatches[i].enabled = item != null;
                if (item != null) _swatches[i].color = item.Color;
                int count = _backpack.Model.CountAt(i);
                _labels[i].text = item == null ? "空" : item.DisplayName + (count > 1 ? " x" + count : "");
            }
        }
        private void OnDestroy()
        {
            if (_backpack == null) return;
            _backpack.ContentsChanged -= Refresh; _backpack.SelectionChanged -= Refresh;
        }
    }
}

