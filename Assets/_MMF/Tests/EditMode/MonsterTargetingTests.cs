using MMF.Combat;
using NUnit.Framework;
using UnityEngine;

namespace MMF.Tests
{
    public sealed class MonsterTargetingTests
    {
        [TestCase(13f, 2f, -6.5f, 0f, 6.5f, 4f)]
        [TestCase(13f, -2f, -6.5f, 0f, 6.5f, -4f)]
        [TestCase(-12f, 5f, 6.5f, 0f, 6.5f, 4f)]
        [TestCase(0f, 10f, 6.5f, 0f, 6.5f, 4f)]
        [TestCase(6.5f, 4f, -6.5f, 0f, -6.5f, 4f)]
        [TestCase(13f, 2f, -12f, -10f, 6.5f, -4f)]
        public void BlockedRouteUsesCornerOnShortestRoute(float x, float y, float targetX,
            float targetY, float cornerX, float cornerY)
        {
            Assert.That(MonsterTargeting.NextWaypoint(new Vector2(x, y),
                new Vector2(targetX, targetY), new Rect(-6f, -3.5f, 12f, 7f), 0.4f),
                Is.EqualTo(new Vector2(cornerX, cornerY)));
        }

        [TestCase(-0.4f, -3.9f, 6.5f, 0f, 6.5f, -4f)]
        [TestCase(0.4f, -3.9f, -6.5f, 0f, -6.5f, -4f)]
        [TestCase(-0.4f, 3.9f, 6.5f, 0f, 6.5f, 4f)]
        [TestCase(0.4f, 3.9f, -6.5f, 0f, -6.5f, 4f)]
        [TestCase(-6.4f, 0f, 6.5f, 0f, -6.5f, -4f)]
        [TestCase(6.4f, 0f, -6.5f, 0f, 6.5f, 4f)]
        public void ClearanceBandStartUsesNearestEdgeCorner(float x, float y, float targetX,
            float targetY, float cornerX, float cornerY)
        {
            Assert.That(MonsterTargeting.NextWaypoint(new Vector2(x, y),
                new Vector2(targetX, targetY), new Rect(-6f, -3.5f, 12f, 7f), 0.4f),
                Is.EqualTo(new Vector2(cornerX, cornerY)));
        }

        [TestCase(13f, 2f, 6.5f, 0f)]
        [TestCase(6.5f, 4f, 6.5f, 0f)]
        [TestCase(-6.5f, 4f, -6.5f, 0f)]
        [TestCase(0f, 10f, 10f, 10f)]
        [TestCase(6.5f, 0f, 6.5f, 0f)]
        [TestCase(6.4f, 0f, 6.5f, 0f)]
        [TestCase(0f, -3.9f, 0f, -6f)]
        public void ClearRouteGoesDirectlyToDestination(float x, float y, float targetX, float targetY)
        {
            Vector2 destination = new Vector2(targetX, targetY);
            Assert.That(MonsterTargeting.NextWaypoint(new Vector2(x, y), destination,
                new Rect(-6f, -3.5f, 12f, 7f), 0.4f), Is.EqualTo(destination));
        }

        [TestCase(13f, 2f, -6.5f)]
        [TestCase(-12f, 5f, 6.5f)]
        [TestCase(0f, 10f, 6.5f)]
        public void RepeatedWaypointsReachAssignedEndWithinTwentySeconds(float x, float y, float targetX)
        {
            Vector2 position = new Vector2(x, y);
            Vector2 destination = new Vector2(targetX, 0f);
            Rect truck = new Rect(-6f, -3.5f, 12f, 7f);
            for (int frame = 0; frame < 1000; frame++)
            {
                Vector2 next = MonsterTargeting.NextWaypoint(position, destination, truck, 0.4f);
                position = Vector2.MoveTowards(position, next, 1.5f * 0.02f);
                Assert.That(Mathf.Abs(position.x) >= 6.5f - 0.0001f ||
                    Mathf.Abs(position.y) >= 4f - 0.0001f, Is.True, "Route cuts through padded truck");
            }
            Assert.That(Vector2.Distance(position, destination), Is.LessThan(0.001f));
        }

        [TestCase(-0.4f, 6.5f)]
        [TestCase(0.4f, -6.5f)]
        public void ClearanceBandRouteReachesEndWithinFifteenSeconds(float x, float targetX)
        {
            Vector2 position = new Vector2(x, -3.9f);
            Vector2 destination = new Vector2(targetX, 0f);
            Rect truck = new Rect(-6f, -3.5f, 12f, 7f);
            for (int frame = 0; frame < 750; frame++)
            {
                Vector2 next = MonsterTargeting.NextWaypoint(position, destination, truck, 0.4f);
                position = Vector2.MoveTowards(position, next, 1.5f * 0.02f);
                Assert.That(Mathf.Abs(position.x) >= 6.4f - 0.0001f ||
                    Mathf.Abs(position.y) >= 3.9f - 0.0001f, Is.True, "Route cuts through truck wall");
            }
            Assert.That(Vector2.Distance(position, destination), Is.LessThan(0.001f));
        }

        [TestCase(false, 7.99f, false, true)]
        [TestCase(false, 8f, false, true)]
        [TestCase(false, 8.01f, false, false)]
        [TestCase(false, 9f, false, false)]
        [TestCase(false, 9f, true, true)]
        [TestCase(false, 10f, true, true)]
        [TestCase(false, 10.01f, true, false)]
        [TestCase(true, 0f, false, false)]
        [TestCase(true, 0f, true, false)]
        [TestCase(true, 11f, true, false)]
        public void ChaseDecisionUsesInteriorAndHysteresis(bool inside, float distance,
            bool chasing, bool expected)
        {
            Assert.That(MonsterTargeting.ShouldChase(inside, distance, chasing, 8f, 10f),
                Is.EqualTo(expected));
        }

        [Test]
        public void ChaseDistancesAreConfigurable()
        {
            Assert.That(MonsterTargeting.ShouldChase(false, 3f, false, 2f, 4f), Is.False);
            Assert.That(MonsterTargeting.ShouldChase(false, 3f, true, 2f, 4f), Is.True);
        }
    }
}
