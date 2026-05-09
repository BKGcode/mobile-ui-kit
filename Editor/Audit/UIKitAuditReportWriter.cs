using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class UIKitAuditReportWriter
    {
        public const string ReportsRootRelative = "Library/UIKitAudit/Reports";
        public const string MirrorRootRelative = "Assets/Editor/QAReports";
        public const string SnapshotRootRelative = "Assets/Editor/QASnapshots";

        public static string ReportsRoot => Path.Combine(GetProjectRoot(), ReportsRootRelative).Replace('\\', '/');
        public static string MirrorRoot => MirrorRootRelative;

        public static string Write(UIKitAuditRunReport run, bool mirrorToAssets)
        {
            EnsureRoot(ReportsRoot);
            for (var i = 0; i < run.Targets.Count; i++) WriteTargetReport(run.Targets[i], ReportsRoot);
            var summaryPath = Path.Combine(ReportsRoot, "_Summary.md").Replace('\\', '/');
            File.WriteAllText(summaryPath, BuildSummary(run), Encoding.UTF8);
            File.WriteAllText(Path.Combine(ReportsRoot, "_Run.json").Replace('\\', '/'), JsonUtility.ToJson(run, true), Encoding.UTF8);
            if (mirrorToAssets) MirrorToAssets(run);
            return summaryPath;
        }

        public static void Clear()
        {
            if (Directory.Exists(ReportsRoot)) Directory.Delete(ReportsRoot, true);
            EnsureRoot(ReportsRoot);
            ClearMirror();
        }

        public static void ClearMirror()
        {
            if (!AssetDatabase.IsValidFolder(MirrorRootRelative)) return;
            AssetDatabase.DeleteAsset(MirrorRootRelative);
            AssetDatabase.Refresh();
        }

        private static void MirrorToAssets(UIKitAuditRunReport run)
        {
            EnsureAssetFolderRecursive(MirrorRootRelative);
            var absRoot = MirrorRootRelative;
            for (var i = 0; i < run.Targets.Count; i++) WriteTargetReport(run.Targets[i], absRoot);
            File.WriteAllText(Path.Combine(absRoot, "_Summary.md"), BuildSummary(run), Encoding.UTF8);
            File.WriteAllText(Path.Combine(absRoot, "_Run.json"), JsonUtility.ToJson(run, true), Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        private static void EnsureRoot(string root)
        {
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        }

        private static string GetProjectRoot()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }

        private static void WriteTargetReport(UIKitAuditTargetReport target, string root)
        {
            var stem = Path.Combine(root, target.Label).Replace('\\', '/');
            File.WriteAllText(stem + ".md", BuildTargetMarkdown(target), Encoding.UTF8);
            File.WriteAllText(stem + ".json", JsonUtility.ToJson(target, true), Encoding.UTF8);
        }

        private static string BuildTargetMarkdown(UIKitAuditTargetReport t)
        {
            var sb = new StringBuilder();
            AppendTargetHeader(sb, t);
            AppendTargetChecks(sb, t);
            AppendTargetFindings(sb, t);
            AppendTargetSnapshot(sb, t);
            return sb.ToString();
        }

        private static void AppendTargetHeader(StringBuilder sb, UIKitAuditTargetReport t)
        {
            sb.Append("# ").AppendLine(t.Label);
            sb.AppendLine();
            sb.Append("**Status:** ").AppendLine(t.Pass ? "PASS" : "FAIL");
            sb.Append("**Kind:** ").AppendLine(t.Kind.ToString());
            sb.Append("**Group:** ").AppendLine(t.Group ?? "-");
            sb.Append("**Catalog:** ").AppendLine(t.IsCatalog ? "yes" : "no");
            if (!string.IsNullOrEmpty(t.TypeFullName)) sb.Append("**Type:** ").AppendLine(t.TypeFullName);
            sb.Append("**Asset:** `").Append(t.AssetPath).AppendLine("`");
            sb.AppendFormat(CultureInfo.InvariantCulture, "**Elapsed:** {0} ms\n", t.ElapsedMs);
            sb.AppendLine();
        }

        private static void AppendTargetChecks(StringBuilder sb, UIKitAuditTargetReport t)
        {
            if (!t.IsCatalog) return;
            sb.AppendLine("## Catalog checks");
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Refs contract: {0}\n", FmtRan(t.RefsCheckRan));
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Theme reactivity: {0} (ThemedImage={1}, ThemedText={2}, pass={3})\n",
                FmtRan(t.ThemeCheckRan), t.ThemedImageCount, t.ThemedTextCount, t.ThemeReactivityPass);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Spec markdown cross-check: {0}\n", FmtRan(t.SpecCheckRan));
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Snapshot: {0}\n", t.SnapshotCaptured ? "captured" : "skipped");
            sb.AppendLine();
        }

        private static void AppendTargetFindings(StringBuilder sb, UIKitAuditTargetReport t)
        {
            sb.AppendLine("## Findings");
            if (t.Findings.Count == 0) { sb.AppendLine("_No findings._"); sb.AppendLine(); return; }
            for (var i = 0; i < t.Findings.Count; i++) AppendFinding(sb, t.Findings[i]);
            sb.AppendLine();
        }

        private static void AppendFinding(StringBuilder sb, UIKitAuditFinding f)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "- **[{0}]** ({1}/{2}) {3}", f.Severity, f.Scope, f.CheckName, f.Message);
            if (!string.IsNullOrEmpty(f.ChildPath)) sb.AppendFormat(CultureInfo.InvariantCulture, " — path `{0}`", f.ChildPath);
            if (!string.IsNullOrEmpty(f.ComponentType)) sb.AppendFormat(CultureInfo.InvariantCulture, " — expected `{0}`", f.ComponentType);
            sb.AppendLine();
        }

        private static void AppendTargetSnapshot(StringBuilder sb, UIKitAuditTargetReport t)
        {
            if (!t.SnapshotCaptured) return;
            sb.AppendLine("## Snapshot");
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Path: `{0}`\n", t.SnapshotPath);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Resolution: {0}x{1}\n", t.SnapshotWidth, t.SnapshotHeight);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- SHA256: `{0}`\n", t.SnapshotSha256);
            sb.AppendLine();
        }

        private static string FmtRan(bool ran)
        {
            return ran ? "ran" : "SKIPPED";
        }

        private static string BuildSummary(UIKitAuditRunReport run)
        {
            var sb = new StringBuilder();
            AppendSummaryHeader(sb, run);
            AppendDimensionBreakdown(sb, run);
            AppendSummaryTable(sb, run);
            return sb.ToString();
        }

        private static void AppendSummaryHeader(StringBuilder sb, UIKitAuditRunReport run)
        {
            sb.AppendLine("# UI Kit Audit — Summary");
            sb.AppendLine();
            sb.Append("- Generated: ").AppendLine(run.GeneratedAtUtc);
            sb.Append("- Unity: ").AppendLine(run.UnityVersion);
            sb.Append("- Git: ").AppendLine(run.GitCommit);
            sb.Append("- Trigger: ").AppendLine(run.Trigger);
            sb.Append("- Scope: ").AppendLine(run.Scope);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Elapsed: {0} ms\n", run.ElapsedMs);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Result: {0}/{1} pass · {2} fail\n", run.TotalPass, run.TotalTargets, run.TotalFail);
            sb.AppendLine();
        }

        private static void AppendDimensionBreakdown(StringBuilder sb, UIKitAuditRunReport run)
        {
            sb.AppendLine("## Breakdown");
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Catalog: {0}/{1}\n", run.CatalogPass, run.CatalogTotal);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Scenes: {0}/{1}\n", run.ScenePass, run.SceneTotal);
            sb.AppendFormat(CultureInfo.InvariantCulture, "- Prefabs: {0}/{1}\n", run.PrefabPass, run.PrefabTotal);
            sb.AppendLine();
        }

        private static void AppendSummaryTable(StringBuilder sb, UIKitAuditRunReport run)
        {
            sb.AppendLine("| Status | Kind | Group | Target | Errors | Warnings | Themed | Snapshot | ms |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
            for (var i = 0; i < run.Targets.Count; i++) AppendSummaryRow(sb, run.Targets[i]);
        }

        private static void AppendSummaryRow(StringBuilder sb, UIKitAuditTargetReport t)
        {
            var themed = t.ThemedImageCount + t.ThemedTextCount;
            sb.AppendFormat(CultureInfo.InvariantCulture, "| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} |\n",
                t.Pass ? "PASS" : "FAIL", t.Kind, t.Group ?? "-", t.Label, t.ErrorCount, t.WarningCount, themed,
                t.SnapshotCaptured ? "yes" : "no", t.ElapsedMs);
        }

        private static void EnsureAssetFolderRecursive(string path)
        {
            var parts = path.Split('/');
            var built = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = built + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(built, parts[i]);
                built = next;
            }
        }
    }
}
