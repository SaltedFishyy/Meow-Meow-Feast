using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared truck health and layout tuning.</summary>
    [TuningGroup("餐车", 10)]
    public sealed class TruckTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("餐车最多几滴血（下次 Play 生效）"), Min(1)] private int _maxHealth = 100;

        [SerializeField, TuningLabel("车长（左右，下次 Play 生效）"), Min(4f)] private float _length = 12f;
        [SerializeField, TuningLabel("车宽（上下，下次 Play 生效）"), Min(3f)] private float _width = 7f;
        [SerializeField, TuningLabel("门宽（下次 Play 生效）"), Min(1.02f)] private float _doorWidth = 2f;

        /// <summary>Horizontal exterior length sampled at the start of Play.</summary>
        public float Length => _length;
        /// <summary>Vertical exterior width sampled at the start of Play.</summary>
        public float Width => _width;
        /// <summary>Width of both door openings sampled at the start of Play.</summary>
        public float DoorWidth => _doorWidth;

        /// <summary>Full health sampled at the start of Play.</summary>
        public int MaxHealth => _maxHealth;
    }
}
