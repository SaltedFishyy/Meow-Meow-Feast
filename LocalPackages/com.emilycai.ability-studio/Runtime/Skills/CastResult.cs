namespace AbilityStudio.Runtime
{
    /// <summary>Outcome of a cast request.</summary>
    public enum CastResult
    {
        /// <summary>The skill was cast.</summary>
        Success,
        /// <summary>Cooldown has not elapsed.</summary>
        OnCooldown,
        /// <summary>Data or aim is invalid.</summary>
        Invalid,
        /// <summary>The caster is stunned.</summary>
        Stunned,
        /// <summary>A held release has started; no attack or cooldown yet.</summary>
        Charging,
        /// <summary>Another attack is currently being charged.</summary>
        Busy
    }
}


