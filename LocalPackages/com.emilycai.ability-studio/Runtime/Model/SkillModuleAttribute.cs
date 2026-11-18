using System;
namespace AbilityStudio.Model
{
    /// <summary>Designer-facing catalog metadata.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SkillModuleAttribute : Attribute
    {
        /// <summary>Creates catalog metadata.</summary>
        public SkillModuleAttribute(string path) { Path = path; }
        /// <summary>Catalog category and name.</summary>
        public string Path { get; }
        /// <summary>Readable module name.</summary>
        public string DisplayName { get; set; }
    }
}
