using System.Reflection;
using System.Text.RegularExpressions;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class LevelCompletePopupTests
    {
        private GameObject _popupGo;
        private GameObject _servicesGo;
        private LevelCompletePopup _popup;
        private int _nextCount;
        private int _retryCount;
        private int _mainMenuCount;
        private int _dismissedCount;
        private LevelCompletePopupData _lastEventData;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            var services = _servicesGo.AddComponent<UIServices>();

            _popupGo = new GameObject("LevelCompletePopup_Test");
            _popupGo.AddComponent<CanvasGroup>();
            _popupGo.AddComponent<UIAnimLevelCompletePopup>();
            _popup = _popupGo.AddComponent<LevelCompletePopup>();
            _popup.SetAnimatorForTests(new NullAnimator());
            _popup.Initialize(null, services);

            _nextCount = 0;
            _retryCount = 0;
            _mainMenuCount = 0;
            _dismissedCount = 0;
            _lastEventData = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_popupGo != null) UnityEngine.Object.DestroyImmediate(_popupGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        private void SubscribeCounters()
        {
            _popup.OnNextRequested += data => { _nextCount++; _lastEventData = data; };
            _popup.OnRetryRequested += data => { _retryCount++; _lastEventData = data; };
            _popup.OnMainMenuRequested += data => { _mainMenuCount++; _lastEventData = data; };
            _popup.OnDismissed += () => _dismissedCount++;
        }

        private static LevelCompletePopupData MakeData(int stars = 3, int score = 1000, bool isNewBest = false)
        {
            return new LevelCompletePopupData
            {
                Stars = stars,
                Score = score,
                BestScore = score,
                IsNewBest = isNewBest,
                Rewards = new[] { new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 } },
            };
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
            Assert.IsNotNull(_popup.DataForTests);
            Assert.AreEqual(0, _popup.DataForTests.Stars);
            Assert.AreEqual(0, _popup.DataForTests.Score);
        }

        [Test]
        public void DTO_Defaults_Match_Spec_Contract()
        {
            var data = new LevelCompletePopupData();
            Assert.AreEqual("Level Complete!", data.Title);
            Assert.AreEqual("", data.LevelLabel);
            Assert.AreEqual(0, data.Stars);
            Assert.AreEqual(0, data.Score);
            Assert.AreEqual(0, data.BestScore);
            Assert.IsFalse(data.IsNewBest);
            Assert.IsNull(data.Rewards);
            Assert.AreEqual("Next", data.NextLabel);
            Assert.AreEqual("Retry", data.RetryLabel);
            Assert.AreEqual("Main Menu", data.MainMenuLabel);
            Assert.IsTrue(data.ShowNext);
            Assert.IsTrue(data.ShowRetry);
            Assert.IsFalse(data.ShowMainMenu);
            Assert.IsFalse(data.CloseOnBackdrop);
        }

        [Test]
        public void Click_Next_Fires_OnNextRequested_With_Data()
        {
            _popup.Bind(MakeData(stars: 3, score: 5000));
            SubscribeCounters();
            _popup.InvokeNextForTests();
            Assert.AreEqual(1, _nextCount);
            Assert.AreSame(_popup.DataForTests, _lastEventData);
            Assert.AreEqual(5000, _lastEventData.Score);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Click_Retry_Fires_OnRetryRequested()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeRetryForTests();
            Assert.AreEqual(1, _retryCount);
            Assert.AreEqual(0, _nextCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Click_MainMenu_Fires_OnMainMenuRequested()
        {
            var data = MakeData();
            data.ShowMainMenu = true;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeMainMenuForTests();
            Assert.AreEqual(1, _mainMenuCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void MainMenu_Click_With_ShowMainMenu_False_Is_NoOp()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeMainMenuForTests();
            Assert.AreEqual(0, _mainMenuCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Next_Click_With_ShowNext_False_Is_NoOp()
        {
            var data = MakeData();
            data.ShowNext = false;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeNextForTests();
            Assert.AreEqual(0, _nextCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Spam_Next_Only_First_Wins()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeNextForTests();
            _popup.InvokeNextForTests();
            _popup.InvokeNextForTests();
            Assert.AreEqual(1, _nextCount);
        }

        [Test]
        public void Back_Press_Routes_To_Retry_L7()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _retryCount);
            Assert.AreEqual(0, _nextCount);
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, _dismissedCount);
        }

        [Test]
        public void Backdrop_Click_With_CloseOnBackdrop_False_Is_Ignored()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeBackdropForTests();
            Assert.IsFalse(_popup.IsDismissing);
            Assert.AreEqual(0, _retryCount);
        }

        [Test]
        public void Backdrop_Click_With_CloseOnBackdrop_True_Routes_To_Retry()
        {
            var data = MakeData();
            data.CloseOnBackdrop = true;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeBackdropForTests();
            Assert.AreEqual(1, _retryCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Stars_Out_Of_Range_Are_Clamped_With_Warning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Stars=5 out of range"));
            _popup.Bind(MakeData(stars: 5));
            Assert.AreEqual(3, _popup.DataForTests.Stars);
        }

        [Test]
        public void Stars_Negative_Are_Clamped_With_Warning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Stars=-1 out of range"));
            _popup.Bind(MakeData(stars: -1));
            Assert.AreEqual(0, _popup.DataForTests.Stars);
        }

        [Test]
        public void All_Ctas_Hidden_Forces_Retry_With_Error()
        {
            var data = MakeData();
            data.ShowNext = false;
            data.ShowRetry = false;
            data.ShowMainMenu = false;
            LogAssert.Expect(LogType.Error, new Regex("All CTAs hidden"));
            _popup.Bind(data);
            Assert.IsTrue(_popup.DataForTests.ShowRetry, "Foot-gun guard must restore ShowRetry=true so player has an exit.");
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeNextForTests();
            var beforeCount = _nextCount;
            _popup.Bind(MakeData());
            _popup.InvokeNextForTests();
            Assert.AreEqual(beforeCount, _nextCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

        [Test]
        public void Event_Data_Carries_Original_Rewards_And_RewardFlow_Credits_Economy_Per_Reward()
        {
            var rewards = new[]
            {
                new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 },
                new RewardPopupData { Kind = RewardKind.Gems, Amount = 5 },
            };
            var data = MakeData();
            data.Rewards = rewards;
            _popup.Bind(data);

            var economy = new FakeEconomyService();
            var managerHost = new GameObject("LCP_HelperIntegration_Mgr");
            var popupRoot = new GameObject("PopupRoot");
            popupRoot.transform.SetParent(managerHost.transform);
            var manager = managerHost.AddComponent<PopupManager>();
            var rewardPrefabGo = new GameObject("RewardPopup_Prefab");
            rewardPrefabGo.SetActive(false);
            rewardPrefabGo.AddComponent<CanvasGroup>();
            rewardPrefabGo.AddComponent<UIAnimRewardPopup>();
            var rewardPrefab = rewardPrefabGo.AddComponent<RewardPopup>();
            SetField(manager, "_popupRoot", popupRoot.transform);
            SetField(manager, "_popupPrefabs", new UIModuleBase[] { rewardPrefab });

            var dummy = manager.Show<RewardPopup>(new RewardPopupData { Kind = RewardKind.Coins, Amount = 0 });
            dummy.SetAnimatorForTests(new NullAnimator());
            manager.Dismiss(dummy);

            var sequenceCompleteCount = 0;
            _popup.OnNextRequested += d =>
            {
                Assert.AreSame(rewards, d.Rewards, "Host needs the same Rewards array reference to call RewardFlow.GrantAndShowSequence (L3 carry-through contract).");
                RewardFlow.GrantAndShowSequence(manager, economy, d.Rewards, () => sequenceCompleteCount++);
            };
            _popup.InvokeNextForTests();
            dummy.OnBackPressed();
            dummy.OnBackPressed();

            Assert.AreEqual(100, economy.Get(CurrencyType.Coins), "Coins reward credited via RewardFlow.GrantAndShowSequence (mejora H integration proof).");
            Assert.AreEqual(5, economy.Get(CurrencyType.Gems), "Gems reward credited via RewardFlow.GrantAndShowSequence.");
            Assert.AreEqual(1, sequenceCompleteCount, "Sequence onComplete fires exactly once after both rewards claimed.");

            UnityEngine.Object.DestroyImmediate(managerHost);
            UnityEngine.Object.DestroyImmediate(rewardPrefabGo);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }
    }
}
