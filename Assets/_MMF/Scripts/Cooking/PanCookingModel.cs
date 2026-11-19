using System;
using MMF.Data;

namespace MMF.Cooking
{
    /// <summary>Tracks one item through both sides of pan cooking.</summary>
    public sealed class PanCookingModel
    {
        public enum CookingState
        {
            Empty,
            CookingFirst,
            ReadyFirst,
            CookingSecond,
            ReadySecond,
            Burnt,
            Holding
        }

        private readonly CookingTuning _tuning;
        private readonly ItemDefinition _burntItem;
        private double _sideElapsed;
        private double _readyElapsed;

        public PanCookingModel(CookingTuning tuning, ItemDefinition burntItem)
        {
            _tuning = tuning;
            _burntItem = burntItem;
        }

        public CookingState State { get; private set; }
        public ItemDefinition Item { get; private set; }
        public float SideElapsed => (float)_sideElapsed;
        public float SideSeconds { get; private set; }
        public float ReadyElapsed => (float)_readyElapsed;
        public float BurnSeconds { get; private set; }

        /// <summary>Raised after the state, item, and timers have been updated.</summary>
        public event Action StateChanged;

        /// <summary>Starts cooking a raw item in an empty pan.</summary>
        public bool TryPlace(ItemDefinition item)
        {
            if (State != CookingState.Empty || item == null || item.FryingResult == null) return false;
            Item = item;
            StartSide(CookingState.CookingFirst);
            return true;
        }

        /// <summary>Flips a ready side and returns the cooked item after the second flip.</summary>
        public bool TryFlip(out ItemDefinition cooked)
        {
            cooked = null;
            if (State == CookingState.ReadyFirst)
            {
                StartSide(CookingState.CookingSecond);
                return true;
            }

            if (State != CookingState.ReadySecond) return false;
            cooked = Item.FryingResult;
            Store(null, CookingState.Empty);
            return true;
        }

        /// <summary>Removes the current item and clears its timers.</summary>
        public bool TryTakeBack(out ItemDefinition item)
        {
            item = Item;
            if (item == null) return false;
            Store(null, CookingState.Empty);
            return true;
        }

        /// <summary>Holds a cooked item in the empty pan until it can be collected.</summary>
        public bool PutBackCooked(ItemDefinition item)
        {
            if (State != CookingState.Empty || item == null) return false;
            Store(item, CookingState.Holding);
            return true;
        }

        /// <summary>Advances the current side and carries excess time into the burn timer.</summary>
        public void Tick(float deltaTime)
        {
            if (!(deltaTime > 0f)) return;

            if (State == CookingState.CookingFirst || State == CookingState.CookingSecond)
            {
                _sideElapsed += deltaTime;
                if (_sideElapsed < SideSeconds) return;

                _readyElapsed = _sideElapsed - SideSeconds;
                _sideElapsed = SideSeconds;
                ChangeState(State == CookingState.CookingFirst ? CookingState.ReadyFirst : CookingState.ReadySecond);
            }
            else if (State == CookingState.ReadyFirst || State == CookingState.ReadySecond)
            {
                _readyElapsed += deltaTime;
            }

            if ((State == CookingState.ReadyFirst || State == CookingState.ReadySecond) && _readyElapsed >= BurnSeconds)
                Store(_burntItem, CookingState.Burnt);
        }

        private void StartSide(CookingState state)
        {
            _sideElapsed = 0d;
            _readyElapsed = 0d;
            SideSeconds = _tuning.SideSeconds;
            BurnSeconds = _tuning.BurnSeconds;
            ChangeState(state);
        }

        private void Store(ItemDefinition item, CookingState state)
        {
            Item = item;
            _sideElapsed = 0d;
            _readyElapsed = 0d;
            SideSeconds = 0f;
            BurnSeconds = 0f;
            ChangeState(state);
        }

        private void ChangeState(CookingState state)
        {
            if (State == state) return;
            State = state;
            StateChanged?.Invoke();
        }
    }
}
