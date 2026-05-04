using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class HUDCurrencyTests
    {
        private GameObject _hudGo;
        private GameObject _servicesGo;
        private UIServices _services;
        private FakeEconomyService _economy;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            _services = _servicesGo.AddComponent<UIServices>();
            _economy = new FakeEconomyService();
            _services.SetEconomy(_economy);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hudGo != null) Object.DestroyImmediate(_hudGo);
            if (_servicesGo != null) Object.DestroyImmediate(_servicesGo);
        }

        private HUDCurrency MakeHud(CurrencyType currency)
        {
            _hudGo = new GameObject($"HUDCurrency_Test_{currency}");
            _hudGo.SetActive(false);
            var hud = _hudGo.AddComponent<HUDCurrency>();
            hud.SetCurrencyForTests(currency);
            hud.SetServicesForTests(_services);
            return hud;
        }

        [TestCase(CurrencyType.Coins)]
        [TestCase(CurrencyType.Gems)]
        public void Refresh_With_Service_Reads_Initial_Value_Without_Subscribe_Side_Effects(CurrencyType currency)
        {
            var hud = MakeHud(currency);
            _economy.SetCurrency(currency, 100);
            hud.ForceRefreshForTests();
            Assert.AreEqual(100, hud.LastValueForTests);
            Assert.AreEqual("100", hud.FormattedTextForTests);
        }

        [TestCase(CurrencyType.Coins)]
        [TestCase(CurrencyType.Gems)]
        public void HandleEconomyChanged_Updates_Last_Value(CurrencyType currency)
        {
            var hud = MakeHud(currency);
            _economy.SetCurrency(currency, 0);
            hud.ForceRefreshForTests();
            _economy.Add(currency, 50);
            Assert.AreEqual(50, hud.LastValueForTests);
        }

        [TestCase(CurrencyType.Coins)]
        [TestCase(CurrencyType.Gems)]
        public void Default_Format_Uses_Thousand_Separators_For_Large_Values(CurrencyType currency)
        {
            var hud = MakeHud(currency);
            _economy.SetCurrency(currency, 0);
            hud.ForceRefreshForTests();
            _economy.Add(currency, 999_100);
            Assert.AreEqual(999_100, hud.LastValueForTests);
            Assert.AreEqual(999_100.ToString("N0"), hud.FormattedTextForTests);
        }

        [Test]
        public void Refresh_With_Null_Service_Logs_Error_And_Does_Not_Throw()
        {
            var hud = MakeHud(CurrencyType.Coins);
            hud.SetServicesForTests(null);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("IEconomyService not available"));
            Assert.DoesNotThrow(() => hud.ForceRefreshForTests());
        }

        [Test]
        public void OnChanged_With_Foreign_Currency_Does_Not_Trigger_ApplyValue()
        {
            var hud = MakeHud(CurrencyType.Coins);
            _economy.SetCurrency(CurrencyType.Coins, 100);
            hud.ForceRefreshForTests();
            var beforeValue = hud.LastValueForTests;
            _economy.Add(CurrencyType.Gems, 50);
            Assert.AreEqual(beforeValue, hud.LastValueForTests, "HUDCurrency for Coins must NOT react to OnChanged(Gems) — currency-filter contract (FD4).");
        }
    }
}
