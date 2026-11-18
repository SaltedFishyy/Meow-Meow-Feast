using MMF.Data;
using MMF.Player;
using MMF.Truck;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Runs the wind-up, lunge and cooldown of a monster's attack.</summary>
    [RequireComponent(typeof(Monster))]
    public sealed class MonsterAttack : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition _definition = null;
        [SerializeField] private PlayerHealth _player = null;
        [SerializeField] private Collider2D _playerCollider = null;
        [SerializeField] private TruckArea _truck = null;
        [SerializeField] private TruckHealth _truckHealth = null;
        [SerializeField] private MonsterTruckRoutine _truckRoutine = null;
        [SerializeField] private Color _warningColor = new Color(1f, 0.45f, 0.15f);
        [SerializeField, Min(0.01f)] private float _warningFlashInterval = 0.08f;
        [SerializeField, Min(0.01f)] private float _lungeSpeed = 14f;

        private Monster _monster;
        private Collider2D _collider;
        private Rigidbody2D _playerBody;
        private SpriteRenderer _renderer;
        private MonsterTargeting _targeting;
        private bool _attackingTruck;
        private bool _windingUp;
        private bool _warningApplied;
        private float _windUpElapsed;
        private float _lastLaunchTime;
        private float _lungeRemaining;
        private Vector2 _lungeDirection;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
            _targeting = GetComponent<MonsterTargeting>();
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _playerBody = _playerCollider.attachedRigidbody;
        }

        private void OnEnable()
        {
            _attackingTruck = false;
            _windingUp = false;
            _windUpElapsed = 0f;
            _lastLaunchTime = float.NegativeInfinity;
            _lungeRemaining = 0f;
        }

        private void OnDisable() => ClearWarning();

        internal bool Step(Vector2 position, bool hasExternalMotion, out Vector2 displacement)
        {
            displacement = Vector2.zero;
            bool chasingPlayer = _targeting.IsChasingPlayer;
            bool truckInRange = !chasingPlayer && TruckInRange(position);
            if (_truckRoutine != null)
                _truckRoutine.Step(chasingPlayer, truckInRange, _lungeRemaining > 0f, position, Time.fixedDeltaTime);

            if (_windingUp && _attackingTruck && (chasingPlayer || !_truckHealth.CanBeHit))
                _windingUp = false;

            if (_windingUp)
            {
                _windUpElapsed += Time.fixedDeltaTime;
                if (_windUpElapsed < _definition.AttackWindUp) return true;
                Launch(position);
                return StepLunge(hasExternalMotion, out displacement);
            }

            if (_lungeRemaining > 0f)
                return StepLunge(hasExternalMotion, out displacement);

            if (_truckRoutine != null && _truckRoutine.IsWandering) return false;

            if (truckInRange && _truckRoutine != null && !_truckRoutine.CanAttackTruck)
                return true;

            if (Time.fixedTime - _lastLaunchTime < _definition.AttackCooldown)
                return _truckRoutine != null ? truckInRange : _attackingTruck && chasingPlayer;

            bool attackTruck = !chasingPlayer;
            if (attackTruck ? !truckInRange : !PlayerInRange()) return false;

            _attackingTruck = attackTruck;
            _windingUp = true;
            _windUpElapsed = 0f;
            if (_definition.AttackWindUp > 0f) return true;
            Launch(position);
            return StepLunge(hasExternalMotion, out displacement);
        }

        internal void StopLunge() => _lungeRemaining = 0f;

        private bool PlayerInRange()
        {
            if (!_player.isActiveAndEnabled || !_player.CanBeHit) return false;
            ColliderDistance2D separation = _collider.Distance(_playerCollider);
            return separation.isValid && separation.distance <= _definition.AttackRange;
        }

        private bool TruckInRange(Vector2 position)
        {
            if (_truckHealth == null || !_truckHealth.CanBeHit) return false;

            Rect exterior = _truck.ExteriorRect;
            if (position.y < exterior.yMin || position.y > exterior.yMax) return false;

            Bounds monsterBounds = _collider.bounds;
            if (_targeting.TargetsFront)
                return position.x >= exterior.xMax &&
                    monsterBounds.min.x - exterior.xMax <= _definition.AttackRange;

            return position.x <= exterior.xMin &&
                exterior.xMin - monsterBounds.max.x <= _definition.AttackRange;
        }

        private void Launch(Vector2 position)
        {
            _windingUp = false;
            _lastLaunchTime = Time.fixedTime;
            if (_attackingTruck)
            {
                _lungeDirection = _targeting.TargetsFront ? Vector2.left : Vector2.right;
                _lungeRemaining = _definition.AttackRange;
                _truckRoutine?.OnTruckAttackLaunched();
                if (TruckInRange(position)) _truckHealth.TakeHit(_definition.TruckDamage);
                return;
            }

            Vector2 playerPosition = _playerBody != null ? _playerBody.position : (Vector2)_player.transform.position;
            _lungeDirection = (playerPosition - position).normalized;
            _lungeRemaining = _lungeDirection == Vector2.zero ? 0f : _definition.AttackRange;
            if (!_truck.Contains(_player.transform.position) && PlayerInRange())
                _player.TakeHit(_definition.AttackDamage, _lungeDirection);
        }

        private bool StepLunge(bool hasExternalMotion, out Vector2 displacement)
        {
            float distance = hasExternalMotion ? 0f : Mathf.Min(_lungeRemaining, _lungeSpeed * Time.fixedDeltaTime);
            _lungeRemaining -= distance;
            displacement = _lungeDirection * distance;
            return true;
        }

        private void LateUpdate()
        {
            if (!_windingUp || _monster.IsHitFlashing)
            {
                ClearWarning();
                return;
            }

            bool showWarning = Mathf.FloorToInt(_windUpElapsed / _warningFlashInterval) % 2 == 0;
            _renderer.color = showWarning ? _warningColor : _definition.Color;
            _warningApplied = showWarning;
        }

        private void ClearWarning()
        {
            if (!_warningApplied) return;
            if (!_monster.IsHitFlashing) _renderer.color = _definition.Color;
            _warningApplied = false;
        }
    }
}
