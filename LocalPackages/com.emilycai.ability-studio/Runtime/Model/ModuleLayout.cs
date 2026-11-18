using System;
using UnityEngine;

namespace AbilityStudio.Model
{
    /// <summary>Editor presentation only; never used by the skill runtime.</summary>
    [Serializable]
    public sealed class ModuleLayout
    {
        [SerializeField] private string moduleId;
        [SerializeField] private Vector2 position;
        /// <summary>Creates a position associated with a stable module identity.</summary>
        public ModuleLayout(string id, Vector2 value) { moduleId = id; position = value; }
        /// <summary>Module identity, or SkillDefinition.CoreLayoutId for the core.</summary>
        public string ModuleId => moduleId;
        /// <summary>Canvas coordinates.</summary>
        public Vector2 Position { get => position; set => position = value; }
    }
}
