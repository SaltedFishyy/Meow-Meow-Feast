using System;
using System.Collections.Generic;
using System.Linq;
using AbilityStudio.Model;
using UnityEngine;

namespace AbilityStudio.Runtime
{
    /// <summary>Shared cast pipeline for gameplay, preview and deterministic tests.</summary>
    public sealed class SkillExecutor
    {
        private readonly ISkillClock _clock;
        private readonly ITargetProvider _targets;
        private readonly IEffectOrderPolicy _order;
        private readonly List<IEffectHandler> _handlers;
        private readonly Dictionary<(ISkillCaster, SkillDefinition, SkillKind), double> _ready = new Dictionary<(ISkillCaster, SkillDefinition, SkillKind), double>();
        private readonly List<SkillProjectile> _projectiles = new List<SkillProjectile>();
        private readonly Dictionary<(ISkillCaster, SkillDefinition, string), double> _feedbackReady = new Dictionary<(ISkillCaster, SkillDefinition, string), double>();
        /// <summary>Presentation-only hit requests; subscribing does not change combat resolution.</summary>
        public event Action<HitFeedbackRequest> FeedbackRequested;
        /// <summary>Presentation adapters clear transient objects and restore camera offsets on reset.</summary>
        public event Action ResetPerformed;
        /// <summary>Creates a runtime with explicit world and time dependencies.</summary>
        public SkillExecutor(ISkillClock clock, ITargetProvider targets, IEffectOrderPolicy order = null, IEnumerable<IEffectHandler> handlers = null)
        { _clock = clock; _targets = targets; _order = order ?? new FixedTypeOrderPolicy(); _handlers = handlers?.ToList() ?? new List<IEffectHandler> { new DamageEffectHandler(), new PullEffectHandler(), new KnockbackEffectHandler(), new StunEffectHandler(), new BleedEffectHandler(), new SlowEffectHandler() }; }
        /// <summary>Live projectiles for presentation.</summary>
        public IReadOnlyList<SkillProjectile> Projectiles => _projectiles;
        /// <summary>Impact notification after damage and effects.</summary>
        public event Action<SkillProjectile, ISkillTarget> Hit;
        /// <summary>Instant area activation for presentation, including casts hitting no targets.</summary>
        public event Action<Vector3, float> AreaCast;
        /// <summary>Rectangle activation: origin, normalized direction, length and width.</summary>
        public event Action<Vector3, Vector3, float, float> RectangleCast;
        /// <summary>Cone activation: origin, direction, range and full angle.</summary>
        public event Action<Vector3, Vector3, float, float> ConeCast;
        /// <summary>Seconds remaining on the current cast's cooldown.</summary>
        public float CooldownRemaining(SkillDefinition skill, ISkillCaster caster, SkillKind? slot = null) =>
            _ready.TryGetValue((caster, skill, skill != null && skill.WeaponMode ? slot ?? skill.Kind : SkillKind.NormalAttack), out var ready) ? (float)Math.Max(0, ready - _clock.Time) : 0;
        private int _generation;
        internal int Generation => _generation;
        private sealed class AttackSnapshot
        {
            internal AttackModule Module;
            internal Vector3 Position;
            internal CastEffects Effects, BodyEffects, TipEffects;
            internal DamageMode DamageMode;
            internal int Pierces, Chains;
            internal float ChainDistance, ReturnMultiplier;
        }
        private static AttackSnapshot SnapshotAttack(SkillDefinition skill, AttackModule attack, ISkillCaster caster, Vector3 aim, float chargeRatio)
        {
            bool explicitDamage = attack.DamageMode == DamageMode.ConnectedDamage;
            float baseDamage = skill.WeaponMode ? skill.ReleaseForAttack(attack).BaseDamage : skill.BaseDamage;
            var snapshot = new AttackSnapshot { Module = CloneAttack(attack), Position = SkillPositions.AttackPosition(skill, attack, caster, aim), DamageMode = attack.DamageMode,
                Effects = SnapshotEffects(skill, attack.ModuleId, caster, aim, "", explicitDamage, baseDamage),
                BodyEffects = SnapshotEffects(skill, attack.ModuleId, caster, aim, BindingRules.BodyTargets, explicitDamage, baseDamage),
                TipEffects = SnapshotEffects(skill, attack.ModuleId, caster, aim, BindingRules.TipTargets, explicitDamage, baseDamage) };
            if (attack is LineThrustAttack rectangle) ((LineThrustAttack)snapshot.Module).Length = SkillChargeGeometry.Length(skill, rectangle, chargeRatio);
            if (attack is CircleAttack circle) ((CircleAttack)snapshot.Module).Radius = SkillChargeGeometry.Radius(skill, circle, chargeRatio);
            if (attack is ConeAttack cone) ((ConeAttack)snapshot.Module).Range = SkillChargeGeometry.Range(skill, cone, chargeRatio);
            foreach (var response in skill.Responses.Where(r => skill.BindingSource(r.ModuleId, BindingRules.Projectile) == attack.ModuleId))
            {
                if (response is PierceResponse pierce) snapshot.Pierces = (int)Math.Min(int.MaxValue, (long)snapshot.Pierces + pierce.Count);
                if (response is ChainResponse chain) { snapshot.Chains = (int)Math.Min(int.MaxValue, (long)snapshot.Chains + chain.Count); snapshot.ChainDistance = Mathf.Max(snapshot.ChainDistance, chain.Distance); }
                if (response is ReturnResponse back) snapshot.ReturnMultiplier = Mathf.Max(snapshot.ReturnMultiplier, back.SpeedMultiplier);
            }
            return snapshot;
        }
        private static EffectModule CloneEffect(EffectModule module)
        {
            switch (module)
            {
                case HitFeedbackEffect feedback: return feedback.Snapshot();
                case DamageEffect damage: return new DamageEffect { MultiplierPercent = damage.MultiplierPercent };
                case PullEffect pull: return new PullEffect { StopDistance = pull.StopDistance, Duration = pull.Duration };
                case StunEffect stun: return new StunEffect { Duration = stun.Duration };
                case KnockbackEffect knockback: return new KnockbackEffect { Distance = knockback.Distance };
                case BleedEffect bleed: return new BleedEffect { DamagePerSecond = bleed.DamagePerSecond, Duration = bleed.Duration };
                case SlowEffect slow: return new SlowEffect { ReductionPercent = slow.ReductionPercent, Duration = slow.Duration };
                default: throw new ArgumentException("Unsupported effect");
            }
        }
        private static AttackModule CloneAttack(AttackModule module)
        {
            switch (module)
            {
                case ProjectileAttack p: return new ProjectileAttack { Speed = p.Speed, Range = p.Range, Size = p.Size };
                case CircleAttack c: return new CircleAttack { Radius = c.Radius };
                case LineThrustAttack r: return new LineThrustAttack { Length = r.Length, Width = r.Width, MinLength = r.MinLength, TipLength = r.TipLength };
                case ConeAttack c: return new ConeAttack { Range = c.Range, AngleDegrees = c.AngleDegrees };
                default: throw new ArgumentException("Unsupported attack");
            }
        }
        private static CastEffects SnapshotEffects(SkillDefinition skill, string attackId, ISkillCaster caster, Vector3 aim, string sourcePort = "", bool explicitDamage = false, float baseDamage = 0)
        {
            var result = new CastEffects { BaseDamage = baseDamage, Skill = skill };
            foreach (var original in skill.Effects)
            {
                var source = skill.BindingSource(original.ModuleId, BindingRules.Targets);
                if (source != attackId || skill.BindingOutput(original.ModuleId, BindingRules.Targets) != sourcePort) continue;
                if (original is DamageEffect && !explicitDamage) continue;
                var clone = CloneEffect(original); result.Modules.Add(clone);
                if (!(original is PullEffect)) continue;
                var destination = skill.BindingSource(original.ModuleId, BindingRules.Destination);
                if (destination != null) result.Destinations[clone] = SkillPositions.ResolveBinding(destination, skill, caster, aim);
                var completed = SnapshotEffects(skill, original.ModuleId, caster, aim, "", explicitDamage, baseDamage);
                if (completed.Modules.Count > 0) result.Completions[clone] = completed;
            }
            return result;
        }
        /// <summary>Reads data/bindings afresh and snapshots positions, target routing and parameters for this cast.</summary>
        public CastResult TryCast(SkillDefinition skill, ISkillCaster caster, Vector3 aimPoint, float chargeRatio = 0, SkillKind? slot = null)
        {
            if (caster == null || !float.IsFinite(chargeRatio) || chargeRatio < 0 || chargeRatio > 1 || SkillValidator.HasErrors(skill)) return CastResult.Invalid;
            if (caster is IStatusReceiver status && status.IsStunned) return CastResult.Stunned;
            var selectedSlot = slot ?? skill.Kind;
            var release = skill.ReleaseFor(selectedSlot);
            var selectedAttacks = skill.AttacksFor(selectedSlot).ToArray();
            if (skill.WeaponMode && (release == null || selectedAttacks.Length == 0)) return CastResult.Invalid;
            var origin = SkillPositions.CastOrigin(skill, caster);
            var delta = aimPoint - origin; delta.z = 0;
            bool needsDirection = selectedAttacks.Any(a => !(a is CircleAttack)) ||
                skill.Bindings.Any(b => skill.FindModule(b.SourceId) is SkillEndpointPosition);
            if (!float.IsFinite(aimPoint.x) || !float.IsFinite(aimPoint.y) || !float.IsFinite(aimPoint.z) ||
                !float.IsFinite(delta.sqrMagnitude) || (delta.sqrMagnitude < 0.0001f && needsDirection)) return CastResult.Invalid;
            if (CooldownRemaining(skill, caster, selectedSlot) > 0) return CastResult.OnCooldown;
            var direction = delta.normalized;
            var attacks = selectedAttacks.Select(a => SnapshotAttack(skill, a, caster, aimPoint, chargeRatio)).ToArray();
            float damage = skill.WeaponMode ? release.BaseDamage : skill.BaseDamage;
            _ready[(caster, skill, skill.WeaponMode ? selectedSlot : SkillKind.NormalAttack)] = _clock.Time + (skill.WeaponMode ? release.Cooldown : skill.Cooldown);
            foreach (var snapshot in attacks)
            {
                var attack = snapshot.Module; var position = snapshot.Position; var effects = snapshot.Effects;
                float automaticDamage = snapshot.DamageMode == DamageMode.BaseDamage ? damage : 0;
                if (attack is ProjectileAttack projectile)
                {
                    _projectiles.Add(new SkillProjectile { Caster = caster, Position = position, Direction = direction,
                        Aim = aimPoint, Speed = projectile.Speed, Remaining = projectile.Range, Radius = projectile.Size,
                        Damage = automaticDamage, Effects = effects, OriginalRange = projectile.Range,
                        Pierces = snapshot.Pierces, Chains = snapshot.Chains, ChainDistance = snapshot.ChainDistance, ReturnMultiplier = snapshot.ReturnMultiplier });
                    continue;
                }
                bool Intersects(ISkillTarget target)
                {
                    if (target == null || !target.IsAlive || target.Team == caster.Team) return false;
                    if (attack is CircleAttack circle)
                    {
                        var offset = target.Position - position; offset.z = 0;
                        return offset.sqrMagnitude <= Mathf.Pow(circle.Radius + Mathf.Max(0, target.Radius), 2);
                    }
                    if (attack is ConeAttack cone) return ConeGeometry.Intersects(position, direction, cone.Range, cone.AngleDegrees, target.Position, target.Radius);
                    var rectangle = (LineThrustAttack)attack;
                    return RectangleGeometry.Intersects(position, direction, rectangle.Length, rectangle.Width, target.Position, target.Radius);
                }
                // Classify the entire hit set before effects can move or kill any targets.
                var candidates = _targets.Targets.Distinct().Where(Intersects).Select(target => (target,
                    tip: attack is LineThrustAttack rect && SkillChargeGeometry.IsTip(position, direction, rect.Length, rect.Width, rect.TipLength, target))).ToArray();
                foreach (var hit in candidates)
                {
                    var target = hit.target;
                    ApplyHit(caster, target, target.Position, aimPoint, automaticDamage, effects);
                    if (attack is LineThrustAttack) ApplyEffects(caster, target, target.Position, aimPoint, hit.tip ? snapshot.TipEffects : snapshot.BodyEffects);
                }
                if (attack is CircleAttack c) AreaCast?.Invoke(position, c.Radius);
                else if (attack is ConeAttack cone) ConeCast?.Invoke(position, direction, cone.Range, cone.AngleDegrees);
                else if (attack is LineThrustAttack r) RectangleCast?.Invoke(position, direction, r.Length, r.Width);
            }
            return CastResult.Success;
        }
        private void ApplyEffect(EffectModule effect, EffectContext context)
        {
            foreach (var handler in _handlers)
                if (handler.CanHandle(effect)) { handler.Apply(effect, context); break; }
        }
        private void ApplyHit(ISkillCaster caster, ISkillTarget target, Vector3 point, Vector3 aim, float damage, CastEffects effects)
        {
            if (damage > 0 && target is IDamageable health) health.ApplyDamage(damage * 100 / (100 + Mathf.Max(0, health.Armor)), caster);
            ApplyEffects(caster, target, point, aim, effects);
        }
        private void ApplyEffects(ISkillCaster caster, ISkillTarget target, Vector3 point, Vector3 aim, CastEffects effects)
        {
            foreach (var effect in _order.Order(effects.Modules))
            {
                if (effect is HitFeedbackEffect feedback)
                {
                    if (feedback.TriggerMode == 1 && !effects.FiredFeedback.Add(feedback)) continue;
                    var key = (caster, effects.Skill, feedback.ModuleId);
                    if (_feedbackReady.TryGetValue(key, out var ready) && _clock.Time < ready) continue;
                    if (feedback.MinInterval > 0) _feedbackReady[key] = _clock.Time + feedback.MinInterval;
                    FeedbackRequested?.Invoke(new HitFeedbackRequest(feedback.Snapshot(), caster, target, point));
                    continue;
                }
                Vector3? destination = effects.Destinations.TryGetValue(effect, out var value) ? value : (Vector3?)null;
                Action completed = null;
                if (effects.Completions.TryGetValue(effect, out var afterPull))
                {
                    int generation = _generation;
                    completed = () =>
                    {
                        if (generation != _generation || !target.IsAlive) return;
                        ApplyEffects(caster, target, target.Position, aim, afterPull);
                    };
                }
                ApplyEffect(effect, new EffectContext(caster, target, point, aim, destination, completed, effects.BaseDamage));
            }
        }
        private static Vector3 GroundDelta(Vector3 from, Vector3 to)
        { var delta = to - from; delta.z = 0; return delta; }
        private static bool ValidTarget(SkillProjectile shot, ISkillTarget target) =>
            target != null && target.IsAlive && target.Team != shot.Caster.Team && !shot.HitTargets.Contains(target);
        private static void EndOutbound(SkillProjectile shot)
        {
            if (shot.Returning || shot.ReturnMultiplier <= 0) { shot.Finished = true; return; }
            shot.Returning = true;
            shot.HitTargets.Clear();
            shot.Speed *= shot.ReturnMultiplier;
            // Bound pursuit so a faster moving caster cannot keep the projectile alive forever.
            shot.Remaining = Mathf.Max(shot.OriginalRange, GroundDelta(shot.Position, shot.Caster.Position).magnitude) * 2;
        }
        private void ContinueAfterHit(SkillProjectile shot, ISkillTarget[] candidates)
        {
            if (shot.Returning) return;
            if (shot.Pierces > 0) { shot.Pierces--; return; }
            if (shot.Chains > 0)
            {
                ISkillTarget next = null; float nearest = shot.ChainDistance * shot.ChainDistance;
                foreach (var target in candidates)
                {
                    if (!ValidTarget(shot, target)) continue;
                    float distance = GroundDelta(shot.Position, target.Position).sqrMagnitude;
                    if (distance > nearest || (next != null && distance == nearest)) continue;
                    nearest = distance; next = target;
                }
                if (next != null)
                {
                    shot.Chains--;
                    var delta = GroundDelta(shot.Position, next.Position);
                    if (delta.sqrMagnitude > .000001f) shot.Direction = delta.normalized;
                    shot.Remaining = shot.ChainDistance;
                    return;
                }
            }
            EndOutbound(shot);
        }
        /// <summary>Consumes swept collisions and leftover frame time across pierces, chains and return transitions.</summary>
        public void Tick(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
            foreach (var key in _feedbackReady.Where(pair => pair.Value <= _clock.Time).Select(pair => pair.Key).ToArray()) _feedbackReady.Remove(key);
            int generation = _generation;
            foreach (var shot in _projectiles.ToArray())
            {
                if (shot.Finished) continue;
                if (shot.Caster is UnityEngine.Object obj && obj == null) { shot.Finished = true; continue; }
                var candidates = _targets.Targets.Distinct().ToArray();
                float time = deltaTime;
                while (time > 0 && !shot.Finished)
                {
                    float homeDistance = float.PositiveInfinity;
                    if (shot.Returning)
                    {
                        var home = GroundDelta(shot.Position, shot.Caster.Position);
                        homeDistance = home.magnitude;
                        if (homeDistance <= .001f) { shot.Finished = true; break; }
                        shot.Direction = home / homeDistance;
                    }
                    if (shot.Remaining <= .000001f) { EndOutbound(shot); continue; }
                    float distance = Mathf.Min(shot.Speed * time, shot.Remaining, homeDistance);
                    ISkillTarget first = null;
                    float nearest = distance;
                    foreach (var target in candidates)
                    {
                        if (!ValidTarget(shot, target)) continue;
                        var offset = GroundDelta(target.Position, shot.Position);
                        float radius = shot.Radius + Mathf.Max(0, target.Radius);
                        float b = Vector3.Dot(offset, shot.Direction);
                        float c = offset.sqrMagnitude - radius * radius;
                        float discriminant = b * b - c;
                        if (discriminant < 0) continue;
                        float entry = c <= 0 ? 0 : -b - Mathf.Sqrt(discriminant);
                        if (entry < 0 || entry > nearest || (first != null && entry == nearest)) continue;
                        nearest = entry; first = target;
                    }
                    shot.Position += shot.Direction * nearest;
                    shot.Remaining = Mathf.Max(0, shot.Remaining - nearest);
                    time = Mathf.Max(0, time - nearest / shot.Speed);
                    if (first != null)
                    {
                        shot.HitTargets.Add(first);
                        ApplyHit(shot.Caster, first, shot.Position, shot.Aim, shot.Damage, shot.Effects);
                        if (_generation != generation) return;
                        ContinueAfterHit(shot, candidates);
                        Hit?.Invoke(shot, first);
                        if (_generation != generation) return;
                    }
                    else if (shot.Returning && nearest >= homeDistance) shot.Finished = true;
                    else if (shot.Remaining <= .000001f) EndOutbound(shot);
                    else break;
                }
            }
            _projectiles.RemoveAll(p => p.Finished);
        }
        /// <summary>Clears projectiles and cooldowns for a fresh test.</summary>
        public void Reset() { _generation++; foreach (var shot in _projectiles) shot.Finished = true; _projectiles.Clear(); _ready.Clear(); _feedbackReady.Clear(); ResetPerformed?.Invoke(); }
    }
}



















