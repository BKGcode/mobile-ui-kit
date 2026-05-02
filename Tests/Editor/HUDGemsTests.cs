using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class HUDGemsTests
    {
        private GameObject _hudGo;
        private GameObject _servicesGo;
        private HUDGems _hud;
        private FakeEconomyService _economy;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            var services = _servicesGo.AddComponent<UIServices>();
            _economy = new FakeEconomyService();
            services.SetEconomy(_economy);

            _hudGo = new GameObject("HUDGems_Test");
            _hudGo.SetActive(false);
            _hud = _hudGo.AddComponent<HUDGems>();
            _hud.SetServicesForTests(services);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hudGo != null) UnityEngine.Object.DestroyImmediate(_hudGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        [Test]
        public void Refresh_With_Service_Reads_Initial_Gems_Value()
        {
            _economy.SetGems(7);
            _hud.ForceRefreshForTests();
            Assert.AreEqual(7, _hud.LastValueForTests);
            Assert.AreEqual("7", _hud.FormattedTextForTests);
        }

        [Test]
        public void HandleGemsChanged_Updates_Last_Value_On_AddGems()
        {
            _economy.SetGems(0);
            _hud.ForceRefreshForTests();
            _economy.AddGems(10);
            Assert.AreEqual(10, _hud.LastValueForTests);
        }

        [Test]
        public void Coins_Changed_Does_NOT_Update_Gems_HUD()
        {
            _economy.SetGems(5);
            _hud.ForceRefreshForTests();
            _economy.AddCoins(1000);
            Assert.AreEqual(5, _hud.LastValueForTests, "HUD-Gems must NOT react to coin changes.");
        }

        [Test]
        public void Refresh_With_Null_Service_Logs_Error_And_Does_Not_Throw()
        {
            _hud.SetServicesForTests(null);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("IEconomyService not available"));
            Assert.DoesNotThrow(() => _hud.ForceRefreshForTests());
        }

    }
}
