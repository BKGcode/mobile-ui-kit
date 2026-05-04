using System.Text.RegularExpressions;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
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
    public class DailyLoginFlowTests
    {
        private GameObject _servicesGo;
        private GameObject _popupsGo;
        private UIServices _services;
        private PopupManager _popups;
        private FakeProgressionService _progression;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            _services = _servicesGo.AddComponent<UIServices>();
            _progression = new FakeProgressionService();
            _services.SetProgression(_progression);

            _popupsGo = new GameObject("PopupManager_Test");
            _popups = _popupsGo.AddComponent<PopupManager>();

            DailyLoginFlow.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
            if (_popupsGo != null) UnityEngine.Object.DestroyImmediate(_popupsGo);
            DailyLoginFlow.ResetForTests();
        }

        private static DailyLoginPopupData MakeTemplate()
        {
            return new DailyLoginPopupData
            {
                RewardEntries = new[]
                {
                    new DailyLoginRewardEntry { Rewards = new[] { new RewardPopupData { Amount = 100 } } },
                },
            };
        }

        [Test]
        public void ShowIfDue_With_Null_Popups_Logs_Error_And_Returns_False()
        {
            LogAssert.Expect(LogType.Error, new Regex("PopupManager not registered on UIServices"));
            var result = DailyLoginFlow.ShowIfDue(null, _services, MakeTemplate());
            Assert.IsFalse(result);
            Assert.IsFalse(DailyLoginFlow.ShownThisLaunchForTests);
        }

        [Test]
        public void ShowIfDue_With_Null_Progression_Logs_Error_And_Returns_False()
        {
            var bareServicesGo = new GameObject("BareServices");
            var bareServices = bareServicesGo.AddComponent<UIServices>();
            LogAssert.Expect(LogType.Error, new Regex("IProgressionService not registered on UIServices"));
            var result = DailyLoginFlow.ShowIfDue(_popups, bareServices, MakeTemplate());
            Assert.IsFalse(result);
            Assert.IsFalse(DailyLoginFlow.ShownThisLaunchForTests);
            UnityEngine.Object.DestroyImmediate(bareServicesGo);
        }

        [Test]
        public void ShowIfDue_With_Null_Services_Logs_Error_And_Returns_False()
        {
            LogAssert.Expect(LogType.Error, new Regex("IProgressionService not registered on UIServices"));
            var result = DailyLoginFlow.ShowIfDue(_popups, null, MakeTemplate());
            Assert.IsFalse(result);
            Assert.IsFalse(DailyLoginFlow.ShownThisLaunchForTests);
        }

        [Test]
        public void ShowIfDue_With_Null_Template_Logs_Error_And_Returns_False()
        {
            LogAssert.Expect(LogType.Error, new Regex("configTemplate is null"));
            var result = DailyLoginFlow.ShowIfDue(_popups, _services, null);
            Assert.IsFalse(result);
            Assert.IsFalse(DailyLoginFlow.ShownThisLaunchForTests);
        }

        [Test]
        public void ShowIfDue_When_Already_Claimed_Today_Returns_False()
        {
            _progression.SetDailyLoginState(new DailyLoginState
            {
                CurrentDay = 3,
                AlreadyClaimedToday = true,
            });
            var result = DailyLoginFlow.ShowIfDue(_popups, _services, MakeTemplate());
            Assert.IsFalse(result);
            Assert.IsFalse(DailyLoginFlow.ShownThisLaunchForTests, "AlreadyClaimedToday must NOT consume the once-per-launch slot.");
        }

        [Test]
        public void ShowIfDue_Idempotent_Within_Same_Launch_FQ2()
        {
            DailyLoginFlow.ForceShownForTests();
            _progression.SetDailyLoginState(new DailyLoginState
            {
                CurrentDay = 1,
                AlreadyClaimedToday = false,
            });
            var result = DailyLoginFlow.ShowIfDue(_popups, _services, MakeTemplate());
            Assert.IsFalse(result, "Second call within same launch must return false even when due (FQ2).");
        }

        [Test]
        public void ShowIfDue_Resets_With_Test_Helper()
        {
            DailyLoginFlow.ForceShownForTests();
            Assert.IsTrue(DailyLoginFlow.ShownThisLaunchForTests);
            DailyLoginFlow.ResetForTests();
            Assert.IsFalse(DailyLoginFlow.ShownThisLaunchForTests);
        }
    }
}
