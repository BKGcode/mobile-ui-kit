using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.HUD
{
    public enum HUDTimerSimpleMode
    {
        Countdown = 0,
        Stopwatch = 1,
    }

    [DisallowMultipleComponent]
    public sealed class HUDTimerSimple : MonoBehaviour
    {
        [Tooltip("TMP label rendering the formatted time. Required.")]
        [SerializeField] private TMP_Text _label;
        [Tooltip("Optional clock icon Image (themed via ThemedImage at prefab time). May be null.")]
        [SerializeField] private Image _iconImage;
        [Tooltip("Optional button covering the HUD. Click triggers _onClickEvent.")]
        [SerializeField] private Button _clickButton;

        [Tooltip("Mode applied on OnEnable. Countdown ticks down to 0; Stopwatch ticks up from 0.")]
        [SerializeField] private HUDTimerSimpleMode _mode = HUDTimerSimpleMode.Countdown;
        [Tooltip("Initial duration in seconds (Countdown only). 0 = waits for SetCountdownSeconds at runtime.")]
        [SerializeField] private float _initialCountdownSeconds = 60f;
        [Tooltip("True = autoplay on enable. False = enable paused; call Resume() to start.")]
        [SerializeField] private bool _autoplay = true;
        [Tooltip("True = uses Time.time (respects timeScale). False = uses Time.unscaledTime.")]
        [SerializeField] private bool _pausesWithTimeScale = false;
        [Tooltip("TimeSpan format string. Default 'mm\\:ss'. Use 'mm\\:ss\\.ff' for ms precision.")]
        [SerializeField] private string _formatString = "mm\\:ss";
        [Tooltip("Label shown when Countdown reaches zero.")]
        [SerializeField] private string _expiredLabel = "00:00";
        [Tooltip("True = hide the label on expiry. False = show _expiredLabel.")]
        [SerializeField] private bool _hideOnExpire = false;
        [Tooltip("Color flashed on the label on Countdown expiry. Set alpha = 0 to disable.")]
        [SerializeField] private Color _expiryFlashColor = new Color(0.30f, 0.78f, 0.45f, 1f);
        [Tooltip("Duration of the expiry flash tween (in + out combined).")]
        [SerializeField] private float _expiryFlashDuration = 0.30f;

        [SerializeField] private UnityEvent _onTimerClickEvent;

        public event Action OnExpired;

        private float _remainingSeconds;
        private float _elapsedSeconds;
        private bool _isRunning;
        private bool _isExpired;
        private bool _hasInitialized;
        private bool _formatValidated;
        private bool _labelColorCaptured;
        private Color _labelOriginalColor = Color.white;
        private Tween _expiryTween;
        private float _lastRealTime;

        public bool IsRunning => _isRunning;
        public bool IsExpired => _isExpired;
        public float RemainingSeconds => _remainingSeconds;
        public float ElapsedSeconds => _elapsedSeconds;

        public void SetCountdownSeconds(float seconds)
        {
            _mode = HUDTimerSimpleMode.Countdown;
            _remainingSeconds = Mathf.Max(0f, seconds);
            _elapsedSeconds = 0f;
            _isExpired = false;
            _hasInitialized = true;
            UpdateLabel();
        }

        public void ResetStopwatch()
        {
            _mode = HUDTimerSimpleMode.Stopwatch;
            _elapsedSeconds = 0f;
            _remainingSeconds = 0f;
            _isExpired = false;
            _hasInitialized = true;
            UpdateLabel();
        }

        public void Pause() => _isRunning = false;

        public void Resume()
        {
            if (_isExpired) return;
            _isRunning = true;
            _lastRealTime = CurrentRealTime();
        }

        private void OnEnable()
        {
            CaptureLabelColorOnce();
            ValidateFormatOnce();
            if (_clickButton != null) _clickButton.onClick.AddListener(HandleClick);
            if (!_hasInitialized) InitializeFromInspector();
            else UpdateLabel();
            _lastRealTime = CurrentRealTime();
            _isRunning = _autoplay && !_isExpired;
        }

        private void OnDisable()
        {
            if (_clickButton != null) _clickButton.onClick.RemoveListener(HandleClick);
            KillExpiryTween();
            _isRunning = false;
        }

        private void OnDestroy()
        {
            KillExpiryTween();
        }

        private void Update()
        {
            if (!_isRunning) return;
            var now = CurrentRealTime();
            var delta = now - _lastRealTime;
            _lastRealTime = now;
            if (delta <= 0f) return;
            if (_mode == HUDTimerSimpleMode.Countdown) TickCountdown(delta);
            else TickStopwatch(delta);
        }

        private void InitializeFromInspector()
        {
            _hasInitialized = true;
            if (_mode == HUDTimerSimpleMode.Countdown) _remainingSeconds = _initialCountdownSeconds;
            else _elapsedSeconds = 0f;
            _isExpired = false;
            UpdateLabel();
        }

        private void TickCountdown(float delta)
        {
            _remainingSeconds = Mathf.Max(0f, _remainingSeconds - delta);
            UpdateLabel();
            if (_remainingSeconds <= 0f) HandleExpiry();
        }

        private void TickStopwatch(float delta)
        {
            _elapsedSeconds += delta;
            UpdateLabel();
        }

        private void HandleExpiry()
        {
            _isExpired = true;
            _isRunning = false;
            if (_hideOnExpire) HideLabel();
            else SetLabelText(_expiredLabel);
            PlayExpiryFlash();
            OnExpired?.Invoke();
        }

        private void UpdateLabel()
        {
            var span = _mode == HUDTimerSimpleMode.Countdown
                ? TimeSpan.FromSeconds(_remainingSeconds)
                : TimeSpan.FromSeconds(_elapsedSeconds);
            SetLabelText(FormatTime(span));
        }

        private void PlayExpiryFlash()
        {
            if (_label == null) return;
            if (_expiryFlashColor.a <= 0f) return;
            KillExpiryTween();
            var half = Mathf.Max(_expiryFlashDuration * 0.5f, 0.01f);
            _expiryTween = DOTween.Sequence()
                .Append(_label.DOColor(_expiryFlashColor, half).SetEase(Ease.OutQuad))
                .Append(_label.DOColor(_labelOriginalColor, half).SetEase(Ease.InQuad))
                .SetLink(gameObject);
        }

        private void CaptureLabelColorOnce()
        {
            if (_labelColorCaptured) return;
            if (_label == null) return;
            _labelOriginalColor = _label.color;
            _labelColorCaptured = true;
        }

        private void ValidateFormatOnce()
        {
            if (_formatValidated) return;
            try { TimeSpan.Zero.ToString(_formatString); }
            catch (FormatException)
            {
                Debug.LogError($"[HUDTimerSimple] _formatString '{_formatString}' is not a valid TimeSpan format. Falling back to 'mm\\:ss'.", this);
                _formatString = "mm\\:ss";
            }
            _formatValidated = true;
        }

        private string FormatTime(TimeSpan span)
        {
            try { return span.ToString(_formatString); }
            catch (FormatException) { return span.ToString("mm\\:ss"); }
        }

        private void SetLabelText(string text)
        {
            if (_label == null) return;
            if (!_label.gameObject.activeSelf) _label.gameObject.SetActive(true);
            _label.SetText(text);
        }

        private void HideLabel()
        {
            if (_label == null) return;
            _label.gameObject.SetActive(false);
        }

        private float CurrentRealTime()
        {
            return _pausesWithTimeScale ? Time.time : Time.unscaledTime;
        }

        private void HandleClick()
        {
            _onTimerClickEvent?.Invoke();
        }

        private void KillExpiryTween()
        {
            _expiryTween?.Kill();
            _expiryTween = null;
        }
    }
}
