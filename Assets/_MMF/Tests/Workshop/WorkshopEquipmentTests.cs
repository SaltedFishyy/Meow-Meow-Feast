using System.Collections;
using System.Collections.Generic;
using AbilityStudio.Model;
using MMF.Combat;
using MMF.Data;
using MMF.Editor.WeaponWorkshop;
using MMF.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace MMF.Tests.Workshop
{
    public sealed class WorkshopEquipmentTests
    {
        private bool _hadLoadout;
        private SkillDefinition _previous;
        private readonly List<string> _assets = new();
        private readonly List<Object> _objects = new();
        [SetUp] public void Setup()
        {
            var loadout = WorkshopEquipment.Load(); _hadLoadout = loadout != null;
            _previous = loadout == null ? null : loadout.PlayerOne;
        }
        [TearDown] public void Cleanup()
        {
            WorkshopEquipment.Equip(_previous);
            foreach (var obj in _objects) if (obj != null) Object.DestroyImmediate(obj);
            _objects.Clear();
            foreach (var path in _assets) AssetDatabase.DeleteAsset(path);
            _assets.Clear();
            if (!_hadLoadout) AssetDatabase.DeleteAsset(WorkshopEquipment.AssetPath);
        }
        private SkillDefinition Weapon(string name)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>(); skill.Id = "workshop_test";
            skill.DisplayName = name; skill.SetAttack(new CircleAttack { Radius = 3 }); skill.Cooldown = 0;
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_MMF/Data/Weapons/" + name + ".asset");
            AssetDatabase.CreateAsset(skill, path); _assets.Add(path); return skill;
        }
        [Test] public void EquipSwitchUnequipPersistsOnePlayerSlot()
        {
            var first = Weapon("WorkshopTestA"); var second = Weapon("WorkshopTestB");
            WorkshopEquipment.Equip(first);
            Assert.That(WorkshopEquipment.IsEquipped(first), Is.True);
            Assert.That(Resources.Load<WorkshopLoadout>(WorkshopLoadout.ResourceName).PlayerOne, Is.SameAs(first));
            WorkshopEquipment.Equip(second);
            Assert.That(WorkshopEquipment.IsEquipped(first), Is.False);
            Assert.That(WorkshopEquipment.IsEquipped(second), Is.True);
            AssetDatabase.ImportAsset(WorkshopEquipment.AssetPath, ImportAssetOptions.ForceUpdate);
            Assert.That(WorkshopEquipment.Load().PlayerOne, Is.SameAs(second));
            WorkshopEquipment.Equip(null); Assert.That(WorkshopEquipment.Load().PlayerOne, Is.Null);
        }
        [UnityTest] public IEnumerator EquipmentRowFollowsNameAndListBadgeTracksSelection()
        {
            var weapon = Weapon("WorkshopUITest"); WorkshopEquipment.Equip(weapon);
            var window = ScriptableObject.CreateInstance<WeaponWorkshopWindow>(); _objects.Add(window); window.Show();
            yield return null;
            var list = window.rootVisualElement.Q<ListView>();
            int index = list.itemsSource.IndexOf(weapon); Assert.That(index, Is.GreaterThanOrEqualTo(0)); list.SetSelection(index);
            yield return new WaitForSecondsRealtime(.5f);
            var name = window.rootVisualElement.Q<TextField>("displayName");
            var row = window.rootVisualElement.Q("main-scene-equipment");
            Assert.That(row, Is.Not.Null); Assert.That(row.parent, Is.SameAs(name.parent));
            Assert.That(row.parent.IndexOf(row), Is.EqualTo(name.parent.IndexOf(name) + 1));
            Assert.That(row.Q<Button>("equip-p1").text, Is.EqualTo("卸下"));
            var item = list.makeItem(); list.bindItem(item, index);
            Assert.That(item.Q<Label>("equipment-badge").text, Is.EqualTo("P1 已装备"));
            WorkshopEquipment.Equip(null); yield return new WaitForSecondsRealtime(.5f);
            Assert.That(row.Q<Button>("equip-p1").text, Is.EqualTo("装备给 P1"));
            list.bindItem(item, index); Assert.That(item.Q<Label>("equipment-badge").text, Is.Empty);
            window.Close();
        }
        [Test] public void PlayerHealingCapsWithoutReviving()
        {
            var go = new GameObject("Health"); go.SetActive(false); _objects.Add(go);
            var health = go.AddComponent<PlayerHealth>();
            typeof(PlayerHealth).GetProperty("Current").SetValue(health, 1);
            typeof(PlayerHealth).GetProperty("Max").SetValue(health, 3);
            health.Heal(int.MaxValue); Assert.That(health.Current, Is.EqualTo(3));
            typeof(PlayerHealth).GetProperty("Current").SetValue(health, 0);
            health.Heal(3); Assert.That(health.Current, Is.Zero);
        }
    }
}
