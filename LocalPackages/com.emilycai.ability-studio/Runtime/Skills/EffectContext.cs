using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Hit context shared by reusable effect handlers.</summary>
    public readonly struct EffectContext
    {
        /// <summary>Creates a hit context.</summary>
        public EffectContext(ISkillCaster caster, ISkillTarget target, Vector3 hitPoint, Vector3 aimPoint, Vector3? destination = null, System.Action displacementCompleted = null, float baseDamage = 0)
        { Caster = caster; Target = target; HitPoint = hitPoint; AimPoint = aimPoint; Destination = destination; DisplacementCompleted = displacementCompleted; BaseDamage = baseDamage; }
        /// <summary>Core base damage captured when the skill was released.</summary>
        public float BaseDamage { get; }
        /// <summary>Explicit cast-time destination, or null for legacy caster-at-impact Pull.</summary>
        public Vector3? Destination { get; }
        /// <summary>Optional completion for this displacement only.</summary>
        public System.Action DisplacementCompleted { get; }
        /// <summary>Cast source.</summary>
        public ISkillCaster Caster { get; }
        /// <summary>Hit target.</summary>
        public ISkillTarget Target { get; }
        /// <summary>Impact position.</summary>
        public Vector3 HitPoint { get; }
        /// <summary>Original aim position.</summary>
        public Vector3 AimPoint { get; }
    }
}


