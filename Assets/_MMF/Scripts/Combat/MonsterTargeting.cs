using MMF.Data;
using MMF.Player;
using MMF.Truck;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Selects a player or a lifetime truck-end destination.</summary>
    public sealed class MonsterTargeting : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition _definition = null;
        [SerializeField] private Transform _player = null;
        [SerializeField] private PlayerHealth _playerHealth = null;
        [SerializeField] private TruckArea _truck = null;
        private bool _chasing;
        private bool _front;
        private const float WallClearance = 0.1f;

        /// <summary>Whether the latest targeting step selected the player.</summary>
        public bool IsChasingPlayer => _chasing;

        /// <summary>Whether this monster is assigned to the front (right) end.</summary>
        public bool TargetsFront => _front;

        private void OnEnable()
        {
            _chasing = false;
            _front = Random.value >= _definition.LeftEndProbability;
        }

        /// <summary>Updates chase hysteresis and returns the current destination.</summary>
        public Vector2 TargetPosition(Vector2 position)
        {
            Vector2 playerPosition = _player.position;
            _chasing = ShouldChase((_playerHealth != null && _playerHealth.IsDown) || _truck.Contains(playerPosition),
                Vector2.Distance(position, playerPosition), _chasing,
                _definition.ChaseStartDistance, _definition.ChaseStopDistance);
            float padding = _definition.Size * 0.5f + WallClearance;
            Vector2 destination = _chasing ? playerPosition :
                _truck.EndPoint(_front) + Vector2.right * (_front ? padding : -padding);
            return NextWaypoint(position, destination, _truck.ExteriorRect, _definition.Size * 0.5f);
        }

        /// <summary>Returns the next corner on the shortest route around the padded truck, or the destination.</summary>
        public static Vector2 NextWaypoint(Vector2 position, Vector2 destination, Rect truck, float halfSize)
        {
            float padding = halfSize + WallClearance;
            Rect bounds = Rect.MinMaxRect(truck.xMin - padding, truck.yMin - padding,
                truck.xMax + padding, truck.yMax + padding);
            if (bounds.Contains(position) && !truck.Contains(position))
            {
                // Recover clearance-band routes from the nearest padded edge.
                float nearest = position.x - bounds.xMin;
                Vector2 projected = new Vector2(bounds.xMin, position.y);
                if (bounds.xMax - position.x < nearest)
                {
                    nearest = bounds.xMax - position.x;
                    projected = new Vector2(bounds.xMax, position.y);
                }
                if (position.y - bounds.yMin < nearest)
                {
                    nearest = position.y - bounds.yMin;
                    projected = new Vector2(position.x, bounds.yMin);
                }
                if (bounds.yMax - position.y < nearest)
                    projected = new Vector2(position.x, bounds.yMax);
                position = projected;
            }
            if (!CrossesInterior(position, destination, bounds)) return destination;

            float shortest = float.PositiveInfinity;
            Vector2 next = destination;
            // Rectangle routes use visible entry/exit corners and either boundary direction.
            for (int first = 0; first < 4; first++)
            {
                Vector2 entry = Corner(bounds, first);
                float approach = Vector2.Distance(position, entry);
                if (approach <= Mathf.Epsilon || CrossesInterior(position, entry, bounds)) continue;
                for (int direction = -1; direction <= 1; direction += 2)
                {
                    float length = approach;
                    int current = first;
                    for (int step = 0; step < 4; step++)
                    {
                        Vector2 exit = Corner(bounds, current);
                        float total = length + Vector2.Distance(exit, destination);
                        if (total < shortest && !CrossesInterior(exit, destination, bounds))
                        {
                            shortest = total;
                            next = entry;
                        }
                        int following = (current + direction + 4) % 4;
                        length += Vector2.Distance(exit, Corner(bounds, following));
                        current = following;
                    }
                }
            }
            return next;
        }

        private static Vector2 Corner(Rect bounds, int index)
        {
            return new Vector2(index < 2 ? bounds.xMin : bounds.xMax,
                index == 0 || index == 3 ? bounds.yMin : bounds.yMax);
        }

        private static bool CrossesInterior(Vector2 start, Vector2 end, Rect bounds)
        {
            float enter = 0f, leave = 1f;
            Vector2 delta = end - start;
            return ClipAxis(start.x, delta.x, bounds.xMin, bounds.xMax, ref enter, ref leave) &&
                ClipAxis(start.y, delta.y, bounds.yMin, bounds.yMax, ref enter, ref leave) &&
                enter < leave;
        }

        private static bool ClipAxis(float start, float delta, float min, float max,
            ref float enter, ref float leave)
        {
            // Tangency and travel along a padded edge are safe; only its open interior blocks a route.
            if (delta == 0f) return start > min && start < max;
            float a = (min - start) / delta;
            float b = (max - start) / delta;
            enter = Mathf.Max(enter, Mathf.Min(a, b));
            leave = Mathf.Min(leave, Mathf.Max(a, b));
            return enter < leave;
        }

        /// <summary>Pure chase decision with separate acquisition and release distances.</summary>
        public static bool ShouldChase(bool playerInside, float distance, bool chasing,
            float startDistance, float stopDistance)
        {
            return !playerInside && distance <= (chasing ? stopDistance : startDistance);
        }
    }
}
