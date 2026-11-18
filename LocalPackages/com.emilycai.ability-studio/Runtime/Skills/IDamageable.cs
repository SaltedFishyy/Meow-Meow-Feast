namespace AbilityStudio.Runtime
{
    /// <summary>Optional target health capability.</summary>
    public interface IDamageable
    {
        /// <summary>Current hit points.</summary>
        float CurrentHp { get; }
        /// <summary>Maximum hit points.</summary>
        float MaxHp { get; }
        /// <summary>Non-negative armor reduces damage.</summary>
        float Armor { get; }
        /// <summary>Applies resolved damage.</summary>
        void ApplyDamage(float amount, ISkillCaster source);
    }
}
