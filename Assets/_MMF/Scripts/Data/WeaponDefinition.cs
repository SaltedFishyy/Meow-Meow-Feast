using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared melee weapon range, arc, damage and cooldown.</summary>
    [TuningGroup("武器", 6)]
    [CreateAssetMenu(menuName = "MMF/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField, TuningLabel("攻击距离"), Min(0f)] private float _range = 1.5f;
        [SerializeField, TuningLabel("攻击角度（度）"), Range(0f, 360f)] private float _angle = 90f;
        [SerializeField, TuningLabel("伤害"), Min(1)] private int _damage = 1;
        [SerializeField, TuningLabel("两次攻击间隔（秒）"), Min(0f)] private float _cooldown = 0.4f;

        [SerializeField, TuningLabel("挥动光效几秒"), Min(0f)] private float _swingFlashDuration = 0.2f;

        public float SwingFlashDuration => _swingFlashDuration;
        public float Range => _range;
        public float Angle => _angle;
        public int Damage => _damage;
        public float Cooldown => _cooldown;
    }
}
