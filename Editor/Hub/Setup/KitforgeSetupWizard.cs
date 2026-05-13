using System;
using KitforgeLabs.UIKit.Bootstrap;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.UIKit.Editor.Hub.Setup
{
    internal sealed class KitforgeSetupWizard
    {
        private const string KitforgeRootPrefabPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab";
        private const string OpenDemoSceneMenuPath = "KitforgeLabs/UI Kit/Open Demo Scene";
        private const string Step2DoneEditorPrefKey = "kf.hub.setup.step2.catalog_visited";

        private readonly KitforgeHubWindow _hostWindow;

        private VisualElement _root;
        private VisualElement _demoBanner;
        private VisualElement _step1Card;
        private VisualElement _step2Card;
        private Label _summaryLabel;

        public KitforgeSetupWizard(KitforgeHubWindow hostWindow)
        {
            _hostWindow = hostWindow;
        }

        public KitforgeSetupWizard() : this(null) { }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.AddToClassList("kfh-wizard");
            _demoBanner = BuildDemoBanner();
            _step1Card = BuildStepCard(1, "Add Scene Root", BuildStep1Description(), "Add Scene Root", RunStep1);
            _step2Card = BuildStepCard(2, "Browse Catalog", BuildStep2Description(), "Open Catalog Tab", RunStep2);
            _root.Add(_demoBanner);
            _root.Add(_step1Card);
            _root.Add(_step2Card);
            _summaryLabel = new Label(string.Empty);
            _summaryLabel.AddToClassList("kfh-wizard-summary");
            _root.Add(_summaryLabel);
            Refresh();
            return _root;
        }

        private VisualElement BuildDemoBanner()
        {
            var banner = new VisualElement();
            banner.AddToClassList("kfh-wizard-banner");
            var title = new Label("Recommended path — Open the Demo Scene");
            title.AddToClassList("kfh-wizard-banner-title");
            banner.Add(title);
            var description = new Label("KitforgeDemoScene.unity ships pre-wired with the 17-element catalog, demo services and theme cycling. Press Play and explore. The 2 steps below are for starting from a blank scene.");
            description.AddToClassList("kfh-wizard-banner-description");
            banner.Add(description);
            var button = new Button(OpenDemoScene) { text = "Open Demo Scene" };
            button.AddToClassList("kfh-wizard-banner-button");
            banner.Add(button);
            return banner;
        }

        private void OpenDemoScene()
        {
            if (!EditorApplication.ExecuteMenuItem(OpenDemoSceneMenuPath))
            {
                Debug.LogError($"[KitforgeSetupWizard] Menu '{OpenDemoSceneMenuPath}' not found. Reinstall the KitforgeLabs UI Kit package.");
            }
        }

        public void Refresh()
        {
            RefreshStep(_step1Card, IsStep1Done());
            RefreshStep(_step2Card, IsStep2Done());
            _summaryLabel.text = AllStepsDone() ? "Setup complete — your scene is wired and you visited the Catalog. Copy spawn snippets and paste into your game code." : string.Empty;
        }

        private VisualElement BuildStepCard(int index, string title, string description, string actionLabel, Action onAction)
        {
            var card = new VisualElement();
            card.AddToClassList("kfh-wizard-card");
            card.Add(BuildCardHeader(index, title));
            var desc = new Label(description);
            desc.AddToClassList("kfh-wizard-card-description");
            card.Add(desc);
            var button = new Button(() => { onAction(); Refresh(); }) { text = actionLabel };
            button.AddToClassList("kfh-wizard-card-button");
            card.Add(button);
            return card;
        }

        private VisualElement BuildCardHeader(int index, string title)
        {
            var header = new VisualElement();
            header.AddToClassList("kfh-wizard-card-header");
            var titleLabel = new Label($"{index}. {title}");
            titleLabel.AddToClassList("kfh-wizard-card-title");
            header.Add(titleLabel);
            var statusLabel = new Label("Pending");
            statusLabel.AddToClassList("kfh-wizard-card-status");
            header.Add(statusLabel);
            return header;
        }

        private void RefreshStep(VisualElement card, bool isDone)
        {
            var status = card.Q<Label>(className: "kfh-wizard-card-status");
            if (status == null) return;
            status.text = isDone ? "Done" : "Pending";
            status.EnableInClassList("kfh-wizard-card-status--done", isDone);
        }

        private bool AllStepsDone() => IsStep1Done() && IsStep2Done();

        private bool IsStep1Done()
        {
            var binders = UnityEngine.Object.FindObjectsByType<KitforgeThemeBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return binders != null && binders.Length > 0;
        }

        private bool IsStep2Done() => EditorPrefs.GetBool(Step2DoneEditorPrefKey, false);

        private void RunStep1() => AddSceneRootToActiveScene();

        private void RunStep2()
        {
            EditorPrefs.SetBool(Step2DoneEditorPrefKey, true);
            if (_hostWindow != null) _hostWindow.SwitchToTab(KitforgeHubState.HubTab.Catalog);
        }

        public static void AddSceneRootToActiveScene()
        {
            if (TryPingExistingBinder()) return;
            InstantiateNewKitforgeRoot();
        }

        private static bool TryPingExistingBinder()
        {
            var binders = UnityEngine.Object.FindObjectsByType<KitforgeThemeBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (binders == null || binders.Length == 0) return false;
            Selection.activeGameObject = binders[0].gameObject;
            EditorGUIUtility.PingObject(binders[0]);
            return true;
        }

        private static void InstantiateNewKitforgeRoot()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KitforgeRootPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[KitforgeSetupWizard] KitforgeRoot.prefab not found at '{KitforgeRootPrefabPath}'. Reinstall the KitforgeLabs UI Kit package.");
                return;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null) return;
            Undo.RegisterCreatedObjectUndo(instance, "Add KitforgeLabs Scene Root");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        private string BuildStep1Description()
        {
            return "Drops KitforgeRoot.prefab into the active scene. Pre-wires 3 managers + UIServices + EventSystem + PopupBackdrop + Theme_Default. Press Play to verify zero LogError.";
        }

        private string BuildStep2Description()
        {
            return "Switches to the Catalog tab. Search any of the 17 catalog elements, copy the canonical Show<T>(dto) / Push<T>(dto) snippet, paste into your game code.";
        }
    }
}
