using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public sealed class QASuiteWindow : EditorWindow
    {
        private const string UssPath = "Packages/com.kitforgelabs.mobile-ui-kit/Editor/QA/QASuiteWindow.uss";

        private List<QATarget> _targets = new();
        private QASuiteReport _report = new();
        private ListView _targetList;
        private ListView _entryList;
        private Label _summary;

        [MenuItem("Kitforge/UI Kit/QA Suite")]
        public static void Open()
        {
            var w = GetWindow<QASuiteWindow>("QA Suite");
            w.minSize = new Vector2(820f, 460f);
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
            rootVisualElement.AddToClassList("qa-root");
            BuildToolbar();
            BuildSplit();
        }

        private void BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("qa-toolbar");
            toolbar.Add(MakeButton("Discover", Discover));
            toolbar.Add(MakeButton("Rebuild Samples", RebuildSamples));
            toolbar.Add(MakeButton("Rebuild + Run All", RebuildAndRunAll));
            toolbar.Add(MakeButton("Run All", RunAll));
            toolbar.Add(MakeButton("Run Selected", RunSelected));
            toolbar.Add(MakeButton("Clear", ClearReport));
            _summary = new Label("Idle");
            _summary.AddToClassList("qa-summary");
            toolbar.Add(_summary);
            rootVisualElement.Add(toolbar);
        }

        private static Button MakeButton(string text, System.Action onClick)
        {
            var b = new Button(onClick) { text = text };
            b.AddToClassList("qa-btn");
            return b;
        }

        private void BuildSplit()
        {
            var split = new TwoPaneSplitView(0, 260f, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            split.Add(BuildTargetPane());
            split.Add(BuildEntryPane());
            rootVisualElement.Add(split);
        }

        private VisualElement BuildTargetPane()
        {
            var pane = new VisualElement();
            pane.AddToClassList("qa-pane");
            pane.Add(MakePaneHeader("Targets"));
            _targetList = new ListView(_targets, 22, MakeTargetItem, BindTargetItem);
            _targetList.selectionType = SelectionType.Multiple;
            _targetList.style.flexGrow = 1;
            pane.Add(_targetList);
            return pane;
        }

        private VisualElement BuildEntryPane()
        {
            var pane = new VisualElement();
            pane.AddToClassList("qa-pane");
            pane.Add(MakePaneHeader("Issues"));
            _entryList = new ListView(_report.Entries, 26, MakeEntryItem, BindEntryItem);
            _entryList.selectionType = SelectionType.Single;
            _entryList.selectionChanged += OnEntrySelected;
            _entryList.style.flexGrow = 1;
            pane.Add(_entryList);
            return pane;
        }

        private static Label MakePaneHeader(string text)
        {
            var label = new Label(text) { name = "qa-pane-header" };
            return label;
        }

        private static VisualElement MakeTargetItem()
        {
            var row = new VisualElement();
            row.AddToClassList("qa-row");
            row.Add(new Label { name = "kind" });
            row.Add(new Label { name = "label" });
            return row;
        }

        private void BindTargetItem(VisualElement row, int index)
        {
            var t = _targets[index];
            var kind = row.Q<Label>("kind");
            kind.text = t.TargetKind == QATarget.Kind.Scene ? "S" : "P";
            kind.RemoveFromClassList("qa-kind--scene");
            kind.RemoveFromClassList("qa-kind--prefab");
            kind.AddToClassList(t.TargetKind == QATarget.Kind.Scene ? "qa-kind--scene" : "qa-kind--prefab");
            row.Q<Label>("label").text = t.Label;
        }

        private static VisualElement MakeEntryItem()
        {
            var row = new VisualElement();
            row.AddToClassList("qa-row");
            row.Add(new Label { name = "severity" });
            row.Add(new Label { name = "check" });
            row.Add(new Label { name = "target" });
            row.Add(new Label { name = "message" });
            return row;
        }

        private void BindEntryItem(VisualElement row, int index)
        {
            var e = _report.Entries[index];
            BindEntrySeverity(row, e);
            row.Q<Label>("check").text = e.CheckName;
            row.Q<Label>("target").text = e.TargetLabel;
            row.Q<Label>("message").text = e.Message;
            row.tooltip = string.IsNullOrEmpty(e.OwnerHierarchy) ? e.Message : $"{e.OwnerHierarchy}\n{e.Message}";
        }

        private static void BindEntrySeverity(VisualElement row, QAEntry e)
        {
            var pill = row.Q<Label>("severity");
            pill.text = e.Severity.ToString().ToUpperInvariant();
            pill.RemoveFromClassList("qa-pill--info");
            pill.RemoveFromClassList("qa-pill--warning");
            pill.RemoveFromClassList("qa-pill--error");
            pill.AddToClassList("qa-pill--" + e.Severity.ToString().ToLowerInvariant());
        }

        private void OnEntrySelected(IEnumerable<object> selected)
        {
            foreach (var s in selected) PingEntry(s as QAEntry);
        }

        private static void PingEntry(QAEntry entry)
        {
            if (entry?.OwnerAsset == null) return;
            EditorGUIUtility.PingObject(entry.OwnerAsset);
            Selection.activeObject = entry.OwnerAsset;
        }

        private void Discover()
        {
            _targets = QASuiteRunner.DiscoverTargets();
            _targetList.itemsSource = _targets;
            _targetList.Rebuild();
            UpdateSummary();
        }

        private void RunAll()
        {
            if (!ConfirmSavePrompt()) return;
            _report.Clear();
            RunWithProgress(_targets);
            LogRunComplete(_targets.Count);
            RebindEntries();
        }

        private void RunSelected()
        {
            if (!ConfirmSavePrompt()) return;
            _report.Clear();
            var selected = new List<QATarget>();
            foreach (var i in _targetList.selectedIndices) selected.Add(_targets[i]);
            RunWithProgress(selected);
            LogRunComplete(selected.Count);
            RebindEntries();
        }

        private void RunWithProgress(IList<QATarget> targets)
        {
            try
            {
                for (var i = 0; i < targets.Count; i++) RunStep(targets, i);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void RunStep(IList<QATarget> targets, int index)
        {
            var t = targets[index];
            EditorUtility.DisplayProgressBar("QA Suite", $"{index + 1}/{targets.Count} · {t.Label}", (float)index / Mathf.Max(1, targets.Count));
            QASuiteRunner.RunOne(t, _report);
        }

        private void LogRunComplete(int scanned)
        {
            Debug.Log($"[QASuite] Run complete · scanned {scanned} · {_report.ErrorCount} errors · {_report.WarningCount} warnings");
        }

        private void ClearReport()
        {
            _report.Clear();
            RebindEntries();
        }

        private void RebuildSamples()
        {
            if (!ConfirmSavePrompt()) return;
            ExecuteRebuild();
            AssetDatabase.Refresh();
            Discover();
        }

        private void RebuildAndRunAll()
        {
            if (!ConfirmSavePrompt()) return;
            ExecuteRebuild();
            AssetDatabase.Refresh();
            Discover();
            _report.Clear();
            RunWithProgress(_targets);
            LogRunComplete(_targets.Count);
            RebindEntries();
        }

        private static void ExecuteRebuild()
        {
            try
            {
                QASuiteBuilders.RebuildAll(OnRebuildProgress);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void OnRebuildProgress(int index, int total, string menuPath)
        {
            EditorUtility.DisplayProgressBar("QA Suite — Rebuild", $"{index}/{total} · {menuPath}", (float)index / Mathf.Max(1, total));
        }

        private static bool ConfirmSavePrompt()
        {
            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        private void RebindEntries()
        {
            _entryList.itemsSource = _report.Entries;
            _entryList.Rebuild();
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            _summary.text = $"{_targets.Count} targets · {_report.ErrorCount} errors · {_report.WarningCount} warnings";
        }
    }
}
