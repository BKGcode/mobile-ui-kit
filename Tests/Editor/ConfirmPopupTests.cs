using KitforgeLabs.MobileUIKit.Catalog.Confirm;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class ConfirmPopupTests
    {
        private GameObject _go;
        private ConfirmPopup _popup;
        private int _confirmedCount;
        private int _cancelledCount;
        private int _dismissedCount;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ConfirmPopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimConfirmPopup>();
            _popup = _go.AddComponent<ConfirmPopup>();
            _confirmedCount = 0;
            _cancelledCount = 0;
            _dismissedCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void SubscribeCounters()
        {
            _popup.OnConfirmed += () => _confirmedCount++;
            _popup.OnCancelled += () => _cancelledCount++;
            _popup.OnDismissed += () => _dismissedCount++;
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_With_Cancel_Visible_Triggers_Cancel_Once()
        {
            _popup.Bind(new ConfirmPopupData { ShowCancel = true });
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _cancelledCount, "Cancel must fire once on back press.");
            Assert.AreEqual(0, _confirmedCount, "Confirm must NOT fire on back press when cancel is visible.");
        }

        [Test]
        public void Back_Press_Single_Button_Triggers_Confirm()
        {
            _popup.Bind(new ConfirmPopupData { ShowCancel = false });
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _confirmedCount, "Confirm must fire when ShowCancel = false.");
            Assert.AreEqual(0, _cancelledCount);
        }

        [Test]
        public void Back_Press_While_Dismissing_Is_Ignored()
        {
            _popup.Bind(new ConfirmPopupData { ShowCancel = true });
            SubscribeCounters();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _cancelledCount, "Subsequent back presses must be ignored once dismissing.");
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(new ConfirmPopupData());
            SubscribeCounters();
            _popup.OnBackPressed();
            var beforeCount = _cancelledCount;
            _popup.Bind(new ConfirmPopupData());
            _popup.OnBackPressed();
            Assert.AreEqual(beforeCount, _cancelledCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }
    }
}
