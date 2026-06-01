using UnityEngine;

namespace AcceleratorTemplate.Runtime
{
    public enum LevelAssetCategory
    {
        Architecture,
        Foliage,
        Prop,
        Landmark,
        Path,
        Gameplay,
        Blocker
    }

    [DisallowMultipleComponent]
    public sealed class LevelDesignTarget : MonoBehaviour
    {
        [SerializeField] private string targetId = "";
        [SerializeField] private LevelAssetCategory category = LevelAssetCategory.Prop;
        [SerializeField] private string sourcePart = "";
        [SerializeField] private string gameplayRole = "";
        [SerializeField] private string preferredFacing = "";
        [TextArea(3, 8)]
        [SerializeField] private string conceptPrompt = "";

        public string TargetId => targetId;
        public LevelAssetCategory Category => category;
        public string SourcePart => sourcePart;
        public string GameplayRole => gameplayRole;
        public string PreferredFacing => preferredFacing;
        public string ConceptPrompt => conceptPrompt;

        public void Configure(
            string id,
            LevelAssetCategory assetCategory,
            string source,
            string role,
            string facing,
            string prompt)
        {
            targetId = id;
            category = assetCategory;
            sourcePart = source;
            gameplayRole = role;
            preferredFacing = facing;
            conceptPrompt = prompt;
        }

        public Bounds WorldBounds()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(transform.position, transform.lossyScale);
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index += 1)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }
    }
}
