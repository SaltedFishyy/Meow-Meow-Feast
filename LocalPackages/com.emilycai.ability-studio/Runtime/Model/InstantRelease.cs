using System;
namespace AbilityStudio.Model
{
    /// <summary>Casts immediately on button press.</summary>
    [Serializable, SkillModule("Release/Instant", DisplayName = "瞬发")]
    public sealed class InstantRelease : ReleaseModule { }
}
