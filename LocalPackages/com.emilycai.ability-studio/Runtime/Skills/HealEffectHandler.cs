using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Heals living allies without armor reduction.</summary>
    public sealed class HealEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule module) => module is HealEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule module, EffectContext context)
        {
            if (!context.Target.IsAlive || context.Target.Team != context.Caster.Team || !(context.Target is IHealable health)) return;
            var heal = (HealEffect)module;
            if (heal.Mode < 0 || heal.Mode > 2 || !float.IsFinite(heal.Amount) || heal.Amount <= 0) return;
            double amount = heal.Mode == 0 ? heal.Amount : (double)(heal.Mode == 1 ? context.BaseDamage : health.MaxHp) * heal.Amount / 100;
            if (double.IsNaN(amount) || amount <= 0) return;
            health.ApplyHealing((float)System.Math.Min(amount, float.MaxValue), context.Caster);
        }
    }
}
