using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.UIKit.Catalog.HUD
{
    [DisallowMultipleComponent]
    public sealed class HUDEnergySimple : HUDSimple
    {
        [Tooltip("Optional max-cap suffix TMP label rendering '/5'. Hidden when Max <= 0 or value >= Max.")]
        [SerializeField] private TMP_Text _maxCapLabel;
        [Tooltip("Optional Image (Filled type) whose fillAmount mirrors Value/Max. Hidden when Max <= 0.")]
        [SerializeField] private Image _energyBarFill;

        [Tooltip("Initial max cap applied on OnEnable. 0 = no cap (suffix label + bar fill stay hidden).")]
        [SerializeField] private int _initialMax;
        [Tooltip("Format for the max-cap suffix label. {0} = Max value. Default '/{0}'.")]
        [SerializeField] private string _capFormatString = "/{0}";

        private int _max;
        private bool _hasMax;

        public int Max => _max;

        public void SetMax(int max)
        {
            _max = max;
            _hasMax = true;
            UpdateCapLabel();
            UpdateBarFill();
        }

        public void SetValueAndMax(int value, int max)
        {
            SetMax(max);
            SetValue(value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_hasMax) SetMax(_initialMax);
            else { UpdateCapLabel(); UpdateBarFill(); }
        }

        public override void SetValue(int value)
        {
            base.SetValue(value);
            UpdateCapLabel();
            UpdateBarFill();
        }

        public override void SetValueWithoutAnim(int value)
        {
            base.SetValueWithoutAnim(value);
            UpdateCapLabel();
            UpdateBarFill();
        }

        private void UpdateCapLabel()
        {
            if (_maxCapLabel == null) return;
            var hide = _max <= 0 || Value >= _max;
            _maxCapLabel.gameObject.SetActive(!hide);
            if (hide) return;
            _maxCapLabel.SetText(string.Format(_capFormatString, _max));
        }

        private void UpdateBarFill()
        {
            if (_energyBarFill == null) return;
            if (_max <= 0) { _energyBarFill.gameObject.SetActive(false); return; }
            _energyBarFill.gameObject.SetActive(true);
            _energyBarFill.fillAmount = Mathf.Clamp01((float)Value / _max);
        }
    }
}
