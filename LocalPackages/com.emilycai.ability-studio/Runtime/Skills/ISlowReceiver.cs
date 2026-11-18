namespace AbilityStudio.Runtime
{
    /// <summary>Optional movement slow capability for game adapters.</summary>
    public interface ISlowReceiver
    {
        /// <summary>Whether a slow is active.</summary>
        bool IsSlowed { get; }
        /// <summary>Remaining slow duration in seconds.</summary>
        float SlowRemaining { get; }
        /// <summary>Multiplier for voluntary movement, restored to one on expiry.</summary>
        float MovementSpeedMultiplier { get; }
        /// <summary>Replaces the reduction and refreshes duration without stacking.</summary>
        void ApplySlow(float reductionPercent, float duration);
    }
}
