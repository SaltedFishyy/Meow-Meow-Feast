using System.Collections.Generic;
namespace AbilityStudio.Runtime
{
    /// <summary>Supplies the active world's target candidates.</summary>
    public interface ITargetProvider
    {
        /// <summary>Active targets, including allies filtered by the executor.</summary>
        IEnumerable<ISkillTarget> Targets { get; }
    }
}
