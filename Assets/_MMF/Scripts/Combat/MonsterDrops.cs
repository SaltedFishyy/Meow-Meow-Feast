using System;
using System.Collections.Generic;
using MMF.Data;
using MMF.Player;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Spawns ground items when a monster dies, including after respawning.</summary>
    [RequireComponent(typeof(Monster))]
    public sealed class MonsterDrops : MonoBehaviour
    {
        [SerializeField] private DropTable _dropTable = null;
        [SerializeField] private Sprite _sprite = null;
        [SerializeField] private BackpackTuning _tuning = null;
        [SerializeField, Min(0.01f)] private float _size = 0.4f;
        private readonly List<ItemDefinition> _results = new List<ItemDefinition>();
        private static readonly Func<float> Random01 = () => UnityEngine.Random.value;
        private Monster _monster;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
            // Died fires after SetActive(false), so stay subscribed while disabled.
            _monster.Died += Drop;
        }

        private void OnDestroy()
        {
            if (_monster != null) _monster.Died -= Drop;
        }

        private void Drop(Monster monster)
        {
            if (_dropTable == null) return;
            _results.Clear();
            _dropTable.Roll(Random01, _results);
            foreach (ItemDefinition item in _results)
            {
                GameObject dropped = new GameObject(item.name);
                dropped.transform.position = monster.transform.position +
                    (Vector3)(UnityEngine.Random.insideUnitCircle * _dropTable.ScatterRadius);
                SpriteRenderer renderer = dropped.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 1;
                WorldItem world = dropped.AddComponent<WorldItem>();
                world.Initialize(item, _sprite, 0f, _tuning.WorldLifetime, _size);
            }
        }
    }
}
