using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Requests a visual-only backward lean followed by recovery.</summary>
    [Serializable, SkillModule("Feedback/Recoil", DisplayName = "后仰")]
    public sealed class RecoilFeedback : HitFeedbackEffect
    {
        [SerializeField] private float angle = 15;
        [SerializeField] private float leanDuration = .06f;
        [SerializeField] private float recoveryDuration = .18f;
        /// <summary>Maximum lean angle in degrees, from zero through 60.</summary>
        public float Angle { get => angle; set => angle = value; }
        /// <summary>Seconds to reach the peak lean.</summary>
        public float LeanDuration { get => leanDuration; set => leanDuration = value; }
        /// <summary>Seconds to return to the original visual rotation.</summary>
        public float RecoveryDuration { get => recoveryDuration; set => recoveryDuration = value; }
    }
}
