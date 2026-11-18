using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>A fixed distance along the caster-to-aim direction from the cast origin.</summary>
    [Serializable, SkillModule("Positions/Endpoint", DisplayName = "技能终点")]
    public sealed class SkillEndpointPosition : PositionModule
    {
        [SerializeField] private float distance = 8;
        /// <summary>Distance from cast origin, in metres.</summary>
        public float Distance { get => distance; set => distance = value; }
    }
}
