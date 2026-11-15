using System.IO;
using MMF.Core;
using MMF.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace MMF.Editor
{
    /// <summary>Builds the disposable T001 prototype scene from source assets.</summary>
    public static class PrototypeSceneBuilder
    {
        private const string Root = "Assets/_MMF";
        private const string ScenePath = Root + "/Scenes/Prototype.unity";
        private const string SpritePath = Root + "/Art/Placeholder/WhiteSquare.png";
        private const string InputPath = Root + "/Input/MMFControls.inputactions";
        private const int TextureSize = 16;
        private const float CameraSize = 5f;

        /// <summary>Rebuilds, saves and opens the prototype scene; safe to invoke repeatedly.</summary>
        [MenuItem("MMF/Build Prototype Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("Stop Play mode before building the prototype scene.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Art/Placeholder");
            Sprite sprite = LoadPlaceholder();
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (controls == null || controls.FindAction("Gameplay/Move") == null)
            {
                Debug.LogError("Cannot build prototype: MMFControls/Gameplay/Move is missing.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();
            Light2D light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;

            SpriteRenderer ground = CreateSprite("Ground", sprite,
                new Color(0.22f, 0.32f, 0.29f), -1);
            GameObject bounds = new GameObject("Bounds");
            BoxCollider2D left = CreateWall("Left", bounds.transform);
            BoxCollider2D right = CreateWall("Right", bounds.transform);
            BoxCollider2D top = CreateWall("Top", bounds.transform);
            BoxCollider2D bottom = CreateWall("Bottom", bounds.transform);

            PrototypeArena arena = bounds.AddComponent<PrototypeArena>();
            SerializedObject arenaData = new SerializedObject(arena);
            arenaData.FindProperty("_camera").objectReferenceValue = camera;
            arenaData.FindProperty("_ground").objectReferenceValue = ground.transform;
            arenaData.FindProperty("_left").objectReferenceValue = left;
            arenaData.FindProperty("_right").objectReferenceValue = right;
            arenaData.FindProperty("_top").objectReferenceValue = top;
            arenaData.FindProperty("_bottom").objectReferenceValue = bottom;
            arenaData.ApplyModifiedPropertiesWithoutUndo();
            arena.RefreshLayout();

            CreatePlayer(sprite, controls);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save " + ScenePath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Prototype scene rebuilt: " + ScenePath);
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.32f, 0.29f);
            return camera;
        }

        private static void CreatePlayer(Sprite sprite, InputActionAsset controls)
        {
            GameObject player = CreateSprite("Player", sprite,
                new Color(1f, 0.69f, 0.24f), 0).gameObject;
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>().size = Vector2.one;
            player.AddComponent<PlayerMovement>();
            PlayerInputReader reader = player.AddComponent<PlayerInputReader>();
            SerializedObject readerData = new SerializedObject(reader);
            readerData.FindProperty("_controls").objectReferenceValue = controls;
            readerData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SpriteRenderer CreateSprite(string name, Sprite sprite, Color color, int order)
        {
            SpriteRenderer renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static BoxCollider2D CreateWall(string name, Transform parent)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            return wall.AddComponent<BoxCollider2D>();
        }

        private static Sprite LoadPlaceholder()
        {
            if (!File.Exists(SpritePath))
            {
                Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
                try
                {
                    Color32[] pixels = new Color32[TextureSize * TextureSize];
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = new Color32(255, 255, 255, 255);
                    texture.SetPixels32(pixels);
                    texture.Apply();
                    File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
                }
                finally
                {
                    Object.DestroyImmediate(texture);
                }
            }

            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = TextureSize;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
