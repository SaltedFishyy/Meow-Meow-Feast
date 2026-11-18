using System;
namespace AbilityStudio.Model
{
    /// <summary>The ground-plane aim position when the skill is cast.</summary>
    [Serializable, SkillModule("Positions/Mouse", DisplayName = "鼠标位置")]
    public sealed class MousePosition : PositionModule { }
}
