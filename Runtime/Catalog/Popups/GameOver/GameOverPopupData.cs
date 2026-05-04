using System;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.GameOver
{
    [Serializable]
    public class GameOverPopupData
    {
        public string Title = "Game Over";
        public string Subtitle = "";
        public int Score = -1;
        public ContinueMode ContinueMode = ContinueMode.Ad;
        public CurrencyType ContinueCurrency = CurrencyType.Gems;
        public int ContinueAmount = 5;
        public int ContinuesUsedThisSession;
        public int MaxContinuesPerSession = 1;
        public string ContinueAdLabel = "Continue";
        public string ContinueCurrencyLabel = "Continue ({amount})";
        public string RestartLabel = "Restart";
        public string MainMenuLabel = "Main Menu";
        public bool ShowRestart = true;
        public bool ShowMainMenu = true;
        public BackPressBehavior BackPressBehavior = BackPressBehavior.Restart;
        public bool CloseOnBackdrop;
    }
}
