using System;
using KitforgeLabs.MobileUIKit.Theme;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    [Flags]
    public enum UIKitAuditDimensions
    {
        None = 0,
        Catalog = 1 << 0,
        PrefabStructural = 1 << 1,
        Scenes = 1 << 2,
        Visual = 1 << 3,
        All = Catalog | PrefabStructural | Scenes | Visual
    }

    public sealed class UIKitAuditOptions
    {
        public UIKitAuditDimensions Dimensions = UIKitAuditDimensions.All;
        public UIKitAuditTrigger Trigger = UIKitAuditTrigger.Manual;
        public UIThemeConfig SnapshotTheme;
        public bool MirrorReportsToAssets;
        public string Scope = "All";
        public Action<int, int, string> Progress;
    }
}
