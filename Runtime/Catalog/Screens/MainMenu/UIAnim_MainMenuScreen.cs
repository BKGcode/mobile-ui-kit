using System;
using DG.Tweening;
using KitforgeLabs.MobileUIKit.Catalog.Internal;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Screens
{
    public sealed class UIAnim_MainMenuScreen : UIAnimScreenBase
    {
        [Tooltip("Root panel RectTransform that slides in/out. Assign the content container, not the screen root.")]
        [SerializeField] private RectTransform _panel;
        [Tooltip("Items staggered on entry (buttons, title, etc.). Skips inactive items.")]
        [SerializeField] private RectTransform[] _staggerItems;
        [Tooltip("Y offset the panel starts below its resting position on show entry.")]
        [SerializeField] private float _panelEntryOffsetY = -80f;
        [Tooltip("Y offset each stagger item starts below its resting position.")]
        [SerializeField] private float _itemEntryOffsetY = -20f;
        [Tooltip("Delay in seconds between each stagger item entry.")]
        [SerializeField] private float _staggerDelay = 0.05f;

        private Vector2 _panelBasePos;
        private Vector2[] _itemBasePositions;

        protected override void Awake()
        {
            base.Awake();
            if (_panel != null) _panelBasePos = _panel.anchoredPosition;
            CaptureItemBasePositions();
        }

        protected override Sequence BuildShowSequence(Action onComplete)
        {
            if (_panel == null) return base.BuildShowSequence(onComplete);
            _panel.anchoredPosition = _panelBasePos + new Vector2(0f, _panelEntryOffsetY);
            var seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            seq.Append(_panel.DOAnchorPos(_panelBasePos, ShowDuration).SetEase(Ease.OutBack));
            AppendStagger(seq);
            seq.OnComplete(() => onComplete?.Invoke());
            return seq;
        }

        protected override Sequence BuildHideSequence(Action onComplete)
        {
            if (_panel == null) return base.BuildHideSequence(onComplete);
            var target = _panelBasePos + new Vector2(0f, _panelEntryOffsetY);
            var seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            seq.Append(_panel.DOAnchorPos(target, HideDuration).SetEase(Ease.InCubic));
            seq.OnComplete(() => onComplete?.Invoke());
            return seq;
        }

        private void CaptureItemBasePositions()
        {
            if (_staggerItems == null) return;
            _itemBasePositions = new Vector2[_staggerItems.Length];
            for (var i = 0; i < _staggerItems.Length; i++)
            {
                if (_staggerItems[i] != null)
                    _itemBasePositions[i] = _staggerItems[i].anchoredPosition;
            }
        }

        private void AppendStagger(Sequence seq)
        {
            if (_staggerItems == null || _itemBasePositions == null) return;
            for (var i = 0; i < _staggerItems.Length; i++)
            {
                var item = _staggerItems[i];
                if (item == null || !item.gameObject.activeSelf) continue;
                var basePos = _itemBasePositions[i];
                item.anchoredPosition = basePos + new Vector2(0f, _itemEntryOffsetY);
                seq.Insert(i * _staggerDelay, item.DOAnchorPos(basePos, ShowDuration * 0.8f).SetEase(Ease.OutCubic));
            }
        }
    }
}
