using System;
using MMF.Data;
using UnityEngine;

namespace MMF.Truck
{
    /// <summary>Tracks the truck's health for the current run.</summary>
    public sealed class TruckHealth : MonoBehaviour
    {
        [SerializeField] private TruckTuning _tuning = null;

        /// <summary>Remaining truck health.</summary>
        public int Current { get; private set; }

        /// <summary>Full health for this run.</summary>
        public int Max { get; private set; }

        /// <summary>Whether the truck still has health.</summary>
        public bool CanBeHit => Current > 0;

        /// <summary>Raised after damage changes health, with current and maximum health.</summary>
        public event Action<int, int> Changed;

        /// <summary>Raised once when damage reduces health to zero.</summary>
        public event Action Destroyed;

        private void Awake()
        {
            Max = _tuning.MaxHealth;
            Current = Max;
        }

        /// <summary>Applies positive damage down to a minimum of zero health.</summary>
        public void TakeHit(int damage)
        {
            if (!CanBeHit || damage <= 0) return;
            Current = Mathf.Max(0, Current - damage);
            bool destroyed = Current == 0;
            Changed?.Invoke(Current, Max);
            if (destroyed) Destroyed?.Invoke();
        }
    }
}
