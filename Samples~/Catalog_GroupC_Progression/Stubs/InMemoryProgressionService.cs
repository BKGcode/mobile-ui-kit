using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupC
{
    /// <summary>
    /// In-memory <see cref="IProgressionService"/> sample stub. Inspector-editable daily login
    /// and energy regen state, level list seeded with 5 levels, automatic regen tick driven by
    /// <c>Update()</c> using <see cref="DateTime.UtcNow"/>. On regen tick calls
    /// <c>_economy.Add(CurrencyType.Energy, 1)</c> via the injected economy reference so HUD
    /// elements that subscribe to <see cref="IEconomyService.OnChanged"/> stay in sync without
    /// extra wiring. Defaults: day 1, never claimed (LastClaimUtc = MinValue so DailyLogin
    /// auto-trigger fires on first Play), energy = max (IsFull = true). <c>[DefaultExecutionOrder(-100)]</c>
    /// guarantees Awake runs before consumer HUDs read state in their OnEnable.
    /// <para>
    /// Daily login persistence (M2 retro-fit 2026-05-04): if <c>_playerDataServiceRef</c> is
    /// wired, daily login state hydrates from <see cref="IPlayerDataService"/> on Awake and
    /// persists on every <see cref="MarkDailyLoginClaimed"/> / <see cref="MarkDailyLoginDoubled"/>.
    /// If null, falls back to in-memory backing (cross-Play state lost — original behavior).
    /// Streak preservation: real demo for buyers evaluating the kit.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class InMemoryProgressionService : MonoBehaviour, IProgressionService
    {
        [Tooltip("MonoBehaviour implementing IEconomyService. Used for the regen tick to call Add(Energy, 1). Wire UIServices' Economy ref or any IEconomyService impl.")]
        [SerializeField] private MonoBehaviour _economyServiceRef;

        [Tooltip("Optional MonoBehaviour implementing IPlayerDataService. If wired, daily login state persists across sessions. If null, in-memory only (original v0.6.0-alpha behavior).")]
        [SerializeField] private MonoBehaviour _playerDataServiceRef;

        [Tooltip("Seconds between energy regen ticks. Default 60s.")]
        [SerializeField] private float _regenSeconds = 60f;

        [SerializeField] private DailyLoginState _dailyLoginState = new DailyLoginState
        {
            CurrentDay = 1,
            LastClaimUtc = default,
            AlreadyClaimedToday = false,
            DoubledToday = false,
            MaxStreakGapDays = 1,
        };

        [SerializeField] private EnergyRegenState _energyRegenState = new EnergyRegenState
        {
            Current = 5,
            Max = 5,
            NextRegenUtc = default,
            IsFull = true,
        };

        [SerializeField] private int _currentLevelIndex;
        [SerializeField] private List<LevelData> _levels = new List<LevelData>();

        public event Action<LevelData> OnLevelCompleted;
        public event Action<int> OnLevelUnlocked;

        private IEconomyService _economy;
        private IPlayerDataService _playerData;

        public void SetEconomyForTests(IEconomyService economy) => _economy = economy;
        public void SetPlayerDataForTests(IPlayerDataService playerData) => _playerData = playerData;

        private void Awake()
        {
            if (_economy == null) _economy = _economyServiceRef as IEconomyService;
            if (_playerData == null) _playerData = _playerDataServiceRef as IPlayerDataService;
            if (_levels.Count == 0) SeedLevels();
            DailyLoginPersistence.Load(_playerData, ref _dailyLoginState);
        }

        private void Update()
        {
            TickRegen();
        }

        private void TickRegen()
        {
            if (_energyRegenState.IsFull) return;
            if (DateTime.UtcNow < _energyRegenState.NextRegenUtc) return;
            _energyRegenState.Current = Mathf.Min(_energyRegenState.Current + 1, _energyRegenState.Max);
            _energyRegenState.IsFull = _energyRegenState.Current >= _energyRegenState.Max;
            _energyRegenState.NextRegenUtc = _energyRegenState.IsFull ? default : DateTime.UtcNow.AddSeconds(_regenSeconds);
            _economy?.Add(CurrencyType.Energy, 1);
        }

        public DailyLoginState GetDailyLoginState()
        {
            var state = _dailyLoginState;
            state.AlreadyClaimedToday = state.LastClaimUtc.Date == DateTime.UtcNow.Date;
            return state;
        }

        public EnergyRegenState GetEnergyRegenState() => _energyRegenState;

        public int GetCurrentLevelIndex() => _currentLevelIndex;

        public LevelData GetLevelData(int levelId)
        {
            for (var i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].Id == levelId) return _levels[i];
            }
            return default;
        }

        public IReadOnlyList<LevelData> GetAllLevels() => _levels;

        public void CompleteLevel(int levelId, int starsEarned)
        {
            for (var i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].Id != levelId) continue;
                var data = _levels[i];
                data.State = LevelState.Complete;
                data.Stars = Mathf.Clamp(starsEarned, 0, 3);
                _levels[i] = data;
                OnLevelCompleted?.Invoke(data);
                return;
            }
        }

        public bool UnlockLevel(int levelId)
        {
            for (var i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].Id != levelId) continue;
                var data = _levels[i];
                if (data.State != LevelState.Locked) return false;
                data.State = LevelState.Available;
                _levels[i] = data;
                OnLevelUnlocked?.Invoke(levelId);
                return true;
            }
            return false;
        }

        public void MarkDailyLoginClaimed()
        {
            _dailyLoginState.LastClaimUtc = DateTime.UtcNow;
            _dailyLoginState.CurrentDay++;
            _dailyLoginState.DoubledToday = false;
            DailyLoginPersistence.Save(_playerData, _dailyLoginState);
        }

        public void MarkDailyLoginDoubled()
        {
            _dailyLoginState.DoubledToday = true;
            DailyLoginPersistence.Save(_playerData, _dailyLoginState);
        }

        public void SetEnergyValue(int value)
        {
            var clamped = Mathf.Clamp(value, 0, _energyRegenState.Max);
            _energyRegenState.Current = clamped;
            _energyRegenState.IsFull = clamped >= _energyRegenState.Max;
            _energyRegenState.NextRegenUtc = _energyRegenState.IsFull ? default : DateTime.UtcNow.AddSeconds(_regenSeconds);
        }

        private void SeedLevels()
        {
            for (var i = 0; i < 5; i++)
            {
                _levels.Add(new LevelData { Id = i, State = i == 0 ? LevelState.Available : LevelState.Locked, Stars = 0, UnlockCost = 0 });
            }
        }

        [ContextMenu("Debug — Reset Daily To Day 1")]
        private void DebugResetDaily()
        {
            _dailyLoginState.CurrentDay = 1;
            _dailyLoginState.LastClaimUtc = default;
            _dailyLoginState.DoubledToday = false;
        }

        [ContextMenu("Debug — Set Daily To Day 7 Ready")]
        private void DebugDailyDay7Ready()
        {
            _dailyLoginState.CurrentDay = 7;
            _dailyLoginState.LastClaimUtc = default;
            _dailyLoginState.DoubledToday = false;
        }

        [ContextMenu("Debug — Mark Today Claimed")]
        private void DebugMarkClaimed() => MarkDailyLoginClaimed();

        [ContextMenu("Debug — Set Energy To Zero")]
        private void DebugZeroEnergy() => SetEnergyValue(0);

        [ContextMenu("Debug — Refill Energy")]
        private void DebugRefillEnergy() => SetEnergyValue(_energyRegenState.Max);
    }
}
