using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Creates a separate reviewable composition without changing the user's existing skill.</summary>
    public static class EndpointPullExample
    {
        /// <summary>Creates and opens the endpoint gather example; never overwrites an existing example.</summary>
        [MenuItem("Tools/技能工坊/创建终点聚拢示例")]
        public static void Create()
        {
            const string path = "Assets/_Sandbox/Skills/EndpointPull.asset";
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>(); skill.Id = "endpoint_pull";
                skill.DisplayName = "终点聚拢"; skill.BaseDamage = 0; skill.Cooldown = 2;
                skill.Distance = 8;
                var circle = new CircleAttack { Radius = 6 };
                var pull = new PullEffect { StopDistance = 0, Duration = 0.6f };
                var stun = new StunEffect { Duration = 2 };
                skill.AddAttack(circle); skill.AddEffect(pull); skill.AddEffect(stun);
                skill.SetBinding(SkillDefinition.CoreLayoutId, circle.ModuleId, BindingRules.Position);
                skill.SetBinding(SkillDefinition.CoreLayoutId, pull.ModuleId, BindingRules.Destination);
                skill.SetBinding(circle.ModuleId, pull.ModuleId, BindingRules.Targets);
                skill.SetBinding(pull.ModuleId, stun.ModuleId, BindingRules.Targets);
                skill.SetBoardPosition(SkillDefinition.CoreLayoutId, new Vector2(20, 20));
                skill.SetBoardPosition(circle.ModuleId, new Vector2(440, 20));
                skill.SetBoardPosition(pull.ModuleId, new Vector2(860, 140));
                skill.SetBoardPosition(stun.ModuleId, new Vector2(1280, 140));
                SkillAssetCommands.Create(skill, path);
            }
            SkillBoardWindow.OpenSkill(skill); Selection.activeObject = skill;
        }
    }
}
