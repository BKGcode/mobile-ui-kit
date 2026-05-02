using System;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Shop
{
    [DisallowMultipleComponent]
    public sealed class ShopItemView : MonoBehaviour
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Display name TMP label.")]
            public TMP_Text DisplayNameLabel;
            [Tooltip("Price TMP label (numeric only — currency icon is separate).")]
            public TMP_Text PriceLabel;
            [Tooltip("Image showing the price currency (Theme.IconCoin / Theme.IconGem).")]
            public Image CurrencyIcon;
            [Tooltip("Buy button. Click triggers OnBuyClicked.")]
            public Button BuyButton;
            [Tooltip("Image whose color toggles between Theme.PrimaryColor (affordable) and Theme.SecondaryColor (unaffordable).")]
            public Image BuyTint;
        }

        [SerializeField] private Refs _refs;

        public event Action<ShopItemData> OnBuyClicked;

        private ShopItemData _item;
        private Color _affordableColor = Color.white;
        private Color _unaffordableColor = Color.gray;

        public ShopItemData Item => _item;
        internal bool IsAffordableForTests { get; private set; } = true;

        private void Awake()
        {
            if (_refs.BuyButton != null) _refs.BuyButton.onClick.AddListener(HandleBuy);
        }

        private void OnDestroy()
        {
            if (_refs.BuyButton != null) _refs.BuyButton.onClick.RemoveListener(HandleBuy);
            OnBuyClicked = null;
        }

        public void Bind(ShopItemData item, Sprite currencyIcon, Color affordableColor, Color unaffordableColor)
        {
            _item = item;
            _affordableColor = affordableColor;
            _unaffordableColor = unaffordableColor;
            if (_refs.DisplayNameLabel != null) _refs.DisplayNameLabel.SetText(item.DisplayName);
            if (_refs.PriceLabel != null) _refs.PriceLabel.SetText(item.PriceAmount.ToString());
            if (_refs.CurrencyIcon != null)
            {
                _refs.CurrencyIcon.sprite = currencyIcon;
                _refs.CurrencyIcon.enabled = currencyIcon != null;
            }
        }

        public void SetAffordable(bool affordable)
        {
            IsAffordableForTests = affordable;
            if (_refs.BuyButton != null) _refs.BuyButton.interactable = affordable;
            if (_refs.BuyTint != null) _refs.BuyTint.color = affordable ? _affordableColor : _unaffordableColor;
        }

        private void HandleBuy()
        {
            OnBuyClicked?.Invoke(_item);
        }
    }
}
