using System;
using DG.Tweening;
using KitforgeLabs.MobileUIKit.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.HUD
{
    [DisallowMultipleComponent]
    public sealed class HUDGems : UIHUDBase
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Gem icon Image (themed via ThemedImage IconGem slot at prefab time).")]
            public Image IconImage;
            [Tooltip("TMP label rendering the formatted gem count.")]
            public TMP_Text CountLabel;
            [Tooltip("Optional button covering the HUD. Click triggers _onGemClickEvent.")]
            public Button ClickButton;
        }

        [SerializeField] private Refs _refs;
        [Tooltip("Format passed to int.ToString. Default 'N0' yields thousand-separated integers (12,345).")]
        [SerializeField] private string _formatString = "N0";
        [Tooltip("Punch tween duration on every value change.")]
        [SerializeField] private float _changeAnimDuration = 0.18f;
        [Tooltip("Punch scale multiplier — 1.20 means scale tween reaches +20%.")]
        [SerializeField] private float _changeAnimScale = 1.20f;
        [SerializeField] private UnityEvent _onGemClickEvent;

        private Tween _punchTween;
        private int _lastValue;
        private bool _hasInitialValue;

        internal int LastValueForTests => _lastValue;
        internal string FormattedTextForTests => _hasInitialValue ? _lastValue.ToString(_formatString) : null;
        internal void SetServicesForTests(KitforgeLabs.MobileUIKit.Services.UIServices services) => SetServicesInternal(services);
        internal void ForceRefreshForTests()
        {
            Subscribe();
            Refresh();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_refs.ClickButton != null) _refs.ClickButton.onClick.AddListener(HandleClick);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_refs.ClickButton != null) _refs.ClickButton.onClick.RemoveListener(HandleClick);
            KillPunchTween();
        }

        private void OnDestroy()
        {
            KillPunchTween();
        }

        protected override void Subscribe()
        {
            var economy = Services?.Economy;
            if (economy == null) return;
            economy.OnGemsChanged += HandleGemsChanged;
        }

        protected override void Unsubscribe()
        {
            var economy = Services?.Economy;
            if (economy == null) return;
            economy.OnGemsChanged -= HandleGemsChanged;
        }

        protected override void Refresh()
        {
            var economy = Services?.Economy;
            if (economy == null)
            {
                SetLabel("--");
                Debug.LogError("[HUDGems] IEconomyService not available — label shows '--'. Assign a UIServices component (with an IEconomyService implementation) on this HUD's _services field.", this);
                return;
            }
            ApplyValue(economy.GetGems(), animate: false);
        }

        private void HandleGemsChanged(int newValue)
        {
            ApplyValue(newValue, animate: true);
        }

        private void ApplyValue(int value, bool animate)
        {
            var changed = _hasInitialValue && value != _lastValue;
            _lastValue = value;
            _hasInitialValue = true;
            SetLabel(value.ToString(_formatString));
            if (animate && changed) PlayPunch();
        }

        private void SetLabel(string text)
        {
            if (_refs.CountLabel != null) _refs.CountLabel.SetText(text);
        }

        private void PlayPunch()
        {
            if (_refs.CountLabel == null) return;
            KillPunchTween();
            var t = _refs.CountLabel.transform;
            t.localScale = Vector3.one;
            _punchTween = t.DOPunchScale(Vector3.one * (_changeAnimScale - 1f), _changeAnimDuration, 4, 0.5f)
                .SetLink(gameObject)
                .SetEase(Ease.OutQuad);
        }

        private void KillPunchTween()
        {
            _punchTween?.Kill();
            _punchTween = null;
        }

        private void HandleClick()
        {
            _onGemClickEvent?.Invoke();
        }
    }
}
