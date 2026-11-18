using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Unity frame driver; games can replace this with their own manual ticking.</summary>
    public sealed class SkillRuntimeHost : MonoBehaviour, ISkillClock
    {
        private readonly ListTargetProvider _targets = new ListTargetProvider();
        private SkillExecutor _executor;
        /// <inheritdoc/>
        public double Time => UnityEngine.Time.timeAsDouble;
        /// <summary>One shared executor instance for this world.</summary>
        public SkillExecutor Executor => _executor ?? (_executor = new SkillExecutor(this, _targets));
        /// <summary>Registers a gameplay target.</summary>
        public void Register(ISkillTarget target) => _targets.Add(target);
        /// <summary>Removes a gameplay target.</summary>
        public void Unregister(ISkillTarget target) => _targets.Remove(target);
        private void Update() => Tick(UnityEngine.Time.deltaTime);
        /// <summary>Advances projectiles and registered combat targets.</summary>
        public void Tick(float deltaTime)
        {
            Executor.Tick(deltaTime);
            foreach (var target in _targets.Targets) if (target is CombatTarget combat && combat != null) combat.Tick(deltaTime);
        }
    }
}
