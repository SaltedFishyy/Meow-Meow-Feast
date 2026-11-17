using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared player movement and monster shove tuning, read live by components.</summary>
    [CreateAssetMenu(menuName = "MMF/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _maxSpeed = 5f;
        [SerializeField, Min(0f)] private float _accelerationTime = 0.1f;
        [SerializeField, Min(0f)] private float _decelerationTime = 0.1f;
        [SerializeField, Range(0f, 1f)] private float _speedRatio = 0.5f;

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
