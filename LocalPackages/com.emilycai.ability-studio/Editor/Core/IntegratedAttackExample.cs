using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Shows the simplified attack-node workflow without overwriting existing samples.</summary>
    public static class IntegratedAttackExample
    {
        /// <summary>Opens a copied, safely migrated weapon or creates a fresh complete-attack example.</summary>
        [MenuItem("Tools/技能工坊/创建一体攻击示例")]
        public static void Create()
        {
            const string path="Assets/_Sandbox/Skills/IntegratedAttacks.asset";
            if(!AssetDatabase.IsValidFolder("Assets/_Sandbox/Skills")) PrototypeSceneCommands.CreateMissingAssets();
            var skill=AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if(skill==null)
            {
                var source=AssetDatabase.LoadAssetAtPath<SkillDefinition>("Assets/_Sandbox/Skills/WeaponAttacks.asset");
                if(source!=null && source.CanIntegrate) {skill=Object.Instantiate(source); skill.IntegrateAttacks();}
                else
                {
                    skill=ScriptableObject.CreateInstance<SkillDefinition>(); skill.IntegratedAttacks=true;
                    var cone=new ConeAttack {Range=10,AngleDegrees=90}; cone.Settings.BaseDamage=2; cone.Settings.Cooldown=.4f;
                    var rect=new LineThrustAttack {ReleaseMode=1,Length=14,MinLength=4,Width=3,TipLength=2,DamageMode=DamageMode.ConnectedDamage}; rect.Settings.BaseDamage=2; rect.Settings.Cooldown=2;
                    skill.AddAttack(cone); skill.AddAttack(rect);
                    skill.SetBinding(SkillDefinition.CoreLayoutId,cone.ModuleId,BindingRules.Trigger,BindingRules.Normal);
                    skill.SetBinding(SkillDefinition.CoreLayoutId,rect.ModuleId,BindingRules.Trigger,BindingRules.Special);
                    var body=new DamageEffect(); var tip=new DamageEffect {MultiplierPercent=150}; var stun=new StunEffect {Duration=2}; skill.AddEffect(body); skill.AddEffect(tip); skill.AddEffect(stun);
                    skill.SetBinding(rect.ModuleId,body.ModuleId,BindingRules.Targets,BindingRules.BodyTargets);
                    skill.SetBinding(rect.ModuleId,tip.ModuleId,BindingRules.Targets,BindingRules.TipTargets);
                    skill.SetBinding(rect.ModuleId,stun.ModuleId,BindingRules.Targets,BindingRules.TipTargets);
                }
                skill.Id="integrated_attacks"; skill.DisplayName="瞬发与蓄力攻击";
                skill.SetBoardPosition(SkillDefinition.CoreLayoutId,new Vector2(0,160)); int index=0;
                foreach(var attack in skill.Attacks) skill.SetBoardPosition(attack.ModuleId,new Vector2(440,index++*500));
                for(int i=0;i<skill.Effects.Count;i++) skill.SetBoardPosition(skill.Effects[i].ModuleId,new Vector2(900,i*240));
                SkillAssetCommands.Create(skill,path);
            }
            SkillBoardWindow.OpenSkill(skill);
        }
    }
}
