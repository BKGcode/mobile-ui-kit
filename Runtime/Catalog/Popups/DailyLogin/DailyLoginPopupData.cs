using System;

namespace KitforgeLabs.MobileUIKit.Catalog.DailyLogin
{
    [Serializable]
    public class DailyLoginPopupData
    {
        public string Title = "Daily Reward";
        public DailyLoginRewardEntry[] RewardEntries;
        public int CurrentDay = 1;
        public DateTime LastClaimUtc;
        public bool AlreadyClaimedToday;
        public bool DoubledToday;
        public int MaxStreakGapDays = 1;
        public string ClaimLabel = "Claim";
        public string WatchToDoubleLabel = "Watch ad to double";
        public bool CloseOnBackdrop;
    }
}
