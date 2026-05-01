using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Tutorial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimTutorialPopup))]
    public sealed class TutorialPopup : UIModule<TutorialPopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Step title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Step body/description TMP label.")]
            public TMP_Text BodyLabel;
            [Tooltip("Optional step illustration. Hidden when step.Image is null.")]
            public Image StepImage;
            [Tooltip("Optional progress label (e.g. \"1 / 4\").")]
            public TMP_Text ProgressLabel;

            [Header("Navigation buttons")]
            [Tooltip("Advances to next step. Becomes \"Done\" on the last step.")]
            public Button NextButton;
            [Tooltip("Returns to previous step. Hidden on first step.")]
            public Button PreviousButton;
            [Tooltip("Skips the tutorial entirely. Hidden when ShowSkip=false.")]
            public Button SkipButton;
            [Tooltip("Full-screen backdrop button. Behaviour driven by CloseOnBackdrop / TapToAdvance.")]
            public Button BackdropButton;

            [Header("Button labels (TMP children)")]
            public TMP_Text NextLabel;
            public TMP_Text PreviousLabel;
            public TMP_Text SkipLabel;
        }

        [SerializeField] private Refs _refs;

        public event Action<int> OnStepChanged;
        public event Action<int> OnNext;
        public event Action<int> OnPrevious;
        public event Action OnSkip;
        public event Action OnCompleted;
        public event Action OnDismissed;

        public int CurrentIndex { get; private set; }
        public int StepCount => _data?.Steps?.Count ?? 0;
        public bool IsFirstStep => CurrentIndex <= 0;
        public bool IsLastStep => StepCount > 0 && CurrentIndex >= StepCount - 1;

        private TutorialPopupData _data;
        private IUIAnimator _animator;
        private bool _themeWarningLogged;

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        internal void SetAnimatorForTests(IUIAnimator animator)
        {
            _animator = animator;
        }

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            SubscribeButtons();
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
            ClearAllEvents();
        }

        public override void Bind(TutorialPopupData data)
        {
            ClearAllEvents();
            _data = data ?? new TutorialPopupData();
            CurrentIndex = ClampIndex(_data.StartIndex);
            ApplyStaticLabels(_data);
            ApplyStep(CurrentIndex);
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            if (Theme == null && !_themeWarningLogged)
            {
                _themeWarningLogged = true;
                Debug.LogWarning("[TutorialPopup] Theme not initialized — animation/audio will not apply. Spawn via PopupManager.", this);
            }
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            if (Animator == null) return;
            Animator.ApplyPreset(ResolvePreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            HandleSkip();
        }

        public void GoNext()
        {
            if (IsDismissing) return;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            if (IsLastStep)
            {
                if (_data != null && _data.LoopBackToFirst)
                {
                    SetIndex(0);
                    OnNext?.Invoke(CurrentIndex);
                    return;
                }
                CompleteAndDismiss();
                return;
            }
            SetIndex(CurrentIndex + 1);
            OnNext?.Invoke(CurrentIndex);
        }

        public void GoPrevious()
        {
            if (IsDismissing || IsFirstStep) return;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            SetIndex(CurrentIndex - 1);
            OnPrevious?.Invoke(CurrentIndex);
        }

        public void SkipTutorial() => HandleSkip();

        public void CompleteTutorial() => CompleteAndDismiss();

        private void HandleSkip()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnSkip?.Invoke();
            DismissWithAnimation();
        }

        private void CompleteAndDismiss()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            OnCompleted?.Invoke();
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null) return;
            if (_data.TapToAdvance) { GoNext(); return; }
            if (_data.CloseOnBackdrop) HandleSkip();
        }

        private void DismissWithAnimation()
        {
            Services?.Audio?.Play(UIAudioCue.PopupClose);
            if (Animator == null) { FinalizeDismissal(); return; }
            Animator.PlayHide(FinalizeDismissal);
        }

        private void FinalizeDismissal()
        {
            RaiseDismissRequested();
            OnDismissed?.Invoke();
        }

        private void SetIndex(int index)
        {
            CurrentIndex = ClampIndex(index);
            ApplyStep(CurrentIndex);
            OnStepChanged?.Invoke(CurrentIndex);
        }

        private int ClampIndex(int index)
        {
            if (StepCount == 0) return 0;
            if (index < 0) return 0;
            if (index >= StepCount) return StepCount - 1;
            return index;
        }

        private void ApplyStaticLabels(TutorialPopupData data)
        {
            if (_refs.PreviousLabel != null) _refs.PreviousLabel.SetText(data.PreviousLabel);
            if (_refs.SkipLabel != null) _refs.SkipLabel.SetText(data.SkipLabel);
            SetActive(_refs.SkipButton, data.ShowSkip);
        }

        private void ApplyStep(int index)
        {
            var step = (StepCount > 0) ? _data.Steps[index] : null;
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(step?.Title ?? string.Empty);
            if (_refs.BodyLabel != null) _refs.BodyLabel.SetText(step?.Body ?? string.Empty);
            ApplyStepImage(step);
            ApplyProgress(index);
            ApplyNextLabel();
            ApplyPreviousVisibility();
        }

        private void ApplyStepImage(TutorialStep step)
        {
            if (_refs.StepImage == null) return;
            var hasImage = step != null && step.Image != null;
            _refs.StepImage.gameObject.SetActive(hasImage);
            if (hasImage) _refs.StepImage.sprite = step.Image;
        }

        private void ApplyProgress(int index)
        {
            if (_refs.ProgressLabel == null) return;
            if (StepCount <= 1) { _refs.ProgressLabel.SetText(string.Empty); return; }
            _refs.ProgressLabel.SetText($"{index + 1} / {StepCount}");
        }

        private void ApplyNextLabel()
        {
            if (_refs.NextLabel == null || _data == null) return;
            var label = (IsLastStep && !_data.LoopBackToFirst) ? _data.DoneLabel : _data.NextLabel;
            _refs.NextLabel.SetText(label);
        }

        private void ApplyPreviousVisibility()
        {
            if (_data == null) return;
            var visible = _data.ShowPrevious && !IsFirstStep;
            SetActive(_refs.PreviousButton, visible);
        }

        private void ClearAllEvents()
        {
            OnStepChanged = null;
            OnNext = null;
            OnPrevious = null;
            OnSkip = null;
            OnCompleted = null;
            OnDismissed = null;
        }

        private static void SetActive(Component c, bool active)
        {
            if (c != null) c.gameObject.SetActive(active);
        }

        private void SubscribeButtons()
        {
            AddListener(_refs.NextButton, GoNext);
            AddListener(_refs.PreviousButton, GoPrevious);
            AddListener(_refs.SkipButton, HandleSkip);
            AddListener(_refs.BackdropButton, HandleBackdrop);
        }

        private void UnsubscribeButtons()
        {
            RemoveListener(_refs.NextButton, GoNext);
            RemoveListener(_refs.PreviousButton, GoPrevious);
            RemoveListener(_refs.SkipButton, HandleSkip);
            RemoveListener(_refs.BackdropButton, HandleBackdrop);
        }

        private static void AddListener(Button b, UnityEngine.Events.UnityAction call)
        {
            if (b != null) b.onClick.AddListener(call);
        }

        private static void RemoveListener(Button b, UnityEngine.Events.UnityAction call)
        {
            if (b != null) b.onClick.RemoveListener(call);
        }

        private UIAnimPreset ResolvePreset()
        {
            if (Theme == null || Theme.AnimPresetLibrary == null) return null;
            var style = AnimStyleOverride ?? Theme.DefaultAnimStyle;
            return Theme.AnimPresetLibrary.Resolve(style);
        }
    }
}
