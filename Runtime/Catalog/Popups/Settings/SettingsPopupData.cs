using System;

namespace KitforgeLabs.MobileUIKit.Catalog.Settings
{
    [Serializable]
    public class SettingsPopupData
    {
        public string Title = "Settings";
        public string CloseLabel = "Close";

        public bool ShowMusicSlider = true;
        public bool ShowSfxSlider = true;
        public bool ShowLanguagePicker = true;
        public bool ShowNotificationsToggle = true;
        public bool ShowHapticsToggle = true;

        public LanguageOption[] LanguageOptions = null;
    }

    [Serializable]
    public class LanguageOption
    {
        public string Code;
        public string DisplayName;
    }
}
