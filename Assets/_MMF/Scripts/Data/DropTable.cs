using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMF.Data
{
    /// <summary>Independent single-item drop chances.</summary>
    [TuningGroup("掉落", 7)]
    [CreateAssetMenu(menuName = "MMF/Drop Table")]
    public sealed class DropTable : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [SerializeField, TuningLabel("掉落物")] private ItemDefinition _item = null;
            [SerializeField, TuningLabel("几率"), Range(0f, 1f)] private float _chance = 0f;
            public ItemDefinition Item => _item;
            public float Chance => _chance;
        }

        [SerializeField, TuningLabel("掉落物和几率（0～1，每样单独算）")] private List<Entry> _entries = new List<Entry>();
        [SerializeField, TuningLabel("散开半径"), Min(0f)] private float _scatterRadius = 0.5f;

        /// <summary>Maximum distance from the monster at which items spawn.</summary>
        public float ScatterRadius => _scatterRadius;

        /// <summary>Appends successful drops, drawing once for every entry.</summary>
        public void Roll(Func<float> random01, List<ItemDefinition> results)
        {
            foreach (Entry entry in _entries)
                if (random01() < entry.Chance && entry.Item != null)
                    results.Add(entry.Item);
        }
    }
}
