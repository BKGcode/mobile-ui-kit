using System;
using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers;
using KitforgeLabs.MobileUIKit.Services;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests
{
    [TestFixture]
    public class HUDTimerTests
    {
        private GameObject _hudGo;
        private GameObject _servicesGo;
        private UIServices _services;
        private FakeTimeService _time;
        private TMP_Text _label;

        [SetUp]
        public void SetUp()
        {
            _servicesGo = new GameObject("Services_Test");
            _services = _servicesGo.AddComponent<UIServices>();
            _time = new FakeTimeService();
            _services.SetTime(_time);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hudGo != null) UnityEngine.Object.DestroyImmediate(_hudGo);
            if (_servicesGo != null) UnityEngine.Object.DestroyImmediate(_servicesGo);
        }

        private HUDTimer MakeTimer(TimerMode mode)
        {
            _hudGo = new GameObject($"HUDTimer_Test_{mode}");
            _hudGo.SetActive(false);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(_hudGo.transform);
            _label = labelGo.AddComponent<TextMeshProUGUI>();

            var hud = _hudGo.AddComponent<HUDTimer>();
            hud.SetModeForTests(mode);
            hud.SetLabelForTests(_label);
            hud.SetServicesForTestsExposed(_services);
            return hud;
        }

        [Test]
        public void Countdown_Initial_Refresh_Computes_Remaining_From_Target()
        {
            var hud = MakeTimer(TimerMode.CountdownToTarget);
            hud.ForceInitForTests();
            hud.SetTarget(_time.GetServerTimeUtc().AddMinutes(5));
            Assert.AreEqual("05:00", hud.LabelTextForTests);
            Assert.IsFalse(hud.IsExpiredForTests);
        }

        [Test]
        public void Countdown_Tick_Decrements_Remaining()
        {
            var hud = MakeTimer(TimerMode.CountdownToTarget);
            hud.ForceInitForTests();
            hud.SetTarget(_time.GetServerTimeUtc().AddMinutes(5));
            _time.Advance(TimeSpan.FromSeconds(30));
            hud.ForceTickForTests();
            Assert.AreEqual("04:30", hud.LabelTextForTests);
        }

        [Test]
        public void Countdown_Expired_Fires_Event_Once_And_Stops_Ticking()
        {
            var hud = MakeTimer(TimerMode.CountdownToTarget);
            hud.ForceInitForTests();
            var fireCount = 0;
            hud.OnExpired += () => fireCount++;
            hud.SetTarget(_time.GetServerTimeUtc().AddSeconds(-1));
            Assert.AreEqual(1, fireCount, "OnExpired must fire exactly once when remaining crosses zero.");
            Assert.AreEqual("00:00", hud.LabelTextForTests, "Default _expiredLabel is '00:00'.");
            Assert.IsTrue(hud.IsExpiredForTests);

            _time.Advance(TimeSpan.FromSeconds(10));
            hud.ForceTickForTests();
            Assert.AreEqual(1, fireCount, "OnExpired must NOT fire again on subsequent ticks within the same cycle.");
        }

        [Test]
        public void Countdown_Warning_Threshold_Fires_Event_Once_When_Crossed()
        {
            var hud = MakeTimer(TimerMode.CountdownToTarget);
            var fireCount = 0;
            hud.OnWarningEntered += () => fireCount++;
            hud.SetWarningThresholdForTests(10f);
            hud.ForceInitForTests();
            hud.SetTarget(_time.GetServerTimeUtc().AddSeconds(15));
            Assert.AreEqual(0, fireCount, "Initial 15s remaining > threshold 10s — warning must NOT fire.");
            Assert.IsFalse(hud.IsInWarningForTests);

            _time.Advance(TimeSpan.FromSeconds(6));
            hud.ForceTickForTests();
            Assert.AreEqual(1, fireCount, "Remaining 9s < threshold 10s — warning must fire once.");
            Assert.IsTrue(hud.IsInWarningForTests);

            hud.ForceTickForTests();
            Assert.AreEqual(1, fireCount, "Re-tick within warning zone must NOT re-fire OnWarningEntered.");
        }

        [Test]
        public void Countdown_Hide_On_Expire_Hides_Label()
        {
            var hud = MakeTimer(TimerMode.CountdownToTarget);
            hud.SetHideOnExpireForTests(true);
            hud.ForceInitForTests();
            hud.SetTarget(_time.GetServerTimeUtc().AddSeconds(-1));
            Assert.IsFalse(hud.LabelVisibleForTests, "_hideOnExpire=true must hide the label on expiry (T5).");
        }

        [Test]
        public void Countup_Initial_Refresh_Computes_Elapsed()
        {
            var hud = MakeTimer(TimerMode.CountupSinceTarget);
            hud.ForceInitForTests();
            hud.SetTarget(_time.GetServerTimeUtc().AddSeconds(-30));
            Assert.AreEqual("00:30", hud.LabelTextForTests);
        }

        [Test]
        public void Stopwatch_OnEnable_Starts_From_Zero()
        {
            var clock = new[] { 100f };
            var hud = MakeTimer(TimerMode.LocalStopwatch);
            hud.RealTimeProviderForTests = () => clock[0];
            hud.SetFormatStringForTests("mm\\:ss\\.ff");
            hud.ForceInitForTests();
            Assert.AreEqual("00:00.00", hud.LabelTextForTests);
        }

        [Test]
        public void Stopwatch_SetPaused_Freezes_Counter()
        {
            var clock = new[] { 0f };
            var hud = MakeTimer(TimerMode.LocalStopwatch);
            hud.RealTimeProviderForTests = () => clock[0];
            hud.SetFormatStringForTests("mm\\:ss");
            hud.ForceInitForTests();

            clock[0] = 5f;
            hud.ForceTickForTests();
            Assert.AreEqual("00:05", hud.LabelTextForTests, "Stopwatch should read 5s after 5s of real time.");

            hud.SetPaused(true);
            clock[0] = 10f;
            hud.ForceTickForTests();
            Assert.AreEqual("00:05", hud.LabelTextForTests, "While paused, advancing real time must NOT change the stopwatch reading.");

            hud.SetPaused(false);
            clock[0] = 13f;
            hud.ForceTickForTests();
            Assert.AreEqual("00:08", hud.LabelTextForTests, "After unpause, stopwatch resumes from accumulated 5s + 3s post-resume = 8s.");
        }

        [Test]
        public void Stopwatch_Reset_Sets_To_Zero()
        {
            var clock = new[] { 0f };
            var hud = MakeTimer(TimerMode.LocalStopwatch);
            hud.RealTimeProviderForTests = () => clock[0];
            hud.SetFormatStringForTests("mm\\:ss");
            hud.ForceInitForTests();

            clock[0] = 10f;
            hud.ForceTickForTests();
            Assert.AreEqual("00:10", hud.LabelTextForTests);

            hud.Reset();
            Assert.AreEqual("00:00", hud.LabelTextForTests, "Reset() returns the stopwatch to 00:00 from any running state.");
        }

        [Test]
        public void Mode_UTC_Without_TimeService_Shows_Dashes_Silently()
        {
            var hud = MakeTimer(TimerMode.CountdownToTarget);
            _services.SetTime(null);
            hud.ForceInitForTests();
            hud.SetTarget(new DateTime(2026, 1, 15, 12, 5, 0, DateTimeKind.Utc));
            Assert.AreEqual("--:--", hud.LabelTextForTests, "UTC mode with null ITimeService must show '--:--' silently per § 4.5 (drift fix vs § Edge cases).");
        }
    }
}
