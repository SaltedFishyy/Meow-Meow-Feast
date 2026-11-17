using UnityEngine;

namespace MMF.Data
{
    /// <summary>Live tuning for throw charging.</summary>
    [TuningGroup("丢东西", 8)]
    [CreateAssetMenu(menuName = "MMF/Throw Tuning")]
    public sealed class ThrowTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("蓄满要几秒"), Min(0.01f)] private float _chargeSeconds = 1f;
        [SerializeField, TuningLabel("没蓄力时的出手速度（格/秒）"), Min(0f)] private float _minimumSpeed = 5f;
        [SerializeField, TuningLabel("蓄满时的出手速度（格/秒）"), Min(0f)] private float _maximumSpeed = 12f;
        [SerializeField, TuningLabel("地面摩擦（每秒减速多少格/秒）"), Min(0f)] private float _friction = 12f;
        [SerializeField, TuningLabel("撞墙后保留多少速度（0～1）"), Min(0f), Range(0f, 1f)] private float _bounce = 0.5f;

        /// <summary>Seconds to reach full charge.</summary>
        public float ChargeSeconds => _chargeSeconds;
        /// <summary>Launch speed before charging.</summary>
        public float MinimumSpeed => _minimumSpeed;
        /// <summary>Launch speed at full charge.</summary>
        public float MaximumSpeed => _maximumSpeed;
        /// <summary>Ground deceleration read live while sliding.</summary>
        public float Friction => _friction;
        /// <summary>Fraction of speed retained after impact.</summary>
        public float Bounce => _bounce;
    }
}
