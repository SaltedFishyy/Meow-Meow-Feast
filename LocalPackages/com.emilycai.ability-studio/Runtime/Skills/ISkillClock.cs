namespace AbilityStudio.Runtime
{
    /// <summary>Injectable clock for deterministic cooldown tests.</summary>
    public interface ISkillClock
    {
        /// <summary>Monotonic time in seconds.</summary>
        double Time { get; }
    }
}
