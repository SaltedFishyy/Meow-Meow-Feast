using AbilityStudio.Model;
using UnityEditor;
using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Creates a separate editable example without overwriting any existing skill.</summary>
    public static class HitResponseExample
    {
        /// <summary>Opens a projectile with independently connected response and damage-effect modules.</summary>
        [MenuItem("Tools/技能工坊/创建命中响应示例")]
        public static void Create()
        {
            const string path = "Assets/_Sandbox/Skills/HitResponses.asset";
            if (!AssetDatabase.IsValidFolder("Assets/_Sandbox/Skills")) PrototypeSceneCommands.CreateMissingAssets();
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>(); skill.Id = "hit_responses";
                skill.DisplayName = "穿透连锁回旋弹"; skill.BaseDamage = 1; skill.Cooldown = 2;
                var projectile = new ProjectileAttack { Speed = 10, Range = 16, Size = .35f }; skill.AddAttack(projectile);
                var responses = new HitResponseModule[] { new PierceResponse { Count = 1 }, new ChainResponse { Count = 2, Distance = 10 }, new ReturnResponse() };
                for (int i = 0; i < responses.Length; i++)
                {
                    skill.AddResponse(responses[i]); skill.SetBinding(projectile.ModuleId, responses[i].ModuleId, BindingRules.Projectile);
                    skill.SetBoardPosition(responses[i].ModuleId, new Vector2(940, i * 220));
                }
                var bleed = new BleedEffect { DamagePerSecond = 1, Duration = 2 }; skill.AddEffect(bleed);
                skill.SetBinding(projectile.ModuleId, bleed.ModuleId, BindingRules.Targets);
                skill.SetBoardPosition(bleed.ModuleId, new Vector2(500, 440));
                SkillAssetCommands.Create(skill, path);
            }
            SkillBoardWindow.OpenSkill(skill);
        }
    }
}
