using System;

namespace KitforgeLabs.MobileUIKit.Catalog.Toasts
{
    [Serializable]
    public class NotificationToastData
    {
        public string Message = string.Empty;
        public ToastSeverity Severity = ToastSeverity.Info;
        public float DurationOverride = -1f;
        public bool TapToDismiss = true;
    }
}
