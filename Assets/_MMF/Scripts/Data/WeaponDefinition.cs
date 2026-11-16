using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared melee weapon range, arc, damage and cooldown.</summary>
    [CreateAssetMenu(menuName = "MMF/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _range = 1.5f;
        [SerializeField, Range(0f, 360f)] private float _angle = 90f;
        [SerializeField, Min(1)] private int _damage = 1;
        [SerializeField, Min(0f)] private float _cooldown = 0.4f;

        public float Range => _range;
        public float Angle => _angle;
        public int Damage => _damage;
        public float Cooldown => _cooldown;
    }
}
