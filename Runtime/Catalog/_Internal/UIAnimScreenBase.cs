using System;
using DG.Tweening;
using KitforgeLabs.UIKit.Animation;
using UnityEngine;

namespace KitforgeLabs.UIKit.Catalog.Internal
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIAnimScreenBase : MonoBehaviour, IUIAnimator
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _showDuration = 0.15f;
        [SerializeField] private float _hideDuration = 0.20f;

        private Sequence _sequence;

        public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();

        protected CanvasGroup CanvasGroup => _canvasGroup;
        protected float ShowDuration => _showDuration;
        protected float HideDuration => _hideDuration;

        protected virtual void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void OnDisable()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        protected virtual void OnDestroy()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        public void ApplyPreset(UIAnimPreset preset) { }

        public void PlayShow(Action onComplete = null)
        {
            _sequence?.Kill();
            _sequence = BuildShowSequence(onComplete);
        }

        public void PlayHide(Action onComplete = null)
        {
            _sequence?.Kill();
            _sequence = BuildHideSequence(onComplete);
        }

        public void Skip()
        {
            if (_sequence == null || !_sequence.IsActive()) return;
            _sequence.Complete(true);
        }

        protected virtual Sequence BuildShowSequence(Action onComplete)
        {
            if (_canvasGroup == null) { onComplete?.Invoke(); return null; }
            _canvasGroup.alpha = 0f;
            return DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(Ease.OutCubic))
                .OnComplete(() => onComplete?.Invoke())
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        protected virtual Sequence BuildHideSequence(Action onComplete)
        {
            if (_canvasGroup == null) { onComplete?.Invoke(); return null; }
            return DOTween.Sequence()
                .Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(Ease.InCubic))
                .OnComplete(() => onComplete?.Invoke())
                .SetLink(gameObject)
                .SetUpdate(true);
        }
    }
}
