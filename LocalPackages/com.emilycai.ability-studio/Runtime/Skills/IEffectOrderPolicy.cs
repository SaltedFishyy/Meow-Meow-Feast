using System.Collections.Generic;
using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Owns execution ordering independently of designer display order.</summary>
    public interface IEffectOrderPolicy
    {
        /// <summary>Enumerates effects in execution order.</summary>
        IEnumerable<EffectModule> Order(IReadOnlyList<EffectModule> effects);
    }
}
