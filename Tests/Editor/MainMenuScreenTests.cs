using KitforgeLabs.MobileUIKit.Catalog.Screens;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class MainMenuScreenTests
    {
        private GameObject _go;
        private GameObject _servicesGo;
        private MainMenuScreen _screen;
        private UIServices _services;
        private FakeProgressionService _progression;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            _services = _servicesGo.AddComponent<UIServices>();
            _progression = new FakeProgressionService();
            _services.SetProgression(_progression);

            _go = new GameObject("MainMenuScreen_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnim_MainMenuScreen>();
            _screen = _go.AddComponent<MainMenuScreen>();
            _screen.SetAnimatorForTests(new NullAnimator());
            _screen.Initialize(null, _services);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_servicesGo != null) Object.DestroyImmediate(_servicesGo);
        }

        [Test]
        public void DTO_Defaults_Are_Correct()
        {
            var data = new MainMenuScreenData();
            Assert.AreEqual("", data.Title);
            Assert.IsTrue(data.ShowPlayButton);
            Assert.IsTrue(data.ShowSettingsButton);
            Assert.IsTrue(data.ShowShopButton);
            Assert.IsTrue(data.ShowDailyButton);
        }

        [Test]
        public void Bind_Null_Falls_Back_To_Defaults()
        {
            Assert.DoesNotThrow(() => _screen.Bind(null));
        }

        [Test]
        public void Bind_Hides_Play_Button_When_ShowPlayButton_False()
        {
            var btn = MakeButton("Play");
            _screen.SetRefsForTests(null, null, btn, null, null, null, null);
            _screen.Bind(new MainMenuScreenData { ShowPlayButton = false });
            Assert.IsFalse(btn.gameObject.activeSelf);
        }

        [Test]
        public void Bind_Hides_Settings_Button_When_ShowSettingsButton_False()
        {
            var btn = MakeButton("Settings");
            _screen.SetRefsForTests(null, null, null, btn, null, null, null);
            _screen.Bind(new MainMenuScreenData { ShowSettingsButton = false });
            Assert.IsFalse(btn.gameObject.activeSelf);
        }

        [Test]
        public void Bind_Hides_Shop_Button_When_ShowShopButton_False()
        {
            var btn = MakeButton("Shop");
            _screen.SetRefsForTests(null, null, null, null, btn, null, null);
            _screen.Bind(new MainMenuScreenData { ShowShopButton = false });
            Assert.IsFalse(btn.gameObject.activeSelf);
        }

        [Test]
        public void Bind_Hides_Daily_Button_When_ShowDailyButton_False()
        {
            var btn = MakeButton("Daily");
            _screen.SetRefsForTests(null, null, null, null, null, btn, null);
            _screen.Bind(new MainMenuScreenData { ShowDailyButton = false });
            Assert.IsFalse(btn.gameObject.activeSelf);
        }

        [Test]
        public void Bind_Shows_TitleLabel_When_Title_NonEmpty()
        {
            var label = MakeLabel();
            _screen.SetRefsForTests(label, null, null, null, null, null, null);
            _screen.Bind(new MainMenuScreenData { Title = "My Game" });
            Assert.IsTrue(label.gameObject.activeSelf);
            Assert.AreEqual("My Game", label.text);
        }

        [Test]
        public void Bind_Hides_TitleLabel_When_Title_Empty()
        {
            var label = MakeLabel();
            _screen.SetRefsForTests(label, null, null, null, null, null, null);
            _screen.Bind(new MainMenuScreenData { Title = "" });
            Assert.IsFalse(label.gameObject.activeSelf);
        }

        [Test]
        public void Bind_Clears_Event_Subscriptions()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var staleCount = 0;
            _screen.OnPlayRequested += () => staleCount++;

            _screen.OnHide();
            _screen.Bind(null);
            _screen.OnShow();
            _screen.InvokePlayForTests();

            Assert.AreEqual(0, staleCount);
        }

        [Test]
        public void Play_Button_Fires_OnPlayRequested()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var fired = false;
            _screen.OnPlayRequested += () => fired = true;
            _screen.InvokePlayForTests();
            Assert.IsTrue(fired);
        }

        [Test]
        public void Settings_Button_Fires_OnSettingsRequested()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var fired = false;
            _screen.OnSettingsRequested += () => fired = true;
            _screen.InvokeSettingsForTests();
            Assert.IsTrue(fired);
        }

        [Test]
        public void ShowDailyButton_False_Hides_Dot_Regardless_Of_Progression()
        {
            var dot = new GameObject("Dot");
            dot.transform.SetParent(_go.transform);
            _screen.SetRefsForTests(null, null, null, null, null, null, dot);
            _progression.SetDailyLoginState(new DailyLoginState { AlreadyClaimedToday = false });

            _screen.Bind(new MainMenuScreenData { ShowDailyButton = false });
            _screen.OnShow();

            Assert.IsFalse(dot.activeSelf);
        }

        [Test]
        public void Null_IProgressionService_Hides_Dot_Silently()
        {
            _services.SetProgression(null);
            var dot = new GameObject("Dot");
            dot.transform.SetParent(_go.transform);
            _screen.SetRefsForTests(null, null, null, null, null, null, dot);

            _screen.Bind(null);
            Assert.DoesNotThrow(() => _screen.OnShow());
            Assert.IsFalse(dot.activeSelf);
        }

        [Test]
        public void AlreadyClaimedToday_False_Shows_Dot()
        {
            _progression.SetDailyLoginState(new DailyLoginState { AlreadyClaimedToday = false });
            var dot = new GameObject("Dot");
            dot.transform.SetParent(_go.transform);
            _screen.SetRefsForTests(null, null, null, null, null, null, dot);

            _screen.Bind(new MainMenuScreenData { ShowDailyButton = true });
            _screen.OnShow();

            Assert.IsTrue(dot.activeSelf);
        }

        [Test]
        public void AlreadyClaimedToday_True_Hides_Dot()
        {
            _progression.SetDailyLoginState(new DailyLoginState { AlreadyClaimedToday = true });
            var dot = new GameObject("Dot");
            dot.transform.SetParent(_go.transform);
            _screen.SetRefsForTests(null, null, null, null, null, null, dot);

            _screen.Bind(new MainMenuScreenData { ShowDailyButton = true });
            _screen.OnShow();

            Assert.IsFalse(dot.activeSelf);
        }

        [Test]
        public void OnBackPressed_Fires_OnBackRequested()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var fired = false;
            _screen.OnBackRequested += () => fired = true;
            _screen.OnBackPressed();
            Assert.IsTrue(fired);
        }

        [Test]
        public void OnBackPressed_While_Hidden_Is_NoOp()
        {
            _screen.Bind(null);
            var fired = false;
            _screen.OnBackRequested += () => fired = true;
            _screen.OnBackPressed();
            Assert.IsFalse(fired);
        }

        private Button MakeButton(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_go.transform);
            go.AddComponent<RectTransform>();
            return go.AddComponent<Button>();
        }

        private TMP_Text MakeLabel()
        {
            var go = new GameObject("Label");
            go.transform.SetParent(_go.transform);
            return go.AddComponent<TextMeshProUGUI>();
        }
    }
}
