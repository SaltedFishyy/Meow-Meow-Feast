using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>A straight projectile that hits the first opposing target.</summary>
    [Serializable, SkillModule("Attacks/Projectile", DisplayName = "投射物")]
    public sealed class ProjectileAttack : AttackModule
    {
        [SerializeField] private float speed = 15;
        [SerializeField] private float range = 12;
        [SerializeField] private float size = 0.5f;
        /// <summary>Travel speed in meters per second.</summary>
        public float Speed { get => speed; set => speed = value; }
        /// <summary>Maximum travel distance.</summary>
        public float Range { get => range; set => range = value; }
        /// <summary>Projectile radius in meters.</summary>
        public float Size { get => size; set => size = value; }
    }
}
