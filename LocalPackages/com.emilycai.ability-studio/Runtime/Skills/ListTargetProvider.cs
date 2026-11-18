using System.Collections.Generic;
namespace AbilityStudio.Runtime
{
    /// <summary>Explicit target registry for small arenas and deterministic tests.</summary>
    public sealed class ListTargetProvider : ITargetProvider
    {
        private readonly List<ISkillTarget> _targets = new List<ISkillTarget>();
        /// <inheritdoc/>
        public IEnumerable<ISkillTarget> Targets => _targets;
        /// <summary>Registers a target once.</summary>
        public void Add(ISkillTarget target) { if (target != null && !_targets.Contains(target)) _targets.Add(target); }
        /// <summary>Unregisters a target.</summary>
        public void Remove(ISkillTarget target) => _targets.Remove(target);
    }
}
