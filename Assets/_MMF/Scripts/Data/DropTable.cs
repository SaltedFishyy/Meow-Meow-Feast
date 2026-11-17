using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMF.Data
{
    /// <summary>Independent single-item drop chances.</summary>
    [CreateAssetMenu(menuName = "MMF/Drop Table")]
    public sealed class DropTable : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private ItemDefinition _item = null;
            [SerializeField, Range(0f, 1f)] private float _chance = 0f;
            public ItemDefinition Item => _item;
            public float Chance => _chance;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();
        [SerializeField, Min(0f)] private float _scatterRadius = 0.5f;

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
