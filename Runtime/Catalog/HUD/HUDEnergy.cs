using System;
using DG.Tweening;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.HUD
{
    /// <summary>
    /// HUD that displays player energy. Seals <see cref="HUDCurrency.ResolveCurrency"/> to
    /// <see cref="CurrencyType.Energy"/> regardless of the inherited <c>_currency</c> Inspector
    /// field — buyer cannot break energy binding via Inspector mistake. Extends the base HUD
    /// with a regen-tick countdown label, a max-cap suffix label, an energy bar fill image,
    /// and a 1Hz <see cref="IProgressionService.GetEnergyRegenState"/> poll. Null-progression
    /// behavior follows CATALOG_GroupC_DELTA § 4.5 (silent degrade — regen UI hidden, base HUD
    /// counter remains functional). The local <c>Update()</c> shim is a temporary workaround
    /// for the <c>UIModuleBase.OnUpdate()</c> infra dispatch gap; M3 sweep removes it
    /// (anchor comment grep).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HUDEnergy : HUDCurrency
    {
        [Serializable]
        private struct EnergyRefs
        {
            [Tooltip("Countdown TMP label rendering '+1 in HH:MM:SS'. Hidden when energy is full.")]
            public TMP_Text RegenCountdownLabel;
            [Tooltip("Max-cap suffix TMP label rendering '/5'. Hidden when energy is full or Max <= 0.")]
            public TMP_Text MaxCapLabel;
            [Tooltip("Image (type Filled) whose fillAmount mirrors Current/Max. Hidden when Max <= 0.")]
            public Image EnergyBarFill;
        }

        [SerializeField] private EnergyRefs _energyRefs;

        [Tooltip("Format for the max-cap suffix label. {0} = Max value. Default '/{0}'.")]
        [SerializeField] private string _capFormatString = "/{0}";
        [Tooltip("Format for the regen countdown label. {0} = HH:MM:SS string. Default '+1 in {0}'.")]
        [SerializeField] private string _regenFormatString = "+1 in {0}";
        [Tooltip("Text shown when next regen tick is overdue (NextRegenUtc <= now).")]
        [SerializeField] private string _regenReadyText = "+1 ready";

        [Tooltip("Fire a color flash on the cap label when Current crosses up to Max. Default true.")]
        [SerializeField] private bool _atMaxFlashEnabled = true;
        [Tooltip("Color the cap label flashes to when Current crosses up to Max.")]
        [SerializeField] private Color _atMaxFlashColor = new Color(0.30f, 0.78f, 0.45f, 1f);
        [Tooltip("Duration of the at-max flash tween (in + out combined).")]
        [SerializeField] private float _atMaxFlashDuration = 0.30f;

        private Tween _capFlashTween;
        private Color _capLabelOriginalColor = Color.white;
        private bool _capColorCaptured;
        private float _lastTickTime;
        private bool _hasLastEnergyState;
        private int _lastEnergyValue;

        internal int LastEnergyValueForTests => _lastEnergyValue;
        internal string CapLabelTextForTests => _energyRefs.MaxCapLabel == null ? null : _energyRefs.MaxCapLabel.text;
        internal bool CapLabelVisibleForTests => _energyRefs.MaxCapLabel != null && _energyRefs.MaxCapLabel.gameObject.activeSelf;
        internal string RegenLabelTextForTests => _energyRefs.RegenCountdownLabel == null ? null : _energyRefs.RegenCountdownLabel.text;
        internal bool RegenLabelVisibleForTests => _energyRefs.RegenCountdownLabel != null && _energyRefs.RegenCountdownLabel.gameObject.activeSelf;
        internal float BarFillAmountForTests => _energyRefs.EnergyBarFill == null ? 0f : _energyRefs.EnergyBarFill.fillAmount;
        internal bool BarFillVisibleForTests => _energyRefs.EnergyBarFill != null && _energyRefs.EnergyBarFill.gameObject.activeSelf;
        internal void SetEnergyRefsForTests(TMP_Text regenLabel, TMP_Text capLabel, Image energyBarFill)
        {
            _energyRefs.RegenCountdownLabel = regenLabel;
            _energyRefs.MaxCapLabel = capLabel;
            _energyRefs.EnergyBarFill = energyBarFill;
        }
        internal void ForceRegenPollForTests() => PollAndApplyRegenState();

        protected override CurrencyType ResolveCurrency() => CurrencyType.Energy;

        protected override void OnEnable()
        {
            CaptureCapLabelColorOnce();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            KillCapFlashTween();
        }

        private void OnDestroy()
        {
            KillCapFlashTween();
        }

        // OnUpdate-workaround-M3-sweep
        private void Update()
        {
            OnUpdate();
        }

        private void OnUpdate()
        {
            TickRegenPoll();
        }

        protected override void Subscribe()
        {
            base.Subscribe();
            var economy = Services?.Economy;
            if (economy != null) economy.OnChanged += HandleEconomyChangedForRegen;
            _lastTickTime = float.NegativeInfinity;
        }

        protected override void Unsubscribe()
        {
            var economy = Services?.Economy;
            if (economy != null) economy.OnChanged -= HandleEconomyChangedForRegen;
            base.Unsubscribe();
        }

        protected override void Refresh()
        {
            base.Refresh();
            PollAndApplyRegenState();
        }

        private void HandleEconomyChangedForRegen(CurrencyType currency, int newValue)
        {
            if (currency != ResolveCurrency()) return;
            PollAndApplyRegenState();
        }

        private void TickRegenPoll()
        {
            if (Time.unscaledTime - _lastTickTime < 1f) return;
            _lastTickTime = Time.unscaledTime;
            PollAndApplyRegenState();
        }

        private void PollAndApplyRegenState()
        {
            var prog = Services?.Progression;
            if (prog == null) { ApplySilentDegrade(); return; }
            var state = prog.GetEnergyRegenState();
            UpdateCapLabel(state);
            UpdateRegenLabel(state);
            UpdateBarFill(state);
            TryFireAtMaxFlash(state);
            _lastEnergyValue = state.Current;
            _hasLastEnergyState = true;
        }

        private void ApplySilentDegrade()
        {
            if (_energyRefs.MaxCapLabel != null) _energyRefs.MaxCapLabel.gameObject.SetActive(false);
            if (_energyRefs.RegenCountdownLabel != null) _energyRefs.RegenCountdownLabel.gameObject.SetActive(false);
            if (_energyRefs.EnergyBarFill != null) _energyRefs.EnergyBarFill.gameObject.SetActive(false);
        }

        private void UpdateCapLabel(EnergyRegenState state)
        {
            var label = _energyRefs.MaxCapLabel;
            if (label == null) return;
            var hide = state.IsFull || state.Max <= 0;
            label.gameObject.SetActive(!hide);
            if (hide) return;
            label.SetText(string.Format(_capFormatString, state.Max));
        }

        private void UpdateRegenLabel(EnergyRegenState state)
        {
            var label = _energyRefs.RegenCountdownLabel;
            if (label == null) return;
            if (state.IsFull) { label.gameObject.SetActive(false); return; }
            label.gameObject.SetActive(true);
            label.SetText(FormatRegenText(state));
        }

        private string FormatRegenText(EnergyRegenState state)
        {
            var time = Services?.Time;
            if (time == null) return _regenReadyText;
            var span = time.GetTimeUntil(state.NextRegenUtc);
            if (span.Ticks <= 0) return _regenReadyText;
            var clock = $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
            return string.Format(_regenFormatString, clock);
        }

        private void UpdateBarFill(EnergyRegenState state)
        {
            var fill = _energyRefs.EnergyBarFill;
            if (fill == null) return;
            if (state.Max <= 0) { fill.gameObject.SetActive(false); return; }
            fill.gameObject.SetActive(true);
            fill.fillAmount = Mathf.Clamp01((float)state.Current / state.Max);
        }

        private void TryFireAtMaxFlash(EnergyRegenState state)
        {
            if (!_atMaxFlashEnabled) return;
            if (!_hasLastEnergyState) return;
            if (state.Current < state.Max) return;
            if (_lastEnergyValue >= state.Max) return;
            FireCapFlash();
        }

        private void FireCapFlash()
        {
            var label = _energyRefs.MaxCapLabel;
            if (label == null) return;
            CaptureCapLabelColorOnce();
            KillCapFlashTween();
            var half = Mathf.Max(_atMaxFlashDuration * 0.5f, 0.01f);
            _capFlashTween = DOTween.Sequence()
                .Append(label.DOColor(_atMaxFlashColor, half).SetEase(Ease.OutQuad))
                .Append(label.DOColor(_capLabelOriginalColor, half).SetEase(Ease.InQuad))
                .SetLink(gameObject);
        }

        private void CaptureCapLabelColorOnce()
        {
            if (_capColorCaptured) return;
            if (_energyRefs.MaxCapLabel == null) return;
            _capLabelOriginalColor = _energyRefs.MaxCapLabel.color;
            _capColorCaptured = true;
        }

        private void KillCapFlashTween()
        {
            _capFlashTween?.Kill();
            _capFlashTween = null;
        }
    }
}
