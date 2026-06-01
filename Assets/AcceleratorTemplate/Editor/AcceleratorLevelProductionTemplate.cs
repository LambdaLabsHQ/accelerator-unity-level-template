using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AcceleratorTemplate.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AcceleratorTemplate.Editor
{
    public static class AcceleratorLevelProductionTemplate
    {
        private const string ScenePath = "Assets/Scenes/GeneratedWhitebox.unity";
        private const string MaterialsPath = "Assets/AcceleratorTemplate/Generated/Materials";
        private const string WhiteboxRootName = "00_Whitebox_From_LevelDesign";
        private const string GeneratedRootName = "03_Placed_Generated_Art";

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        [MenuItem("Accelerator/Level Production/Export From-Zero Graph")]
        public static string ExportFromZeroProductionGraph()
        {
            var builder = new StringBuilder();
            builder.Append("{");
            AppendProperty(builder, "template", "accelerator-unity-level-template", true);
            AppendProperty(builder, "unityScenePath", ScenePath, true);
            AppendProperty(builder, "gameplayDesignDoc", DefaultGameplayDesignDoc(), true);
            AppendProperty(builder, "targetExperiencePlayerFlow", DefaultPlayerFlow(), true);
            builder.Append("\"stages\":[");
            AppendQuoted(builder, "level-plan");
            builder.Append(",");
            AppendQuoted(builder, "level-layout");
            builder.Append(",");
            AppendQuoted(builder, "whitebox-generation");
            builder.Append(",");
            AppendQuoted(builder, "concept-art");
            builder.Append(",");
            AppendQuoted(builder, "image-to-3d");
            builder.Append(",");
            AppendQuoted(builder, "normalize");
            builder.Append(",");
            AppendQuoted(builder, "unity-placement");
            builder.Append(",");
            AppendQuoted(builder, "audit");
            builder.Append("],\"jobs\":[");
            AppendLevelDesignJobs(builder);
            builder.Append(",");
            AppendWhiteboxJob(builder);
            foreach (var spec in SeedTargets())
            {
                builder.Append(",");
                AppendConceptJob(builder, spec);
                builder.Append(",");
                AppendMeshJob(builder, spec);
                builder.Append(",");
                AppendNormalizeJob(builder, spec);
                builder.Append(",");
                AppendPlacementJob(builder, spec);
            }

            builder.Append("]}");
            return builder.ToString();
        }

        [MenuItem("Accelerator/Level Production/Generate Seed Whitebox")]
        public static string GenerateSeedWhitebox()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory(MaterialsPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "GeneratedWhitebox";
            var materials = CreateMaterials();
            var root = new GameObject("Accelerator_FromZero_Level");
            var whiteboxRoot = CreateChild(root.transform, WhiteboxRootName);
            CreateChild(root.transform, "01_Source_Whitebox_Screenshot");
            CreateChild(root.transform, "02_Concept_And_Mesh_Artifact_Slots");
            CreateChild(root.transform, GeneratedRootName);
            CreateChild(root.transform, "04_Audit");

            CreateGround(materials.Ground);
            foreach (var spec in SeedTargets())
            {
                CreateWhiteboxTarget(whiteboxRoot, spec, materials.ForCategory(spec.Category));
            }

            CreateLighting();
            CreateOverviewCamera();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AuditWhitebox();
        }

        [MenuItem("Accelerator/Level Production/Apply Generated Placeholders")]
        public static string ApplyGeneratedAssetPlaceholders()
        {
            EnsureWhiteboxSceneLoaded();
            var generatedRoot = GameObject.Find(GeneratedRootName);
            if (!generatedRoot)
            {
                generatedRoot = new GameObject(GeneratedRootName);
            }

            for (var index = generatedRoot.transform.childCount - 1; index >= 0; index -= 1)
            {
                UnityEngine.Object.DestroyImmediate(generatedRoot.transform.GetChild(index).gameObject);
            }

            var materials = CreateMaterials();
            foreach (var target in FindTargets())
            {
                CreateGeneratedPlaceholder(generatedRoot.transform, target, materials.Generated);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            return AuditPlacements();
        }

        public static string AuditWhitebox()
        {
            EnsureWhiteboxSceneLoaded();
            var targets = FindTargets();
            return "{"
                + $"\"sceneExists\":{Bool(File.Exists(ScenePath))},"
                + $"\"activeScene\":\"{Escape(SceneManager.GetActiveScene().path)}\","
                + $"\"whiteboxTargets\":{targets.Length},"
                + $"\"levelDesignJobs\":3,"
                + $"\"conceptJobs\":{targets.Length},"
                + $"\"meshJobs\":{targets.Length},"
                + $"\"hasOverviewCamera\":{Bool(Camera.main != null)}"
                + "}";
        }

        [MenuItem("Accelerator/Level Production/Audit Placements")]
        public static string AuditPlacements()
        {
            EnsureWhiteboxSceneLoaded();
            var targets = FindTargets();
            var placements = UnityEngine.Object.FindObjectsOfType<GeneratedAssetPlacement>();
            var matched = targets.Count(target => placements.Any(placement => placement.TargetId == target.TargetId));
            var missing = targets
                .Where(target => !placements.Any(placement => placement.TargetId == target.TargetId))
                .Select(target => target.TargetId)
                .ToArray();

            var builder = new StringBuilder();
            builder.Append("{");
            AppendProperty(builder, "sceneExists", File.Exists(ScenePath), true);
            AppendProperty(builder, "activeScene", SceneManager.GetActiveScene().path, true);
            AppendProperty(builder, "whiteboxTargets", targets.Length, true);
            AppendProperty(builder, "generatedPlacements", placements.Length, true);
            AppendProperty(builder, "matchedPlacements", matched, true);
            AppendProperty(builder, "coverage", targets.Length == 0 ? 0f : matched / (float)targets.Length, true);
            builder.Append("\"missingTargets\":[");
            for (var index = 0; index < missing.Length; index += 1)
            {
                if (index > 0)
                {
                    builder.Append(",");
                }

                AppendQuoted(builder, missing[index]);
            }

            builder.Append("]}");
            return builder.ToString();
        }

        public static string EnterPlayMode()
        {
            EnsureWhiteboxSceneLoaded();
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.EnterPlaymode();
            }

            return "{"
                + $"\"isPlaying\":{Bool(EditorApplication.isPlaying)},"
                + $"\"willChangePlaymode\":{Bool(EditorApplication.isPlayingOrWillChangePlaymode)},"
                + $"\"activeScene\":\"{Escape(SceneManager.GetActiveScene().path)}\""
                + "}";
        }

        public static string ExitPlayMode()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
            }

            return "{"
                + $"\"isPlaying\":{Bool(EditorApplication.isPlaying)},"
                + $"\"willChangePlaymode\":{Bool(EditorApplication.isPlayingOrWillChangePlaymode)}"
                + "}";
        }

        private static void EnsureWhiteboxSceneLoaded()
        {
            if (!File.Exists(ScenePath))
            {
                GenerateSeedWhitebox();
                return;
            }

            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
        }

        private static LevelDesignTarget[] FindTargets()
        {
            return UnityEngine.Object.FindObjectsOfType<LevelDesignTarget>()
                .OrderBy(target => target.TargetId, StringComparer.Ordinal)
                .ToArray();
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void CreateGround(Material material)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "WB_ground_walkable_village_square";
            ground.transform.position = new Vector3(0f, -0.15f, 0f);
            ground.transform.localScale = new Vector3(34f, 0.3f, 26f);
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateWhiteboxTarget(Transform root, TargetSpec spec, Material material)
        {
            var targetObject = GameObject.CreatePrimitive(spec.Primitive);
            targetObject.name = $"WB_{spec.TargetId}";
            targetObject.transform.SetParent(root, false);
            targetObject.transform.SetPositionAndRotation(spec.Position, Quaternion.Euler(0f, spec.Yaw, 0f));
            targetObject.transform.localScale = spec.Scale;
            targetObject.GetComponent<Renderer>().sharedMaterial = material;

            var target = targetObject.AddComponent<LevelDesignTarget>();
            target.Configure(
                spec.TargetId,
                spec.Category,
                spec.SourcePart,
                spec.GameplayRole,
                spec.PreferredFacing,
                spec.ConceptPrompt);
        }

        private static void CreateGeneratedPlaceholder(Transform root, LevelDesignTarget target, Material material)
        {
            var sourceBounds = target.WorldBounds();
            var placed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placed.name = $"GEN_{target.TargetId}";
            placed.transform.SetParent(root, false);
            placed.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
            placed.transform.localScale = new Vector3(
                Mathf.Max(0.2f, sourceBounds.size.x * 0.92f),
                Mathf.Max(0.2f, sourceBounds.size.y * 0.92f),
                Mathf.Max(0.2f, sourceBounds.size.z * 0.92f));
            placed.GetComponent<Renderer>().sharedMaterial = material;

            var placement = placed.AddComponent<GeneratedAssetPlacement>();
            placement.Configure(
                target.TargetId,
                target.SourcePart,
                $"concept:{target.TargetId}:image",
                $"mesh:{target.TargetId}:normalized_glb",
                "placeholder-fitted-to-whitebox");
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Sun_KeyLight");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }

        private static void CreateOverviewCamera()
        {
            var cameraObject = new GameObject("Overview_Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 48f;
            cameraObject.transform.position = new Vector3(0f, 18f, -22f);
            cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        private static MaterialSet CreateMaterials()
        {
            return new MaterialSet(
                CreateMaterial("MAT_WB_Ground", new Color(0.20f, 0.22f, 0.20f)),
                CreateMaterial("MAT_WB_Path", new Color(0.32f, 0.31f, 0.28f)),
                CreateMaterial("MAT_WB_Architecture", new Color(0.30f, 0.54f, 0.82f)),
                CreateMaterial("MAT_WB_Foliage", new Color(0.24f, 0.62f, 0.32f)),
                CreateMaterial("MAT_WB_Prop", new Color(0.90f, 0.66f, 0.24f)),
                CreateMaterial("MAT_WB_Landmark", new Color(0.72f, 0.42f, 0.86f)),
                CreateMaterial("MAT_WB_Gameplay", new Color(0.90f, 0.34f, 0.32f)),
                CreateMaterial("MAT_WB_Blocker", new Color(0.52f, 0.52f, 0.52f)),
                CreateMaterial("MAT_Generated_Placement", new Color(0.18f, 0.86f, 0.78f)));
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var path = $"{MaterialsPath}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static TargetSpec[] SeedTargets()
        {
            return new[]
            {
                new TargetSpec("main_path", LevelAssetCategory.Path, "stone_road_module", "critical path from spawn to gate", "north-south", "A modular worn stone road kit for a stylized village square, readable direction and walkable edges.", PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), new Vector3(5f, 0.08f, 22f), 0f),
                new TargetSpec("north_house", LevelAssetCategory.Architecture, "village_house_small", "destination landmark on the north edge", "south", "Small stylized village house facing the road, blue slate roof, plaster walls, warm wood beams, low-poly adventure style.", PrimitiveType.Cube, new Vector3(-7.5f, 1.8f, 5.4f), new Vector3(4.2f, 3.6f, 3.4f), 12f),
                new TargetSpec("south_house", LevelAssetCategory.Architecture, "village_house_small", "secondary destination opposite the north house", "north", "Compact stylized village house sharing the same kit, green shutters, readable front door facing the road.", PrimitiveType.Cube, new Vector3(7f, 1.6f, -5f), new Vector3(4f, 3.2f, 3.2f), -18f),
                new TargetSpec("market_stall", LevelAssetCategory.Prop, "market_stall", "interaction point near route center", "west", "Colorful fantasy market stall with cloth canopy, crates, readable counter silhouette, mid-size interactable prop.", PrimitiveType.Cube, new Vector3(6.5f, 0.9f, 1.8f), new Vector3(3.6f, 1.8f, 2.2f), -90f),
                new TargetSpec("gate_landmark", LevelAssetCategory.Landmark, "village_gate", "framing landmark at path end", "south", "Stylized wooden village gate with stone bases, broad arch silhouette, enough negative space for the road.", PrimitiveType.Cube, new Vector3(0f, 2.4f, 9.2f), new Vector3(6.2f, 4.8f, 1.2f), 0f),
                new TargetSpec("oak_tree_a", LevelAssetCategory.Foliage, "broadleaf_tree", "soft cover and skyline breakup", "any", "Broadleaf stylized tree with chunky trunk, round canopy masses, low-poly readable silhouette.", PrimitiveType.Sphere, new Vector3(-11f, 2.2f, 0.5f), new Vector3(3.2f, 4.4f, 3.2f), 0f),
                new TargetSpec("oak_tree_b", LevelAssetCategory.Foliage, "broadleaf_tree", "clone placement for vegetation validation", "any", "Broadleaf stylized tree clone sharing the same generated source as oak_tree_a, minor scale variation only.", PrimitiveType.Sphere, new Vector3(11f, 2f, 5.8f), new Vector3(3f, 4f, 3f), 0f),
                new TargetSpec("street_lamp_a", LevelAssetCategory.Prop, "street_lamp", "navigation affordance and light anchor", "road", "Slim stylized street lamp with readable base, post, lantern head, vertical orientation and warm village material language.", PrimitiveType.Cylinder, new Vector3(-3.4f, 1.5f, 1.6f), new Vector3(0.45f, 1.5f, 0.45f), 0f),
                new TargetSpec("street_lamp_b", LevelAssetCategory.Prop, "street_lamp", "clone placement for repeated upright props", "road", "Matching street lamp clone using the same source mesh as street_lamp_a, validated for upright placement.", PrimitiveType.Cylinder, new Vector3(3.4f, 1.5f, -2.6f), new Vector3(0.45f, 1.5f, 0.45f), 0f),
                new TargetSpec("supply_crates", LevelAssetCategory.Prop, "crate_stack", "cover and clutter beside market stall", "road", "Small stack of stylized wooden supply crates with rope bands, varied boxes, simple low-poly silhouette.", PrimitiveType.Cube, new Vector3(9.5f, 0.65f, -1.6f), new Vector3(2.3f, 1.3f, 1.5f), 8f),
                new TargetSpec("player_start_marker", LevelAssetCategory.Gameplay, "spawn_marker", "player spawn and composition anchor", "north", "Small diegetic player start marker such as a carved stone disc or banner base, subtle and non-blocking.", PrimitiveType.Cylinder, new Vector3(0f, 0.18f, -9.2f), new Vector3(1.2f, 0.18f, 1.2f), 0f),
                new TargetSpec("sightline_blocker", LevelAssetCategory.Blocker, "temporary_blocker", "whitebox-only line-of-sight gate", "none", "Do not generate final art unless design keeps this blocker; gray whitebox volume controls first vista reveal.", PrimitiveType.Cube, new Vector3(-4.8f, 1.1f, -3.8f), new Vector3(2.4f, 2.2f, 1.2f), 28f)
            };
        }

        private static void AppendLevelDesignJobs(StringBuilder builder)
        {
            builder.Append("{");
            AppendProperty(builder, "id", "level-plan:seed", true);
            AppendProperty(builder, "capabilityId", "gp-level-plan", true);
            builder.Append("\"inputs\":{");
            AppendProperty(builder, "gameplay_design_doc", DefaultGameplayDesignDoc(), true);
            AppendProperty(builder, "target_experience_player_flow", DefaultPlayerFlow(), false);
            builder.Append("},\"outputs\":{\"result\":\"level-plan:seed:json\"}}");
            builder.Append(",{");
            AppendProperty(builder, "id", "level-layout:seed", true);
            AppendProperty(builder, "capabilityId", "gp-level-layout", true);
            builder.Append("\"inputs\":{");
            AppendProperty(builder, "level_planning_document", "level-plan:seed:json", true);
            AppendProperty(builder, "level_mechanic_design_spec", DefaultMechanicSpec(), false);
            builder.Append("},\"outputs\":{\"result\":\"level-layout:seed:json\"}}");
        }

        private static void AppendWhiteboxJob(StringBuilder builder)
        {
            builder.Append("{");
            AppendProperty(builder, "id", "whitebox:seed", true);
            AppendProperty(builder, "capabilityId", "gp-whitebox", true);
            AppendProperty(builder, "provider", "native-cli", true);
            builder.Append("\"inputs\":{");
            AppendProperty(builder, "level_brief", "level-plan:seed:json", true);
            AppendProperty(builder, "level_floor_layout", "level-layout:seed:json", true);
            AppendProperty(builder, "level_mechanic_spec", DefaultMechanicSpec(), true);
            AppendProperty(builder, "whitebox_constraints", "Use Unity primitives only; every art-producing object must receive a LevelDesignTarget component and stable targetId.", false);
            builder.Append("},\"outputs\":{");
            AppendProperty(builder, "source_image", "whitebox:seed:source_image", true);
            AppendProperty(builder, "whitebox_manifest", "whitebox:seed:manifest", true);
            AppendProperty(builder, "unity_repl_transcript", "whitebox:seed:transcript", false);
            builder.Append("}}");
        }

        private static void AppendConceptJob(StringBuilder builder, TargetSpec spec)
        {
            builder.Append("{");
            AppendProperty(builder, "id", $"concept:{spec.TargetId}", true);
            AppendProperty(builder, "capabilityId", "2d-prop-ref", true);
            builder.Append("\"inputs\":{");
            AppendProperty(builder, "prop_design_document", spec.ConceptPrompt, true);
            AppendProperty(builder, "prop_style_design_spec", StyleBrief(), true);
            AppendProperty(builder, "style_checklist", "single asset on neutral background; preserve whitebox footprint; front and three-quarter view; no text labels", false);
            builder.Append("},\"outputs\":{\"asset\":\"concept:");
            builder.Append(Escape(spec.TargetId));
            builder.Append(":image\"}}");
        }

        private static void AppendMeshJob(StringBuilder builder, TargetSpec spec)
        {
            builder.Append("{");
            AppendProperty(builder, "id", $"mesh:{spec.TargetId}", true);
            AppendProperty(builder, "capabilityId", "art-image-to-3d", true);
            AppendProperty(builder, "provider", "tripo", true);
            builder.Append("\"inputs\":{\"part_or_pack_image\":\"concept:");
            builder.Append(Escape(spec.TargetId));
            builder.Append(":image\"},\"outputs\":{\"raw_glb\":\"mesh:");
            builder.Append(Escape(spec.TargetId));
            builder.Append(":raw_glb\"}}");
        }

        private static void AppendNormalizeJob(StringBuilder builder, TargetSpec spec)
        {
            builder.Append("{");
            AppendProperty(builder, "id", $"normalize:{spec.TargetId}", true);
            AppendProperty(builder, "capabilityId", "art-normalize", true);
            builder.Append("\"inputs\":{\"raw_glb\":\"mesh:");
            builder.Append(Escape(spec.TargetId));
            builder.Append(":raw_glb\"},\"outputs\":{\"normalized_glb\":\"mesh:");
            builder.Append(Escape(spec.TargetId));
            builder.Append(":normalized_glb\"}}");
        }

        private static void AppendPlacementJob(StringBuilder builder, TargetSpec spec)
        {
            builder.Append("{");
            AppendProperty(builder, "id", $"place:{spec.TargetId}", true);
            AppendProperty(builder, "capabilityId", "unity-repl", true);
            AppendProperty(builder, "targetId", spec.TargetId, true);
            AppendProperty(builder, "sourcePart", spec.SourcePart, true);
            AppendProperty(builder, "category", spec.Category.ToString(), true);
            AppendProperty(builder, "preferredFacing", spec.PreferredFacing, true);
            builder.Append("\"whiteboxTransform\":{");
            builder.Append("\"position\":");
            AppendVector(builder, spec.Position);
            builder.Append(",\"scale\":");
            AppendVector(builder, spec.Scale);
            builder.Append(",\"yaw\":");
            builder.Append(spec.Yaw.ToString("0.###", Invariant));
            builder.Append("},\"inputs\":{\"normalized_glb\":\"mesh:");
            builder.Append(Escape(spec.TargetId));
            builder.Append(":normalized_glb\"},\"outputs\":{\"unity_object\":\"GEN_");
            builder.Append(Escape(spec.TargetId));
            builder.Append("\"}}");
        }

        private static string DefaultGameplayDesignDoc()
        {
            return "Small third-person village approach level. The player starts outside a village square, follows a main path, reads a market stall interaction, passes environmental cover, and reaches a gate landmark.";
        }

        private static string DefaultPlayerFlow()
        {
            return "spawn -> reveal village square -> inspect market stall -> move around soft cover -> reach gate landmark; traversal should be readable from one overview camera.";
        }

        private static string DefaultMechanicSpec()
        {
            return "No combat in the seed template. Use collision-safe walkable areas, sightline blocker, optional interaction marker, and repeated vegetation/prop clones for placement regression.";
        }

        private static string StyleBrief()
        {
            return "Stylized readable low-poly village adventure level; warm hand-painted material direction; clean silhouettes; production assets must preserve the whitebox footprint and gameplay clearance.";
        }

        private static void AppendProperty(StringBuilder builder, string key, string value, bool trailingComma)
        {
            AppendQuoted(builder, key);
            builder.Append(":");
            AppendQuoted(builder, value);
            if (trailingComma)
            {
                builder.Append(",");
            }
        }

        private static void AppendProperty(StringBuilder builder, string key, int value, bool trailingComma)
        {
            AppendQuoted(builder, key);
            builder.Append(":");
            builder.Append(value.ToString(Invariant));
            if (trailingComma)
            {
                builder.Append(",");
            }
        }

        private static void AppendProperty(StringBuilder builder, string key, float value, bool trailingComma)
        {
            AppendQuoted(builder, key);
            builder.Append(":");
            builder.Append(value.ToString("0.###", Invariant));
            if (trailingComma)
            {
                builder.Append(",");
            }
        }

        private static void AppendProperty(StringBuilder builder, string key, bool value, bool trailingComma)
        {
            AppendQuoted(builder, key);
            builder.Append(":");
            builder.Append(Bool(value));
            if (trailingComma)
            {
                builder.Append(",");
            }
        }

        private static void AppendVector(StringBuilder builder, Vector3 value)
        {
            builder.Append("[");
            builder.Append(value.x.ToString("0.###", Invariant));
            builder.Append(",");
            builder.Append(value.y.ToString("0.###", Invariant));
            builder.Append(",");
            builder.Append(value.z.ToString("0.###", Invariant));
            builder.Append("]");
        }

        private static void AppendQuoted(StringBuilder builder, string value)
        {
            builder.Append("\"");
            builder.Append(Escape(value));
            builder.Append("\"");
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private readonly struct TargetSpec
        {
            public TargetSpec(string targetId, LevelAssetCategory category, string sourcePart, string gameplayRole, string preferredFacing, string conceptPrompt, PrimitiveType primitive, Vector3 position, Vector3 scale, float yaw)
            {
                TargetId = targetId;
                Category = category;
                SourcePart = sourcePart;
                GameplayRole = gameplayRole;
                PreferredFacing = preferredFacing;
                ConceptPrompt = conceptPrompt;
                Primitive = primitive;
                Position = position;
                Scale = scale;
                Yaw = yaw;
            }

            public string TargetId { get; }
            public LevelAssetCategory Category { get; }
            public string SourcePart { get; }
            public string GameplayRole { get; }
            public string PreferredFacing { get; }
            public string ConceptPrompt { get; }
            public PrimitiveType Primitive { get; }
            public Vector3 Position { get; }
            public Vector3 Scale { get; }
            public float Yaw { get; }
        }

        private readonly struct MaterialSet
        {
            public MaterialSet(Material ground, Material path, Material architecture, Material foliage, Material prop, Material landmark, Material gameplay, Material blocker, Material generated)
            {
                Ground = ground;
                Path = path;
                Architecture = architecture;
                Foliage = foliage;
                Prop = prop;
                Landmark = landmark;
                Gameplay = gameplay;
                Blocker = blocker;
                Generated = generated;
            }

            public Material Ground { get; }
            public Material Path { get; }
            public Material Generated { get; }

            public Material ForCategory(LevelAssetCategory category)
            {
                return category switch
                {
                    LevelAssetCategory.Path => Path,
                    LevelAssetCategory.Architecture => Architecture,
                    LevelAssetCategory.Foliage => Foliage,
                    LevelAssetCategory.Landmark => Landmark,
                    LevelAssetCategory.Gameplay => Gameplay,
                    LevelAssetCategory.Blocker => Blocker,
                    _ => Prop,
                };
            }

            private Material Architecture { get; }
            private Material Foliage { get; }
            private Material Prop { get; }
            private Material Landmark { get; }
            private Material Gameplay { get; }
            private Material Blocker { get; }
        }
    }
}
