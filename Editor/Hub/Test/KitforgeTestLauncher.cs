using System;
using System.IO;
using System.Reflection;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;
using KitforgeLabs.MobileUIKit.Editor.Hub.Setup;
using KitforgeLabs.MobileUIKit.Toast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Test
{
    internal sealed class KitforgeTestLauncher
    {
        private const string CatalogAllDemoScenePath = "Assets/Catalog_All_Demo/AllDemo.unity";

        private readonly KitforgeHubState _state;
        private readonly EditorWindow _hostWindow;

        private VisualElement _root;
        private bool _hasPopupManager;
        private bool _hasUIManager;
        private bool _hasToastManager;

        public KitforgeTestLauncher(KitforgeHubState state, EditorWindow hostWindow)
        {
            _state = state;
            _hostWindow = hostWindow;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.AddToClassList("kfh-test");
            DetectManagers();
            BuildBanner();
            BuildElementList();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            _root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                EditorSceneManager.sceneOpened -= OnSceneOpened;
            });
            return _root;
        }

        private void OnPlayModeChanged(PlayModeStateChange change) => Refresh();
        private void OnHierarchyChanged() => Refresh();
        private void OnSceneOpened(Scene scene, OpenSceneMode mode) => Refresh();

        private void Refresh()
        {
            DetectManagers();
            _root.Clear();
            BuildBanner();
            BuildElementList();
        }

        private void DetectManagers()
        {
            _hasPopupManager = UnityEngine.Object.FindAnyObjectByType<PopupManager>() != null;
            _hasUIManager = UnityEngine.Object.FindAnyObjectByType<UIManager>() != null;
            _hasToastManager = UnityEngine.Object.FindAnyObjectByType<ToastManager>() != null;
        }

        private bool AnyManagerPresent() => _hasPopupManager || _hasUIManager || _hasToastManager;

        private void BuildBanner()
        {
            var banner = new VisualElement();
            banner.AddToClassList("kfh-test-banner");
            if (!AnyManagerPresent())
            {
                banner.AddToClassList("kfh-test-banner--warn");
                BuildEmptyStateBanner(banner);
            }
            else if (Application.isPlaying)
            {
                banner.AddToClassList("kfh-test-banner--play");
                AppendBannerMessage(banner, "Play mode active — click Spawn on any element below.");
            }
            else
            {
                AppendBannerMessage(banner, "KitforgeRoot detected — enter Play mode to spawn catalog elements with mock DTO data + Force scenarios.");
            }
            _root.Add(banner);
        }

        private void BuildEmptyStateBanner(VisualElement banner)
        {
            var column = new VisualElement();
            column.AddToClassList("kfh-test-banner-column");
            var msg = new Label("No KitforgeRoot in active scene — Test tab needs PopupManager / UIManager / ToastManager to spawn elements.");
            msg.AddToClassList("kfh-test-banner-message");
            column.Add(msg);
            var actions = new VisualElement();
            actions.AddToClassList("kfh-test-banner-actions");
            var addRoot = new Button(AddSceneRoot) { text = "Add Scene Root" };
            addRoot.AddToClassList("kfh-test-banner-action");
            addRoot.SetEnabled(!Application.isPlaying);
            actions.Add(addRoot);
            if (CatalogAllDemoSceneExists())
            {
                var openDemo = new Button(OpenCatalogAllDemo) { text = "Open Catalog_All_Demo" };
                openDemo.AddToClassList("kfh-test-banner-action");
                openDemo.SetEnabled(!Application.isPlaying);
                actions.Add(openDemo);
            }
            var goSetup = new Button(GoToSetupTab) { text = "Open Setup tab" };
            goSetup.AddToClassList("kfh-test-banner-action");
            actions.Add(goSetup);
            column.Add(actions);
            if (Application.isPlaying)
            {
                var hint = new Label("Exit Play mode to bootstrap a scene — KitforgeRoot must exist before pressing Play.");
                hint.AddToClassList("kfh-test-banner-hint");
                column.Add(hint);
            }
            banner.Add(column);
        }

        private static void AppendBannerMessage(VisualElement banner, string text)
        {
            var msg = new Label(text);
            msg.AddToClassList("kfh-test-banner-message");
            banner.Add(msg);
        }

        private void AddSceneRoot()
        {
            KitforgeSetupWizard.AddSceneRootToActiveScene();
            Refresh();
        }

        private void OpenCatalogAllDemo()
        {
            if (!CatalogAllDemoSceneExists())
            {
                Debug.LogError($"[KitforgeTestLauncher] Catalog_All_Demo scene not found at '{CatalogAllDemoScenePath}'. Import 'Catalog — All — Single-import master demo' from Package Manager → Samples and run Tools → Kitforge → UI Kit → Build Catalog_All_Demo.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(CatalogAllDemoScenePath, OpenSceneMode.Single);
        }

        private void GoToSetupTab()
        {
            if (_hostWindow is KitforgeHubWindow hub) hub.SwitchToTab(KitforgeHubState.HubTab.Setup);
        }

        private static bool CatalogAllDemoSceneExists()
        {
            return File.Exists(CatalogAllDemoScenePath);
        }

        private void BuildElementList()
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("kfh-test-list");
            AppendForceScenariosSection(scroll);
            AppendPatternSection(scroll, KitforgeSpawnPattern.Popup);
            AppendPatternSection(scroll, KitforgeSpawnPattern.Toast);
            AppendPatternSection(scroll, KitforgeSpawnPattern.Screen);
            AppendHUDSection(scroll);
            _root.Add(scroll);
        }

        private void AppendForceScenariosSection(ScrollView scroll)
        {
            var header = new Label("Force scenarios");
            header.AddToClassList("kfh-test-section-header");
            scroll.Add(header);
            var note = new Label("Spawn catalog popups pre-configured for specific edge cases (DTO override; no service mutation). Requires the relevant Group sample built and prefabs wired in the active scene's PopupManager — easiest path: open the demo scene from Tools → Kitforge → UI Kit → Build Group X Sample.");
            note.AddToClassList("kfh-test-section-note");
            scroll.Add(note);
            foreach (var scenario in KitforgeForceScenariosRegistry.All)
            {
                scroll.Add(BuildForceScenarioRow(scenario));
            }
        }

        private void AppendPatternSection(ScrollView scroll, KitforgeSpawnPattern pattern)
        {
            var header = new Label(PatternSectionLabel(pattern));
            header.AddToClassList("kfh-test-section-header");
            scroll.Add(header);
            foreach (var entry in KitforgeCatalogRegistry.All)
            {
                if (entry.Pattern == pattern) scroll.Add(BuildElementRow(entry));
            }
        }

        private void AppendHUDSection(ScrollView scroll)
        {
            var header = new Label("HUDs");
            header.AddToClassList("kfh-test-section-header");
            scroll.Add(header);
            var note = new Label("HUDs are scene prefabs — drag them from the Catalog tab into KitforgeRoot/UICanvas/ScreenRoot. They auto-bind to services on enable.");
            note.AddToClassList("kfh-test-section-note");
            scroll.Add(note);
        }

        private VisualElement BuildElementRow(KitforgeCatalogEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("kfh-test-row");
            var label = new Label($"{entry.DisplayName}  ·  Group {entry.Group}  ·  {entry.ComponentType.Name}");
            label.AddToClassList("kfh-test-row-label");
            row.Add(label);
            var btn = new Button(() => SpawnEntry(entry)) { text = "Spawn" };
            btn.AddToClassList("kfh-test-row-button");
            btn.SetEnabled(IsSpawnEnabled(entry.Pattern));
            row.Add(btn);
            return row;
        }

        private VisualElement BuildForceScenarioRow(KitforgeForceScenario scenario)
        {
            var row = new VisualElement();
            row.AddToClassList("kfh-test-row");
            var label = new Label($"{scenario.Label}  —  {scenario.Description}");
            label.AddToClassList("kfh-test-row-label");
            row.Add(label);
            var btn = new Button(() => SpawnForceScenario(scenario)) { text = "Spawn" };
            btn.AddToClassList("kfh-test-row-button");
            btn.SetEnabled(IsSpawnEnabled(scenario.Pattern));
            row.Add(btn);
            return row;
        }

        private bool IsSpawnEnabled(KitforgeSpawnPattern pattern)
        {
            if (!Application.isPlaying) return false;
            return pattern switch
            {
                KitforgeSpawnPattern.Popup => _hasPopupManager,
                KitforgeSpawnPattern.Screen => _hasUIManager,
                KitforgeSpawnPattern.Toast => _hasToastManager,
                _ => false,
            };
        }

        private void SpawnEntry(KitforgeCatalogEntry entry)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[KitforgeTestLauncher] Enter Play mode to spawn elements.");
                return;
            }
            var data = KitforgeMockDtoFactory.Create(entry);
            SpawnTypedData(entry.ComponentType, entry.Pattern, data, entry.DisplayName);
        }

        private void SpawnForceScenario(KitforgeForceScenario scenario)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[KitforgeTestLauncher] Enter Play mode to spawn Force scenarios.");
                return;
            }
            var data = KitforgeMockDtoFactory.CreateForComponent(scenario.ComponentType, scenario.Label);
            if (data != null) scenario.ConfigureMock(data);
            SpawnTypedData(scenario.ComponentType, scenario.Pattern, data, scenario.Label);
        }

        private void SpawnTypedData(Type componentType, KitforgeSpawnPattern pattern, object data, string displayName)
        {
            switch (pattern)
            {
                case KitforgeSpawnPattern.Popup: SpawnViaManager<PopupManager>(componentType, "Show", data, displayName); break;
                case KitforgeSpawnPattern.Screen: SpawnViaManager<UIManager>(componentType, "Push", data, displayName); break;
                case KitforgeSpawnPattern.Toast: SpawnViaManager<ToastManager>(componentType, "Show", data, displayName); break;
            }
        }

        private void SpawnViaManager<TManager>(Type componentType, string methodName, object data, string displayName) where TManager : MonoBehaviour
        {
            var manager = UnityEngine.Object.FindAnyObjectByType<TManager>();
            if (manager == null)
            {
                Debug.LogError($"[KitforgeTestLauncher] {typeof(TManager).Name} not found in active scene → Run Setup → Step 2 (Add Scene Root) to drop KitforgeRoot.prefab. Or open a Group X demo scene built via Tools → Kitforge → UI Kit → Build Group X Sample.");
                NotifyHub($"Spawn blocked: no {typeof(TManager).Name}");
                return;
            }
            InvokeManagerMethod(manager, methodName, componentType, data, displayName);
        }

        private void InvokeManagerMethod(MonoBehaviour manager, string methodName, Type componentType, object data, string displayName)
        {
            var generic = FindGenericMethod(manager.GetType(), methodName);
            if (generic == null)
            {
                Debug.LogError($"[KitforgeTestLauncher] Generic method '{methodName}' not found on {manager.GetType().Name} → reinstall the Kitforge Mobile UI Kit package; runtime contract drift detected.");
                NotifyHub($"Spawn failed: {displayName}");
                return;
            }
            var concrete = generic.MakeGenericMethod(componentType);
            var args = BuildInvokeArgs(concrete, data);
            try
            {
                var result = concrete.Invoke(manager, args);
                NotifyHub(result == null ? $"Spawn failed: {displayName} (see Console)" : $"Spawned {displayName}");
            }
            catch (TargetInvocationException ex)
            {
                Debug.LogError($"[KitforgeTestLauncher] Spawn failed for {displayName} → {ex.InnerException?.Message ?? ex.Message}. Likely cause: prefab not wired into PopupManager._popupPrefabs[] in the active scene. Open a Group X sample scene built via Tools → Kitforge → UI Kit → Build Group X Sample (those ship pre-wired).");
                NotifyHub($"Spawn failed: {displayName}");
            }
        }

        private void NotifyHub(string message)
        {
            if (_hostWindow == null) return;
            _hostWindow.ShowNotification(new GUIContent(message));
        }

        private static MethodInfo FindGenericMethod(Type managerType, string methodName)
        {
            foreach (var m in managerType.GetMethods())
            {
                if (m.Name == methodName && m.IsGenericMethod) return m;
            }
            return null;
        }

        private static object[] BuildInvokeArgs(MethodInfo method, object data)
        {
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];
            args[0] = data;
            for (var i = 1; i < parameters.Length; i++)
            {
                args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
            }
            return args;
        }

        private static string PatternSectionLabel(KitforgeSpawnPattern pattern)
        {
            return pattern switch
            {
                KitforgeSpawnPattern.Popup => "Popups",
                KitforgeSpawnPattern.Toast => "Toasts",
                KitforgeSpawnPattern.Screen => "Screens",
                _ => pattern.ToString(),
            };
        }
    }
}
