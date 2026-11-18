using System;
using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbilityStudio.Editor
{
    /// <summary>Dockable, independently clocked preview that remains available during the main game's Play Mode.</summary>
    public sealed class PlaygroundWindow : EditorWindow
    {
        [SerializeField] private SkillDefinition _skill;
        [SerializeField] private bool _paused;
        private PlaygroundPreviewRenderer _preview;
        private VisualElement _viewport;
        private Image _image;
        private Label _status, _result, _feedback;
        private ObjectField _asset;
        private ToolbarToggle _move;
        private readonly List<Label> _labels = new List<Label>();
        private readonly HashSet<KeyCode> _keys = new HashSet<KeyCode>();
        private int _heldMouse = -1;
        private bool _spaceHeld, _visible = true, _transitioning, _failed;
        private double _lastUpdate;

        /// <summary>Opens the selected asset without opening or saving any scene.</summary>
        [MenuItem("Tools/技能工坊/独立 Playground 预览")]
        public static void Open() => OpenSkill(Selection.activeObject as SkillDefinition ??
            AssetDatabase.LoadAssetAtPath<SkillDefinition>(PrototypeSceneCommands.SkillPath));
        /// <summary>Shares the original asset with the board; simulation state is private.</summary>
        public static void OpenSkill(SkillDefinition skill)
        {
            var window = GetWindow<PlaygroundWindow>("Playground 预览");
            window.SetSkill(skill); window.Show();
        }
        /// <summary>Updates only existing preview windows when the board changes its asset.</summary>
        public static void FollowSkill(SkillDefinition skill)
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<PlaygroundWindow>()) window.SetSkill(skill);
        }
        private void SetSkill(SkillDefinition skill)
        {
            if (_skill == skill && _preview != null) return;
            ClearInput(); _skill = skill; _asset?.SetValueWithoutNotify(skill);
            if (_preview != null) { _preview.Simulation.Bind(skill); _preview.ClearFlashes(); }
            _move?.SetValueWithoutNotify(false); _failed = false;
        }
        private void OnEnable()
        {
            _lastUpdate = EditorApplication.timeSinceStartup;
            EditorApplication.update += UpdatePreview;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Release;
            EditorApplication.quitting += Release;
        }
        /// <summary>Creates focus-scoped UI input; no InputSystem events are injected into the game.</summary>
        public void CreateGUI()
        {
            ClearInput(); minSize = new Vector2(560, 420); rootVisualElement.Clear(); _labels.Clear();
            var toolbar = new Toolbar();
            _asset = new ObjectField { objectType = typeof(SkillDefinition), allowSceneObjects = false, value = _skill };
            _asset.style.flexGrow = 1; _asset.RegisterValueChangedCallback(evt => SetSkill(evt.newValue as SkillDefinition));
            toolbar.Add(_asset);
            toolbar.Add(new ToolbarButton(ResetWorld) { text = "重置" });
            toolbar.Add(new ToolbarButton(() => _preview?.Simulation.AddDummy()) { text = "添加木桩" });
            rootVisualElement.Add(toolbar);
            var controls = new Toolbar();
            var pause = new ToolbarToggle { text = "暂停预览", value = _paused };
            pause.RegisterValueChangedCallback(evt => { _paused = evt.newValue; ClearInput(); }); controls.Add(pause);
            _move = new ToolbarToggle { text = "移动木桩" };
            _move.RegisterValueChangedCallback(evt => { if (_preview != null) _preview.Simulation.MoveDummies = evt.newValue; }); controls.Add(_move);
            rootVisualElement.Add(controls);
            _status = new Label { style = { whiteSpace = WhiteSpace.Normal, paddingLeft = 8, paddingTop = 5, paddingBottom = 5 } };
            rootVisualElement.Add(_status);
            _viewport = new VisualElement { name = "playground-viewport", focusable = true };
            _viewport.style.flexGrow = 1; _viewport.style.minHeight = 160; _viewport.style.overflow = Overflow.Hidden;
            _viewport.style.backgroundColor = new Color(.025f, .04f, .06f);
            _image = new Image { scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
            _image.StretchToParentSize(); _viewport.Add(_image); rootVisualElement.Add(_viewport);
            _result = new Label("点击预览获得焦点 · WASD 移动 · 左键普通／右键特殊 · 按住蓄力，松开释放 · Esc 取消");
            _result.style.whiteSpace = WhiteSpace.Normal; rootVisualElement.Add(_result);
            _feedback = new Label("音效仅显示触发提示；不会混入主游戏音频。");
            _feedback.style.whiteSpace = WhiteSpace.Normal; rootVisualElement.Add(_feedback);
            _viewport.RegisterCallback<MouseMoveEvent>(evt => Aim(evt.localMousePosition));
            _viewport.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0 && evt.button != 1) return;
                _viewport.Focus(); Aim(evt.localMousePosition);
                if (_preview == null || _paused || _heldMouse >= 0 || _spaceHeld) return;
                var slot = evt.button == 1 ? SkillKind.SpecialAttack : SkillKind.NormalAttack;
                if (_skill != null && !_skill.WeaponMode && slot != _skill.Kind) return;
                var result = _preview.Simulation.Press(slot); ShowResult(result);
                if (result == CastResult.Charging) { _heldMouse = evt.button; _viewport.CaptureMouse(); }
                evt.StopPropagation();
            });
            _viewport.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (_heldMouse != evt.button) return;
                Aim(evt.localMousePosition); _heldMouse = -1;
                if (_preview != null && !_paused) ShowResult(_preview.Simulation.Release());
                _viewport.ReleaseMouse(); evt.StopPropagation();
            });
            _viewport.RegisterCallback<MouseCaptureOutEvent>(_ => { if (_heldMouse >= 0) ClearInput(); });
            _viewport.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape) { ClearInput(); evt.StopPropagation(); return; }
                if (!IsInputKey(evt.keyCode)) return;
                if (_keys.Add(evt.keyCode) && evt.keyCode == KeyCode.Space && _preview != null && !_paused && _heldMouse < 0)
                {
                    var result = _preview.Simulation.Press(_preview.Simulation.PreviewSlot);
                    _spaceHeld = result == CastResult.Charging; ShowResult(result);
                }
                evt.StopPropagation();
            });
            _viewport.RegisterCallback<KeyUpEvent>(evt =>
            {
                if (!IsInputKey(evt.keyCode)) return;
                _keys.Remove(evt.keyCode);
                if (evt.keyCode == KeyCode.Space && _spaceHeld)
                { _spaceHeld = false; if (_preview != null && !_paused) ShowResult(_preview.Simulation.Release()); }
                evt.StopPropagation();
            });
            _viewport.RegisterCallback<BlurEvent>(_ => ClearInput());
        }
        private static bool IsInputKey(KeyCode key) => key == KeyCode.W || key == KeyCode.A || key == KeyCode.S || key == KeyCode.D || key == KeyCode.Space;
        private void Aim(Vector2 point)
        { if (_preview != null && _preview.GroundPoint(point, _viewport.contentRect.size, out var aim)) _preview.Simulation.SetAim(aim); }
        private void ShowResult(CastResult result)
        {
            _result.text = result == CastResult.Charging ? "蓄力中，松开释放；Esc 或切换焦点取消" : result == CastResult.Success ? "技能已释放"
                : result == CastResult.OnCooldown ? "该攻击冷却中" : result == CastResult.Busy ? "已有攻击正在蓄力" : "当前攻击不可释放，请检查技能连接与参数。";
        }
        private void ResetWorld()
        {
            ClearInput(); _failed = false;
            if (_preview != null) { _preview.Simulation.Reset(); _preview.ClearFlashes(); }
            _move?.SetValueWithoutNotify(false);
        }
        private void ClearInput()
        {
            _keys.Clear(); _heldMouse = -1; _spaceHeld = false;
            _preview?.Simulation.Cancel();
            if (_viewport != null && _viewport.HasMouseCapture()) _viewport.ReleaseMouse();
        }
        private void UpdatePreview()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - _lastUpdate < 1.0 / 30) return;
            float delta = Mathf.Min(.05f, (float)(now - _lastUpdate)); _lastUpdate = now;
            if (_viewport == null || !_visible || _viewport.panel == null || _transitioning || _failed) return;
            float width = _viewport.contentRect.width, height = _viewport.contentRect.height;
            if (!float.IsFinite(width) || !float.IsFinite(height) || width < 8 || height < 8) return;
            try
            {
                if (_preview == null) { _preview = new PlaygroundPreviewRenderer(); _preview.Simulation.Bind(_skill); }
                var world = _preview.Simulation;
                if (!_paused)
                {
                    bool focused = focusedWindow == this && _viewport.focusController.focusedElement == _viewport;
                    var movement = focused ? new Vector2((_keys.Contains(KeyCode.D) ? 1 : 0) - (_keys.Contains(KeyCode.A) ? 1 : 0),
                        (_keys.Contains(KeyCode.W) ? 1 : 0) - (_keys.Contains(KeyCode.S) ? 1 : 0)) : Vector2.zero;
                    world.Tick(delta, movement); _preview.Feedback.Tick(delta);
                }
                _image.image = _preview.Render(width, height);
                var errors = SkillValidator.Validate(_skill);
                _status.text = errors.Count > 0 ? string.Join("\n", errors) : (_paused ? "预览已暂停" : "独立运行") +
                    " · 普通冷却 " + world.Executor.CooldownRemaining(_skill, world, SkillKind.NormalAttack).ToString("0.0") +
                    "s · 特殊冷却 " + world.Executor.CooldownRemaining(_skill, world, SkillKind.SpecialAttack).ToString("0.0") +
                    "s" + (world.IsCharging ? " · 蓄力 " + (world.ChargeRatio * 100).ToString("0") + "%" : "");
                if (!string.IsNullOrEmpty(_preview.Feedback.LastMessage)) _feedback.text = _preview.Feedback.LastMessage;
                while (_labels.Count < world.Targets.Count)
                {
                    var label = new Label { pickingMode = PickingMode.Ignore };
                    label.style.position = UnityEngine.UIElements.Position.Absolute; label.style.fontSize = 11;
                    label.style.color = Color.white; label.style.backgroundColor = new Color(.04f, .04f, .04f, .85f);
                    _viewport.Add(label); _labels.Add(label);
                }
                for (int i = 0; i < _labels.Count; i++)
                {
                    var label = _labels[i]; if (i >= world.Targets.Count) { label.style.display = DisplayStyle.None; continue; }
                    var target = world.Targets[i]; var point = _preview.Camera.WorldToViewportPoint(target.Position + Vector3.up);
                    label.style.display = point.z > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    label.style.left = point.x * width - 40; label.style.top = (1 - point.y) * height;
                    label.text = $"{target.CurrentHp:0.#}/{target.MaxHp:0.#} HP" + (target.IsStunned ? $"\n眩晕 {target.StunRemaining:0.0}s" : "") +
                        (target.IsBleeding ? $"\n流血 {target.BleedRemaining:0.0}s" : "") + (target.IsSlowed ? $"\n减速 {target.SlowRemaining:0.0}s" : "");
                }
                Repaint();
            }
            catch (Exception exception) { Release(); _failed = true; _status.text = "预览失败，可点击重置重试：" + exception.Message; Debug.LogException(exception); }
        }
        private void OnLostFocus() => ClearInput();
        private void OnBecameVisible() { _visible = true; _lastUpdate = EditorApplication.timeSinceStartup; }
        private void OnBecameInvisible() { _visible = false; ClearInput(); }
        private void PlayModeChanged(PlayModeStateChange state)
        {
            _transitioning = state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode;
            if (_transitioning) Release();
            else { _failed = false; _lastUpdate = EditorApplication.timeSinceStartup; }
        }
        private void Release()
        {
            ClearInput(); if (_image != null) _image.image = null;
            _preview?.Dispose(); _preview = null; _move?.SetValueWithoutNotify(false);
            foreach (var label in _labels) label.RemoveFromHierarchy(); _labels.Clear();
        }
        private void OnDisable()
        {
            EditorApplication.update -= UpdatePreview; EditorApplication.playModeStateChanged -= PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Release; EditorApplication.quitting -= Release; Release();
        }
    }
}
