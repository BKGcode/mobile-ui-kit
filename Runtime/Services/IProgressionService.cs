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

        event Action<LevelData> OnLevelCompleted;
        event Action<int> OnLevelUnlocked;
    }
}
