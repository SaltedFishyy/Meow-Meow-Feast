using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Target adapter; implement damage and displacement capabilities as needed.</summary>
    public interface ISkillTarget
    {
        /// <summary>World center on the ground plane.</summary>
        Vector3 Position { get; }
        /// <summary>XY hit radius; Z is presentation depth.</summary>
        float Radius { get; }
        /// <summary>Friendly-fire group.</summary>
        int Team { get; }
        /// <summary>Whether this target can currently be hit.</summary>
        bool IsAlive { get; }
    }
}
