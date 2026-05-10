using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Hub
{
    [InitializeOnLoad]
    internal static class KitforgeHubAutoOpen
    {
        private const string FirstOpenPrefKey = "kf.hub.first_open_done";

        static KitforgeHubAutoOpen()
        {
            if (Application.isBatchMode) return;
            if (EditorPrefs.GetBool(FirstOpenPrefKey, false)) return;
            EditorPrefs.SetBool(FirstOpenPrefKey, true);
            EditorApplication.delayCall += DeferredOpen;
        }

        private static void DeferredOpen()
        {
            EditorApplication.delayCall -= DeferredOpen;
            KitforgeHubWindow.Open();
        }
    }
}
