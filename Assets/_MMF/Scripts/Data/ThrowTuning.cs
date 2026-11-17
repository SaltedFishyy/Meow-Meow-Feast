using UnityEngine;

namespace MMF.Data
{
    /// <summary>Live tuning for throw charging.</summary>
    [TuningGroup("丢东西", 8)]
    [CreateAssetMenu(menuName = "MMF/Throw Tuning")]
    public sealed class ThrowTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("蓄满要几秒"), Min(0.01f)] private float _chargeSeconds = 1f;
        [SerializeField, TuningLabel("没蓄力时丢多远"), Min(0f)] private float _minimumDistance = 1f;
        [SerializeField, TuningLabel("蓄满时丢多远"), Min(0f)] private float _maximumDistance = 6f;

        /// <summary>Seconds to reach full charge.</summary>
        public float ChargeSeconds => _chargeSeconds;
        /// <summary>Distance before charging.</summary>
        public float MinimumDistance => _minimumDistance;
        /// <summary>Distance at full charge.</summary>
        public float MaximumDistance => _maximumDistance;
    }
}
