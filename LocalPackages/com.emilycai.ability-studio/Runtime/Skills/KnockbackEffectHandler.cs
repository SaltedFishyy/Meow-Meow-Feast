using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Pushes targets away from the caster, with facing as the coincident-position fallback.</summary>
    public sealed class KnockbackEffectHandler : IEffectHandler
    {
        /// <inheritdoc/>
        public bool CanHandle(EffectModule effect) => effect is KnockbackEffect;
        /// <inheritdoc/>
        public void Apply(EffectModule effect, EffectContext context)
        {
            if (!(context.Target is IDisplaceable movable)) return;
            var direction = context.Target.Position - context.Caster.Position;
            direction.z = 0;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = context.Caster.Rotation * Vector3.right;
                direction.z = 0;
                if (direction.sqrMagnitude < 0.0001f) direction = Vector3.right;
            }
            movable.Displace(direction.normalized * ((KnockbackEffect)effect).Distance, 0.3f);
        }
    }
}
