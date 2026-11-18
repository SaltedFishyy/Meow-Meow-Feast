using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Generic pull toward the source, clamped to avoid pushing nearby targets away.</summary>
    public sealed class PullEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule effect) => effect is PullEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule effect, EffectContext context)
        {
            if (!(context.Target is IDisplaceable movable)) return;
            var pull = (PullEffect)effect;
            var delta = (context.Destination ?? context.Caster.Position) - context.Target.Position;
            delta.z = 0;
            var movement = delta.normalized * Mathf.Max(0, delta.magnitude - pull.StopDistance);
            if (movable is ITrackedDisplaceable tracked) tracked.DisplaceTracked(movement, pull.Duration, context.DisplacementCompleted);
            else movable.Displace(movement, pull.Duration);
        }
    }
}


