using System.Collections;
using AbilityStudio.Model;
using AbilityStudio.Playground;
using AbilityStudio.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbilityStudio.Tests
{
    public sealed class XY2DPlaygroundTests
    {
        [UnityTest]
        public IEnumerator ArenaHitsUpwardWith2DCollidersAndResetsFeedback()
        {
            var root = new GameObject("2D test world");
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            try
            {
                var camera = new GameObject("Camera").AddComponent<Camera>(); camera.transform.SetParent(root.transform);
                camera.transform.position = new Vector3(0, 0, -20); camera.orthographic = true;
                var runtime = new GameObject("Runtime").AddComponent<SkillRuntimeHost>(); runtime.transform.SetParent(root.transform);
                var player = new GameObject("Player").AddComponent<PlaygroundPlayer>(); player.transform.SetParent(root.transform);
                skill.SetAttack(new ProjectileAttack { Speed = 15, Range = 12 }); skill.BaseDamage = 1;
                var recoil = new RecoilFeedback(); var shake = new CameraShakeFeedback();
                foreach (var effect in new HitFeedbackEffect[] { recoil, shake })
                { skill.AddEffect(effect); skill.SetBinding(skill.Attack.ModuleId, effect.ModuleId, BindingRules.Targets); }
                player.Configure(skill, runtime, camera); player.InputBlocked = true;
                var arena = new GameObject("Arena").AddComponent<PlaygroundArena>(); arena.transform.SetParent(root.transform);
                arena.Configure(player, runtime, camera, material, material, material, "2D 验证");
                yield return null; yield return null;
                Assert.That(arena.Dummies.Count, Is.EqualTo(3));
                Assert.That(root.GetComponentsInChildren<Collider>(), Is.Empty);
                Assert.That(root.GetComponentsInChildren<CircleCollider2D>().Length, Is.EqualTo(3));
                var dummy = arena.Dummies[0];
                Assert.That(dummy.Position, Is.EqualTo(new Vector3(0, 5, 0)));
                Assert.That(player.CastAt(dummy.Position), Is.EqualTo(CastResult.Success));
                runtime.Tick(.4f);
                Assert.That(dummy.CurrentHp, Is.EqualTo(9));
                var view = dummy.GetComponentInChildren<PlaygroundRecoil>();
                Assert.That(view.IsActive, Is.True);
                Assert.That(dummy.transform.rotation, Is.EqualTo(Quaternion.identity));
                Assert.That(dummy.Position.z, Is.Zero);
                arena.ResetArena();
                Assert.That(dummy.CurrentHp, Is.EqualTo(10)); Assert.That(view.IsActive, Is.False);
                Assert.That(runtime.Executor.Projectiles, Is.Empty);
                Assert.That(arena.GetComponent<PlaygroundHitFeedback>().CameraOffset, Is.EqualTo(Vector3.zero));
            }
            finally { Object.Destroy(root); Object.Destroy(skill); Object.Destroy(material); }
            yield return null;
        }
    }
}
