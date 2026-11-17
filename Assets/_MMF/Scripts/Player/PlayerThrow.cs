using MMF.Data;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Tracks charge and aim without reading input or removing inventory items.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerThrow : MonoBehaviour
    {
        [SerializeField] private ThrowTuning _tuning = null;
        [SerializeField] private PlayerBackpack _backpack = null;
        [SerializeField] private PlayerInteractor _interactor = null;
        private float _chargeStarted;

        /// <summary>Whether a charge is active.</summary>
        public bool Charging { get; private set; }
        /// <summary>Current distance calculated using live tuning.</summary>
        public float Distance => Mathf.Lerp(_tuning.MinimumDistance, _tuning.MaximumDistance,
            Charging ? Mathf.Clamp01((Time.time - _chargeStarted) / _tuning.ChargeSeconds) : 0f);
        /// <summary>Last nonzero aim direction, initially right.</summary>
        public Vector2 Direction { get; private set; } = Vector2.right;

        /// <summary>Starts charging if the selected item and interaction state allow it.</summary>
        public bool TryStartCharge()
        {
            if (Charging || _tuning == null || _backpack == null || _backpack.Model == null ||
                (_interactor != null && !_interactor.CanDrop) ||
                _backpack.ItemAt(_backpack.Model.SelectedSlot) == null) return false;
            _chargeStarted = Time.time;
            Charging = true;
            return true;
        }

        /// <summary>Ends charging without throwing an item in T008a.</summary>
        public void Release()
        {
            if (!Charging) return;
            Charging = false;
        }

        /// <summary>Discards the current charge.</summary>
        public void Cancel() => Charging = false;

        /// <summary>Updates aim, retaining the last direction at the player center.</summary>
        public void SetAim(Vector2 worldPosition)
        {
            Vector2 offset = worldPosition - (Vector2)transform.position;
            if (offset.sqrMagnitude > 0f) Direction = offset / offset.magnitude;
        }

        private void OnDisable() => Cancel();
    }
}
