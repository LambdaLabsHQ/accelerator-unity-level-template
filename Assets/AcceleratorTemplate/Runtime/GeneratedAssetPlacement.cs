using UnityEngine;

namespace AcceleratorTemplate.Runtime
{
    [DisallowMultipleComponent]
    public sealed class GeneratedAssetPlacement : MonoBehaviour
    {
        [SerializeField] private string targetId = "";
        [SerializeField] private string sourcePart = "";
        [SerializeField] private string conceptArtifactSlot = "";
        [SerializeField] private string meshArtifactSlot = "";
        [SerializeField] private string placementStatus = "placeholder";

        public string TargetId => targetId;
        public string SourcePart => sourcePart;
        public string ConceptArtifactSlot => conceptArtifactSlot;
        public string MeshArtifactSlot => meshArtifactSlot;
        public string PlacementStatus => placementStatus;

        public void Configure(
            string id,
            string source,
            string conceptSlot,
            string meshSlot,
            string status)
        {
            targetId = id;
            sourcePart = source;
            conceptArtifactSlot = conceptSlot;
            meshArtifactSlot = meshSlot;
            placementStatus = status;
        }
    }
}
