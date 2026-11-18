using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Moves a hit target toward the caster's position at impact.</summary>
    [Serializable, SkillModule("Effects/Pull", DisplayName = "拉拽")]
    public sealed class PullEffect : EffectModule
    {
        [SerializeField] private float stopDistance = 1.5f;
        [SerializeField] private float duration = 0.45f;
        /// <summary>Distance to leave between target and caster.</summary>
        public float StopDistance { get => stopDistance; set => stopDistance = value; }
        /// <summary>Time in seconds for the visible displacement.</summary>
        public float Duration { get => duration; set => duration = value; }
    }
}
