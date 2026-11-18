using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AbilityStudio.Model;
using UnityEditor;

namespace AbilityStudio.Editor
{
    /// <summary>Discovers designer modules from their declared catalog metadata.</summary>
    public static class ModuleCatalog
    {
        /// <summary>Concrete, constructible module types in stable category order.</summary>
        public static IReadOnlyList<Type> Types => TypeCache.GetTypesWithAttribute<SkillModuleAttribute>()
            .Where(t => t != typeof(SkillEndpointPosition) && t != typeof(SelfPosition) && t != typeof(MousePosition) && !t.IsAbstract && !t.ContainsGenericParameters &&
                (typeof(AttackModule).IsAssignableFrom(t) || typeof(EffectModule).IsAssignableFrom(t) || typeof(PositionModule).IsAssignableFrom(t) || typeof(HitResponseModule).IsAssignableFrom(t) || typeof(ReleaseModule).IsAssignableFrom(t)) &&
                t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => Metadata(t).Path, StringComparer.Ordinal).ToArray();
        /// <summary>Returns the declared library category and display name.</summary>
        public static SkillModuleAttribute Metadata(Type type) => type.GetCustomAttribute<SkillModuleAttribute>();
        /// <summary>Whether the asset and constructible module type are available.</summary>
        public static bool CanAdd(SkillDefinition skill, Type type) => skill != null && Types.Contains(type) && !(skill.IntegratedAttacks && typeof(ReleaseModule).IsAssignableFrom(type));
        /// <summary>Attack instances and their serialized paths, including the legacy primary slot.</summary>
        public static IEnumerable<(AttackModule module, string path)> AttackEntries(SkillDefinition skill)
        {
            if (skill.Attack != null) yield return (skill.Attack, "attack");
            for (int i = 0; i < skill.AdditionalAttacks.Count; i++)
                yield return (skill.AdditionalAttacks[i], "additionalAttacks.Array.data[" + i + "]");
        }
    }
}

