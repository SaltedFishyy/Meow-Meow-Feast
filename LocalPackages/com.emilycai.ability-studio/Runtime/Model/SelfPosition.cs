using System;
namespace AbilityStudio.Model
{
    /// <summary>The caster's position when the skill is cast.</summary>
    [Serializable, SkillModule("Positions/Self", DisplayName = "自身")]
    public sealed class SelfPosition : PositionModule { }
}
