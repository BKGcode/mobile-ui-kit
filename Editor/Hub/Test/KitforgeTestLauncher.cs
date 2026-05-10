using System;
using System.Reflection;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;
using KitforgeLabs.MobileUIKit.Toast;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Test
{
    internal sealed class KitforgeTestLauncher
    {
        private readonly KitforgeHubState _state;
        private readonly EditorWindow _hostWindow;

        private VisualElement _root;

        public KitforgeTestLauncher(KitforgeHubState state, EditorWindow hostWindow)
        {
            _state = state;
            _hostWindow = hostWindow;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.AddToClassList("kfh-test");
            BuildBanner();
            BuildElementList();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            _root.RegisterCallback<DetachFromPanelEvent>(_ => EditorApplication.playModeStateChanged -= OnPlayModeChanged);
            return _root;
        }

        private void OnPlayModeChanged(PlayModeStateChange change) => Refresh();

        private void Refresh()
        {
            _root.Clear();
            BuildBanner();
            BuildElementList();
        }

        private void BuildBanner()
        {
            var banner = new VisualElement();
            banner.AddToClassList("kfh-test-banner");
            if (Application.isPlaying) banner.AddToClassList("kfh-test-banner--play");
            var msg = new Label(Application.isPlaying
                ? "Play mode active — click Spawn on any element below."
                : "Enter Play mode to spawn catalog elements with mock DTO data + Force scenarios.");
            msg.AddToClassList("kfh-test-banner-message");
            banner.Add(msg);
            _root.Add(banner);
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
            btn.SetEnabled(Application.isPlaying);
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
            btn.SetEnabled(Application.isPlaying);
            row.Add(btn);
            return row;
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
