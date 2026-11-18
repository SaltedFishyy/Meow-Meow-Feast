using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared appearance, movement and lifetime tuning for a monster kind.</summary>
    [TuningGroup("怪物", 5)]
    [CreateAssetMenu(menuName = "MMF/Monster Definition")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [SerializeField, TuningLabel("大小（下次出生生效）"), Min(0.01f)] private float _size = 0.8f;
        [SerializeField, TuningLabel("血量（下次出生生效）"), Min(1)] private int _health = 3;
        [SerializeField, TuningLabel("移动速度"), Min(0f)] private float _speed = 1.5f;
        [SerializeField, TuningLabel("死后几秒重生"), Min(0f)] private float _respawnDelay = 20f;
        [SerializeField, TuningLabel("被打闪白几秒"), Min(0f)] private float _flashDuration = 0.1f;
        [SerializeField, TuningLabel("被打后退距离"), Min(0f)] private float _knockbackDistance = 0.3f;
        [SerializeField, TuningLabel("颜色（下次出生生效）")] private Color _color = new Color(0.6f, 0.25f, 0.8f);

        [SerializeField, TuningLabel("多近开始追玩家"), Min(0f)] private float _chaseStartDistance = 8f;
        [SerializeField, TuningLabel("多远不再追玩家"), Min(0f)] private float _chaseStopDistance = 10f;
        [SerializeField, TuningLabel("去车左边的几率（下次出生生效）"), Range(0f, 1f)] private float _leftEndProbability = 0.7f;

        [SerializeField, TuningLabel("绕开障碍几秒"), Min(0f)] private float _avoidanceDuration = 0.5f;

        [SerializeField, TuningLabel("攻击范围（边到边距离）"), Min(0f)] private float _attackRange = 1f;
        [SerializeField, TuningLabel("攻击前提示几秒"), Min(0f)] private float _attackWindUp = 0.5f;
        [SerializeField, TuningLabel("攻击冷却几秒"), Min(0f)] private float _attackCooldown = 1.5f;
        [SerializeField, TuningLabel("每下伤害"), Min(0)] private int _attackDamage = 1;
        [SerializeField, TuningLabel("打车每下伤害"), Min(0)] private int _truckDamage = 1;

        public float AttackRange => _attackRange;
        public float AttackWindUp => _attackWindUp;
        public float AttackCooldown => _attackCooldown;
        public int AttackDamage => _attackDamage;

        /// <summary>Damage dealt to the truck when a lunge hits.</summary>
        public int TruckDamage => _truckDamage;

        public float AvoidanceDuration => _avoidanceDuration;
        public float ChaseStartDistance => _chaseStartDistance;
        public float ChaseStopDistance => _chaseStopDistance;
        public float LeftEndProbability => _leftEndProbability;
        public float Size => _size;
        public int Health => _health;
        public float Speed => _speed;
        public float RespawnDelay => _respawnDelay;
        public float FlashDuration => _flashDuration;
        public float KnockbackDistance => _knockbackDistance;
        public Color Color => _color;
    }
}
