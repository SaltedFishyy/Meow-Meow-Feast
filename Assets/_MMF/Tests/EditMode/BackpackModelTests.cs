using MMF.Player;
using NUnit.Framework;
namespace MMF.Tests
{
    public sealed class BackpackModelTests
    {
        [Test]
        public void PartialMatchingStackWinsOverEarlierEmptySlot()
        {
            var model = new BackpackModel(5, 3);
            model.TryAdd("meat"); model.TryAdd("mushroom");
            model.TryDrop(out _);
            Assert.That(model.TryAdd("mushroom"), Is.True);
            Assert.That(model.CountAt(0), Is.Zero);
            Assert.That(model.CountAt(1), Is.EqualTo(2));
        }
        [Test]
        public void FullStackUsesFirstEmptySlotAndFullBackpackRejectsItem()
        {
            var model = new BackpackModel(2, 3);
            for (int i = 0; i < 6; i++) Assert.That(model.TryAdd("meat"), Is.True);
            Assert.That(model.CountAt(0), Is.EqualTo(3));
            Assert.That(model.CountAt(1), Is.EqualTo(3));
            Assert.That(model.TryAdd("meat"), Is.False);
            Assert.That(model.TryAdd("mushroom"), Is.False);
        }
        [Test]
        public void DropRemovesOneAndClearsLastItem()
        {
            var model = new BackpackModel(5, 3);
            model.TryAdd("meat"); model.TryAdd("meat");
            Assert.That(model.TryDrop(out string item), Is.True);
            Assert.That(item, Is.EqualTo("meat"));
            Assert.That(model.CountAt(0), Is.EqualTo(1));
            model.TryDrop(out _);
            Assert.That(model.ItemAt(0), Is.Null);
            Assert.That(model.TryDrop(out _), Is.False);
        }
        [Test]
        public void SelectionStartsAtZeroAndNeverWraps()
        {
            var model = new BackpackModel(5, 3);
            Assert.That(model.SelectedSlot, Is.Zero);
            model.Select(-1); Assert.That(model.SelectedSlot, Is.Zero);
            model.Select(4); model.Select(model.SelectedSlot + 1);
            Assert.That(model.SelectedSlot, Is.EqualTo(4));
        }
    }
}
