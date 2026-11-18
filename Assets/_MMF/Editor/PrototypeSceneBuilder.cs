using System.IO;
using MMF.Core;
using MMF.Combat;
using MMF.Cooking;
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
            string[] requiredActions = { "SelectSlot1", "SelectSlot2", "SelectSlot3", "SelectSlot4", "SelectSlot5", "ScrollSlot", "Drop", "Interact", "Cancel", "Lift", "Throw", "FryW", "FryA", "FryS", "FryD", "Restart" };
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
            arenaData.FindProperty("_tuning").objectReferenceValue = LoadMapTuning();
            arenaData.FindProperty("_camera").objectReferenceValue = camera;
            arenaData.FindProperty("_ground").objectReferenceValue = ground.transform;
            arenaData.FindProperty("_left").objectReferenceValue = left;
            arenaData.FindProperty("_right").objectReferenceValue = right;
            arenaData.FindProperty("_top").objectReferenceValue = top;
            arenaData.FindProperty("_bottom").objectReferenceValue = bottom;
            arenaData.ApplyModifiedPropertiesWithoutUndo();


            PlayerBackpack backpack = CreatePlayer(sprite, controls);
            TruckArea truck = CreateTruck(sprite);
            SetReference(backpack.GetComponent<PlayerDowned>(), "_truck", truck);
            SerializedObject throwData = new SerializedObject(backpack.GetComponent<PlayerThrow>());
            SerializedProperty throwWalls = throwData.FindProperty("_walls");
            SerializedObject truckData = new SerializedObject(truck);
            SerializedProperty truckWalls = truckData.FindProperty("_walls");
            throwWalls.arraySize = truckWalls.arraySize + 4;
            for (int i = 0; i < truckWalls.arraySize; i++)
                throwWalls.GetArrayElementAtIndex(i).objectReferenceValue =
                    ((Transform)truckWalls.GetArrayElementAtIndex(i).objectReferenceValue).GetComponent<BoxCollider2D>();
            BoxCollider2D[] mapWalls = { left, right, top, bottom };
            for (int i = 0; i < mapWalls.Length; i++)
                throwWalls.GetArrayElementAtIndex(truckWalls.arraySize + i).objectReferenceValue = mapWalls[i];
            throwData.ApplyModifiedPropertiesWithoutUndo();
            PlayerAttack attack = backpack.gameObject.AddComponent<PlayerAttack>();
            SetReference(attack, "_truck", truck);
            GameObject swingFlash = new GameObject("Swing Flash");
            swingFlash.transform.SetParent(backpack.transform, false);
            MeshFilter flashFilter = swingFlash.AddComponent<MeshFilter>();
            MeshRenderer flashRenderer = swingFlash.AddComponent<MeshRenderer>();
            flashRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            flashRenderer.sortingOrder = 5;
            flashRenderer.enabled = false;
            SwingFlash flash = swingFlash.AddComponent<SwingFlash>();
            SetReference(flash, "_attack", attack);
            SetReference(flash, "_meshFilter", flashFilter);
            SetReference(flash, "_meshRenderer", flashRenderer);
            CreateMonsters(sprite, truck, backpack, arena);
            FollowCamera follow = camera.gameObject.AddComponent<FollowCamera>();
            SetReference(follow, "_tuning", LoadCameraTuning());
            SetReference(follow, "_target", backpack.transform);
            SetReference(follow, "_map", arena);
            SetReference(follow, "_truck", truck);
            RectTransform row = CreateBackpackContent(sprite, backpack);
            CreatePlayerHealthBar(row.parent, backpack.GetComponent<PlayerHealth>());
            CreateTruckHealthBar(row.parent, truck.GetComponent<TruckHealth>());
            CreateFrying(sprite, backpack, truck, row);
            CreateGameOverScreen(row.parent, truck.GetComponent<TruckHealth>(),
                backpack.GetComponent<PlayerInputReader>(), controls);
            SetReference(arena, "_follow", follow);
            SetReference(arena, "_backpackRow", row);
            Canvas.ForceUpdateCanvases();
            arena.RefreshLayout();
            ValidateReferences(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save " + ScenePath);
            RegisterPrototypeScene();
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateReferences(scene);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Prototype scene rebuilt: " + ScenePath);
        }

        private static void RegisterPrototypeScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path != ScenePath) continue;
                if (!scenes[i].enabled)
                {
                    scenes[i].enabled = true;
                    EditorBuildSettings.scenes = scenes;
                }
                return;
            }

            System.Array.Resize(ref scenes, scenes.Length + 1);
            scenes[scenes.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = scenes;
        }

        private static TruckArea CreateTruck(Sprite sprite)
        {
            TruckArea truck = new GameObject("Truck").AddComponent<TruckArea>();
            TruckHealth health = truck.gameObject.AddComponent<TruckHealth>();
            SetReference(health, "_tuning", LoadTruckTuning());
            SpriteRenderer floor = CreateSprite("Truck Floor", sprite, new Color(0.7f, 0.67f, 0.52f), 0);
            floor.transform.SetParent(truck.transform, false);
            SetReference(truck, "_floor", floor.transform);
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
        private static void CreateMonsters(Sprite sprite, TruckArea truck, PlayerBackpack player, MapBounds arena)
        {
            EnsureFolder(Root + "/Data/Monsters");
            string path = Root + "/Data/Monsters/CursedDumpling.asset";
            MonsterDefinition definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            DropTable drops = LoadMonsterDrops();
            BackpackTuning backpackTuning = LoadBackpackTuning();
            MonsterSpawner spawner = new GameObject("Monster Spawner").AddComponent<MonsterSpawner>();
            SetReference(spawner, "_definition", definition);
            SerializedObject truckData = new SerializedObject(truck);
            SerializedProperty doors = truckData.FindProperty("_doorBlockers");
            doors.arraySize = 2;
            SerializedObject spawnData = new SerializedObject(spawner);
            SerializedProperty blockers = spawnData.FindProperty("_doorBlockers");
            blockers.arraySize = 2;
            for (int i = 0; i < 2; i++)
            {
                BoxCollider2D door = CreateWall("Monster Door Blocker " + i, truck.transform);
                doors.GetArrayElementAtIndex(i).objectReferenceValue = door.transform;
                blockers.GetArrayElementAtIndex(i).objectReferenceValue = door;
            }
            truckData.ApplyModifiedPropertiesWithoutUndo();
            truck.RefreshLayout();
            Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();
            SerializedProperty players = spawnData.FindProperty("_playerColliders");
            players.arraySize = playerColliders.Length;
            for (int i = 0; i < playerColliders.Length; i++)
                players.GetArrayElementAtIndex(i).objectReferenceValue = playerColliders[i];
            TruckHealth truckHealth = truck.GetComponent<TruckHealth>();
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Vector2[] positions = { new Vector2(0f, 7f), new Vector2(-13f, 5f),
                new Vector2(13f, 2f), new Vector2(-10f, -5f), new Vector2(9f, -5.5f) };
            SerializedProperty points = spawnData.FindProperty("_spawnPoints");
            SerializedProperty monsters = spawnData.FindProperty("_monsters");
            points.arraySize = monsters.arraySize = positions.Length;
            for (int i = 0; i < positions.Length; i++)
            {
                Transform point = new GameObject("Monster Spawn " + i).transform;
                point.SetParent(spawner.transform, false);
                point.position = truck.transform.position + (Vector3)positions[i];
                SpriteRenderer visual = CreateSprite("Cursed Dumpling " + i, sprite, definition.Color, 2);
                visual.transform.position = point.position;
                visual.transform.localScale = Vector3.one * definition.Size;
                Rigidbody2D body = visual.gameObject.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.useFullKinematicContacts = true;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                visual.gameObject.AddComponent<BoxCollider2D>();
                MonsterTargeting targeting = visual.gameObject.AddComponent<MonsterTargeting>();
                SetReference(targeting, "_definition", definition);
                SetReference(targeting, "_player", player.transform);
                SetReference(targeting, "_playerHealth", playerHealth);
                SetReference(targeting, "_truck", truck);
                Monster monster = visual.gameObject.AddComponent<Monster>();
                SetReference(monster, "_definition", definition);
                MonsterAttack attack = visual.gameObject.AddComponent<MonsterAttack>();
                SetReference(attack, "_definition", definition);
                SetReference(attack, "_player", playerHealth);
                SetReference(attack, "_playerCollider", playerCollider);
                SetReference(attack, "_truck", truck);
                SetReference(attack, "_truckHealth", truckHealth);
                MonsterTruckRoutine truckRoutine = visual.gameObject.AddComponent<MonsterTruckRoutine>();
                SetReference(truckRoutine, "_definition", definition);
                SetReference(truckRoutine, "_truck", truck);
                SetReference(truckRoutine, "_map", arena);
                SetReference(targeting, "_truckRoutine", truckRoutine);
                SetReference(attack, "_truckRoutine", truckRoutine);
                MonsterDrops dropsComponent = visual.gameObject.AddComponent<MonsterDrops>();
                SetReference(dropsComponent, "_dropTable", drops);
                SetReference(dropsComponent, "_tuning", backpackTuning);
                SetReference(dropsComponent, "_sprite", sprite);
                SerializedObject dropsData = new SerializedObject(dropsComponent);
                dropsData.FindProperty("_size").floatValue = 0.4f;
                dropsData.ApplyModifiedPropertiesWithoutUndo();
                points.GetArrayElementAtIndex(i).objectReferenceValue = point;
                monsters.GetArrayElementAtIndex(i).objectReferenceValue = monster;
            }
            spawnData.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static DropTable LoadMonsterDrops()
        {
            EnsureFolder(Root + "/Data/Items");
            ItemDefinition meat = LoadItem("MonsterMeat", "怪肉", new Color(0.85f, 0.2f, 0.2f));
            ItemDefinition arm = LoadMonsterPart("MonsterArm", "怪物手臂", new Color(0.75f, 0.35f, 0.55f));
            ItemDefinition leg = LoadMonsterPart("MonsterLeg", "怪物腿", new Color(0.8f, 0.5f, 0.25f));
            string path = Root + "/Data/Monsters/CursedDumplingDrops.asset";
            DropTable table = AssetDatabase.LoadAssetAtPath<DropTable>(path);
            if (table != null) return table;
            table = ScriptableObject.CreateInstance<DropTable>();
            SerializedObject data = new SerializedObject(table);
            SerializedProperty entries = data.FindProperty("_entries");
            ItemDefinition[] items = { meat, arm, leg };
            float[] chances = { 0.6f, 0.3f, 0.3f };
            entries.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_item").objectReferenceValue = items[i];
                entry.FindPropertyRelative("_chance").floatValue = chances[i];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(table, path);
            return table;
        }

        private static ItemDefinition LoadMonsterPart(string id, string displayName, Color color)
        {
            Color friedColor = color * new Color(0.65f, 0.65f, 0.65f, 1f);
            ItemDefinition fried = LoadItem("Fried" + id, "煎" + displayName, friedColor);
            ConfigureMonsterPart(fried, "Fried" + id, "煎" + displayName, friedColor, null);
            ItemDefinition raw = LoadItem(id, displayName, color);
            ConfigureMonsterPart(raw, id, displayName, color, fried);
            return raw;
        }

        private static void ConfigureMonsterPart(ItemDefinition item, string id, string displayName,
            Color color, ItemDefinition fryingResult)
        {
            SerializedObject data = new SerializedObject(item);
            data.FindProperty("_id").stringValue = id;
            data.FindProperty("_displayName").stringValue = displayName;
            data.FindProperty("_color").colorValue = color;
            data.FindProperty("_fryingResult").objectReferenceValue = fryingResult;
            data.FindProperty("_weapon").objectReferenceValue = null;
            data.FindProperty("_neverExpiresOnGround").boolValue = false;
            data.ApplyModifiedPropertiesWithoutUndo();
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
            GameObject player = new GameObject("Player");
            SpriteRenderer playerVisual = CreateSprite("Player Visual", sprite,
                new Color(1f, 0.69f, 0.24f), 2);
            playerVisual.transform.SetParent(player.transform, false);
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
            playerCollider.size = Vector2.one;
            PlayerHealth health = player.AddComponent<PlayerHealth>();
            PlayerHealthTuning healthTuning = LoadPlayerHealthTuning();
            SetReference(health, "_tuning", healthTuning);
            PlayerTuning tuning = LoadPlayerTuning();
            PlayerMovement movement = player.AddComponent<PlayerMovement>();
            SetReference(movement, "_tuning", tuning);
            PlayerHitReaction hitReaction = player.AddComponent<PlayerHitReaction>();
            SetReference(hitReaction, "_health", health);
            SetReference(hitReaction, "_tuning", healthTuning);
            SetReference(hitReaction, "_movement", movement);
            SetReference(hitReaction, "_body", body);
            SetReference(hitReaction, "_collider", playerCollider);
            SetReference(hitReaction, "_renderer", playerVisual);
            PlayerShove shove = player.AddComponent<PlayerShove>();
            SetReference(shove, "_hitReaction", hitReaction);
            SetReference(shove, "_tuning", tuning);
            SetReference(shove, "_movement", movement);
            SetReference(shove, "_body", body);
            PlayerBackpack backpack = player.AddComponent<PlayerBackpack>();
            SetReference(backpack, "_tuning", LoadBackpackTuning());
            SpriteRenderer icon = CreateSprite("Held Item", sprite, Color.white, 2);
            icon.transform.SetParent(player.transform, false);
            icon.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            icon.transform.localScale = Vector3.one * 0.25f;
            icon.enabled = false;
            SetReference(backpack, "_itemSprite", sprite);
            SetReference(backpack, "_heldIcon", icon);
            SetReference(backpack, "_failureSprite", LoadMark(false));
            PlayerInteractor interactor = player.AddComponent<PlayerInteractor>();
            PlayerThrow playerThrow = player.AddComponent<PlayerThrow>();
            SetReference(playerThrow, "_tuning", LoadThrowTuning());
            SetReference(playerThrow, "_backpack", backpack);
            SetReference(playerThrow, "_interactor", interactor);
            GameObject arrow = new GameObject("Throw Arrow");
            arrow.transform.SetParent(player.transform, false);
            MeshFilter arrowFilter = arrow.AddComponent<MeshFilter>();
            MeshRenderer arrowRenderer = arrow.AddComponent<MeshRenderer>();
            arrowRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            arrowRenderer.sortingOrder = 5;
            arrowRenderer.enabled = false;
            ThrowArrow throwArrow = arrow.AddComponent<ThrowArrow>();
            SetReference(throwArrow, "_throw", playerThrow);
            SetReference(throwArrow, "_meshFilter", arrowFilter);
            SetReference(throwArrow, "_meshRenderer", arrowRenderer);
            PlayerInputReader reader = player.AddComponent<PlayerInputReader>();
            SerializedObject readerData = new SerializedObject(reader);
            readerData.FindProperty("_controls").objectReferenceValue = controls;
            readerData.ApplyModifiedPropertiesWithoutUndo();
            PlayerDowned downed = player.AddComponent<PlayerDowned>();
            SetReference(downed, "_tuning", healthTuning);

            TextMesh countdown = new GameObject("Revive Countdown").AddComponent<TextMesh>();
            countdown.transform.SetParent(player.transform, false);
            countdown.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            MeshRenderer countdownRenderer = countdown.GetComponent<MeshRenderer>();
            countdownRenderer.sharedMaterial = countdown.font.material;
            countdownRenderer.sortingOrder = 6;
            countdown.anchor = TextAnchor.MiddleCenter;
            countdown.alignment = TextAlignment.Center;
            countdown.color = Color.white;
            countdown.characterSize = 0.1f;
            countdown.gameObject.SetActive(false);

            PlayerDownedView downedView = player.AddComponent<PlayerDownedView>();
            SetReference(downedView, "_health", health);
            SetReference(downedView, "_downed", downed);
            SetReference(downedView, "_visual", playerVisual);
            SetReference(downedView, "_countdown", countdown);
            SetReference(downedView, "_heldIcon", icon.gameObject);
            return backpack;
        }

        private static void SetReference(Object target, string property, Object value)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(property).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PlayerTuning LoadPlayerTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/PlayerTuning.asset";
            PlayerTuning tuning = AssetDatabase.LoadAssetAtPath<PlayerTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<PlayerTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static PlayerHealthTuning LoadPlayerHealthTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/PlayerHealthTuning.asset";
            PlayerHealthTuning tuning = AssetDatabase.LoadAssetAtPath<PlayerHealthTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<PlayerHealthTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static TruckTuning LoadTruckTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/TruckTuning.asset";
            TruckTuning tuning = AssetDatabase.LoadAssetAtPath<TruckTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<TruckTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static ThrowTuning LoadThrowTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/ThrowTuning.asset";
            ThrowTuning tuning = AssetDatabase.LoadAssetAtPath<ThrowTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<ThrowTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static BackpackTuning LoadBackpackTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/BackpackTuning.asset";
            BackpackTuning tuning = AssetDatabase.LoadAssetAtPath<BackpackTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<BackpackTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static CookingTuning LoadCookingTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/CookingTuning.asset";
            CookingTuning tuning = AssetDatabase.LoadAssetAtPath<CookingTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<CookingTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static CameraTuning LoadCameraTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/CameraTuning.asset";
            CameraTuning tuning = AssetDatabase.LoadAssetAtPath<CameraTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<CameraTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static MapTuning LoadMapTuning()
        {
            EnsureFolder(Root + "/Data/Tuning");
            string path = Root + "/Data/Tuning/MapTuning.asset";
            MapTuning tuning = AssetDatabase.LoadAssetAtPath<MapTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<MapTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        private static WeaponDefinition LoadFryingPanWeapon()
        {
            EnsureFolder(Root + "/Data/Weapons");
            string path = Root + "/Data/Weapons/FryingPan.asset";
            WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (weapon != null) return weapon;
            weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            AssetDatabase.CreateAsset(weapon, path);
            return weapon;
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
            ItemDefinition fryingPan = LoadItem("FryingPan", "平底锅", new Color(0.55f, 0.55f, 0.55f));
            SerializedObject panData = new SerializedObject(fryingPan);
            panData.FindProperty("_id").stringValue = "FryingPan";
            panData.FindProperty("_displayName").stringValue = "平底锅";
            panData.FindProperty("_color").colorValue = new Color(0.55f, 0.55f, 0.55f);
            panData.FindProperty("_fryingResult").objectReferenceValue = null;
            panData.FindProperty("_weapon").objectReferenceValue = LoadFryingPanWeapon();
            panData.FindProperty("_neverExpiresOnGround").boolValue = true;
            panData.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject backpackData = new SerializedObject(backpack);
            SerializedProperty startingItems = backpackData.FindProperty("_startingItems");
            startingItems.arraySize = 1;
            startingItems.GetArrayElementAtIndex(0).objectReferenceValue = fryingPan;
            backpackData.ApplyModifiedPropertiesWithoutUndo();
            ItemDefinition mushroom = LoadItem("Mushroom", "蘑菇", new Color(0.85f, 0.75f, 0.55f));
            Vector2[] positions =
            {
                new Vector2(2f, 6f), new Vector2(-11f, -3f),
                new Vector2(12f, 6f), new Vector2(-13f, 7f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                SpriteRenderer renderer = CreateSprite(mushroom.name + " " + i, sprite, mushroom.Color, 1);
                renderer.transform.position = (Vector3)positions[i];
                renderer.transform.localScale = Vector3.one * 0.4f;
                WorldItem world = renderer.gameObject.AddComponent<WorldItem>();
                SetReference(world, "_item", mushroom);
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

        private static void CreatePlayerHealthBar(Transform canvas, PlayerHealth health)
        {
            RectTransform row = new GameObject("Player Health Row", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(canvas, false);
            row.anchorMin = row.anchorMax = row.pivot = new Vector2(0f, 1f);
            row.sizeDelta = Vector2.zero;
            PlayerHealthBar bar = row.gameObject.AddComponent<PlayerHealthBar>();
            SetReference(bar, "_health", health);
        }

        private static void CreateTruckHealthBar(Transform canvas, TruckHealth health)
        {
            RectTransform rect = UiRect("Truck Health Bar", canvas, Vector2.zero, Vector2.zero);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
            rect.gameObject.AddComponent<Image>().raycastTarget = false;

            Image fill = UiRect("Fill", rect, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.raycastTarget = false;

            Text label = UiRect("Health Value", rect, Vector2.zero, Vector2.zero).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;

            TruckHealthBar bar = rect.gameObject.AddComponent<TruckHealthBar>();
            SetReference(bar, "_health", health);
            SetReference(bar, "_fill", fill);
            SetReference(bar, "_label", label);
        }

        private static void CreateGameOverScreen(Transform canvas, TruckHealth health,
            PlayerInputReader playerInput, InputActionAsset controls)
        {
            RectTransform overlay = UiRect("Game Over Overlay", canvas, Vector2.zero, Vector2.zero);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

            Text title = UiText("Defeat", overlay, new Vector2(480f, 100f), new Vector2(0f, 36f), 72);
            title.text = "失败";
            Text hint = UiText("Restart Hint", overlay, new Vector2(480f, 54f), new Vector2(0f, -50f), 32);
            hint.text = "按 R 重新开始";
            overlay.gameObject.SetActive(false);

            GameOverScreen screen = canvas.gameObject.AddComponent<GameOverScreen>();
            SetReference(screen, "_health", health);
            SetReference(screen, "_playerInput", playerInput);
            SetReference(screen, "_controls", controls);
            SetReference(screen, "_overlay", overlay.gameObject);
        }

        private static void CreateFrying(Sprite sprite, PlayerBackpack backpack, TruckArea truck, RectTransform backpackRow)
        {
            Transform canvas = backpackRow.parent;
            ItemDefinition meat = AssetDatabase.LoadAssetAtPath<ItemDefinition>(Root + "/Data/Items/MonsterMeat.asset");
            ItemDefinition mushroom = AssetDatabase.LoadAssetAtPath<ItemDefinition>(Root + "/Data/Items/Mushroom.asset");
            ItemDefinition friedMeat = LoadItem("FriedMonsterMeat", "煎怪肉", meat.Color * new Color(0.65f, 0.65f, 0.65f, 1f));
            ItemDefinition friedMushroom = LoadItem("FriedMushroom", "煎蘑菇", mushroom.Color * new Color(0.65f, 0.65f, 0.65f, 1f));
            ItemDefinition burnt = LoadItem("Burnt", "焦土", new Color(0.15f, 0.08f, 0.04f));
            // Only set defaults on raw definitions; keep subsequent Inspector tuning.
            if (meat.FryingResult == null) SetReference(meat, "_fryingResult", friedMeat);
            if (mushroom.FryingResult == null) SetReference(mushroom, "_fryingResult", friedMushroom);
            SpriteRenderer stationBody = CreateSprite("Frying Station", sprite, new Color(0.25f, 0.27f, 0.3f), 3);
            stationBody.transform.position = (Vector3)truck.TopLeftCell(Vector2.one);
            stationBody.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
            FryingStation station = stationBody.gameObject.AddComponent<FryingStation>();
            SetReference(station, "_tuning", LoadCookingTuning());
            SpriteRenderer food = CreateSprite("Pan Contents", sprite, Color.white, 4);
            food.transform.SetParent(station.transform, false); food.transform.localScale = Vector3.one * 0.4f;
            food.enabled = false; SetReference(station, "_food", food); SetReference(station, "_burntItem", burnt);

            FryingPanel panel = canvas.gameObject.AddComponent<FryingPanel>();
            SetReference(panel, "_tuning", LoadCookingTuning());
            RectTransform view = UiRect("Frying View", canvas, Vector2.zero, Vector2.zero);
            view.anchorMin = new Vector2(2f / 3f, 0f); view.anchorMax = Vector2.one;
            view.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);
            RectTransform panRect = UiRect("Pan", view, new Vector2(280f, 280f), Vector2.zero);
            Image pan = panRect.gameObject.AddComponent<Image>(); pan.sprite = LoadMark(true); pan.color = new Color(0.3f, 0.32f, 0.35f);
            Image foodImage = UiRect("Food", panRect, new Vector2(90f, 90f), Vector2.zero).gameObject.AddComponent<Image>();
            Text letters = UiText("Letters", view, new Vector2(400f, 60f), new Vector2(0f, -190f), 34);
            Text status = UiText("Side and Timer", view, new Vector2(400f, 120f), new Vector2(0f, 220f), 24);
            SetReference(panel, "_backpackRow", backpackRow);
            SetReference(panel, "_view", view); SetReference(panel, "_pan", pan);
            SetReference(panel, "_food", foodImage); SetReference(panel, "_letters", letters);
            SetReference(panel, "_status", status); SetReference(panel, "_panSprite", pan.sprite);
            PlayerInteractor interactor = backpack.GetComponent<PlayerInteractor>();
            SetReference(interactor, "_station", station); SetReference(interactor, "_panel", panel); SetReference(interactor, "_truck", truck);
            var preview = new GameObject("Placement Preview").AddComponent<UnityEngine.Rendering.SortingGroup>();
            preview.sortingOrder = 1;
            SpriteRenderer shadow = CreateSprite("Placement Shadow", stationBody.sprite,
                new Color(stationBody.color.r, stationBody.color.g, stationBody.color.b, 0.5f), 0);
            SpriteRenderer failure = CreateSprite("Placement Failure", LoadMark(false), Color.red, 1);
            shadow.transform.SetParent(preview.transform, false);
            failure.transform.SetParent(preview.transform, false);
            shadow.enabled = false; failure.enabled = false;
            SetReference(interactor, "_stationRenderer", stationBody);
            SetReference(interactor, "_placementShadow", shadow);
            SetReference(interactor, "_placementFailure", failure);
            view.gameObject.SetActive(false); AssetDatabase.SaveAssets();
        }
        private static RectTransform UiRect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }
        private static Text UiText(string name, Transform parent, Vector2 size, Vector2 position, int fontSize)
        {
            Text text = UiRect(name, parent, size, position).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static Sprite LoadMark(bool pan)
        {
            string path = Root + "/Art/Placeholder/" + (pan ? "Pan" : "Failure") + ".png";
            if (!File.Exists(path))
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                try
                {
                    var pixels = new Color32[size * size];
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - (size - 1) * 0.5f, dy = y - (size - 1) * 0.5f;
                        bool visible = pan ? dx * dx + dy * dy <= 30f * 30f :
                            (Mathf.Abs(dx - dy) <= 5f || Mathf.Abs(dx + dy) <= 5f) && Mathf.Abs(dx) < 27f && Mathf.Abs(dy) < 27f;
                        pixels[y * size + x] = visible ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                    }
                    texture.SetPixels32(pixels); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
                }
                finally { Object.DestroyImmediate(texture); }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
