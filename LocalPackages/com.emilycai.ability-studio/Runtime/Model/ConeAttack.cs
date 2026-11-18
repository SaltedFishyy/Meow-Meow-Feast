using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Instant ground-plane sector centered on the aim direction.</summary>
    [Serializable, SkillModule("Attacks/Cone", DisplayName = "扇形")]
    public sealed class ConeAttack : AttackModule
    {
        [SerializeField] private float range = 6;
        [SerializeField] private float minRange = 2;
        /// <summary>Minimum reach when driven by a connected charge.</summary>
        public float MinRange { get => minRange; set => minRange = value; }
        [SerializeField] private float angleDegrees = 90;
        /// <summary>Radial reach in metres.</summary>
        public float Range { get => range; set => range = value; }
        /// <summary>Full opening angle in degrees, greater than zero through 360.</summary>
        public float AngleDegrees { get => angleDegrees; set => angleDegrees = value; }
    }
}
