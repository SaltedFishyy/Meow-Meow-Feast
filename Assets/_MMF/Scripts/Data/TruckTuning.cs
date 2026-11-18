using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared truck health tuning.</summary>
    [TuningGroup("餐车", 10)]
    public sealed class TruckTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("餐车最多几滴血（下次 Play 生效）"), Min(1)] private int _maxHealth = 100;

        /// <summary>Full health sampled at the start of Play.</summary>
        public int MaxHealth => _maxHealth;
    }
}
