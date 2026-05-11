using System;
using KitforgeLabs.UIKit.Catalog.Reward;

namespace KitforgeLabs.UIKit.Catalog.DailyLogin
{
    [Serializable]
    public class DailyLoginRewardEntry
    {
        public RewardPopupData[] Rewards;
        public bool IsBigReward;
        public bool AllowDouble;
        public string Label = string.Empty;
    }
}
