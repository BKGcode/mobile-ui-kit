using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Shop
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimShopPopup))]
    public sealed class ShopPopup : UIModule<ShopPopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Close button (top-right). Triggers OnClosed.")]
            public Button CloseButton;
            [Tooltip("Full-screen backdrop button. Closes when ShopPopupData.CloseOnBackdrop = true (default).")]
            public Button BackdropButton;
            [Tooltip("RectTransform that holds the spawned cells. Usually has a GridLayoutGroup.")]
            public RectTransform GridContainer;
            [Tooltip("Disabled ShopItemView used as a clone-from-template at runtime. The builder creates this as a sibling of the grid.")]
            public ShopItemView CellTemplate;
            [Tooltip("Placeholder shown when the provider returns 0 items.")]
            public GameObject EmptyStatePlaceholder;
        }

        [SerializeField] private Refs _refs;

        public event Action<ShopItemData, PurchaseResult> OnPurchaseCompleted;
        public event Action<ShopItemData> OnPurchaseInsufficient;
        public event Action OnClosed;
        public event Action OnDismissed;

        private ShopPopupData _data;
        private IUIAnimator _animator;
        private readonly List<ShopItemView> _cells = new List<ShopItemView>();
        private bool _economySubscribed;

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        internal void SetAnimatorForTests(IUIAnimator animator)
        {
            _animator = animator;
        }

        internal IReadOnlyList<ShopItemView> CellsForTests => _cells;

        internal void DispatchPurchaseForTests(ShopItemData item, PurchaseResult result) => DispatchPurchase(item, result);
        internal void RegisterCellForTests(ShopItemView cell) => _cells.Add(cell);

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.CloseButton != null) _refs.CloseButton.onClick.AddListener(HandleClose);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
            if (_refs.CellTemplate != null) _refs.CellTemplate.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            UnsubscribeEconomy();
            if (_refs.CloseButton != null) _refs.CloseButton.onClick.RemoveListener(HandleClose);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(ShopPopupData data)
        {
            ClearAllEvents();
            CancelInFlightAnimation();
            _data = data ?? new ShopPopupData();
            ApplyTexts(_data);
            ApplyVisibility(_data);
            RebuildGrid();
            IsDismissing = false;
        }

        private void CancelInFlightAnimation()
        {
            if (Animator != null && Animator.IsPlaying) Animator.Skip();
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            SubscribeEconomy();
            if (Animator == null) return;
            Animator.ApplyPreset(ResolveAnimPreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            UnsubscribeEconomy();
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            HandleClose();
        }

        private void HandleClose()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnClosed?.Invoke();
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            HandleClose();
        }

        // No IsDismissing flip after purchase — popup stays open across buys (spec S2).
        // Cell.SetAffordable(false) on InsufficientFunds prevents repeat-fire on the same item.
        private void HandleBuy(ShopItemData item)
        {
            if (IsDismissing) return;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            var provider = Services?.ShopData;
            if (provider == null)
            {
                Debug.LogError("[ShopPopup] No IShopDataProvider available. Purchase ignored. Add a UIServices component to your scene and assign an IShopDataProvider.", this);
                return;
            }
            var result = provider.Purchase(item.Id);
            DispatchPurchase(item, result);
            RefreshAffordability();
        }

        private void DispatchPurchase(ShopItemData item, PurchaseResult result)
        {
            if (result == PurchaseResult.InsufficientFunds)
            {
                Services?.Audio?.Play(UIAudioCue.Error);
                DisableCellForItem(item.Id);
                OnPurchaseInsufficient?.Invoke(item);
                return;
            }
            switch (result)
            {
                case PurchaseResult.Success:      Services?.Audio?.Play(UIAudioCue.Success); break;
                case PurchaseResult.AlreadyOwned:
                case PurchaseResult.Failed:       Services?.Audio?.Play(UIAudioCue.Error);   break;
            }
            OnPurchaseCompleted?.Invoke(item, result);
        }

        private void DismissWithAnimation()
        {
            Services?.Audio?.Play(UIAudioCue.PopupClose);
            if (Animator == null) { FinalizeDismissal(); return; }
            Animator.PlayHide(FinalizeDismissal);
        }

        private void FinalizeDismissal()
        {
            RaiseDismissRequested();
            OnDismissed?.Invoke();
        }

        private void ClearAllEvents()
        {
            OnPurchaseCompleted = null;
            OnPurchaseInsufficient = null;
            OnClosed = null;
            OnDismissed = null;
        }

        private void ApplyTexts(ShopPopupData data)
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(data.Title);
        }

        private void ApplyVisibility(ShopPopupData data)
        {
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = data.CloseOnBackdrop;
        }

        private void RebuildGrid()
        {
            ClearCells();
            var items = ResolveItems();
            ToggleEmptyState(items == null || items.Count == 0);
            if (items == null || items.Count == 0) return;
            for (var i = 0; i < items.Count; i++) SpawnCell(items[i]);
            RefreshAffordability();
        }

        internal void RebuildGridForTests() => RebuildGrid();

        private IReadOnlyList<ShopItemData> ResolveItems()
        {
            var provider = Services?.ShopData;
            if (provider == null)
            {
                Debug.LogError("[ShopPopup] No IShopDataProvider available. Showing empty state. Add a UIServices component to your scene and assign an IShopDataProvider.", this);
                return null;
            }
            return _data.Category.UseFilter ? provider.GetItems(_data.Category.Category) : provider.GetAllItems();
        }

        private void ToggleEmptyState(bool empty)
        {
            if (_refs.EmptyStatePlaceholder != null) _refs.EmptyStatePlaceholder.SetActive(empty);
            if (_refs.GridContainer != null) _refs.GridContainer.gameObject.SetActive(!empty);
        }

        private void SpawnCell(ShopItemData item)
        {
            if (_refs.CellTemplate == null || _refs.GridContainer == null)
            {
                Debug.LogError("[ShopPopup] CellTemplate or GridContainer not assigned. Cannot spawn cells. Run 'Tools/Kitforge/UI Kit/Build Group B Sample' to regenerate the prefab, or wire both fields manually in the Inspector.", this);
                return;
            }
            var cell = Instantiate(_refs.CellTemplate, _refs.GridContainer, false);
            cell.gameObject.SetActive(true);
            cell.Bind(item, ResolveCurrencyIcon(item.PriceCurrency), ResolveAffordableColor(), ResolveUnaffordableColor());
            cell.OnBuyClicked += HandleBuy;
            _cells.Add(cell);
        }

        private void ClearCells()
        {
            for (var i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] == null) continue;
                _cells[i].OnBuyClicked -= HandleBuy;
                Destroy(_cells[i].gameObject);
            }
            _cells.Clear();
        }

        private Sprite ResolveCurrencyIcon(CurrencyType currency)
        {
            if (Theme == null) return null;
            return currency == CurrencyType.Coins ? Theme.IconCoin : Theme.IconGem;
        }

        private Color ResolveAffordableColor() => Theme != null ? Theme.PrimaryColor : Color.white;
        private Color ResolveUnaffordableColor() => Theme != null ? Theme.SecondaryColor : Color.gray;

        private void RefreshAffordability()
        {
            for (var i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] == null) continue;
                _cells[i].SetAffordable(IsAffordable(_cells[i].Item));
            }
        }

        internal void RefreshAffordabilityForTests() => RefreshAffordability();

        private void DisableCellForItem(string itemId)
        {
            for (var i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] == null) continue;
                if (_cells[i].Item.Id == itemId) _cells[i].SetAffordable(false);
            }
        }

        private bool IsAffordable(ShopItemData item)
        {
            var economy = Services?.Economy;
            if (economy == null) return true;
            return economy.CanAfford(item.PriceCurrency, item.PriceAmount);
        }

        private void SubscribeEconomy()
        {
            var economy = Services?.Economy;
            if (economy == null || _economySubscribed) return;
            economy.OnChanged += HandleCurrencyChanged;
            _economySubscribed = true;
        }

        private void UnsubscribeEconomy()
        {
            var economy = Services?.Economy;
            if (economy == null || !_economySubscribed) return;
            economy.OnChanged -= HandleCurrencyChanged;
            _economySubscribed = false;
        }

        private void HandleCurrencyChanged(CurrencyType _, int __) => RefreshAffordability();
    }
}
