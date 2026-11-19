using AbilityStudio.Model;
using UnityEngine;
namespace MMF.Data
{
    /// <summary>Persisted main-scene weapon selection; null restores the backpack weapon.</summary>
    public sealed class WorkshopLoadout : ScriptableObject
    {
        public const string ResourceName = "MMFWorkshopLoadout";
        [SerializeField] private SkillDefinition _playerOne;
        public SkillDefinition PlayerOne => _playerOne;
        /// <summary>Assigns the single current player slot.</summary>
        public void EquipPlayerOne(SkillDefinition weapon) => _playerOne = weapon;
    }
}
