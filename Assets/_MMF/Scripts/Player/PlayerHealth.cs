using System;
using MMF.Data;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Tracks the player's health and reports changes to listeners.</summary>
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerHealthTuning _tuning = null;

        private double _invincibleUntil;

        /// <summary>Raised when an accepted hit reduces health.</summary>
        public event Action<Vector2> Hit;

        /// <summary>Raised when damage immunity starts.</summary>
        public event Action InvincibilityStarted;

        /// <summary>Raised when a hit reduces health to zero.</summary>
        public event Action Downed;

        /// <summary>Raised after revival starts damage immunity.</summary>
        public event Action Revived;

        /// <summary>Raised when current or maximum health changes.</summary>
        public event Action Changed;

        /// <summary>Health remaining in this life.</summary>
        public int Current { get; private set; }

        /// <summary>Maximum health sampled on enable and revival.</summary>
        public int Max { get; private set; }

        /// <summary>Whether the player is waiting to revive.</summary>
        public bool IsDown => Current == 0;

        /// <summary>Whether damage immunity is still active.</summary>
        public bool IsInvincible => Time.timeAsDouble < _invincibleUntil;

        /// <summary>Whether this player can currently lose health.</summary>
        public bool CanBeHit => Current > 0 && !IsInvincible;

        /// <summary>Starts damage immunity using the current tuning.</summary>
        public void StartInvincibility()
        {
            _invincibleUntil = Time.timeAsDouble + _tuning.InvincibleSeconds;
            if (IsInvincible) InvincibilityStarted?.Invoke();
        }

        private void OnEnable()
        {
            _invincibleUntil = 0;
            Max = _tuning.MaxHealth;
            Current = Max;
            Changed?.Invoke();
        }

        /// <summary>Applies positive damage down to a minimum of zero health.</summary>
        public void TakeHit(int damage, Vector2 direction)
        {
            if (!CanBeHit || damage <= 0) return;
            Current = Mathf.Max(0, Current - damage);
            if (!IsDown) StartInvincibility();
            Hit?.Invoke(direction);
            if (IsDown) Downed?.Invoke();
            Changed?.Invoke();
        }

        /// <summary>Restores the downed player to full health and starts immunity.</summary>
        public void Revive()
        {
            if (!IsDown) return;
            Max = _tuning.MaxHealth;
            Current = Max;
            Changed?.Invoke();
            StartInvincibility();
            Revived?.Invoke();
        }
    }
}
