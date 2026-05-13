using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Hub
{
    public static class KitforgeDemoLauncher
    {
        private const string DemoScenePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Demo/KitforgeDemoScene.unity";

        [MenuItem("KitforgeLabs/UI Kit/Open Demo Scene", priority = 10)]
        public static void OpenDemoScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(DemoScenePath);
            if (sceneAsset == null)
            {
                Debug.LogError($"[KitforgeDemoLauncher] Demo scene not found at {DemoScenePath}. Reinstall the KitforgeLabs UI Kit package.");
                return;
            }
            EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
        }
    }
}
