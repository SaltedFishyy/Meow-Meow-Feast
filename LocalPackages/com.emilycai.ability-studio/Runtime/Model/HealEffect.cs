using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Restores allied health using a designer-selected calculation.</summary>
    [Serializable, SkillModule("Effects/Heal", DisplayName = "治疗")]
    public sealed class HealEffect : EffectModule
    {
        [SerializeField] private int mode;
        [SerializeField] private float amount;
        /// <summary>0: fixed HP; 1: percent of cast base value; 2: percent of target maximum HP.</summary>
        public int Mode { get => mode; set => mode = value; }
        /// <summary>Hit points in fixed mode, percentage otherwise.</summary>
        public float Amount { get => amount; set => amount = value; }
    }
}
