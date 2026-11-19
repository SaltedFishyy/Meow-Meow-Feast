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
using UnityEngine.InputSystem;
using System.Linq;
namespace MMF.Tests.Workshop
{
    public sealed class WorkshopMainSceneTests
    {
        [UnityTest] public IEnumerator EquippedWeaponHitsInMainSceneAndUnequipRestoresBackpack()
        {
            const string loadoutPath = "Assets/_MMF/Data/Resources/MMFWorkshopLoadout.asset";
            var loadout = AssetDatabase.LoadAssetAtPath<WorkshopLoadout>(loadoutPath);
            bool previousPreview = loadout == null || loadout.ShowAttackPreview;
            bool created = loadout == null; var previous = created ? null : loadout.PlayerOne;
            if (created)
            {
                if (!AssetDatabase.IsValidFolder("Assets/_MMF/Data/Resources")) AssetDatabase.CreateFolder("Assets/_MMF/Data", "Resources");
                loadout = ScriptableObject.CreateInstance<WorkshopLoadout>(); AssetDatabase.CreateAsset(loadout, loadoutPath);
            }
            var skill = ScriptableObject.CreateInstance<SkillDefinition>(); skill.Id = "equipment_test";
            skill.SetAttack(new CircleAttack { Radius = 1 }); skill.BaseDamage = 1; skill.Cooldown = 0; skill.Distance = 20;
            SkillGraphMigration.Upgrade(skill);
            skill.SetBinding(SkillDefinition.CoreLayoutId, skill.Attack.ModuleId, BindingRules.Position);
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_MMF/Data/Weapons/WorkshopRuntimeTest.asset");
            AssetDatabase.CreateAsset(skill, path); loadout.EquipPlayerOne(skill); EditorUtility.SetDirty(loadout); AssetDatabase.SaveAssetIfDirty(loadout);
            var testMouse = InputSystem.AddDevice<Mouse>();
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
                loadout.ShowAttackPreview = true;
                InputSystem.QueueDeltaStateEvent(Mouse.current.position, (Vector2)Camera.main.WorldToScreenPoint(target.Position));
                InputSystem.Update();
                yield return null;
                bool PreviewVisible() => player.GetComponentsInChildren<LineRenderer>().Any(line => line.name == "武器攻击预览" && line.enabled);
                Assert.That(PreviewVisible(), Is.True);
                loadout.ShowAttackPreview = false; yield return null;
                Assert.That(PreviewVisible(), Is.False);
                loadout.ShowAttackPreview = true; yield return null;
                Assert.That(PreviewVisible(), Is.True);
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

                float beforeSpecial = target.CurrentHp;
                Assert.That(player.PressWorkshopInput(SkillInputKey.MouseRight, target.Position), Is.False);
                player.ReleaseWorkshopInput(SkillInputKey.MouseLeft, target.Position);
                Assert.That(target.CurrentHp, Is.EqualTo(beforeSpecial));
                player.ReleaseWorkshopInput(SkillInputKey.MouseRight, target.Position);
                Assert.That(target.CurrentHp, Is.EqualTo(beforeSpecial));
                skill.AttackMode = 0;
                Assert.That(player.PressWorkshopInput(SkillInputKey.MouseRight, target.Position), Is.False);
                var backpack = player.GetComponent<PlayerBackpack>();
                var weaponItem = backpack.ItemAt(backpack.Model.SelectedSlot);
                Assert.That(weaponItem.WorkshopWeapon, Is.SameAs(skill));
                backpack.SelectSlot(1); Assert.That(controller.HasWeapon, Is.False);
                yield return null; Assert.That(PreviewVisible(), Is.False);
                backpack.SelectSlot(0); Assert.That(controller.HasWeapon, Is.True);
                yield return null; Assert.That(PreviewVisible(), Is.True);
                var throwing = player.GetComponent<PlayerThrow>();
                Assert.That(throwing.TryStartCharge(), Is.True);
                throwing.Release();
                Assert.That(throwing.Charging, Is.False);
                yield return null; Assert.That(PreviewVisible(), Is.False);
                Assert.That(backpack.ItemAt(backpack.Model.SelectedSlot), Is.Null);
                Assert.That(controller.HasWeapon, Is.False);
                Assert.That(player.TryAttack(target.Position), Is.False);
                backpack.TryAdd(weaponItem);
                Assert.That(controller.HasWeapon, Is.True);
                loadout.EquipPlayerOne(null);
                Assert.That(controller.HasWeapon, Is.False);
                int swings = 0; player.Swung += (weapon, aim) => swings++;
                // No default frying pan remains after unequipping the workshop weapon.
                Assert.That(player.TryAttack((Vector2)player.transform.position + Vector2.right), Is.False);
                Assert.That(swings, Is.Zero);
            }
            finally
            {
                InputSystem.RemoveDevice(testMouse);
                loadout.ShowAttackPreview = previousPreview; loadout.EquipPlayerOne(previous); EditorUtility.SetDirty(loadout); AssetDatabase.SaveAssetIfDirty(loadout);
                AssetDatabase.DeleteAsset(path); if (created) AssetDatabase.DeleteAsset(loadoutPath);
            }
        }
    }
}
#endif
