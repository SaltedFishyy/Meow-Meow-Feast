using System.Collections.Generic;
using System.Linq;
using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Type-based ordering, with Pull last. Additional handlers can extend the policy.</summary>
    public sealed class FixedTypeOrderPolicy : IEffectOrderPolicy
    {
        /// <inheritdoc/>
        public IEnumerable<EffectModule> Order(IReadOnlyList<EffectModule> effects) =>
            effects.OrderBy(x => x is DamageEffect ? -100 : x is PullEffect ? 100 : x is KnockbackEffect ? 50 : 0).ThenBy(x => x.GetType().FullName, System.StringComparer.Ordinal);
    }
}

