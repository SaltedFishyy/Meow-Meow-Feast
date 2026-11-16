using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared appearance, movement and lifetime tuning for a monster kind.</summary>
    [CreateAssetMenu(menuName = "MMF/Monster Definition")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _size = 0.8f;
        [SerializeField, Min(1)] private int _health = 3;
        [SerializeField, Min(0f)] private float _speed = 1.5f;
        [SerializeField, Min(0f)] private float _respawnDelay = 20f;
        [SerializeField, Min(0f)] private float _flashDuration = 0.1f;
        [SerializeField, Min(0f)] private float _knockbackDistance = 0.3f;
        [SerializeField] private Color _color = new Color(0.6f, 0.25f, 0.8f);

        [SerializeField, Min(0f)] private float _chaseStartDistance = 8f;
        [SerializeField, Min(0f)] private float _chaseStopDistance = 10f;
        [SerializeField, Range(0f, 1f)] private float _leftEndProbability = 0.7f;

        [SerializeField, Min(0f)] private float _avoidanceDuration = 0.5f;

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
