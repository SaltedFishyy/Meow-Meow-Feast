namespace AbilityStudio.Runtime
{
    /// <summary>Optional damage-over-time capability, independent of stun support.</summary>
    public interface IBleedReceiver
    {
        /// <summary>Whether bleeding is active.</summary>
        bool IsBleeding { get; }
        /// <summary>Remaining duration in seconds.</summary>
        float BleedRemaining { get; }
        /// <summary>Refreshes duration and rate without stacking or delaying the next tick.</summary>
        void ApplyBleed(float damagePerSecond, float duration, ISkillCaster source);
    }
}
