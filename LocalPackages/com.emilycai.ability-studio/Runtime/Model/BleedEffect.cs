using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Refreshes damage over time on hit targets.</summary>
    [Serializable, SkillModule("Effects/Bleed", DisplayName = "流血")]
    public sealed class BleedEffect : EffectModule
    {
        [SerializeField] private float damagePerSecond = 1;
        [SerializeField] private float duration = 3;
        /// <summary>Raw damage per second, before target armor.</summary>
        public float DamagePerSecond { get => damagePerSecond; set => damagePerSecond = value; }
        /// <summary>Seconds of bleeding applied on impact.</summary>
        public float Duration { get => duration; set => duration = value; }
    }
}
