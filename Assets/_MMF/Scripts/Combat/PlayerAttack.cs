using System.Collections.Generic;
using MMF.Data;
using MMF.Player;
using MMF.Truck;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Executes selected weapon swings without reading input.</summary>
    [RequireComponent(typeof(PlayerBackpack))]
    public sealed class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private TruckArea _truck = null;
        private PlayerBackpack _backpack;
        private float _nextAttackTime;
        private readonly List<Collider2D> _colliders = new List<Collider2D>();
        private readonly HashSet<Monster> _hitMonsters = new HashSet<Monster>();
        private ContactFilter2D _filter;

        private void Awake()
        {
            _backpack = GetComponent<PlayerBackpack>();
            _filter = new ContactFilter2D { useTriggers = false };
        }

        /// <summary>Returns true when a weapon swings, including harmless swings inside the truck.</summary>
        public bool TryAttack(Vector2 aimWorldPosition)
        {
            ItemDefinition item = _backpack.ItemAt(_backpack.Model.SelectedSlot);
            if (item == null || item.Weapon == null || Time.time < _nextAttackTime) return false;
            WeaponDefinition weapon = item.Weapon;
            _nextAttackTime = Time.time + weapon.Cooldown;
            Vector2 origin = transform.position;
            if (_truck.Contains(origin)) return true;

            Vector2 aimDirection = aimWorldPosition - origin;
            _hitMonsters.Clear();
            int count = Physics2D.OverlapCircle(origin, weapon.Range, _filter, _colliders);
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = _colliders[i];
                Monster monster = collider.GetComponentInParent<Monster>();
                if (monster == null || !monster.isActiveAndEnabled || _hitMonsters.Contains(monster)) continue;
                if (!WeaponArc.Contains(origin, aimDirection, collider.ClosestPoint(origin),
                    weapon.Range, weapon.Angle)) continue;
                _hitMonsters.Add(monster);
                monster.TakeHit(weapon.Damage, (Vector2)monster.transform.position - origin);
            }
            _colliders.Clear();
            _hitMonsters.Clear();
            return true;
        }
    }
}
