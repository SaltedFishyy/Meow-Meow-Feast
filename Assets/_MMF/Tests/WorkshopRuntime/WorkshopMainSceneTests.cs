#if UNITY_EDITOR
using System.Collections;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using MMF.Combat;
using MMF.Data;
using MMF.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace MMF.Tests.Workshop
{
    public sealed class WorkshopMainSceneTests
    {
        [UnityTest] public IEnumerator EquippedWeaponHitsInMainSceneAndUnequipRestoresBackpack()
        {
            const string loadoutPath = "Assets/_MMF/Data/Resources/MMFWorkshopLoadout.asset";
            var loadout = AssetDatabase.LoadAssetAtPath<WorkshopLoadout>(loadoutPath);
            bool created = loadout == null; var previous = created ? null : loadout.PlayerOne;
            if (created)
            {
                if (!AssetDatabase.IsValidFolder("Assets/_MMF/Data/Resources")) AssetDatabase.CreateFolder("Assets/_MMF/Data", "Resources");
                loadout = ScriptableObject.CreateInstance<WorkshopLoadout>(); AssetDatabase.CreateAsset(loadout, loadoutPath);
            }
            var skill = ScriptableObject.CreateInstance<SkillDefinition>(); skill.Id = "equipment_test";
            skill.SetAttack(new CircleAttack { Radius = 1 }); skill.BaseDamage = 1; skill.Cooldown = 0; skill.Distance = 20;
            SkillGraphMigration.Upgrade(skill);
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_MMF/Data/Weapons/WorkshopRuntimeTest.asset");
            AssetDatabase.CreateAsset(skill, path); loadout.EquipPlayerOne(skill); EditorUtility.SetDirty(loadout); AssetDatabase.SaveAssetIfDirty(loadout);
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_MMF/Scenes/Prototype.unity", new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                var player = Object.FindAnyObjectByType<PlayerAttack>();
                var controller = player.GetComponent<WorkshopWeaponController>();
                Assert.That(controller, Is.Not.Null); Assert.That(controller.HasWeapon, Is.True);
                player.transform.position = new Vector3(10, 6, 0);
                var target = Object.FindAnyObjectByType<MonsterSkillTarget>(); target.transform.position = new Vector3(12, 6, 0);
                Physics2D.SyncTransforms();
                float hp = target.CurrentHp;
                Assert.That(player.TryAttack(target.Position), Is.True);
                Assert.That(target.CurrentHp, Is.EqualTo(hp - 1));
                var health = player.GetComponent<PlayerHealth>(); health.TakeHit(1, Vector2.zero);
                var heal = new HealEffect { Amount = 1 }; skill.Attack.Settings.BaseDamage = 0; skill.AddEffect(heal);
                skill.SetBinding(skill.Attack.ModuleId, heal.ModuleId, BindingRules.Targets);
                Assert.That(player.TryAttack(player.transform.position), Is.True);
                Assert.That(health.Current, Is.EqualTo(health.Max));
                skill.AttackMode = 1;
                var specialRelease = new ChargeRelease { Duration = .2f }; skill.AddRelease(specialRelease);
                var special = new CircleAttack { Radius = 1 }; special.Settings.BaseDamage = 1; special.Settings.Cooldown = 0; special.Settings.Distance = 20; skill.AddAttack(special);
                skill.SetBinding(SkillDefinition.CoreLayoutId, specialRelease.ModuleId, BindingRules.Trigger, BindingRules.Special);
                skill.SetBinding(specialRelease.ModuleId, special.ModuleId, BindingRules.Cast);
                Assert.That(player.WorkshopOwnsRightMouse, Is.True);
                float beforeSpecial = target.CurrentHp;
                Assert.That(player.PressWorkshopInput(SkillInputKey.MouseRight, target.Position), Is.True);
                player.ReleaseWorkshopInput(SkillInputKey.MouseLeft, target.Position);
                Assert.That(target.CurrentHp, Is.EqualTo(beforeSpecial));
                player.ReleaseWorkshopInput(SkillInputKey.MouseRight, target.Position);
                Assert.That(target.CurrentHp, Is.EqualTo(beforeSpecial - 1));
                skill.AttackMode = 0;
                Assert.That(player.WorkshopOwnsRightMouse, Is.False);
                Assert.That(player.PressWorkshopInput(SkillInputKey.MouseRight, target.Position), Is.False);
                loadout.EquipPlayerOne(null);
                Assert.That(controller.HasWeapon, Is.False);
                int swings = 0; player.Swung += (weapon, aim) => swings++;
                // The prototype starts with the frying pan selected in the backpack.
                Assert.That(player.TryAttack((Vector2)player.transform.position + Vector2.right), Is.True);
                Assert.That(swings, Is.EqualTo(1));
            }
            finally
            {
                loadout.EquipPlayerOne(previous); EditorUtility.SetDirty(loadout); AssetDatabase.SaveAssetIfDirty(loadout);
                AssetDatabase.DeleteAsset(path); if (created) AssetDatabase.DeleteAsset(loadoutPath);
            }
        }
    }
}
#endif
