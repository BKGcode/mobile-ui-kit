using System;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UIKitAuditIgnoreAttribute : Attribute
    {
        public string Reason { get; }

        public UIKitAuditIgnoreAttribute(string reason = null)
        {
            Reason = reason;
        }
    }
}
