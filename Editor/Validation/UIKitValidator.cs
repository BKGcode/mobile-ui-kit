using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Toast;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KitforgeLabs.MobileUIKit.Editor.Validation
{
    public static class UIKitValidator
    {
        [MenuItem("Kitforge/UI Kit/Validate Active Scene")]
        public static void ValidateActiveSceneMenu()
        {
            var report = ValidateActiveScene();
            Debug.Log(report.Format());
        }

        public static ValidationReport ValidateActiveScene()
        {
            var report = new ValidationReport();
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                report.AddError("No active scene to validate.");
                return report;
            }
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                Inspect(roots[i], report);
            }
            return report;
        }

        private static void Inspect(GameObject root, ValidationReport report)
        {
            ValidateManagers(root.GetComponentsInChildren<UIManager>(true), report);
            ValidatePopupManagers(root.GetComponentsInChildren<PopupManager>(true), report);
            ValidateToastManagers(root.GetComponentsInChildren<ToastManager>(true), report);
        }

        private static void ValidateManagers(UIManager[] managers, ValidationReport report)
        {
            for (var i = 0; i < managers.Length; i++)
            {
                var so = new SerializedObject(managers[i]);
                CheckNotNull(so, "_themeConfig", managers[i], report);
                CheckNotNull(so, "_screenRoot", managers[i], report);
                CheckArrayUniqueTypes(so, "_screenPrefabs", managers[i], report);
            }
        }

        private static void ValidatePopupManagers(PopupManager[] managers, ValidationReport report)
        {
            for (var i = 0; i < managers.Length; i++)
            {
                var so = new SerializedObject(managers[i]);
                CheckNotNull(so, "_themeConfig", managers[i], report);
                CheckNotNull(so, "_popupRoot", managers[i], report);
                CheckArrayUniqueTypes(so, "_popupPrefabs", managers[i], report);
            }
        }

        private static void ValidateToastManagers(ToastManager[] managers, ValidationReport report)
        {
            for (var i = 0; i < managers.Length; i++)
            {
                var so = new SerializedObject(managers[i]);
                CheckNotNull(so, "_themeConfig", managers[i], report);
                CheckNotNull(so, "_toastRoot", managers[i], report);
                CheckArrayUniqueTypes(so, "_toastPrefabs", managers[i], report);
            }
        }

        private static void CheckNotNull(SerializedObject so, string field, Object owner, ValidationReport report)
        {
            var prop = so.FindProperty(field);
            if (prop == null) return;
            if (prop.objectReferenceValue == null)
            {
                report.AddError($"{owner.GetType().Name} '{owner.name}': field '{field}' is null.");
            }
        }

        private static void CheckArrayUniqueTypes(SerializedObject so, string field, Object owner, ValidationReport report)
        {
            var prop = so.FindProperty(field);
            if (prop == null || !prop.isArray) return;
            var seen = new HashSet<System.Type>();
            for (var i = 0; i < prop.arraySize; i++)
            {
                var element = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element == null)
                {
                    report.AddWarning($"{owner.GetType().Name} '{owner.name}': '{field}' slot {i} is null.");
                    continue;
                }
                var type = element.GetType();
                if (!seen.Add(type))
                {
                    report.AddError($"{owner.GetType().Name} '{owner.name}': '{field}' has duplicate type {type.Name} at slot {i}.");
                }
            }
        }
    }

    public class ValidationReport
    {
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        public int ErrorCount => _errors.Count;
        public int WarningCount => _warnings.Count;
        public bool HasErrors => _errors.Count > 0;

        public void AddError(string message) => _errors.Add(message);
        public void AddWarning(string message) => _warnings.Add(message);

        public string Format()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[UIKitValidator] Errors: {_errors.Count} | Warnings: {_warnings.Count}");
            for (var i = 0; i < _errors.Count; i++) sb.AppendLine($"  ERROR: {_errors[i]}");
            for (var i = 0; i < _warnings.Count; i++) sb.AppendLine($"  WARN : {_warnings[i]}");
            return sb.ToString();
        }
    }

    public sealed class UIKitBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 100;

        public void OnPreprocessBuild(BuildReport report)
        {
            var validation = UIKitValidator.ValidateActiveScene();
            if (validation.HasErrors)
            {
                Debug.LogError(validation.Format());
                throw new BuildFailedException($"[UIKitValidator] Build aborted: {validation.ErrorCount} validation errors in active scene.");
            }
            if (validation.WarningCount > 0) Debug.LogWarning(validation.Format());
        }
    }
}
