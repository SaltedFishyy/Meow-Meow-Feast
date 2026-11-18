using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Applies movement slow through an optional target capability.</summary>
    public sealed class SlowEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule effect) => effect is SlowEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule effect, EffectContext context)
        {
            if (context.Target is ISlowReceiver receiver)
            {
                var slow = (SlowEffect)effect;
                receiver.ApplySlow(slow.ReductionPercent, slow.Duration);
            }
        }
    }
}
