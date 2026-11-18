using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Moves hit targets away from the caster on the ground plane.</summary>
    [Serializable, SkillModule("Effects/Knockback", DisplayName = "击退")]
    public sealed class KnockbackEffect : EffectModule
    {
        [SerializeField] private float distance = 3;
        /// <summary>Distance in metres to push each target.</summary>
        public float Distance { get => distance; set => distance = value; }
    }
}
