using System;
using MMF.Data;
using UnityEngine;

namespace MMF.Cooking
{
    /// <summary>Runs pan cooking on the station and keeps its contents marker in sync.</summary>
    [RequireComponent(typeof(FryingStation))]
    public sealed class PanCooking : MonoBehaviour
    {
        [SerializeField] private FryingStation _station = null;
        [SerializeField] private CookingTuning _tuning = null;

        private PanCookingModel _model;

        public PanCookingModel.CookingState State => _model.State;
        public ItemDefinition Item => _model.Item;
        public float SideElapsed => _model.SideElapsed;
        public float SideSeconds => _model.SideSeconds;
        public float ReadyElapsed => _model.ReadyElapsed;
        public float BurnSeconds => _model.BurnSeconds;

        /// <summary>Raised after a pan state change updates the station marker.</summary>
        public event Action StateChanged;

        /// <summary>Raised after a successful flip with the original item and whether it was the final flip.</summary>
        public event Action<ItemDefinition, bool> Flipped;

        private void Awake()
        {
            _model = new PanCookingModel(_tuning, _station.BurntItem);
            _model.StateChanged += OnModelStateChanged;
        }

        private void OnDestroy()
        {
            _model.StateChanged -= OnModelStateChanged;
        }

        private void Update() => _model.Tick(Time.deltaTime);

        /// <summary>Places a raw item if the pan is available.</summary>
        public bool TryPlace(ItemDefinition item) => _model.TryPlace(item);

        /// <summary>Flips a ready side and outputs the result on the final flip.</summary>
        public bool TryFlip(out ItemDefinition cooked)
        {
            cooked = null;
            ItemDefinition item = _model.Item;
            bool finalFlip = _model.State == PanCookingModel.CookingState.ReadySecond;
            if (!_model.TryFlip(out cooked)) return false;
            Flipped?.Invoke(item, finalFlip);
            return true;
        }

        /// <summary>Collects one item from the pan.</summary>
        public bool TryTakeBack(out ItemDefinition item) => _model.TryTakeBack(out item);

        /// <summary>Returns a cooked item to the pan when collection failed.</summary>
        public bool PutBackCooked(ItemDefinition item) => _model.PutBackCooked(item);

        private void OnModelStateChanged()
        {
            _station.SetContents(_model.Item);
            StateChanged?.Invoke();
        }
    }
}
