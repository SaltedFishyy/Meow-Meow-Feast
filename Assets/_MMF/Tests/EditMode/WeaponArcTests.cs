using MMF.Combat;
using NUnit.Framework;
using UnityEngine;

namespace MMF.Tests
{
    public sealed class WeaponArcTests
    {
        [TestCase(1f, 0f, true)]
        [TestCase(1.5f, 0f, true)]
        [TestCase(1.6f, 0f, false)]
        [TestCase(1f, 1f, true)]
        [TestCase(1f, -1f, true)]
        [TestCase(0.6427876f, 0.7660444f, false)]
        [TestCase(-1f, 0f, false)]
        [TestCase(0f, 0f, true)]
        public void ContainsUsesInclusiveRangeAndHalfAngle(float x, float y, bool expected)
        {
            Assert.That(WeaponArc.Contains(Vector2.zero, Vector2.right,
                new Vector2(x, y), 1.5f, 90f), Is.EqualTo(expected));
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, 0f)]
        public void ZeroAimNeverContainsTarget(float x, float y)
        {
            Assert.That(WeaponArc.Contains(Vector2.zero, Vector2.zero,
                new Vector2(x, y), 1.5f, 90f), Is.False);
        }

        [Test]
        public void SupportsTranslatedOriginAndNonUnitAim()
        {
            Assert.That(WeaponArc.Contains(new Vector2(3f, 4f), Vector2.up * 5f,
                new Vector2(3f, 5f), 1.5f, 90f), Is.True);
        }
    }
}
