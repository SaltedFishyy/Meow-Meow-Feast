using MMF.Combat;
using MMF.Data;
using UnityEngine;

namespace MMF.Player
{
    /// <summary>Glides and slides against the supplied walls and active monsters, then creates a normal drop.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ThrownItem : MonoBehaviour
    {
        private const float StopSpeed = 0.05f;
        private const float ContactGap = 0.0001f;
        private const int MaximumImpactsPerFrame = 32;
        private ItemDefinition _item;
        private PlayerBackpack _backpack;
        private ThrowTuning _tuning;
        private BoxCollider2D[] _walls;
        private BoxCollider2D[] _monsterColliders;
        private Customer[] _customers;
        private Vector2 _direction;
        private Vector2 _halfSize;
        private float _speed;
        private float _glideElapsed;
        private float _glideTime;

        private bool IsGliding => _glideElapsed < _glideTime;

        /// <summary>Configures the same appearance as a ground item without registering it for pickup.</summary>
        public void Initialize(ItemDefinition item, Sprite sprite, float size, PlayerBackpack backpack,
            ThrowTuning tuning, Vector2 velocity, BoxCollider2D[] walls, float airTime = 0f)
        {
            _item = item;
            _backpack = backpack;
            _tuning = tuning;
            _walls = walls;
            // Spawners reuse these monsters by deactivating/reactivating them on death/respawn.
            Monster[] monsters = FindObjectsByType<Monster>(FindObjectsInactive.Include);
            _monsterColliders = new BoxCollider2D[monsters.Length];
            _customers = new Customer[monsters.Length];
            for (int i = 0; i < monsters.Length; i++)
            {
                _monsterColliders[i] = monsters[i].GetComponent<BoxCollider2D>();
                _customers[i] = monsters[i].GetComponent<Customer>();
            }
            _speed = velocity.magnitude;
            _direction = velocity.normalized;
            _glideTime = Mathf.Max(0f, airTime);
            _glideElapsed = 0f;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = item.Color;
            transform.localScale = new Vector3(size, size, 1f);
            _halfSize = renderer.bounds.extents;
        }

        private void Update()
        {
            float remaining = Time.deltaTime;
            if (IsGliding)
            {
                float duration = Mathf.Min(remaining, _glideTime - _glideElapsed);
                if (Move(duration, 0f)) return;
                _glideElapsed = Mathf.Min(_glideTime, _glideElapsed + duration);
                remaining = Mathf.Max(0f, remaining - duration);
                if (IsGliding) return;
            }
            if (Move(remaining, Mathf.Max(0f, _tuning.Friction))) return;
            if (_speed > StopSpeed) return;
            _backpack.DropItem(_item, transform.position);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private bool Move(float remaining, float friction)
        {
            Vector2 position = transform.position;
            for (int impact = 0; impact < MaximumImpactsPerFrame && remaining > 0f && _speed > (IsGliding ? 0f : StopSpeed); impact++)
            {
                float duration = friction > 0f ? Mathf.Min(remaining, _speed / friction) : remaining;
                float distance = _speed * duration - 0.5f * friction * duration * duration;
                // Resolve overlaps with current obstacle bounds before choosing the earliest impact.
                for (int i = 0; i < _walls.Length + _monsterColliders.Length; i++)
                {
                    BoxCollider2D wall = ObstacleAt(i);
                    if (wall == null || !wall.enabled || !wall.gameObject.activeInHierarchy) continue;
                    Bounds bounds = wall.bounds;
                    Separate(ref position, (Vector2)bounds.min - _halfSize, (Vector2)bounds.max + _halfSize);
                }
                float nearest = distance;
                float bounce = _tuning.Bounce;
                Vector2 normal = Vector2.zero;
                Customer customer = null;
                for (int i = 0; i < _walls.Length + _monsterColliders.Length; i++)
                {
                    BoxCollider2D wall = ObstacleAt(i);
                    if (wall == null || !wall.enabled || !wall.gameObject.activeInHierarchy) continue;
                    Bounds bounds = wall.bounds;
                    Vector2 min = (Vector2)bounds.min - _halfSize;
                    Vector2 max = (Vector2)bounds.max + _halfSize;
                    if (!Sweep(position, _direction, min, max, out float hit, out Vector2 hitNormal) ||
                        hit > nearest) continue;
                    float obstacleBounce = i < _walls.Length ? _tuning.Bounce : _tuning.MonsterBounce;
                    if (Mathf.Abs(hit - nearest) <= ContactGap && normal != Vector2.zero)
                    {
                        normal = (normal + hitNormal).normalized;
                        bounce = Mathf.Min(bounce, obstacleBounce);
                    }
                    else
                    {
                        normal = hitNormal;
                        bounce = obstacleBounce;
                    }
                    Customer hitCustomer = i < _walls.Length ? null : _customers[i - _walls.Length];
                    if (hit < nearest || customer == null) customer = hitCustomer;
                    nearest = hit;
                }

                if (_item.FeedsCustomer && customer != null && customer.TryFeed())
                {
                    gameObject.SetActive(false);
                    Destroy(gameObject);
                    return true;
                }

                position += _direction * nearest;
                if (normal == Vector2.zero)
                {
                    _speed = Mathf.Max(0f, _speed - friction * duration);
                    remaining -= duration;
                    break;
                }

                // Stable inverse of d = v*t - a*t*t/2, including zero friction.
                float endSpeed = Mathf.Sqrt(Mathf.Max(0f, _speed * _speed - 2f * friction * nearest));
                float elapsed = nearest > 0f ? 2f * nearest / (_speed + endSpeed) : 0f;
                remaining = Mathf.Max(0f, remaining - elapsed);
                position += normal * ContactGap;
                _direction = Vector2.Reflect(_direction, normal).normalized;
                _speed = endSpeed * Mathf.Clamp01(bounce);
            }
            // If the impact budget is exhausted, discard this frame's unused time rather than tunnel.
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            return false;
        }

        private BoxCollider2D ObstacleAt(int index)
        {
            return index < _walls.Length ? _walls[index] : _monsterColliders[index - _walls.Length];
        }

        private static void Separate(ref Vector2 position, Vector2 min, Vector2 max)
        {
            if (position.x <= min.x || position.x >= max.x ||
                position.y <= min.y || position.y >= max.y) return;
            float distance = position.x - min.x;
            Vector2 correction = Vector2.left * (distance + ContactGap);
            if (max.x - position.x < distance)
            {
                distance = max.x - position.x;
                correction = Vector2.right * (distance + ContactGap);
            }
            if (position.y - min.y < distance)
            {
                distance = position.y - min.y;
                correction = Vector2.down * (distance + ContactGap);
            }
            if (max.y - position.y < distance)
                correction = Vector2.up * (max.y - position.y + ContactGap);
            position += correction;
        }

        private static bool Sweep(Vector2 origin, Vector2 direction, Vector2 min, Vector2 max,
            out float distance, out Vector2 normal)
        {
            distance = float.NegativeInfinity;
            float exit = float.PositiveInfinity;
            normal = Vector2.zero;
            for (int axis = 0; axis < 2; axis++)
            {
                if (Mathf.Abs(direction[axis]) < 0.000001f)
                {
                    if (origin[axis] < min[axis] || origin[axis] > max[axis]) return false;
                    continue;
                }
                float near = (min[axis] - origin[axis]) / direction[axis];
                float far = (max[axis] - origin[axis]) / direction[axis];
                if (near > far) { float swap = near; near = far; far = swap; }
                Vector2 face = axis == 0 ? Vector2.left : Vector2.down;
                face *= Mathf.Sign(direction[axis]);
                if (near > distance)
                {
                    distance = near;
                    normal = face;
                }
                else if (near == distance)
                    normal = (normal + face).normalized;
                exit = Mathf.Min(exit, far);
                if (distance > exit) return false;
            }
            return distance >= 0f && exit >= distance;
        }
    }
}
