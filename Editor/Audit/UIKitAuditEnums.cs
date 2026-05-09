namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public enum UIKitAuditTargetKind
    {
        Scene,
        Prefab
    }

    public enum UIKitCheckScope
    {
        Scene,
        Prefab,
        Catalog,
        Visual
    }

    public enum UIKitAuditSeverity
    {
        Info,
        Warning,
        Error
    }

    public enum UIKitAuditTrigger
    {
        Manual,
        Headless,
        BuilderPostpass
    }
}
