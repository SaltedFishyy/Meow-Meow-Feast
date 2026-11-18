using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Returns the projectile to the caster after its outbound flight ends.</summary>
    [Serializable, SkillModule("Responses/Return", DisplayName = "返回")]
    public sealed class ReturnResponse : HitResponseModule
    {
        [SerializeField] private float speedMultiplier = 1;
        /// <summary>Return speed relative to the original projectile speed.</summary>
        public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }
    }
}
