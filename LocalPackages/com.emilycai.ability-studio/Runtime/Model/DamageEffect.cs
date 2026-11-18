using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Direct damage relative to the cast-time core base damage.</summary>
    [Serializable, SkillModule("Effects/Damage", DisplayName = "倍率")]
    public sealed class DamageEffect : EffectModule
    {
        [SerializeField] private float multiplierPercent = 100;
        /// <summary>100 means one times core base damage; armor is applied once.</summary>
        public float MultiplierPercent { get => multiplierPercent; set => multiplierPercent = value; }
    }
}
