using System;
using KitforgeLabs.MobileUIKit.Catalog.Reward;

namespace KitforgeLabs.MobileUIKit.Catalog.DailyLogin
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
