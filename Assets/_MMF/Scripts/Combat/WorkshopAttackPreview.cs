using System.Collections.Generic;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using UnityEngine;
namespace MMF.Combat
{
    /// <summary>Reusable outlines for the held weapon's normal attack; shares runtime geometry.</summary>
    public sealed class WorkshopAttackPreview : MonoBehaviour
    {
        private WorkshopWeaponController _controller;
        private Material _material;
        private readonly List<LineRenderer> _lines = new();
        private int _used;
        public void Bind(WorkshopWeaponController controller)
        {
            _controller = controller;
            _material = new Material(Shader.Find("Sprites/Default"));
        }
        private void LateUpdate()
        {
            _used = 0;
            if (_controller != null && _controller.TryGetPreview(out var skill, out var aim, out var charge))
            {
                DrawArea(skill, skill.Attack, aim, charge);
                for (int i = 0; i < skill.AdditionalAttacks.Count; i++) DrawArea(skill, skill.AdditionalAttacks[i], aim, charge);
            }
            for (int i = _used; i < _lines.Count; i++) _lines[i].enabled = false;
        }
        private void DrawArea(SkillDefinition skill, AttackModule area, Vector3 aim, float charge)
        {
            if (area == null) return;
            if (skill.WeaponMode)
            {
                if (skill.IntegratedAttacks)
                { if (skill.BindingSource(area.ModuleId, BindingRules.Trigger) != SkillDefinition.CoreLayoutId || skill.BindingOutput(area.ModuleId, BindingRules.Trigger) != BindingRules.Normal) return; }
                else
                {
                    var release = skill.BindingSource(area.ModuleId, BindingRules.Cast);
                    if (release == null || skill.BindingSource(release, BindingRules.Trigger) != SkillDefinition.CoreLayoutId || skill.BindingOutput(release, BindingRules.Trigger) != BindingRules.Normal) return;
                }
            }
            var origin = SkillPositions.AttackPosition(skill, area, _controller, aim);
            var direction = SkillPositions.AreaDirection(skill, area, _controller, aim);
            if (area is CircleAttack circle) Circle(origin, SkillChargeGeometry.Radius(skill, circle, charge), Color.cyan);
            else if (area is LineThrustAttack rectangle)
            {
                float length = SkillChargeGeometry.Length(skill, rectangle, charge);
                Rectangle(origin, direction, length, rectangle.Width, Color.cyan);
                float tip = Mathf.Min(length, rectangle.TipLength);
                if (tip > 0) Rectangle(SkillChargeGeometry.TipOrigin(origin, direction, length, tip), direction, tip, rectangle.Width, Color.yellow);
            }
            else if (area is ConeAttack cone)
            {
                var line = Line(34, true, Color.cyan); Point(line, 0, origin);
                for (int i = 1; i < 34; i++) Point(line, i, ConeGeometry.ArcPoint(origin, direction, SkillChargeGeometry.Range(skill, cone, charge), cone.AngleDegrees, (i - 1f) / 32));
            }
            else if (area is ProjectileAttack shot)
            {
                Rectangle(origin, direction, shot.Range, shot.Size * 2, Color.cyan);
                var line = Line(2, false, Color.cyan); Point(line, 0, origin); Point(line, 1, origin + direction * shot.Range);
            }
            // The endpoint marks maximum mouse-targeted cast distance, independently of shape reach.
            Circle(SkillPositions.AreaEndpoint(skill, area, _controller, aim), .12f, Color.yellow);
        }
        private LineRenderer Line(int count, bool loop, Color color)
        {
            if (_used == _lines.Count)
            {
                var go = new GameObject("武器攻击预览"); go.transform.SetParent(transform, false);
                var added = go.AddComponent<LineRenderer>(); added.sharedMaterial = _material;
                added.useWorldSpace = true; added.widthMultiplier = .025f; added.sortingOrder = 19; _lines.Add(added);
            }
            var line = _lines[_used++]; line.enabled = true; line.loop = loop;
            line.startColor = line.endColor = color; line.positionCount = count; return line;
        }
        private static void Point(LineRenderer line, int index, Vector3 point) => line.SetPosition(index, point + Vector3.back * .05f);
        private void Circle(Vector3 center, float radius, Color color)
        {
            var line = Line(48, true, color);
            for (int i = 0; i < 48; i++) { float a = i * Mathf.PI * 2 / 48; Point(line, i, center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius); }
        }
        private void Rectangle(Vector3 origin, Vector3 direction, float length, float width, Color color)
        {
            var line = Line(4, true, color);
            for (int i = 0; i < 4; i++) Point(line, i, RectangleGeometry.Corner(origin, direction, length, width, i));
        }
        private void OnDisable() { foreach (var line in _lines) if (line != null) line.enabled = false; }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
