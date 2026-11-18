using System;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Optional displacement completion. Replacement/reset cancels the old completion callback.</summary>
    public interface ITrackedDisplaceable : IDisplaceable
    {
        /// <summary>Starts a displacement and invokes completion once only if it finishes without cancellation.</summary>
        void DisplaceTracked(Vector3 worldDelta, float duration, Action completed);
    }
}
