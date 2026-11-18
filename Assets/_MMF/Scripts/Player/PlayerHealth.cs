using System;
using MMF.Data;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Tracks the player's health and reports changes to listeners.</summary>
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerHealthTuning _tuning = null;

        /// <summary>Raised after health is restored on enable or reduced by a hit.</summary>
        public event Action Changed;

        /// <summary>Health remaining in this life.</summary>
        public int Current { get; private set; }

        /// <summary>Maximum health sampled when enabled.</summary>
        public int Max { get; private set; }

        /// <summary>Whether this player has health left to lose.</summary>
        public bool CanBeHit => Current > 0;

        private void OnEnable()
        {
            Max = _tuning.MaxHealth;
            Current = Max;
            Changed?.Invoke();
        }

        /// <summary>Applies positive damage down to a minimum of zero health.</summary>
        public void TakeHit(int damage, Vector2 direction)
        {
            if (!CanBeHit || damage <= 0) return;
            Current = Mathf.Max(0, Current - damage);
            Changed?.Invoke();
        }
    }
}
