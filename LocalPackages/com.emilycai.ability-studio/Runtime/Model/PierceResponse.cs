using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Allows a projectile to continue straight after additional hits.</summary>
    [Serializable, SkillModule("Responses/Pierce", DisplayName = "穿透")]
    public sealed class PierceResponse : HitResponseModule
    {
        [SerializeField] private int count = 2;
        /// <summary>Additional hits the projectile can pass through.</summary>
        public int Count { get => count; set => count = value; }
    }
}
