using System;
namespace AbilityStudio.Model
{
    /// <summary>Defines how a skill reaches targets.</summary>
    [Serializable] public abstract class AttackModule : SkillModule
    {
        [UnityEngine.SerializeField] private int releaseMode;
        [UnityEngine.SerializeReference] private ChargeRelease settings = new ChargeRelease();
        [NonSerialized] private InstantRelease instantSettings;
        /// <summary>Zero is instant, one is held charge. Both retain the same authored settings.</summary>
        public int ReleaseMode { get => releaseMode; set => releaseMode = value; }
        /// <summary>Authored damage, cooldown, endpoint distance and charge time.</summary>
        public ChargeRelease Settings => settings;
        /// <summary>Whether this integrated attack charges on hold.</summary>
        public bool IsCharged => releaseMode == 1;
        /// <summary>Adapter to the shared release lifecycle without a separate board node.</summary>
        public ReleaseModule ReleaseSettings
        {
            get
            {
                if (IsCharged) return settings;
                if (instantSettings == null) instantSettings = new InstantRelease();
                instantSettings.BaseDamage=settings.BaseDamage; instantSettings.Cooldown=settings.Cooldown; instantSettings.Distance=settings.Distance;
                return instantSettings;
            }
        }
        [UnityEngine.SerializeField] private DamageMode damageMode;
        /// <summary>Defaults to legacy automatic base damage.</summary>
        public DamageMode DamageMode { get => damageMode; set => damageMode = value; }
    }
}
