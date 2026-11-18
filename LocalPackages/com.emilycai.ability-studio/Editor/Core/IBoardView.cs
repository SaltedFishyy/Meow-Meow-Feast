using System;
using AbilityStudio.Model;
using UnityEngine.UIElements;

namespace AbilityStudio.Editor
{
    /// <summary>Framework-independent composition canvas boundary.</summary>
    public interface IBoardView : IDisposable
    {
        /// <summary>UI element hosted by the editor window.</summary>
        VisualElement Element { get; }
        /// <summary>Displays the shared asset; null unbinds the canvas.</summary>
        void Bind(SkillDefinition skill);
        /// <summary>Fits all modules in the visible canvas.</summary>
        void FrameAllModules();
        /// <summary>Removes selected attack/effect nodes as one undoable operation.</summary>
        void DeleteSelectedModules();
    }
}
