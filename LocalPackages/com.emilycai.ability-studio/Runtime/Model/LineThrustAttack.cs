using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Instant rectangular attack extending forward from the cast origin.</summary>
    [Serializable, SkillModule("Attacks/LineThrust", DisplayName = "长方形")]
    public sealed class LineThrustAttack : AttackModule
    {
        [SerializeField] private float length = 8;
        [SerializeField] private float width = 3;
        [SerializeField] private float minLength = 3;
        [SerializeField] private float tipLength = 1.5f;
        /// <summary>Minimum length used when a charge-ratio input is connected.</summary>
        public float MinLength { get => minLength; set => minLength = value; }
        /// <summary>Front zone depth, capped to the actual cast length.</summary>
        public float TipLength { get => tipLength; set => tipLength = value; }
        /// <summary>Forward reach in metres.</summary>
        public float Length { get => length; set => length = value; }
        /// <summary>Full width in metres, centered across the aim direction.</summary>
        public float Width { get => width; set => width = value; }
    }
}
