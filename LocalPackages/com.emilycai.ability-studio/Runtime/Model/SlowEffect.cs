using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Reduces voluntary movement speed for a limited time.</summary>
    [Serializable, SkillModule("Effects/Slow", DisplayName = "减速")]
    public sealed class SlowEffect : EffectModule
    {
        [SerializeField] private float reductionPercent = 50;
        [SerializeField] private float duration = 3;
        /// <summary>Movement speed reduction from greater than zero through 100 percent.</summary>
        public float ReductionPercent { get => reductionPercent; set => reductionPercent = value; }
        /// <summary>Duration in seconds.</summary>
        public float Duration { get => duration; set => duration = value; }
    }
}
