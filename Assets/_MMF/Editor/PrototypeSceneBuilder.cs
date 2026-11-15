using System.IO;
using MMF.Core;
using MMF.Player;
using MMF.Data;
using MMF.UI;
using MMF.Truck;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MMF.Editor
{
    /// <summary>Builds the map, truck, follow camera and backpack prototype from source assets.</summary>
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
            // Load assets after replacing the scene so scene unloading cannot invalidate them.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Sprite sprite = LoadPlaceholder();
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (controls == null || controls.FindAction("Gameplay/Move") == null)
            {
                Debug.LogError("Cannot build prototype: MMFControls/Gameplay/Move is missing.");
                return;
            }
            string[] requiredActions = { "SelectSlot1", "SelectSlot2", "SelectSlot3", "SelectSlot4", "SelectSlot5", "ScrollSlot", "Drop" };
            foreach (string action in requiredActions)
                if (controls.FindAction("Gameplay/" + action) == null)
                {
                    Debug.LogError("Cannot build prototype: missing Gameplay/" + action);
                    return;
                }
            if (sprite == null)
            {
                Debug.LogError("Cannot build prototype: placeholder sprite is missing.");
                return;
            }

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

            MapBounds arena = bounds.AddComponent<MapBounds>();
            SerializedObject arenaData = new SerializedObject(arena);
            arenaData.FindProperty("_camera").objectReferenceValue = camera;
            arenaData.FindProperty("_ground").objectReferenceValue = ground.transform;
            arenaData.FindProperty("_left").objectReferenceValue = left;
            arenaData.FindProperty("_right").objectReferenceValue = right;
            arenaData.FindProperty("_top").objectReferenceValue = top;
            arenaData.FindProperty("_bottom").objectReferenceValue = bottom;
            arenaData.ApplyModifiedPropertiesWithoutUndo();


            PlayerBackpack backpack = CreatePlayer(sprite, controls);
            TruckArea truck = CreateTruck(sprite, backpack.transform);
            FollowCamera follow = camera.gameObject.AddComponent<FollowCamera>();
            SetReference(follow, "_target", backpack.transform);
            SetReference(follow, "_map", arena);
            SetReference(follow, "_truck", truck);
            RectTransform row = CreateBackpackContent(sprite, backpack);
            SetReference(arena, "_follow", follow);
            SetReference(arena, "_backpackRow", row);
            Canvas.ForceUpdateCanvases();
            arena.RefreshLayout();
            ValidateReferences(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save " + ScenePath);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateReferences(scene);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Prototype scene rebuilt: " + ScenePath);
        }

        private static TruckArea CreateTruck(Sprite sprite, Transform player)
        {
            TruckArea truck = new GameObject("Truck").AddComponent<TruckArea>();
            SpriteRenderer floor = CreateSprite("Truck Floor", sprite, new Color(0.7f, 0.67f, 0.52f), 0);
            floor.transform.SetParent(truck.transform, false);
            SetReference(truck, "_floor", floor.transform);
            SetReference(truck, "_player", player);
            SerializedObject data = new SerializedObject(truck);
            SerializedProperty walls = data.FindProperty("_walls");
            walls.arraySize = 6;
            for (int i = 0; i < walls.arraySize; i++)
            {
                SpriteRenderer wall = CreateSprite("Truck Wall " + i, sprite, new Color(0.12f, 0.14f, 0.16f), 1);
                wall.transform.SetParent(truck.transform, false);
                wall.gameObject.AddComponent<BoxCollider2D>();
                walls.GetArrayElementAtIndex(i).objectReferenceValue = wall.transform;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            truck.RefreshLayout();
            return truck;
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

        private static PlayerBackpack CreatePlayer(Sprite sprite, InputActionAsset controls)
        {
            GameObject player = CreateSprite("Player", sprite,
                new Color(1f, 0.69f, 0.24f), 2).gameObject;
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>().size = Vector2.one;
            player.AddComponent<PlayerMovement>();
            PlayerBackpack backpack = player.AddComponent<PlayerBackpack>();
            SpriteRenderer icon = CreateSprite("Held Item", sprite, Color.white, 2);
            icon.transform.SetParent(player.transform, false);
            icon.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            icon.transform.localScale = Vector3.one * 0.25f;
            icon.enabled = false;
            SetReference(backpack, "_itemSprite", sprite);
            SetReference(backpack, "_heldIcon", icon);
            PlayerInputReader reader = player.AddComponent<PlayerInputReader>();
            SerializedObject readerData = new SerializedObject(reader);
            readerData.FindProperty("_controls").objectReferenceValue = controls;
            readerData.ApplyModifiedPropertiesWithoutUndo();
            return backpack;
        }

        private static void SetReference(Object target, string property, Object value)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(property).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ItemDefinition LoadItem(string id, string displayName, Color color)
        {
            string path = Root + "/Data/Items/" + id + ".asset";
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item != null) return item;
            item = ScriptableObject.CreateInstance<ItemDefinition>();
            SerializedObject data = new SerializedObject(item);
            data.FindProperty("_id").stringValue = id;
            data.FindProperty("_displayName").stringValue = displayName;
            data.FindProperty("_color").colorValue = color;
            data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        private static RectTransform CreateBackpackContent(Sprite sprite, PlayerBackpack backpack)
        {
            EnsureFolder(Root + "/Data/Items");
            ItemDefinition meat = LoadItem("MonsterMeat", "怪肉", new Color(0.85f, 0.2f, 0.2f));
            ItemDefinition mushroom = LoadItem("Mushroom", "蘑菇", new Color(0.85f, 0.75f, 0.55f));
            Vector2[] positions =
            {
                new Vector2(-8f, 1f), new Vector2(9f, -2f),
                new Vector2(-24f, 12f), new Vector2(24f, -11f),
                new Vector2(2f, 6f), new Vector2(-12f, -8f),
                new Vector2(24f, 12f), new Vector2(-24f, -11f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                ItemDefinition item = i < 4 ? meat : mushroom;

                SpriteRenderer renderer = CreateSprite(item.name + " " + i, sprite, item.Color, 1);
                renderer.transform.position = (Vector3)positions[i];
                renderer.transform.localScale = Vector3.one * 0.4f;
                WorldItem world = renderer.gameObject.AddComponent<WorldItem>();
                SetReference(world, "_item", item);
                SetReference(world, "_renderer", renderer);
            }
            GameObject canvasObject = new GameObject("Backpack Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            RectTransform row = new GameObject("Backpack Row", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(canvasObject.transform, false);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0f);
            row.pivot = new Vector2(0.5f, 0f);
            row.anchoredPosition = new Vector2(0f, 20f);
            row.sizeDelta = new Vector2(582f, 72f);
            BackpackBar bar = canvasObject.AddComponent<BackpackBar>();
            SetReference(bar, "_backpack", backpack);
            SetReference(bar, "_row", row);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            SetReference(bar, "_font", font);
            if (font == null || !font.HasCharacter('怪') || !font.HasCharacter('肉') ||
                !font.HasCharacter('蘑') || !font.HasCharacter('菇') || !font.HasCharacter('空'))
                Debug.LogError("Backpack font cannot display the required Chinese characters.");
            AssetDatabase.SaveAssets();
            return row;
        }

        private static void ValidateReferences(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) throw new System.InvalidOperationException("Missing script under " + root.name);
                if (component.GetType().Namespace == null || !component.GetType().Namespace.StartsWith("MMF.")) continue;
                SerializedProperty property = new SerializedObject(component).GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                        throw new System.InvalidOperationException(component.name + "/" + component.GetType().Name + ": missing " + property.propertyPath);
            }
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
