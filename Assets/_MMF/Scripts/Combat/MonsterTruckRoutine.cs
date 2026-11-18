using MMF.Core;
using MMF.Data;
using MMF.Truck;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Tracks waiting and wandering between a monster's truck attacks.</summary>
    public sealed class MonsterTruckRoutine : MonoBehaviour
    {
        private enum State { Approaching, Waiting, Ready, Wandering }

        private const float ArrivalDistance = 0.1f;
        private const float ProgressDistance = 0.05f;
        private const float StuckTime = 2f;
        private const int TargetAttempts = 32;

        [SerializeField] private MonsterDefinition _definition = null;
        [SerializeField] private TruckArea _truck = null;
        [SerializeField] private MapBounds _map = null;
        private State _state;
        private float _remaining;
        private float _wanderNear;
        private float _wanderFar;
        private bool _hasWanderTarget;
        private Vector2 _waypoint;
        private float _bestDistance;
        private float _stuckElapsed;

        /// <summary>Whether the current wait has elapsed.</summary>
        public bool CanAttackTruck => _state == State.Ready;

        /// <summary>Whether truck attacks are suspended for wandering.</summary>
        public bool IsWandering => _state == State.Wandering;

        /// <summary>Current destination in world coordinates.</summary>
        public Vector2 WanderTarget { get; private set; }

        private void OnDisable() => ResetWait();

        internal void Step(bool chasingPlayer, bool truckInRange, bool lunging, Vector2 position, float deltaTime)
        {
            if (chasingPlayer)
            {
                ResetWait();
                return;
            }

            if (IsWandering)
            {
                if (!lunging) StepWander(position, deltaTime);
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

        /// <summary>Enters wandering at launch, with movement and timing deferred until after the lunge.</summary>
        public void OnTruckAttackLaunched()
        {
            float min = _definition.WanderMin;
            _remaining = Random.Range(min, Mathf.Max(min, _definition.WanderMax));
            _wanderNear = _definition.WanderNear;
            _wanderFar = Mathf.Max(_wanderNear, _definition.WanderFar);
            _state = State.Wandering;
            SelectWanderTarget(transform.position);
        }

        private void StepWander(Vector2 position, float deltaTime)
        {
            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                ResetWait();
                return;
            }

            if (_hasWanderTarget && (WanderTarget - position).sqrMagnitude <= ArrivalDistance * ArrivalDistance)
            {
                SelectWanderTarget(position);
                return;
            }

            if (_hasWanderTarget)
            {
                Vector2 waypoint = MonsterTargeting.NextWaypoint(position, WanderTarget, _truck.ExteriorRect,
                    _definition.Size * 0.5f);
                float distance = Vector2.Distance(position, waypoint);
                if (waypoint != _waypoint || distance <= _bestDistance - ProgressDistance)
                {
                    _waypoint = waypoint;
                    _bestDistance = distance;
                    _stuckElapsed = 0f;
                }
            }

            _stuckElapsed += deltaTime;
            if (_stuckElapsed >= StuckTime) SelectWanderTarget(position);
        }

        private void SelectWanderTarget(Vector2 position)
        {
            _stuckElapsed = 0f;
            _bestDistance = float.PositiveInfinity;
            _hasWanderTarget = false;
            WanderTarget = position;
            if (_truck == null || _map == null) return;

            Rect truck = _truck.ExteriorRect;
            Rect map = _map.WorldRect;
            float halfSize = transform.localScale.x * 0.5f;
            Rect inside = Rect.MinMaxRect(map.xMin + halfSize, map.yMin + halfSize,
                map.xMax - halfSize, map.yMax - halfSize);

            // Project an exterior sample from its nearest wall point to the sampled distance.
            // This also works when both distance settings are equal.
            float spread = Mathf.Max(_wanderFar, halfSize);
            for (int i = 0; i < TargetAttempts; i++)
            {
                Vector2 sample = new Vector2(Random.Range(truck.xMin - spread, truck.xMax + spread),
                    Random.Range(truck.yMin - spread, truck.yMax + spread));
                Vector2 wall = new Vector2(Mathf.Clamp(sample.x, truck.xMin, truck.xMax),
                    Mathf.Clamp(sample.y, truck.yMin, truck.yMax));
                Vector2 outward = sample - wall;
                if (outward == Vector2.zero) continue;

                Vector2 target = wall + outward.normalized * Random.Range(_wanderNear, _wanderFar);
                if (!inside.Contains(target) || (target - position).sqrMagnitude <= ArrivalDistance * ArrivalDistance)
                    continue;

                WanderTarget = target;
                _hasWanderTarget = true;
                return;
            }
        }

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
            _hasWanderTarget = false;
        }
    }
}
