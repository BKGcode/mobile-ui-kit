using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using KitforgeLabs.MobileUIKit.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Toasts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimNotificationToast))]
    public sealed class NotificationToast : UIToast<NotificationToastData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Body TMP label. Toasts have no title.")]
            public TMP_Text MessageLabel;
            [Tooltip("Image whose color changes by Severity (Info=Primary, Success=Success, Warning=Accent, Error=Danger).")]
            public Image SeverityTint;
            [Tooltip("Optional severity icon. Sprite is swapped from Theme by Severity.")]
            public Image SeverityIcon;
            [Tooltip("Optional full-area button used when TapToDismiss = true. Leave null to disable tap.")]
            public Button TapArea;
        }

        [SerializeField] private Refs _refs;
        [Tooltip("Seconds the toast stays on screen when NotificationToastData.DurationOverride is <= 0.")]
        [SerializeField] private float _defaultDuration = 3f;

        public event Action OnTapped;
        public event Action OnDismissed;

        public override float DefaultDuration => _data?.DurationOverride > 0f ? _data.DurationOverride : _defaultDuration;

        private NotificationToastData _data;
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
            if (_refs.TapArea != null) _refs.TapArea.onClick.AddListener(HandleTap);
        }

        private void OnDestroy()
        {
            if (_refs.TapArea != null) _refs.TapArea.onClick.RemoveListener(HandleTap);
            ClearAllEvents();
        }

        public override void Bind(NotificationToastData data)
        {
            ClearAllEvents();
            _data = data ?? new NotificationToastData();
            ApplyMessage(_data);
            ApplySeverity(_data.Severity);
            ApplyTapArea(_data.TapToDismiss);
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            if (Theme == null && !_themeWarningLogged)
            {
                _themeWarningLogged = true;
                Debug.LogWarning("[NotificationToast] Theme not initialized — colors/audio will not apply. Spawn via ToastManager.", this);
            }
            Services?.Audio?.Play(SeverityToCue(_data.Severity));
            if (Animator == null) return;
            Animator.ApplyPreset(ResolveAnimPreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            Animator?.Skip();
        }

        public void DismissNow()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            DismissWithAnimation();
        }

        internal void HandleTap()
        {
            if (IsDismissing || _data == null || !_data.TapToDismiss) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnTapped?.Invoke();
            DismissWithAnimation();
        }

        private void DismissWithAnimation()
        {
            if (Animator == null) { FinalizeDismissal(); return; }
            Animator.PlayHide(FinalizeDismissal);
        }

        private void FinalizeDismissal()
        {
            RaiseDismissRequested();
            OnDismissed?.Invoke();
        }

        private void ClearAllEvents()
        {
            OnTapped = null;
            OnDismissed = null;
        }

        private void ApplyMessage(NotificationToastData data)
        {
            if (_refs.MessageLabel != null) _refs.MessageLabel.SetText(data.Message);
        }

        private void ApplySeverity(ToastSeverity severity)
        {
            if (Theme == null) return;
            if (_refs.SeverityTint != null) _refs.SeverityTint.color = SeverityToColor(severity, Theme);
            if (_refs.SeverityIcon != null)
            {
                var icon = SeverityToIcon(severity, Theme);
                if (icon != null) _refs.SeverityIcon.sprite = icon;
                _refs.SeverityIcon.gameObject.SetActive(icon != null);
            }
        }

        private void ApplyTapArea(bool tapToDismiss)
        {
            if (_refs.TapArea == null) return;
            _refs.TapArea.interactable = tapToDismiss;
        }

        internal static Color SeverityToColor(ToastSeverity severity, UIThemeConfig theme)
        {
            switch (severity)
            {
                case ToastSeverity.Success: return theme.SuccessColor;
                case ToastSeverity.Warning: return theme.WarningColor;
                case ToastSeverity.Error:   return theme.DangerColor;
                default:                    return theme.PrimaryColor;
            }
        }

        internal static Sprite SeverityToIcon(ToastSeverity severity, UIThemeConfig theme)
        {
            switch (severity)
            {
                case ToastSeverity.Success: return theme.IconCheck;
                case ToastSeverity.Warning: return theme.IconWarning;
                case ToastSeverity.Error:   return theme.IconError;
                default:                    return theme.IconInfo;
            }
        }

        internal static UIAudioCue SeverityToCue(ToastSeverity severity)
        {
            switch (severity)
            {
                case ToastSeverity.Success: return UIAudioCue.Success;
                case ToastSeverity.Error:   return UIAudioCue.Error;
                case ToastSeverity.Warning: return UIAudioCue.Notification;
                default:                    return UIAudioCue.Notification;
            }
        }

    }
}
