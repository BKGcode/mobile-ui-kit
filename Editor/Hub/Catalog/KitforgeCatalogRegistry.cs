using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.Confirm;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
using KitforgeLabs.MobileUIKit.Catalog.NotEnough;
using KitforgeLabs.MobileUIKit.Catalog.Pause;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Screens;
using KitforgeLabs.MobileUIKit.Catalog.Settings;
using KitforgeLabs.MobileUIKit.Catalog.Shop;
using KitforgeLabs.MobileUIKit.Catalog.Toasts;
using KitforgeLabs.MobileUIKit.Catalog.Tutorial;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Catalog
{
    public static class KitforgeCatalogRegistry
    {
        public static IReadOnlyList<KitforgeCatalogEntry> All { get; } = BuildAll();

        private static IReadOnlyList<KitforgeCatalogEntry> BuildAll()
        {
            var list = new List<KitforgeCatalogEntry>();
            AppendGroupA(list);
            AppendGroupB(list);
            AppendGroupC(list);
            AppendGroupD(list);
            AppendGroupE(list);
            return list;
        }

        private static void AppendGroupA(List<KitforgeCatalogEntry> list)
        {
            list.Add(new KitforgeCatalogEntry(typeof(ConfirmPopup), "Confirm", "A", KitforgeSpawnPattern.Popup, "Yes/No question. Collapses to single-button alert when ShowCancel=false."));
            list.Add(new KitforgeCatalogEntry(typeof(PausePopup), "Pause", "A", KitforgeSpawnPattern.Popup, "In-game pause overlay. Owns Time.timeScale while visible."));
            list.Add(new KitforgeCatalogEntry(typeof(TutorialPopup), "Tutorial", "A", KitforgeSpawnPattern.Popup, "Multi-step onboarding with Next/Previous/Skip."));
            list.Add(new KitforgeCatalogEntry(typeof(NotificationToast), "Notification Toast", "A", KitforgeSpawnPattern.Toast, "Auto-dismiss banner. Severity-tinted (Info/Success/Warning/Error)."));
        }

        private static void AppendGroupB(List<KitforgeCatalogEntry> list)
        {
            list.Add(new KitforgeCatalogEntry(typeof(RewardPopup), "Reward", "B", KitforgeSpawnPattern.Popup, "Player gets coins/gems/item/bundle. Auto-claim option."));
            list.Add(new KitforgeCatalogEntry(typeof(ShopPopup), "Shop", "B", KitforgeSpawnPattern.Popup, "Grid of items + buy buttons. Category filter."));
            list.Add(new KitforgeCatalogEntry(typeof(NotEnoughCurrencyPopup), "Not Enough Currency", "B", KitforgeSpawnPattern.Popup, "Player can't afford. Offers Buy more / Watch ad."));
            list.Add(new KitforgeCatalogEntry(typeof(HUDCurrency), "HUDCoins", "B", KitforgeSpawnPattern.HUD, "Coin counter. Binds to IEconomyService.OnChanged with punch tween on update."));
            list.Add(new KitforgeCatalogEntry(typeof(HUDCurrency), "HUDGems", "B", KitforgeSpawnPattern.HUD, "Gem counter. Binds to IEconomyService.OnChanged with punch tween on update."));
        }

        private static void AppendGroupC(List<KitforgeCatalogEntry> list)
        {
            list.Add(new KitforgeCatalogEntry(typeof(DailyLoginPopup), "Daily Login", "C", KitforgeSpawnPattern.Popup, "7-day calendar reward. Auto-trigger via DailyLoginFlow.ShowIfDue."));
            list.Add(new KitforgeCatalogEntry(typeof(LevelCompletePopup), "Level Complete", "C", KitforgeSpawnPattern.Popup, "End-of-level victory. Stars + score + rewards array."));
            list.Add(new KitforgeCatalogEntry(typeof(GameOverPopup), "Game Over", "C", KitforgeSpawnPattern.Popup, "Player died. Continue (ad/currency) / Restart / Quit."));
            list.Add(new KitforgeCatalogEntry(typeof(HUDEnergy), "HUD Energy", "C", KitforgeSpawnPattern.HUD, "Energy bar + regen countdown + cap label. 1Hz IProgressionService poll."));
            list.Add(new KitforgeCatalogEntry(typeof(HUDTimer), "HUD Timer", "C", KitforgeSpawnPattern.HUD, "Live timer (3 modes: CountdownToTarget / CountupSinceTarget / LocalStopwatch)."));
        }

        private static void AppendGroupD(List<KitforgeCatalogEntry> list)
        {
            list.Add(new KitforgeCatalogEntry(typeof(SettingsPopup), "Settings", "D", KitforgeSpawnPattern.Popup, "Audio / language / notifications / haptics. Persists via IPlayerDataService."));
        }

        private static void AppendGroupE(List<KitforgeCatalogEntry> list)
        {
            list.Add(new KitforgeCatalogEntry(typeof(LoadingScreen), "Loading Screen", "E", KitforgeSpawnPattern.Screen, "Initial boot / async load. Optional progress bar + spinner."));
            list.Add(new KitforgeCatalogEntry(typeof(MainMenuScreen), "Main Menu Screen", "E", KitforgeSpawnPattern.Screen, "Hub after Loading. Play / Settings / Shop / Daily buttons."));
        }
    }
}
