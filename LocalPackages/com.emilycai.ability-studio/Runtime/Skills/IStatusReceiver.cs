namespace AbilityStudio.Runtime
{
    /// <summary>Optional timed-status capability; movement controllers should suppress voluntary actions while stunned.</summary>
    public interface IStatusReceiver
    {
        /// <summary>Whether voluntary actions are currently blocked by stun.</summary>
        bool IsStunned { get; }
        /// <summary>Remaining stun time in seconds.</summary>
        float StunRemaining { get; }
        /// <summary>Refreshes stun to this duration, without adding to the previous duration.</summary>
        void ApplyStun(float duration);
    }
}
