using System.Collections.Generic;
using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>A cast-time snapshot ticked by the shared executor; has no rendering dependency.</summary>
    public sealed class SkillProjectile
    {
        internal ISkillCaster Caster;
        internal Vector3 Direction;
        internal Vector3 Aim;
        internal float Speed;
        internal float Remaining;
        internal float Damage;
        internal CastEffects Effects;
        internal int Pierces, Chains;
        internal float ChainDistance, ReturnMultiplier, OriginalRange;
        internal readonly HashSet<ISkillTarget> HitTargets = new HashSet<ISkillTarget>();
        /// <summary>Whether this shot is on its return leg.</summary>
        public bool Returning { get; internal set; }
        /// <summary>Current world position.</summary>
        public Vector3 Position { get; internal set; }
        /// <summary>Collision radius.</summary>
        public float Radius { get; internal set; }
        /// <summary>Whether the shot has expired or hit.</summary>
        public bool Finished { get; internal set; }
    }
}

