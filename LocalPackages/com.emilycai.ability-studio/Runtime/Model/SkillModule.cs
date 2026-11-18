using System;
using UnityEngine;

namespace AbilityStudio.Model
{
    /// <summary>Stable identity for a reusable piece of a skill composition.</summary>
    [Serializable]
    public abstract class SkillModule
    {
        [SerializeField] private string moduleId = Guid.NewGuid().ToString("N");
        /// <summary>Persistent identity independent of its position in the composition.</summary>
        public string ModuleId => moduleId;
    }
}
