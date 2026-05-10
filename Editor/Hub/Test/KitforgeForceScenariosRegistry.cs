using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
using KitforgeLabs.MobileUIKit.Catalog.NotEnough;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Test
{
    public static class KitforgeForceScenariosRegistry
    {
        public static IReadOnlyList<KitforgeForceScenario> All { get; } = BuildAll();

        private static IReadOnlyList<KitforgeForceScenario> BuildAll()
        {
            var list = new List<KitforgeForceScenario>();
            list.Add(BuildDailyLoginDay1());
            list.Add(BuildNotEnoughCoins());
            list.Add(BuildGameOverWithAd());
            list.Add(BuildLevelCompleteNewBest());
            return list;
        }

        private static KitforgeForceScenario BuildDailyLoginDay1()
        {
            return new KitforgeForceScenario(
                "DailyLogin · Day 1 first claim",
                "Spawn DailyLoginPopup as if it's the player's first day.",
                typeof(DailyLoginPopup),
                KitforgeSpawnPattern.Popup,
                ConfigureDailyLoginDay1);
        }

        private static void ConfigureDailyLoginDay1(object data)
        {
            var d = (DailyLoginPopupData)data;
            d.Title = "Daily Reward (Day 1)";
            d.CurrentDay = 1;
            d.AlreadyClaimedToday = false;
            d.LastClaimUtc = DateTime.MinValue;
        }

        private static KitforgeForceScenario BuildNotEnoughCoins()
        {
            return new KitforgeForceScenario(
                "NotEnoughCurrency · Coins (500)",
                "Player can't afford 500 coins.",
                typeof(NotEnoughCurrencyPopup),
                KitforgeSpawnPattern.Popup,
                ConfigureNotEnoughCoins);
        }

        private static void ConfigureNotEnoughCoins(object data)
        {
            var d = (NotEnoughCurrencyPopupData)data;
            d.Currency = CurrencyType.Coins;
            d.Required = 500;
            d.Missing = 500;
            d.Title = "Not enough coins!";
        }

        private static KitforgeForceScenario BuildGameOverWithAd()
        {
            return new KitforgeForceScenario(
                "GameOver · Continue with ad",
                "Game over with rewarded-ad continue option.",
                typeof(GameOverPopup),
                KitforgeSpawnPattern.Popup,
                ConfigureGameOverWithAd);
        }

        private static void ConfigureGameOverWithAd(object data)
        {
            var d = (GameOverPopupData)data;
            d.ContinueMode = ContinueMode.Ad;
            d.Score = 1234;
            d.Title = "Game Over";
            d.Subtitle = "Score: 1234";
        }

        private static KitforgeForceScenario BuildLevelCompleteNewBest()
        {
            return new KitforgeForceScenario(
                "LevelComplete · 3 stars new best",
                "Level cleared with 3 stars, new high score.",
                typeof(LevelCompletePopup),
                KitforgeSpawnPattern.Popup,
                ConfigureLevelCompleteNewBest);
        }

        private static void ConfigureLevelCompleteNewBest(object data)
        {
            var d = (LevelCompletePopupData)data;
            d.Stars = 3;
            d.Score = 12345;
            d.BestScore = 12000;
            d.IsNewBest = true;
            d.Title = "Level Complete!";
        }
    }
}
