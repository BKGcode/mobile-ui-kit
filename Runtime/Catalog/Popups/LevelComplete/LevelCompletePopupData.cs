using System;
using KitforgeLabs.UIKit.Catalog.Reward;

namespace KitforgeLabs.UIKit.Catalog.LevelComplete
{
    [Serializable]
    public class LevelCompletePopupData
    {
        public string Title = "Level Complete!";
        public string LevelLabel = "";
        public int Stars;
        public int Score;
        public int BestScore;
        public bool IsNewBest;
        public RewardPopupData[] Rewards;
        public string NextLabel = "Next";
        public string RetryLabel = "Retry";
        public string MainMenuLabel = "Main Menu";
        public bool ShowNext = true;
        public bool ShowRetry = true;
        public bool ShowMainMenu;
        public bool CloseOnBackdrop;
    }
}
