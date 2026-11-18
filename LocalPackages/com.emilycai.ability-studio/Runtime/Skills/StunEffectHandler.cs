using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Applies stun through an optional game-facing status capability.</summary>
    public sealed class StunEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule effect) => effect is StunEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule effect, EffectContext context)
        {
            if (context.Target is IStatusReceiver receiver) receiver.ApplyStun(((StunEffect)effect).Duration);
        }
    }
}
