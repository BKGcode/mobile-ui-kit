using System.Text.RegularExpressions;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class GameOverPopupTests
    {
        private GameObject _popupGo;
        private GameObject _servicesGo;
        private GameOverPopup _popup;
        private FakeEconomyService _economy;
        private FakeAdsService _ads;
        private int _continueAdCount;
        private int _continueCurrencyCount;
        private int _affordCheckFailedCount;
        private int _restartCount;
        private int _mainMenuCount;
        private int _dismissedCount;
        private CurrencyType _lastCurrency;
        private int _lastAmount;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            var services = _servicesGo.AddComponent<UIServices>();
            _economy = new FakeEconomyService();
            _ads = new FakeAdsService();
            services.SetEconomy(_economy);
            services.SetAds(_ads);

            _popupGo = new GameObject("GameOverPopup_Test");
            _popupGo.AddComponent<CanvasGroup>();
            _popupGo.AddComponent<UIAnimGameOverPopup>();
            _popup = _popupGo.AddComponent<GameOverPopup>();
            _popup.SetAnimatorForTests(new NullAnimator());
            _popup.Initialize(null, services);

            _continueAdCount = 0;
            _continueCurrencyCount = 0;
            _affordCheckFailedCount = 0;
            _restartCount = 0;
            _mainMenuCount = 0;
            _dismissedCount = 0;
            _lastCurrency = (CurrencyType)(-1);
            _lastAmount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_popupGo != null) UnityEngine.Object.DestroyImmediate(_popupGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        private void SubscribeCounters()
        {
            _popup.OnContinueWithAdRequested += () => _continueAdCount++;
            _popup.OnContinueWithCurrencyRequested += (c, a) => { _continueCurrencyCount++; _lastCurrency = c; _lastAmount = a; };
            _popup.OnContinueAffordCheckFailed += (c, a) => { _affordCheckFailedCount++; _lastCurrency = c; _lastAmount = a; };
            _popup.OnRestartRequested += () => _restartCount++;
            _popup.OnMainMenuRequested += () => _mainMenuCount++;
            _popup.OnDismissed += () => _dismissedCount++;
        }

        private static GameOverPopupData MakeData(
            ContinueMode continueMode = ContinueMode.Ad,
            int continueAmount = 5,
            int used = 0,
            int max = 1)
        {
            return new GameOverPopupData
            {
                ContinueMode = continueMode,
                ContinueCurrency = CurrencyType.Gems,
                ContinueAmount = continueAmount,
                ContinuesUsedThisSession = used,
                MaxContinuesPerSession = max,
            };
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
            Assert.IsNotNull(_popup.DataForTests);
            Assert.AreEqual("Game Over", _popup.DataForTests.Title);
            Assert.AreEqual(ContinueMode.Ad, _popup.DataForTests.ContinueMode);
        }

        [Test]
        public void DTO_Defaults_Match_Spec_Contract()
        {
            var data = new GameOverPopupData();
            Assert.AreEqual("Game Over", data.Title);
            Assert.AreEqual(-1, data.Score);
            Assert.AreEqual(ContinueMode.Ad, data.ContinueMode);
            Assert.AreEqual(CurrencyType.Gems, data.ContinueCurrency);
            Assert.AreEqual(5, data.ContinueAmount);
            Assert.AreEqual(0, data.ContinuesUsedThisSession);
            Assert.AreEqual(1, data.MaxContinuesPerSession);
            Assert.IsTrue(data.ShowRestart);
            Assert.IsTrue(data.ShowMainMenu);
            Assert.AreEqual(BackPressBehavior.Restart, data.BackPressBehavior);
            Assert.IsFalse(data.CloseOnBackdrop);
        }

        [Test]
        public void Click_ContinueAd_Fires_OnContinueWithAdRequested_And_Dismisses()
        {
            _popup.Bind(MakeData(continueMode: ContinueMode.Ad));
            SubscribeCounters();
            _popup.InvokeContinueAdForTests();
            Assert.AreEqual(1, _continueAdCount);
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, _dismissedCount);
        }

        [Test]
        public void Spam_ContinueAd_Only_First_Wins()
        {
            _popup.Bind(MakeData(continueMode: ContinueMode.Ad));
            SubscribeCounters();
            _popup.InvokeContinueAdForTests();
            _popup.InvokeContinueAdForTests();
            _popup.InvokeContinueAdForTests();
            Assert.AreEqual(1, _continueAdCount);
        }

        [Test]
        public void Click_ContinueCurrency_When_Affordable_Fires_OnContinueWithCurrencyRequested()
        {
            _economy.SetCurrency(CurrencyType.Gems, 10);
            _popup.Bind(MakeData(continueMode: ContinueMode.Currency, continueAmount: 5));
            SubscribeCounters();
            _popup.InvokeContinueCurrencyForTests();
            Assert.AreEqual(1, _continueCurrencyCount);
            Assert.AreEqual(0, _affordCheckFailedCount);
            Assert.AreEqual(CurrencyType.Gems, _lastCurrency);
            Assert.AreEqual(5, _lastAmount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Click_ContinueCurrency_When_Not_Affordable_Fires_AffordCheckFailed_Without_Dismiss()
        {
            _economy.SetCurrency(CurrencyType.Gems, 2);
            _popup.Bind(MakeData(continueMode: ContinueMode.Currency, continueAmount: 5));
            SubscribeCounters();
            _popup.InvokeContinueCurrencyForTests();
            Assert.AreEqual(0, _continueCurrencyCount, "Affordable event must NOT fire on insufficient funds (GO3).");
            Assert.AreEqual(1, _affordCheckFailedCount);
            Assert.AreEqual(CurrencyType.Gems, _lastCurrency);
            Assert.AreEqual(5, _lastAmount);
            Assert.IsFalse(_popup.IsDismissing, "Popup must NOT dismiss when affordability fails (GO3).");
        }

        [Test]
        public void ContinueCurrency_Click_With_No_Economy_Service_Is_NoOp()
        {
            var bareGo = new GameObject("BareServices");
            var bareServices = bareGo.AddComponent<UIServices>();
            var popupGo = new GameObject("BarePopup");
            popupGo.AddComponent<CanvasGroup>();
            popupGo.AddComponent<UIAnimGameOverPopup>();
            var popup = popupGo.AddComponent<GameOverPopup>();
            popup.SetAnimatorForTests(new NullAnimator());
            popup.Initialize(null, bareServices);
            popup.Bind(MakeData(continueMode: ContinueMode.Currency));
            var failed = 0;
            popup.OnContinueAffordCheckFailed += (_, __) => failed++;
            popup.InvokeContinueCurrencyForTests();
            Assert.AreEqual(0, failed);
            Assert.IsFalse(popup.IsDismissing);
            UnityEngine.Object.DestroyImmediate(popupGo);
            UnityEngine.Object.DestroyImmediate(bareGo);
        }

        [Test]
        public void Click_Restart_Fires_OnRestartRequested()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeRestartForTests();
            Assert.AreEqual(1, _restartCount);
            Assert.AreEqual(0, _mainMenuCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Click_MainMenu_Fires_OnMainMenuRequested()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeMainMenuForTests();
            Assert.AreEqual(1, _mainMenuCount);
            Assert.AreEqual(0, _restartCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_Restart_Behavior_Routes_To_Restart()
        {
            var data = MakeData();
            data.BackPressBehavior = BackPressBehavior.Restart;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _restartCount);
            Assert.AreEqual(0, _mainMenuCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_MainMenu_Behavior_Routes_To_MainMenu()
        {
            var data = MakeData();
            data.BackPressBehavior = BackPressBehavior.MainMenu;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(0, _restartCount);
            Assert.AreEqual(1, _mainMenuCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_Ignore_Behavior_Is_NoOp()
        {
            var data = MakeData();
            data.BackPressBehavior = BackPressBehavior.Ignore;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(0, _restartCount);
            Assert.AreEqual(0, _mainMenuCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Limit_Reached_Hides_Continue_Ctas_So_Clicks_Are_NoOp()
        {
            _ads.SetRewardedReady(true);
            _economy.SetCurrency(CurrencyType.Gems, 100);
            _popup.Bind(MakeData(continueMode: ContinueMode.AdOrCurrency, used: 1, max: 1));
            SubscribeCounters();
            _popup.InvokeContinueAdForTests();
            _popup.InvokeContinueCurrencyForTests();
            Assert.AreEqual(0, _continueAdCount);
            Assert.AreEqual(0, _continueCurrencyCount);
            Assert.AreEqual(0, _affordCheckFailedCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void ContinueMode_None_Hides_Both_Continue_Ctas()
        {
            _popup.Bind(MakeData(continueMode: ContinueMode.None));
            SubscribeCounters();
            _popup.InvokeContinueAdForTests();
            _popup.InvokeContinueCurrencyForTests();
            Assert.AreEqual(0, _continueAdCount);
            Assert.AreEqual(0, _continueCurrencyCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Foot_Gun_Guard_Forces_ShowMainMenu_When_All_Ctas_Hidden()
        {
            var data = MakeData(continueMode: ContinueMode.None);
            data.ShowRestart = false;
            data.ShowMainMenu = false;
            LogAssert.Expect(LogType.Error, new Regex("All CTAs hidden"));
            _popup.Bind(data);
            Assert.IsTrue(_popup.DataForTests.ShowMainMenu, "Foot-gun guard must force ShowMainMenu=true so player has an exit.");
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(MakeData(continueMode: ContinueMode.Ad));
            SubscribeCounters();
            _popup.InvokeContinueAdForTests();
            var beforeCount = _continueAdCount;
            _popup.Bind(MakeData(continueMode: ContinueMode.Ad));
            _popup.InvokeContinueAdForTests();
            Assert.AreEqual(beforeCount, _continueAdCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

        [Test]
        public void Backdrop_Click_With_CloseOnBackdrop_True_Routes_To_BackPressBehavior()
        {
            var data = MakeData();
            data.CloseOnBackdrop = true;
            data.BackPressBehavior = BackPressBehavior.Restart;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeBackdropForTests();
            Assert.AreEqual(1, _restartCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Backdrop_Click_With_CloseOnBackdrop_False_Is_Ignored()
        {
            var data = MakeData();
            data.CloseOnBackdrop = false;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeBackdropForTests();
            Assert.IsFalse(_popup.IsDismissing);
            Assert.AreEqual(0, _restartCount);
        }
    }
}
