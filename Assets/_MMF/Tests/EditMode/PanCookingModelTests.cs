using MMF.Cooking;
using MMF.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using State = MMF.Cooking.PanCookingModel.CookingState;

namespace MMF.Tests.EditMode
{
    public sealed class PanCookingModelTests
    {
        private CookingTuning _tuning;
        private ItemDefinition _raw;
        private ItemDefinition _cooked;
        private ItemDefinition _burnt;
        private PanCookingModel _pan;

        [SetUp]
        public void SetUp()
        {
            _tuning = ScriptableObject.CreateInstance<CookingTuning>();
            _raw = ScriptableObject.CreateInstance<ItemDefinition>();
            _cooked = ScriptableObject.CreateInstance<ItemDefinition>();
            _burnt = ScriptableObject.CreateInstance<ItemDefinition>();
            using var rawData = new SerializedObject(_raw);
            rawData.FindProperty("_fryingResult").objectReferenceValue = _cooked;
            rawData.ApplyModifiedPropertiesWithoutUndo();
            _pan = new PanCookingModel(_tuning, _burnt);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_raw);
            Object.DestroyImmediate(_cooked);
            Object.DestroyImmediate(_burnt);
            Object.DestroyImmediate(_tuning);
        }

        [Test]
        public void PlacementRequiresAnEmptyPanAndFryableItem()
        {
            Assert.That(_pan.TryPlace(null), Is.False);
            Assert.That(_pan.TryPlace(_cooked), Is.False);
            Assert.That(_pan.TryPlace(_burnt), Is.False);
            Assert.That(_pan.TryPlace(_raw), Is.True);
            Assert.That(_pan.TryPlace(_raw), Is.False);
            Assert.That(_pan.Item, Is.SameAs(_raw));
        }

        [Test]
        public void FirstFlipRequiresThreeSecondsAndStartsSecondSide()
        {
            int changes = 0;
            _pan.StateChanged += () => changes++;
            _pan.TryPlace(_raw);
            _pan.Tick(2.5f);
            Assert.That(_pan.TryFlip(out var result), Is.False);
            Assert.That(result, Is.Null);
            Assert.That(_pan.State, Is.EqualTo(State.CookingFirst));
            Assert.That(_pan.SideElapsed, Is.EqualTo(2.5f));
            Assert.That(changes, Is.EqualTo(1));

            _pan.Tick(0.5f);
            Assert.That(_pan.State, Is.EqualTo(State.ReadyFirst));
            Assert.That(_pan.TryFlip(out result), Is.True);
            Assert.That(result, Is.Null);
            Assert.That(_pan.State, Is.EqualTo(State.CookingSecond));
            Assert.That(_pan.SideElapsed, Is.Zero);
            Assert.That(changes, Is.EqualTo(3));
        }

        [Test]
        public void FinalFlipReturnsFryingResultAndEmptiesPan()
        {
            _pan.TryPlace(_raw);
            _pan.Tick(3f);
            _pan.TryFlip(out _);
            _pan.Tick(3f);
            Assert.That(_pan.State, Is.EqualTo(State.ReadySecond));
            Assert.That(_pan.TryFlip(out var result), Is.True);
            Assert.That(result, Is.SameAs(_cooked));
            AssertEmpty();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EitherSideBurnsAfterFiveReadySeconds(bool secondSide)
        {
            _pan.TryPlace(_raw);
            if (secondSide)
            {
                _pan.Tick(3f);
                _pan.TryFlip(out _);
            }

            _pan.Tick(7.5f);
            Assert.That(_pan.State, Is.EqualTo(secondSide ? State.ReadySecond : State.ReadyFirst));
            Assert.That(_pan.ReadyElapsed, Is.EqualTo(4.5f));
            _pan.Tick(0.5f);
            Assert.That(_pan.State, Is.EqualTo(State.Burnt));
            Assert.That(_pan.TryFlip(out _), Is.False);
            Assert.That(_pan.TryTakeBack(out var result), Is.True);
            Assert.That(result, Is.SameAs(_burnt));
            AssertEmpty();
        }

        [TestCase(false, 0f)]
        [TestCase(false, 1f)]
        [TestCase(false, 4f)]
        [TestCase(true, 1f)]
        [TestCase(true, 4f)]
        public void TakingRawContentsClearsThePan(bool secondSide, float elapsed)
        {
            _pan.TryPlace(_raw);
            if (secondSide)
            {
                _pan.Tick(3f);
                _pan.TryFlip(out _);
            }

            _pan.Tick(elapsed);
            Assert.That(_pan.TryTakeBack(out var result), Is.True);
            Assert.That(result, Is.SameAs(_raw));
            AssertEmpty();
            Assert.That(_pan.TryTakeBack(out _), Is.False);
        }

        [Test]
        public void CookedItemCanOnlyBeTakenBack()
        {
            Assert.That(_pan.PutBackCooked(_cooked), Is.True);
            Assert.That(_pan.PutBackCooked(_cooked), Is.False);
            _pan.Tick(30f);
            Assert.That(_pan.State, Is.EqualTo(State.Holding));
            Assert.That(_pan.TryFlip(out _), Is.False);
            Assert.That(_pan.TryPlace(_raw), Is.False);
            Assert.That(_pan.TryTakeBack(out var result), Is.True);
            Assert.That(result, Is.SameAs(_cooked));
            AssertEmpty();
        }

        [Test]
        public void TuningChangesApplyWhenTheNextSideStarts()
        {
            _pan.TryPlace(_raw);
            using var tuningData = new SerializedObject(_tuning);
            tuningData.FindProperty("_sideSeconds").floatValue = 4f;
            tuningData.FindProperty("_burnSeconds").floatValue = 6f;
            tuningData.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(_pan.SideSeconds, Is.EqualTo(3f));
            Assert.That(_pan.BurnSeconds, Is.EqualTo(5f));

            _pan.Tick(3f);
            Assert.That(_pan.TryFlip(out _), Is.True);
            Assert.That(_pan.SideSeconds, Is.EqualTo(4f));
            Assert.That(_pan.BurnSeconds, Is.EqualTo(6f));
        }

        private void AssertEmpty()
        {
            Assert.That(_pan.State, Is.EqualTo(State.Empty));
            Assert.That(_pan.Item, Is.Null);
            Assert.That(_pan.SideElapsed, Is.Zero);
            Assert.That(_pan.ReadyElapsed, Is.Zero);
            Assert.That(_pan.SideSeconds, Is.Zero);
            Assert.That(_pan.BurnSeconds, Is.Zero);
        }
    }
}
