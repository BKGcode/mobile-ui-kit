using System.Text.RegularExpressions;
using KitforgeLabs.MobileUIKit.Catalog.Settings;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class SettingsPopupTests
    {
        private GameObject _go;
        private GameObject _servicesGo;
        private SettingsPopup _popup;
        private UIServices _services;
        private FakePlayerDataService _playerData;
        private FakeLocalizationService _localization;
        private UIThemeConfig _theme;

        private static readonly LanguageOption[] DemoLanguages =
        {
            new LanguageOption { Code = "en", DisplayName = "English" },
            new LanguageOption { Code = "es", DisplayName = "Español" },
            new LanguageOption { Code = "fr", DisplayName = "Français" }
        };

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("SettingsPopup_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnimSettingsPopup>();
            _popup = _go.AddComponent<SettingsPopup>();
            _popup.SetAnimatorForTests(new NullAnimator());

            _theme = ScriptableObject.CreateInstance<UIThemeConfig>();
            _servicesGo = new GameObject("SettingsPopup_Services");
            _services = _servicesGo.AddComponent<UIServices>();
            _playerData = new FakePlayerDataService();
            _localization = new FakeLocalizationService("en", new[] { "en", "es", "fr" });
            _services.SetPlayerData(_playerData);
            _services.SetLocalization(_localization);
            _popup.Initialize(_theme, _services);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_servicesGo != null) Object.DestroyImmediate(_servicesGo);
            if (_theme != null) Object.DestroyImmediate(_theme);
        }

        [Test]
        public void DTO_Defaults_Match_Spec()
        {
            var data = new SettingsPopupData();
            Assert.AreEqual("Settings", data.Title);
            Assert.AreEqual("Close", data.CloseLabel);
            Assert.IsTrue(data.ShowMusicSlider);
            Assert.IsTrue(data.ShowSfxSlider);
            Assert.IsTrue(data.ShowLanguagePicker);
            Assert.IsTrue(data.ShowNotificationsToggle);
            Assert.IsTrue(data.ShowHapticsToggle);
            Assert.IsNull(data.LanguageOptions);
        }

        [Test]
        public void Bind_With_Null_Data_Uses_Defaults()
        {
            _popup.Bind(null);
            Assert.IsNotNull(_popup.DataForTests);
            Assert.AreEqual("Settings", _popup.DataForTests.Title);
        }

        [Test]
        public void Bind_Sets_IsDismissing_False()
        {
            _popup.Bind(new SettingsPopupData());
            Assert.IsFalse(_popup.IsDismissing);
        }

        [Test]
        public void Music_Volume_Change_Writes_PlayerData_And_Raises_Event()
        {
            _popup.Bind(new SettingsPopupData());
            var captured = -1f;
            _popup.OnMusicVolumeChanged += v => captured = v;
            _popup.InvokeMusicChangedForTests(0.42f);
            Assert.AreEqual(0.42f, _playerData.GetFloat(SettingsPopup.KeyMusicVolume));
            Assert.AreEqual(0.42f, captured);
        }

        [Test]
        public void Sfx_Volume_Change_Writes_PlayerData_And_Raises_Event()
        {
            _popup.Bind(new SettingsPopupData());
            var captured = -1f;
            _popup.OnSfxVolumeChanged += v => captured = v;
            _popup.InvokeSfxChangedForTests(0.33f);
            Assert.AreEqual(0.33f, _playerData.GetFloat(SettingsPopup.KeySfxVolume));
            Assert.AreEqual(0.33f, captured);
        }

        [Test]
        public void Notifications_Change_Writes_PlayerData_And_Raises_Event()
        {
            _popup.Bind(new SettingsPopupData());
            bool? captured = null;
            _popup.OnNotificationsChanged += v => captured = v;
            _popup.InvokeNotificationsChangedForTests(false);
            Assert.IsFalse(_playerData.GetBool(SettingsPopup.KeyNotifications, true));
            Assert.AreEqual(false, captured);
        }

        [Test]
        public void Haptics_Change_Writes_PlayerData_And_Raises_Event()
        {
            _popup.Bind(new SettingsPopupData());
            bool? captured = null;
            _popup.OnHapticsChanged += v => captured = v;
            _popup.InvokeHapticsChangedForTests(false);
            Assert.IsFalse(_playerData.GetBool(SettingsPopup.KeyHaptics, true));
            Assert.AreEqual(false, captured);
        }

        [Test]
        public void Language_Change_Writes_PlayerData_And_Calls_Localization_And_Raises_Event()
        {
            _popup.Bind(new SettingsPopupData { LanguageOptions = DemoLanguages });
            string captured = null;
            _popup.OnLanguageChanged += code => captured = code;
            _popup.InvokeLanguageChangedForTests(1);
            Assert.AreEqual("es", _playerData.GetString(SettingsPopup.KeyLanguage));
            Assert.AreEqual("es", _localization.CurrentLanguage);
            Assert.AreEqual("es", captured);
        }

        [Test]
        public void Music_Change_During_Dismiss_Is_Ignored()
        {
            _popup.Bind(new SettingsPopupData());
            var raised = false;
            _popup.OnMusicVolumeChanged += _ => raised = true;
            _popup.InvokeCloseForTests();
            _popup.InvokeMusicChangedForTests(0.5f);
            Assert.IsFalse(raised);
            Assert.IsFalse(_playerData.Has(SettingsPopup.KeyMusicVolume));
        }

        [Test]
        public void Sfx_Change_During_Dismiss_Is_Ignored()
        {
            _popup.Bind(new SettingsPopupData());
            var raised = false;
            _popup.OnSfxVolumeChanged += _ => raised = true;
            _popup.InvokeCloseForTests();
            _popup.InvokeSfxChangedForTests(0.5f);
            Assert.IsFalse(raised);
            Assert.IsFalse(_playerData.Has(SettingsPopup.KeySfxVolume));
        }

        [Test]
        public void Haptics_Change_During_Dismiss_Is_Ignored()
        {
            _popup.Bind(new SettingsPopupData());
            var raised = false;
            _popup.OnHapticsChanged += _ => raised = true;
            _popup.InvokeCloseForTests();
            _popup.InvokeHapticsChangedForTests(false);
            Assert.IsFalse(raised);
            Assert.IsFalse(_playerData.Has(SettingsPopup.KeyHaptics));
        }

        [Test]
        public void Language_Change_During_Dismiss_Is_Ignored()
        {
            _popup.Bind(new SettingsPopupData { LanguageOptions = DemoLanguages });
            var raised = false;
            _popup.OnLanguageChanged += _ => raised = true;
            _popup.InvokeCloseForTests();
            _popup.InvokeLanguageChangedForTests(2);
            Assert.IsFalse(raised);
            Assert.AreEqual("en", _localization.CurrentLanguage);
        }

        [Test]
        public void OnHide_Calls_PlayerData_Save()
        {
            _popup.Bind(new SettingsPopupData());
            Assert.AreEqual(0, _playerData.SaveCallCount);
            _popup.OnHide();
            Assert.AreEqual(1, _playerData.SaveCallCount);
        }

        [Test]
        public void Re_Bind_Clears_Event_Subscriptions()
        {
            _popup.Bind(new SettingsPopupData());
            var fireCount = 0;
            _popup.OnMusicVolumeChanged += _ => fireCount++;
            _popup.InvokeMusicChangedForTests(0.5f);
            Assert.AreEqual(1, fireCount);

            _popup.Bind(new SettingsPopupData());
            _popup.InvokeMusicChangedForTests(0.7f);
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void Close_Sets_IsDismissing_And_Fires_OnDismissed()
        {
            _popup.Bind(new SettingsPopupData());
            var dismissedCount = 0;
            _popup.OnDismissed += () => dismissedCount++;
            _popup.InvokeCloseForTests();
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, dismissedCount);
        }

        [Test]
        public void Close_While_Dismissing_Is_Ignored()
        {
            _popup.Bind(new SettingsPopupData());
            var dismissedCount = 0;
            _popup.OnDismissed += () => dismissedCount++;
            _popup.InvokeCloseForTests();
            _popup.InvokeCloseForTests();
            Assert.AreEqual(1, dismissedCount);
        }

        [Test]
        public void Backdrop_Triggers_Close_Behavior()
        {
            _popup.Bind(new SettingsPopupData());
            var dismissedCount = 0;
            _popup.OnDismissed += () => dismissedCount++;
            _popup.InvokeBackdropForTests();
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, dismissedCount);
        }

        [Test]
        public void OnBackPressed_Triggers_Close_Behavior()
        {
            _popup.Bind(new SettingsPopupData());
            var dismissedCount = 0;
            _popup.OnDismissed += () => dismissedCount++;
            _popup.OnBackPressed();
            Assert.IsTrue(_popup.IsDismissing);
            Assert.AreEqual(1, dismissedCount);
        }

        [Test]
        public void Saved_Language_Not_In_Options_Falls_Back_With_Warning()
        {
            _playerData.SetString(SettingsPopup.KeyLanguage, "de");
            LogAssert.Expect(LogType.Warning, new Regex("Saved language 'de' not in LanguageOptions"));
            _popup.Bind(new SettingsPopupData { LanguageOptions = DemoLanguages });
            string captured = null;
            _popup.OnLanguageChanged += code => captured = code;
            _popup.InvokeLanguageChangedForTests(0);
            Assert.AreEqual("en", captured);
        }
    }
}
