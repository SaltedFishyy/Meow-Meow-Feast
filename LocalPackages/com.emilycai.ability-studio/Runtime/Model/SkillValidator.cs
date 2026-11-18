using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
namespace AbilityStudio.Model
{
    /// <summary>Checks the implemented vertical slice; unsupported modules are rejected explicitly.</summary>
    public static class SkillValidator
    {
        /// <summary>Returns actionable errors. An empty list permits casting.</summary>
        public static IReadOnlyList<string> Validate(SkillDefinition skill)
        {
            var errors = new List<string>();
            if (skill == null) { errors.Add("请选择技能资产。"); return errors; }
            if (!System.Enum.IsDefined(typeof(SkillKind), skill.Kind)) errors.Add("请选择有效的技能类型。");
            if (skill.SchemaVersion != 1) errors.Add("不支持此技能数据版本。");
            if (string.IsNullOrEmpty(skill.Id) || !Regex.IsMatch(skill.Id, "^[a-z][a-z0-9_]*$")) errors.Add("标识只能使用小写英文字母、数字和下划线，且须以字母开头。");
            if (!NonNegative(skill.Cooldown)) errors.Add("冷却时间必须是有限的非负数。");
            if (!NonNegative(skill.BaseDamage)) errors.Add("伤害必须是有限的非负数。");
            if (!Positive(skill.Distance)) errors.Add("最大施法距离必须是有限正数。");
            var offset = skill.CastOriginOffset;
            if (!Finite(offset.x) || !Finite(offset.y) || !Finite(offset.z)) errors.Add("施法偏移必须是有限数值。");
            if (!skill.Attacks.Any()) errors.Add("请从模块库添加技能范围后再施法。");
            foreach (var release in skill.IntegratedAttacks ? System.Array.Empty<ReleaseModule>() : skill.Releases)
            {
                if (skill.WeaponMode && release != null && (!NonNegative(release.BaseDamage) || !NonNegative(release.Cooldown) || !Positive(release.Distance))) errors.Add("释放节点的伤害、冷却须为非负数，施法距离须为正数。");
                if (release is ChargeRelease charge) { if (!Positive(charge.Duration)) errors.Add("最长蓄力时间必须是有限正数。"); }
                else if (!(release is InstantRelease)) errors.Add("不支持的释放方式。");
            }
            foreach (var attack in skill.Attacks)
            {
                if (skill.IntegratedAttacks && attack != null)
                {
                    if (attack.ReleaseMode < 0 || attack.ReleaseMode > 1) errors.Add("请选择瞬发或蓄力。");
                    if (!NonNegative(attack.Settings.BaseDamage) || !NonNegative(attack.Settings.Cooldown) || !Positive(attack.Settings.Distance) || (attack.IsCharged && !Positive(attack.Settings.Duration))) errors.Add("攻击伤害与冷却须为非负数，施法距离与蓄满时间须为正数。");
                }
                if (attack != null && !System.Enum.IsDefined(typeof(DamageMode), attack.DamageMode)) errors.Add("请选择有效的伤害结算方式。");
                if (attack is ProjectileAttack p)
                {
                    if (!Positive(p.Speed) || !Positive(p.Range) || !Positive(p.Size)) errors.Add("投射物速度、距离和半径必须是有限正数。");
                }
                else if (attack is CircleAttack circle)
                {
                    if ((skill.IntegratedAttacks ? circle.IsCharged : skill.BindingSource(circle.ModuleId, BindingRules.Charge) != null) && (!Positive(circle.MinRadius) || circle.MinRadius > circle.Radius)) errors.Add("蓄力最小半径必须为正数且不超过最大半径。");
                    if (!Positive(circle.Radius)) errors.Add("圆形范围半径必须是有限正数。");
                }
                else if (attack is ConeAttack cone)
                {
                    if ((skill.IntegratedAttacks ? cone.IsCharged : skill.BindingSource(cone.ModuleId, BindingRules.Charge) != null) && (!Positive(cone.MinRange) || cone.MinRange > cone.Range)) errors.Add("蓄力最短距离必须为正数且不超过最大距离。");
                    if (!Positive(cone.Range) || !Positive(cone.AngleDegrees) || cone.AngleDegrees > 360) errors.Add("扇形距离必须为正数，角度须大于 0 且不超过 360 度。");
                }
                else if (attack is LineThrustAttack rectangle)
                {
                    if (!Positive(rectangle.TipLength)) errors.Add("前端区域长度必须是有限正数。");
                    if ((skill.IntegratedAttacks ? rectangle.IsCharged : skill.BindingSource(rectangle.ModuleId, BindingRules.Charge) != null) && (!Positive(rectangle.MinLength) || rectangle.MinLength > rectangle.Length)) errors.Add("最短长度必须为正数且不超过最长长度。");
                    if (!Positive(rectangle.Length) || !Positive(rectangle.Width)) errors.Add("长方形长度和宽度必须是有限正数。");
                }
                else errors.Add("攻击模块缺失或不受支持。");
            }
            foreach (var position in skill.Positions)
            {
                if (position is SkillEndpointPosition endpoint) { if (!Positive(endpoint.Distance)) errors.Add("终点距离必须是有限正数。"); }
                else if (!(position is SelfPosition) && !(position is MousePosition)) errors.Add("位置来源缺失或不受支持。");
            }
            foreach (var response in skill.Responses)
            {
                if (response is PierceResponse pierce)
                { if (pierce.Count < 1 || pierce.Count > 64) errors.Add("穿透次数必须为 1 到 64 的整数。"); }
                else if (response is ChainResponse chain)
                { if (chain.Count < 1 || chain.Count > 64 || !Positive(chain.Distance)) errors.Add("连锁次数必须为 1 到 64 的整数，搜索距离必须为有限正数。"); }
                else if (response is ReturnResponse back)
                { if (!Positive(back.SpeedMultiplier) || back.SpeedMultiplier > 10) errors.Add("返回速度倍率必须大于 0 且不超过 10。"); }
                else errors.Add("请移除空的或不支持的命中响应。");
            }
            foreach (var binding in skill.Bindings)
                if (binding == null || !BindingRules.CanConnect(skill, binding.SourceId, binding.TargetId, binding.Input, binding.SourcePort)) errors.Add("请删除无效的模块连线。");
            if (skill.Bindings.Where(b => b != null).GroupBy(b => b.TargetId + ":" + b.Input).Any(g => g.Count() > 1)) errors.Add("每个输入接口只能连接一个来源。");
            if (skill.WeaponMode && skill.Bindings.Where(b => b != null && b.Input == BindingRules.Trigger).GroupBy(b => b.SourcePort).Any(g => g.Count() > 1)) errors.Add("普通攻击与特殊攻击各只能接一个释放节点。");
            foreach (var effect in skill.Effects)
            {
                if (effect == null) { errors.Add("请删除空的效果模块。"); continue; }
                if (effect is HitFeedbackEffect feedback)
                {
                    if (feedback.TriggerMode < 0 || feedback.TriggerMode > 1 || !NonNegative(feedback.MinInterval)) errors.Add("命中反馈触发方式无效，最短间隔须为有限非负数。");
                    if (feedback is RecoilFeedback recoil)
                    { if (!NonNegative(recoil.Angle) || recoil.Angle > 60 || !Positive(recoil.LeanDuration) || !Positive(recoil.RecoveryDuration)) errors.Add("后仰角度须为 0–60 度，后仰与回正时间须为有限正数。"); }
                    else if (feedback is CameraShakeFeedback shake)
                    { if (!NonNegative(shake.Strength) || !Positive(shake.Duration) || !Positive(shake.Frequency)) errors.Add("镜头震动强度须为非负数，时间和频率须为有限正数。"); }
                    else if (feedback is HitVfxFeedback vfx)
                    { if (!Positive(vfx.Scale) || !Positive(vfx.Lifetime) || !Finite(vfx.Offset.x) || !Finite(vfx.Offset.y) || !Finite(vfx.Offset.z)) errors.Add("命中特效大小与存活时间须为有限正数，偏移须为有限数值。"); }
                    else if (feedback is HitAudioFeedback audio)
                    { if (!NonNegative(audio.Volume) || audio.Volume > 1 || !Positive(audio.Pitch) || audio.Pitch < .1f || audio.Pitch > 3) errors.Add("音量须为 0–1，音调须为 0.1–3 的有限数值。"); }
                    else errors.Add("不支持的命中反馈。");
                }
                else if (effect is DamageEffect direct) { if (!NonNegative(direct.MultiplierPercent) || !float.IsFinite(skill.BaseDamage * (direct.MultiplierPercent / 100f))) errors.Add("伤害倍率须为有限非负数，结算伤害不得溢出。"); }
                else if (effect is PullEffect pull)
                {
                    if (!NonNegative(pull.StopDistance) || !Positive(pull.Duration)) errors.Add("拉拽目的地间距须为非负数，持续时间须为正数。");
                }
                else if (effect is SlowEffect slow) { if (!Positive(slow.ReductionPercent) || slow.ReductionPercent > 100 || !Positive(slow.Duration)) errors.Add("减速比例须大于 0 且不超过 100%，持续时间须为正数。"); }
                else if (effect is BleedEffect bleed) { if (!Positive(bleed.DamagePerSecond) || !Positive(bleed.Duration)) errors.Add("流血每秒伤害和持续时间必须是有限正数。"); }
                else if (effect is StunEffect stun) { if (!Positive(stun.Duration)) errors.Add("眩晕持续时间必须是有限正数。"); }
                else if (effect is KnockbackEffect knockback) { if (!Positive(knockback.Distance)) errors.Add("击退距离必须是有限正数。"); }
                else errors.Add("不支持的效果：" + effect.GetType().Name);
            }
            return errors;
        }
        /// <summary>Whether errors prevent casting.</summary>
        public static bool HasErrors(SkillDefinition skill) => Validate(skill).Count > 0;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Positive(float value) => Finite(value) && value > 0;
        private static bool NonNegative(float value) => Finite(value) && value >= 0;
    }
}







