using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using AbilityStudio.Playground;
using MMF.Data;
using MMF.Player;
using UnityEngine;
namespace MMF.Combat
{
    /// <summary>Routes the equipped workshop weapon through the shared executor in the main scene.</summary>
    [DisallowMultipleComponent]
    public sealed class WorkshopWeaponController : MonoBehaviour, ISkillCaster, ISkillTarget, IHealable, ISkillClock, ITargetProvider
    {
        private WorkshopLoadout _loadout;
        private SkillDefinition _weapon;
        private SkillExecutor _executor;
        private SkillReleaseSession _release;
        private PlayerAttack _attack;
        private PlayerBackpack _backpack;
        private PlayerHealth _health;
        private PlayerInputReader _input;
        private PlayerInteractor _interactor;
        private Collider2D _collider;
        private Quaternion _rotation = Quaternion.identity;
        private float _pendingHealing;
        private SkillInputKey? _heldKey;
        public bool PressInput(SkillInputKey key, Vector2 aim)
        {
            if (key != SkillInputKey.MouseLeft || !HasWeapon) return false;
            bool result = Press(aim);
            if (_release.IsCharging && _heldKey == null) _heldKey = key;
            return result;
        }
        public void ReleaseInput(SkillInputKey key, Vector2 aim)
        { if (_heldKey == key) { Release(aim); _heldKey = null; } }
        private double _nextLoad;
        public bool HasWeapon { get { RefreshWeapon(); return _weapon != null; } }
        public Vector3 Position => transform.position;
        public Quaternion Rotation => _rotation;
        public int Team => 0;
        public float Radius => _collider == null ? 0 : Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.y);
        public bool IsAlive => isActiveAndEnabled && (_health == null || !_health.IsDown);
        public float MaxHp => _health == null ? 0 : _health.Max;
        public double Time => UnityEngine.Time.timeAsDouble;
        public IEnumerable<ISkillTarget> Targets
        {
            get
            {
                // Preserve the main game's harmless attacks inside the truck.
                if (_attack.Truck != null && _attack.Truck.Contains(Position)) yield break;
                foreach (var target in MonsterSkillTarget.Active)
                    if (target != null && target.gameObject.scene == gameObject.scene) yield return target;
            }
        }
        private void Awake()
        {
            _backpack = GetComponent<PlayerBackpack>();
            _attack = GetComponent<PlayerAttack>(); _health = GetComponent<PlayerHealth>();
            _input = GetComponent<PlayerInputReader>(); _interactor = GetComponent<PlayerInteractor>();
            _collider = GetComponent<Collider2D>();
            _executor = new SkillExecutor(this, this); _release = new SkillReleaseSession(_executor);
            var visuals = gameObject.AddComponent<WorkshopWeaponView>(); visuals.Bind(_executor);
            var feedback = gameObject.AddComponent<PlaygroundHitFeedback>(); feedback.Bind(_executor, Camera.main);
            gameObject.AddComponent<WorkshopAttackPreview>().Bind(this);
            RefreshWeapon();
        }
        private void RefreshWeapon()
        {
            if (_loadout == null && UnityEngine.Time.realtimeSinceStartupAsDouble >= _nextLoad)
            {
                _loadout = Resources.Load<WorkshopLoadout>(WorkshopLoadout.ResourceName);
                _nextLoad = UnityEngine.Time.realtimeSinceStartupAsDouble + .3;
            }
            var item = _backpack == null || _backpack.Model == null ? null : _backpack.ItemAt(_backpack.Model.SelectedSlot);
            var selected = item == null || _loadout == null || item.WorkshopWeapon != _loadout.PlayerOne ? null : item.WorkshopWeapon;
            if (selected == _weapon) return;
            Cancel(); _executor?.Reset(); _weapon = selected; _pendingHealing = 0;
        }
        public bool TryGetPreview(out SkillDefinition skill, out Vector3 aim, out float charge)
        {
            skill = null; aim = default; charge = 0;
            if (!HasWeapon || !IsAlive || !_loadout.ShowAttackPreview || _input == null || !_input.isActiveAndEnabled ||
                (_interactor != null && !_interactor.CanDrop) || !_input.TryGetMouseWorldPosition(out var point)) return false;
            Aim(point); skill = _weapon; aim = point; charge = _release.IsCharging ? _release.ChargeRatio : 1;
            return true;
        }
        private void Aim(Vector2 aim)
        {
            Vector2 direction = aim - (Vector2)Position;
            if (direction.sqrMagnitude > .000001f) _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
        /// <summary>Begins the normal attack, or the only legacy skill branch.</summary>
        public bool Press(Vector2 aim)
        {
            if (!HasWeapon || !IsAlive) return false;
            return PressSlot(aim, _weapon.WeaponMode ? SkillKind.NormalAttack : _weapon.Kind);
        }
        private bool PressSlot(Vector2 aim, SkillKind slot)
        {
            if (!IsAlive || _weapon == null || (_interactor != null && !_interactor.CanDrop)) return false;
            Aim(aim);
            var result = _release.Press(_weapon, this, aim, slot);
            return result == CastResult.Success || result == CastResult.Charging;
        }
        public void Release(Vector2 aim)
        {
            RefreshWeapon();
            if (!IsAlive || (_input != null && !_input.isActiveAndEnabled) || (_interactor != null && _interactor.Cooking)) { Cancel(); return; }
            Aim(aim); if (_release.IsCharging) _release.Release(aim);
        }
        public void Cancel() { _release?.Cancel(); _heldKey = null; }
        private void Update()
        {
            RefreshWeapon();
            if (!IsAlive || (_input != null && !_input.isActiveAndEnabled)) { Cancel(); _executor.Reset(); return; }
            if (_interactor != null && _interactor.Cooking) Cancel();
            _release.Tick(UnityEngine.Time.deltaTime);
            if (!_release.IsCharging) _heldKey = null;
            _executor.Tick(UnityEngine.Time.deltaTime);
        }
        public void ApplyHealing(float amount, ISkillCaster source)
        {
            if (_health == null || !IsAlive || !float.IsFinite(amount) || amount <= 0) return;
            _pendingHealing = Mathf.Min(_health.Max - _health.Current, _pendingHealing + amount);
            int points = Mathf.FloorToInt(_pendingHealing); _pendingHealing -= points; _health.Heal(points);
        }
        private void OnDisable() { Cancel(); _executor?.Reset(); }
    }
}
