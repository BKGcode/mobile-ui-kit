using System;
using System.Text.RegularExpressions;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class DailyLoginPopupTests
    {
        private GameObject _popupGo;
        private GameObject _servicesGo;
        private DailyLoginPopup _popup;
        private FakeTimeService _time;
        private FakeAdsService _ads;
        private FakeProgressionService _progression;
        private int _claimedCount;
        private int _watchAdCount;
        private int _dismissedCount;
        private int _lastDay;
        private RewardPopupData[] _lastRewards;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            var services = _servicesGo.AddComponent<UIServices>();
            _time = new FakeTimeService();
            _ads = new FakeAdsService();
            _progression = new FakeProgressionService();
            services.SetTime(_time);
            services.SetAds(_ads);
            services.SetProgression(_progression);

            _popupGo = new GameObject("DailyLoginPopup_Test");
            _popupGo.AddComponent<CanvasGroup>();
            _popupGo.AddComponent<UIAnimDailyLoginPopup>();
            _popup = _popupGo.AddComponent<DailyLoginPopup>();
            _popup.SetAnimatorForTests(new NullAnimator());
            _popup.Initialize(null, services);

            _claimedCount = 0;
            _watchAdCount = 0;
            _dismissedCount = 0;
            _lastDay = 0;
            _lastRewards = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_popupGo != null) UnityEngine.Object.DestroyImmediate(_popupGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        private void SubscribeCounters()
        {
            _popup.OnDayClaimed += (day, rewards) => { _claimedCount++; _lastDay = day; _lastRewards = rewards; };
            _popup.OnWatchAdRequested += (day, rewards) => { _watchAdCount++; _lastDay = day; _lastRewards = rewards; };
            _popup.OnDismissed += () => _dismissedCount++;
        }

        private static DailyLoginPopupData MakeData(int currentDay = 1, bool alreadyClaimed = false, bool doubledToday = false, bool allowDouble = false)
        {
            var entry = new DailyLoginRewardEntry
            {
                Rewards = new[] { new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 } },
                AllowDouble = allowDouble,
            };
            return new DailyLoginPopupData
            {
                RewardEntries = new[] { entry, entry, entry },
                CurrentDay = currentDay,
                AlreadyClaimedToday = alreadyClaimed,
                DoubledToday = doubledToday,
            };
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
            Assert.IsNotNull(_popup.DataForTests);
            Assert.AreEqual(1, _popup.DataForTests.CurrentDay);
        }

        [Test]
        public void DTO_Defaults_Match_Spec_Contract()
        {
            var data = new DailyLoginPopupData();
            Assert.AreEqual("Daily Reward", data.Title);
            Assert.AreEqual(1, data.CurrentDay);
            Assert.IsFalse(data.AlreadyClaimedToday);
            Assert.IsFalse(data.DoubledToday);
            Assert.AreEqual(1, data.MaxStreakGapDays);
            Assert.AreEqual("Claim", data.ClaimLabel);
            Assert.AreEqual("Watch ad to double", data.WatchToDoubleLabel);
            Assert.IsFalse(data.CloseOnBackdrop);
        }

        [Test]
        public void Click_Claim_Fires_OnDayClaimed_With_Day_And_Rewards()
        {
            _popup.Bind(MakeData(currentDay: 3));
            SubscribeCounters();
            _popup.InvokeClaimForTests();
            Assert.AreEqual(1, _claimedCount);
            Assert.AreEqual(3, _lastDay);
            Assert.AreEqual(1, _lastRewards.Length);
            Assert.AreEqual(100, _lastRewards[0].Amount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Spam_Claim_Only_First_Wins()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeClaimForTests();
            _popup.InvokeClaimForTests();
            _popup.InvokeClaimForTests();
            Assert.AreEqual(1, _claimedCount);
        }

        [Test]
        public void Claim_While_AlreadyClaimedToday_Is_Ignored()
        {
            _popup.Bind(MakeData(alreadyClaimed: true));
            SubscribeCounters();
            _popup.InvokeClaimForTests();
            Assert.AreEqual(0, _claimedCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void WatchAd_When_Ad_Ready_Fires_OnWatchAdRequested()
        {
            _ads.SetRewardedReady(true);
            _popup.Bind(MakeData(allowDouble: true));
            SubscribeCounters();
            _popup.InvokeWatchAdForTests();
            Assert.AreEqual(1, _watchAdCount);
            Assert.AreEqual(0, _claimedCount, "OnDayClaimed must NOT fire on watch-ad path — paths are mutually exclusive (D5).");
        }

        [Test]
        public void WatchAd_When_Ad_Not_Ready_Is_NoOp()
        {
            _ads.SetRewardedReady(false);
            _popup.Bind(MakeData(allowDouble: true));
            SubscribeCounters();
            _popup.InvokeWatchAdForTests();
            Assert.AreEqual(0, _watchAdCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void WatchAd_When_DoubledToday_Is_NoOp_FQ3()
        {
            _ads.SetRewardedReady(true);
            _popup.Bind(MakeData(allowDouble: true, alreadyClaimed: true, doubledToday: true));
            SubscribeCounters();
            _popup.InvokeWatchAdForTests();
            Assert.AreEqual(0, _watchAdCount, "DoubledToday=true must hide the watch-to-double CTA (FQ3).");
        }

        [Test]
        public void WatchAd_AlreadyClaimed_With_DoubledToday_False_Still_Fires_FQ3()
        {
            _ads.SetRewardedReady(true);
            _popup.Bind(MakeData(allowDouble: true, alreadyClaimed: true, doubledToday: false));
            SubscribeCounters();
            _popup.InvokeWatchAdForTests();
            Assert.AreEqual(1, _watchAdCount, "Already-claimed + DoubledToday=false + ad ready must keep CTA active (FQ3).");
        }

        [Test]
        public void Back_Press_Triggers_Silent_Dismiss()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(0, _claimedCount);
            Assert.AreEqual(0, _watchAdCount);
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, _dismissedCount);
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
            Assert.AreEqual(0, _dismissedCount);
        }

        [Test]
        public void Backdrop_Click_With_CloseOnBackdrop_True_Dismisses_Silently()
        {
            var data = MakeData();
            data.CloseOnBackdrop = true;
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeBackdropForTests();
            Assert.AreEqual(0, _claimedCount);
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, _dismissedCount);
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(MakeData());
            SubscribeCounters();
            _popup.InvokeClaimForTests();
            var beforeCount = _claimedCount;
            _popup.Bind(MakeData());
            _popup.InvokeClaimForTests();
            Assert.AreEqual(beforeCount, _claimedCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

        [Test]
        public void CurrentDay_Beyond_Length_Wraps_Modulo()
        {
            var entries = new[]
            {
                new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Amount = 1 } } },
                new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Amount = 2 } } },
                new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Amount = 3 } } },
            };
            var data = new DailyLoginPopupData { RewardEntries = entries, CurrentDay = 7 };
            _popup.Bind(data);
            SubscribeCounters();
            _popup.InvokeClaimForTests();
            Assert.AreEqual(7, _lastDay, "Day index passed through unchanged.");
            Assert.AreEqual(1, _lastRewards[0].Amount, "(7-1) % 3 = 0 → first entry.");
        }

        [Test]
        public void Show_With_Null_Time_Service_Logs_Error_And_Aborts()
        {
            var bareGo = new GameObject("Bare");
            bareGo.AddComponent<CanvasGroup>();
            bareGo.AddComponent<UIAnimDailyLoginPopup>();
            var bare = bareGo.AddComponent<DailyLoginPopup>();
            bare.SetAnimatorForTests(new NullAnimator());
            bare.Bind(MakeData());
            LogAssert.Expect(LogType.Error, new Regex("ITimeService not registered on UIServices"));
            bare.OnShow();
            Assert.IsTrue(bare.IsDismissing);
            UnityEngine.Object.DestroyImmediate(bareGo);
        }

        [Test]
        public void Show_With_Empty_RewardEntries_Logs_Error_And_Aborts()
        {
            _popup.Bind(new DailyLoginPopupData { RewardEntries = Array.Empty<DailyLoginRewardEntry>() });
            LogAssert.Expect(LogType.Error, new Regex("RewardEntries is null or empty"));
            _popup.OnShow();
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void AdvanceCountdown_Ticks_Down_When_AlreadyClaimed()
        {
            _popup.Bind(MakeData(alreadyClaimed: true));
            var initial = _popup.CountdownRemainingForTests;
            Assume.That(initial, Is.GreaterThan(0f));
            _popup.AdvanceCountdown(1f);
            Assert.AreEqual(initial - 1f, _popup.CountdownRemainingForTests, 0.01f);
        }

        [Test]
        public void AdvanceCountdown_NoOp_When_ClaimReady()
        {
            _popup.Bind(MakeData(alreadyClaimed: false));
            _popup.AdvanceCountdown(10f);
            Assert.AreEqual(0f, _popup.CountdownRemainingForTests);
        }

        [Test]
        public void Countdown_Crossing_Zero_AutoTransitions_To_ClaimReady_FQ1()
        {
            _progression.SetDailyLoginState(new DailyLoginState
            {
                CurrentDay = 2,
                AlreadyClaimedToday = false,
            });
            _popup.Bind(MakeData(alreadyClaimed: true));
            _popup.AdvanceCountdown(1_000_000f);
            Assert.IsFalse(_popup.DataForTests.AlreadyClaimedToday, "Auto-transition must flip AlreadyClaimedToday to false.");
            Assert.AreEqual(2, _popup.DataForTests.CurrentDay, "Auto-transition must adopt the new CurrentDay from progression service.");
        }
    }
}
