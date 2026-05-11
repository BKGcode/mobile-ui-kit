using System;
using DG.Tweening;
using KitforgeLabs.UIKit.Animation;
using KitforgeLabs.UIKit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.UIKit.Catalog.Screens
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnim_LoadingScreen))]
    public sealed class LoadingScreen : UIModule<LoadingScreenData>
    {
        [Serializable]
        private struct LoadingRefs
        {
            [Tooltip("Top TMP label. Hidden when Title is empty.")]
            public TMP_Text TitleLabel;
            [Tooltip("Secondary TMP label. Hidden when Subtitle is empty.")]
            public TMP_Text SubtitleLabel;
            [Tooltip("Progress bar fill Image (type Filled). Hidden when ShowProgressBar = false.")]
            public Image ProgressBarFill;
            [Tooltip("Progress bar track Image. Hidden with the fill when ShowProgressBar = false.")]
            public Image ProgressBarTrack;
            [Tooltip("Spinner Image. Rotates continuously while showing. Hidden when ShowSpinner = false.")]
            public Image SpinnerImage;
        }

        [SerializeField] private LoadingRefs _refs;
        [Tooltip("Duration of the progress bar fill tween in seconds.")]
        [SerializeField] private float _progressTweenDuration = 0.3f;
        [Tooltip("False = progress bar snaps instantly instead of tweening.")]
        [SerializeField] private bool _animateProgress = true;
        [Tooltip("Spinner rotation speed in revolutions per minute.")]
        [SerializeField] private float _spinnerRpm = 120f;

        private IUIAnimator _animator;
        private Tween _progressTween;
        private Tween _spinnerTween;
        private float _currentProgress;
        private bool _progressCompleteFired;
        private bool _minTimeFired;
        private float _showTime;
        private float _minDisplaySeconds;
        private bool _isShowing;
        private LoadingScreenData _data;

        public event Action OnProgressComplete;
        public event Action OnMinDisplayTimeElapsed;

        internal Func<float> RealTimeProviderForTests { get; set; }
        internal float CurrentProgressForTests => _currentProgress;
        internal bool IsShowingForTests => _isShowing;
        internal void SetAnimatorForTests(IUIAnimator animator) => _animator = animator;
        internal void ForceTickForTests() => TickMinDisplayCheck();
        internal void SetRefsForTests(TMP_Text title, TMP_Text subtitle, Image fill, Image track, Image spinner)
        {
            _refs.TitleLabel = title;
            _refs.SubtitleLabel = subtitle;
            _refs.ProgressBarFill = fill;
            _refs.ProgressBarTrack = track;
            _refs.SpinnerImage = spinner;
        }

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
        }

        private void Update()
        {
            TickMinDisplayCheck();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        public override void Bind(LoadingScreenData data)
        {
            _data = data ?? new LoadingScreenData();
            _progressCompleteFired = false;
            _minTimeFired = false;
            _minDisplaySeconds = Mathf.Max(_data.MinDisplaySeconds, 0f);
            OnProgressComplete = null;
            OnMinDisplayTimeElapsed = null;
            ApplyDataToRefs(_data);
            SnapProgressBar(_data.InitialProgress);
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            _isShowing = true;
            _showTime = CurrentRealTime();
            StartSpinner();
            Animator?.PlayShow();
        }

        public override void OnHide()
        {
            _isShowing = false;
            KillTweens();
            Animator?.Skip();
        }

        public override void OnBackPressed() { }

        public void SetProgress(float progress)
        {
            if (!_isShowing) return;
            progress = Mathf.Clamp01(progress);
            _currentProgress = progress;
            AnimateProgressBar(progress);
            if (progress >= 1f && !_progressCompleteFired)
            {
                _progressCompleteFired = true;
                OnProgressComplete?.Invoke();
            }
        }

        public void SetLoadingText(string title, string subtitle)
        {
            SetLabel(_refs.TitleLabel, title);
            SetLabel(_refs.SubtitleLabel, subtitle);
        }

        private void ApplyDataToRefs(LoadingScreenData data)
        {
            SetLoadingText(data.Title, data.Subtitle);
            SetBarVisible(data.ShowProgressBar);
            if (_refs.SpinnerImage != null)
                _refs.SpinnerImage.gameObject.SetActive(data.ShowSpinner);
        }

        private void SetBarVisible(bool visible)
        {
            if (_refs.ProgressBarFill != null) _refs.ProgressBarFill.gameObject.SetActive(visible);
            if (_refs.ProgressBarTrack != null) _refs.ProgressBarTrack.gameObject.SetActive(visible);
        }

        private static void SetLabel(TMP_Text label, string text)
        {
            if (label == null) return;
            var show = !string.IsNullOrEmpty(text);
            label.gameObject.SetActive(show);
            if (show) label.SetText(text);
        }

        private void SnapProgressBar(float progress)
        {
            _currentProgress = Mathf.Clamp01(progress);
            if (_refs.ProgressBarFill != null) _refs.ProgressBarFill.fillAmount = _currentProgress;
        }

        private void AnimateProgressBar(float target)
        {
            var fill = _refs.ProgressBarFill;
            if (fill == null) return;
            if (!_animateProgress) { fill.fillAmount = target; return; }
            _progressTween?.Kill();
            _progressTween = fill.DOFillAmount(target, _progressTweenDuration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject);
        }

        private void StartSpinner()
        {
            var spinner = _refs.SpinnerImage;
            if (spinner == null || !spinner.gameObject.activeSelf) return;
            _spinnerTween?.Kill();
            _spinnerTween = spinner.rectTransform
                .DORotate(new Vector3(0f, 0f, -360f), 60f / _spinnerRpm, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);
        }

        private void TickMinDisplayCheck()
        {
            if (_minTimeFired || _minDisplaySeconds <= 0f || !_isShowing) return;
            if (CurrentRealTime() - _showTime < _minDisplaySeconds) return;
            _minTimeFired = true;
            OnMinDisplayTimeElapsed?.Invoke();
        }

        private void KillTweens()
        {
            _progressTween?.Kill();
            _progressTween = null;
            _spinnerTween?.Kill();
            _spinnerTween = null;
        }

        private float CurrentRealTime()
        {
            if (RealTimeProviderForTests != null) return RealTimeProviderForTests();
            return Time.unscaledTime;
        }
    }
}
