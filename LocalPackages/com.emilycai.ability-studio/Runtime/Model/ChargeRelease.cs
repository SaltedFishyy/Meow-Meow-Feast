using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Charges while held and casts on release, retaining full charge until released.</summary>
    [Serializable, SkillModule("Release/Charge", DisplayName = "蓄力")]
    public sealed class ChargeRelease : ReleaseModule
    {
        [SerializeField] private float duration = 2;
        /// <summary>Seconds required to reach full charge.</summary>
        public float Duration { get => duration; set => duration = value; }
    }
}
