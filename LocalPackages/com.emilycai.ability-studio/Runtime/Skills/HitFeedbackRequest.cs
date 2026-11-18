using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Presentation request emitted by the shared runtime; games choose camera/audio/VFX adapters.</summary>
    public readonly struct HitFeedbackRequest
    {
        /// <summary>Creates a request with cast-time parameters and hit-time context.</summary>
        public HitFeedbackRequest(HitFeedbackEffect effect, ISkillCaster caster, ISkillTarget target, Vector3 position)
        { Effect = effect; Caster = caster; Target = target; Position = position; }
        /// <summary>Detached parameter snapshot; never the editable skill module.</summary>
        public HitFeedbackEffect Effect { get; }
        /// <summary>Source for player/camera filtering.</summary>
        public ISkillCaster Caster { get; }
        /// <summary>Target, including a target killed by this hit.</summary>
        public ISkillTarget Target { get; }
        /// <summary>Impact position in runtime world coordinates.</summary>
        public Vector3 Position { get; }
    }
}
