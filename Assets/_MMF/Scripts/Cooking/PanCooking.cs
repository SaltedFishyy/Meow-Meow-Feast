using System;
using MMF.Data;
using MMF.Player;
using UnityEngine;

namespace MMF.Cooking
{
    /// <summary>Runs pan cooking on the station and keeps its contents marker in sync.</summary>
    [RequireComponent(typeof(FryingStation))]
    public sealed class PanCooking : MonoBehaviour
    {
        [SerializeField] private FryingStation _station = null;
        [SerializeField] private CookingTuning _tuning = null;
        [SerializeField] private PlayerInteractor _legacyInteractor = null;

        private PanCookingModel _model;
        private bool _writingStation;

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
            _model.SetExternalContents(_station.Item);
            _model.StateChanged += OnModelStateChanged;
            _station.ContentsChanged += OnStationContentsChanged;
        }

        private void OnDestroy()
        {
            _station.ContentsChanged -= OnStationContentsChanged;
            _model.StateChanged -= OnModelStateChanged;
        }

        private void Update() => _model.Tick(Time.deltaTime);

        /// <summary>Places a raw item if the pan is available.</summary>
        public bool TryPlace(ItemDefinition item) => !_legacyInteractor.Cooking && _model.TryPlace(item);

        /// <summary>Flips a ready side and outputs the result on the final flip.</summary>
        public bool TryFlip(out ItemDefinition cooked)
        {
            cooked = null;
            ItemDefinition item = _model.Item;
            bool finalFlip = _model.State == PanCookingModel.CookingState.ReadySecond;
            if (_legacyInteractor.Cooking || !_model.TryFlip(out cooked)) return false;
            Flipped?.Invoke(item, finalFlip);
            return true;
        }

        /// <summary>Collects one item from the pan, preserving any remaining legacy batch.</summary>
        public bool TryTakeBack(out ItemDefinition item)
        {
            item = null;
            if (_legacyInteractor.Cooking) return false;
            if (_station.Count <= 1) return _model.TryTakeBack(out item);

            item = _station.Item;
            _station.SetContents(item, _station.Count - 1);
            return true;
        }

        /// <summary>Returns a cooked item to the pan when collection failed.</summary>
        public bool PutBackCooked(ItemDefinition item) => !_legacyInteractor.Cooking && _model.PutBackCooked(item);

        private void OnModelStateChanged()
        {
            if (_station.Item != _model.Item)
            {
                _writingStation = true;
                _station.SetContents(_model.Item, _model.Item == null ? 0 : 1);
                _writingStation = false;
            }

            StateChanged?.Invoke();
        }

        private void OnStationContentsChanged()
        {
            if (!_writingStation) _model.SetExternalContents(_station.Item);
        }
    }
}
