using System.Collections.Generic;
using System.Linq;
using AbilityStudio.Model;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Canvas = UnityEditor.Experimental.GraphView.GraphView;

namespace AbilityStudio.Editor.Board.GraphView
{
    /// <summary>GraphView adapter for module composition, with typed data bindings and backwards-compatible defaults.</summary>
    public sealed class CompositionBoardView : Canvas, IBoardView
    {
        private sealed class BindingPort
        {
            internal readonly string ModuleId, Input, SourcePort;
            internal BindingPort(string moduleId, string input = null, string sourcePort = "") { ModuleId = moduleId; Input = input; SourcePort = sourcePort; }
        }
        private SkillDefinition _skill;
        private string _signature;
        private bool _needsFrame;
        private readonly IVisualElementScheduledItem _watch;
        private readonly DotGridBackground _grid;
        private readonly EdgeCutManipulator _edgeCutter;
        private readonly VisualElement _help;
        private readonly Label _helpTitle, _helpBody;
        private Vector2 _helpPosition;
        /// <inheritdoc/>
        public VisualElement Element => this;
        /// <inheritdoc/>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_edgeCutter != null && _edgeCutter.SuppressMenu) return;
            evt.menu.AppendAction("删除选中", _ => DeleteSelectedModules());
            evt.menu.AppendAction("显示全部", _ => FrameAllModules());
        }
        /// <summary>Creates a canvas without owning or cloning skill data.</summary>
        public CompositionBoardView()
        {
            style.flexGrow = 1;
            _grid = new DotGridBackground(this);
            hierarchy.Insert(0, _grid); _grid.StretchToParentSize();
            _help = new VisualElement { name = "node-hover-help", pickingMode = PickingMode.Ignore };
            _help.style.position = Position.Absolute; _help.style.width = 340;
            _help.style.backgroundColor = new Color(.12f, .12f, .12f, 1);
            _help.style.paddingLeft = _help.style.paddingRight = 14;
            _help.style.paddingTop = _help.style.paddingBottom = 12;
            _help.style.borderTopLeftRadius = _help.style.borderTopRightRadius = 6;
            _help.style.borderBottomLeftRadius = _help.style.borderBottomRightRadius = 6;
            _helpTitle = new Label { pickingMode = PickingMode.Ignore };
            _helpTitle.style.unityFontStyleAndWeight = FontStyle.Bold; _helpTitle.style.marginBottom = 8;
            _helpBody = new Label { pickingMode = PickingMode.Ignore };
            _helpBody.style.whiteSpace = WhiteSpace.Normal; _helpBody.style.fontSize = 13;
            _help.Add(_helpTitle); _help.Add(_helpBody); hierarchy.Add(_help);
            _help.style.display = DisplayStyle.None;
            _help.RegisterCallback<GeometryChangedEvent>(_ => PlaceHelp());
            RegisterCallback<DetachFromPanelEvent>(_ => HideHelp());
            viewTransformChanged += RepaintGrid;
            SetupZoom(0.3f, 1.5f);
            this.AddManipulator(new ContentDragger());
            _edgeCutter = new EdgeCutManipulator(CutEdges);
            this.AddManipulator(_edgeCutter);
            // ContentDragger updates the content transform during a drag, but only
            // raises viewTransformChanged on release. Repaint the sibling grid as we move.
            RegisterCallback<MouseMoveEvent>(OnPanMove, TrickleDown.TrickleDown);
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            graphViewChanged = OnGraphChanged;
            RegisterCallback<DragUpdatedEvent>(evt =>
            {
                var type = DragAndDrop.GetGenericData(ModuleLibraryView.DragTypeKey) as System.Type;
                if (type == null) return;
                DragAndDrop.visualMode = CanDrop(type) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            });
            RegisterCallback<DragPerformEvent>(evt =>
            {
                var type = DragAndDrop.GetGenericData(ModuleLibraryView.DragTypeKey) as System.Type;
                if (type == null) return;
                if (CanDrop(type))
                {
                    var position = contentViewContainer.WorldToLocal(evt.mousePosition);
                    if (SkillAssetCommands.AddModule(_skill, type, position))
                    {
                        DragAndDrop.AcceptDrag();
                        Rebuild();
                    }
                }
                evt.StopPropagation();
            });
            Undo.undoRedoPerformed += Rebuild;
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (_needsFrame && evt.newRect.width > 0 && evt.newRect.height > 0)
                {
                    _needsFrame = false;
                    schedule.Execute(FrameAllModules).ExecuteLater(100);
                }
            });
            _watch = schedule.Execute(() => { if (Signature() != _signature) Rebuild(); }).Every(200);
        }
        private bool CanDrop(System.Type type) =>
            DragAndDrop.GetGenericData(ModuleLibraryView.DragSkillKey) as SkillDefinition == _skill &&
            ModuleCatalog.CanAdd(_skill, type);
        private void RepaintGrid(Canvas _) { _grid.MarkDirtyRepaint(); HideHelp(); }
        private void OnPanMove(MouseMoveEvent evt)
        {
            if ((evt.pressedButtons & (1 << (int)MouseButton.MiddleMouse)) != 0 ||
                (evt.altKey && (evt.pressedButtons & 1) != 0)) RepaintGrid(this);
        }
        private void HideHelp() => _help.style.display = DisplayStyle.None;
        private void PlaceHelp()
        {
            float width = float.IsNaN(_help.layout.width) ? 340 : _help.layout.width;
            float height = float.IsNaN(_help.layout.height) ? 0 : _help.layout.height;
            float x = _helpPosition.x + 18, y = _helpPosition.y + 18;
            if (x + width > layout.width - 8) x = _helpPosition.x - width - 18;
            if (y + height > layout.height - 8) y = _helpPosition.y - height - 18;
            _help.style.left = Mathf.Clamp(x, 8, Mathf.Max(8, layout.width - width - 8));
            _help.style.top = Mathf.Clamp(y, 8, Mathf.Max(8, layout.height - height - 8));
        }
        private void ShowHelp(string title, string body, Vector2 mouse)
        {
            _helpTitle.text = title; _helpBody.text = body;
            _helpPosition = this.WorldToLocal(mouse);
            _help.style.display = DisplayStyle.Flex; _help.BringToFront(); PlaceHelp();
        }
        /// <inheritdoc/>
        public void Bind(SkillDefinition skill)
        {
            SkillAssetCommands.MigrateEndpoint(skill);
            _skill = skill; Rebuild(); _needsFrame = true;
            if (layout.width > 0 && layout.height > 0)
            {
                _needsFrame = false;
                schedule.Execute(FrameAllModules).ExecuteLater(100);
            }
        }
        /// <inheritdoc/>
        public void FrameAllModules() { if (nodes.Any()) FrameAll(); }
        /// <inheritdoc/>
        public void DeleteSelectedModules()
        {
            bool changed = RemoveBindings(selection.OfType<Edge>());
            changed |= SkillAssetCommands.RemoveModules(_skill, selection.OfType<Node>().Select(node => (string)node.userData));
            if (changed) Rebuild();
        }
        private string Signature() => _skill == null ? "" : _skill.GetEntityId() + ":" + _skill.WeaponMode + ":" + _skill.IntegratedAttacks + ":" + string.Join(",",_skill.Attacks.Select(a=>a.ReleaseMode)) + ":" +
            string.Join(",", _skill.Modules.Select(m => m?.ModuleId)) + ":" +
            string.Join(",", _skill.Bindings.Select(b => b.SourceId + ">" + b.TargetId + ":" + b.Input + ":" + b.SourcePort));
        private void Rebuild()
        {
            HideHelp();
            ClearSelection();
            foreach (var element in graphElements.ToList()) RemoveElement(element);
            _signature = Signature();
            if (_skill == null) return;
            var core = AddModule("技能基础", SkillDefinition.CoreLayoutId, "", new Vector2(50, 140), new Color(0.12f, 0.46f, 0.56f));
            var moduleNodes = new Dictionary<string, Node>();
            for (int i = 0; i < _skill.Positions.Count; i++)
            {
                var source = _skill.Positions[i]; if (source == null) continue;
                moduleNodes[source.ModuleId] = AddModule(ModuleCatalog.Metadata(source.GetType()).DisplayName, source.ModuleId,
                    "positions.Array.data[" + i + "]", new Vector2(50, 420 + i * 180), new Color(0.18f, 0.48f, 0.35f));
            }
            int attackIndex = 0;
            foreach (var entry in ModuleCatalog.AttackEntries(_skill))
            {
                if (entry.module == null) continue;
                var title = ModuleCatalog.Metadata(entry.module.GetType())?.DisplayName ?? entry.module.GetType().Name;
                var node = AddModule(title, entry.module.ModuleId, entry.path, new Vector2(500, 40 + attackIndex++ * 240), new Color(0.42f, 0.31f, 0.6f));
                moduleNodes[entry.module.ModuleId] = node;

            }
            for (int i = 0; i < _skill.Effects.Count; i++)
                if (_skill.Effects[i] is EffectModule effect)
                {
                    var node = AddModule(ModuleCatalog.Metadata(effect.GetType())?.DisplayName ?? effect.GetType().Name, effect.ModuleId,
                        "effects.Array.data[" + i + "]", new Vector2(920, 40 + i * 260),
                        effect is HitFeedbackEffect ? new Color(0.68f, 0.20f, 0.40f) : new Color(0.65f, 0.35f, 0.15f));
                    moduleNodes[effect.ModuleId] = node;

                }
            for (int i = 0; i < _skill.Responses.Count; i++)
                if (_skill.Responses[i] is HitResponseModule response)
                    moduleNodes[response.ModuleId] = AddModule(ModuleCatalog.Metadata(response.GetType()).DisplayName, response.ModuleId,
                        "responses.Array.data[" + i + "]", new Vector2(1360, 40 + i * 200), new Color(.2f, .48f, .42f));
            for (int i = 0; !_skill.IntegratedAttacks && i < _skill.Releases.Count; i++)
                if (_skill.Releases[i] is ReleaseModule release)
                    moduleNodes[release.ModuleId] = AddModule(ModuleCatalog.Metadata(release.GetType()).DisplayName, release.ModuleId,
                        "releases.Array.data[" + i + "]", new Vector2(-400, 100 + i * 220), new Color(.38f, .42f, .65f));
            var inputs = new Dictionary<(string, string), Port>();
            var outputs = new Dictionary<string, Port>();
            if (_skill.WeaponMode)
            {
                foreach (var slot in new[] { BindingRules.Normal, BindingRules.Special })
                {
                    var port = core.InstantiatePort(Orientation.Horizontal,Direction.Output,Port.Capacity.Single,typeof(SkillKind));
                    port.portName = slot == BindingRules.Normal ? "普通攻击" : "特殊攻击";
                    port.name = SkillDefinition.CoreLayoutId + ":output:" + slot;
                    port.userData = new BindingPort(SkillDefinition.CoreLayoutId,null,slot);
                    core.outputContainer.Add(port); outputs[SkillDefinition.CoreLayoutId + ":" + slot] = port;
                }
            }
            else
            {
            var endpointPort = core.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Vector3));
            endpointPort.portName = "技能终点"; endpointPort.name = SkillDefinition.CoreLayoutId + ":output";
            endpointPort.userData = new BindingPort(SkillDefinition.CoreLayoutId);
            core.outputContainer.Add(endpointPort); outputs[SkillDefinition.CoreLayoutId] = endpointPort;
            var releaseInput = core.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ReleaseModule));
            releaseInput.portName = "释放方式"; releaseInput.name = SkillDefinition.CoreLayoutId + ":release";
            releaseInput.tooltip = "未连接时默认瞬发"; releaseInput.userData = new BindingPort(SkillDefinition.CoreLayoutId, BindingRules.Release);
            core.inputContainer.Add(releaseInput); inputs[(SkillDefinition.CoreLayoutId, BindingRules.Release)] = releaseInput;
            }
            core.RefreshExpandedState(); core.RefreshPorts();
            foreach (var pair in moduleNodes)
            {
                var id = pair.Key; var node = pair.Value; var module = _skill.FindModule(id);
                void Input(string key, string label, string fallback, System.Type type)
                {
                    var port = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type);
                    port.portName = label;
                    port.name = id + ":" + key; port.userData = new BindingPort(id, key);
                    port.tooltip = "断开后：" + fallback;
                    node.inputContainer.Add(port); inputs[(id, key)] = port;
                }
                void Output(string label, System.Type type, string suffix = "")
                {
                    var port = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
                    port.portName = label; port.name = id + ":output" + suffix; port.userData = new BindingPort(id, null, suffix == ":response" ? "" : suffix.TrimStart(':'));
                    node.outputContainer.Add(port); outputs[id + suffix] = port;
                }
                if (module is ReleaseModule)
                {
                    if (_skill.WeaponMode)
                    {
                        Input(BindingRules.Trigger,"攻击入口","未连接时不参与攻击",typeof(SkillKind));
                        Output("释放",typeof(ReleaseModule));
                        Output("技能终点",typeof(Vector3),":endpoint");
                    }
                    else Output("释放方式",typeof(ReleaseModule));
                }
                if (module is ChargeRelease) Output("蓄力比例", typeof(float), ":charge");
                if (!_skill.IntegratedAttacks && module is CircleAttack) Input(BindingRules.Charge, "半径比例", "接蓄力比例；未连接时使用固定半径", typeof(float));
                if (!_skill.IntegratedAttacks && module is ConeAttack) Input(BindingRules.Charge, "距离比例", "接蓄力比例；未连接时使用固定距离", typeof(float));
                if (module is LineThrustAttack)
                {
                    if (!_skill.IntegratedAttacks) Input(BindingRules.Charge, "长度比例", "未连接时使用固定长度", typeof(float));
                    Output("普通区域目标", typeof(EffectModule), ":body");
                    Output("前端区域目标", typeof(EffectModule), ":tip");
                }
                if (module is PositionModule) Output("位置", typeof(Vector3));
                if (module is AttackModule)
                {
                    if (_skill.IntegratedAttacks) { Input(BindingRules.Trigger,"攻击按键","未连接时不参与攻击",typeof(SkillKind)); Output("技能终点",typeof(Vector3),":endpoint"); }
                    else if (_skill.WeaponMode) Input(BindingRules.Cast,"释放","未连接时不参与攻击",typeof(ReleaseModule));
                    Input(BindingRules.Position, module is CircleAttack ? "圆心" : "起点", module is CircleAttack ? "默认使用受最大施法距离限制的技能终点" : "默认施法起点", typeof(Vector3));
                    Output("命中目标", typeof(EffectModule));
                }
                if (module is ProjectileAttack) Output("命中响应", typeof(HitResponseModule), ":response");
                if (module is HitResponseModule) Input(BindingRules.Projectile, "投射物", "未连接时不生效", typeof(HitResponseModule));
                if (module is EffectModule)
                    Input(BindingRules.Targets, "目标", "未连接，不生效", typeof(EffectModule));
                if (module is PullEffect)
                {
                    Input(BindingRules.Destination, "目的地", "默认命中时的施法者位置", typeof(Vector3));
                    Output("到达目标", typeof(EffectModule));
                }
                node.RefreshExpandedState(); node.RefreshPorts();
            }
            foreach (var binding in _skill.Bindings)
            {
                if (!outputs.TryGetValue(binding.SourceId + (binding.SourcePort != "" ? ":" + binding.SourcePort : binding.Input == BindingRules.Projectile ? ":response" : ""), out var output) || !inputs.TryGetValue((binding.TargetId, binding.Input), out var input)) continue;
                var edge = output.ConnectTo(input); edge.capabilities = Capabilities.Selectable | Capabilities.Deletable;
                AddElement(edge);
            }
        }
        private Node AddModule(string title, string id, string path, Vector2 initial, Color color)
        {
            var node = new Node { title = title, name = id, userData = id };
            node.capabilities = Capabilities.Movable | Capabilities.Selectable;
            if (id != SkillDefinition.CoreLayoutId) node.capabilities |= Capabilities.Deletable;
            node.style.width = 350;
            node.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            node.titleContainer.style.backgroundColor = color;
            string help = NodeHelpContent.For(_skill.FindModule(id)?.GetType(), _skill.IntegratedAttacks);
            node.titleContainer.RegisterCallback<MouseEnterEvent>(evt =>
            { if (evt.pressedButtons == 0) ShowHelp(title, help, evt.mousePosition); });
            node.titleContainer.RegisterCallback<MouseMoveEvent>(evt =>
            { if (evt.pressedButtons == 0) ShowHelp(title, help, evt.mousePosition); else HideHelp(); });
            node.titleContainer.RegisterCallback<MouseLeaveEvent>(_ => HideHelp());
            node.titleContainer.RegisterCallback<MouseDownEvent>(_ => HideHelp(), TrickleDown.TrickleDown);
            node.extensionContainer.Add(SkillParameterFields.Create(_skill, path));
            node.SetPosition(new Rect(_skill.GetBoardPosition(id, initial), new Vector2(350, 180)));
            node.RefreshExpandedState(); node.RefreshPorts();
            AddElement(node);
            return node;
        }
        private bool RemoveBindings(IEnumerable<Edge> edgesToRemove)
        {
            bool changed = false;
            foreach (var edge in edgesToRemove.ToArray())
                if (edge.input?.userData is BindingPort input)
                    changed |= SkillAssetCommands.SetBinding(_skill, null, input.ModuleId, input.Input);
            return changed;
        }
        private void CutEdges(Edge[] crossed)
        {
            if (_skill == null || !RemoveBindings(crossed)) return;
            foreach (var edge in crossed)
            {
                edge.input?.Disconnect(edge); edge.output?.Disconnect(edge);
                RemoveFromSelection(edge); RemoveElement(edge);
            }
            _signature = Signature(); HideHelp();
        }
        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            bool changed = false;
            if (_skill != null && change.movedElements != null)
                foreach (var node in change.movedElements.OfType<Node>())
                {
                    var rect = node.GetPosition(); rect.position = BoardGrid.Snap(rect.position); node.SetPosition(rect);
                    SkillAssetCommands.MoveModule(_skill, (string)node.userData, node.GetPosition().position);
                }
            if (change.elementsToRemove != null)
            {
                changed |= RemoveBindings(change.elementsToRemove.OfType<Edge>());
                changed |= SkillAssetCommands.RemoveModules(_skill, change.elementsToRemove.OfType<Node>().Select(node => (string)node.userData));
            }
            if (change.edgesToCreate != null)
                foreach (var edge in change.edgesToCreate)
                    if (edge.output?.userData is BindingPort output && edge.input?.userData is BindingPort input)
                        changed |= SkillAssetCommands.SetBinding(_skill, output.ModuleId, input.ModuleId, input.Input, output.SourcePort);
            change.edgesToCreate?.Clear(); change.elementsToRemove?.Clear();
            if (changed) schedule.Execute(Rebuild);
            return change;
        }
        /// <inheritdoc/>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (!(startPort.userData is BindingPort start)) return new List<Port>();
            return ports.Where(port => port.direction != startPort.direction && port.portType == startPort.portType && port.userData is BindingPort).Where(port =>
            {
                var other = (BindingPort)port.userData;
                var output = startPort.direction == Direction.Output ? start : other;
                var input = startPort.direction == Direction.Input ? start : other;
                return BindingRules.CanConnect(_skill, output.ModuleId, input.ModuleId, input.Input, output.SourcePort);
            }).ToList();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            this.RemoveManipulator(_edgeCutter);
            viewTransformChanged -= RepaintGrid;
            UnregisterCallback<MouseMoveEvent>(OnPanMove, TrickleDown.TrickleDown);
            Undo.undoRedoPerformed -= Rebuild;
            _watch.Pause();
            graphViewChanged = null;
            _skill = null;
            Clear();
        }
    }
}



