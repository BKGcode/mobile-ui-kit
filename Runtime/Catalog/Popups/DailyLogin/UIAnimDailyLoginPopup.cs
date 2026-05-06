using DG.Tweening;
using KitforgeLabs.MobileUIKit.Catalog.Internal;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.DailyLogin
{
    public sealed class UIAnimDailyLoginPopup : UIAnimPopupBase
    {
        [Tooltip("Container whose direct children are the day cells. Cells are runtime-spawned by DailyLoginPopup.Bind() OR baked by builder. Null = polish no-op (graceful for upgrade).")]
        [SerializeField] private RectTransform _cellContainer;

        [Tooltip("Delay between consecutive cells (seconds). 0.1s × 6 + duration 0.4s = ~1.0s total cascade. Below 0.08s the stagger becomes perceptually instant.")]
        [SerializeField, Range(0.05f, 0.25f)] private float _cellStaggerDelay = 0.10f;

        [Tooltip("Easing curve per cell. OutBack overshoots and settles — signature DailyLogin moment.")]
        [SerializeField] private Ease _cellEntryEase = Ease.OutBack;

        [Tooltip("Duration of each cell entry tween (seconds).")]
        [SerializeField, Range(0.20f, 0.80f)] private float _cellEntryDuration = 0.40f;

        private RectTransform[] _cellsCache;
        private CanvasGroup[] _groupsCache;

        protected override void PlayPolishShow(float baseTime)
        {
            ResolveCells();
            if (_cellsCache == null || _cellsCache.Length == 0) return;
            var safeBaseTime = baseTime + SpawnFrameWarmup;
            for (var i = 0; i < _cellsCache.Length; i++)
            {
                if (_cellsCache[i] == null) continue;
                var delay = safeBaseTime + i * _cellStaggerDelay;
                _cellsCache[i].DOScale(Vector3.one, _cellEntryDuration)
                    .SetDelay(delay)
                    .SetEase(_cellEntryEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                    .SetUpdate(true);
                if (_groupsCache[i] != null)
                {
                    _groupsCache[i].DOFade(1f, _cellEntryDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.Linear)
                        .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                        .SetUpdate(true);
                }
            }
        }

        protected override void ResetPolishToStart()
        {
            ResolveCells();
            if (_cellsCache == null) return;
            for (var i = 0; i < _cellsCache.Length; i++)
            {
                if (_cellsCache[i] != null) _cellsCache[i].localScale = Vector3.zero;
                if (_groupsCache[i] != null) _groupsCache[i].alpha = 0f;
            }
        }

        protected override void SnapPolish(bool visible)
        {
            ResolveCells();
            if (_cellsCache == null) return;
            for (var i = 0; i < _cellsCache.Length; i++)
            {
                if (_cellsCache[i] != null) _cellsCache[i].localScale = visible ? Vector3.one : Vector3.zero;
                if (_groupsCache[i] != null) _groupsCache[i].alpha = visible ? 1f : 0f;
            }
        }

        private void ResolveCells()
        {
            if (_cellContainer == null) { _cellsCache = null; _groupsCache = null; return; }
            var count = _cellContainer.childCount;
            if (_cellsCache == null || _cellsCache.Length != count)
            {
                _cellsCache = new RectTransform[count];
                _groupsCache = new CanvasGroup[count];
            }
            for (var i = 0; i < count; i++)
            {
                var child = _cellContainer.GetChild(i) as RectTransform;
                _cellsCache[i] = child;
                if (child != null) _groupsCache[i] = child.GetComponent<CanvasGroup>() ?? child.gameObject.AddComponent<CanvasGroup>();
                else _groupsCache[i] = null;
            }
        }
    }
}
