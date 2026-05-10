using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.HUD
{
    [DisallowMultipleComponent]
    public class HUDSimple : MonoBehaviour
    {
        [Tooltip("Optional icon Image (themed via ThemedImage at prefab time, or assigned in Inspector). May be null.")]
        [SerializeField] private Image _iconImage;
        [Tooltip("TMP label rendering the formatted value. Required.")]
        [SerializeField] private TMP_Text _countLabel;
        [Tooltip("Optional button covering the HUD. Click triggers _onClickEvent.")]
        [SerializeField] private Button _clickButton;

        [Tooltip("Initial value applied on OnEnable (only if SetValue has not been called yet).")]
        [SerializeField] private int _initialValue;
        [Tooltip("Format passed to int.ToString. Default 'N0' yields thousand-separated integers (12,345).")]
        [SerializeField] private string _formatString = "N0";
        [Tooltip("Punch tween duration on every value change.")]
        [SerializeField] private float _changeAnimDuration = 0.18f;
        [Tooltip("Punch scale multiplier — 1.20 means scale tween reaches +20%. Set to 1 to disable the punch tween entirely.")]
        [SerializeField] private float _changeAnimScale = 1.20f;
        [SerializeField] private UnityEvent _onClickEvent;

        private Tween _punchTween;
        private int _lastValue;
        private bool _hasInitialValue;

        public int Value => _lastValue;
        public Image IconImage => _iconImage;
        public TMP_Text CountLabel => _countLabel;

        public virtual void SetValue(int value)
        {
            ApplyValue(value, animate: _hasInitialValue);
        }

        public virtual void SetValueWithoutAnim(int value)
        {
            ApplyValue(value, animate: false);
        }

        protected virtual void OnEnable()
        {
            if (_clickButton != null) _clickButton.onClick.AddListener(HandleClick);
            if (!_hasInitialValue) ApplyValue(_initialValue, animate: false);
            else SetLabel(_lastValue.ToString(_formatString));
        }

        protected virtual void OnDisable()
        {
            if (_clickButton != null) _clickButton.onClick.RemoveListener(HandleClick);
            KillPunchTween();
        }

        private void OnDestroy()
        {
            KillPunchTween();
        }

        private void ApplyValue(int value, bool animate)
        {
            var changed = _hasInitialValue && value != _lastValue;
            _lastValue = value;
            _hasInitialValue = true;
            SetLabel(value.ToString(_formatString));
            if (animate && changed) PlayPunch();
        }

        protected void SetLabel(string text)
        {
            if (_countLabel != null) _countLabel.SetText(text);
        }

        private void PlayPunch()
        {
            if (_countLabel == null) return;
            if (_changeAnimScale <= 1.0001f) return;
            KillPunchTween();
            var t = _countLabel.transform;
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
