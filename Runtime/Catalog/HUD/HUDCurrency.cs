using System;
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
    /// Parameterized HUD that displays a single currency from <see cref="IEconomyService"/>.
    /// Subscribes to <see cref="IEconomyService.OnChanged"/> and filters foreign currencies
    /// silently — a sentinel value such as <c>(CurrencyType)(-1)</c> used by RewardPopup for
    /// Item/Bundle rewards is naturally excluded by the equality check, no special-casing required.
    /// Subclasses (e.g. HUDEnergy) override <see cref="ResolveCurrency"/> to seal the currency
    /// to a specific value regardless of the inherited Inspector field.
    /// </summary>
    [DisallowMultipleComponent]
    public class HUDCurrency : UIHUDBase
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Currency icon Image (themed via ThemedImage IconCoin/IconGem/IconEnergy slot at prefab time).")]
            public Image IconImage;
            [Tooltip("TMP label rendering the formatted currency count.")]
            public TMP_Text CountLabel;
            [Tooltip("Optional button covering the HUD. Click triggers _onClickEvent.")]
            public Button ClickButton;
        }

        [Tooltip("Currency this HUD displays. Set at prefab time only — runtime changes do not re-subscribe and will silently leave the HUD bound to the previous currency.")]
        [SerializeField] private CurrencyType _currency = CurrencyType.Coins;

        [SerializeField] private Refs _refs;
        [Tooltip("Format passed to int.ToString. Default 'N0' yields thousand-separated integers (12,345).")]
        [SerializeField] private string _formatString = "N0";
        [Tooltip("Punch tween duration on every value change.")]
        [SerializeField] private float _changeAnimDuration = 0.18f;
        [Tooltip("Punch scale multiplier — 1.20 means scale tween reaches +20%.")]
        [SerializeField] private float _changeAnimScale = 1.20f;
        [SerializeField] private UnityEvent _onClickEvent;

        private Tween _punchTween;
        private int _lastValue;
        private bool _hasInitialValue;

        internal int LastValueForTests => _lastValue;
        internal string FormattedTextForTests => _hasInitialValue ? _lastValue.ToString(_formatString) : null;
        internal void SetServicesForTests(UIServices services) => SetServicesInternal(services);
        internal void SetCurrencyForTests(CurrencyType currency) => _currency = currency;
        internal void ForceRefreshForTests()
        {
            Subscribe();
            Refresh();
        }

        protected virtual CurrencyType ResolveCurrency() => _currency;

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
            economy.OnChanged += HandleEconomyChanged;
        }

        protected override void Unsubscribe()
        {
            var economy = Services?.Economy;
            if (economy == null) return;
            economy.OnChanged -= HandleEconomyChanged;
        }

        protected override void Refresh()
        {
            var economy = Services?.Economy;
            if (economy == null)
            {
                SetLabel("--");
                Debug.LogError($"[HUDCurrency] IEconomyService not available — label shows '--'. Assign a UIServices component (with an IEconomyService implementation) on this HUD's _services field. Currency: {ResolveCurrency()}.", this);
                return;
            }
            ApplyValue(economy.Get(ResolveCurrency()), animate: false);
        }

        private void HandleEconomyChanged(CurrencyType currency, int newValue)
        {
            if (currency != ResolveCurrency()) return;
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
            _onClickEvent?.Invoke();
        }
    }
}
