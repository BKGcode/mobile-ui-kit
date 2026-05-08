using System.Collections.Generic;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public enum QASeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class QAEntry
    {
        public string TargetPath;
        public string TargetLabel;
        public string CheckName;
        public QASeverity Severity;
        public string Message;
        public string OwnerHierarchy;
        public Object OwnerAsset;
    }

    public sealed class QASuiteReport
    {
        public List<QAEntry> Entries { get; } = new();
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }

        public void Add(QAEntry entry)
        {
            Entries.Add(entry);
            if (entry.Severity == QASeverity.Error) ErrorCount++;
            else if (entry.Severity == QASeverity.Warning) WarningCount++;
        }

        public void Clear()
        {
            Entries.Clear();
            ErrorCount = 0;
            WarningCount = 0;
        }
    }

    internal static class QAHierarchy
    {
        public static string Of(Transform t)
        {
            if (t == null) return string.Empty;
            if (t.parent == null) return t.name;
            return Of(t.parent) + "/" + t.name;
        }
    }
}
