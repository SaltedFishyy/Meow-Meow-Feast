using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared player movement and monster shove tuning, read live by components.</summary>
    [TuningGroup("玩家", 1)]
    [CreateAssetMenu(menuName = "MMF/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("最大速度"), Min(0f)] private float _maxSpeed = 5f;
        [SerializeField, TuningLabel("加速时间（秒）"), Min(0f)] private float _accelerationTime = 0.1f;
        [SerializeField, TuningLabel("减速时间（秒）"), Min(0f)] private float _decelerationTime = 0.1f;
        [SerializeField, TuningLabel("挤怪物时的速度比例"), Range(0f, 1f)] private float _speedRatio = 0.5f;

        /// <summary>Unmodified maximum movement speed.</summary>
        public float MaxSpeed => _maxSpeed;
        /// <summary>Time to accelerate from rest to maximum speed.</summary>
        public float AccelerationTime => _accelerationTime;
        /// <summary>Time to decelerate from maximum speed to rest.</summary>
        public float DecelerationTime => _decelerationTime;
        /// <summary>Movement speed scale while shoving monsters.</summary>
        public float SpeedRatio => _speedRatio;
    }
}
