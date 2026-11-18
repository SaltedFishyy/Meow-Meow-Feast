using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Instant circular attack centered on the ground-plane aim point.</summary>
    [Serializable, SkillModule("Attacks/Circle", DisplayName = "圆形范围")]
    public sealed class CircleAttack : AttackModule
    {
        [SerializeField] private float radius = 3;
        [SerializeField] private float minRadius = 1;
        /// <summary>Minimum radius when driven by a connected charge.</summary>
        public float MinRadius { get => minRadius; set => minRadius = value; }
        /// <summary>Ground-plane radius in meters; intersecting target bounds are included.</summary>
        public float Radius { get => radius; set => radius = value; }
    }
}

