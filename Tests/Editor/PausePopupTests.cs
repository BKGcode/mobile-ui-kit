using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Catalog.Pause;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class PausePopupTests
    {
        private GameObject _go;
        private GameObject _servicesGo;
        private PausePopup _popup;
        private UIThemeConfig _theme;
        private UIAnimPreset _preset;
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
            _theme = ScriptableObject.CreateInstance<UIThemeConfig>();
            _preset = ScriptableObject.CreateInstance<UIAnimPreset>();
            var so = new SerializedObject(_theme);
            so.FindProperty("_defaultAnimPreset").objectReferenceValue = _preset;
            so.ApplyModifiedPropertiesWithoutUndo();
            _servicesGo = new GameObject("PausePopup_Services");
            var services = _servicesGo.AddComponent<UIServices>();
            _popup.Initialize(_theme, services);
            _resumeCount = 0;
            _dismissedCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_servicesGo != null) Object.DestroyImmediate(_servicesGo);
            if (_theme != null) Object.DestroyImmediate(_theme);
            if (_preset != null) Object.DestroyImmediate(_preset);
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

        [Test]
        public void OnShow_Fires_OnPaused_Event_Once()
        {
            var pausedCount = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnPaused += () => pausedCount++;
            _popup.OnShow();
            Assert.AreEqual(1, pausedCount, "OnPaused must fire exactly once on OnShow.");
        }

        [Test]
        public void Resume_Fires_OnResumed_Event_Once()
        {
            var resumedCount = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnResumed += () => resumedCount++;
            _popup.OnShow();
            _popup.OnBackPressed();
            Assert.AreEqual(1, resumedCount, "OnResumed must fire exactly once after resume.");
        }

        [Test]
        public void Restart_Click_Fires_Event_And_Dismisses()
        {
            var count = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnRestart += () => count++;
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleRestart();
            Assert.AreEqual(1, count, "OnRestart must fire once.");
            Assert.AreEqual(1, _dismissedCount, "Dismiss must follow Restart.");
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Home_Click_Fires_Event_And_Dismisses()
        {
            var count = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnHome += () => count++;
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleHome();
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, _dismissedCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Quit_Click_Fires_Event_And_Dismisses()
        {
            var count = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnQuit += () => count++;
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleQuit();
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, _dismissedCount);
            Assert.IsTrue(_popup.IsDismissing);
        }

        [Test]
        public void Settings_Click_Fires_Event_And_Stays_Open()
        {
            var count = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnSettings += () => count++;
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleSettings();
            Assert.AreEqual(1, count, "OnSettings must fire once.");
            Assert.AreEqual(0, _dismissedCount, "Settings is a shortcut — popup must stay open.");
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Shop_Click_Fires_Event_And_Stays_Open()
        {
            var count = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnShop += () => count++;
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleShop();
            Assert.AreEqual(1, count);
            Assert.AreEqual(0, _dismissedCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Help_Click_Fires_Event_And_Stays_Open()
        {
            var count = 0;
            _popup.Bind(new PausePopupData());
            _popup.OnHelp += () => count++;
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleHelp();
            Assert.AreEqual(1, count);
            Assert.AreEqual(0, _dismissedCount);
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Sound_Toggle_Mutates_Data_And_Fires_Event_And_Stays_Open()
        {
            var data = new PausePopupData { SoundOn = true };
            var lastValue = true;
            var count = 0;
            _popup.Bind(data);
            _popup.OnSoundChanged += v => { count++; lastValue = v; };
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleSoundChanged(false);
            Assert.AreEqual(1, count, "OnSoundChanged must fire once.");
            Assert.IsFalse(lastValue, "Event payload must reflect new toggle state.");
            Assert.IsFalse(data.SoundOn, "DTO field must be mutated to match.");
            Assert.AreEqual(0, _dismissedCount, "Toggle must NOT dismiss the popup.");
        }

        [Test]
        public void Music_Toggle_Mutates_Data_And_Fires_Event_And_Stays_Open()
        {
            var data = new PausePopupData { MusicOn = true };
            var lastValue = true;
            var count = 0;
            _popup.Bind(data);
            _popup.OnMusicChanged += v => { count++; lastValue = v; };
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleMusicChanged(false);
            Assert.AreEqual(1, count);
            Assert.IsFalse(lastValue);
            Assert.IsFalse(data.MusicOn);
            Assert.AreEqual(0, _dismissedCount);
        }

        [Test]
        public void Vibration_Toggle_Mutates_Data_And_Fires_Event_And_Stays_Open()
        {
            var data = new PausePopupData { VibrationOn = true };
            var lastValue = true;
            var count = 0;
            _popup.Bind(data);
            _popup.OnVibrationChanged += v => { count++; lastValue = v; };
            _popup.OnDismissed += () => _dismissedCount++;
            _popup.HandleVibrationChanged(false);
            Assert.AreEqual(1, count);
            Assert.IsFalse(lastValue);
            Assert.IsFalse(data.VibrationOn);
            Assert.AreEqual(0, _dismissedCount);
        }
    }
}
