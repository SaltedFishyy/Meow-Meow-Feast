using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Creates a separate charged rectangle example without replacing user skills.</summary>
    public static class ChargedRectangleExample
    {
        [MenuItem("Tools/技能工坊/创建蓄力长方形示例")]
        public static void Create()
        {
            const string path = "Assets/_Sandbox/Skills/ChargedRectangle.asset";
            if (!AssetDatabase.IsValidFolder("Assets/_Sandbox/Skills")) PrototypeSceneCommands.CreateMissingAssets();
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>();
                skill.Id = "charged_rectangle"; skill.DisplayName = "蓄力长方形·前端重击";
                skill.Kind = SkillKind.SpecialAttack; skill.BaseDamage = 2; skill.Cooldown = 2;
                var charge = new ChargeRelease { Duration = 2 }; skill.AddRelease(charge);
                var rectangle = new LineThrustAttack { MinLength = 4, Length = 14, Width = 12, TipLength = 3, DamageMode = DamageMode.ConnectedDamage }; skill.AddAttack(rectangle);
                var body = new DamageEffect { MultiplierPercent = 100 }; var tip = new DamageEffect { MultiplierPercent = 150 }; var stun = new StunEffect { Duration = 2 };
                skill.AddEffect(body); skill.AddEffect(tip); skill.AddEffect(stun);
                skill.SetBinding(charge.ModuleId, SkillDefinition.CoreLayoutId, BindingRules.Release);
                skill.SetBinding(charge.ModuleId, rectangle.ModuleId, BindingRules.Charge, BindingRules.Charge);
                skill.SetBinding(rectangle.ModuleId, body.ModuleId, BindingRules.Targets, BindingRules.BodyTargets);
                skill.SetBinding(rectangle.ModuleId, tip.ModuleId, BindingRules.Targets, BindingRules.TipTargets);
                skill.SetBinding(rectangle.ModuleId, stun.ModuleId, BindingRules.Targets, BindingRules.TipTargets);
                skill.SetBoardPosition(charge.ModuleId, new Vector2(-500, 0));
                skill.SetBoardPosition(SkillDefinition.CoreLayoutId, new Vector2(0, 0));
                skill.SetBoardPosition(rectangle.ModuleId, new Vector2(500, 0));
                skill.SetBoardPosition(body.ModuleId, new Vector2(1050, 0));
                skill.SetBoardPosition(tip.ModuleId, new Vector2(1050, 220));
                skill.SetBoardPosition(stun.ModuleId, new Vector2(1050, 440));
                SkillAssetCommands.Create(skill, path);
            }
            SkillBoardWindow.OpenSkill(skill);
        }
    }
}
