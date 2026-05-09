using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    internal static class HierarchyPath
    {
        public static string Of(Transform t)
        {
            if (t == null) return string.Empty;
            if (t.parent == null) return t.name;
            return Of(t.parent) + "/" + t.name;
        }
    }
}
