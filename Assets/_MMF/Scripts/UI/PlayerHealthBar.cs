using System.Collections.Generic;
using MMF.Player;
using UnityEngine;
using UnityEngine.UI;

namespace MMF.UI
{
    /// <summary>Displays the player's remaining health as a row of squares.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _health = null;
        [SerializeField] private float _squareSize = 18f;
        [SerializeField] private float _spacing = 5f;
        [SerializeField] private Vector2 _edgePadding = new Vector2(20f, 20f);
        [SerializeField] private Color _filledColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color _emptyColor = new Color(0.18f, 0.18f, 0.18f, 0.65f);

        private readonly List<Image> _squares = new List<Image>();
        private RectTransform _row;
        private int _visibleCount;

        private void Awake()
        {
            _row = (RectTransform)transform;
            _row.anchoredPosition = new Vector2(_edgePadding.x, -_edgePadding.y);
        }

        private void OnEnable()
        {
            _health.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            _health.Changed -= Refresh;
        }

        private void Refresh()
        {
            int max = _health.Max;
            while (_squares.Count < max)
                AddSquare();

            for (int i = max; i < _visibleCount; i++)
                _squares[i].gameObject.SetActive(false);
            for (int i = _visibleCount; i < max; i++)
                _squares[i].gameObject.SetActive(true);
            _visibleCount = max;

            for (int i = 0; i < max; i++)
                _squares[i].color = i < _health.Current ? _filledColor : _emptyColor;
        }

        private void AddSquare()
        {
            Image square = new GameObject("Health Square", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            RectTransform rect = square.rectTransform;
            rect.SetParent(_row, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(_squareSize, _squareSize);
            rect.anchoredPosition = new Vector2(_squares.Count * (_squareSize + _spacing), 0f);
            square.raycastTarget = false;
            _squares.Add(square);
        }
    }
}
