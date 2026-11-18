using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared player health tuning.</summary>
    [TuningGroup("玩家血量", 9)]
    public sealed class PlayerHealthTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("最多几格血（下次 Play 生效）"), Min(1)] private int _maxHealth = 5;

        public int MaxHealth => _maxHealth;
    }
}
