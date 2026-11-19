using System;
namespace MMF.Player
{
    /// <summary>Engine-independent inventory using stable item IDs.</summary>
    public sealed class BackpackModel
    {
        private readonly string[] _items;
        private readonly int[] _counts;
        public int SlotCount => _items.Length;
        public int StackLimit { get; }
        public int SelectedSlot { get; private set; }
        public event Action ContentsChanged;
        public event Action SelectionChanged;
        public BackpackModel(int slotCount, int stackLimit)
        {
            if (slotCount < 1 || stackLimit < 1) throw new ArgumentOutOfRangeException();
            _items = new string[slotCount]; _counts = new int[slotCount]; StackLimit = stackLimit;
        }
        public string ItemAt(int slot) => _items[slot];
        public int CountAt(int slot) => _counts[slot];
        /// <summary>Adds one item, preferring partial matching stacks.</summary>
        public bool TryAdd(string item)
        {
            int slot = FindAddSlot(item);
            return slot >= 0 && AddAt(slot, item);
        }
        /// <summary>Checks whether one item fits without changing inventory.</summary>
        public bool CanAdd(string item) => FindAddSlot(item) >= 0;
        private int FindAddSlot(string item)
        {
            if (string.IsNullOrEmpty(item)) return -1;
            for (int i = 0; i < SlotCount; i++)
                if (_items[i] == item && _counts[i] < StackLimit) return i;
            for (int i = 0; i < SlotCount; i++)
                if (_counts[i] == 0) return i;
            return -1;
        }
        private bool AddAt(int slot, string item)
        {
            _items[slot] = item; _counts[slot]++; ContentsChanged?.Invoke(); return true;
        }
        /// <summary>Removes exactly one selected item.</summary>
        public bool TryDrop(out string item) => TryTakeOne(SelectedSlot, out item);
        /// <summary>Removes one item from a slot without changing selection.</summary>
        public bool TryTakeOne(int slot, out string item)
        {
            item = null;
            if (slot < 0 || slot >= SlotCount || _counts[slot] == 0) return false;
            item = _items[slot];
            if (--_counts[slot] == 0) _items[slot] = null;
            ContentsChanged?.Invoke(); return true;
        }
        /// <summary>Selects a clamped zero-based slot without wrapping.</summary>
        public void Select(int slot)
        {
            int next = Math.Max(0, Math.Min(SlotCount - 1, slot));
            if (next == SelectedSlot) return;
            SelectedSlot = next; SelectionChanged?.Invoke();
        }
    }
}
