using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Shared cast-time position resolution for gameplay and live previews.</summary>
    public static class SkillPositions
    {
        /// <summary>World cast origin before an explicit position binding.</summary>
        public static Vector3 CastOrigin(SkillDefinition skill, ISkillCaster caster) 
        {
            var offset = caster.Rotation * new Vector3(skill.CastOriginOffset.x, skill.CastOriginOffset.y, 0);
            offset.z = 0;
            return caster.Position + offset;
        }
        /// <summary>Area-owned local offset, shared by execution and previews.</summary>
        public static Vector3 AreaOrigin(SkillDefinition skill, AttackModule area, ISkillCaster caster, Vector3 aim)
        {
            if (!skill.AreaGraph) return CastOrigin(skill, caster);
            var offset = caster.Rotation * new Vector3(area.CastOriginOffset.x, area.CastOriginOffset.y, 0); offset.z = 0;
            var source = skill.BindingSource(area.ModuleId, BindingRules.Origin);
            var origin = caster.Position;
            if (source == SkillDefinition.CoreLayoutId)
            {
                var delta = aim - caster.Position; delta.z = 0;
                origin += Vector3.ClampMagnitude(delta, area.Settings.Distance);
            }
            else if (source != null) origin = ResolveBinding(source, skill, caster, aim);
            return origin + offset;
        }
        public static Vector3 AreaDirection(SkillDefinition skill, AttackModule area, ISkillCaster caster, Vector3 aim)
        { var delta = aim - AreaOrigin(skill, area, caster, aim); delta.z = 0; return delta.normalized; }
        public static Vector3 AreaEndpoint(SkillDefinition skill, AttackModule area, ISkillCaster caster, Vector3 aim)
        {
            var origin = AreaOrigin(skill, area, caster, aim); var delta = aim - origin; delta.z = 0;
            return origin + Vector3.ClampMagnitude(delta, area.Settings.Distance);
        }
        /// <summary>Ground-plane direction of the cast, independent of an area's position.</summary>
        public static Vector3 Direction(SkillDefinition skill, ISkillCaster caster, Vector3 aim)
        { var delta = aim - CastOrigin(skill, caster); delta.z = 0; return delta.normalized; }
        /// <summary>Mouse ground point clamped to Skill Core's maximum cast distance.</summary>
        public static Vector3 Endpoint(SkillDefinition skill, ISkillCaster caster, Vector3 aim)
        {
            var origin = CastOrigin(skill, caster);
            var delta = aim - origin; delta.z = 0;
            return origin + Vector3.ClampMagnitude(delta, skill.Distance);
        }
        /// <summary>Resolves a bound position identity, including Skill Core.</summary>
        public static Vector3 ResolveBinding(string sourceId, SkillDefinition skill, ISkillCaster caster, Vector3 aim) =>
            skill.IntegratedAttacks && skill.FindModule(sourceId) is AttackModule area ? BranchEndpoint(skill,area.ReleaseSettings,caster,aim) : skill.FindModule(sourceId) is ReleaseModule release ? BranchEndpoint(skill, release, caster, aim) : sourceId == SkillDefinition.CoreLayoutId ? Endpoint(skill, caster, aim) : Resolve(skill.FindModule(sourceId) as PositionModule, skill, caster, aim);
        /// <summary>Endpoint clamped by the owning release settings.</summary>
        public static Vector3 BranchEndpoint(SkillDefinition skill, ReleaseModule release, ISkillCaster caster, Vector3 aim)
        { var origin = CastOrigin(skill,caster); var delta=aim-origin; delta.z=0; return origin+Vector3.ClampMagnitude(delta,release.Distance); }
        /// <summary>Resolves a position source using the current cast/preview context.</summary>
        public static Vector3 Resolve(PositionModule source, SkillDefinition skill, ISkillCaster caster, Vector3 aim)
        {
            if (source is SelfPosition) return caster.Position;
            var origin = CastOrigin(skill, caster);
            if (source is SkillEndpointPosition endpoint) return origin + Direction(skill, caster, aim) * endpoint.Distance;
            return new Vector3(aim.x, aim.y, origin.z);
        }
        /// <summary>Bound circle center, or the area release origin when no center is connected.</summary>
        public static Vector3 AttackPosition(SkillDefinition skill, AttackModule attack, ISkillCaster caster, Vector3 aim)
        {
            var source = skill.BindingSource(attack.ModuleId, BindingRules.Position);
            if (skill.AreaGraph)
            {
                if (!(attack is CircleAttack)) return AreaOrigin(skill, attack, caster, aim);
                if (source != null && source != SkillDefinition.CoreLayoutId) return ResolveBinding(source, skill, caster, aim);
                return source == SkillDefinition.CoreLayoutId ? AreaEndpoint(skill, attack, caster, aim) : AreaOrigin(skill, attack, caster, aim);
            }
            if (source != null) return ResolveBinding(source, skill, caster, aim);
            var origin = CastOrigin(skill, caster);
            return attack is CircleAttack ? (skill.WeaponMode && skill.ReleaseForAttack(attack) is ReleaseModule branch ? BranchEndpoint(skill,branch,caster,aim) : Endpoint(skill, caster, aim)) : origin;
        }
    }
}
