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
            if (string.IsNullOrEmpty(item)) return false;
            for (int i = 0; i < SlotCount; i++)
                if (_items[i] == item && _counts[i] < StackLimit) return AddAt(i, item);
            for (int i = 0; i < SlotCount; i++)
                if (_counts[i] == 0) return AddAt(i, item);
            return false;
        }
        private bool AddAt(int slot, string item)
        {
            _items[slot] = item; _counts[slot]++; ContentsChanged?.Invoke(); return true;
        }
        /// <summary>Removes exactly one selected item.</summary>
        public bool TryDrop(out string item)
        {
            item = _items[SelectedSlot];
            if (_counts[SelectedSlot] == 0) return false;
            if (--_counts[SelectedSlot] == 0) _items[SelectedSlot] = null;
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
