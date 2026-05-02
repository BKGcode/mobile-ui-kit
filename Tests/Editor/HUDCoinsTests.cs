using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class HUDCoinsTests
    {
        private GameObject _hudGo;
        private GameObject _servicesGo;
        private HUDCoins _hud;
        private FakeEconomyService _economy;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            var services = _servicesGo.AddComponent<UIServices>();
            _economy = new FakeEconomyService();
            services.SetEconomy(_economy);

            _hudGo = new GameObject("HUDCoins_Test");
            _hudGo.SetActive(false);
            _hud = _hudGo.AddComponent<HUDCoins>();
            _hud.SetServicesForTests(services);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hudGo != null) UnityEngine.Object.DestroyImmediate(_hudGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        [Test]
        public void Refresh_With_Service_Reads_Initial_Value_Without_Subscribe_Side_Effects()
        {
            _economy.SetCoins(100);
            _hud.ForceRefreshForTests();
            Assert.AreEqual(100, _hud.LastValueForTests);
            Assert.AreEqual("100", _hud.FormattedTextForTests);
        }

        [Test]
        public void HandleCoinsChanged_Updates_Last_Value()
        {
            _economy.SetCoins(0);
            _hud.ForceRefreshForTests();
            _economy.AddCoins(50);
            Assert.AreEqual(50, _hud.LastValueForTests);
        }

        [Test]
        public void Default_Format_Uses_Thousand_Separators_For_Large_Values()
        {
            _economy.SetCoins(0);
            _hud.ForceRefreshForTests();
            _economy.AddCoins(999_100);
            Assert.AreEqual(999_100, _hud.LastValueForTests);
            Assert.AreEqual(999_100.ToString("N0"), _hud.FormattedTextForTests);
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
