namespace AbilityStudio.Runtime
{
    /// <summary>Optional health restoration capability.</summary>
    public interface IHealable
    {
        /// <summary>Maximum hit points used for percentage healing.</summary>
        float MaxHp { get; }
        /// <summary>Restores health up to maximum HP; does not revive.</summary>
        void ApplyHealing(float amount, ISkillCaster source);
    }
}
