using System;
using KitforgeLabs.UIKit.Animation;
using KitforgeLabs.UIKit.Bootstrap;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.UIKit.Editor.Hub.Setup
{
    internal sealed class KitforgeSetupWizard
    {
        private const string KitforgeRootPrefabPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab";
        private const string BootstrapDefaultsMenuPath = "KitforgeLabs/UI Kit/Bootstrap Defaults";

        private readonly KitforgeHubWindow _hostWindow;

        private VisualElement _root;
        private VisualElement _step1Card;
        private VisualElement _step2Card;
        private VisualElement _step3Card;
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
            _step1Card = BuildStepCard(1, "Bootstrap Defaults", BuildStep1Description(), "Run Bootstrap Defaults", RunStep1);
            _step2Card = BuildStepCard(2, "Add Scene Root", BuildStep2Description(), "Add Scene Root", RunStep2);
            _step3Card = BuildStepCard(3, "Browse Catalog", BuildStep3Description(), "Open Catalog Tab", RunStep3);
            _root.Add(_step1Card);
            _root.Add(_step2Card);
            _root.Add(_step3Card);
            _summaryLabel = new Label(string.Empty);
            _summaryLabel.AddToClassList("kfh-wizard-summary");
            _root.Add(_summaryLabel);
            Refresh();
            return _root;
        }

        public void Refresh()
        {
            RefreshStep(_step1Card, IsStep1Done());
            RefreshStep(_step2Card, IsStep2Done());
            RefreshStep(_step3Card, IsStep3Done());
            _summaryLabel.text = AllStepsDone() ? "Setup complete — open the Catalog tab to browse the 17 elements and copy spawn snippets." : string.Empty;
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
            var guids = AssetDatabase.FindAssets($"t:{nameof(UIAnimPreset)}");
            return guids != null && guids.Length >= 1;
        }

        private bool IsStep2Done()
        {
            var binders = UnityEngine.Object.FindObjectsByType<KitforgeThemeBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return binders != null && binders.Length > 0;
        }

        private bool IsStep3Done() => false;

        private void RunStep1() => InvokeMenu(BootstrapDefaultsMenuPath);

        private void RunStep2() => AddSceneRootToActiveScene();

        private void RunStep3()
        {
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

        private void InvokeMenu(string path)
        {
            if (!EditorApplication.ExecuteMenuItem(path))
            {
                Debug.LogError($"[KitforgeSetupWizard] Menu '{path}' not found. Package may be partially installed; reinstall the KitforgeLabs UI Kit.");
            }
        }

        private string BuildStep1Description()
        {
            return "Generates 10 UIAnimPreset assets at Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets/. Required once per project. Idempotent — safe to re-run.";
        }

        private string BuildStep2Description()
        {
            return "Drops KitforgeRoot.prefab into the active scene. Pre-wires 3 managers + UIServices + EventSystem + PopupBackdrop + Theme_Default. Press Play to verify zero LogError.";
        }

        private string BuildStep3Description()
        {
            return "Switches to the Catalog tab. Search any of the 17 catalog elements, copy the canonical Show<T>(dto) / Push<T>(dto) snippet, paste into your game code.";
        }
    }
}
