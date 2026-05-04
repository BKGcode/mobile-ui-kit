using System;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupC
{
    public sealed class CatalogGroupCDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform _popupParent;
        [SerializeField] private UIThemeConfig _theme;
        [SerializeField] private UIServices _services;

        [Header("Catalog prefabs (assigned by Build Group C Sample). Drop a prefab whose root has the corresponding catalog component.")]
        [SerializeField] private GameObject _dailyLoginPrefab;
        [SerializeField] private GameObject _levelCompletePrefab;
        [SerializeField] private GameObject _gameOverPrefab;

        [Header("Reward prefab from Group B (required for LevelComplete → Reward sequence + DailyLogin claim chains).")]
        [SerializeField] private GameObject _rewardPrefab;

        [Header("HUD scene instances (live in scene, wired by builder).")]
        [SerializeField] private HUDEnergy _hudEnergy;
        [SerializeField] private HUDTimer _hudTimer;

        private void Start()
        {
            if (_hudTimer != null) _hudTimer.SetTarget(DateTime.UtcNow.AddMinutes(5));
        }

        [ContextMenu("DailyLogin — Show day 1 fresh streak")]
        private void ShowDailyLoginDay1() => SpawnDailyLogin(BuildDailyLoginConfig(currentDay: 1, alreadyClaimed: false));

        [ContextMenu("DailyLogin — Show day 5 (allow watch-to-double)")]
        private void ShowDailyLoginDay5() => SpawnDailyLogin(BuildDailyLoginConfig(currentDay: 5, alreadyClaimed: false));

        [ContextMenu("DailyLogin — Show already claimed (read-only countdown)")]
        private void ShowDailyLoginClaimed() => SpawnDailyLogin(BuildDailyLoginConfig(currentDay: 3, alreadyClaimed: true));

        [ContextMenu("LevelComplete — 3 stars + new best (no rewards)")]
        private void ShowLevelCompleteWin() => SpawnLevelComplete(new LevelCompletePopupData
        {
            Title = "Level Complete!",
            LevelLabel = "Level 7",
            Stars = 3,
            Score = 12500,
            BestScore = 10000,
            IsNewBest = true
        });

        [ContextMenu("LevelComplete — 1 star + last level (Next hidden)")]
        private void ShowLevelCompleteLastLevel() => SpawnLevelComplete(new LevelCompletePopupData
        {
            Title = "Campaign Complete!",
            LevelLabel = "Level 30",
            Stars = 1,
            Score = 4200,
            BestScore = 4200,
            IsNewBest = false,
            ShowNext = false,
            ShowMainMenu = true
        });

        [ContextMenu("GameOver — Continue Ad")]
        private void ShowGameOverAd() => SpawnGameOver(new GameOverPopupData
        {
            Title = "Game Over",
            Score = 7500,
            ContinueMode = ContinueMode.Ad
        });

        [ContextMenu("GameOver — Continue Currency (5 gems)")]
        private void ShowGameOverCurrency() => SpawnGameOver(new GameOverPopupData
        {
            Title = "Game Over",
            Score = 7500,
            ContinueMode = ContinueMode.Currency,
            ContinueCurrency = CurrencyType.Gems,
            ContinueAmount = 5
        });

        [ContextMenu("GameOver — AdOrCurrency (both visible)")]
        private void ShowGameOverBoth() => SpawnGameOver(new GameOverPopupData
        {
            Title = "Game Over",
            Score = 7500,
            ContinueMode = ContinueMode.AdOrCurrency,
            ContinueCurrency = CurrencyType.Gems,
            ContinueAmount = 5
        });

        [ContextMenu("HUD — Add 1 energy")]
        private void DebugAddEnergy() => _services?.Economy?.Add(CurrencyType.Energy, 1);

        [ContextMenu("HUD — Spend 1 energy")]
        private void DebugSpendEnergy() => _services?.Economy?.Spend(CurrencyType.Energy, 1);

        [ContextMenu("HUD — Set energy to 0 (empty state)")]
        private void DebugEmptyEnergy()
        {
            var current = _services?.Economy?.Get(CurrencyType.Energy) ?? 0;
            if (current > 0) _services?.Economy?.Spend(CurrencyType.Energy, current);
        }

        [ContextMenu("HUD Timer — Set 5 min countdown")]
        private void DebugSetTimer5Min() => SetTimerTarget(DateTime.UtcNow.AddMinutes(5));

        [ContextMenu("HUD Timer — Set 10s countdown (warning zone)")]
        private void DebugSetTimer10s() => SetTimerTarget(DateTime.UtcNow.AddSeconds(10));

        [ContextMenu("HUD Timer — Set 3min countdown (multi-timer requires extra prefab instances)")]
        private void DebugSetTimer3Min()
        {
            SetTimerTarget(DateTime.UtcNow.AddMinutes(3));
            Debug.Log("[CatalogGroupCDemo] HUDTimer set to 3 min. For multi-timer test (QA scenario 11): instantiate additional HUDTimer prefabs in scene with distinct _mode/_targetUtc; each ticks independently.");
        }

        [ContextMenu("Chain — LevelComplete → Reward sequence (3 rewards)")]
        private void ChainLevelCompleteRewards() => SpawnLevelComplete(new LevelCompletePopupData
        {
            Title = "Level Complete!",
            Stars = 3,
            Score = 9500,
            BestScore = 8000,
            IsNewBest = true,
            Rewards = new[]
            {
                new RewardPopupData { Title = "Stage Reward", Kind = RewardKind.Coins, Amount = 100 },
                new RewardPopupData { Title = "Combo Bonus", Kind = RewardKind.Gems, Amount = 3 },
                new RewardPopupData { Title = "Daily Mission", Kind = RewardKind.Coins, Amount = 50 }
            }
        });

        [ContextMenu("Chain — GameOver Continue Ad → reward")]
        private void ChainGameOverAd() => SpawnGameOver(new GameOverPopupData
        {
            Title = "Game Over",
            Score = 4200,
            ContinueMode = ContinueMode.Ad
        });

        [ContextMenu("Chain — GameOver Continue Currency (deduct gems)")]
        private void ChainGameOverCurrency() => SpawnGameOver(new GameOverPopupData
        {
            Title = "Game Over",
            Score = 4200,
            ContinueMode = ContinueMode.Currency,
            ContinueCurrency = CurrencyType.Gems,
            ContinueAmount = 5
        });

        [ContextMenu("Chain — DailyLogin auto-trigger (Day 1)")]
        private void ChainDailyLoginAutoTrigger() => SpawnDailyLogin(BuildDailyLoginConfig(currentDay: 1, alreadyClaimed: false));

        [ContextMenu("Chain — Energy regen tick (manual +1)")]
        private void ChainEnergyRegen()
        {
            _services?.Economy?.Add(CurrencyType.Energy, 1);
            Debug.Log("[CatalogGroupCDemo] Manually added +1 energy. HUDEnergy reflects via OnChanged subscription. InMemoryProgressionService also auto-ticks every 60s.");
        }

        private DailyLoginPopupData BuildDailyLoginConfig(int currentDay, bool alreadyClaimed)
        {
            return new DailyLoginPopupData
            {
                Title = "Daily Reward",
                CurrentDay = currentDay,
                AlreadyClaimedToday = alreadyClaimed,
                LastClaimUtc = alreadyClaimed ? DateTime.UtcNow : default,
                RewardEntries = new[]
                {
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 1", Kind = RewardKind.Coins, Amount = 50 } } },
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 2", Kind = RewardKind.Coins, Amount = 100 } } },
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 3", Kind = RewardKind.Gems, Amount = 3 } } },
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 4", Kind = RewardKind.Coins, Amount = 200 } } },
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 5", Kind = RewardKind.Gems, Amount = 5 } }, AllowDouble = true },
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 6", Kind = RewardKind.Coins, Amount = 500 } } },
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Title = "Day 7", Kind = RewardKind.Gems, Amount = 10 } }, IsBigReward = true }
                }
            };
        }

        private void SpawnDailyLogin(DailyLoginPopupData data)
        {
            var instance = SpawnPopup<DailyLoginPopup>(_dailyLoginPrefab, "DailyLoginPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnDayClaimed += (day, rewards) =>
            {
                Debug.Log($"[CatalogGroupCDemo] DailyLogin Day {day} claimed. Rewards={rewards.Length}");
                SpawnRewardSequence(rewards);
            };
            instance.OnWatchAdRequested += (day, rewards) =>
            {
                Debug.Log($"[CatalogGroupCDemo] DailyLogin Day {day} watch-to-double requested.");
                ShowAdThenDoubledRewards(rewards);
            };
            instance.OnDismissed += () => Destroy(instance.gameObject);
            instance.OnShow();
        }

        private void SpawnLevelComplete(LevelCompletePopupData data)
        {
            var instance = SpawnPopup<LevelCompletePopup>(_levelCompletePrefab, "LevelCompletePopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnNextRequested += d =>
            {
                Debug.Log($"[CatalogGroupCDemo] LevelComplete Next → reward sequence ({d.Rewards?.Length ?? 0} rewards)");
                SpawnRewardSequence(d.Rewards);
            };
            instance.OnRetryRequested += _ => Debug.Log("[CatalogGroupCDemo] LevelComplete Retry");
            instance.OnMainMenuRequested += _ => Debug.Log("[CatalogGroupCDemo] LevelComplete MainMenu");
            instance.OnDismissed += () => Destroy(instance.gameObject);
            instance.OnShow();
        }

        private void SpawnGameOver(GameOverPopupData data)
        {
            var instance = SpawnPopup<GameOverPopup>(_gameOverPrefab, "GameOverPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnContinueWithAdRequested += () =>
            {
                Debug.Log("[CatalogGroupCDemo] GameOver Continue Ad → ShowRewardedAd");
                _services?.Ads?.ShowRewardedAd(success => Debug.Log($"[CatalogGroupCDemo] Ad success={success}. In production: resume game on success."));
            };
            instance.OnContinueWithCurrencyRequested += (currency, amount) =>
            {
                if (_services?.Economy?.Spend(currency, amount) == true)
                    Debug.Log($"[CatalogGroupCDemo] GameOver Continue Currency: spent {amount} {currency}. Resume game.");
            };
            instance.OnContinueAffordCheckFailed += (currency, amount) =>
                Debug.Log($"[CatalogGroupCDemo] GameOver affordability fail: needs {amount} {currency}. In production: open Shop or NotEnough popup.");
            instance.OnRestartRequested += () => Debug.Log("[CatalogGroupCDemo] GameOver Restart");
            instance.OnMainMenuRequested += () => Debug.Log("[CatalogGroupCDemo] GameOver MainMenu");
            instance.OnDismissed += () => Destroy(instance.gameObject);
            instance.OnShow();
        }

        private void SpawnRewardSequence(RewardPopupData[] rewards)
        {
            if (rewards == null || rewards.Length == 0) return;
            SpawnNextRewardInSequence(rewards, 0);
        }

        private void SpawnNextRewardInSequence(RewardPopupData[] rewards, int index)
        {
            if (index >= rewards.Length) { Debug.Log("[CatalogGroupCDemo] Reward sequence complete."); return; }
            var instance = SpawnPopup<RewardPopup>(_rewardPrefab, "RewardPopup");
            if (instance == null) return;
            instance.Bind(rewards[index]);
            instance.OnClaimed += HandleRewardClaimed;
            instance.OnDismissed += () =>
            {
                Destroy(instance.gameObject);
                SpawnNextRewardInSequence(rewards, index + 1);
            };
            instance.OnShow();
        }

        private void ShowAdThenDoubledRewards(RewardPopupData[] rewards)
        {
            var ads = _services?.Ads;
            if (ads == null) { Debug.LogWarning("[CatalogGroupCDemo] No IAdsService — cannot fulfil watch-to-double."); return; }
            ads.ShowRewardedAd(success =>
            {
                if (!success) { Debug.Log("[CatalogGroupCDemo] Ad cancelled — no double reward."); return; }
                if (rewards == null || rewards.Length == 0) return;
                var doubled = new RewardPopupData
                {
                    Title = $"{rewards[0].Title} (×2)",
                    Kind = rewards[0].Kind,
                    Amount = rewards[0].Amount * 2
                };
                SpawnRewardSequence(new[] { doubled });
            });
        }

        private void HandleRewardClaimed(CurrencyType currency, int amount)
        {
            if (_services?.Economy == null) return;
            if ((int)currency == RewardPopup.ItemCurrencySentinel) return;
            _services.Economy.Add(currency, amount);
        }

        private void SetTimerTarget(DateTime utc)
        {
            if (_hudTimer == null) { Debug.LogWarning("[CatalogGroupCDemo] _hudTimer not assigned. Run 'Tools/Kitforge/UI Kit/Build Group C Sample'."); return; }
            _hudTimer.SetTarget(utc);
        }

        private T SpawnPopup<T>(GameObject prefab, string popupName) where T : UIModuleBase
        {
            if (prefab == null) { Debug.LogError($"[CatalogGroupCDemo] {popupName} prefab not assigned. Run 'Tools/Kitforge/UI Kit/Build Group C Sample' (and 'Build Group B Sample' for RewardPopup).", this); return null; }
            var go = Instantiate(prefab, _popupParent, false);
            var instance = go.GetComponent<T>();
            if (instance == null) { Debug.LogError($"[CatalogGroupCDemo] Spawned prefab is missing the {popupName} component on its root.", this); Destroy(go); return null; }
            instance.Initialize(_theme, _services);
            return instance;
        }
    }
}
