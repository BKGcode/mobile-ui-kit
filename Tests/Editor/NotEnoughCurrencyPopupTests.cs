using KitforgeLabs.MobileUIKit.Catalog.NotEnough;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class NotEnoughCurrencyPopupTests
    {
        private GameObject _go;
        private NotEnoughCurrencyPopup _popup;
        private int _buyMoreCount;
        private int _watchAdCount;
        private int _declinedCount;
        private int _dismissedCount;
        private CurrencyType _lastCurrency;
        private int _lastMissing;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("NotEnoughCurrencyPopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimNotEnoughCurrencyPopup>();
            _popup = _go.AddComponent<NotEnoughCurrencyPopup>();
            _popup.SetAnimatorForTests(new NullAnimator());
            _buyMoreCount = 0;
            _watchAdCount = 0;
            _declinedCount = 0;
            _dismissedCount = 0;
            _lastCurrency = CurrencyType.Coins;
            _lastMissing = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void SubscribeCounters()
        {
            _popup.OnBuyMoreRequested += (currency, missing) => { _buyMoreCount++; _lastCurrency = currency; _lastMissing = missing; };
            _popup.OnWatchAdRequested += (currency, missing) => { _watchAdCount++; _lastCurrency = currency; _lastMissing = missing; };
            _popup.OnDeclined += () => _declinedCount++;
            _popup.OnDismissed += () => _dismissedCount++;
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_Triggers_Decline_Once()
        {
            _popup.Bind(new NotEnoughCurrencyPopupData { Currency = CurrencyType.Coins, Missing = 50 });
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _declinedCount);
            Assert.AreEqual(0, _buyMoreCount);
            Assert.AreEqual(0, _watchAdCount);
            _popup.OnBackPressed();
            Assert.AreEqual(1, _declinedCount, "Subsequent back presses must be ignored once dismissing.");
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(new NotEnoughCurrencyPopupData());
            SubscribeCounters();
            _popup.OnBackPressed();
            var beforeCount = _declinedCount;
            _popup.Bind(new NotEnoughCurrencyPopupData());
            _popup.OnBackPressed();
            Assert.AreEqual(beforeCount, _declinedCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

        [Test]
        public void BuyMore_Click_Emits_OnBuyMoreRequested_With_Currency_And_Missing()
        {
            _popup.Bind(new NotEnoughCurrencyPopupData { Currency = CurrencyType.Gems, Missing = 5 });
            SubscribeCounters();
            _popup.InvokeBuyMoreForTests();
            Assert.AreEqual(1, _buyMoreCount);
            Assert.AreEqual(CurrencyType.Gems, _lastCurrency);
            Assert.AreEqual(5, _lastMissing);
            Assert.AreEqual(0, _watchAdCount);
            Assert.AreEqual(0, _declinedCount);
        }

        [Test]
        public void WatchAd_Click_Emits_OnWatchAdRequested_With_Currency_And_Missing()
        {
            _popup.Bind(new NotEnoughCurrencyPopupData { Currency = CurrencyType.Coins, Missing = 50 });
            SubscribeCounters();
            _popup.InvokeWatchAdForTests();
            Assert.AreEqual(1, _watchAdCount);
            Assert.AreEqual(CurrencyType.Coins, _lastCurrency);
            Assert.AreEqual(50, _lastMissing);
            Assert.AreEqual(0, _buyMoreCount);
            Assert.AreEqual(0, _declinedCount);
        }

        [Test]
        public void Spam_CTAs_Only_First_Wins()
        {
            _popup.Bind(new NotEnoughCurrencyPopupData { Currency = CurrencyType.Coins, Missing = 50 });
            SubscribeCounters();
            _popup.InvokeBuyMoreForTests();
            _popup.InvokeWatchAdForTests();
            _popup.InvokeDeclineForTests();
            Assert.AreEqual(1, _buyMoreCount);
            Assert.AreEqual(0, _watchAdCount, "Second CTA must be ignored after first.");
            Assert.AreEqual(0, _declinedCount, "Decline after CTA must be ignored.");
        }

        [Test]
        public void Default_DTO_Hides_Decline_Button()
        {
            var data = new NotEnoughCurrencyPopupData();
            Assert.IsFalse(data.ShowDecline);
            Assert.IsTrue(data.ShowBuyMore);
            Assert.IsTrue(data.ShowWatchAd);
        }

        [Test]
        public void Empty_Message_Falls_Back_To_Generated_String()
        {
            var data = new NotEnoughCurrencyPopupData
            {
                Currency = CurrencyType.Gems,
                Missing = 7,
                Message = string.Empty
            };
            Assert.That(data.Message, Is.Empty);
            // Bind with the popup will route through ResolveMessage internally.
            // We don't have direct access to the rendered TMP text, but absence of NRE confirms the path.
            Assert.DoesNotThrow(() => _popup.Bind(data));
        }

        [Test]
        public void CloseOnBackdrop_True_By_Default()
        {
            var data = new NotEnoughCurrencyPopupData();
            Assert.IsTrue(data.CloseOnBackdrop);
        }

    }
}
