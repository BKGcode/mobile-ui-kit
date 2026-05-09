using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class UIKitAuditHeadless
    {
        [MenuItem("Tools/Kitforge/UI Kit/Audit (Run Headless)")]
        public static void RunFromMenu()
        {
            var exit = RunHeadless();
            Debug.Log($"[UIKitAudit] Headless exit code = {exit}");
        }

        public static int RunHeadless()
        {
            UIKitAuditRunReport run = null;
            try
            {
                var options = new UIKitAuditOptions
                {
                    Dimensions = UIKitAuditDimensions.All,
                    Trigger = UIKitAuditTrigger.Headless,
                    MirrorReportsToAssets = true,
                    Scope = "All"
                };
                run = UIKitAuditService.AuditAll(options);
                UIKitAuditReportWriter.Write(run, options.MirrorReportsToAssets);
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIKitAudit] Headless run threw {e.GetType().Name}: {e.Message}");
                return 2;
            }
            LogResult(run);
            return run.TotalFail == 0 ? 0 : 1;
        }

        private static void LogResult(UIKitAuditRunReport run)
        {
            Debug.Log($"[UIKitAudit] Headless · {run.TotalPass}/{run.TotalTargets} pass · {run.TotalFail} fail · Catalog {run.CatalogPass}/{run.CatalogTotal} · Scenes {run.ScenePass}/{run.SceneTotal} · See {UIKitAuditReportWriter.ReportsRoot}/_Summary.md");
        }
    }
}
