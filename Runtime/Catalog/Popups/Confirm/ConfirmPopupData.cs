using System;

namespace KitforgeLabs.MobileUIKit.Catalog.Confirm
{
    [Serializable]
    public class ConfirmPopupData
    {
        public string Title = string.Empty;
        public string Message = string.Empty;
        public string ConfirmLabel = "OK";
        public string CancelLabel = "Cancel";
        public ConfirmTone Tone = ConfirmTone.Neutral;
        public bool ShowCancel = true;
        public bool CloseOnBackdrop = false;
    }
}
