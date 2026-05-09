using System;
using System.Collections.Generic;
using System.IO;
using KitforgeLabs.MobileUIKit.Editor.Generators;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class UIKitAuditWindow : EditorWindow
    {
        private const string UssPath = "Packages/com.kitforgelabs.mobile-ui-kit/Editor/Audit/UIKitAuditWindow.uss";

        public enum Tab { Overview, Prefabs, Scenes, Snapshots, Reports, Settings }

        private List<UIKitAuditTarget> _targets = new();
        private UIKitAuditRunReport _lastRun;
        private string _filter = "All";
        private bool _mirrorReportsToAssets;
        private UIThemeConfig _selectedTheme;
        private Tab _activeTab = Tab.Overview;
        private VisualElement _content;
        private Label _summaryLabel;
        private VisualElement _tabsBar;
        private DropdownField _filterDropdown;

        public List<UIKitAuditTarget> Targets => _targets;
        public UIKitAuditRunReport LastRun => _lastRun;
        public string Filter => _filter;
        public UIThemeConfig SelectedTheme => _selectedTheme;
        public bool MirrorReportsToAssets { get => _mirrorReportsToAssets; set => _mirrorReportsToAssets = value; }

        [MenuItem("Tools/Kitforge/UI Kit/Audit")]
        public static void Open()
        {
            var w = GetWindow<UIKitAuditWindow>("UI Kit Audit");
            w.minSize = new Vector2(1080f, 600f);
        }

        private void CreateGUI()
        {
            BuildLayout();
            Discover();
        }

        private void BuildLayout()
        {
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (ss != null) rootVisualElement.styleSheets.Add(ss);
            rootVisualElement.AddToClassList("audit-root");
            BuildToolbar();
            BuildTabsBar();
            BuildContentArea();
            RenderActiveTab();
        }

        private void BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("audit-toolbar");
            toolbar.Add(MakeButton("Discover", Discover));
            toolbar.Add(MakeButton("Regenerate + Audit", RegenerateAndRunEverything));
            toolbar.Add(MakeButton("Regenerate Samples", RegenerateAllSamples));
            toolbar.Add(MakeButton("Run Everything", () => RunAudit(UIKitAuditDimensions.All, "All")));
            toolbar.Add(MakeButton("Run Catalog", () => RunAudit(UIKitAuditDimensions.Catalog | UIKitAuditDimensions.Visual, "Catalog")));
            toolbar.Add(MakeButton("Run Scenes", () => RunAudit(UIKitAuditDimensions.Scenes, "Scenes")));
            toolbar.Add(MakeButton("Run Prefab Structural", () => RunAudit(UIKitAuditDimensions.PrefabStructural, "PrefabStructural")));
            toolbar.Add(MakeButton("Capture Snapshots", () => RunAudit(UIKitAuditDimensions.Catalog | UIKitAuditDimensions.Visual, "Snapshots")));
            toolbar.Add(MakeButton("Clean Snapshots", CleanSnapshotsConfirmed));
            toolbar.Add(MakeButton("Clear Reports", ClearReportsConfirmed));
            toolbar.Add(MakeButton("Clear All", ClearAllConfirmed));
            toolbar.Add(MakeButton("Reveal Reports", RevealReports));
            BuildFilterDropdown(toolbar);
            var spacer = new VisualElement();
            spacer.AddToClassList("audit-toolbar-spacer");
            toolbar.Add(spacer);
            _summaryLabel = new Label("Idle");
            _summaryLabel.AddToClassList("audit-summary");
            toolbar.Add(_summaryLabel);
            rootVisualElement.Add(toolbar);
        }

        private void BuildFilterDropdown(VisualElement toolbar)
        {
            _filterDropdown = new DropdownField("Show", new List<string> { "All", "FAIL", "PASS" }, 0);
            _filterDropdown.RegisterValueChangedCallback(evt => { _filter = evt.newValue; RenderActiveTab(); });
            toolbar.Add(_filterDropdown);
        }

        private void BuildTabsBar()
        {
            _tabsBar = new VisualElement();
            _tabsBar.AddToClassList("audit-tabs");
            foreach (Tab tab in Enum.GetValues(typeof(Tab))) _tabsBar.Add(MakeTabButton(tab));
            rootVisualElement.Add(_tabsBar);
        }

        private VisualElement MakeTabButton(Tab tab)
        {
            var btn = new Label(tab.ToString());
            btn.AddToClassList("audit-tab");
            btn.userData = tab;
            btn.RegisterCallback<ClickEvent>(_ => SwitchTab(tab));
            return btn;
        }

        private void BuildContentArea()
        {
            _content = new VisualElement();
            _content.AddToClassList("audit-content");
            _content.style.flexGrow = 1;
            rootVisualElement.Add(_content);
        }

        private void SwitchTab(Tab tab)
        {
            _activeTab = tab;
            RenderActiveTab();
        }

        private void RenderActiveTab()
        {
            UpdateTabHighlights();
            _content.Clear();
            switch (_activeTab)
            {
                case Tab.Overview: UIKitAuditWindowTabs.BuildOverview(_content, this); break;
                case Tab.Prefabs: UIKitAuditWindowTabs.BuildTargetsTab(_content, this, UIKitAuditTargetKind.Prefab); break;
                case Tab.Scenes: UIKitAuditWindowTabs.BuildTargetsTab(_content, this, UIKitAuditTargetKind.Scene); break;
                case Tab.Snapshots: UIKitAuditWindowTabs.BuildSnapshotsGrid(_content, this); break;
                case Tab.Reports: UIKitAuditWindowTabs.BuildReportsTab(_content, this); break;
                case Tab.Settings: UIKitAuditWindowTabs.BuildSettingsTab(_content, this); break;
            }
        }

        private void UpdateTabHighlights()
        {
            for (var i = 0; i < _tabsBar.childCount; i++) UpdateTabHighlight(_tabsBar[i]);
        }

        private void UpdateTabHighlight(VisualElement tabElement)
        {
            tabElement.RemoveFromClassList("audit-tab--active");
            if ((Tab)tabElement.userData == _activeTab) tabElement.AddToClassList("audit-tab--active");
        }

        public void Discover()
        {
            _targets = UIKitAuditDiscovery.DiscoverAll();
            _lastRun = null;
            UpdateSummary();
            RenderActiveTab();
        }

        public void RunAudit(UIKitAuditDimensions dimensions, string scope)
        {
            if (!ConfirmSavePrompt()) return;
            try { ExecuteAuditWithProgress(dimensions, scope); }
            finally { EditorUtility.ClearProgressBar(); AssetDatabase.Refresh(); }
            LogRunComplete(scope);
            UpdateSummary();
            RenderActiveTab();
        }

        private void RegenerateAllSamples()
        {
            if (!ConfirmSavePrompt()) return;
            var ok = ExecuteRegenerationWithProgress();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Discover();
            Debug.Log(ok ? "[UIKitAudit] Regenerated all samples (A→E)." : "[UIKitAudit] Regeneration aborted — see errors above.");
        }

        private void RegenerateAndRunEverything()
        {
            if (!ConfirmSavePrompt()) return;
            if (!ExecuteRegenerationWithProgress())
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                Debug.LogError("[UIKitAudit] Regeneration failed — audit skipped. Fix errors above and retry.");
                return;
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Discover();
            try { ExecuteAuditWithProgress(UIKitAuditDimensions.All, "RegenAndAudit"); }
            finally { EditorUtility.ClearProgressBar(); AssetDatabase.Refresh(); }
            LogRunComplete("RegenAndAudit");
            UpdateSummary();
            RenderActiveTab();
        }

        private static bool ExecuteRegenerationWithProgress()
        {
            EditorUtility.DisplayProgressBar("Regenerate Samples", "Group A...", 0f);
            if (!CatalogGroupABuilder.BuildAllForAudit()) return false;
            EditorUtility.DisplayProgressBar("Regenerate Samples", "Group B...", 0.2f);
            if (!CatalogGroupBBuilder.BuildAllForAudit()) return false;
            EditorUtility.DisplayProgressBar("Regenerate Samples", "Group C...", 0.4f);
            if (!CatalogGroupCBuilder.BuildAllForAudit()) return false;
            EditorUtility.DisplayProgressBar("Regenerate Samples", "Group D...", 0.6f);
            if (!CatalogGroupDBuilder.BuildAllForAudit()) return false;
            EditorUtility.DisplayProgressBar("Regenerate Samples", "Group E...", 0.8f);
            if (!CatalogGroupEBuilder.BuildAllForAudit()) return false;
            return true;
        }

        private void ExecuteAuditWithProgress(UIKitAuditDimensions dimensions, string scope)
        {
            var options = new UIKitAuditOptions
            {
                Dimensions = dimensions,
                Trigger = UIKitAuditTrigger.Manual,
                Scope = scope,
                MirrorReportsToAssets = _mirrorReportsToAssets,
                SnapshotTheme = _selectedTheme,
                Progress = OnAuditProgress
            };
            _lastRun = UIKitAuditService.AuditAll(options);
            UIKitAuditReportWriter.Write(_lastRun, _mirrorReportsToAssets);
        }

        public void RunSelected(UIKitAuditTarget target)
        {
            if (target == null) return;
            if (!ConfirmSavePrompt()) return;
            var options = new UIKitAuditOptions
            {
                Dimensions = UIKitAuditDimensions.All,
                Trigger = UIKitAuditTrigger.Manual,
                Scope = "Selected:" + target.Label,
                MirrorReportsToAssets = _mirrorReportsToAssets,
                SnapshotTheme = _selectedTheme
            };
            try { _lastRun = UIKitAuditService.AuditOne(target, options); UIKitAuditReportWriter.Write(_lastRun, _mirrorReportsToAssets); }
            finally { AssetDatabase.Refresh(); }
            UpdateSummary();
            RenderActiveTab();
        }

        private static void OnAuditProgress(int index, int total, string label)
        {
            EditorUtility.DisplayProgressBar("UI Kit Audit", $"{index + 1}/{total} · {label}", (float)index / Mathf.Max(1, total));
        }

        private void LogRunComplete(string scope)
        {
            if (_lastRun == null) return;
            Debug.Log($"[UIKitAudit] {scope} complete · {_lastRun.TotalPass}/{_lastRun.TotalTargets} pass · {_lastRun.TotalFail} fail · See {UIKitAuditReportWriter.ReportsRoot}/_Summary.md");
        }

        private void CleanSnapshotsConfirmed()
        {
            if (!EditorUtility.DisplayDialog("Clean Snapshots",
                $"This will delete every PNG under {PrefabSnapshotCapture.SnapshotFolderRelative}. Continue?",
                "Delete", "Cancel")) return;
            PrefabSnapshotCapture.Clean();
            Debug.Log("[UIKitAudit] Snapshots cleaned.");
        }

        private void ClearReportsConfirmed()
        {
            if (!EditorUtility.DisplayDialog("Clear Reports",
                $"This will delete every report under {UIKitAuditReportWriter.ReportsRoot} and the mirror in {UIKitAuditReportWriter.MirrorRoot}. Continue?",
                "Delete", "Cancel")) return;
            UIKitAuditReportWriter.Clear();
            _lastRun = null;
            UpdateSummary();
            RenderActiveTab();
            Debug.Log("[UIKitAudit] Reports cleared.");
        }

        private void ClearAllConfirmed()
        {
            if (!EditorUtility.DisplayDialog("Clear All",
                $"This will delete every audit artifact:\n\n• Reports under {UIKitAuditReportWriter.ReportsRoot}\n• Mirror reports under {UIKitAuditReportWriter.MirrorRoot}\n• Snapshots under {PrefabSnapshotCapture.SnapshotFolderRelative}\n\nContinue?",
                "Delete All", "Cancel")) return;
            UIKitAuditReportWriter.Clear();
            PrefabSnapshotCapture.Clean();
            _lastRun = null;
            UpdateSummary();
            RenderActiveTab();
            Debug.Log("[UIKitAudit] All artifacts cleared (reports + mirror + snapshots).");
        }

        private static void RevealReports()
        {
            var path = UIKitAuditReportWriter.ReportsRoot;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        public void SetTheme(UIThemeConfig theme)
        {
            _selectedTheme = theme;
        }

        public UIKitAuditTargetReport FindReport(string label)
        {
            if (_lastRun == null) return null;
            for (var i = 0; i < _lastRun.Targets.Count; i++) if (_lastRun.Targets[i].Label == label) return _lastRun.Targets[i];
            return null;
        }

        private void UpdateSummary()
        {
            if (_summaryLabel == null) return;
            if (_lastRun == null) { _summaryLabel.text = $"{_targets.Count} targets · no audit yet"; return; }
            _summaryLabel.text = BuildSummaryText();
        }

        private string BuildSummaryText()
        {
            return $"{_lastRun.TotalPass}/{_lastRun.TotalTargets} pass · Catalog {_lastRun.CatalogPass}/{_lastRun.CatalogTotal} · Scenes {_lastRun.ScenePass}/{_lastRun.SceneTotal} · {_lastRun.GeneratedAtUtc}";
        }

        private static Button MakeButton(string text, Action onClick)
        {
            return new Button(onClick) { text = text };
        }

        private static bool ConfirmSavePrompt()
        {
            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
    }
}
