using System;
namespace AbilityStudio.Model
{
    /// <summary>Reusable release configuration selected by the core's release input.</summary>
    [Serializable] public abstract class ReleaseModule : SkillModule
    {
        [UnityEngine.SerializeField] private float baseDamage = 10, cooldown = 1, distance = 8;
        /// <summary>Branch damage before effect multipliers and armor.</summary>
        public float BaseDamage { get => baseDamage; set => baseDamage = value; }
        /// <summary>Independent branch cooldown.</summary>
        public float Cooldown { get => cooldown; set => cooldown = value; }
        /// <summary>Maximum endpoint distance for this branch.</summary>
        public float Distance { get => distance; set => distance = value; }
    }
}
