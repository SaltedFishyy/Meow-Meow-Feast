using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Canvas = UnityEditor.Experimental.GraphView.GraphView;
namespace AbilityStudio.Editor.Board.GraphView
{
    /// <summary>Right-button sweep over visible edges, grouped into one undo operation per gesture.</summary>
    internal sealed class EdgeCutManipulator : MouseManipulator
    {
        private readonly Action<Edge[]> _cut;
        private bool _armed, _dragging, _suppressMenu;
        private Vector2 _start, _last;
        private readonly Dictionary<Edge, StyleFloat> _pending = new Dictionary<Edge, StyleFloat>();
        private readonly List<Vector2> _trailPoints = new List<Vector2>();
        private VisualElement _trail;
        internal EdgeCutManipulator(Action<Edge[]> cut) { _cut = cut; }
        internal bool SuppressMenu => _dragging || _suppressMenu;
        protected override void RegisterCallbacksOnTarget()
        {
            _trail = new VisualElement { name = "edge-cut-trail", pickingMode = PickingMode.Ignore };
            _trail.style.overflow = Overflow.Hidden;
            _trail.style.display = DisplayStyle.None;
            target.hierarchy.Add(_trail); _trail.StretchToParentSize();
            _trail.generateVisualContent += DrawTrail;
            target.RegisterCallback<MouseDownEvent>(Down, TrickleDown.TrickleDown);
            target.RegisterCallback<MouseMoveEvent>(Move, TrickleDown.TrickleDown);
            target.RegisterCallback<MouseUpEvent>(Up, TrickleDown.TrickleDown);
            target.RegisterCallback<MouseCaptureOutEvent>(LostCapture);
            target.RegisterCallback<DetachFromPanelEvent>(Detached);
            target.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
        }
        protected override void UnregisterCallbacksFromTarget()
        {
            Finish();
            _trail.generateVisualContent -= DrawTrail; _trail.RemoveFromHierarchy();
            target.UnregisterCallback<MouseDownEvent>(Down, TrickleDown.TrickleDown);
            target.UnregisterCallback<MouseMoveEvent>(Move, TrickleDown.TrickleDown);
            target.UnregisterCallback<MouseUpEvent>(Up, TrickleDown.TrickleDown);
            target.UnregisterCallback<MouseCaptureOutEvent>(LostCapture);
            target.UnregisterCallback<DetachFromPanelEvent>(Detached);
            target.UnregisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
        }
        private void Down(MouseDownEvent evt)
        {
            _suppressMenu = false;
            if (evt.button != (int)MouseButton.RightMouse || evt.pressedButtons != 2) return;
            // Preserve text-field/port/node contextual gestures; start a cut on the canvas or an edge.
            var element = evt.target as VisualElement;
            if (element is Node || element?.GetFirstAncestorOfType<Node>() != null) return;
            Finish();
            _armed = true; _start = _last = evt.mousePosition;
            _trailPoints.Add(_trail.WorldToLocal(_start));
        }
        private void Move(MouseMoveEvent evt)
        {
            if (!_armed) return;
            if ((evt.pressedButtons & 2) == 0) { Finish(); return; }
            if (!_dragging)
            {
                if (Vector2.Distance(_start, evt.mousePosition) < 6) return;
                _dragging = true; _suppressMenu = true; target.CaptureMouse(); target.Focus();
                _trail.style.display = DisplayStyle.Flex; _trail.BringToFront();
            }
            var board = (Canvas)target;
            var from = _last; var to = evt.mousePosition;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / 2));
            var nodes = board.nodes.ToArray();
            var crossed = board.edges.Where(edge => edge.input != null && edge.output != null && Crosses(edge)).ToArray();
            bool Crosses(Edge edge)
            {
                for (int i = 0; i <= steps; i++)
                {
                    var point = Vector2.Lerp(from, to, (float)i / steps);
                    if (!target.worldBound.Contains(point) || nodes.Any(node => node.worldBound.Contains(point))) continue;
                    if (edge.edgeControl.ContainsPoint(edge.edgeControl.WorldToLocal(point))) return true;
                }
                return false;
            }
            foreach (var edge in crossed)
                if (!_pending.ContainsKey(edge)) { _pending.Add(edge, edge.style.opacity); edge.style.opacity = .35f; }
            _trailPoints.Add(_trail.WorldToLocal(to)); _trail.MarkDirtyRepaint();
            _last = to; evt.StopImmediatePropagation();
        }
        private void Up(MouseUpEvent evt)
        {
            if (evt.button != (int)MouseButton.RightMouse || !_armed) return;
            bool dragged = _dragging; Finish(dragged);
            if (dragged) evt.StopImmediatePropagation();
        }
        private void LostCapture(MouseCaptureOutEvent evt) => Finish();
        private void Detached(DetachFromPanelEvent evt) => Finish();
        private void KeyDown(KeyDownEvent evt)
        {
            if (!_armed || evt.keyCode != KeyCode.Escape) return;
            Finish(); evt.StopImmediatePropagation();
        }
        private void DrawTrail(MeshGenerationContext context)
        {
            if (_trailPoints.Count < 2) return;
            var painter = context.painter2D;
            painter.strokeColor = Color.white; painter.lineWidth = 1f;
            painter.BeginPath(); painter.MoveTo(_trailPoints[0]);
            for (int i = 1; i < _trailPoints.Count; i++) painter.LineTo(_trailPoints[i]);
            painter.Stroke();
        }
        private void Finish(bool commit = false)
        {
            _armed = false; _dragging = false;
            var crossed = _pending.Keys.Where(edge => edge.parent != null && edge.input != null && edge.output != null).ToArray();
            foreach (var pair in _pending) pair.Key.style.opacity = pair.Value;
            _pending.Clear(); _trailPoints.Clear();
            if (_trail != null) { _trail.style.display = DisplayStyle.None; _trail.MarkDirtyRepaint(); }
            if (commit && crossed.Length > 0)
            {
                Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                try { _cut(crossed); }
                finally { Undo.SetCurrentGroupName("划线断开连接"); Undo.CollapseUndoOperations(group); Undo.IncrementCurrentGroup(); }
            }
            if (target != null && target.HasMouseCapture()) target.ReleaseMouse();
        }
    }
}
