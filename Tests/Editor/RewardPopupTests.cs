using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class RewardPopupTests
    {
        private GameObject _go;
        private RewardPopup _popup;
        private CurrencyType _lastCurrency;
        private int _lastAmount;
        private int _claimedCount;
        private int _dismissedCount;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("RewardPopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimRewardPopup>();
            _popup = _go.AddComponent<RewardPopup>();
            _popup.SetAnimatorForTests(new NullAnimator());
            _lastCurrency = CurrencyType.Coins;
            _lastAmount = 0;
            _claimedCount = 0;
            _dismissedCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void SubscribeCounters()
        {
            _popup.OnClaimed += (currency, amount) =>
            {
                _claimedCount++;
                _lastCurrency = currency;
                _lastAmount = amount;
            };
            _popup.OnDismissed += () => _dismissedCount++;
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_Triggers_Claim_Once_With_Coins()
        {
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Coins, Amount = 100 });
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _claimedCount);
            Assert.AreEqual(CurrencyType.Coins, _lastCurrency);
            Assert.AreEqual(100, _lastAmount);
        }

        [Test]
        public void Back_Press_While_Dismissing_Is_Ignored()
        {
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Gems, Amount = 5 });
            SubscribeCounters();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _claimedCount);
        }

        [Test]
        public void Item_Kind_Emits_Sentinel_Currency_And_Zero_Amount()
        {
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Item, ItemId = "epic_sword" });
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _claimedCount);
            Assert.AreEqual((CurrencyType)RewardPopup.ItemCurrencySentinel, _lastCurrency);
            Assert.AreEqual(0, _lastAmount);
        }

        [Test]
        public void Bundle_Kind_With_Null_Lines_Does_Not_Throw()
        {
            Assert.DoesNotThrow(() => _popup.Bind(new RewardPopupData { Kind = RewardKind.Bundle, BundleLines = null }));
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _claimedCount);
            Assert.AreEqual((CurrencyType)RewardPopup.ItemCurrencySentinel, _lastCurrency);
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Coins, Amount = 50 });
            SubscribeCounters();
            _popup.OnBackPressed();
            var beforeCount = _claimedCount;
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Coins, Amount = 99 });
            _popup.OnBackPressed();
            Assert.AreEqual(beforeCount, _claimedCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

        [Test]
        public void AutoClaim_Fires_After_Threshold_Elapses()
        {
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Coins, Amount = 10, AutoClaimSeconds = 1f });
            SubscribeCounters();
            _popup.OnShow();
            _popup.AdvanceAutoClaim(0.4f);
            Assert.AreEqual(0, _claimedCount, "Auto-claim must NOT fire before threshold elapses.");
            _popup.AdvanceAutoClaim(0.7f);
            Assert.AreEqual(1, _claimedCount, "Auto-claim must fire exactly once after threshold.");
            _popup.AdvanceAutoClaim(1f);
            Assert.AreEqual(1, _claimedCount, "Auto-claim must NOT re-fire after dismissing.");
        }

        [Test]
        public void Manual_Claim_Cancels_AutoClaim_Timer()
        {
            _popup.Bind(new RewardPopupData { Kind = RewardKind.Gems, Amount = 3, AutoClaimSeconds = 5f });
            SubscribeCounters();
            _popup.OnShow();
            _popup.AdvanceAutoClaim(0.5f);
            _popup.OnBackPressed();
            _popup.AdvanceAutoClaim(10f);
            Assert.AreEqual(1, _claimedCount, "Manual claim must cancel the timer; only one claim total.");
        }

    }
}
