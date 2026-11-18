using System.Collections;
using System.Linq;
using AbilityStudio.Editor;
using AbilityStudio.Model;
using AbilityStudio.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace AbilityStudio.Tests
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PreviewPrefabProbe : MonoBehaviour
    {
        public static int Calls;
        private void Awake() { Calls++; }
        private void OnEnable() { Calls++; }
    }

    public sealed class IndependentPreviewTests
    {
        private SkillDefinition CreateSkill(bool charged = false)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>(); skill.IntegratedAttacks = true;
            var normal = new ConeAttack { Range = 15, AngleDegrees = 120, ReleaseMode = charged ? 1 : 0 };
            normal.Settings.BaseDamage = 1; normal.Settings.Cooldown = .5f; normal.Settings.Duration = 1;
            var special = new CircleAttack { Radius = 3 }; special.Settings.BaseDamage = 2;
            skill.AddAttack(normal); skill.AddAttack(special);
            skill.SetBinding(SkillDefinition.CoreLayoutId, normal.ModuleId, BindingRules.Trigger, BindingRules.Normal);
            skill.SetBinding(SkillDefinition.CoreLayoutId, special.ModuleId, BindingRules.Trigger, BindingRules.Special);
            return skill;
        }
        [Test] public void PreviewKeepsHostSceneAndSkillUntouchedAndDisposesItsScene()
        {
            var active = SceneManager.GetActiveScene(); int count = SceneManager.sceneCount;
            var roots = active.GetRootGameObjects(); bool dirty = active.isDirty;
            var skill = CreateSkill(); string before = EditorJsonUtility.ToJson(skill);
            Scene privateScene;
            try
            {
                using (var world = new PlaygroundSimulation())
                {
                    privateScene = world.Scene; world.Bind(skill);
                    Assert.That(EditorSceneManager.IsPreviewScene(privateScene), Is.True);
                    Assert.That(world.Press(SkillKind.NormalAttack), Is.EqualTo(CastResult.Success));
                    world.Tick(.1f); Assert.That(world.Targets.Any(t => t.CurrentHp < 10), Is.True);
                    Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(active));
                    Assert.That(SceneManager.sceneCount, Is.EqualTo(count));
                    Assert.That(active.GetRootGameObjects(), Is.EquivalentTo(roots)); Assert.That(active.isDirty, Is.EqualTo(dirty));
                    Assert.That(EditorJsonUtility.ToJson(skill), Is.EqualTo(before));
                }
                Assert.That(privateScene.IsValid(), Is.False);
            }
            finally { Object.DestroyImmediate(skill); }
        }
        [Test] public void IndependentWorldsDoNotShareTargetsClocksOrCooldowns()
        {
            var skill = CreateSkill();
            try
            {
                using (var a = new PlaygroundSimulation()) using (var b = new PlaygroundSimulation())
                {
                    a.Bind(skill); b.Bind(skill); a.Press(SkillKind.NormalAttack); a.Tick(.2f);
                    Assert.That(b.Time, Is.Zero); Assert.That(b.Targets.All(t => t.CurrentHp == 10), Is.True);
                    Assert.That(b.Executor.CooldownRemaining(skill, b, SkillKind.NormalAttack), Is.Zero);
                    Assert.That(a.Executor.CooldownRemaining(skill, a, SkillKind.NormalAttack), Is.GreaterThan(0));
                    a.Reset(); Assert.That(a.Targets.All(t => t.CurrentHp == 10), Is.True);
                }
            }
            finally { Object.DestroyImmediate(skill); }
        }
        [Test] public void ChargeCancelReleaseAndLiveEditsUseSharedRuntime()
        {
            var skill = CreateSkill(true);
            try
            {
                using (var world = new PlaygroundSimulation())
                {
                    world.Bind(skill); Assert.That(world.Press(SkillKind.NormalAttack), Is.EqualTo(CastResult.Charging));
                    world.Tick(.5f); Assert.That(world.ChargeRatio, Is.EqualTo(.5f));
                    Assert.That(world.Press(SkillKind.SpecialAttack), Is.EqualTo(CastResult.Busy));
                    world.Cancel(); Assert.That(world.Release(), Is.EqualTo(CastResult.Invalid));
                    Assert.That(world.Executor.CooldownRemaining(skill, world), Is.Zero);
                    skill.AttackFor(SkillKind.NormalAttack).Settings.BaseDamage = 3;
                    world.Press(SkillKind.NormalAttack); world.Tick(1); world.Release();
                    Assert.That(world.Targets[1].CurrentHp, Is.EqualTo(7));
                    world.Reset(); world.Press(SkillKind.SpecialAttack);
                    Assert.That(world.Targets[1].CurrentHp, Is.EqualTo(8));
                }
            }
            finally { Object.DestroyImmediate(skill); }
        }
        [Test] public void FocusLossCancelsHeldPreviewInput()
        {
            var skill = CreateSkill(true); var window = ScriptableObject.CreateInstance<PlaygroundWindow>();
            try
            {
                using (var renderer = new PlaygroundPreviewRenderer())
                {
                    renderer.Simulation.Bind(skill); renderer.Simulation.Press(SkillKind.NormalAttack);
                    var field = typeof(PlaygroundWindow).GetField("_preview", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field.SetValue(window, renderer);
                    typeof(PlaygroundWindow).GetMethod("OnLostFocus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(window, null);
                    Assert.That(renderer.Simulation.IsCharging, Is.False);
                    field.SetValue(window, null);
                }
            }
            finally { Object.DestroyImmediate(window); Object.DestroyImmediate(skill); }
        }
        [Test] public void FeedbackStaysPrivateAndPrefabScriptsNeverRun()
        {
            var skill = CreateSkill(); var source = new GameObject("Visual source"); source.SetActive(false);
            source.AddComponent<PreviewPrefabProbe>(); source.SetActive(true); PreviewPrefabProbe.Calls = 0;
            var effect = new HitVfxFeedback { Prefab = source, Lifetime = .2f };
            skill.AddEffect(effect); skill.SetBinding(skill.Attack.ModuleId, effect.ModuleId, BindingRules.Targets);
            try
            {
                using (var renderer = new PlaygroundPreviewRenderer())
                {
                    renderer.Simulation.Bind(skill); renderer.Simulation.Press(SkillKind.NormalAttack);
                    Assert.That(renderer.Feedback.ActiveVisuals, Is.GreaterThan(0));
                    Assert.That(PreviewPrefabProbe.Calls, Is.Zero);
                    Assert.That(renderer.Simulation.Root.GetComponentsInChildren<PreviewPrefabProbe>(true), Is.Empty);
                    Assert.That(renderer.Simulation.Root.GetComponentsInChildren<Collider2D>(true), Is.Empty);
                    renderer.Feedback.Tick(.3f); Assert.That(renderer.Feedback.ActiveVisuals, Is.Zero);
                    renderer.Simulation.Reset(); Assert.That(renderer.Feedback.LastMessage, Is.Empty);
                }
            }
            finally { Object.DestroyImmediate(source); Object.DestroyImmediate(skill); }
        }
        [Test] public void RendererAndPointerProjectionWorkWithMain2DRenderPipeline()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) Assert.Ignore("Requires a graphics device for the offscreen render.");
            var skill = CreateSkill();
            try
            {
                using (var renderer = new PlaygroundPreviewRenderer())
                {
                    renderer.Simulation.Bind(skill);
                    var texture = renderer.Render(800, 600) as RenderTexture;
                    Assert.That(texture, Is.Not.Null);
                    Assert.That(renderer.GroundPoint(new Vector2(400, 300), new Vector2(800, 600), out var point), Is.True);
                    Assert.That(point.magnitude, Is.LessThan(.001f));
                    Assert.That(renderer.GroundPoint(new Vector2(400, 100), new Vector2(800, 600), out var above), Is.True);
                    Assert.That(above.y, Is.GreaterThan(0)); Assert.That(above.z, Is.EqualTo(0).Within(.001f));
                    var pixels = new Texture2D(800, 600, TextureFormat.RGB24, false);
                    var previous = RenderTexture.active;
                    try
                    {
                        RenderTexture.active = texture; pixels.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); pixels.Apply();
                        Assert.That(pixels.GetPixels().Count(c => c.g > .3f && c.b > .3f), Is.GreaterThan(20));
                        System.IO.File.WriteAllBytes("../preview.png", pixels.EncodeToPNG());
                    }
                    finally { RenderTexture.active = previous; Object.DestroyImmediate(pixels); }
                }
            }
            finally { Object.DestroyImmediate(skill); }
        }
        [UnityTest] public IEnumerator PreviewOperatesWhileMainGameIsPlayingEvenWithZeroTimeScale()
        {
            yield return new EnterPlayMode();
            var active = SceneManager.GetActiveScene(); var skill = CreateSkill();
            var host = new GameObject("Main game sentinel"); host.transform.position = new Vector3(50, 60, 0);
            var camera = host.AddComponent<Camera>(); camera.enabled = false;
            int roots = active.rootCount; float oldScale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                using (var preview = new PlaygroundPreviewRenderer())
                {
                    preview.Simulation.Bind(skill); preview.Simulation.Press(SkillKind.NormalAttack);
                    preview.Simulation.Tick(.25f, Vector2.right);
                    Assert.That(preview.Simulation.Time, Is.EqualTo(.25).Within(.0001));
                    Assert.That(preview.Simulation.Targets[1].CurrentHp, Is.EqualTo(9));
                    Assert.That(host.transform.position, Is.EqualTo(new Vector3(50, 60, 0)));
                    Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(active)); Assert.That(active.rootCount, Is.EqualTo(roots));
                    Assert.That(Time.timeScale, Is.Zero); Assert.That(EditorApplication.isPlaying, Is.True);
                    if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) Assert.That(preview.Render(640, 480), Is.Not.Null);
                }
            }
            finally { Time.timeScale = oldScale; Object.DestroyImmediate(host); Object.DestroyImmediate(skill); }
            yield return new ExitPlayMode();
        }
    }
}
