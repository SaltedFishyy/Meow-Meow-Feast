using System;
using System.Collections;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Displays the downed pose and revival countdown.</summary>
    public sealed class PlayerDownedView : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _health;
        [SerializeField] private PlayerDowned _downed;
        [SerializeField] private SpriteRenderer _visual;
        [SerializeField] private TextMesh _countdown;
        [SerializeField] private GameObject _heldIcon;
        [SerializeField] private Color _downedColor = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Vector2 _downedScale = new Vector2(1.2f, 0.5f);
        [SerializeField, Min(1)] private int _countdownFontSize = 48;
        [SerializeField] private Vector3 _countdownOffset = new Vector3(0f, 0.65f, 0f);

        private Color _standingColor;
        private Vector3 _standingScale;
        private bool _heldIconWasActive;
        private Coroutine _countdownRoutine;

        private void OnEnable()
        {
            _health.Downed += ShowDowned;
            _health.Revived += HideDowned;
        }

        private void OnDisable()
        {
            _health.Downed -= ShowDowned;
            _health.Revived -= HideDowned;
            HideDowned();
        }

        private void ShowDowned()
        {
            _standingColor = _visual.color;
            _standingScale = _visual.transform.localScale;
            SetRgb(_downedColor);
            _visual.transform.localScale = Vector3.Scale(_standingScale,
                new Vector3(_downedScale.x, _downedScale.y, 1f));

            _heldIconWasActive = _heldIcon.activeSelf;
            _heldIcon.SetActive(false);
            _countdown.fontSize = _countdownFontSize;
            _countdown.transform.localPosition = _countdownOffset;
            _countdown.text = string.Empty;
            _countdown.gameObject.SetActive(true);
            _countdownRoutine = StartCoroutine(ShowCountdown());
        }

        private IEnumerator ShowCountdown()
        {
            // Let all Downed listeners run before reading the revival deadline.
            double displayedSeconds = 0d;
            while (true)
            {
                yield return null;
                double seconds = Math.Ceiling(_downed.RemainingSeconds);
                if (seconds == displayedSeconds) continue;

                displayedSeconds = seconds;
                _countdown.text = seconds > 0d ? seconds.ToString("0") : string.Empty;
            }
        }

        private void HideDowned()
        {
            if (_countdownRoutine == null) return;

            StopCoroutine(_countdownRoutine);
            _countdownRoutine = null;
            _countdown.gameObject.SetActive(false);
            _heldIcon.SetActive(_heldIconWasActive);
            _visual.transform.localScale = _standingScale;
            SetRgb(_standingColor);
        }

        private void SetRgb(Color rgb)
        {
            Color color = _visual.color;
            _visual.color = new Color(rgb.r, rgb.g, rgb.b, color.a);
        }
    }
}
