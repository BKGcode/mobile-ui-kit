using KitforgeLabs.MobileUIKit.Catalog.Pause;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class PausePopupTests
    {
        private GameObject _go;
        private PausePopup _popup;
        private float _initialTimeScale;
        private int _resumeCount;
        private int _dismissedCount;

        [SetUp]
        public void SetUp()
        {
            _initialTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            _go = new GameObject("PausePopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimPausePopup>();
            _popup = _go.AddComponent<PausePopup>();
            _resumeCount = 0;
            _dismissedCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            Time.timeScale = _initialTimeScale;
        }

        private void SubscribeCounters()
        {
            _popup.OnResume += () => _resumeCount++;
            _popup.OnDismissed += () => _dismissedCount++;
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsFalse(_popup.IsDismissing);
            Assert.IsFalse(_popup.IsPaused);
        }

        [Test]
        public void Back_Press_Triggers_Resume_Once_And_Dismisses()
        {
            _popup.Bind(new PausePopupData());
            SubscribeCounters();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _resumeCount, "Resume must fire once on back press.");
            Assert.AreEqual(1, _dismissedCount, "Dismiss must fire once on back press.");
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Back_Press_While_Dismissing_Is_Ignored()
        {
            _popup.Bind(new PausePopupData());
            SubscribeCounters();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            _popup.OnBackPressed();
            Assert.AreEqual(1, _resumeCount, "Subsequent back presses must be ignored once dismissing.");
        }

        [Test]
        public void OnShow_Pauses_TimeScale_And_Resume_Restores()
        {
            _popup.Bind(new PausePopupData());
            _popup.OnShow();
            Assert.AreEqual(0f, Time.timeScale, 0.0001f, "OnShow must set Time.timeScale to pause value.");
            Assert.IsTrue(_popup.IsPaused);
            _popup.OnBackPressed();
            Assert.AreEqual(1f, Time.timeScale, 0.0001f, "Resume must restore the original timeScale.");
            Assert.IsFalse(_popup.IsPaused);
        }

        [Test]
        public void Resume_Restores_Original_TimeScale_Not_Hardcoded_One()
        {
            Time.timeScale = 0.5f;
            _popup.Bind(new PausePopupData());
            _popup.OnShow();
            Assert.AreEqual(0f, Time.timeScale, 0.0001f);
            _popup.OnBackPressed();
            Assert.AreEqual(0.5f, Time.timeScale, 0.0001f, "Resume must restore the captured value, not hardcode 1.");
        }

        [Test]
        public void Bind_Resets_Event_Listeners()
        {
            _popup.Bind(new PausePopupData());
            SubscribeCounters();
            _popup.OnBackPressed();
            var beforeCount = _resumeCount;
            _popup.Bind(new PausePopupData());
            _popup.OnBackPressed();
            Assert.AreEqual(beforeCount, _resumeCount, "Listeners attached before re-Bind must NOT receive events after re-Bind.");
        }
    }
}
