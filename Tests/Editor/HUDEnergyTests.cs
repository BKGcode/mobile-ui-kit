using System;
using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class HUDEnergyTests
    {
        private GameObject _hudGo;
        private GameObject _servicesGo;
        private UIServices _services;
        private FakeEconomyService _economy;
        private FakeProgressionService _progression;
        private FakeTimeService _time;
        private TMP_Text _regenLabel;
        private TMP_Text _capLabel;
        private Image _barFill;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            _services = _servicesGo.AddComponent<UIServices>();
            _economy = new FakeEconomyService();
            _progression = new FakeProgressionService();
            _time = new FakeTimeService();
            _services.SetEconomy(_economy);
            _services.SetProgression(_progression);
            _services.SetTime(_time);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hudGo != null) UnityEngine.Object.DestroyImmediate(_hudGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        private HUDEnergy MakeHud()
        {
            _hudGo = new GameObject("HUDEnergy_Test");
            _hudGo.SetActive(false);

            var regenGo = new GameObject("Regen");
            regenGo.transform.SetParent(_hudGo.transform);
            _regenLabel = regenGo.AddComponent<TextMeshProUGUI>();

            var capGo = new GameObject("Cap");
            capGo.transform.SetParent(_hudGo.transform);
            _capLabel = capGo.AddComponent<TextMeshProUGUI>();

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(_hudGo.transform);
            _barFill = fillGo.AddComponent<Image>();
            _barFill.type = Image.Type.Filled;

            var hud = _hudGo.AddComponent<HUDEnergy>();
            hud.SetServicesForTests(_services);
            hud.SetEnergyRefsForTests(_regenLabel, _capLabel, _barFill);
            return hud;
        }

        private static EnergyRegenState State(int current, int max, DateTime nextRegenUtc, bool isFull)
        {
            return new EnergyRegenState
            {
                Current = current,
                Max = max,
                NextRegenUtc = nextRegenUtc,
                IsFull = isFull,
            };
        }

        [Test]
        public void Refresh_With_Service_Reads_Initial_Energy_State()
        {
            var hud = MakeHud();
            _economy.SetCurrency(CurrencyType.Energy, 3);
            _progression.SetEnergyRegenState(State(3, 5, _time.GetServerTimeUtc().AddSeconds(120), false));
            hud.ForceRefreshForTests();
            Assert.AreEqual(3, hud.LastEnergyValueForTests);
            Assert.AreEqual("/5", hud.CapLabelTextForTests);
            Assert.IsTrue(hud.RegenLabelVisibleForTests);
            Assert.AreEqual(3f / 5f, hud.BarFillAmountForTests, 0.001f);
        }

        [Test]
        public void Refresh_With_Null_ProgressionService_Hides_Energy_UI_Silently()
        {
            var hud = MakeHud();
            _services.SetProgression(null);
            _economy.SetCurrency(CurrencyType.Energy, 3);
            hud.ForceRefreshForTests();
            Assert.IsFalse(hud.CapLabelVisibleForTests, "Cap label must be hidden when Progression is null (silent degrade per § 4.5).");
            Assert.IsFalse(hud.RegenLabelVisibleForTests, "Regen label must be hidden when Progression is null.");
            Assert.IsFalse(hud.BarFillVisibleForTests, "Bar fill must be hidden when Progression is null.");
        }

        [Test]
        public void Cap_Label_Shows_Suffix_When_Bounded()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(2, 7, _time.GetServerTimeUtc().AddSeconds(60), false));
            hud.ForceRefreshForTests();
            Assert.AreEqual("/7", hud.CapLabelTextForTests);
            Assert.IsTrue(hud.CapLabelVisibleForTests);
        }

        [Test]
        public void Cap_Label_Hides_When_IsFull()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(5, 5, DateTime.MinValue, true));
            hud.ForceRefreshForTests();
            Assert.IsFalse(hud.CapLabelVisibleForTests, "Cap label must be hidden when IsFull (E4 — no slash when full).");
        }

        [Test]
        public void Cap_Label_Hides_When_Max_Is_Zero()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(0, 0, DateTime.MinValue, false));
            hud.ForceRefreshForTests();
            Assert.IsFalse(hud.CapLabelVisibleForTests, "Cap label must be hidden when Max <= 0 (E4).");
        }

        [Test]
        public void Regen_Label_Shows_Ready_Text_When_NextRegen_In_Past()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(2, 5, _time.GetServerTimeUtc().AddSeconds(-30), false));
            hud.ForceRefreshForTests();
            Assert.AreEqual("+1 ready", hud.RegenLabelTextForTests);
            Assert.IsTrue(hud.RegenLabelVisibleForTests);
        }

        [Test]
        public void Regen_Label_Hides_When_IsFull()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(5, 5, DateTime.MinValue, true));
            hud.ForceRefreshForTests();
            Assert.IsFalse(hud.RegenLabelVisibleForTests, "Regen label must be hidden when energy is full (E5).");
        }

        [Test]
        public void EnergyBarFill_Sets_FillAmount_From_Current_Max()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(3, 5, _time.GetServerTimeUtc().AddSeconds(60), false));
            hud.ForceRefreshForTests();
            Assert.AreEqual(0.6f, hud.BarFillAmountForTests, 0.001f);
            Assert.IsTrue(hud.BarFillVisibleForTests);
        }

        [Test]
        public void Sealed_To_Energy_Even_If_Inspector_Currency_Set_To_Coins()
        {
            var hud = MakeHud();
            hud.SetCurrencyForTests(CurrencyType.Coins);
            _economy.SetCurrency(CurrencyType.Energy, 4);
            _economy.SetCurrency(CurrencyType.Coins, 999);
            _progression.SetEnergyRegenState(State(4, 5, _time.GetServerTimeUtc().AddSeconds(60), false));
            hud.ForceRefreshForTests();
            Assert.AreEqual(4, hud.LastValueForTests, "ResolveCurrency seals to Energy regardless of _currency Inspector field — buyer-proof per E1 LOCKED Option A.");
        }

        [Test]
        public void OnChanged_Energy_Triggers_Regen_Poll_Refresh()
        {
            var hud = MakeHud();
            _progression.SetEnergyRegenState(State(3, 5, _time.GetServerTimeUtc().AddSeconds(60), false));
            hud.ForceRefreshForTests();
            Assert.AreEqual("/5", hud.CapLabelTextForTests, "Initial poll renders /5.");

            _progression.SetEnergyRegenState(State(4, 10, _time.GetServerTimeUtc().AddSeconds(60), false));
            _economy.Add(CurrencyType.Energy, 1);

            Assert.AreEqual("/10", hud.CapLabelTextForTests, "OnChanged(Energy) must trigger PollAndApplyRegenState — new Max picked up immediately, not on next 1Hz tick.");
        }
    }
}
