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
        private const string ScenesDirectory = "Assets/Scenes";
        private const string DefaultScenePath = "Assets/Scenes/GeneratedLevel.unity";
        private const string WhiteboxRootName = "00_Whitebox_From_LevelDesign";
        private const string ArtifactsRootName = "01_Generated_Artifacts";
        private const string GeneratedRootName = "02_Placed_Generated_Art";
        private const string AuditRootName = "03_Audit";

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        [MenuItem("Accelerator/Level Production/Create Fresh Production Scene")]
        public static string CreateFreshProductionScene()
        {
            Directory.CreateDirectory(ScenesDirectory);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "GeneratedLevel";

            var root = new GameObject("Accelerator_FromZero_Level");
            CreateChild(root.transform, WhiteboxRootName);
            CreateChild(root.transform, ArtifactsRootName);
            CreateChild(root.transform, GeneratedRootName);
            CreateChild(root.transform, AuditRootName);

            EditorSceneManager.SaveScene(scene, DefaultScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(DefaultScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AuditActiveScene();
        }

        [MenuItem("Accelerator/Level Production/Audit Active Scene")]
        public static string AuditActiveScene()
        {
            var targets = FindTargets();
            var placements = UnityEngine.Object.FindObjectsOfType<GeneratedAssetPlacement>();
            var matched = targets.Count(target => placements.Any(placement => placement.TargetId == target.TargetId));
            var missing = targets
                .Where(target => !placements.Any(placement => placement.TargetId == target.TargetId))
                .Select(target => target.TargetId)
                .ToArray();

            var builder = new StringBuilder();
            builder.Append("{");
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
    }
}
