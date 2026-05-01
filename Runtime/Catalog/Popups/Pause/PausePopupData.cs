using System;

namespace KitforgeLabs.MobileUIKit.Catalog.Pause
{
    [Serializable]
    public class PausePopupData
    {
        public string Title = "Paused";
        public string Subtitle = string.Empty;

        public bool ShowResume = true;
        public bool ShowRestart = false;
        public bool ShowSettings = false;
        public bool ShowHome = false;
        public bool ShowShop = false;
        public bool ShowHelp = false;
        public bool ShowQuit = false;

        public bool ShowSoundToggle = false;
        public bool ShowMusicToggle = false;
        public bool ShowVibrationToggle = false;

        public bool SoundOn = true;
        public bool MusicOn = true;
        public bool VibrationOn = true;

        public bool CloseOnBackdrop = false;
    }
}
