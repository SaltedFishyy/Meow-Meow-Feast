using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Game adapter for the source of a cast.</summary>
    public interface ISkillCaster
    {
        /// <summary>World position.</summary>
        Vector3 Position { get; }
        /// <summary>World orientation for local cast offsets.</summary>
        Quaternion Rotation { get; }
        /// <summary>Targets on this team are excluded.</summary>
        int Team { get; }
    }
}
