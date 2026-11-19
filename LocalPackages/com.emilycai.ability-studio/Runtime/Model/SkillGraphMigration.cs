using System.Linq;
namespace AbilityStudio.Model
{
    /// <summary>Loss-preserving upgrade of legacy and integrated graphs to explicit release chains.</summary>
    public static class SkillGraphMigration
    {
        public static bool Upgrade(SkillDefinition skill)
        {
            if (skill == null || skill.AreaGraph) return false;
            skill.MigrateEndpointToCore();
            bool integrated = skill.IntegratedAttacks, dual = skill.WeaponMode;
            var primary = SkillInputKey.MouseLeft;
            var areas = skill.Attacks.ToArray();
            var active = skill.ActiveRelease;
            var bindings = skill.Bindings.ToArray();
            foreach (var area in areas)
            {
                var release = integrated ? area.ReleaseSettings : dual ? skill.ReleaseForAttack(area) : active;
                if (!integrated)
                {
                    area.Settings.BaseDamage = dual && release != null ? release.BaseDamage : skill.BaseDamage;
                    area.Settings.Cooldown = dual && release != null ? release.Cooldown : skill.Cooldown;
                    area.Settings.Distance = dual && release != null ? release.Distance : skill.Distance;
                }
                area.CastOriginOffset = skill.CastOriginOffset;
            }
            skill.IntegratedAttacks = false; skill.WeaponMode = true;
            skill.MarkAreaGraph(dual, primary);
            if (integrated)
            {
                foreach (var area in areas)
                {
                    var trigger = bindings.FirstOrDefault(b => b.TargetId == area.ModuleId && b.Input == BindingRules.Trigger);
                    skill.SetBinding(null, area.ModuleId, BindingRules.Trigger);
                    ReleaseModule release = area.IsCharged ? (ReleaseModule)new ChargeRelease { Duration = area.Settings.Duration } : new InstantRelease();
                    skill.AddRelease(release);
                    if (trigger != null && skill.HasCore) skill.SetBinding(SkillDefinition.CoreLayoutId, release.ModuleId, BindingRules.Trigger, trigger.SourcePort);
                    skill.SetBinding(release.ModuleId, area.ModuleId, BindingRules.Cast);
                    if (area.IsCharged && (area is CircleAttack || area is ConeAttack || area is LineThrustAttack))
                        skill.SetBinding(release.ModuleId, area.ModuleId, BindingRules.Charge, BindingRules.Charge);
                }
            }
            else if (!dual)
            {
                skill.SetBinding(null, SkillDefinition.CoreLayoutId, BindingRules.Release);
                if (active == null && areas.Length > 0) { active = new InstantRelease(); skill.AddRelease(active); }
                if (active != null)
                {
                    if (skill.HasCore) skill.SetBinding(SkillDefinition.CoreLayoutId, active.ModuleId, BindingRules.Trigger, BindingRules.Normal);
                    foreach (var area in areas) skill.SetBinding(active.ModuleId, area.ModuleId, BindingRules.Cast);
                }
            }
            // Endpoints remain on the core; their numeric resolution uses the current area.
            foreach (var binding in bindings)
                if (binding.SourcePort == BindingRules.Endpoint && (skill.FindModule(binding.SourceId) is ReleaseModule || skill.FindModule(binding.SourceId) is AttackModule))
                    skill.SetBinding(skill.HasCore ? SkillDefinition.CoreLayoutId : null, binding.TargetId, binding.Input);
            return true;
        }
    }
}
