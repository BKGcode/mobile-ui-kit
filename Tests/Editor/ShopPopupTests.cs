using KitforgeLabs.MobileUIKit.Catalog.Shop;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class ShopPopupTests
    {
        private GameObject _go;
        private GameObject _servicesGo;
        private ShopPopup _popup;
        private FakeShopDataProvider _shopData;
        private int _completedCount;
        private int _insufficientCount;
        private int _closedCount;
        private int _dismissedCount;
        private PurchaseResult _lastResult;
        private ShopItemData _lastItem;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            var services = _servicesGo.AddComponent<UIServices>();
            _shopData = new FakeShopDataProvider();
            services.SetShopData(_shopData);

            _go = new GameObject("ShopPopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimShopPopup>();
            _popup = _go.AddComponent<ShopPopup>();
            _popup.SetAnimatorForTests(new NullAnimator());
            _popup.Initialize(null, services);
            ResetCounters();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        private void ResetCounters()
        {
            _completedCount = 0;
            _insufficientCount = 0;
            _closedCount = 0;
            _dismissedCount = 0;
            _lastResult = PurchaseResult.Failed;
            _lastItem = default;
        }

        private void SubscribeCounters()
        {
            _popup.OnPurchaseCompleted += (item, result) => { _completedCount++; _lastResult = result; _lastItem = item; };
            _popup.OnPurchaseInsufficient += item => { _insufficientCount++; _lastItem = item; };
            _popup.OnClosed += () => _closedCount++;
            _popup.OnDismissed += () => _dismissedCount++;
        }

        private static ShopItemData MakeItem(string id = "item_a") => new ShopItemData
        {
            Id = id,
            DisplayName = id,
            Category = ShopCategory.Currency,
            PriceCurrency = CurrencyType.Coins,
            PriceAmount = 100
        };

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Purchase_Success_Fires_OnPurchaseCompleted_With_Success()
        {
            _popup.Bind(new ShopPopupData());
            SubscribeCounters();
            _popup.DispatchPurchaseForTests(MakeItem(), PurchaseResult.Success);
            Assert.AreEqual(1, _completedCount);
            Assert.AreEqual(PurchaseResult.Success, _lastResult);
            Assert.AreEqual(0, _insufficientCount, "InsufficientFunds event must not fire on success.");
            Assert.IsFalse(_popup.IsDismissing, "Popup must stay open after purchase.");
        }

        [Test]
        public void Purchase_InsufficientFunds_Fires_Insufficient_Event_Only()
        {
            _popup.Bind(new ShopPopupData());
            SubscribeCounters();
            _popup.DispatchPurchaseForTests(MakeItem(), PurchaseResult.InsufficientFunds);
            Assert.AreEqual(1, _insufficientCount);
            Assert.AreEqual(0, _completedCount, "OnPurchaseCompleted must NOT fire for InsufficientFunds.");
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Purchase_AlreadyOwned_Fires_OnPurchaseCompleted_With_AlreadyOwned()
        {
            _popup.Bind(new ShopPopupData());
            SubscribeCounters();
            _popup.DispatchPurchaseForTests(MakeItem(), PurchaseResult.AlreadyOwned);
            Assert.AreEqual(1, _completedCount);
            Assert.AreEqual(PurchaseResult.AlreadyOwned, _lastResult);
            Assert.AreEqual(0, _insufficientCount);
        }

        [Test]
        public void Purchase_Failed_Fires_OnPurchaseCompleted_With_Failed()
        {
            _popup.Bind(new ShopPopupData());
            SubscribeCounters();
            _popup.DispatchPurchaseForTests(MakeItem(), PurchaseResult.Failed);
            Assert.AreEqual(1, _completedCount);
            Assert.AreEqual(PurchaseResult.Failed, _lastResult);
        }

        [Test]
        public void Back_Press_Triggers_Close_Once()
        {
            _popup.Bind(new ShopPopupData());
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _closedCount);
            _popup.OnBackPressed();
            Assert.AreEqual(1, _closedCount, "Subsequent back presses must be ignored once dismissing.");
        }

        [Test]
        public void InsufficientFunds_Disables_The_Matching_Cell_Until_Affordability_Refreshes()
        {
            var cellGoA = new GameObject("CellA");
            var cellA = cellGoA.AddComponent<ShopItemView>();
            cellA.Bind(MakeItem("item_a"), null, Color.white, Color.gray);
            cellA.SetAffordable(true);

            var cellGoB = new GameObject("CellB");
            var cellB = cellGoB.AddComponent<ShopItemView>();
            cellB.Bind(MakeItem("item_b"), null, Color.white, Color.gray);
            cellB.SetAffordable(true);

            _popup.RegisterCellForTests(cellA);
            _popup.RegisterCellForTests(cellB);
            SubscribeCounters();

            _popup.DispatchPurchaseForTests(MakeItem("item_a"), PurchaseResult.InsufficientFunds);
            Assert.AreEqual(1, _insufficientCount);
            Assert.IsFalse(cellA.IsAffordableForTests, "Cell for the unaffordable item must be disabled.");
            Assert.IsTrue(cellB.IsAffordableForTests, "Other cells must NOT be affected.");

            _popup.DispatchPurchaseForTests(MakeItem("item_a"), PurchaseResult.InsufficientFunds);
            _popup.DispatchPurchaseForTests(MakeItem("item_a"), PurchaseResult.InsufficientFunds);
            Assert.IsFalse(cellA.IsAffordableForTests, "Cell stays disabled across repeated insufficient attempts.");

            UnityEngine.Object.DestroyImmediate(cellGoA);
            UnityEngine.Object.DestroyImmediate(cellGoB);
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(new ShopPopupData());
            SubscribeCounters();
            _popup.DispatchPurchaseForTests(MakeItem(), PurchaseResult.Success);
            var beforeCount = _completedCount;
            _popup.Bind(new ShopPopupData());
            _popup.DispatchPurchaseForTests(MakeItem(), PurchaseResult.Success);
            Assert.AreEqual(beforeCount, _completedCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }

    }
}
