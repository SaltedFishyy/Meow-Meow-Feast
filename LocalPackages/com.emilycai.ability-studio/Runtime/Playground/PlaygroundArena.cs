using System.Collections.Generic;
using System.Linq;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbilityStudio.Playground
{
    /// <summary>Optional arena presentation and dummy controls, with no skill-specific behaviour.</summary>
    public sealed class PlaygroundArena : MonoBehaviour
    {
        [SerializeField] private PlaygroundPlayer _player;
        [SerializeField] private SkillRuntimeHost _runtime;
        [SerializeField] private Camera _camera;
        [SerializeField] private Material _dummyMaterial;
        [SerializeField] private Material _shotMaterial;
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private string _title = "技能工坊 / 测试场景";
        private readonly List<CombatTarget> _dummies = new List<CombatTarget>();
        private readonly List<DummyView> _dummyViews = new List<DummyView>();
        private readonly Dictionary<SkillProjectile, GameObject> _shots = new Dictionary<SkillProjectile, GameObject>();
        private readonly List<SkillProjectile> _expired = new List<SkillProjectile>();
        private PanelSettings _panel;
        private Font _chineseFont;
        private UIDocument _document;
        private Label _cooldown, _message, _skillTitle, _instructions;
        private VisualElement _overlay;
        private ProgressBar _cooldownBar;
        private LineRenderer _aimLine;
        private readonly List<LineRenderer> _positionPreviews = new List<LineRenderer>();
        private LineRenderer _coreEndpointPreview;
        private readonly List<LineRenderer> _conePreviews = new List<LineRenderer>();
        private readonly List<LineRenderer> _tipPreviews = new List<LineRenderer>();
        private readonly List<LineRenderer> _rectanglePreviews = new List<LineRenderer>();
        private readonly List<LineRenderer> _areaPreviews = new List<LineRenderer>();
        private readonly List<(LineRenderer line, float until)> _areaFlashes = new List<(LineRenderer, float)>();
        private Vector3 _playerSpawn;
        private float _messageUntil;
        private int _addedCount;
        private Toggle _moveDummies;
        /// <summary>Enables optional moving targets for status playtesting.</summary>
        public bool MoveDummies { get; set; }
        /// <summary>Advances optional patrol using the shared voluntary movement adapter.</summary>
        public void TickDummyMovement(float deltaTime)
        {
            if (!MoveDummies || !float.IsFinite(deltaTime) || deltaTime <= 0) return;
            foreach (var view in _dummyViews)
            {
                if (view.Target.Position.x >= 10) view.MoveDirection = -1;
                else if (view.Target.Position.x <= -10) view.MoveDirection = 1;
                view.Target.MoveVoluntarily(Vector3.right * (view.MoveDirection * 2 * deltaTime));
            }
        }
        private sealed class DummyView
        {
            internal CombatTarget Target;
            internal float MoveDirection = 1;
            internal Label Label;
            internal Label Damage;
            internal LineRenderer Tether;
            internal float DamageUntil;
            internal System.Action<float> OnDamage;
        }
        /// <summary>Targets available to tests and gameplay integrations.</summary>
        public IReadOnlyList<CombatTarget> Dummies => _dummies;
        /// <summary>Scene's player adapter.</summary>
        public PlaygroundPlayer Player => _player;
        /// <summary>Scene's shared runtime.</summary>
        public SkillRuntimeHost Runtime => _runtime;
        /// <summary>Assigns scene references during authoring.</summary>
        public void Configure(PlaygroundPlayer player, SkillRuntimeHost runtime, Camera camera,
            Material dummyMaterial, Material shotMaterial, Material lineMaterial, string title)
        { _player = player; _runtime = runtime; _camera = camera; _dummyMaterial = dummyMaterial;
          _shotMaterial = shotMaterial; _lineMaterial = lineMaterial; _title = title; }
        private void Start()
        {
            _playerSpawn = _player.Position;
            BuildHud();
            _aimLine = CreateLine("Aim range", 0.035f, new Color(0.25f, 0.85f, 1, 0.45f));
            _player.CastAttempted += OnCast;
            _runtime.Executor.AreaCast += OnAreaCast;
            _runtime.Executor.RectangleCast += OnRectangleCast;
            _runtime.Executor.ConeCast += OnConeCast;
            var feedback = GetComponent<PlaygroundHitFeedback>();
            if (feedback == null) feedback = gameObject.AddComponent<PlaygroundHitFeedback>();
            feedback.Bind(_runtime.Executor, _camera);
            AddDummy(new Vector3(0, 5, 0));
            AddDummy(new Vector3(-5, 3, 0));
            AddDummy(new Vector3(5, 6, 0));
        }
        /// <summary>Adds a target at a deterministic free arena position.</summary>
        public void AddDummy()
        {
            if (_dummies.Count >= 24) { ShowMessage("最多可添加 24 个木桩。"); return; }
            int index = _addedCount++;
            AddDummy(new Vector3(-9 + (index % 7) * 3, 8 - (index / 7) * 3, 0));
        }
        /// <summary>Adds a target using the shared CombatTarget adapter.</summary>
        public CombatTarget AddDummy(Vector3 position)
        {
            var root = new GameObject("Dummy " + (_dummies.Count + 1));
            root.transform.SetParent(transform); root.transform.position = position;
            var pivot = new GameObject("Visual recoil pivot"); pivot.transform.SetParent(root.transform, false);
            var recoil = pivot.AddComponent<PlaygroundRecoil>();
            var body = GameObject.CreatePrimitive(PrimitiveType.Quad);
            body.name = "Body"; body.transform.SetParent(pivot.transform, false);
            body.transform.localPosition = Vector3.zero; body.transform.localScale = new Vector3(1.2f, 1.2f, 1);
            // Keep physical bounds on the stationary target root, separate from the leaning model.
            var bodyCollider = body.GetComponent<Collider>(); bodyCollider.enabled = false; Destroy(bodyCollider);
            var collider = root.AddComponent<CircleCollider2D>(); collider.radius = .6f; collider.isTrigger = true;
            body.GetComponent<Renderer>().sharedMaterial = _dummyMaterial;
            var target = root.AddComponent<CombatTarget>();
            GetComponent<PlaygroundHitFeedback>().RegisterRecoil(target, recoil);
            _runtime.Register(target); _dummies.Add(target);
            var view = new DummyView { Target = target, Label = new Label(), Damage = new Label(),
                Tether = CreateLine("Pull feedback", 0.07f, new Color(0.3f, 1, 0.85f)) };
            foreach (var label in new[] { view.Label, view.Damage })
            {
                label.pickingMode = PickingMode.Ignore; label.style.position = UnityEngine.UIElements.Position.Absolute;
                label.style.width = 160; label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.color = Color.white; label.style.fontSize = 15; _overlay.Add(label);
            }
            view.Damage.style.color = new Color(1, 0.65f, 0.25f); view.Damage.style.fontSize = 22;
            view.OnDamage = amount => { view.Damage.text = "−" + amount.ToString("0.#"); view.DamageUntil = Time.time + 1.2f; };
            target.Damaged += view.OnDamage; _dummyViews.Add(view);
            return target;
        }
        /// <summary>Restores dummy HP/placement, player position, cooldown and projectiles.</summary>
        public void ResetArena()
        {
            _player.CancelCharge(); _runtime.Executor.Reset(); _player.transform.position = _playerSpawn;
            MoveDummies = false; _moveDummies?.SetValueWithoutNotify(false);
            foreach (var view in _dummyViews) view.MoveDirection = 1;
            foreach (var flash in _areaFlashes) Destroy(flash.line.gameObject);
            _areaFlashes.Clear();
            foreach (var dummy in _dummies) dummy.ResetTarget();
            foreach (var view in _dummyViews) view.DamageUntil = 0;
            ShowMessage("重置完成，可以施法");
        }
        private void BuildHud()
        {
            _panel = ScriptableObject.CreateInstance<PanelSettings>();
            _panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("AbilityStudioTheme");
            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize; _panel.referenceResolution = new Vector2Int(1280, 720);
            _panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight; _panel.match = 0.5f;
            _document = gameObject.AddComponent<UIDocument>(); _document.panelSettings = _panel;
            var root = _document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            var available = Font.GetOSInstalledFontNames();
            var family = new[] { "Microsoft YaHei", "Microsoft YaHei UI", "SimHei", "Noto Sans CJK SC", "PingFang SC" }.FirstOrDefault(available.Contains);
            _chineseFont = family == null ? null : Font.CreateDynamicFontFromOSFont(family, 18);
            root.style.unityFont = _chineseFont != null ? _chineseFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            root.style.color = Color.white;
            _overlay = new VisualElement { pickingMode = PickingMode.Ignore };
            _overlay.style.position = UnityEngine.UIElements.Position.Absolute;
            _overlay.style.left = _overlay.style.right = _overlay.style.top = _overlay.style.bottom = 0;
            root.Add(_overlay);
            var panel = new VisualElement();
            panel.style.position = UnityEngine.UIElements.Position.Absolute; panel.style.left = 20; panel.style.top = 20;
            panel.style.width = 310; panel.style.paddingLeft = panel.style.paddingRight = 18;
            panel.style.paddingTop = panel.style.paddingBottom = 16;
            panel.style.backgroundColor = new Color(0.035f, 0.06f, 0.10f, 0.94f);
            panel.style.borderLeftWidth = 3; panel.style.borderLeftColor = new Color(0.2f, 0.9f, 0.85f);
            panel.RegisterCallback<PointerEnterEvent>(_ => _player.PointerOverHud = true);
            panel.RegisterCallback<PointerLeaveEvent>(_ => _player.PointerOverHud = false);
            root.Add(panel);
            AddLabel(panel, _title == "ABILITY STUDIO / PLAYGROUND" ? "技能工坊 / 测试场景" : _title == "ABILITY STUDIO / GAMEPLAY" ? "技能工坊 / 游戏场景" : _title, 12, new Color(0.35f, 0.9f, 0.85f));
            _skillTitle = AddLabel(panel, _player.Skill.DisplayName, 27, Color.white);
            _instructions = AddLabel(panel, "WASD 移动\n鼠标瞄准\n左键 / 空格施法", 15, new Color(0.7f, 0.8f, 0.9f));
            _cooldown = AddLabel(panel, "就绪", 18, Color.white);
            _cooldownBar = new ProgressBar { lowValue = 0, highValue = 1, value = 1 };
            _cooldownBar.style.height = 8; _cooldownBar.style.marginBottom = 12; panel.Add(_cooldownBar);
            var background = _cooldownBar.Q(className: "unity-progress-bar__background");
            if (background != null) { background.style.height = 8; background.style.backgroundColor = new Color(0.12f, 0.18f, 0.25f); }
            var progress = _cooldownBar.Q(className: "unity-progress-bar__progress");
            if (progress != null) { progress.style.height = 8; progress.style.backgroundColor = new Color(0.2f, 0.9f, 0.85f); }
            AddButton(panel, "+ 添加木桩", AddDummy);
            AddButton(panel, "重置场地", ResetArena);
            _moveDummies = new Toggle("木桩移动") { name = "move-dummies" };
            _moveDummies.RegisterValueChangedCallback(evt => MoveDummies = evt.newValue);
            panel.Add(_moveDummies);
            AddLabel(panel, "在技能画板或检查器中修改模块。\n点击“保存技能”保留试玩中的修改。", 13, new Color(0.65f, 0.75f, 0.85f));
            _message = AddLabel(panel, "瞄准木桩测试技能。", 14, Color.white);
            _message.name = "skill-composition";
            _cooldown.name = "skill-readiness";
        }
        private static Label AddLabel(VisualElement parent, string text, int size, Color color)
        {
            var label = new Label(text); label.style.fontSize = size; label.style.color = color;
            label.style.whiteSpace = WhiteSpace.Normal; label.style.marginBottom = 12;
            parent.Add(label); return label;
        }
        private static void AddButton(VisualElement parent, string text, System.Action action)
        {
            var button = new Button(action) { text = text };
            button.style.height = 36; button.style.marginBottom = 8;
            button.style.backgroundColor = new Color(0.12f, 0.24f, 0.31f); button.style.color = Color.white;
            button.style.unityTextAlign = TextAnchor.MiddleCenter; button.style.fontSize = 14;
            parent.Add(button);
        }
        private LineRenderer CreateLine(string label, float width, Color color)
        {
            var go = new GameObject(label); go.transform.SetParent(transform);
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = _lineMaterial;
            line.positionCount = 2; line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color; line.enabled = false;
            return line;
        }
        private void OnCast(CastResult result)
        {
            var errors = SkillValidator.Validate(_player.Skill);
            ShowMessage(result == CastResult.Charging ? "按住蓄力，松开释放；Esc 取消" : result == CastResult.Stunned ? "眩晕中，无法施法" : result == CastResult.Success ? "技能已释放" : result == CastResult.OnCooldown ?
                "技能冷却中，重置场地可立即重试" : errors.Count > 0 ? string.Join("\n", errors) : "请瞄准离玩家更远的位置。");
        }
        private static void DrawCircle(LineRenderer line, Vector3 center, float radius)
        {
            line.loop = true; line.positionCount = 64; line.enabled = true;
            for (int i = 0; i < 64; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -0.1f));
            }
        }
        private static void DrawCone(LineRenderer line, Vector3 origin, Vector3 direction, float range, float angle)
        {
            int segments = Mathf.Max(2, Mathf.CeilToInt(angle / 5));
            bool fullCircle = angle >= 360;
            line.loop = true; line.enabled = true;
            line.positionCount = fullCircle ? segments : segments + 2;
            if (!fullCircle) line.SetPosition(0, origin + Vector3.back * 0.1f);
            for (int i = 0; i < (fullCircle ? segments : segments + 1); i++)
                line.SetPosition(fullCircle ? i : i + 1, ConeGeometry.ArcPoint(origin, direction, range, angle, (float)i / segments) + Vector3.back * 0.1f);
        }
        private void OnConeCast(Vector3 origin, Vector3 direction, float range, float angle)
        {
            var line = CreateLine("Cone impact", 0.15f, new Color(1, 0.65f, 0.2f));
            DrawCone(line, origin, direction, range, angle);
            _areaFlashes.Add((line, Time.time + 0.4f));
        }
        private static void DrawRectangle(LineRenderer line, Vector3 origin, Vector3 direction, float length, float width)
        {
            line.loop = true; line.positionCount = 4; line.enabled = true;
            for (int i = 0; i < 4; i++) line.SetPosition(i, RectangleGeometry.Corner(origin, direction, length, width, i) + Vector3.back * 0.1f);
        }
        private void OnRectangleCast(Vector3 origin, Vector3 direction, float length, float width)
        {
            var line = CreateLine("Rectangle impact", 0.15f, new Color(1, 0.65f, 0.2f));
            DrawRectangle(line, origin, direction, length, width);
            _areaFlashes.Add((line, Time.time + 0.4f));
        }
        private void OnAreaCast(Vector3 center, float radius)
        {
            var line = CreateLine("AoE impact", 0.15f, new Color(1, 0.65f, 0.2f));
            DrawCircle(line, center, radius);
            _areaFlashes.Add((line, Time.time + 0.4f));
        }
        private void ShowMessage(string message) { _message.text = message; _messageUntil = Time.time + 3; }
        private void LateUpdate()
        {
            if (_document == null) return;
            _skillTitle.text = _player.Skill.DisplayName;
            string castButton = _player.Skill.Kind == SkillKind.SpecialAttack ? "右键" : "左键";
            string slot = _player.Skill.Kind == SkillKind.SpecialAttack ? "特殊攻击" : "普通攻击";
            _instructions.text = "WASD 移动 · 鼠标瞄准\n" + slot + "：" + castButton + " / 空格\n" + (_player.PreviewRelease is ChargeRelease ? "按住蓄力，松开释放 · Esc 取消" : "按下立即释放");
            if (_player.Skill.WeaponMode) _instructions.text = "WASD 移动 · 鼠标瞄准\n左键普通攻击 · 右键特殊攻击\n蓄力时松开释放 · Esc 取消\n预览：" + (_player.PreviewSlot == SkillKind.NormalAttack ? "普通攻击" : "特殊攻击");
            TickDummyMovement(Time.deltaTime);
            float remaining = _runtime.Executor.CooldownRemaining(_player.Skill, _player, _player.PreviewSlot);
            var errors = SkillValidator.Validate(_player.Skill);
            _cooldown.text = errors.Count > 0 ? "技能配置无效" : _player.IsCharging ? "蓄力 " + (_player.ChargeRatio * 100).ToString("0") + "% · 松开释放" : remaining > 0 ? "冷却中  " + remaining.ToString("0.0") + " 秒" : "就绪  ·  " + _player.Skill.BaseDamage.ToString("0.#") + " 伤害";
            _cooldownBar.value = _player.IsCharging ? _player.ChargeRatio : remaining <= 0 ? 1 : 1 - Mathf.Clamp01(remaining / Mathf.Max(0.001f, _player.Skill.Cooldown));
            if (_player.Skill.WeaponMode)
            {
                float normal = _runtime.Executor.CooldownRemaining(_player.Skill,_player,SkillKind.NormalAttack);
                float special = _runtime.Executor.CooldownRemaining(_player.Skill,_player,SkillKind.SpecialAttack);
                _cooldown.text = "普攻 " + (_player.Skill.ReleaseFor(SkillKind.NormalAttack) == null ? "未配置" : normal > 0 ? normal.ToString("0.0") + "秒" : "就绪") + " · 特攻 " + (_player.Skill.ReleaseFor(SkillKind.SpecialAttack) == null ? "未配置" : special > 0 ? special.ToString("0.0") + "秒" : "就绪") + (_player.IsCharging ? "\n蓄力 " + (_player.ChargeRatio*100).ToString("0") + "%" : "");
                _cooldownBar.value = _player.IsCharging ? _player.ChargeRatio : remaining <= 0 ? 1 : 1-Mathf.Clamp01(remaining/Mathf.Max(.001f,_player.PreviewRelease?.Cooldown ?? 1));
            }
            if (errors.Count > 0) _message.text = string.Join("\n", errors);
            else if (Time.time > _messageUntil)
            {
                var names = _player.PreviewAttacks.Select(a => a is CircleAttack ? "圆形范围" : a is LineThrustAttack ? "长方形" : a is ConeAttack ? "扇形" : "投射物");
                var activeEffects = _player.Skill.Effects.Where(effect => BindingRules.HasAttackTargets(_player.Skill, effect)).ToArray();
                int pulls = activeEffects.Count(effect => effect is PullEffect);
                int slows = activeEffects.Count(effect => effect is SlowEffect);
                int bleeds = activeEffects.Count(effect => effect is BleedEffect);
                int stuns = activeEffects.Count(effect => effect is StunEffect);
                int knockbacks = activeEffects.Count(effect => effect is KnockbackEffect);
                _message.text = string.Join(" + ", names) + " → 伤害" + (slows > 0 ? " + 减速" + (slows > 1 ? " × " + slows : "") : "") + (bleeds > 0 ? " + 流血" + (bleeds > 1 ? " × " + bleeds : "") : "") + (stuns > 0 ? " + 眩晕" + (stuns > 1 ? " × " + stuns : "") : "") + (knockbacks > 0 ? " + 击退" + (knockbacks > 1 ? " × " + knockbacks : "") : "") + (pulls > 0 ? " + 拉拽" + (pulls > 1 ? " × " + pulls : "") : "");
            }
            var direction = _player.AimPoint - _player.Position; direction.z = 0;
            float range = _player.PreviewAttacks.OfType<ProjectileAttack>().Select(a => a.Range).DefaultIfEmpty(0).Max();
            _aimLine.enabled = range > 0 && direction.sqrMagnitude > 0.001f;
            _aimLine.SetPosition(0, _player.Position + Vector3.back * 0.08f);
            _aimLine.SetPosition(1, _player.Position + direction.normalized * Mathf.Min(range, direction.magnitude) + Vector3.back * 0.08f);
            if (_coreEndpointPreview == null) _coreEndpointPreview = CreateLine("Skill Core endpoint", 0.06f, new Color(0.4f, 1, 0.5f));
            if (!_player.Skill.WeaponMode && _player.Skill.Bindings.Any(b => b.SourceId == SkillDefinition.CoreLayoutId))
                DrawCircle(_coreEndpointPreview, SkillPositions.Endpoint(_player.Skill, _player, _player.AimPoint), 0.15f);
            else _coreEndpointPreview.enabled = false;
            while (_positionPreviews.Count < _player.Skill.Positions.Count)
                _positionPreviews.Add(CreateLine("Position source", 0.06f, new Color(0.4f, 1, 0.5f)));
            for (int i = 0; i < _positionPreviews.Count; i++)
            {
                if (i < _player.Skill.Positions.Count && _player.Skill.Positions[i] != null)
                    DrawCircle(_positionPreviews[i], SkillPositions.Resolve(_player.Skill.Positions[i], _player.Skill, _player, _player.AimPoint), 0.15f);
                else _positionPreviews[i].enabled = false;
            }
            var rectangles = _player.PreviewAttacks.OfType<LineThrustAttack>().ToArray();
            var castOrigin = _player.Position + _player.Rotation * _player.Skill.CastOriginOffset;
            var rectangleDirection = _player.AimPoint - castOrigin; rectangleDirection.z = 0;
            bool validDirection = float.IsFinite(rectangleDirection.sqrMagnitude) && rectangleDirection.sqrMagnitude >= 0.0001f;
            while (_rectanglePreviews.Count < rectangles.Length)
                _rectanglePreviews.Add(CreateLine("Rectangle aim bounds", 0.045f, new Color(0.25f, 0.85f, 1, 0.7f)));
            while (_tipPreviews.Count < rectangles.Length)
                _tipPreviews.Add(CreateLine("Rectangle tip preview", .075f, new Color(1, .8f, .2f)));
            for (int i = 0; i < _rectanglePreviews.Count; i++)
            {
                if (i < rectangles.Length && validDirection && float.IsFinite(rectangles[i].Length) && rectangles[i].Length > 0 && float.IsFinite(rectangles[i].Width) && rectangles[i].Width > 0)
                {
                    var rect = rectangles[i]; var origin = SkillPositions.AttackPosition(_player.Skill, rect, _player, _player.AimPoint);
                    float length = SkillChargeGeometry.Length(_player.Skill, rect, _player.ChargeRatio);
                    DrawRectangle(_rectanglePreviews[i], origin, rectangleDirection.normalized, length, rect.Width);
                    bool hasZone = _player.Skill.Bindings.Any(b => b.SourceId == rect.ModuleId && (b.SourcePort == BindingRules.TipTargets || b.SourcePort == BindingRules.BodyTargets));
                    if (hasZone) DrawRectangle(_tipPreviews[i], SkillChargeGeometry.TipOrigin(origin, rectangleDirection.normalized, length, rect.TipLength), rectangleDirection.normalized, Mathf.Min(length, rect.TipLength), rect.Width);
                    else _tipPreviews[i].enabled = false;
                }
                else { _rectanglePreviews[i].enabled = false; _tipPreviews[i].enabled = false; }
            }
            var cones = _player.PreviewAttacks.OfType<ConeAttack>().ToArray();
            while (_conePreviews.Count < cones.Length)
                _conePreviews.Add(CreateLine("Cone aim bounds", 0.045f, new Color(0.25f, 0.85f, 1, 0.7f)));
            for (int i = 0; i < _conePreviews.Count; i++)
            {
                if (i < cones.Length && validDirection && float.IsFinite(cones[i].Range) && cones[i].Range > 0 && float.IsFinite(cones[i].AngleDegrees) && cones[i].AngleDegrees > 0 && cones[i].AngleDegrees <= 360)
                    DrawCone(_conePreviews[i], SkillPositions.AttackPosition(_player.Skill, cones[i], _player, _player.AimPoint), rectangleDirection.normalized, SkillChargeGeometry.Range(_player.Skill, cones[i], _player.ChargeRatio), cones[i].AngleDegrees);
                else _conePreviews[i].enabled = false;
            }
            var areas = _player.PreviewAttacks.OfType<CircleAttack>().ToArray();
            while (_areaPreviews.Count < areas.Length)
                _areaPreviews.Add(CreateLine("AoE aim radius", 0.045f, new Color(0.25f, 0.85f, 1, 0.7f)));
            for (int i = 0; i < _areaPreviews.Count; i++)
            {
                _areaPreviews[i].enabled = i < areas.Length;
                if (i < areas.Length && float.IsFinite(areas[i].Radius) && areas[i].Radius > 0)
                    DrawCircle(_areaPreviews[i], SkillPositions.AttackPosition(_player.Skill, areas[i], _player, _player.AimPoint), SkillChargeGeometry.Radius(_player.Skill, areas[i], _player.ChargeRatio));
                else _areaPreviews[i].enabled = false;
            }
            for (int i = _areaFlashes.Count - 1; i >= 0; i--)
                if (Time.time >= _areaFlashes[i].until)
                { Destroy(_areaFlashes[i].line.gameObject); _areaFlashes.RemoveAt(i); }
            foreach (var shot in _runtime.Executor.Projectiles)
            {
                if (!_shots.TryGetValue(shot, out var visual))
                {
                    visual = GameObject.CreatePrimitive(PrimitiveType.Quad); visual.name = "Skill projectile";
                    visual.transform.SetParent(transform); visual.transform.localScale = Vector3.one * shot.Radius * 2;
                    visual.GetComponent<Collider>().enabled = false; Destroy(visual.GetComponent<Collider>()); visual.GetComponent<Renderer>().sharedMaterial = _shotMaterial;
                    _shots.Add(shot, visual);
                }
                visual.transform.position = shot.Position + Vector3.back * 0.2f;
                var tint = new MaterialPropertyBlock();
                tint.SetColor("_BaseColor", shot.Returning ? new Color(1, .65f, .2f) : new Color(.3f, 1, .75f));
                visual.GetComponent<Renderer>().SetPropertyBlock(tint);
            }
            _expired.Clear();
            foreach (var pair in _shots) if (pair.Key.Finished) { Destroy(pair.Value); _expired.Add(pair.Key); }
            foreach (var shot in _expired) _shots.Remove(shot);
            foreach (var view in _dummyViews)
            {
                var target = view.Target;
                var screen = _camera.WorldToScreenPoint(target.Position + Vector3.up * 1.1f);
                var point = RuntimePanelUtils.ScreenToPanel(_document.rootVisualElement.panel, new Vector2(screen.x, Screen.height - screen.y));
                view.Label.style.left = point.x - 80; view.Label.style.top = point.y;
                view.Label.text = target.IsAlive ? target.CurrentHp.ToString("0.#") + " / " + target.MaxHp.ToString("0.#") + " 生命" + (target.IsSlowed ? "\n减速 " + ((1 - target.MovementSpeedMultiplier) * 100).ToString("0") + "% " + target.SlowRemaining.ToString("0.0") + " 秒" : "") + (target.IsBleeding ? "\n流血 " + target.BleedRemaining.ToString("0.0") + " 秒" : "") + (target.IsStunned ? "\n眩晕 " + target.StunRemaining.ToString("0.0") + " 秒" : "") + (target.IsMoving ? "\n位移中" : "") : "已击败";
                view.Label.style.color = target.IsAlive ? (target.IsStunned ? new Color(1, 0.85f, 0.25f) : target.IsBleeding ? new Color(1, 0.5f, 0.4f) : Color.white) : new Color(1, 0.5f, 0.4f);
                view.Damage.style.left = point.x - 80; view.Damage.style.top = point.y - 30 - (1.2f - (view.DamageUntil - Time.time)) * 20;
                view.Damage.style.display = Time.time < view.DamageUntil ? DisplayStyle.Flex : DisplayStyle.None;
                view.Tether.enabled = target.IsMoving;
                view.Tether.SetPosition(0, target.DisplacementDestination + Vector3.back * 0.1f);
                view.Tether.SetPosition(1, target.Position + Vector3.back * 0.1f);
            }
        }
        private void OnDestroy()
        {
            if (_player != null) _player.CastAttempted -= OnCast;
            if (_runtime != null) { _runtime.Executor.AreaCast -= OnAreaCast; _runtime.Executor.RectangleCast -= OnRectangleCast; _runtime.Executor.ConeCast -= OnConeCast; }
            foreach (var view in _dummyViews)
                if (view.Target != null) { view.Target.Damaged -= view.OnDamage; if (_runtime != null) _runtime.Unregister(view.Target); }
            if (_panel != null) Destroy(_panel);
            if (_chineseFont != null) Destroy(_chineseFont);
        }
    }
}







































