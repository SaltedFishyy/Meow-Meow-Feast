using AbilityStudio.Model;
using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Shared charge-scaled geometry for targeting and live previews.</summary>
    public static class SkillChargeGeometry
    {
        /// <summary>Unconnected rectangles keep their original fixed length.</summary>
        public static float Length(SkillDefinition skill, LineThrustAttack rectangle, float chargeRatio)
        {
            if (skill.IntegratedAttacks) return rectangle.IsCharged ? Mathf.Lerp(rectangle.MinLength,rectangle.Length,Mathf.Clamp01(chargeRatio)) : rectangle.Length;
            var source = skill.BindingSource(rectangle.ModuleId, BindingRules.Charge);
            if (source == null) return rectangle.Length;
            float ratio = (skill.WeaponMode ? skill.ReleaseForAttack(rectangle)?.ModuleId : skill.ActiveRelease?.ModuleId) == source ? Mathf.Clamp01(chargeRatio) : 0;
            return Mathf.Lerp(rectangle.MinLength, rectangle.Length, ratio);
        }
        /// <summary>Unconnected circles retain their fixed radius.</summary>
        public static float Radius(SkillDefinition skill, CircleAttack circle, float chargeRatio) => Scale(skill, circle, circle.MinRadius, circle.Radius, chargeRatio);
        /// <summary>Unconnected cones retain their fixed reach and angle.</summary>
        public static float Range(SkillDefinition skill, ConeAttack cone, float chargeRatio) => Scale(skill, cone, cone.MinRange, cone.Range, chargeRatio);
        private static float Scale(SkillDefinition skill, AttackModule attack, float min, float max, float chargeRatio)
        {
            if (skill.IntegratedAttacks) return attack.IsCharged ? Mathf.Lerp(min,max,Mathf.Clamp01(chargeRatio)) : max;
            var source = skill.BindingSource(attack.ModuleId, BindingRules.Charge);
            if (source == null) return max;
            return Mathf.Lerp(min, max, (skill.WeaponMode ? skill.ReleaseForAttack(attack)?.ModuleId : skill.ActiveRelease?.ModuleId) == source ? Mathf.Clamp01(chargeRatio) : 0);
        }
        /// <summary>Front-zone origin using actual length rather than maximum configured length.</summary>
        public static Vector3 TipOrigin(Vector3 origin, Vector3 direction, float length, float tipLength) =>
            origin + direction * Mathf.Max(0, length - tipLength);
        /// <summary>Targets overlapping the front zone belong exclusively to the front subset.</summary>
        public static bool IsTip(Vector3 origin, Vector3 direction, float length, float width, float tipLength, ISkillTarget target) =>
            RectangleGeometry.Intersects(TipOrigin(origin, direction, length, tipLength), direction, Mathf.Min(length, tipLength), width, target.Position, target.Radius);
    }
}
