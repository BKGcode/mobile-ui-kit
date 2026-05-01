using KitforgeLabs.MobileUIKit.Catalog.Toasts;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class NotificationToastTests
    {
        private GameObject _go;
        private NotificationToast _toast;
        private int _tappedCount;
        private int _dismissedCount;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("NotificationToast_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimNotificationToast>();
            _toast = _go.AddComponent<NotificationToast>();
            _tappedCount = 0;
            _dismissedCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void SubscribeCounters()
        {
            _toast.OnTapped += () => _tappedCount++;
            _toast.OnDismissed += () => _dismissedCount++;
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _toast.Bind(null);
            Assert.IsFalse(_toast.IsDismissing);
        }

        [Test]
        public void Default_Duration_Falls_Back_When_Override_Not_Set()
        {
            _toast.Bind(new NotificationToastData());
            Assert.Greater(_toast.DefaultDuration, 0f, "Default duration must be positive when no override is set.");
        }

        [Test]
        public void Default_Duration_Honors_Override_When_Positive()
        {
            _toast.Bind(new NotificationToastData { DurationOverride = 7.5f });
            Assert.AreEqual(7.5f, _toast.DefaultDuration, 0.0001f);
        }

        [Test]
        public void Dismiss_Now_Is_Idempotent()
        {
            _toast.Bind(new NotificationToastData());
            SubscribeCounters();
            _toast.DismissNow();
            _toast.DismissNow();
            _toast.DismissNow();
            Assert.AreEqual(1, _dismissedCount, "DismissNow must fire OnDismissed only once even when called repeatedly.");
            Assert.IsTrue(_toast.IsDismissing);
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _toast.Bind(new NotificationToastData());
            SubscribeCounters();
            _toast.DismissNow();
            var beforeCount = _dismissedCount;
            _toast.Bind(new NotificationToastData());
            _toast.DismissNow();
            Assert.AreEqual(beforeCount, _dismissedCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }
    }
}
