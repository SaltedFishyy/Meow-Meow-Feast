using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Applies bleeding through the target capability.</summary>
    public sealed class BleedEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule effect) => effect is BleedEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule effect, EffectContext context)
        {
            if (context.Target is IBleedReceiver receiver)
            {
                var bleed = (BleedEffect)effect;
                receiver.ApplyBleed(bleed.DamagePerSecond, bleed.Duration, context.Caster);
            }
        }
    }
}
