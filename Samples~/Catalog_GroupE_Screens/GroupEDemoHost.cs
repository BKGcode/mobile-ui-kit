using System.Collections;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Screens;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupE
{
    public sealed class GroupEDemoHost : MonoBehaviour
    {
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PopupManager _popupManager;
        [SerializeField] private UIServices _services;

        private MainMenuScreen _mainMenu;

        private void Start() => Boot();

        [ContextMenu("Boot Demo — Loading to MainMenu to DailyLogin")]
        private void Boot()
        {
            var data = new LoadingScreenData { Title = "Loading...", MinDisplaySeconds = 1.5f };
            var loading = _uiManager.Push<LoadingScreen>(data);
            if (loading == null) { ShowMainMenu(); return; }
            loading.OnProgressComplete += ShowMainMenu;
            StartCoroutine(SimulateLoading(loading));
        }

        private IEnumerator SimulateLoading(LoadingScreen loading)
        {
            for (var i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.15f);
                loading.SetProgress((i + 1) / 10f);
            }
        }

        [ContextMenu("Show MainMenu — Skip Loading")]
        private void ShowMainMenu()
        {
            _mainMenu = _uiManager.Replace<MainMenuScreen>(new MainMenuScreenData { Title = "My Game" });
            if (_mainMenu == null) return;
            _mainMenu.OnPlayRequested += OnPlay;
            _mainMenu.OnSettingsRequested += OnSettings;
            _mainMenu.OnShopRequested += OnShop;
            _mainMenu.OnDailyRequested += OnDailyTapped;
            _mainMenu.OnBackRequested += OnBack;
            _mainMenu.OnShown += TryAutoTriggerDailyLogin;
        }

        private void TryAutoTriggerDailyLogin()
        {
            DailyLoginFlow.ShowIfDue(_popupManager, _services, BuildDailyLoginTemplate(),
                (day, _) => Debug.Log($"[GroupEDemoHost] Daily day {day} claimed."));
        }

        [ContextMenu("Trigger DailyLogin — Force Spawn")]
        private void TriggerDailyLoginForce()
        {
            var popup = _popupManager.Show<DailyLoginPopup>(BuildDailyLoginTemplate());
            if (popup == null) { Debug.LogWarning("[GroupEDemoHost] DailyLoginPopup not registered in PopupManager._popupPrefabs."); return; }
            popup.OnDayClaimed += (day, _) => Debug.Log($"[GroupEDemoHost] Daily day {day} claimed.");
        }

        private static DailyLoginPopupData BuildDailyLoginTemplate()
        {
            return new DailyLoginPopupData
            {
                Title = "Daily Reward",
                CloseOnBackdrop = true,
                RewardEntries = new[]
                {
                    MakeEntry("Day 1", RewardKind.Coins, 50),
                    MakeEntry("Day 2", RewardKind.Coins, 100),
                    MakeEntry("Day 3", RewardKind.Gems, 3),
                    MakeEntry("Day 4", RewardKind.Coins, 200),
                    MakeEntry("Day 5", RewardKind.Gems, 5, allowDouble: true),
                    MakeEntry("Day 6", RewardKind.Coins, 500),
                    MakeEntry("Day 7", RewardKind.Gems, 10, isBigReward: true),
                },
            };
        }

        private static DailyLoginRewardEntry MakeEntry(string title, RewardKind kind, int amount, bool allowDouble = false, bool isBigReward = false)
        {
            return new DailyLoginRewardEntry
            {
                Rewards = new[] { new RewardPopupData { Title = title, Kind = kind, Amount = amount } },
                AllowDouble = allowDouble,
                IsBigReward = isBigReward,
            };
        }

        private void OnPlay() => Debug.Log("[GroupEDemoHost] Play → push GameScreen. In production: UIManager.Push<GameScreen>() or load level scene.");
        private void OnSettings() => Debug.Log("[GroupEDemoHost] Settings → PopupManager.Show<SettingsPopup>(). Register SettingsPopup.prefab from Group D in PopupManager._popupPrefabs.");
        private void OnShop() => Debug.Log("[GroupEDemoHost] Shop → PopupManager.Show<ShopPopup>(). Register ShopPopup.prefab from Group B in PopupManager._popupPrefabs.");
        private void OnDailyTapped() => TriggerDailyLoginForce();
        private void OnBack() => Debug.Log("[GroupEDemoHost] Back pressed. In production: wire Application.Quit() or PopupManager.Show<ConfirmPopup>().");
    }
}
