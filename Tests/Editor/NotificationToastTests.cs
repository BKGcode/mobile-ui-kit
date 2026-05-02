using System.Reflection;
using KitforgeLabs.MobileUIKit.Catalog.Toasts;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
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

        [Test]
        public void Tap_With_TapToDismiss_True_Fires_Event_And_Dismisses()
        {
            _toast.Bind(new NotificationToastData { TapToDismiss = true });
            SubscribeCounters();
            _toast.HandleTap();
            Assert.AreEqual(1, _tappedCount, "OnTapped must fire once on tap.");
            Assert.AreEqual(1, _dismissedCount, "Tap must trigger dismissal.");
            Assert.IsTrue(_toast.IsDismissing);
        }

        [Test]
        public void Tap_With_TapToDismiss_False_Is_NoOp()
        {
            _toast.Bind(new NotificationToastData { TapToDismiss = false });
            SubscribeCounters();
            _toast.HandleTap();
            Assert.AreEqual(0, _tappedCount, "OnTapped must NOT fire when TapToDismiss=false.");
            Assert.AreEqual(0, _dismissedCount);
            Assert.IsFalse(_toast.IsDismissing);
        }

        [Test]
        public void Tap_During_Dismiss_Is_Ignored()
        {
            _toast.Bind(new NotificationToastData());
            SubscribeCounters();
            _toast.DismissNow();
            _toast.HandleTap();
            Assert.AreEqual(0, _tappedCount, "Tap after DismissNow must NOT fire OnTapped.");
            Assert.AreEqual(1, _dismissedCount, "OnDismissed must fire only once.");
        }

        [Test]
        public void SeverityToColor_Mapping_Is_Correct()
        {
            var theme = BuildTheme();
            Assert.AreEqual(theme.PrimaryColor, NotificationToast.SeverityToColor(ToastSeverity.Info, theme));
            Assert.AreEqual(theme.SuccessColor, NotificationToast.SeverityToColor(ToastSeverity.Success, theme));
            Assert.AreEqual(theme.WarningColor, NotificationToast.SeverityToColor(ToastSeverity.Warning, theme));
            Assert.AreEqual(theme.DangerColor, NotificationToast.SeverityToColor(ToastSeverity.Error, theme));
            Object.DestroyImmediate(theme);
        }

        [Test]
        public void SeverityToIcon_Mapping_Is_Correct()
        {
            var theme = BuildTheme();
            var info = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            var check = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            var warn = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            var err = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            SetPrivateField(theme, "_iconInfo", info);
            SetPrivateField(theme, "_iconCheck", check);
            SetPrivateField(theme, "_iconWarning", warn);
            SetPrivateField(theme, "_iconError", err);
            Assert.AreSame(info, NotificationToast.SeverityToIcon(ToastSeverity.Info, theme));
            Assert.AreSame(check, NotificationToast.SeverityToIcon(ToastSeverity.Success, theme));
            Assert.AreSame(warn, NotificationToast.SeverityToIcon(ToastSeverity.Warning, theme));
            Assert.AreSame(err, NotificationToast.SeverityToIcon(ToastSeverity.Error, theme));
            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(info);
            Object.DestroyImmediate(check);
            Object.DestroyImmediate(warn);
            Object.DestroyImmediate(err);
        }

        [Test]
        public void SeverityToCue_Mapping_Is_Correct()
        {
            Assert.AreEqual(UIAudioCue.Notification, NotificationToast.SeverityToCue(ToastSeverity.Info));
            Assert.AreEqual(UIAudioCue.Success, NotificationToast.SeverityToCue(ToastSeverity.Success));
            Assert.AreEqual(UIAudioCue.Notification, NotificationToast.SeverityToCue(ToastSeverity.Warning));
            Assert.AreEqual(UIAudioCue.Error, NotificationToast.SeverityToCue(ToastSeverity.Error));
        }

        [Test]
        public void Rebind_With_Different_Severity_Does_Not_Throw()
        {
            _toast.Bind(new NotificationToastData { Severity = ToastSeverity.Info });
            Assert.DoesNotThrow(() => _toast.Bind(new NotificationToastData { Severity = ToastSeverity.Error }));
        }

        private static UIThemeConfig BuildTheme()
        {
            return ScriptableObject.CreateInstance<UIThemeConfig>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
