using Random = System.Random;
using MMF.Cooking;
using MMF.Data;
using NUnit.Framework;
using MMF.Truck;
using MMF.Player;
using UnityEditor;
using UnityEngine;
namespace MMF.Tests
{
    public sealed class FryingSequenceTests
    {
        [TestCase(1.01f, 0f)] [TestCase(-1.01f, 0f)]
        [TestCase(0f, 1.01f)] [TestCase(0f, -1.01f)]
        public void StationRangeUsesFootprintEvenWithDisabledCollider(float x, float y)
        {
            var go = new GameObject("Range test");
            try
            {
                var station = go.AddComponent<FryingStation>();
                Assert.IsTrue(station.InRange(new Vector2(x, y)));
                Assert.IsFalse(station.InRange(new Vector2(x, y) * 2f));
                station.SetCarried(true);
                Assert.IsTrue(station.InRange(new Vector2(x, y)));
                Assert.IsFalse(station.InRange(new Vector2(x, y) * 2f));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        [TestCase(-5f, 2.5f, true)] [TestCase(5f, -2.5f, true)]
        [TestCase(-5f, -2.5f, true)] [TestCase(5f, 2.5f, true)]
        [TestCase(-5.1f, 2.5f, false)] [TestCase(0f, 2.6f, false)]
        [TestCase(6f, 0f, false)] [TestCase(0f, -4f, false)]
        public void TruckFootprintAllowsWallContactButRejectsOverlapAndOutside(float x, float y, bool expected)
        {
            var go = new GameObject("Interior test");
            try { Assert.AreEqual(expected, go.AddComponent<TruckArea>().ContainsFootprint(new Vector2(x, y), Vector2.one)); }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        [Test]
        public void GridUsesTruckInteriorOriginAndFollowsTruckTranslation()
        {
            var go = new GameObject("Grid test");
            try
            {
                var truck = go.AddComponent<TruckArea>();
                Assert.AreEqual(new Vector2(-5f, 2.5f), truck.TopLeftCell(Vector2.one));
                Assert.AreEqual(new Vector2(1f, 0.5f), truck.SnapCell(new Vector2(1.1f, 0.1f), Vector2.one));
                go.transform.position = new Vector3(3.25f, 7.25f);
                Assert.AreEqual(new Vector2(-1.75f, 9.75f), truck.TopLeftCell(Vector2.one));
                Assert.AreEqual(new Vector2(4.25f, 7.75f), truck.SnapCell(new Vector2(4.35f, 7.35f), Vector2.one));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        [TestCase(0f, false, true)]
        [TestCase(0.1f, false, false)]
        [TestCase(0f, true, false)]
        public void PlacementAllowsPlayerContactButRejectsPlayerOrWallOverlap(float playerX, bool blockedByWall, bool expected)
        {
            var truckObject = new GameObject("Truck test");
            var player = new GameObject("Player test");
            var stationObject = new GameObject("Station test");
            var wall = new GameObject("Wall test");
            var tuning = ScriptableObject.CreateInstance<PlayerTuning>();
            try
            {
                var truck = truckObject.AddComponent<TruckArea>();
                player.transform.position = new Vector2(playerX, 0.5f);
                player.AddComponent<BoxCollider2D>().size = Vector2.one;
                var movement = player.AddComponent<PlayerMovement>();
                var movementData = new SerializedObject(movement);
                movementData.FindProperty("_tuning").objectReferenceValue = tuning;
                movementData.ApplyModifiedPropertiesWithoutUndo();
                var interactor = player.AddComponent<PlayerInteractor>();
                movement.SetMoveDirection(Vector2.right);
                stationObject.transform.position = new Vector2(0.8f, 0.5f);
                var station = stationObject.AddComponent<FryingStation>();
                var data = new SerializedObject(interactor);
                data.FindProperty("_station").objectReferenceValue = station;
                data.FindProperty("_truck").objectReferenceValue = truck;
                data.ApplyModifiedPropertiesWithoutUndo();
                if (blockedByWall)
                {
                    wall.transform.position = new Vector2(1.5f, 0.5f);
                    wall.AddComponent<BoxCollider2D>().size = Vector2.one;
                }
                interactor.Lift();
                Assert.IsFalse(stationObject.GetComponent<BoxCollider2D>().enabled);
                interactor.Lift();
                Assert.AreEqual(expected, stationObject.GetComponent<BoxCollider2D>().enabled);
                if (expected) Assert.AreEqual(new Vector3(1f, 0.5f, 0f), stationObject.transform.position);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stationObject);
                UnityEngine.Object.DestroyImmediate(player);
                UnityEngine.Object.DestroyImmediate(truckObject);
                UnityEngine.Object.DestroyImmediate(wall);
                UnityEngine.Object.DestroyImmediate(tuning);
            }
        }
        private static FryingSequence Create()
        {
            var sequence = new FryingSequence(4, 10f, new Random(4)); sequence.Start(); return sequence;
        }
        private static void CompleteRound(FryingSequence sequence)
        {
            for (int i = 0; i < sequence.Length; i++) sequence.Press(sequence.LetterAt(i));
        }
        [Test] public void CorrectLetterAdvances()
        {
            var sequence = Create(); sequence.Press(sequence.LetterAt(0)); Assert.AreEqual(1, sequence.Progress);
        }
        [Test] public void WrongLetterDoesNotAdvanceOrPenalizeTime()
        {
            var sequence = Create(); sequence.Press('X');
            Assert.AreEqual(0, sequence.Progress); Assert.AreEqual(10f, sequence.Remaining);
        }
        [Test] public void RequiresTwoRoundsAndResetsSecondRoundTimer()
        {
            var sequence = Create(); sequence.Tick(7f); CompleteRound(sequence);
            Assert.IsTrue(sequence.Flipping); Assert.IsFalse(sequence.Cooked);
            sequence.Tick(20f); Assert.IsFalse(sequence.Burnt);
            sequence.Press(sequence.LetterAt(0)); Assert.AreEqual(4, sequence.Progress);
            sequence.FinishFlip(); Assert.AreEqual(2, sequence.Round);
            Assert.AreEqual(0, sequence.Progress); Assert.AreEqual(10f, sequence.Remaining);
            CompleteRound(sequence); Assert.IsTrue(sequence.Cooked); Assert.IsFalse(sequence.Active);
        }
        [TestCase(false)] [TestCase(true)] public void EitherRoundCanBurn(bool secondRound)
        {
            var sequence = Create();
            if (secondRound) { CompleteRound(sequence); sequence.FinishFlip(); }
            sequence.Tick(10f); Assert.IsTrue(sequence.Burnt); Assert.IsFalse(sequence.Cooked);
            sequence.Press(sequence.LetterAt(0)); Assert.IsFalse(sequence.Active);
        }
        [Test] public void CancelClearsProgressAndStopsTimer()
        {
            var sequence = Create(); sequence.Press(sequence.LetterAt(0)); sequence.Tick(3f);
            sequence.Cancel(); sequence.Tick(20f);
            Assert.AreEqual(0, sequence.Progress); Assert.AreEqual(0f, sequence.Remaining);
            Assert.IsFalse(sequence.Burnt); Assert.IsFalse(sequence.Active);
            sequence.Start(); Assert.AreEqual(1, sequence.Round); Assert.AreEqual(10f, sequence.Remaining);
        }
        [Test] public void GeneratedLettersAlwaysBelongToWasdAndCanRepeat()
        {
            var sequence = new FryingSequence(20, 10f, new Random(1)); sequence.Start();
            for (int i = 0; i < sequence.Length; i++) Assert.That("WASD", Does.Contain(sequence.LetterAt(i).ToString()));
            // More than four positions guarantees repeated letters without a probabilistic assertion.
            Assert.AreEqual(20, sequence.Length);
        }
    }
}
