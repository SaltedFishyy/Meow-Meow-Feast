using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Explicit direct damage, independent from bleed and other effects.</summary>
    public sealed class DamageEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule module) => module is DamageEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule module, EffectContext context)
        {
            if (context.Target is IDamageable health)
                health.ApplyDamage(context.BaseDamage * (((DamageEffect)module).MultiplierPercent / 100f) * (100f / (100f + Mathf.Max(0, health.Armor))), context.Caster);
        }
    }
}
