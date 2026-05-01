using DG.Tweening;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Catalog.Internal;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Confirm
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIAnimConfirmPopup : MonoBehaviour, IUIAnimator
    {
        [Tooltip("CanvasGroup driving fade. Auto-resolved if empty.")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [Tooltip("Card RectTransform that scales/moves. Usually the popup content rect, NOT the root.")]
        [SerializeField] private RectTransform _card;

        private UIAnimPreset _preset;
        private Sequence _sequence;
        private Vector3 _baseScale = Vector3.one;
        private Vector2 _basePosition;

        public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_card != null) { _baseScale = _card.localScale; _basePosition = _card.anchoredPosition; }
        }

        private void OnDisable()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        public void ApplyPreset(UIAnimPreset preset)
        {
            _preset = preset;
        }

        public void PlayShow(System.Action onComplete = null)
        {
            _sequence?.Kill();
            if (_preset == null || _card == null) { Snap(true); onComplete?.Invoke(); return; }
            ResetToShowStart();
            _sequence = BuildShowSequence(onComplete);
        }

        public void PlayHide(System.Action onComplete = null)
        {
            _sequence?.Kill();
            if (_preset == null || _card == null) { Snap(false); onComplete?.Invoke(); return; }
            _sequence = BuildHideSequence(onComplete);
        }

        public void Skip()
        {
            if (_sequence == null || !_sequence.IsActive()) return;
            _sequence.Complete(true);
        }

        private Sequence BuildShowSequence(System.Action onComplete)
        {
            var ease = _preset.ShowEase.ToDoTween();
            var duration = _preset.ShowDuration;
            var seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(false);
            if (_preset.UseScale) seq.Join(_card.DOScale(_baseScale, duration).SetEase(ease));
            if (_preset.UseFade) seq.Join(_canvasGroup.DOFade(1f, duration).SetEase(ease));
            if (_preset.UsePosition) seq.Join(_card.DOAnchorPos(_basePosition, duration).SetEase(ease));
            if (_preset.UseRotation) seq.Join(_card.DOLocalRotate(Vector3.zero, duration).SetEase(ease));
            seq.OnComplete(() => onComplete?.Invoke());
            return seq;
        }

        private Sequence BuildHideSequence(System.Action onComplete)
        {
            var ease = _preset.HideEase.ToDoTween();
            var duration = _preset.HideDuration;
            var seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(false);
            if (_preset.UseScale) seq.Join(_card.DOScale(_baseScale * _preset.HideScaleTo, duration).SetEase(ease));
            if (_preset.UseFade) seq.Join(_canvasGroup.DOFade(0f, duration).SetEase(ease));
            seq.OnComplete(() => onComplete?.Invoke());
            return seq;
        }

        private void ResetToShowStart()
        {
            if (_preset.UseScale) _card.localScale = _baseScale * _preset.ScaleFrom;
            if (_preset.UseFade) _canvasGroup.alpha = 0f;
            if (_preset.UsePosition) _card.anchoredPosition = _basePosition + _preset.PositionOffset;
            if (_preset.UseRotation) _card.localEulerAngles = new Vector3(0f, 0f, _preset.RotationFrom);
        }

        private void Snap(bool visible)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = visible ? 1f : 0f;
            if (_card == null) return;
            _card.localScale = _baseScale;
            _card.anchoredPosition = _basePosition;
            _card.localEulerAngles = Vector3.zero;
        }
    }
}
