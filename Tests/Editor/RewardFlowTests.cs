using System.Reflection;
using System.Text.RegularExpressions;
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
    public class RewardFlowTests
    {
        private GameObject _host;
        private GameObject _popupRoot;
        private GameObject _rewardPrefabGo;
        private PopupManager _manager;
        private FakeEconomyService _economy;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject(nameof(RewardFlowTests));
            _popupRoot = new GameObject("PopupRoot");
            _popupRoot.transform.SetParent(_host.transform);
            _manager = _host.AddComponent<PopupManager>();

            _rewardPrefabGo = new GameObject("RewardPopup_Prefab");
            _rewardPrefabGo.SetActive(false);
            _rewardPrefabGo.AddComponent<CanvasGroup>();
            _rewardPrefabGo.AddComponent<UIAnimRewardPopup>();
            var rewardPrefab = _rewardPrefabGo.AddComponent<RewardPopup>();

            SetField(_manager, "_popupRoot", _popupRoot.transform);
            SetField(_manager, "_popupPrefabs", new UIModuleBase[] { rewardPrefab });

            _economy = new FakeEconomyService();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) UnityEngine.Object.DestroyImmediate(_host);
            if (_rewardPrefabGo != null) UnityEngine.Object.DestroyImmediate(_rewardPrefabGo);
        }

        private RewardPopup PreWarmRewardPopupWithNullAnimator()
        {
            var dummy = new RewardPopupData { Kind = RewardKind.Coins, Amount = 0 };
            var instance = _manager.Show<RewardPopup>(dummy);
            instance.SetAnimatorForTests(new NullAnimator());
            _manager.Dismiss(instance);
            return instance;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        [Test]
        public void Sequence_With_Null_PopupManager_LogsError_And_NoOps()
        {
            var completeCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("popups argument is null"));
            RewardFlow.GrantAndShowSequence(null, _economy, new[] { new RewardPopupData() }, () => completeCount++);
            Assert.AreEqual(0, completeCount, "onSequenceComplete must NOT fire on validation error.");
        }

        [Test]
        public void Sequence_With_Null_Economy_LogsError_And_NoOps()
        {
            var completeCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("IEconomyService argument is null"));
            RewardFlow.GrantAndShowSequence(_manager, null, new[] { new RewardPopupData() }, () => completeCount++);
            Assert.AreEqual(0, completeCount);
        }

        [Test]
        public void Sequence_With_Null_Rewards_LogsError_And_NoOps()
        {
            var completeCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("rewards collection is null"));
            RewardFlow.GrantAndShowSequence(_manager, _economy, null, () => completeCount++);
            Assert.AreEqual(0, completeCount);
        }

        [Test]
        public void Sequence_With_Empty_Collection_LogsError_And_NoOps()
        {
            var completeCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("rewards collection is empty"));
            RewardFlow.GrantAndShowSequence(_manager, _economy, new RewardPopupData[0], () => completeCount++);
            Assert.AreEqual(0, completeCount);
            Assert.AreEqual(0, _manager.ActiveCount, "No popups should be on the active stack after a no-op call.");
        }

        [Test]
        public void Sequence_With_Single_Reward_Shows_And_Credits_Economy_On_Claim_Then_Fires_Complete()
        {
            var cached = PreWarmRewardPopupWithNullAnimator();
            var completeCount = 0;
            var rewards = new[] { new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 } };

            RewardFlow.GrantAndShowSequence(_manager, _economy, rewards, () => completeCount++);
            cached.OnBackPressed();

            Assert.AreEqual(100, _economy.Get(CurrencyType.Coins), "Single Coins reward must credit economy on claim.");
            Assert.AreEqual(1, completeCount, "onSequenceComplete fires after the last reward dismisses.");
        }

        [Test]
        public void Sequence_With_3_Rewards_Chains_Through_OnDismissed_Crediting_Each_And_Fires_Complete_After_Last()
        {
            var cached = PreWarmRewardPopupWithNullAnimator();
            var completeCount = 0;
            var rewards = new[]
            {
                new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 },
                new RewardPopupData { Kind = RewardKind.Gems, Amount = 5 },
                new RewardPopupData { Kind = RewardKind.Coins, Amount = 50 },
            };

            RewardFlow.GrantAndShowSequence(_manager, _economy, rewards, () => completeCount++);
            cached.OnBackPressed();
            cached.OnBackPressed();
            cached.OnBackPressed();

            Assert.AreEqual(150, _economy.Get(CurrencyType.Coins), "Two Coins rewards (100 + 50) must accumulate.");
            Assert.AreEqual(5, _economy.Get(CurrencyType.Gems), "Single Gems reward must credit.");
            Assert.AreEqual(1, completeCount, "onSequenceComplete fires exactly once after the last reward.");
        }

        [Test]
        public void Single_With_Null_PopupManager_LogsError_And_NoOps()
        {
            var callbackCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("popups argument is null"));
            RewardFlow.GrantAndShow(null, _economy, new RewardPopupData(), (c, a) => callbackCount++);
            Assert.AreEqual(0, callbackCount, "onClaimed must NOT fire on validation error.");
        }

        [Test]
        public void Single_With_Null_Economy_LogsError_And_NoOps()
        {
            var callbackCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("IEconomyService argument is null"));
            RewardFlow.GrantAndShow(_manager, null, new RewardPopupData(), (c, a) => callbackCount++);
            Assert.AreEqual(0, callbackCount);
        }

        [Test]
        public void Single_With_Null_Reward_LogsError_And_NoOps()
        {
            var callbackCount = 0;
            LogAssert.Expect(LogType.Error, new Regex("reward argument is null"));
            RewardFlow.GrantAndShow(_manager, _economy, null, (c, a) => callbackCount++);
            Assert.AreEqual(0, callbackCount);
        }

        [Test]
        public void Single_With_Coins_Reward_Shows_And_Credits_Economy_On_Claim_Then_Fires_Callback()
        {
            var cached = PreWarmRewardPopupWithNullAnimator();
            var receivedCurrency = (CurrencyType)(-99);
            var receivedAmount = 0;

            RewardFlow.GrantAndShow(_manager, _economy, new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 },
                (c, a) => { receivedCurrency = c; receivedAmount = a; });
            cached.OnBackPressed();

            Assert.AreEqual(100, _economy.Get(CurrencyType.Coins), "Coins reward must credit economy on claim.");
            Assert.AreEqual(CurrencyType.Coins, receivedCurrency, "onClaimed must receive the granted currency.");
            Assert.AreEqual(100, receivedAmount, "onClaimed must receive the granted amount.");
        }

        [Test]
        public void Single_With_Item_Sentinel_Currency_Does_Not_Credit_Economy_But_Fires_Callback()
        {
            var cached = PreWarmRewardPopupWithNullAnimator();
            var callbackCount = 0;

            RewardFlow.GrantAndShow(_manager, _economy, new RewardPopupData { Kind = RewardKind.Item, ItemId = "epic_sword" },
                (c, a) => callbackCount++);
            cached.OnBackPressed();

            Assert.AreEqual(0, _economy.Get(CurrencyType.Coins), "Item reward must NOT auto-credit economy (host resolves).");
            Assert.AreEqual(0, _economy.Get(CurrencyType.Gems));
            Assert.AreEqual(1, callbackCount, "onClaimed still fires for Item rewards (host resolves the actual grant).");
        }

        [Test]
        public void Sequence_Filters_Item_Bundle_Sentinel_Currency_Without_Crediting()
        {
            var cached = PreWarmRewardPopupWithNullAnimator();
            var completeCount = 0;
            var rewards = new[]
            {
                new RewardPopupData { Kind = RewardKind.Item, ItemId = "magic_ring" },
                new RewardPopupData { Kind = RewardKind.Bundle, BundleLines = new[] { "100 Coins", "1 Gem" } },
                new RewardPopupData { Kind = RewardKind.Coins, Amount = 50 },
            };

            RewardFlow.GrantAndShowSequence(_manager, _economy, rewards, () => completeCount++);
            cached.OnBackPressed();
            cached.OnBackPressed();
            cached.OnBackPressed();

            Assert.AreEqual(50, _economy.Get(CurrencyType.Coins), "Only the non-sentinel Coins reward credits — Item/Bundle pass through silently per FD4 contract.");
            Assert.AreEqual(0, _economy.Get(CurrencyType.Gems), "Bundle line text is buyer-resolved; no auto-credit.");
            Assert.AreEqual(1, completeCount);
        }
    }
}
