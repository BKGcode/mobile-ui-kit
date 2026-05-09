using System.Collections.Generic;
using System.IO;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class UIKitAuditWindowTabs
    {
        public static void BuildOverview(VisualElement container, UIKitAuditWindow window)
        {
            var grid = new VisualElement();
            grid.AddToClassList("overview-grid");
            AppendOverviewCards(grid, window);
            container.Add(grid);
            AppendOverviewLastFails(container, window);
        }

        private static void AppendOverviewCards(VisualElement grid, UIKitAuditWindow window)
        {
            var run = window.LastRun;
            grid.Add(MakeOverviewCard("Total targets", window.Targets.Count.ToString(), null));
            if (run == null) { grid.Add(MakeOverviewCard("Last run", "—", null)); return; }
            grid.Add(MakeOverviewCard("Pass", run.TotalPass + "/" + run.TotalTargets, run.TotalFail == 0 ? "pass" : "fail"));
            grid.Add(MakeOverviewCard("Catalog", run.CatalogPass + "/" + run.CatalogTotal, run.CatalogPass == run.CatalogTotal ? "pass" : "fail"));
            grid.Add(MakeOverviewCard("Scenes", run.ScenePass + "/" + run.SceneTotal, run.ScenePass == run.SceneTotal ? "pass" : "fail"));
            grid.Add(MakeOverviewCard("Prefabs", run.PrefabPass + "/" + run.PrefabTotal, run.PrefabPass == run.PrefabTotal ? "pass" : "fail"));
            grid.Add(MakeOverviewCard("Elapsed", run.ElapsedMs + " ms", null));
        }

        private static VisualElement MakeOverviewCard(string label, string value, string mood)
        {
            var card = new VisualElement();
            card.AddToClassList("overview-card");
            card.Add(MakeOverviewLabel(label));
            card.Add(MakeOverviewValue(value, mood));
            return card;
        }

        private static Label MakeOverviewLabel(string text)
        {
            var l = new Label(text);
            l.AddToClassList("overview-card-label");
            return l;
        }

        private static Label MakeOverviewValue(string text, string mood)
        {
            var l = new Label(text);
            l.AddToClassList("overview-card-value");
            if (mood == "pass") l.AddToClassList("overview-card-value--pass");
            if (mood == "fail") l.AddToClassList("overview-card-value--fail");
            return l;
        }

        private static void AppendOverviewLastFails(VisualElement container, UIKitAuditWindow window)
        {
            if (window.LastRun == null) return;
            var section = new Label("Recent failures");
            section.AddToClassList("detail-section");
            section.style.marginLeft = 18;
            container.Add(section);
            var run = window.LastRun;
            for (var i = 0; i < run.Targets.Count; i++) AppendIfFail(container, run.Targets[i], window);
        }

        private static void AppendIfFail(VisualElement container, UIKitAuditTargetReport target, UIKitAuditWindow window)
        {
            if (target.Pass) return;
            var row = BuildTargetRow(target, () => RevealTargetReport(target));
            row.style.marginLeft = 18;
            container.Add(row);
        }

        public static void BuildTargetsTab(VisualElement container, UIKitAuditWindow window, UIKitAuditTargetKind kind)
        {
            var detailPane = BuildTargetsDetail();
            var listPane = BuildTargetsList(window, kind, detailPane);
            var split = new TwoPaneSplitView(0, 420f, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            split.Add(listPane);
            split.Add(detailPane);
            container.Add(split);
        }

        private static VisualElement BuildTargetsList(UIKitAuditWindow window, UIKitAuditTargetKind kind, ScrollView detailPane)
        {
            var pane = new VisualElement();
            pane.style.flexGrow = 1;
            pane.Add(MakePaneHeader(kind == UIKitAuditTargetKind.Prefab ? "Prefabs" : "Scenes"));
            var rows = BuildDisplayRows(window, kind);
            var listView = new ListView(rows, 24, MakeRowItem, (e, i) => BindRowItem(e, rows, i));
            listView.selectionType = SelectionType.Single;
            listView.style.flexGrow = 1;
            listView.selectionChanged += selected =>
            {
                foreach (var s in selected) RenderDetailFromRow(detailPane, s as DisplayRow);
            };
            pane.Add(listView);
            return pane;
        }

        private static ScrollView BuildTargetsDetail()
        {
            var pane = new ScrollView(ScrollViewMode.Vertical);
            pane.AddToClassList("detail-root");
            pane.style.flexGrow = 1;
            pane.Add(new Label("Select a row on the left to view the report."));
            return pane;
        }

        private static void RenderDetailFromRow(ScrollView detailPane, DisplayRow row)
        {
            detailPane.Clear();
            if (row == null) { detailPane.Add(new Label("Select a row on the left to view the report.")); return; }
            RenderTargetDetail(detailPane, row);
        }

        private static void RenderTargetDetail(VisualElement detail, DisplayRow row)
        {
            var title = new Label(row.Label);
            title.AddToClassList("detail-title");
            detail.Add(title);
            var meta = new Label($"{row.Kind} · {row.Group} · {row.Status} · {row.Target.AssetPath}");
            meta.AddToClassList("detail-meta");
            detail.Add(meta);
            if (row.Report == null) { detail.Add(new Label("No audit report yet — run 'Run Everything' or 'Run Catalog'.")); return; }
            AppendFindingsSection(detail, row.Report);
            AppendSnapshotSection(detail, row.Report);
            AppendActions(detail, row);
        }

        private static void AppendFindingsSection(VisualElement detail, UIKitAuditTargetReport report)
        {
            var section = new Label("Findings");
            section.AddToClassList("detail-section");
            detail.Add(section);
            if (report.Findings.Count == 0) { detail.Add(new Label("No findings.")); return; }
            for (var i = 0; i < report.Findings.Count; i++) detail.Add(BuildFindingRow(report.Findings[i]));
        }

        private static VisualElement BuildFindingRow(UIKitAuditFinding f)
        {
            var row = new VisualElement();
            row.AddToClassList("detail-finding");
            var pill = new Label(f.Severity.ToString().ToUpperInvariant());
            pill.AddToClassList("pill");
            pill.AddToClassList("pill--" + f.Severity.ToString().ToLowerInvariant());
            row.Add(pill);
            var check = new Label(f.Scope + "/" + f.CheckName);
            check.AddToClassList("check");
            row.Add(check);
            var body = new Label(BuildFindingBody(f));
            body.AddToClassList("body");
            row.Add(body);
            return row;
        }

        private static string BuildFindingBody(UIKitAuditFinding f)
        {
            var body = f.Message;
            if (!string.IsNullOrEmpty(f.ChildPath)) body += $" — path `{f.ChildPath}`";
            if (!string.IsNullOrEmpty(f.ComponentType)) body += $" — expected `{f.ComponentType}`";
            return body;
        }

        private static void AppendSnapshotSection(VisualElement detail, UIKitAuditTargetReport report)
        {
            if (!report.SnapshotCaptured) return;
            var section = new Label("Snapshot");
            section.AddToClassList("detail-section");
            detail.Add(section);
            var image = new VisualElement();
            image.AddToClassList("snapshot-image");
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(report.SnapshotPath);
            if (tex != null) image.style.backgroundImage = new StyleBackground(tex);
            detail.Add(image);
            var meta = new Label($"{report.SnapshotWidth}x{report.SnapshotHeight} · sha256={report.SnapshotSha256?.Substring(0, 8)}…");
            meta.AddToClassList("snapshot-meta");
            detail.Add(meta);
        }

        private static void AppendActions(VisualElement detail, DisplayRow row)
        {
            var actions = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8 } };
            actions.Add(new Button(() => RunOnlyThisTarget(row)) { text = "Audit This Target" });
            actions.Add(new Button(() => PingAsset(row.Target)) { text = "Open Asset" });
            actions.Add(new Button(() => RevealTargetReport(row.Report)) { text = "Reveal Report" });
            detail.Add(actions);
        }

        private static void RunOnlyThisTarget(DisplayRow row)
        {
            var window = EditorWindow.GetWindow<UIKitAuditWindow>();
            window.RunSelected(row.Target);
        }

        private static void PingAsset(UIKitAuditTarget target)
        {
            if (target.PrefabAsset != null) { EditorGUIUtility.PingObject(target.PrefabAsset); return; }
            var asset = AssetDatabase.LoadMainAssetAtPath(target.AssetPath);
            if (asset != null) EditorGUIUtility.PingObject(asset);
        }

        private static void RevealTargetReport(UIKitAuditTargetReport report)
        {
            if (report == null) return;
            var path = Path.Combine(UIKitAuditReportWriter.ReportsRoot, report.Label + ".md");
            if (File.Exists(path)) EditorUtility.RevealInFinder(path);
        }

        private static List<DisplayRow> BuildDisplayRows(UIKitAuditWindow window, UIKitAuditTargetKind kind)
        {
            var rows = new List<DisplayRow>();
            for (var i = 0; i < window.Targets.Count; i++) AppendDisplayRow(rows, window.Targets[i], window, kind);
            rows.Sort(CompareRows);
            return ApplyFilter(rows, window.Filter);
        }

        private static void AppendDisplayRow(List<DisplayRow> rows, UIKitAuditTarget target, UIKitAuditWindow window, UIKitAuditTargetKind kindFilter)
        {
            if (target.Kind != kindFilter) return;
            var report = window.FindReport(target.Label);
            rows.Add(new DisplayRow
            {
                Kind = target.Kind.ToString(),
                Group = target.Group ?? "-",
                Label = target.Label,
                Status = report == null ? "—" : (report.Pass ? "PASS" : "FAIL"),
                Pass = report?.Pass ?? true,
                Badge = report == null ? string.Empty : BuildBadge(report),
                Report = report,
                Target = target
            });
        }

        private static string BuildBadge(UIKitAuditTargetReport report)
        {
            if (report.ErrorCount > 0) return $"{report.ErrorCount} err";
            if (report.WarningCount > 0) return $"{report.WarningCount} warn";
            return string.Empty;
        }

        private static List<DisplayRow> ApplyFilter(List<DisplayRow> rows, string filter)
        {
            if (filter == "All") return rows;
            var filtered = new List<DisplayRow>();
            for (var i = 0; i < rows.Count; i++)
            {
                if (filter == "FAIL" && !rows[i].Pass) filtered.Add(rows[i]);
                if (filter == "PASS" && rows[i].Pass) filtered.Add(rows[i]);
            }
            return filtered;
        }

        private static int CompareRows(DisplayRow a, DisplayRow b)
        {
            if (a.Pass != b.Pass) return a.Pass ? 1 : -1;
            var byGroup = string.Compare(a.Group, b.Group, System.StringComparison.OrdinalIgnoreCase);
            if (byGroup != 0) return byGroup;
            return string.Compare(a.Label, b.Label, System.StringComparison.OrdinalIgnoreCase);
        }

        private static VisualElement MakeRowItem()
        {
            var row = new VisualElement();
            row.AddToClassList("audit-row");
            row.Add(MakeRowLabel("status", "status"));
            row.Add(MakeRowLabel("kind", "kind"));
            row.Add(MakeRowLabel("group", "group"));
            row.Add(MakeRowLabel("label", "label"));
            row.Add(MakeRowLabel("badge", "badge"));
            return row;
        }

        private static Label MakeRowLabel(string name, string klass)
        {
            var l = new Label { name = name };
            l.AddToClassList(klass);
            return l;
        }

        private static void BindRowItem(VisualElement element, List<DisplayRow> rows, int index)
        {
            if (index >= rows.Count) return;
            var data = rows[index];
            var status = element.Q<Label>("status");
            status.text = data.Status;
            status.RemoveFromClassList("status--pass");
            status.RemoveFromClassList("status--fail");
            status.RemoveFromClassList("status--unknown");
            status.AddToClassList(data.Status == "PASS" ? "status--pass" : data.Status == "FAIL" ? "status--fail" : "status--unknown");
            element.Q<Label>("kind").text = data.Kind;
            element.Q<Label>("group").text = data.Group;
            element.Q<Label>("label").text = data.Label;
            element.Q<Label>("badge").text = data.Badge;
            element.userData = data;
        }

        private static VisualElement BuildTargetRow(UIKitAuditTargetReport report, System.Action onClick)
        {
            var row = new VisualElement();
            row.AddToClassList("audit-row");
            var status = new Label("FAIL"); status.AddToClassList("status"); status.AddToClassList("status--fail");
            var label = new Label(report.Label); label.AddToClassList("label");
            var badge = new Label(BuildBadge(report)); badge.AddToClassList("badge");
            row.Add(status); row.Add(label); row.Add(badge);
            row.RegisterCallback<ClickEvent>(_ => onClick());
            return row;
        }

        public static void BuildSnapshotsGrid(VisualElement container, UIKitAuditWindow window)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            var grid = new VisualElement();
            grid.AddToClassList("snapshot-grid");
            for (var i = 0; i < window.Targets.Count; i++) AppendSnapshotCard(grid, window, window.Targets[i]);
            scroll.Add(grid);
            container.Add(scroll);
        }

        private static void AppendSnapshotCard(VisualElement grid, UIKitAuditWindow window, UIKitAuditTarget target)
        {
            if (target.Kind != UIKitAuditTargetKind.Prefab) return;
            var report = window.FindReport(target.Label);
            if (report == null || !report.SnapshotCaptured) return;
            var card = new VisualElement();
            card.AddToClassList("snapshot-card");
            var image = new VisualElement();
            image.AddToClassList("snapshot-card-image");
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(report.SnapshotPath);
            if (tex != null) image.style.backgroundImage = new StyleBackground(tex);
            card.Add(image);
            var label = new Label(target.Label);
            label.AddToClassList("snapshot-card-label");
            card.Add(label);
            grid.Add(card);
        }

        public static void BuildReportsTab(VisualElement container, UIKitAuditWindow window)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.Add(BuildReportsHeader(window));
            scroll.Add(BuildReportsList(window));
            container.Add(scroll);
        }

        private static VisualElement BuildReportsHeader(UIKitAuditWindow window)
        {
            var section = new VisualElement();
            section.style.flexDirection = FlexDirection.Column;
            section.style.paddingTop = 8;
            section.style.paddingBottom = 8;
            section.style.paddingLeft = 8;
            section.style.paddingRight = 8;
            section.Add(new Label($"Reports root: {UIKitAuditReportWriter.ReportsRoot}"));
            section.Add(new Label($"Mirror root: {UIKitAuditReportWriter.MirrorRoot} (active = {window.MirrorReportsToAssets})"));
            section.Add(new Label($"Snapshots root: {PrefabSnapshotCapture.SnapshotFolderRelative}"));
            return section;
        }

        private static VisualElement BuildReportsList(UIKitAuditWindow window)
        {
            var list = new VisualElement();
            list.Add(MakePaneHeader("Generated reports"));
            if (!Directory.Exists(UIKitAuditReportWriter.ReportsRoot)) { list.Add(new Label("No reports yet.")); return list; }
            var files = Directory.GetFiles(UIKitAuditReportWriter.ReportsRoot, "*.md", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < files.Length; i++) list.Add(BuildReportRow(files[i]));
            return list;
        }

        private static VisualElement BuildReportRow(string path)
        {
            var row = new VisualElement();
            row.AddToClassList("reports-row");
            row.Add(new Label(path));
            row.Add(new Button(() => EditorUtility.RevealInFinder(path)) { text = "Reveal" });
            row.Add(new Button(() => EditorUtility.OpenWithDefaultApp(path)) { text = "Open" });
            row.Add(new Button(() => GUIUtility.systemCopyBuffer = path) { text = "Copy Path" });
            return row;
        }

        public static void BuildSettingsTab(VisualElement container, UIKitAuditWindow window)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.Add(MakeSectionLabel("Reports"));
            AppendMirrorToggle(scroll, window);
            scroll.Add(MakeSectionLabel("Snapshots"));
            AppendThemePicker(scroll, window);
            scroll.Add(MakeSectionLabel("Discovery"));
            AppendDiscoverySummary(scroll, window);
            container.Add(scroll);
        }

        private static void AppendMirrorToggle(VisualElement container, UIKitAuditWindow window)
        {
            var row = MakeSettingsRow("Mirror reports to Assets/Editor/QAReports/");
            var toggle = new Toggle { value = window.MirrorReportsToAssets };
            toggle.RegisterValueChangedCallback(evt => window.MirrorReportsToAssets = evt.newValue);
            row.Add(toggle);
            container.Add(row);
            var hint = new Label("ON = versionable + Claude-readable + git-diffable PR-side. OFF = ephemeral in Library/.");
            hint.AddToClassList("snapshot-meta");
            hint.style.marginLeft = 232;
            container.Add(hint);
        }

        private static void AppendThemePicker(VisualElement container, UIKitAuditWindow window)
        {
            var row = MakeSettingsRow("Snapshot theme override");
            var field = new ObjectField { objectType = typeof(UIThemeConfig), value = window.SelectedTheme };
            field.RegisterValueChangedCallback(evt => window.SetTheme(evt.newValue as UIThemeConfig));
            row.Add(field);
            container.Add(row);
            var hint = new Label("Leave empty to auto-resolve the first UIThemeConfig in the project.");
            hint.AddToClassList("snapshot-meta");
            hint.style.marginLeft = 232;
            container.Add(hint);
        }

        private static void AppendDiscoverySummary(VisualElement container, UIKitAuditWindow window)
        {
            var row = MakeSettingsRow("Discovered targets");
            row.Add(new Label(window.Targets.Count.ToString()));
            container.Add(row);
            var rebtn = new Button(window.Discover) { text = "Rediscover" };
            rebtn.style.marginLeft = 12;
            container.Add(rebtn);
        }

        private static VisualElement MakeSettingsRow(string label)
        {
            var row = new VisualElement();
            row.AddToClassList("settings-row");
            row.Add(new Label(label));
            return row;
        }

        private static Label MakeSectionLabel(string text)
        {
            var l = new Label(text);
            l.AddToClassList("settings-section");
            return l;
        }

        private static Label MakePaneHeader(string text)
        {
            var label = new Label(text);
            label.AddToClassList("audit-pane-header");
            return label;
        }

        public sealed class DisplayRow
        {
            public string Kind;
            public string Group;
            public string Label;
            public string Status;
            public bool Pass;
            public string Badge;
            public UIKitAuditTargetReport Report;
            public UIKitAuditTarget Target;
        }
    }
}
