using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public interface IQAIntegrityCheck
    {
        string Name { get; }
        bool SupportsScene { get; }
        bool SupportsPrefab { get; }
        void RunOnScene(string scenePath, QASuiteReport report);
        void RunOnPrefab(GameObject prefabRoot, string prefabPath, QASuiteReport report);
    }
}
