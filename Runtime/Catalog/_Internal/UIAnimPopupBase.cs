using DG.Tweening;
using KitforgeLabs.MobileUIKit.Animation;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Internal
{
    // === M3b POLISH CONVENTIONS (locked 2026-05-06, revised after 4 failed iterations) ===
    // Sealed canonical UIAnim* extends polish via 3 virtual hooks below.
    // Cascade items (5/8 — DailyLogin cells, LevelComplete stars+rollup, GameOver CTAs,
    // RewardPopup icons, ShopPopup cards): override PlayPolishShow to fire STANDALONE tweens
    // with .SetDelay(baseTime + i * staggerDelay).
    //
    // CRITICAL — DO NOT add polish tweens to the outer show Sequence via parent.Insert. Validated
    // 2026-05-06 across 4 iterations: the spawn-frame lifecycle (Instantiate → Awakes cascade →
    // AddComponent runtime → ApplyPreset → PlayShow) consumes >1s of accumulated time before
    // DOTween's first Update; any Sequence created during this window has its first tick apply a
    // deltaTime that fast-forwards Insert'd tweens past their offsets, firing OnStart+OnComplete
    // for ALL of them in one frame. Standalone tweens with .SetDelay register their start time
    // individually with DOTweenManager and do not share the outer Sequence's catch-up calculation.
    //
    // Pulse items (3/8 — HUD-Energy regen, HUD-Timer tick, NotEnoughCurrency CTA): expose
    // public NON-virtual Pulse() on their own canonical — no base hook needed.
    //
    // Tween property whitelist (cascade): transform.localScale, RectTransform.anchoredPosition,
    // CanvasGroup.alpha. Forbidden: Image.color, Image.fillAmount, MaterialPropertyBlock during
    // cascade. Reason: SRP Batcher discipline; N×Image color tween = SetPass explosion on 30fps floor.
    //
    // Lifecycle safety — every standalone polish tween MUST chain
    // .SetLink(gameObject, LinkBehaviour.KillOnDisable) so the tween is killed when the popup
    // GameObject is disabled (pool return) or destroyed. No manual List<Tween> tracking required.
    //
    // Spawn-frame warmup — every cascade-pattern PlayPolishShow override MUST add SpawnFrameWarmup
    // to baseTime when computing per-element delays:
    //   var safeBaseTime = baseTime + SpawnFrameWarmup;
    //   parent.DOScale(...).SetDelay(safeBaseTime + i * staggerDelay).SetLink(...).SetUpdate(true);
    // WHY: validated 2026-05-06 with DailyLogin item 1/8 — cells with delay shorter than the
    // first-tick deltaTime spike (~1s after popup spawn) are fast-forwarded to final state by
    // DOTween's catch-up. Without warmup, only the cell whose delay exceeds the spike survives
    // (1/7 partial cascade observed). With warmup, all cells animate visibly. Trade-off: ~1s gap
    // between card show end and cascade start; acceptable as 'pause for emphasis'. SpawnFrameWarmup
    // value (1.0s) is empirical Editor measurement — validate cross-device in M4 perf bench;
    // increase if cells snap on low-end Android (Galaxy A12 baseline).
    // Pulse items (HUD-Energy, HUD-Timer, NotEnoughCurrency) DO NOT need warmup — they fire
    // post-spawn via gameplay events, never during the spawn frame.
    //
    // Sealing enforcement: NO new public virtual / protected virtual polish methods on canonical.
    // The 3 hooks below are the ONLY override points. Polish trigger methods (Pulse) are
    // public non-virtual. All implementation helpers are private.
    // === END POLISH CONVENTIONS ===
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIAnimPopupBase : MonoBehaviour, IUIAnimator
    {
        protected const float SpawnFrameWarmup = 1.0f;

        [Tooltip("CanvasGroup driving fade. Auto-resolved if empty.")]
        [SerializeField] protected CanvasGroup _canvasGroup;
        [Tooltip("Card RectTransform that scales/moves. Usually the popup content rect, NOT the root.")]
        [SerializeField] protected RectTransform _card;

        private UIAnimPreset _preset;
        private Sequence _sequence;
        private Vector3 _baseScale = Vector3.one;
        private Vector2 _basePosition;

        public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();

        protected virtual void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_card != null) { _baseScale = _card.localScale; _basePosition = _card.anchoredPosition; }
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
            var seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            if (_preset.UseScale) seq.Join(_card.DOScale(_baseScale, duration).SetEase(ease));
            if (_preset.UseFade) seq.Join(_canvasGroup.DOFade(1f, duration).SetEase(ease));
            if (_preset.UsePosition) seq.Join(_card.DOAnchorPos(_basePosition, duration).SetEase(ease));
            if (_preset.UseRotation) seq.Join(_card.DOLocalRotate(Vector3.zero, duration).SetEase(ease));
            PlayPolishShow(duration);
            seq.OnComplete(() => onComplete?.Invoke());
            return seq;
        }

        private Sequence BuildHideSequence(System.Action onComplete)
        {
            var ease = _preset.HideEase.ToDoTween();
            var duration = _preset.HideDuration;
            var seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
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
            ResetPolishToStart();
        }

        private void Snap(bool visible)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = visible ? 1f : 0f;
            if (_card != null)
            {
                _card.localScale = _baseScale;
                _card.anchoredPosition = _basePosition;
                _card.localEulerAngles = Vector3.zero;
            }
            SnapPolish(visible);
        }

        protected virtual void PlayPolishShow(float baseTime) { }

        protected virtual void ResetPolishToStart() { }

        protected virtual void SnapPolish(bool visible) { }
    }
}
