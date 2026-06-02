using UnityEditor;

namespace AcceleratorTemplate.Editor
{
    public static class AcceleratorStarterSceneBuilder
    {
        [MenuItem("Accelerator/Create Starter Scene")]
        public static string CreateStarterScene()
        {
            return AcceleratorLevelProductionTemplate.CreateFreshProductionScene();
        }

        public static string SmokeCheck()
        {
            return AcceleratorLevelProductionTemplate.AuditActiveScene();
        }

        public static string EnterPlayMode()
        {
            return AcceleratorLevelProductionTemplate.EnterPlayMode();
        }

        public static string PlayModeSmokeCheck()
        {
            return AcceleratorLevelProductionTemplate.AuditActiveScene();
        }

        public static string ExitPlayMode()
        {
            return AcceleratorLevelProductionTemplate.ExitPlayMode();
        }
    }
}
