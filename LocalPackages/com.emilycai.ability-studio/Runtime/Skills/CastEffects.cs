using System.Collections.Generic;
using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    // Immutable routing/parameter snapshot owned by a cast, never by the board.
    internal sealed class CastEffects
    {
        internal float BaseDamage;
        internal SkillDefinition Skill;
        internal readonly HashSet<HitFeedbackEffect> FiredFeedback = new HashSet<HitFeedbackEffect>();
        internal readonly List<EffectModule> Modules = new List<EffectModule>();
        internal readonly Dictionary<EffectModule, Vector3> Destinations = new Dictionary<EffectModule, Vector3>();
        internal readonly Dictionary<EffectModule, CastEffects> Completions = new Dictionary<EffectModule, CastEffects>();
    }
}
