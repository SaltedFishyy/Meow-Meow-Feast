using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Typed data binding identified by persistent module IDs, not board coordinates.</summary>
    [Serializable]
    public sealed class ModuleBinding
    {
        [SerializeField] private string sourceId;
        [SerializeField] private string targetId;
        [SerializeField] private string input;
        [SerializeField] private string sourcePort;
        /// <summary>Output channel; empty preserves the original default output.</summary>
        public string SourcePort => sourcePort ?? "";
        /// <summary>Creates a data binding.</summary>
        public ModuleBinding(string source, string target, string inputName, string output = "")
        { sourceId = source; targetId = target; input = inputName; sourcePort = output; }
        /// <summary>Source module ID.</summary>
        public string SourceId => sourceId;
        /// <summary>Receiving module ID.</summary>
        public string TargetId => targetId;
        /// <summary>Receiving input name.</summary>
        public string Input => input;
    }
}
