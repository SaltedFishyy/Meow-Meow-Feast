using AbilityStudio.Model;
using UnityEngine;
using System.Linq;
namespace AbilityStudio.Runtime
{
    /// <summary>Input-independent press/hold/release lifecycle shared by gameplay adapters and tests.</summary>
    public sealed class SkillReleaseSession
    {
        private readonly SkillExecutor _executor;
        private SkillDefinition _skill;
        private ISkillCaster _caster;
        private string _releaseId;
        private int _generation;
        private SkillKind _kind;
        private bool _weaponMode;
        /// <summary>Input slot currently being held.</summary>
        public SkillKind Slot => _kind;
        private float _elapsed, _duration;
        /// <summary>Creates one input session for one caster.</summary>
        public SkillReleaseSession(SkillExecutor executor) { _executor = executor; }
        /// <summary>True while a valid charge is held.</summary>
        public bool IsCharging => _skill != null;
        /// <summary>Current normalized charge, saturated at one until release.</summary>
        public float ChargeRatio => IsCharging ? Mathf.Clamp01(_elapsed / _duration) : 0;
        /// <summary>Press begins charging without damage/cooldown, or immediately casts an instant skill.</summary>
        public CastResult Press(SkillDefinition skill, ISkillCaster caster, Vector3 aim, SkillKind? slot = null)
        {
            if (IsCharging && Valid) return CastResult.Busy;
            Cancel();
            if (caster == null || SkillValidator.HasErrors(skill)) return CastResult.Invalid;
            if (caster is IStatusReceiver status && status.IsStunned) return CastResult.Stunned;
            var selectedSlot = slot ?? skill.Kind;
            var selectedRelease = skill.ReleaseFor(selectedSlot);
            if (skill.WeaponMode && (selectedRelease == null || !skill.AttacksFor(selectedSlot).Any())) return CastResult.Invalid;
            if (_executor.CooldownRemaining(skill, caster, selectedSlot) > 0) return CastResult.OnCooldown;
            if (!(selectedRelease is ChargeRelease charge)) return _executor.TryCast(skill, caster, aim, 0, selectedSlot);
            _skill = skill; _caster = caster; _releaseId = charge.ModuleId; _duration = charge.Duration;
            _elapsed = 0; _generation = _executor.Generation; _kind = selectedSlot; _weaponMode = skill.WeaponMode;
            return CastResult.Charging;
        }
        private bool Valid => IsCharging && _generation == _executor.Generation && _skill.WeaponMode == _weaponMode && (_weaponMode || _skill.Kind == _kind) &&
            _skill.ReleaseFor(_kind)?.ModuleId == _releaseId && (!_weaponMode || _skill.AttacksFor(_kind).Any()) && !SkillValidator.HasErrors(_skill) &&
            !(_caster is Object obj && obj == null) && !(_caster is IStatusReceiver status && status.IsStunned);
        /// <summary>Advances a held charge; reset, incompatible edits and stun cancel without casting.</summary>
        public void Tick(float deltaTime)
        {
            if (!IsCharging) return;
            if (!Valid) { Cancel(); return; }
            if (float.IsFinite(deltaTime) && deltaTime > 0) _elapsed = Mathf.Min(_duration, _elapsed + deltaTime);
        }
        /// <summary>Snapshots charge and uses the current aim exactly once on release.</summary>
        public CastResult Release(Vector3 aim)
        {
            if (!Valid) { Cancel(); return CastResult.Invalid; }
            var skill = _skill; var caster = _caster; float ratio = ChargeRatio;
            var slot = _kind; Cancel(); return _executor.TryCast(skill, caster, aim, ratio, slot);
        }
        /// <summary>Cancels without damage or spending cooldown.</summary>
        public void Cancel() { _skill = null; _caster = null; _elapsed = 0; }
    }
}
