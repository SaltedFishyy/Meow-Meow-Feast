using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Tracks the wait between a monster's truck attacks.</summary>
    public sealed class MonsterTruckRoutine : MonoBehaviour
    {
        private enum State { Approaching, Waiting, Ready }

        [SerializeField] private MonsterDefinition _definition = null;
        private State _state;
        private float _remaining;

        /// <summary>Whether the current wait has elapsed.</summary>
        public bool CanAttackTruck => _state == State.Ready;

        private void OnDisable() => ResetWait();

        internal void Step(bool chasingPlayer, bool truckInRange, float deltaTime)
        {
            if (chasingPlayer)
            {
                ResetWait();
                return;
            }

            if (!truckInRange) return;

            switch (_state)
            {
                case State.Approaching:
                    BeginWait();
                    break;
                case State.Waiting:
                    _remaining -= deltaTime;
                    if (_remaining <= 0f) _state = State.Ready;
                    break;
            }
        }

        /// <summary>Starts a fresh wait as soon as a truck lunge is launched.</summary>
        public void OnTruckAttackLaunched() => BeginWait();

        private void BeginWait()
        {
            float min = _definition.TruckWaitMin;
            _remaining = Random.Range(min, Mathf.Max(min, _definition.TruckWaitMax));
            _state = _remaining > 0f ? State.Waiting : State.Ready;
        }

        private void ResetWait()
        {
            _state = State.Approaching;
            _remaining = 0f;
        }
    }
}
