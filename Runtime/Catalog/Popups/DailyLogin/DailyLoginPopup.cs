using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.DailyLogin
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimDailyLoginPopup))]
    public sealed class DailyLoginPopup : UIModule<DailyLoginPopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Claim button. Hidden in already-claimed-today state.")]
            public Button ClaimButton;
            [Tooltip("TMP label inside the claim button.")]
            public TMP_Text ClaimLabel;
            [Tooltip("Countdown TMP label shown in already-claimed-today state. Format HH:MM:SS until next day boundary (UTC midnight).")]
            public TMP_Text CountdownLabel;
            [Tooltip("Watch-ad-to-double button. Visible when current entry has AllowDouble=true AND IAdsService.IsRewardedAdReady()=true AND DTO.DoubledToday=false.")]
            public Button WatchToDoubleButton;
            [Tooltip("TMP label inside the watch-to-double button.")]
            public TMP_Text WatchToDoubleLabel;
            [Tooltip("Full-screen backdrop button. Routes to silent dismiss when DTO CloseOnBackdrop = true.")]
            public Button BackdropButton;
            [Tooltip("Container whose children represent calendar day cells. Optional — animation hook only; cell authoring is buyer/builder concern.")]
            public RectTransform DayCellContainer;
        }

        [SerializeField] private Refs _refs;

        public event Action<int, RewardPopupData[]> OnDayClaimed;
        public event Action<int, RewardPopupData[]> OnWatchAdRequested;
        public event Action OnDismissed;

        private DailyLoginPopupData _data;
        private IUIAnimator _animator;
        private float _countdownTimer;

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        internal void SetAnimatorForTests(IUIAnimator animator) => _animator = animator;
        internal void InvokeClaimForTests() => HandleClaim();
        internal void InvokeWatchAdForTests() => HandleWatchAd();
        internal void InvokeBackdropForTests() => HandleBackdrop();
        internal float CountdownRemainingForTests => _countdownTimer;
        internal DailyLoginPopupData DataForTests => _data;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.ClaimButton != null) _refs.ClaimButton.onClick.AddListener(HandleClaim);
            if (_refs.WatchToDoubleButton != null) _refs.WatchToDoubleButton.onClick.AddListener(HandleWatchAd);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
        }

        private void OnDestroy()
        {
            if (_refs.ClaimButton != null) _refs.ClaimButton.onClick.RemoveListener(HandleClaim);
            if (_refs.WatchToDoubleButton != null) _refs.WatchToDoubleButton.onClick.RemoveListener(HandleWatchAd);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(DailyLoginPopupData data)
        {
            ClearAllEvents();
            if (Animator != null && Animator.IsPlaying) Animator.Skip();
            _data = data ?? new DailyLoginPopupData();
            ApplyTexts();
            ApplyVisibility();
            InitCountdown();
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            if (!ValidateBindOrAbort()) return;
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            if (Animator == null) return;
            Animator.ApplyPreset(ResolveAnimPreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            DismissWithoutClaim();
        }

        public override void OnUpdate()
        {
            AdvanceCountdown(Time.unscaledDeltaTime);
        }

        private void Update()
        {
            OnUpdate();
        }

        internal void AdvanceCountdown(float deltaTime)
        {
            if (_data == null || !_data.AlreadyClaimedToday || IsDismissing) return;
            _countdownTimer -= deltaTime;
            if (_countdownTimer > 0f) { UpdateCountdownLabel(); return; }
            TryAutoTransitionToClaimReady();
        }

        private bool ValidateBindOrAbort()
        {
            if (Services == null || Services.Time == null)
            {
                Debug.LogError("DailyLoginPopup: ITimeService not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding.", this);
                AbortShow();
                return false;
            }
            if (_data.RewardEntries == null || _data.RewardEntries.Length == 0)
            {
                Debug.LogError("DailyLoginPopup: RewardEntries is null or empty. Bind a non-empty calendar before showing.", this);
                AbortShow();
                return false;
            }
            return true;
        }

        private void AbortShow()
        {
            IsDismissing = true;
            RaiseDismissRequested();
            OnDismissed?.Invoke();
        }

        private void HandleClaim()
        {
            if (IsDismissing || _data == null || _data.AlreadyClaimedToday) return;
            var rewards = GetCurrentRewards();
            if (rewards.Length == 0) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.Success);
            OnDayClaimed?.Invoke(_data.CurrentDay, rewards);
            DismissWithAnimation();
        }

        private void HandleWatchAd()
        {
            if (IsDismissing || _data == null) return;
            if (!ShouldShowWatchToDouble()) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnWatchAdRequested?.Invoke(_data.CurrentDay, GetCurrentRewards());
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            DismissWithoutClaim();
        }

        private void DismissWithoutClaim()
        {
            IsDismissing = true;
            DismissWithAnimation();
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

        private void ClearAllEvents()
        {
            OnDayClaimed = null;
            OnWatchAdRequested = null;
            OnDismissed = null;
        }

        private void ApplyTexts()
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(_data.Title);
            if (_refs.ClaimLabel != null) _refs.ClaimLabel.SetText(_data.ClaimLabel);
            if (_refs.WatchToDoubleLabel != null) _refs.WatchToDoubleLabel.SetText(_data.WatchToDoubleLabel);
        }

        private void ApplyVisibility()
        {
            var alreadyClaimed = _data.AlreadyClaimedToday;
            if (_refs.ClaimButton != null) _refs.ClaimButton.gameObject.SetActive(!alreadyClaimed);
            if (_refs.CountdownLabel != null) _refs.CountdownLabel.gameObject.SetActive(alreadyClaimed);
            if (_refs.WatchToDoubleButton != null) _refs.WatchToDoubleButton.gameObject.SetActive(ShouldShowWatchToDouble());
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = _data.CloseOnBackdrop;
        }

        private bool ShouldShowWatchToDouble()
        {
            if (_data == null || _data.DoubledToday) return false;
            var entry = ResolveCurrentEntry();
            if (entry == null || !entry.AllowDouble) return false;
            var ads = Services?.Ads;
            if (ads == null) return false;
            return ads.IsRewardedAdReady();
        }

        private DailyLoginRewardEntry ResolveCurrentEntry()
        {
            var entries = _data?.RewardEntries;
            if (entries == null || entries.Length == 0) return null;
            var index = (_data.CurrentDay - 1) % entries.Length;
            if (index < 0) index += entries.Length;
            return entries[index];
        }

        private RewardPopupData[] GetCurrentRewards()
        {
            var entry = ResolveCurrentEntry();
            return entry?.Rewards ?? Array.Empty<RewardPopupData>();
        }

        private void InitCountdown()
        {
            if (_data == null || !_data.AlreadyClaimedToday) { _countdownTimer = 0f; return; }
            var time = Services?.Time;
            if (time == null) { _countdownTimer = 0f; UpdateCountdownLabel(TimeSpan.Zero); return; }
            var span = time.GetTimeUntil(ComputeNextDayBoundaryUtc(time.GetServerTimeUtc()));
            _countdownTimer = (float)span.TotalSeconds;
            UpdateCountdownLabel(span);
        }

        private void TryAutoTransitionToClaimReady()
        {
            var prog = Services?.Progression;
            if (prog == null) { InitCountdown(); return; }
            var state = prog.GetDailyLoginState();
            if (state.AlreadyClaimedToday) { _data.LastClaimUtc = state.LastClaimUtc; InitCountdown(); return; }
            _data.CurrentDay = state.CurrentDay;
            _data.LastClaimUtc = state.LastClaimUtc;
            _data.AlreadyClaimedToday = false;
            _data.DoubledToday = state.DoubledToday;
            ApplyVisibility();
        }

        private void UpdateCountdownLabel()
        {
            UpdateCountdownLabel(TimeSpan.FromSeconds(Mathf.Max(0f, _countdownTimer)));
        }

        private void UpdateCountdownLabel(TimeSpan span)
        {
            if (_refs.CountdownLabel == null) return;
            _refs.CountdownLabel.SetText(FormatCountdown(span));
        }

        private static string FormatCountdown(TimeSpan span)
        {
            if (span.Ticks <= 0) return "00:00:00";
            return $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }

        private static DateTime ComputeNextDayBoundaryUtc(DateTime nowUtc)
        {
            var todayUtc = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
            return todayUtc.AddDays(1);
        }
    }
}
