using System.IO;
using KitforgeLabs.MobileUIKit.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public sealed class UIKitManagerCheck : IQAIntegrityCheck
    {
        public string Name => "UIKitManager";
        public bool SupportsScene => true;
        public bool SupportsPrefab => false;

        public void RunOnScene(string scenePath, QASuiteReport report)
        {
            var validation = UIKitValidator.ValidateActiveScene();
            var sceneAsset = AssetDatabase.LoadMainAssetAtPath(scenePath);
            for (var i = 0; i < validation.Errors.Count; i++) AddEntry(validation.Errors[i], scenePath, sceneAsset, QASeverity.Error, report);
            for (var i = 0; i < validation.Warnings.Count; i++) AddEntry(validation.Warnings[i], scenePath, sceneAsset, QASeverity.Warning, report);
        }

        public void RunOnPrefab(GameObject prefabRoot, string prefabPath, QASuiteReport report) { }

        private static void AddEntry(string message, string scenePath, Object asset, QASeverity severity, QASuiteReport report)
        {
            report.Add(new QAEntry
            {
                TargetPath = scenePath,
                TargetLabel = Path.GetFileNameWithoutExtension(scenePath),
                CheckName = "UIKitManager",
                Severity = severity,
                Message = message,
                OwnerHierarchy = string.Empty,
                OwnerAsset = asset
            });
        }
    }
}
