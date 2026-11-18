using AbilityStudio.Model;
namespace AbilityStudio.Runtime
{
    /// <summary>Extension boundary for reusable effects.</summary>
    public interface IEffectHandler
    {
        /// <summary>Whether this handler implements the module.</summary>
        bool CanHandle(EffectModule effect);
        /// <summary>Applies one module to a hit.</summary>
        void Apply(EffectModule effect, EffectContext context);
    }
}
