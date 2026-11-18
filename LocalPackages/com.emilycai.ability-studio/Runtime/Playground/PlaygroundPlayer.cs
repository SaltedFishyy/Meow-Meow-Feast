using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace AbilityStudio.Playground
{
    /// <summary>Optional WASD/mouse adapter; casting stays in SkillExecutor.</summary>
    public sealed class PlaygroundPlayer : MonoBehaviour, ISkillCaster, ISkillTarget, IHealable
    {
        [SerializeField] private SkillDefinition _skill;
        [SerializeField] private SkillRuntimeHost _runtime;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _moveSpeed = 6;
        private CombatTarget _health;
        /// <summary>Shared health adapter for the caster.</summary>
        public CombatTarget Health => _health;
        /// <inheritdoc/>
        public float Radius => _health == null ? 0 : _health.Radius;
        /// <inheritdoc/>
        public bool IsAlive => _health != null && _health.IsAlive;
        /// <inheritdoc/>
        public float MaxHp => _health == null ? 0 : _health.MaxHp;
        /// <inheritdoc/>
        public void ApplyHealing(float amount, ISkillCaster source) => _health?.ApplyHealing(amount, source);
        private void Awake()
        {
            _health = GetComponent<CombatTarget>();
            if (_health == null) _health = gameObject.AddComponent<CombatTarget>();
            _health.SetTeam(Team);
        }
        private Vector3 _aim = Vector3.up * 8;
        private SkillReleaseSession _release;
        private int _heldInput;
        private SkillInputKey? _heldAuthoredKey;
        /// <summary>Most recently selected input branch, used for live previews.</summary>
        public SkillKind PreviewSlot { get; private set; }
        /// <summary>Release driving the current preview.</summary>
        public ReleaseModule PreviewRelease => _skill == null ? null : _skill.ReleaseFor(PreviewSlot);
        /// <summary>Only explicitly connected areas for the current weapon branch.</summary>
        public IEnumerable<AttackModule> PreviewAttacks => _skill.AttacksFor(PreviewSlot);
        /// <summary>Whether a held release is active.</summary>
        public bool IsCharging => _release?.IsCharging == true;
        /// <summary>Normalized charge used by previews.</summary>
        public float ChargeRatio => _release?.ChargeRatio ?? 0;
        /// <summary>Editor host may suppress input while another editor panel owns focus.</summary>
        public bool EditorInputBlocked { get; set; }
        /// <inheritdoc/>
        public Vector3 Position => transform.position;
        /// <inheritdoc/>
        public Quaternion Rotation => transform.rotation;
        /// <inheritdoc/>
        public int Team => 0;
        /// <summary>Current shared asset.</summary>
        public SkillDefinition Skill => _skill;
        /// <summary>Current world-space aim.</summary>
        public Vector3 AimPoint => _aim;
        /// <summary>UI can suppress casts over its interactive panel.</summary>
        public bool PointerOverHud { get; set; }
        /// <summary>UI can suppress movement while typing in runtime controls.</summary>
        public bool InputBlocked { get; set; }
        /// <summary>Result notification for HUD feedback.</summary>
        public event System.Action<CastResult> CastAttempted;
        /// <summary>Connects scene references.</summary>
        public void Configure(SkillDefinition skill, SkillRuntimeHost runtime, Camera camera)
        { CancelCharge(); _skill = skill; PreviewSlot = skill == null ? SkillKind.NormalAttack : skill.WeaponMode ? (skill.ReleaseFor(SkillKind.NormalAttack) != null ? SkillKind.NormalAttack : SkillKind.SpecialAttack) : skill.Kind; _runtime = runtime; _camera = camera; _release = runtime == null ? null : new SkillReleaseSession(runtime.Executor); }
        private void Update() => TickInput(Time.deltaTime);
        /// <summary>Processes one input step; an explicit delta also permits deterministic input tests.</summary>
        public void TickInput(float deltaTime)
        {
            if (_runtime == null || _camera == null || InputBlocked || EditorInputBlocked) { CancelCharge(); return; }
            if (_skill != null && _skill.AreaGraph && !_skill.DualAttack) PreviewSlot = SkillKind.NormalAttack;
            TickCharge(deltaTime);
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null)
            {
                float x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
                float y = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
                var next = Position + Vector3.ClampMagnitude(new Vector3(x, y, 0), 1) * (_moveSpeed * deltaTime);
                next.x = Mathf.Clamp(next.x, -13, 13); next.y = Mathf.Clamp(next.y, -10, 10);
                transform.position = next;
            }
            if (mouse != null)
            {
                var plane = new Plane(Vector3.forward, Position);
                var ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
                if (plane.Raycast(ray, out float distance)) _aim = ray.GetPoint(distance);
            }
            var direction = _aim - Position; direction.z = 0;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            if (PointerOverHud || keyboard?.escapeKey.wasPressedThisFrame == true) { CancelCharge(); return; }
            if (_skill != null && _skill.AreaGraph)
            {
                if (IsCharging && _heldAuthoredKey.HasValue)
                {
                    var held = SkillInput.Control(_heldAuthoredKey.Value);
                    if (held == null) CancelCharge();
                    else if (!held.isPressed) ReleaseCast(_aim);
                    return;
                }
                if (IsCharging && _heldInput == 3)
                {
                    if (keyboard == null) CancelCharge();
                    else if (!keyboard.spaceKey.isPressed) ReleaseCast(_aim);
                    return;
                }
                if (IsCharging) return;
                var normal = SkillInput.Control(_skill.NormalKey);
                var special = _skill.DualAttack ? SkillInput.Control(_skill.SpecialKey) : null;
                if (normal?.wasPressedThisFrame == true || special?.wasPressedThisFrame == true)
                {
                    bool useSpecial = special?.wasPressedThisFrame == true;
                    if (PressCast(_aim, useSpecial ? SkillKind.SpecialAttack : SkillKind.NormalAttack) == CastResult.Charging)
                        _heldAuthoredKey = useSpecial ? _skill.SpecialKey : _skill.NormalKey;
                }
                else if (keyboard?.spaceKey.wasPressedThisFrame == true && PressCast(_aim, PreviewSlot) == CastResult.Charging) _heldInput = 3;
                return;
            }
            var button = _skill != null && _skill.Kind == SkillKind.SpecialAttack ? mouse?.rightButton : mouse?.leftButton;
            if (IsCharging && _heldInput != 0)
            {
                var held = _heldInput == 3 ? keyboard?.spaceKey : _heldInput == 2 ? mouse?.rightButton : mouse?.leftButton;
                if (held == null) { CancelCharge(); return; }
                if (!held.isPressed) { ReleaseCast(_aim); return; }
            }
            if (_skill != null && _skill.WeaponMode)
            {
                if (IsCharging) return;
                bool special = mouse?.rightButton.wasPressedThisFrame == true;
                bool normal = mouse?.leftButton.wasPressedThisFrame == true;
                bool space = keyboard?.spaceKey.wasPressedThisFrame == true;
                if (special || normal || space)
                {
                    var slot = special ? SkillKind.SpecialAttack : normal ? SkillKind.NormalAttack : PreviewSlot;
                    if (PressCast(_aim, slot) == CastResult.Charging) _heldInput = special ? 2 : normal ? 1 : 3;
                }
                return;
            }
            if (_skill != null && !IsCharging && (button?.wasPressedThisFrame == true || keyboard?.spaceKey.wasPressedThisFrame == true))
            {
                int source = button?.wasPressedThisFrame == true ? (_skill.Kind == SkillKind.SpecialAttack ? 2 : 1) : 3;
                if (PressCast(_aim) == CastResult.Charging) _heldInput = source;
            }
        }
        /// <summary>Begins an instant or held cast through the shared release session.</summary>
        public CastResult PressCast(Vector3 aim, SkillKind? slot = null)
        {
            if (_runtime == null) return CastResult.Invalid;
            if (_release == null) _release = new SkillReleaseSession(_runtime.Executor);
            if (IsCharging) return CastResult.Busy;
            PreviewSlot = slot ?? (_skill != null && _skill.WeaponMode ? PreviewSlot : _skill.Kind);
            var result = _release.Press(_skill, this, aim, PreviewSlot); CastAttempted?.Invoke(result); return result;
        }
        /// <summary>Advances charge explicitly for deterministic preview tests.</summary>
        public void TickCharge(float deltaTime) { _release?.Tick(deltaTime); if (!IsCharging) _heldInput = 0; }
        /// <summary>Releases a held cast once, using the current mouse direction.</summary>
        public CastResult ReleaseCast(Vector3 aim)
        {
            var result = _release?.Release(aim) ?? CastResult.Invalid; _heldInput = 0; _heldAuthoredKey = null; CastAttempted?.Invoke(result); return result;
        }
        /// <summary>Cancels without attacking or consuming cooldown.</summary>
        public void CancelCharge() { _release?.Cancel(); _heldInput = 0; _heldAuthoredKey = null; }
        private void OnDisable() => CancelCharge();
        private void OnApplicationFocus(bool focused) { if (!focused) CancelCharge(); }
        /// <summary>Casts through the shared runtime, also used by smoke tests.</summary>
        public CastResult CastAt(Vector3 aim) => _runtime.Executor.TryCast(_skill, this, aim);
    }
}
