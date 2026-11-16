using System.Collections.Generic;
using MMF.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MMF.Tests
{
    public sealed class DropTableTests
    {
        private DropTable _table;
        private ItemDefinition[] _items;
        private readonly List<ItemDefinition> _results = new List<ItemDefinition>();

        [SetUp]
        public void SetUp()
        {
            _table = ScriptableObject.CreateInstance<DropTable>();
            _items = new[] { ScriptableObject.CreateInstance<ItemDefinition>(),
                ScriptableObject.CreateInstance<ItemDefinition>(), ScriptableObject.CreateInstance<ItemDefinition>() };
            SerializedObject data = new SerializedObject(_table);
            SerializedProperty entries = data.FindProperty("_entries");
            entries.arraySize = _items.Length;
            float[] chances = { 0.6f, 0.3f, 0.3f };
            for (int i = 0; i < _items.Length; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_item").objectReferenceValue = _items[i];
                entry.FindPropertyRelative("_chance").floatValue = chances[i];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            _results.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_table);
            foreach (ItemDefinition item in _items) Object.DestroyImmediate(item);
        }

        [Test]
        public void ZeroDropsEveryItem()
        {
            _table.Roll(() => 0f, _results);
            CollectionAssert.AreEqual(_items, _results);
        }

        [Test]
        public void HighRollDropsNothing()
        {
            _table.Roll(() => 0.99f, _results);
            Assert.That(_results, Is.Empty);
        }

        [Test]
        public void EntriesRollIndependently()
        {
            float[] rolls = { 0.59f, 0.31f, 0.29f };
            int index = 0;
            _table.Roll(() => rolls[index++], _results);
            CollectionAssert.AreEqual(new[] { _items[0], _items[2] }, _results);
        }

        [Test]
        public void EqualChanceDoesNotDrop()
        {
            _table.Roll(() => 0.6f, _results);
            Assert.That(_results, Is.Empty);
        }

        [TestCase(0f)]
        [TestCase(0.99f)]
        public void DrawsExactlyOncePerEntry(float roll)
        {
            int calls = 0;
            _table.Roll(() => { calls++; return roll; }, _results);
            Assert.That(calls, Is.EqualTo(_items.Length));
        }
    }
}
