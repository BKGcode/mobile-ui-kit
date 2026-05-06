using System;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Screens
{
    [Serializable]
    public class MainMenuScreenData
    {
        [Tooltip("Game title text. Empty = TitleLabel hidden.")]
        public string Title = "";
        [Tooltip("Show the Play button.")]
        public bool ShowPlayButton = true;
        [Tooltip("Show the Settings button.")]
        public bool ShowSettingsButton = true;
        [Tooltip("Show the Shop button.")]
        public bool ShowShopButton = true;
        [Tooltip("Show the DailyLogin button. Also hides the daily indicator dot when false.")]
        public bool ShowDailyButton = true;
    }
}
