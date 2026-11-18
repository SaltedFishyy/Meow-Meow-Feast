using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Independent two-input weapon example; never overwrites existing skills.</summary>
    public static class WeaponAttackExample
    {
        /// <summary>Creates or reopens the example board.</summary>
        [MenuItem("Tools/技能工坊/创建武器双攻击示例")]
        public static void Create()
        {
            const string path="Assets/_Sandbox/Skills/WeaponAttacks.asset";
            if (!AssetDatabase.IsValidFolder("Assets/_Sandbox/Skills")) PrototypeSceneCommands.CreateMissingAssets();
            var skill=AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (skill == null)
            {
                skill=ScriptableObject.CreateInstance<SkillDefinition>(); skill.WeaponMode=true; skill.Id="weapon_attacks"; skill.DisplayName="武器·挥砍与蓄力重击";
                var normal=new InstantRelease {BaseDamage=2,Cooldown=.4f,Distance=10};
                var special=new ChargeRelease {BaseDamage=2,Cooldown=2,Distance=14,Duration=2}; skill.AddRelease(normal); skill.AddRelease(special);
                var cone=new ConeAttack {Range=12,AngleDegrees=100,DamageMode=DamageMode.ConnectedDamage};
                var rectangle=new LineThrustAttack {MinLength=4,Length=14,Width=12,TipLength=3,DamageMode=DamageMode.ConnectedDamage}; skill.AddAttack(cone); skill.AddAttack(rectangle);
                skill.SetBinding(SkillDefinition.CoreLayoutId,normal.ModuleId,BindingRules.Trigger,BindingRules.Normal);
                skill.SetBinding(SkillDefinition.CoreLayoutId,special.ModuleId,BindingRules.Trigger,BindingRules.Special);
                skill.SetBinding(normal.ModuleId,cone.ModuleId,BindingRules.Cast); skill.SetBinding(special.ModuleId,rectangle.ModuleId,BindingRules.Cast);
                skill.SetBinding(special.ModuleId,rectangle.ModuleId,BindingRules.Charge,BindingRules.Charge);
                var normalDamage=new DamageEffect(); var body=new DamageEffect(); var tip=new DamageEffect {MultiplierPercent=150}; var stun=new StunEffect {Duration=2};
                skill.AddEffect(normalDamage); skill.AddEffect(body); skill.AddEffect(tip); skill.AddEffect(stun);
                skill.SetBinding(cone.ModuleId,normalDamage.ModuleId,BindingRules.Targets);
                skill.SetBinding(rectangle.ModuleId,body.ModuleId,BindingRules.Targets,BindingRules.BodyTargets);
                skill.SetBinding(rectangle.ModuleId,tip.ModuleId,BindingRules.Targets,BindingRules.TipTargets);
                skill.SetBinding(rectangle.ModuleId,stun.ModuleId,BindingRules.Targets,BindingRules.TipTargets);
                skill.SetBoardPosition(SkillDefinition.CoreLayoutId,new Vector2(0,240));
                skill.SetBoardPosition(normal.ModuleId,new Vector2(440,0)); skill.SetBoardPosition(special.ModuleId,new Vector2(440,400));
                skill.SetBoardPosition(cone.ModuleId,new Vector2(880,0)); skill.SetBoardPosition(rectangle.ModuleId,new Vector2(880,400));
                skill.SetBoardPosition(normalDamage.ModuleId,new Vector2(1320,0)); skill.SetBoardPosition(body.ModuleId,new Vector2(1320,360));
                skill.SetBoardPosition(tip.ModuleId,new Vector2(1320,580)); skill.SetBoardPosition(stun.ModuleId,new Vector2(1320,800));
                SkillAssetCommands.Create(skill,path);
            }
            SkillBoardWindow.OpenSkill(skill);
        }
    }
}
