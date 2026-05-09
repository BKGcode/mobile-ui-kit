using System.Collections.Generic;
using System.IO;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class PrefabSpecMarkdownCheck : IUIKitCheck
    {
        private const string SpecRoot = "Packages/com.kitforgelabs.mobile-ui-kit/Documentation~/Specs/Catalog";
        private const string ThemeSection = "## Theme tokens consumed";

        public string Name => "SpecMarkdown";
        public UIKitCheckScope Scope => UIKitCheckScope.Catalog;

        public bool AppliesTo(UIKitAuditTarget target)
        {
            return target.IsCatalog && !string.IsNullOrEmpty(target.TypeShortName);
        }

        public void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            var specPath = ResolveSpecPath(target.TypeShortName);
            if (specPath == null) { ReportNoSpec(report, target.TypeShortName); return; }
            report.SpecCheckRan = true;
            var tokens = ParseThemeTokens(specPath);
            if (tokens.Count == 0) return;
            CrossCheckThemeTokens(tokens, prefabRoot, report);
        }

        private static string ResolveSpecPath(string typeShortName)
        {
            var candidate = Path.Combine(SpecRoot, typeShortName + ".md").Replace('\\', '/');
            return File.Exists(candidate) ? candidate : null;
        }

        private static void ReportNoSpec(UIKitAuditTargetReport report, string typeShortName)
        {
            report.SpecCheckRan = false;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "SpecMarkdown",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Info,
                Message = $"No spec markdown found at {SpecRoot}/{typeShortName}.md — cross-check skipped",
                ChildPath = string.Empty,
                ComponentType = string.Empty
            });
        }

        private static List<string> ParseThemeTokens(string specPath)
        {
            var tokens = new List<string>();
            var lines = File.ReadAllLines(specPath);
            var inSection = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (TryToggleSection(lines[i], ref inSection)) continue;
                if (inSection) ExtractTokensFromLine(lines[i], tokens);
            }
            return tokens;
        }

        private static bool TryToggleSection(string line, ref bool inSection)
        {
            if (line.StartsWith(ThemeSection)) { inSection = true; return true; }
            if (inSection && line.StartsWith("## ")) { inSection = false; return true; }
            return false;
        }

        private static void ExtractTokensFromLine(string line, List<string> tokens)
        {
            var parts = line.Split('`');
            for (var i = 1; i < parts.Length; i += 2) AddCandidateToken(parts[i], tokens);
        }

        private static void AddCandidateToken(string candidate, List<string> tokens)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return;
            if (!IsLikelyColorSlot(candidate)) return;
            if (!tokens.Contains(candidate)) tokens.Add(candidate);
        }

        private static bool IsLikelyColorSlot(string candidate)
        {
            if (candidate.EndsWith("Color")) return true;
            if (candidate.EndsWith("Slot")) return true;
            if (candidate.Contains("Tint")) return true;
            return false;
        }

        private static void CrossCheckThemeTokens(List<string> tokens, GameObject prefabRoot, UIKitAuditTargetReport report)
        {
            var themedImages = prefabRoot.GetComponentsInChildren<ThemedImage>(true).Length;
            var themedTexts = prefabRoot.GetComponentsInChildren<ThemedText>(true).Length;
            if (themedImages == 0 && themedTexts == 0) AddSpecMismatchFinding(report, tokens);
        }

        private static void AddSpecMismatchFinding(UIKitAuditTargetReport report, List<string> tokens)
        {
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "SpecMarkdown",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Warning,
                Message = $"Spec lists Theme tokens ({string.Join(", ", tokens)}) but prefab has no ThemedImage/ThemedText to consume them",
                ChildPath = string.Empty,
                ComponentType = string.Empty
            });
        }
    }
}
