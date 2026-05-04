using System;
using System.Globalization;
using DG.Tweening;
using KitforgeLabs.MobileUIKit.HUD;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.HUD
{
    /// <summary>
    /// Live timer label HUD with three modes (<see cref="TimerMode"/>): countdown to a UTC target,
    /// countup since a UTC start, or local stopwatch driven by <c>Time.unscaledTime</c>. Mode is
    /// immutable post-OnEnable per FQ6 — buyers swap modes via disable→set→enable cycle. Null
    /// service behavior follows CATALOG_GroupC_DELTA § 4.5 (HUD = silent degrade — UTC modes
    /// without an <see cref="ITimeService"/> render <c>"--:--"</c> with no LogError). Local
    /// <c>Update()</c> shim is a temporary workaround for <c>UIModuleBase.OnUpdate()</c> infra
    /// dispatch gap; M3 sweep removes it (anchor comment grep).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HUDTimer : UIHUDBase
    {
        private enum ExpiryStyle
        {
            Success = 0,
            Failure = 1,
        }

        [Serializable]
        private struct TimerRefs
        {
            [Tooltip("TMP label rendering the formatted time. Required.")]
            public TMP_Text Label;
            [Tooltip("Optional clock icon Image (themed via ThemedImage IconClock slot at prefab time).")]
            public Image IconImage;
            [Tooltip("Optional button covering the HUD. Click triggers _onTimerClickEvent.")]
            public Button ClickButton;
        }

        [Tooltip("Timer mode. Read once on OnEnable — runtime changes are UNDEFINED. Buyer swaps modes via disable → assign new mode → enable cycle.")]
        [SerializeField] private TimerMode _mode = TimerMode.CountdownToTarget;

        [SerializeField] private TimerRefs _refs;

        [Tooltip("Tick rate in Hz. UTC modes typically 1Hz (sub-second precision is meaningless when computed from server time). LocalStopwatch typically 30Hz for millisecond display. Buyer tunes per prefab.")]
        [SerializeField] private float _tickRateHz = 1f;

        [Tooltip("TimeSpan format string. Default 'mm\\:ss'. Stopwatch typically uses 'mm\\:ss\\.ff'. Buyer can localize.")]
        [SerializeField] private string _formatString = "mm\\:ss";

        [Tooltip("Optional ISO 8601 UTC string parsed in OnEnable. Sets _targetUtc statically for UTC modes. Use SetTarget(DateTime) for runtime targets. Empty = use SetTarget at runtime.")]
        [SerializeField] private string _targetUtcIso = "";

        [Tooltip("Label shown when CountdownToTarget reaches zero (when _hideOnExpire is false).")]
        [SerializeField] private string _expiredLabel = "00:00";

        [Tooltip("True = hide the label on expiry. False = show _expiredLabel. CountdownToTarget mode only.")]
        [SerializeField] private bool _hideOnExpire = false;

        [Tooltip("Seconds remaining at which the warning pulse fires (CountdownToTarget only). 0 = disabled.")]
        [SerializeField] private float _warningThresholdSeconds = 0f;

        [Tooltip("LocalStopwatch only: true = stopwatch respects Time.timeScale (uses Time.time). False = uses Time.unscaledTime. Ignored in UTC modes.")]
        [SerializeField] private bool _pausesWithTimeScale = false;

        [Tooltip("Color flashed on the label when expiring. Success = positive (level cleared), Failure = negative (game over).")]
        [SerializeField] private ExpiryStyle _expiryStyle = ExpiryStyle.Success;

        [Tooltip("Color the label tints to during warning zone (CountdownToTarget only).")]
        [SerializeField] private Color _warningColor = new Color(1.00f, 0.60f, 0.15f, 1f);

        [Tooltip("Color the label flashes to on expiry when _expiryStyle = Success.")]
        [SerializeField] private Color _expirySuccessColor = new Color(0.30f, 0.78f, 0.45f, 1f);

        [Tooltip("Color the label flashes to on expiry when _expiryStyle = Failure.")]
        [SerializeField] private Color _expiryFailureColor = new Color(0.898f, 0.224f, 0.208f, 1f);

        [Tooltip("Duration of one half-cycle of the warning pulse YoYo loop.")]
        [SerializeField] private float _warningTweenDuration = 0.40f;

        [Tooltip("Duration of the expiry flash tween (in + out combined).")]
        [SerializeField] private float _expiryFlashDuration = 0.30f;

        [SerializeField] private UnityEvent _onTimerClickEvent;

        public event Action OnExpired;
        public event Action OnWarningEntered;

        private DateTime _targetUtc = DateTime.MinValue;
        private float _stopwatchAccumulated;
        private float _stopwatchStartRealTime;
        private bool _isPaused;
        private bool _isExpired;
        private bool _isInWarning;
        private bool _formatValidated;
        private bool _labelColorCaptured;
        private Color _labelOriginalColor = Color.white;
        private Tween _warningTween;
        private Tween _expiryTween;
        private float _lastTickTime;

        internal Func<float> RealTimeProviderForTests { get; set; }
        internal string LabelTextForTests => _refs.Label == null ? null : _refs.Label.text;
        internal bool LabelVisibleForTests => _refs.Label != null && _refs.Label.gameObject.activeSelf;
        internal bool IsExpiredForTests => _isExpired;
        internal bool IsInWarningForTests => _isInWarning;
        internal DateTime TargetUtcForTests => _targetUtc;
        internal void SetModeForTests(TimerMode mode) => _mode = mode;
        internal void SetTargetUtcIsoForTests(string iso) => _targetUtcIso = iso;
        internal void SetWarningThresholdForTests(float seconds) => _warningThresholdSeconds = seconds;
        internal void SetHideOnExpireForTests(bool hide) => _hideOnExpire = hide;
        internal void SetFormatStringForTests(string format) => _formatString = format;
        internal void SetServicesForTestsExposed(UIServices services) => SetServicesInternal(services);
        internal void SetLabelForTests(TMP_Text label) => _refs.Label = label;
        internal void ForceInitForTests()
        {
            CaptureLabelColorOnce();
            ValidateFormatOnce();
            InitializeForCurrentMode();
            Tick(force: true);
        }
        internal void ForceTickForTests() => Tick(force: true);

        protected override void OnEnable()
        {
            CaptureLabelColorOnce();
            ValidateFormatOnce();
            InitializeForCurrentMode();
            if (_refs.ClickButton != null) _refs.ClickButton.onClick.AddListener(HandleClick);
            if (Services == null && _mode == TimerMode.LocalStopwatch)
            {
                Tick(force: true);
                return;
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_refs.ClickButton != null) _refs.ClickButton.onClick.RemoveListener(HandleClick);
            KillAllTweens();
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        // OnUpdate-workaround-M3-sweep
        private void Update()
        {
            OnUpdate();
        }

        private void OnUpdate()
        {
            var now = CurrentRealTime();
            var minInterval = 1f / Mathf.Max(_tickRateHz, 0.001f);
            if (now - _lastTickTime < minInterval) return;
            _lastTickTime = now;
            Tick(force: false);
        }

        protected override void Subscribe() { }
        protected override void Unsubscribe() { }
        protected override void Refresh()
        {
            Tick(force: true);
        }

        public void SetTarget(DateTime utc)
        {
            _targetUtc = utc;
            _isExpired = false;
            ResetTransientFlags();
            Tick(force: true);
        }

        public void Reset()
        {
            _stopwatchAccumulated = 0f;
            _stopwatchStartRealTime = CurrentRealTime();
            _isPaused = false;
            _isExpired = false;
            ResetTransientFlags();
            Tick(force: true);
        }

        public void SetPaused(bool paused)
        {
            if (_mode != TimerMode.LocalStopwatch) return;
            if (paused == _isPaused) return;
            if (paused)
            {
                _stopwatchAccumulated += CurrentRealTime() - _stopwatchStartRealTime;
                _isPaused = true;
                return;
            }
            _stopwatchStartRealTime = CurrentRealTime();
            _isPaused = false;
        }

        private void ResetTransientFlags()
        {
            if (_isInWarning) ExitWarningZone();
            KillExpiryTween();
        }

        private void Tick(bool force)
        {
            if (_isExpired && !force) return;
            switch (_mode)
            {
                case TimerMode.CountdownToTarget: TickCountdown(); break;
                case TimerMode.CountupSinceTarget: TickCountup(); break;
                case TimerMode.LocalStopwatch: TickStopwatch(); break;
            }
        }

        private void TickCountdown()
        {
            var time = Services?.Time;
            if (time == null) { ApplyDashesSilently(); return; }
            var remaining = _targetUtc - time.GetServerTimeUtc();
            if (remaining.Ticks <= 0)
            {
                if (!_isExpired) HandleExpiry();
                return;
            }
            UpdateLabel(FormatTime(remaining));
            EvaluateWarning(remaining);
        }

        private void TickCountup()
        {
            var time = Services?.Time;
            if (time == null) { ApplyDashesSilently(); return; }
            var elapsed = time.GetServerTimeUtc() - _targetUtc;
            if (elapsed.Ticks < 0) elapsed = TimeSpan.Zero;
            UpdateLabel(FormatTime(elapsed));
        }

        private void TickStopwatch()
        {
            var elapsed = TimeSpan.FromSeconds(GetStopwatchElapsed());
            UpdateLabel(FormatTime(elapsed));
        }

        private float GetStopwatchElapsed()
        {
            if (_isPaused) return _stopwatchAccumulated;
            return _stopwatchAccumulated + (CurrentRealTime() - _stopwatchStartRealTime);
        }

        private void HandleExpiry()
        {
            _isExpired = true;
            KillWarningTween();
            if (_hideOnExpire) HideLabel();
            else UpdateLabel(_expiredLabel);
            PlayExpiryFlash();
            Services?.Audio?.Play(UIAudioCue.Success);
            OnExpired?.Invoke();
        }

        private void EvaluateWarning(TimeSpan remaining)
        {
            if (_warningThresholdSeconds <= 0f) return;
            var isInZone = remaining.TotalSeconds <= _warningThresholdSeconds;
            if (isInZone && !_isInWarning) EnterWarningZone();
            else if (!isInZone && _isInWarning) ExitWarningZone();
        }

        private void EnterWarningZone()
        {
            _isInWarning = true;
            SetLabelColor(_warningColor);
            StartWarningTween();
            Services?.Audio?.Play(UIAudioCue.Error);
            OnWarningEntered?.Invoke();
        }

        private void ExitWarningZone()
        {
            _isInWarning = false;
            KillWarningTween();
            SetLabelColor(_labelOriginalColor);
        }

        private void StartWarningTween()
        {
            var label = _refs.Label;
            if (label == null) return;
            KillWarningTween();
            _warningTween = label.DOColor(_labelOriginalColor, _warningTweenDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);
        }

        private void PlayExpiryFlash()
        {
            var label = _refs.Label;
            if (label == null) return;
            KillExpiryTween();
            var color = _expiryStyle == ExpiryStyle.Failure ? _expiryFailureColor : _expirySuccessColor;
            var half = Mathf.Max(_expiryFlashDuration * 0.5f, 0.01f);
            _expiryTween = DOTween.Sequence()
                .Append(label.DOColor(color, half).SetEase(Ease.OutQuad))
                .Append(label.DOColor(_labelOriginalColor, half).SetEase(Ease.InQuad))
                .SetLink(gameObject);
        }

        private void InitializeForCurrentMode()
        {
            _isExpired = false;
            _isInWarning = false;
            _lastTickTime = float.NegativeInfinity;
            if (_mode == TimerMode.LocalStopwatch)
            {
                if (!_isPaused) _stopwatchStartRealTime = CurrentRealTime();
                return;
            }
            ParseTargetUtcIso();
        }

        private void ParseTargetUtcIso()
        {
            if (string.IsNullOrEmpty(_targetUtcIso)) return;
            const DateTimeStyles styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
            if (DateTime.TryParse(_targetUtcIso, CultureInfo.InvariantCulture, styles, out var parsed))
            {
                _targetUtc = parsed;
                return;
            }
            Debug.LogError($"[HUDTimer] _targetUtcIso '{_targetUtcIso}' is not a valid ISO 8601 UTC date. Falling back to DateTime.MinValue.", this);
        }

        private void ValidateFormatOnce()
        {
            if (_formatValidated) return;
            try { TimeSpan.Zero.ToString(_formatString); }
            catch (FormatException)
            {
                Debug.LogError($"[HUDTimer] _formatString '{_formatString}' is not a valid TimeSpan format. Falling back to 'mm\\:ss'.", this);
                _formatString = "mm\\:ss";
            }
            _formatValidated = true;
        }

        private void CaptureLabelColorOnce()
        {
            if (_labelColorCaptured) return;
            if (_refs.Label == null) return;
            _labelOriginalColor = _refs.Label.color;
            _labelColorCaptured = true;
        }

        private string FormatTime(TimeSpan span)
        {
            try { return span.ToString(_formatString); }
            catch (FormatException) { return span.ToString("mm\\:ss"); }
        }

        private void UpdateLabel(string text)
        {
            var label = _refs.Label;
            if (label == null) return;
            if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);
            label.SetText(text);
        }

        private void HideLabel()
        {
            if (_refs.Label == null) return;
            _refs.Label.gameObject.SetActive(false);
        }

        private void SetLabelColor(Color color)
        {
            if (_refs.Label == null) return;
            _refs.Label.color = color;
        }

        private void ApplyDashesSilently()
        {
            UpdateLabel("--:--");
        }

        private float CurrentRealTime()
        {
            if (RealTimeProviderForTests != null) return RealTimeProviderForTests();
            return _pausesWithTimeScale ? Time.time : Time.unscaledTime;
        }

        private void HandleClick()
        {
            _onTimerClickEvent?.Invoke();
        }

        private void KillAllTweens()
        {
            KillWarningTween();
            KillExpiryTween();
        }

        private void KillWarningTween()
        {
            _warningTween?.Kill();
            _warningTween = null;
        }

        private void KillExpiryTween()
        {
            _expiryTween?.Kill();
            _expiryTween = null;
        }
    }
}
