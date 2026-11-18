using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Redirects a projectile to nearby enemies it has not hit.</summary>
    [Serializable, SkillModule("Responses/Chain", DisplayName = "连锁")]
    public sealed class ChainResponse : HitResponseModule
    {
        [SerializeField] private int count = 3;
        [SerializeField] private float distance = 5;
        /// <summary>Maximum redirects after hits.</summary>
        public int Count { get => count; set => count = value; }
        /// <summary>Search radius from the last hit point.</summary>
        public float Distance { get => distance; set => distance = value; }
    }
}
