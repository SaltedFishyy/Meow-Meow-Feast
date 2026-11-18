using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Optional target movement capability.</summary>
    public interface IDisplaceable
    {
        /// <summary>Moves smoothly by a world delta over a duration.</summary>
        void Displace(Vector3 worldDelta, float duration);
    }
}
