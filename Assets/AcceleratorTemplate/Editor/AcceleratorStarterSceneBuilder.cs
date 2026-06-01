using UnityEditor;

namespace AcceleratorTemplate.Editor
{
    public static class AcceleratorStarterSceneBuilder
    {
        [MenuItem("Accelerator/Create Starter Scene")]
        public static string CreateStarterScene()
        {
            return AcceleratorLevelProductionTemplate.GenerateSeedWhitebox();
        }

        public static string SmokeCheck()
        {
            return AcceleratorLevelProductionTemplate.AuditWhitebox();
        }

        public static string EnterPlayMode()
        {
            return AcceleratorLevelProductionTemplate.EnterPlayMode();
        }

        public static string PlayModeSmokeCheck()
        {
            return AcceleratorLevelProductionTemplate.AuditPlacements();
        }

        public static string ExitPlayMode()
        {
            return AcceleratorLevelProductionTemplate.ExitPlayMode();
        }
    }
}
