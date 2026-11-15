using System.Text;
using MMF.Cooking;
using MMF.Data;
using UnityEngine;
using UnityEngine.UI;
namespace MMF.UI
{
    /// <summary>Right-hand frying view and flip animation, driven by forwarded input.</summary>
    public sealed class FryingPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _view = null;
        [SerializeField] private RectTransform _backpackRow = null;
        private readonly Vector3[] _corners = new Vector3[4];
        [SerializeField] private Image _pan = null;
        [SerializeField] private Image _food = null;
        [SerializeField] private Text _letters = null;
        [SerializeField] private Text _status = null;
        [SerializeField] private Sprite _panSprite = null;
        [SerializeField, Range(0.1f, 1f)] private float _widthFraction = 0.333333f;
        [SerializeField, Min(0.01f)] private float _flipSeconds = 0.4f;
        private FryingSequence _sequence;
        private Color _cookedColor;
        private float _flipElapsed;
        private int _lastSecond = -1;
        public bool Finished => _sequence != null && (_sequence.Cooked || _sequence.Burnt);
        public bool Burnt => _sequence != null && _sequence.Burnt;
        private void Awake() { _view.gameObject.SetActive(false); }
        /// <summary>Displays a fresh first round for the raw item.</summary>
        public void Open(FryingSequence sequence, ItemDefinition item)
        {
            _sequence = sequence; sequence.Start(); _flipElapsed = 0f; _lastSecond = -1;
            _view.anchorMin = new Vector2(1f - _widthFraction, 0f); _view.anchorMax = Vector2.one;
            _view.offsetMin = _view.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            RefreshLayout();
            _pan.sprite = _panSprite; _food.color = item.Color; _food.transform.localScale = Vector3.one;
            _cookedColor = item.FryingResult.Color;
            _view.gameObject.SetActive(true); RefreshLetters(); RefreshStatus();
        }
        /// <summary>Clears the session and hides the frying view.</summary>
        public void Close()
        {
            _sequence?.Cancel(); _sequence = null; _view.gameObject.SetActive(false);
        }
        /// <summary>Applies a forwarded letter and refreshes progress.</summary>
        public void Press(char letter)
        {
            if (_sequence == null) return;
            int before = _sequence.Progress;
            _sequence.Press(letter);
            if (before != _sequence.Progress) RefreshLetters();
        }
        private void Update()
        {
            if (_sequence == null || !_sequence.Active) return;
            if (_sequence.Flipping)
            {
                _flipElapsed += Time.deltaTime;
                float phase = Mathf.Clamp01(_flipElapsed / _flipSeconds);
                _food.transform.localScale = new Vector3(1f, Mathf.Abs(1f - 2f * phase), 1f);
                if (phase >= 0.5f) _food.color = _cookedColor;
                if (phase >= 1f)
                {
                    _sequence.FinishFlip(); _food.transform.localScale = Vector3.one;
                    _lastSecond = -1; RefreshLetters();
                }
            }
            else _sequence.Tick(Time.deltaTime);
            RefreshStatus();
        }
        private void LateUpdate()
        {
            if (_sequence != null) RefreshLayout();
        }
        private void RefreshLayout()
        {
            _backpackRow.GetWorldCorners(_corners);
            RectTransform parent = (RectTransform)_view.parent;
            float top = parent.InverseTransformPoint(_corners[1]).y;
            _view.offsetMin = new Vector2(0f, Mathf.Max(0f, top - parent.rect.yMin));
        }
        private void RefreshLetters()
        {
            var text = new StringBuilder();
            for (int i = 0; i < _sequence.Length; i++)
            {
                text.Append(i < _sequence.Progress ? "<color=#65DD80>" : "<color=#FFFFFF>");
                text.Append(_sequence.LetterAt(i)); text.Append("</color> ");
            }
            _letters.text = text.ToString();
        }
        private void RefreshStatus()
        {
            int second = Mathf.CeilToInt(_sequence.Remaining);
            if (second == _lastSecond) return;
            _lastSecond = second;
            _status.text = (_sequence.Round == 1 ? "第 1 轮（正面）" : "第 2 轮（反面）") + "\n剩余 " + second + " 秒\nE / Esc 退出";
        }
    }
}
