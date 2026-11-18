using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Refreshes a timed stun on each hit target.</summary>
    [Serializable, SkillModule("Effects/Stun", DisplayName = "眩晕")]
    public sealed class StunEffect : EffectModule
    {
        [SerializeField] private float duration = 2;
        /// <summary>Seconds of stun applied on impact.</summary>
        public float Duration { get => duration; set => duration = value; }
    }
}
