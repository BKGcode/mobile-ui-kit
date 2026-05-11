#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace KitforgeLabs.MobileUIKit.Bootstrap
{
    public static class KitforgeInputBootstrap
    {
        private const string InputSystemModuleTypeName = "UnityEngine.InputSystem.UI.InputSystemUIInputModule";
        private static Type _moduleType;
        private static bool _resolved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            PatchAll();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PatchAll();
        }

        private static void PatchAll()
        {
            var moduleType = Resolve();
            if (moduleType == null) return;
            foreach (var es in UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                PatchEventSystem(es, moduleType);
            }
        }

        private static void PatchEventSystem(EventSystem es, Type moduleType)
        {
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy == null) return;
            UnityEngine.Object.Destroy(legacy);
            if (es.GetComponent(moduleType) == null) es.gameObject.AddComponent(moduleType);
        }

        private static Type Resolve()
        {
            if (_resolved) return _moduleType;
            _resolved = true;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(InputSystemModuleTypeName, false);
                if (t == null) continue;
                _moduleType = t;
                return t;
            }
            return null;
        }
    }
}
#endif
