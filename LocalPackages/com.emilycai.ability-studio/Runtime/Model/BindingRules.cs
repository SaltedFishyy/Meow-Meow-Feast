using System.Linq;
namespace AbilityStudio.Model
{
    /// <summary>Small typed vocabulary for position, hit-target and completed-pull bindings; no arbitrary execution graph.</summary>
    public static class BindingRules
    {
        public const string Trigger = "trigger", Normal = "normal", Special = "special", Cast = "cast", Endpoint = "endpoint";
        public const string Release = "release";
        public const string Charge = "charge";
        public const string BodyTargets = "body";
        public const string TipTargets = "tip";
        /// <summary>Area center or attack origin.</summary>
        public const string Position = "position";
        /// <summary>Targets from an attack or a completed Pull.</summary>
        public const string Targets = "targets";
        /// <summary>Pull destination position.</summary>
        public const string Destination = "destination";
        /// <summary>Projectile whose flight is modified by a hit response.</summary>
        public const string Projectile = "projectile";
        /// <summary>Whether an effect has a target chain rooted in an attack.</summary>
        public static bool HasAttackTargets(SkillDefinition skill, EffectModule effect)
        {
            if (skill == null || effect == null) return false;
            var visited = new System.Collections.Generic.HashSet<string>();
            string current = effect.ModuleId;
            while (visited.Add(current))
            {
                var source = skill.FindModule(skill.BindingSource(current, Targets));
                if (source is AttackModule) return true;
                if (!(source is PullEffect)) return false;
                current = source.ModuleId;
            }
            return false;
        }
        /// <summary>Whether a typed connection is legal and does not introduce a completion cycle.</summary>
        public static bool CanConnect(SkillDefinition skill, string sourceId, string targetId, string input, string sourcePort = "")
        {
            if (skill == null || (sourceId == targetId && !(skill.IntegratedAttacks && input == Position && sourcePort == Endpoint))) return false;
            var source = skill.FindModule(sourceId); var target = skill.FindModule(targetId);
            sourcePort = sourcePort ?? "";
            if (input == Trigger) return skill.WeaponMode && sourceId == SkillDefinition.CoreLayoutId && (skill.IntegratedAttacks ? target is AttackModule : target is ReleaseModule) && (sourcePort == Normal || sourcePort == Special) && !skill.Bindings.Any(b => b.Input == Trigger && b.SourcePort == sourcePort && b.TargetId != targetId);
            if (input == Cast) return skill.WeaponMode && !skill.IntegratedAttacks && source is ReleaseModule && target is AttackModule && sourcePort == "";
            if ((source is ReleaseModule || (skill.IntegratedAttacks && source is AttackModule)) && sourcePort == Endpoint) return skill.WeaponMode && ((input == Position && target is AttackModule) || (input == Destination && target is PullEffect));
            if (input == Release) return !skill.WeaponMode && source is ReleaseModule && targetId == SkillDefinition.CoreLayoutId && sourcePort == "";
            if (input == Charge) return !skill.IntegratedAttacks && source is ChargeRelease && (target is LineThrustAttack || target is CircleAttack || target is ConeAttack) && sourcePort == Charge;
            if (sourcePort != "" && !(source is LineThrustAttack && input == Targets && (sourcePort == BodyTargets || sourcePort == TipTargets))) return false;
            if (input == Projectile) return source is ProjectileAttack && target is HitResponseModule;
            bool isPosition = source is PositionModule || sourceId == SkillDefinition.CoreLayoutId;
            if (input == Position) return isPosition && target is AttackModule;
            if (input == Destination) return isPosition && target is PullEffect;
            if (input == Targets)
            {
                if (!(target is EffectModule)) return false;
                if (source is AttackModule) return true;
                if (!(source is PullEffect)) return false;
                var visited = new System.Collections.Generic.HashSet<string>();
                var current = sourceId;
                while (current != null)
                {
                    if (current == targetId || !visited.Add(current)) return false;
                    current = skill.BindingSource(current, Targets);
                }
                return true;
            }
            return false;
        }
    }
}
