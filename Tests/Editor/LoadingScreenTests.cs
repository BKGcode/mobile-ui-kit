using KitforgeLabs.MobileUIKit.Catalog.Screens;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class LoadingScreenTests
    {
        private GameObject _go;
        private LoadingScreen _screen;
        private float _fakeTime;

        [SetUp]
        public void SetUp()
        {
            _fakeTime = 0f;
            _go = new GameObject("LoadingScreen_Test");
            _go.AddComponent<CanvasGroup>();
            _go.AddComponent<UIAnim_LoadingScreen>();
            _screen = _go.AddComponent<LoadingScreen>();
            _screen.SetAnimatorForTests(new NullAnimator());
            _screen.RealTimeProviderForTests = () => _fakeTime;
            _screen.Initialize(null, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void DTO_Defaults_Are_Correct()
        {
            var data = new LoadingScreenData();
            Assert.AreEqual("Loading...", data.Title);
            Assert.AreEqual("", data.Subtitle);
            Assert.AreEqual(0f, data.InitialProgress);
            Assert.IsTrue(data.ShowProgressBar);
            Assert.IsTrue(data.ShowSpinner);
            Assert.AreEqual(0f, data.MinDisplaySeconds);
        }

        [Test]
        public void Bind_Null_Falls_Back_To_Defaults()
        {
            _screen.Bind(null);
            Assert.AreEqual(0f, _screen.CurrentProgressForTests);
        }

        [Test]
        public void Bind_Sets_InitialProgress_On_Bar()
        {
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(_go.transform);
            var fill = fillGo.AddComponent<Image>();
            _screen.SetRefsForTests(null, null, fill, null, null);

            _screen.Bind(new LoadingScreenData { InitialProgress = 0.6f });

            Assert.AreEqual(0.6f, fill.fillAmount, 0.001f);
            Assert.AreEqual(0.6f, _screen.CurrentProgressForTests, 0.001f);
        }

        [Test]
        public void SetProgress_Updates_Internal_Value()
        {
            _screen.Bind(null);
            _screen.OnShow();
            _screen.SetProgress(0.5f);
            Assert.AreEqual(0.5f, _screen.CurrentProgressForTests, 0.001f);
        }

        [Test]
        public void SetProgress_One_Fires_OnProgressComplete()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var fired = false;
            _screen.OnProgressComplete += () => fired = true;
            _screen.SetProgress(1f);
            Assert.IsTrue(fired);
        }

        [Test]
        public void SetProgress_One_Twice_Does_Not_Refire_OnProgressComplete()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var count = 0;
            _screen.OnProgressComplete += () => count++;
            _screen.SetProgress(1f);
            _screen.SetProgress(1f);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Bind_Clears_OnProgressComplete_Subscription()
        {
            _screen.Bind(null);
            _screen.OnShow();
            var staleCount = 0;
            _screen.OnProgressComplete += () => staleCount++;

            _screen.OnHide();
            _screen.Bind(null);
            _screen.OnShow();

            _screen.SetProgress(1f);
            Assert.AreEqual(0, staleCount);
        }

        [Test]
        public void MinDisplaySeconds_Zero_Does_Not_Fire_OnMinDisplayTimeElapsed()
        {
            _screen.Bind(new LoadingScreenData { MinDisplaySeconds = 0f });
            _screen.OnShow();
            var fired = false;
            _screen.OnMinDisplayTimeElapsed += () => fired = true;
            _fakeTime = 999f;
            _screen.ForceTickForTests();
            Assert.IsFalse(fired);
        }

        [Test]
        public void MinDisplaySeconds_Fires_After_Elapsed_Time()
        {
            _screen.Bind(new LoadingScreenData { MinDisplaySeconds = 2f });
            _screen.OnShow();
            var fired = false;
            _screen.OnMinDisplayTimeElapsed += () => fired = true;

            _fakeTime = 1f;
            _screen.ForceTickForTests();
            Assert.IsFalse(fired);

            _fakeTime = 2.1f;
            _screen.ForceTickForTests();
            Assert.IsTrue(fired);
        }

        [Test]
        public void MinDisplaySeconds_Fires_At_Most_Once()
        {
            _screen.Bind(new LoadingScreenData { MinDisplaySeconds = 1f });
            _screen.OnShow();
            var count = 0;
            _screen.OnMinDisplayTimeElapsed += () => count++;
            _fakeTime = 1.5f;
            _screen.ForceTickForTests();
            _screen.ForceTickForTests();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SetProgress_While_Hidden_Is_Ignored()
        {
            _screen.Bind(null);
            _screen.OnShow();
            _screen.OnHide();
            Assert.DoesNotThrow(() => _screen.SetProgress(0.8f));
            Assert.AreEqual(0f, _screen.CurrentProgressForTests, 0.001f);
        }

        [Test]
        public void OnBackPressed_Is_NoOp()
        {
            _screen.Bind(null);
            _screen.OnShow();
            Assert.DoesNotThrow(() => _screen.OnBackPressed());
            Assert.IsTrue(_screen.IsShowingForTests);
        }

        [Test]
        public void ShowProgressBar_False_Hides_Bar_Elements()
        {
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(_go.transform);
            var fill = fillGo.AddComponent<Image>();
            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(_go.transform);
            var track = trackGo.AddComponent<Image>();
            _screen.SetRefsForTests(null, null, fill, track, null);

            _screen.Bind(new LoadingScreenData { ShowProgressBar = false });

            Assert.IsFalse(fill.gameObject.activeSelf);
            Assert.IsFalse(track.gameObject.activeSelf);
        }

        [Test]
        public void ShowSpinner_False_Hides_Spinner()
        {
            var spinnerGo = new GameObject("Spinner");
            spinnerGo.transform.SetParent(_go.transform);
            var spinner = spinnerGo.AddComponent<Image>();
            _screen.SetRefsForTests(null, null, null, null, spinner);

            _screen.Bind(new LoadingScreenData { ShowSpinner = false });

            Assert.IsFalse(spinner.gameObject.activeSelf);
        }

        [Test]
        public void SetLoadingText_Hides_Labels_When_Empty()
        {
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(_go.transform);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            _screen.SetRefsForTests(title, null, null, null, null);
            _screen.Bind(new LoadingScreenData { Title = "Go" });
            Assert.IsTrue(title.gameObject.activeSelf);

            _screen.SetLoadingText("", null);
            Assert.IsFalse(title.gameObject.activeSelf);
        }

    }
}
