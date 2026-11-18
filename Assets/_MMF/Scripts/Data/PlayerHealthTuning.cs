using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared player health tuning.</summary>
    [TuningGroup("玩家血量", 9)]
    public sealed class PlayerHealthTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("最多几格血（下次 Play 生效）"), Min(1)] private int _maxHealth = 5;

        [SerializeField, TuningLabel("被打弹开多远"), Min(0f)] private float _knockbackDistance = 0.5f;
        [SerializeField, TuningLabel("被打后无敌几秒"), Min(0f)] private float _invincibleSeconds = 1f;

        [SerializeField, TuningLabel("倒地几秒"), Min(0f)] private float _downSeconds = 3f;

        /// <summary>Duration sampled at the start of a down.</summary>
        public float DownSeconds => _downSeconds;

        /// <summary>Distance traveled on an unobstructed hit.</summary>
        public float KnockbackDistance => _knockbackDistance;

        /// <summary>Duration of damage immunity after a hit.</summary>
        public float InvincibleSeconds => _invincibleSeconds;

        public int MaxHealth => _maxHealth;
    }
}
