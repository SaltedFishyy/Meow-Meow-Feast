using System;
using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbilityStudio.Editor
{
    /// <summary>Isolated, manually ticked editor world using the same combat runtime as gameplay.</summary>
    public sealed class PlaygroundSimulation : IDisposable, ISkillClock, ISkillCaster, ISkillTarget, IHealable
    {
        /// <summary>Health adapter for the preview caster.</summary>
        public CombatTarget Health { get; }
        /// <inheritdoc/>
        public float Radius => Health.Radius;
        /// <inheritdoc/>
        public bool IsAlive => Health.IsAlive;
        /// <inheritdoc/>
        public float MaxHp => Health.MaxHp;
        /// <inheritdoc/>
        public void ApplyHealing(float amount, ISkillCaster source) => Health.ApplyHealing(amount, source);
        private readonly ListTargetProvider _provider = new ListTargetProvider();
        private readonly List<CombatTarget> _targets = new List<CombatTarget>();
        private readonly List<Vector3> _spawns = new List<Vector3>();
        private readonly bool _ownsScene;
        private bool _disposed;
        private readonly SkillReleaseSession _release;
        /// <summary>Input branch currently selected for previews and casts.</summary>
        public SkillKind PreviewSlot { get; private set; }
        /// <summary>Whether the isolated preview is holding a charged attack.</summary>
        public bool IsCharging => _release.IsCharging;
        /// <summary>Charge used by both execution and outlines.</summary>
        public float ChargeRatio => _release.ChargeRatio;
        /// <summary>Preview-only scene; never made the active editor scene.</summary>
        public Scene Scene { get; }
        /// <summary>Root owned by this simulation.</summary>
        public GameObject Root { get; }
        /// <summary>Original skill asset, read again on each cast.</summary>
        public SkillDefinition Skill { get; private set; }
        /// <summary>Shared gameplay executor.</summary>
        public SkillExecutor Executor { get; }
        /// <summary>Registered practice targets.</summary>
        public IReadOnlyList<CombatTarget> Targets => _targets;
        /// <inheritdoc/>
        public double Time { get; private set; }
        /// <inheritdoc/>
        public Vector3 Position { get; private set; } = new Vector3(0, -5, 0);
        /// <inheritdoc/>
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        /// <inheritdoc/>
        public int Team => 0;
        /// <summary>Current ground aim.</summary>
        public Vector3 Aim { get; private set; } = new Vector3(0, 3, 0);
        /// <summary>Whether dummies patrol, respecting statuses and displacement.</summary>
        public bool MoveDummies { get; set; }
        /// <summary>Creates a private preview scene, or uses a supplied preview-renderer scene.</summary>
        public PlaygroundSimulation(Scene scene = default)
        {
            if (scene.IsValid() && !EditorSceneManager.IsPreviewScene(scene))
                throw new ArgumentException("Playground requires a preview scene.");
            _ownsScene = !scene.IsValid(); Scene = _ownsScene ? EditorSceneManager.NewPreviewScene() : scene;
            Root = new GameObject("Ability Studio isolated playground") { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(Root, Scene);
            Health = Root.AddComponent<CombatTarget>(); Health.SetTeam(Team); Health.ResetTarget();
            Executor = new SkillExecutor(this, _provider);
            _release = new SkillReleaseSession(Executor);
            AddDummy(new Vector3(-3, 3, 0)); AddDummy(new Vector3(0, 5, 0)); AddDummy(new Vector3(3, 3, 0));
        }
        /// <summary>Switches assets and cancels old casts/statuses without changing either asset.</summary>
        public void Bind(SkillDefinition skill)
        {
            Skill = skill;
            PreviewSlot = skill == null ? SkillKind.NormalAttack : skill.WeaponMode
                ? (skill.ReleaseFor(SkillKind.NormalAttack) != null ? SkillKind.NormalAttack : SkillKind.SpecialAttack) : skill.Kind;
            Reset();
        }
        /// <summary>Adds a target without adding anything to the host scene.</summary>
        public CombatTarget AddDummy(Vector3? position = null)
        {
            if (_targets.Count >= 24) return null;
            var point = position ?? new Vector3((_targets.Count % 7 - 3) * 2, 1 + _targets.Count / 7 * 2, 0);
            var go = new GameObject("Dummy " + (_targets.Count + 1)) { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(go, Scene); go.transform.SetParent(Root.transform);
            var target = go.AddComponent<CombatTarget>(); target.ResetTarget(); go.transform.position = point;
            _targets.Add(target); _spawns.Add(point); _provider.Add(target); return target;
        }
        /// <summary>Updates aim and facing without changing position.</summary>
        public void SetAim(Vector3 point)
        {
            if (!float.IsFinite(point.x) || !float.IsFinite(point.y)) return;
            Aim = new Vector3(point.x, point.y, 0);
            var direction = Aim - Position;
            if (direction.sqrMagnitude > .0001f) Rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
        /// <summary>Casts the selected skill at current aim.</summary>
        public CastResult Press(SkillKind slot)
        {
            if (IsCharging) return CastResult.Busy;
            PreviewSlot = Skill != null && !Skill.WeaponMode ? Skill.Kind : slot;
            return _release.Press(Skill, this, Aim, PreviewSlot);
        }
        /// <summary>Releases a held attack with the current pointer aim.</summary>
        public CastResult Release() => _release.Release(Aim);
        /// <summary>Cancels input without spending cooldown or affecting other worlds.</summary>
        public void Cancel() => _release.Cancel();
        /// <summary>Advances one deterministic time step. Input is a normalized XY movement intention.</summary>
        public void Tick(float delta, Vector2 movement = default)
        {
            if (_disposed || !float.IsFinite(delta) || delta <= 0) return;
            if (Skill != null && Skill.AreaGraph && !Skill.DualAttack) PreviewSlot = SkillKind.NormalAttack;
            Time += delta; _release.Tick(delta);
            var move = Vector2.ClampMagnitude(movement, 1) * (6 * delta);
            Position = new Vector3(Mathf.Clamp(Position.x + move.x, -13, 13), Mathf.Clamp(Position.y + move.y, -10, 10), 0);
            SetAim(Aim);
            Executor.Tick(delta);
            for (int i = 0; i < _targets.Count; i++)
            {
                var target = _targets[i]; target.Tick(delta);
                if (MoveDummies)
                {
                    float direction = Math.Sin(Time * .5 + i) >= 0 ? 1 : -1;
                    target.MoveVoluntarily(Vector3.right * (direction * 2 * delta));
                }
            }
        }
        /// <summary>Clears cooldowns, projectiles and statuses, restoring all spawn positions.</summary>
        public void Reset()
        {
            Health.ResetTarget();
            Cancel(); Executor.Reset(); Time = 0; Position = new Vector3(0, -5, 0); MoveDummies = false;
            for (int i = 0; i < _targets.Count; i++) { _targets[i].ResetTarget(); _targets[i].transform.position = _spawns[i]; }
            SetAim(new Vector3(0, 3, 0));
        }
        /// <summary>Releases preview objects and owned scene. Safe to call more than once.</summary>
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; Cancel(); Executor.Reset();
            if (Root != null) UnityEngine.Object.DestroyImmediate(Root);
            _targets.Clear(); _spawns.Clear();
            if (_ownsScene && Scene.IsValid()) EditorSceneManager.ClosePreviewScene(Scene);
        }
    }
}
