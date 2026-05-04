using System;
using System.Collections.Generic;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IProgressionService
    {
        int GetCurrentLevelIndex();
        LevelData GetLevelData(int levelId);
        IReadOnlyList<LevelData> GetAllLevels();
        void CompleteLevel(int levelId, int starsEarned);
        bool UnlockLevel(int levelId);

        DailyLoginState GetDailyLoginState();
        EnergyRegenState GetEnergyRegenState();

        event Action<LevelData> OnLevelCompleted;
        event Action<int> OnLevelUnlocked;
    }

    [Serializable]
    public struct DailyLoginState
    {
        public int CurrentDay;
        public DateTime LastClaimUtc;
        public bool AlreadyClaimedToday;
        public bool DoubledToday;
        public int MaxStreakGapDays;
    }

    [Serializable]
    public struct EnergyRegenState
    {
        public int Current;
        public int Max;
        public DateTime NextRegenUtc;
        public bool IsFull;
    }
}
