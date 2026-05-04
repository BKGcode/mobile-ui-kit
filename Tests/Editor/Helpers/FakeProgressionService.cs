using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeProgressionService : IProgressionService
    {
        private readonly List<LevelData> _levels = new List<LevelData>();
        private int _currentLevelIndex;
        private DailyLoginState _dailyLogin;
        private EnergyRegenState _energyRegen;

        public event Action<LevelData> OnLevelCompleted;
        public event Action<int> OnLevelUnlocked;

        public int GetCurrentLevelIndex() => _currentLevelIndex;
        public LevelData GetLevelData(int levelId) => default;
        public IReadOnlyList<LevelData> GetAllLevels() => _levels;
        public void CompleteLevel(int levelId, int starsEarned) => OnLevelCompleted?.Invoke(default);
        public bool UnlockLevel(int levelId) { OnLevelUnlocked?.Invoke(levelId); return true; }

        public DailyLoginState GetDailyLoginState() => _dailyLogin;
        public EnergyRegenState GetEnergyRegenState() => _energyRegen;

        public void SetCurrentLevelIndex(int index) => _currentLevelIndex = index;
        public void SetDailyLoginState(DailyLoginState state) => _dailyLogin = state;
        public void SetEnergyRegenState(EnergyRegenState state) => _energyRegen = state;
    }
}
