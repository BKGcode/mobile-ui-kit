using System;
using System.Collections.Generic;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    [Serializable]
    public sealed class UIKitAuditTargetReport
    {
        public UIKitAuditTargetKind Kind;
        public string Label;
        public string AssetPath;
        public string Group;
        public string TypeShortName;
        public string TypeFullName;
        public bool IsCatalog;
        public List<UIKitAuditFinding> Findings = new();

        public bool RefsCheckRan;
        public bool ThemeCheckRan;
        public bool SpecCheckRan;
        public int ThemedImageCount;
        public int ThemedTextCount;
        public bool ThemeReactivityPass;

        public bool SnapshotCaptured;
        public string SnapshotPath;
        public string SnapshotSha256;
        public int SnapshotWidth;
        public int SnapshotHeight;

        public long ElapsedMs;

        public int ErrorCount => CountBySeverity(UIKitAuditSeverity.Error);
        public int WarningCount => CountBySeverity(UIKitAuditSeverity.Warning);
        public bool Pass => ErrorCount == 0;

        private int CountBySeverity(UIKitAuditSeverity severity)
        {
            var n = 0;
            for (var i = 0; i < Findings.Count; i++) if (Findings[i].Severity == severity) n++;
            return n;
        }
    }

    [Serializable]
    public sealed class UIKitAuditRunReport
    {
        public string GeneratedAtUtc;
        public string UnityVersion;
        public string GitCommit;
        public string Trigger;
        public string Scope;
        public long ElapsedMs;
        public List<UIKitAuditTargetReport> Targets = new();

        public int TotalTargets => Targets.Count;
        public int TotalPass => CountTargets(true);
        public int TotalFail => Targets.Count - TotalPass;
        public int CatalogTotal => CountCatalog(false);
        public int CatalogPass => CountCatalog(true);
        public int SceneTotal => CountKind(UIKitAuditTargetKind.Scene, false);
        public int ScenePass => CountKind(UIKitAuditTargetKind.Scene, true);
        public int PrefabTotal => CountKind(UIKitAuditTargetKind.Prefab, false);
        public int PrefabPass => CountKind(UIKitAuditTargetKind.Prefab, true);

        private int CountTargets(bool passOnly)
        {
            var n = 0;
            for (var i = 0; i < Targets.Count; i++) if (!passOnly || Targets[i].Pass) n++;
            return n;
        }

        private int CountCatalog(bool passOnly)
        {
            var n = 0;
            for (var i = 0; i < Targets.Count; i++)
            {
                if (!Targets[i].IsCatalog) continue;
                if (passOnly && !Targets[i].Pass) continue;
                n++;
            }
            return n;
        }

        private int CountKind(UIKitAuditTargetKind kind, bool passOnly)
        {
            var n = 0;
            for (var i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].Kind != kind) continue;
                if (passOnly && !Targets[i].Pass) continue;
                n++;
            }
            return n;
        }
    }
}
