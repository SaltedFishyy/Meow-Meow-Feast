using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbilityStudio.Model
{
    /// <summary>Shared, versioned composition used by authoring, preview and gameplay.</summary>
    [CreateAssetMenu(menuName = "技能工坊/技能")]
    public sealed class SkillDefinition : ScriptableObject
    {
        [SerializeField] private int schemaVersion = 1;
        [SerializeField] private bool coreRemoved;
        [SerializeField] private bool areaGraph;
        [SerializeField] private int attackMode;
        [SerializeField] private SkillInputKey normalKey = SkillInputKey.MouseLeft;
        [SerializeField] private SkillInputKey specialKey = SkillInputKey.MouseRight;
        /// <summary>Explicit core-release-area graph with parameters owned by areas.</summary>
        public bool AreaGraph => areaGraph;
        public bool DualAttack => areaGraph ? attackMode == 1 : weaponMode;
        public int AttackMode { get => attackMode; set => attackMode = value; }
        public SkillInputKey NormalKey { get => normalKey; set => normalKey = value; }
        public SkillInputKey SpecialKey { get => specialKey; set => specialKey = value; }
        internal void MarkAreaGraph(bool dual, SkillInputKey primary)
        { areaGraph = true; attackMode = dual ? 1 : 0; normalKey = primary; kind = SkillKind.NormalAttack; }
        /// <summary>Maps an authored input to a visible attack branch.</summary>
        public SkillKind? SlotForKey(SkillInputKey key)
        {
            if (key == normalKey) return SkillKind.NormalAttack;
            if (DualAttack && key == specialKey) return SkillKind.SpecialAttack;
            return null;
        }
        /// <summary>Legacy assets retain their core until it is explicitly removed.</summary>
        public bool HasCore => !coreRemoved;
        /// <summary>Restores the core without reconnecting deleted edges or resetting parameters.</summary>
        public bool AddCore()
        {
            if (HasCore) return false;
            coreRemoved = false; return true;
        }
        [SerializeField] private string id = "new_skill";
        [SerializeField] private string displayName = "新技能";
        [SerializeField] private Vector3 castOriginOffset;
        [SerializeField] private float cooldown = 1;
        [SerializeField] private float baseDamage = 2;
        [SerializeField] private float distance = 8;
        [SerializeField] private bool integratedAttacks;
        /// <summary>Complete attack nodes connected directly to the two input slots.</summary>
        public bool IntegratedAttacks { get => integratedAttacks; set { integratedAttacks=value; if(value) weaponMode=true; } }
        /// <summary>One complete attack selected by a root output.</summary>
        public AttackModule AttackFor(SkillKind slot) => Attacks.FirstOrDefault(a => BindingSource(a.ModuleId,BindingRules.Trigger)==CoreLayoutId && BindingOutput(a.ModuleId,BindingRules.Trigger)==(slot==SkillKind.NormalAttack?BindingRules.Normal:BindingRules.Special));
        [SerializeField] private bool weaponMode;
        /// <summary>Explicit dual-attack routing; false preserves legacy skill behavior.</summary>
        public bool WeaponMode { get => weaponMode; set => weaponMode = value; }
        /// <summary>Release connected to a weapon input, or legacy release for single skills.</summary>
        public ReleaseModule ReleaseFor(SkillKind slot) => areaGraph && !DualAttack && slot == SkillKind.SpecialAttack ? null : integratedAttacks ? AttackFor(slot)?.ReleaseSettings : !weaponMode ? ActiveRelease : releases.FirstOrDefault(r => r != null && BindingSource(r.ModuleId, BindingRules.Trigger) == CoreLayoutId && BindingOutput(r.ModuleId, BindingRules.Trigger) == (slot == SkillKind.NormalAttack ? BindingRules.Normal : BindingRules.Special));
        /// <summary>Owning branch of an explicitly wired attack.</summary>
        public ReleaseModule ReleaseForAttack(AttackModule area) => integratedAttacks ? area.ReleaseSettings : FindModule(BindingSource(area.ModuleId, BindingRules.Cast)) as ReleaseModule;
        /// <summary>Areas that participate in the requested branch.</summary>
        public IEnumerable<AttackModule> AttacksFor(SkillKind slot) => integratedAttacks ? Attacks.Where(a => a == AttackFor(slot)) : !weaponMode ? Attacks : Attacks.Where(a => ReleaseFor(slot) != null && BindingSource(a.ModuleId, BindingRules.Cast) == ReleaseFor(slot).ModuleId);
        [SerializeField] private SkillKind kind;
        /// <summary>Normal or special input slot; old assets default to normal.</summary>
        public SkillKind Kind { get => kind; set => kind = value; }
        [SerializeReference] private List<ReleaseModule> releases = new List<ReleaseModule>();
        /// <summary>Release modules, active only when connected to the core.</summary>
        public IReadOnlyList<ReleaseModule> Releases => releases;
        /// <summary>Selected release, or null for the legacy instant default.</summary>
        public ReleaseModule ActiveRelease => FindModule(BindingSource(CoreLayoutId, BindingRules.Release)) as ReleaseModule;
        /// <summary>Adds an unconnected release module.</summary>
        public void AddRelease(ReleaseModule value)
        { if (value == null) throw new ArgumentNullException(nameof(value)); releases.Add(value); }
        [SerializeField] private bool coreEndpointMigrated;
        /// <summary>Maximum ground-plane cast distance from cast origin.</summary>
        public float Distance { get => distance; set => distance = value; }
        /// <summary>Whether a legacy endpoint is available to merge into the core.</summary>
        public bool NeedsEndpointMigration => HasCore && !coreEndpointMigrated && positions.OfType<SkillEndpointPosition>().Any();
        /// <summary>Moves the first legacy endpoint into the core, preserving differently configured legacy sources.</summary>
        public bool MigrateEndpointToCore()
        {
            if (!NeedsEndpointMigration) return false;
            distance = positions.OfType<SkillEndpointPosition>().First().Distance;
            var ids = new HashSet<string>(positions.OfType<SkillEndpointPosition>().Where(p => p.Distance == distance).Select(p => p.ModuleId));
            for (int i = 0; i < bindings.Count; i++)
                if (ids.Contains(bindings[i].SourceId)) bindings[i] = new ModuleBinding(CoreLayoutId, bindings[i].TargetId, bindings[i].Input, bindings[i].SourcePort);
            positions.RemoveAll(p => p != null && ids.Contains(p.ModuleId));
            boardLayout.RemoveAll(p => ids.Contains(p.ModuleId));
            coreEndpointMigrated = true;
            return true;
        }
        [SerializeReference] private AttackModule attack;
        [SerializeReference] private List<AttackModule> additionalAttacks = new List<AttackModule>();
        [SerializeReference] private List<EffectModule> effects = new List<EffectModule>();
        [SerializeReference] private List<PositionModule> positions = new List<PositionModule>();
        [SerializeReference] private List<HitResponseModule> responses = new List<HitResponseModule>();
        /// <summary>Independent projectile continuation modules.</summary>
        public IReadOnlyList<HitResponseModule> Responses => responses;
        /// <summary>Adds a response without automatically connecting it.</summary>
        public void AddResponse(HitResponseModule value)
        { if (value == null) throw new ArgumentNullException(nameof(value)); responses.Add(value); }
        [SerializeField] private List<ModuleBinding> bindings = new List<ModuleBinding>();
        /// <summary>Reusable position sources.</summary>
        public IReadOnlyList<PositionModule> Positions => positions;
        /// <summary>Explicit input bindings. Unbound inputs retain legacy defaults.</summary>
        public IReadOnlyList<ModuleBinding> Bindings => bindings;
        /// <summary>All modules with persistent identity.</summary>
        public IEnumerable<SkillModule> Modules => Attacks.Cast<SkillModule>().Concat(effects).Concat(positions).Concat(responses).Concat(releases);
        /// <summary>Looks up a module by identity.</summary>
        public SkillModule FindModule(string moduleId) => Modules.FirstOrDefault(m => m != null && m.ModuleId == moduleId);
        /// <summary>Looks up the explicit source of an input, or null for its default.</summary>
        public string BindingSource(string targetId, string input) => bindings.FirstOrDefault(b => b.TargetId == targetId && b.Input == input)?.SourceId;
        /// <summary>Returns the selected source output channel, empty for old bindings.</summary>
        public string BindingOutput(string targetId, string input) => bindings.FirstOrDefault(b => b.TargetId == targetId && b.Input == input)?.SourcePort ?? "";
        /// <summary>Adds a position source without changing existing default bindings.</summary>
        public void AddPosition(PositionModule value) { if (value == null) throw new ArgumentNullException(nameof(value)); positions.Add(value); }
        /// <summary>Replaces one input binding, or disconnects it to restore the default.</summary>
        public void SetBinding(string sourceId, string targetId, string input, string sourcePort = "")
        {
            if (sourceId != null && !BindingRules.CanConnect(this, sourceId, targetId, input, sourcePort)) throw new ArgumentException("Incompatible module binding.");
            bindings.RemoveAll(b => b.TargetId == targetId && b.Input == input);
            if (sourceId != null) bindings.Add(new ModuleBinding(sourceId, targetId, input, sourcePort));
        }
        [SerializeField] private List<ModuleLayout> boardLayout = new List<ModuleLayout>();
        /// <summary>Reserved presentation identity for the skill core.</summary>
        public const string CoreLayoutId = "skill_core";
        /// <summary>Returns a saved canvas position or the supplied initial position.</summary>
        public Vector2 GetBoardPosition(string moduleId, Vector2 fallback) =>
            boardLayout.Find(item => item.ModuleId == moduleId)?.Position ?? fallback;
        /// <summary>Updates editor presentation without changing skill behavior.</summary>
        public void SetBoardPosition(string moduleId, Vector2 position)
        {
            var item = boardLayout.Find(entry => entry.ModuleId == moduleId);
            if (item == null) boardLayout.Add(new ModuleLayout(moduleId, position));
            else item.Position = position;
        }
        /// <summary>Persisted schema version.</summary>
        public int SchemaVersion => schemaVersion;
        /// <summary>Stable skill identifier.</summary>
        public string Id { get => id; set => id = value; }
        /// <summary>Designer-facing name.</summary>
        public string DisplayName { get => displayName; set => displayName = value; }
        /// <summary>Origin offset in caster space.</summary>
        public Vector3 CastOriginOffset { get => castOriginOffset; set => castOriginOffset = value; }
        /// <summary>Seconds between successful casts.</summary>
        public float Cooldown { get => cooldown; set => cooldown = value; }
        /// <summary>Raw damage per hit.</summary>
        public float BaseDamage { get => baseDamage; set => baseDamage = value; }
        /// <summary>Legacy primary attack slot, retained to preserve existing assets.</summary>
        public AttackModule Attack => attack;
        /// <summary>Additional independent attack instances.</summary>
        public IReadOnlyList<AttackModule> AdditionalAttacks => additionalAttacks;
        /// <summary>Every attack instance participating in each cast.</summary>
        public IEnumerable<AttackModule> Attacks
        {
            get
            {
                if (attack != null) yield return attack;
                foreach (var module in additionalAttacks) yield return module;
            }
        }
        /// <summary>Adds an independent attack, retaining the legacy primary slot when possible.</summary>
        public void AddAttack(AttackModule value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (attack == null) attack = value;
            else additionalAttacks.Add(value);
        }
        /// <summary>Effects; list order does not define execution order.</summary>
        public IReadOnlyList<EffectModule> Effects => effects;
        /// <summary>Replaces the primary attack; additional instances are unaffected.</summary>
        public void SetAttack(AttackModule value) => attack = value ?? throw new ArgumentNullException(nameof(value));
        /// <summary>Adds an effect instance; multiple instances of the same type are allowed.</summary>
        public bool AddEffect(EffectModule value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            effects.Add(value);
            return true;
        }
        /// <summary>Removes a module by stable identity.</summary>
        public bool RemoveEffect(string moduleId)
        {
            bool removed = effects.RemoveAll(x => x != null && x.ModuleId == moduleId) > 0;
            if (removed) bindings.RemoveAll(b => b.SourceId == moduleId || b.TargetId == moduleId);
            return removed;
        }
        /// <summary>Removes a node, its incident bindings and its presentation entry.</summary>
        public bool RemoveModule(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId)) return false;
            if (moduleId == CoreLayoutId)
            {
                if (!HasCore) return false;
                coreRemoved = true;
                bindings.RemoveAll(b => b.SourceId == CoreLayoutId || b.TargetId == CoreLayoutId);
                boardLayout.RemoveAll(item => item.ModuleId == CoreLayoutId);
                return true;
            }
            bool removed;
            if (attack != null && attack.ModuleId == moduleId) { attack = null; removed = true; }
            else removed = additionalAttacks.RemoveAll(x => x != null && x.ModuleId == moduleId) > 0 || RemoveEffect(moduleId) || positions.RemoveAll(x => x != null && x.ModuleId == moduleId) > 0 || responses.RemoveAll(x => x != null && x.ModuleId == moduleId) > 0 || releases.RemoveAll(x => x != null && x.ModuleId == moduleId) > 0;
            if (removed) { boardLayout.RemoveAll(item => item.ModuleId == moduleId); bindings.RemoveAll(b => b.SourceId == moduleId || b.TargetId == moduleId); }
            return removed;
        }
    }
}





