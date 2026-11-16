using MMF.Combat;
using NUnit.Framework;
using UnityEngine;

namespace MMF.Tests
{
    public sealed class MonsterAvoidanceTests
    {
        [TestCase(1f, 0f, 0f, -1f)]
        [TestCase(-1f, 0f, 0f, 1f)]
        [TestCase(0f, 1f, 1f, 0f)]
        [TestCase(0f, -1f, -1f, 0f)]
        public void ClearRightTakesPriority(float x, float y, float expectedX, float expectedY)
        {
            Assert.That(MonsterAvoidance.SelectDirection(new Vector2(x, y), true, true),
                Is.EqualTo(new Vector2(expectedX, expectedY)));
        }

        [Test]
        public void BlockedRightFallsBackToLeft()
        {
            Assert.That(MonsterAvoidance.SelectDirection(Vector2.right, false, true),
                Is.EqualTo(Vector2.up));
        }

        [TestCase(0f, -1f, 0f, 0f)]
        [TestCase(0f, 0f, 0f, -1f)]
        [TestCase(0f, 1f, 0f, -1f)]
        [TestCase(1f, 0f, 0f, -1f)]
        public void LeftFallbackWaitsOnlyForSameDirectionSidestep(float otherX, float otherY,
            float expectedX, float expectedY)
        {
            Assert.That(MonsterAvoidance.SelectDirection(Vector2.left, false, true,
                new Vector2(otherX, otherY)), Is.EqualTo(new Vector2(expectedX, expectedY)));
        }

        [Test]
        public void RightSidestepKeepsPriorityWhileOtherMonsterMovesSameWay()
        {
            Assert.That(MonsterAvoidance.SelectDirection(Vector2.right, true, true, Vector2.down),
                Is.EqualTo(Vector2.down));
        }

        [Test]
        public void LeftFallbackYieldsWhenRightSidestepStartsAfterIt()
        {
            Vector2 leftFallback = MonsterAvoidance.SelectDirection(Vector2.left, false, true);
            Vector2 rightSidestep = MonsterAvoidance.SelectDirection(Vector2.right, true, true, leftFallback);
            Assert.That(rightSidestep, Is.EqualTo(Vector2.down));
            Assert.That(MonsterAvoidance.SelectDirection(Vector2.left, false, true, rightSidestep),
                Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void BothSidesBlockedStops()
        {
            Assert.That(MonsterAvoidance.SelectDirection(Vector2.right, false, false),
                Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void DirectionHasUnitLength()
        {
            Assert.That(MonsterAvoidance.SelectDirection(new Vector2(3f, 4f), true, false).magnitude,
                Is.EqualTo(1f).Within(0.00001f));
        }

        [Test]
        public void NoMovementStops()
        {
            Assert.That(MonsterAvoidance.SelectDirection(Vector2.zero, true, true),
                Is.EqualTo(Vector2.zero));
        }
    }
}
