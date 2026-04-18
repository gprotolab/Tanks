using UnityEditor;
using UnityEditor.SceneManagement;

namespace ANut.Core.DevTools.Editor
{
    [InitializeOnLoad]
    public static class BootSceneAutoLoader
    {
        private const string EnabledKey = "BootSceneAutoLoader.Enabled";
        private const string MenuPath = "My Game/Auto Load Boot Scene";

        private static bool IsEnabled => EditorPrefs.GetBool(EnabledKey, true);

        static BootSceneAutoLoader() => ApplyPlayModeStartScene();

        [MenuItem(MenuPath)]
        private static void ToggleAutoLoad()
        {
            bool isEnabled = !IsEnabled;
            EditorPrefs.SetBool(EnabledKey, isEnabled);
            ApplyPlayModeStartScene();
            Log.Info("<color=cyan>[Boot Auto Loader]</color> {0}", isEnabled ? "Enabled" : "Disabled");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleAutoLoadValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static void ApplyPlayModeStartScene()
        {
            if (!IsEnabled)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            string bootSceneName = "BootScene";
            foreach (var sceneGuid in AssetDatabase.FindAssets($"{bootSceneName} t:Scene"))
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (fileName.Equals(bootSceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                    return;
                }
            }

            Log.Warning("[Boot Auto Loader] Boot scene '{0}' not found in project.", bootSceneName);
        }
    }
}