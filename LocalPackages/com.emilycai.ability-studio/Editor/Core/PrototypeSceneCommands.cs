using System.IO;
using AbilityStudio.Model;
using AbilityStudio.Playground;
using AbilityStudio.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AbilityStudio.Editor
{
    /// <summary>Creates host-project sample assets without overwriting existing scenes or settings.</summary>
    public static class PrototypeSceneCommands
    {
        /// <summary>Shared sample skill asset path.</summary>
        public const string SkillPath = "Assets/_Sandbox/Skills/GrapplingHook.asset";
        /// <summary>Playable preview scene path.</summary>
        public const string PlaygroundPath = "Assets/_Sandbox/Playground2D/AbilityPlayground2D.unity";
        /// <summary>Independent gameplay scene path.</summary>
        public const string GameplayPath = "Assets/_Sandbox/Gameplay2D/AbilityGameplay2D.unity";
        /// <summary>Creates missing prototype assets, callable from batch mode.</summary>
        [MenuItem("Tools/技能工坊/创建缺失的测试资源")]
        public static void CreateMissingAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateAssets(true);
            Debug.Log("Ability Studio prototype assets ready. Both scenes reference " + SkillPath);
        }
        private static void CreateAssets(bool includeGameplay)
        {
            EnsureFolder("Assets/_Sandbox/Skills"); EnsureFolder("Assets/_Sandbox/Playground2D");
            if (includeGameplay) EnsureFolder("Assets/_Sandbox/Gameplay2D");
            EnsureFolder("Assets/_Sandbox/Playground2D/Materials");
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(SkillPath);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>();
                skill.Id = "grappling_hook"; skill.DisplayName = "抓钩"; skill.Cooldown = 6; skill.BaseDamage = 2;
                skill.SetAttack(new ProjectileAttack { Speed = 15, Range = 18, Size = 0.25f }); skill.AddEffect(new PullEffect());
                skill.SetBinding(skill.Attack.ModuleId, skill.Effects[0].ModuleId, BindingRules.Targets);
                SkillAssetCommands.Create(skill, SkillPath);
            }
            var floor = Material("Floor", new Color(0.07f, 0.12f, 0.17f));
            var grid = Material("Grid", new Color(0.12f, 0.22f, 0.27f));
            var player = Material("Player", new Color(0.15f, 0.8f, 0.9f));
            var dummy = Material("Dummy", new Color(0.95f, 0.34f, 0.18f));
            var shot = Material("Projectile", new Color(0.3f, 1, 0.75f));
            var line = Material("Line", Color.white, true);
            CreateScene(PlaygroundPath, skill, floor, grid, player, dummy, shot, line, false);
            if (includeGameplay) CreateScene(GameplayPath, skill, floor, grid, player, dummy, shot, line, true);
        }
        /// <summary>Selects the shared asset for live Inspector editing.</summary>
        [MenuItem("Tools/技能工坊/编辑示例技能")]
        public static void EditSkill() => Selection.activeObject = AssetDatabase.LoadAssetAtPath<SkillDefinition>(SkillPath);
        /// <summary>Opens the standalone playground without changing build settings.</summary>
        [MenuItem("Tools/技能工坊/打开测试场景")]
        public static void OpenPlayground() => TryOpenPlayground();
        /// <summary>Creates a missing playground, then opens it; cancellation never enters Play Mode.</summary>
        public static bool TryOpenPlayground()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return false;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (!File.Exists(PlaygroundPath)) CreateAssets(false);
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != PlaygroundPath)
                EditorSceneManager.OpenScene(PlaygroundPath);
            return true;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
        private static Material Material(string name, Color color, bool unlit = false)
        {
            string path = "Assets/_Sandbox/Playground2D/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path); if (existing != null) return existing;
            if (File.Exists(path)) throw new System.InvalidOperationException("Refusing to overwrite " + path);
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new System.InvalidOperationException("The prototype requires the installed URP shaders.");
            var material = new Material(shader) { name = name }; material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.25f);
            AssetDatabase.CreateAsset(material, path); return material;
        }
        private static void CreateScene(string path, SkillDefinition skill, Material floor, Material grid,
            Material playerMaterial, Material dummy, Material shot, Material line, bool gameplay)
        {
            if (File.Exists(path)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -20); camera.transform.rotation = Quaternion.identity;
            camera.orthographic = true; camera.orthographicSize = 14; camera.nearClipPlane = 0.1f; camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.025f, 0.04f, 0.07f);
            camera.gameObject.AddComponent<AudioListener>();
            Quad("Arena floor", new Vector3(0, 0, 1), new Vector3(28, 22, 1), floor);
            for (int x = -14; x <= 14; x += 2) Quad("Grid", new Vector3(x, 0, .9f), new Vector3(.025f, 22, 1), grid);
            for (int y = -11; y <= 11; y += 2) Quad("Grid", new Vector3(0, y, .9f), new Vector3(28, .025f, 1), grid);
            Quad("North border", new Vector3(0, 11, 0), new Vector3(28, .2f, 1), playerMaterial);
            Quad("South border", new Vector3(0, -11, 0), new Vector3(28, .2f, 1), playerMaterial);
            Quad("West border", new Vector3(-14, 0, 0), new Vector3(.2f, 22, 1), playerMaterial);
            Quad("East border", new Vector3(14, 0, 0), new Vector3(.2f, 22, 1), playerMaterial);
            var runtime = new GameObject("Shared Skill Runtime").AddComponent<SkillRuntimeHost>();
            var player = new GameObject("Player").AddComponent<PlaygroundPlayer>(); player.transform.position = new Vector3(gameplay ? -3 : 0, -5, 0);
            var body = Quad("Player body", player.transform.position, Vector3.one, playerMaterial);
            body.transform.SetParent(player.transform, true);
            var pointer = Quad("Facing marker", player.transform.position + new Vector3(.8f, 0, -.1f), new Vector3(.7f, .3f, 1), shot);
            pointer.transform.SetParent(player.transform, true);
            player.Configure(skill, runtime, camera);
            var arena = new GameObject("Arena controls and feedback").AddComponent<PlaygroundArena>();
            arena.Configure(player, runtime, camera, dummy, shot, line, gameplay ? "技能工坊 / 2D 游戏示例" : "技能工坊 / 2D 测试场景");
            foreach (var root in scene.GetRootGameObjects()) Undo.RegisterCreatedObjectUndo(root, "Create prototype scene");
            if (!EditorSceneManager.SaveScene(scene, path)) throw new System.IO.IOException("Could not save " + path);
        }
        private static GameObject Quad(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad); go.name = name; go.transform.position = position; go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
    }
}
