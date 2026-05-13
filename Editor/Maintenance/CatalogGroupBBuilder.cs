using KitforgeLabs.UIKit.Catalog.HUD;
using KitforgeLabs.UIKit.Catalog.NotEnough;
using KitforgeLabs.UIKit.Catalog.Reward;
using KitforgeLabs.UIKit.Catalog.Shop;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KitforgeLabs.UIKit.Editor.Maintenance.CatalogGroupBuilderShared;

namespace KitforgeLabs.UIKit.Editor.Maintenance
{
    internal static class CatalogGroupBBuilder
    {
        private const string DefaultThemePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset";

        private static string RewardPath => $"{ResolvePrefabsFolder()}/RewardPopup.prefab";
        private static string ShopPath => $"{ResolvePrefabsFolder()}/ShopPopup.prefab";
        private static string NotEnoughPath => $"{ResolvePrefabsFolder()}/NotEnoughCurrencyPopup.prefab";
        private static string HUDCoinsPath => $"{ResolvePrefabsFolder()}/HUDCoins.prefab";
        private static string HUDGemsPath => $"{ResolvePrefabsFolder()}/HUDGems.prefab";

        private static readonly Color CellColor = new Color(0.94f, 0.95f, 0.98f, 1f);
        private static readonly Color HeaderTintColor = new Color(1.00f, 0.60f, 0.15f, 1f);
        private static readonly Color SuccessTintColor = new Color(0.30f, 0.78f, 0.45f, 1f);
        private static readonly Color HUDBackgroundColor = new Color(0f, 0f, 0f, 0.45f);

        public static void BuildAll()
        {
            EnsurePrefabsFolder(ResolvePrefabsFolder());
            BuildRewardPopup();
            BuildShopPopup();
            BuildNotEnoughCurrencyPopup();
            BuildHUDCoins();
            BuildHUDGems();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static RewardPopup BuildRewardPopup()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = CreateRoot("RewardPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 720f));

            var title = CreateThemedText(card.transform, "Title", "Reward!", 40, FontStyles.Bold, ThemeBuilderSlots.Heading);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 64f);
            title.alignment = TextAlignmentOptions.Center;

            var iconGO = CreateChild(card.transform, "RewardIcon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0f, 80f);
            iconRT.sizeDelta = new Vector2(220f, 220f);
            var icon = AddImage(iconGO, Color.white);
            if (theme != null) icon.sprite = theme.IconCoin;

            var amount = CreateThemedText(card.transform, "AmountLabel", "+100", 56, FontStyles.Bold, ThemeBuilderSlots.Heading);
            var amountRT = amount.GetComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.5f, 0.5f);
            amountRT.anchorMax = new Vector2(0.5f, 0.5f);
            amountRT.pivot = new Vector2(0.5f, 0.5f);
            amountRT.anchoredPosition = new Vector2(0f, -90f);
            amountRT.sizeDelta = new Vector2(600f, 80f);
            amount.alignment = TextAlignmentOptions.Center;

            var item = CreateThemedText(card.transform, "ItemLabel", "", 32, FontStyles.Italic, ThemeBuilderSlots.Body);
            var itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0.5f, 0.5f);
            itemRT.anchorMax = new Vector2(0.5f, 0.5f);
            itemRT.pivot = new Vector2(0.5f, 0.5f);
            itemRT.anchoredPosition = new Vector2(0f, -160f);
            itemRT.sizeDelta = new Vector2(600f, 60f);
            item.alignment = TextAlignmentOptions.Center;
            item.gameObject.SetActive(false);

            var (claimGO, claimButton, claimImg) = CreateButton(card.transform, "ClaimButton", ButtonPrimaryColor, ThemeSpriteSlot.ButtonPrimary, ThemeColorSlot.None);
            var claimRT = claimGO.GetComponent<RectTransform>();
            claimRT.anchorMin = new Vector2(0.5f, 0f);
            claimRT.anchorMax = new Vector2(0.5f, 0f);
            claimRT.pivot = new Vector2(0.5f, 0f);
            claimRT.anchoredPosition = new Vector2(0f, 40f);
            claimRT.sizeDelta = new Vector2(360f, 96f);
            claimImg.color = SuccessTintColor;
            var claimLabel = CreateThemedText(claimGO.transform, "Label", "Claim", 30, FontStyles.Bold, ThemeBuilderSlots.PrimaryButtonLabel);
            claimLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(claimLabel.GetComponent<RectTransform>());

            var animator = root.AddComponent<UIAnimRewardPopup>();
            var popup = root.AddComponent<RewardPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.AmountLabel").objectReferenceValue = amount;
            so.FindProperty("_refs.ItemLabel").objectReferenceValue = item;
            so.FindProperty("_refs.ClaimLabel").objectReferenceValue = claimLabel;
            so.FindProperty("_refs.ClaimButton").objectReferenceValue = claimButton;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.RewardIcon").objectReferenceValue = icon;
            so.FindProperty("_refs.ClaimTint").objectReferenceValue = claimImg;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<RewardPopup>(root, RewardPath);
        }

        private static ShopPopup BuildShopPopup()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = CreateRoot("ShopPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(900f, 1500f));

            var title = CreateThemedText(card.transform, "Title", "Shop", 44, FontStyles.Bold, ThemeBuilderSlots.Heading);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 72f);
            title.alignment = TextAlignmentOptions.Center;

            var (closeGO, closeButton, closeImg) = CreateSecondaryButton(card.transform, "CloseButton");
            var closeRT = closeGO.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1f, 1f);
            closeRT.anchorMax = new Vector2(1f, 1f);
            closeRT.pivot = new Vector2(1f, 1f);
            closeRT.anchoredPosition = new Vector2(-20f, -20f);
            closeRT.sizeDelta = new Vector2(80f, 80f);
            if (theme != null && theme.IconClose != null) closeImg.sprite = theme.IconClose;

            var grid = CreateChild(card.transform, "Grid");
            var gridRT = grid.GetComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0f, 0f);
            gridRT.anchorMax = new Vector2(1f, 1f);
            gridRT.offsetMin = new Vector2(40f, 60f);
            gridRT.offsetMax = new Vector2(-40f, -120f);
            var gridLayout = grid.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(200f, 260f);
            gridLayout.spacing = new Vector2(16f, 16f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;

            var cellTemplate = BuildCellTemplate(grid.transform, theme);

            var emptyGO = CreateChild(card.transform, "EmptyPlaceholder");
            var emptyRT = emptyGO.GetComponent<RectTransform>();
            StretchInside(emptyRT);
            var emptyText = CreateThemedText(emptyGO.transform, "Label", "Shop is empty.", 28, FontStyles.Italic, ThemeBuilderSlots.Body);
            StretchInside(emptyText.GetComponent<RectTransform>());
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyGO.SetActive(false);

            var animator = root.AddComponent<UIAnimShopPopup>();
            var popup = root.AddComponent<ShopPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.CloseButton").objectReferenceValue = closeButton;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.GridContainer").objectReferenceValue = gridRT;
            so.FindProperty("_refs.CellTemplate").objectReferenceValue = cellTemplate;
            so.FindProperty("_refs.EmptyStatePlaceholder").objectReferenceValue = emptyGO;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<ShopPopup>(root, ShopPath);
        }

        private static ShopItemView BuildCellTemplate(Transform parent, UIThemeConfig theme)
        {
            var cellGO = CreateChild(parent, "CellTemplate");
            var cellRT = cellGO.GetComponent<RectTransform>();
            cellRT.sizeDelta = new Vector2(200f, 260f);
            AddImage(cellGO, CellColor);

            var nameText = CreateThemedText(cellGO.transform, "DisplayName", "Item", 22, FontStyles.Bold, ThemeBuilderSlots.BodyPrimary);
            var nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 1f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.pivot = new Vector2(0.5f, 1f);
            nameRT.anchoredPosition = new Vector2(0f, -10f);
            nameRT.sizeDelta = new Vector2(-12f, 50f);
            nameText.alignment = TextAlignmentOptions.Center;

            var iconGO = CreateChild(cellGO.transform, "ItemIcon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0f, 20f);
            iconRT.sizeDelta = new Vector2(80f, 80f);
            AddImage(iconGO, new Color(0.85f, 0.86f, 0.90f, 1f));

            var priceRow = CreateChild(cellGO.transform, "PriceRow");
            var priceRowRT = priceRow.GetComponent<RectTransform>();
            priceRowRT.anchorMin = new Vector2(0f, 0f);
            priceRowRT.anchorMax = new Vector2(1f, 0f);
            priceRowRT.pivot = new Vector2(0.5f, 0f);
            priceRowRT.anchoredPosition = new Vector2(0f, 70f);
            priceRowRT.sizeDelta = new Vector2(-12f, 32f);
            var priceLayout = priceRow.AddComponent<HorizontalLayoutGroup>();
            priceLayout.spacing = 4f;
            priceLayout.childAlignment = TextAnchor.MiddleCenter;
            priceLayout.childControlWidth = false;
            priceLayout.childControlHeight = true;
            priceLayout.childForceExpandWidth = false;

            var currencyIconGO = CreateChild(priceRow.transform, "CurrencyIcon");
            var currencyIconRT = currencyIconGO.GetComponent<RectTransform>();
            currencyIconRT.sizeDelta = new Vector2(28f, 28f);
            var currencyIcon = AddImage(currencyIconGO, Color.white);
            if (theme != null) currencyIcon.sprite = theme.IconCoin;
            var currencyLE = currencyIconGO.AddComponent<LayoutElement>();
            currencyLE.preferredWidth = 28f;

            var priceText = CreateThemedText(priceRow.transform, "Price", "100", 24, FontStyles.Bold, ThemeBuilderSlots.BodyPrimary);
            priceText.alignment = TextAlignmentOptions.Left;
            var priceLE = priceText.gameObject.AddComponent<LayoutElement>();
            priceLE.preferredWidth = 80f;

            var (buyGO, buyButton, buyImg) = CreatePrimaryButton(cellGO.transform, "BuyButton");
            var buyRT = buyGO.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.5f, 0f);
            buyRT.anchorMax = new Vector2(0.5f, 0f);
            buyRT.pivot = new Vector2(0.5f, 0f);
            buyRT.anchoredPosition = new Vector2(0f, 16f);
            buyRT.sizeDelta = new Vector2(160f, 48f);
            var buyLabel = CreateThemedText(buyGO.transform, "Label", "Buy", 22, FontStyles.Bold, ThemeBuilderSlots.PrimaryButtonLabel);
            buyLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(buyLabel.GetComponent<RectTransform>());

            var view = cellGO.AddComponent<ShopItemView>();
            var so = new SerializedObject(view);
            so.FindProperty("_refs.DisplayNameLabel").objectReferenceValue = nameText;
            so.FindProperty("_refs.PriceLabel").objectReferenceValue = priceText;
            so.FindProperty("_refs.CurrencyIcon").objectReferenceValue = currencyIcon;
            so.FindProperty("_refs.BuyButton").objectReferenceValue = buyButton;
            so.FindProperty("_refs.BuyTint").objectReferenceValue = buyImg;
            so.ApplyModifiedPropertiesWithoutUndo();

            cellGO.SetActive(false);
            return view;
        }

        private static NotEnoughCurrencyPopup BuildNotEnoughCurrencyPopup()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = CreateRoot("NotEnoughCurrencyPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 760f));

            var headerGO = CreateChild(card.transform, "HeaderTint");
            var headerRT = headerGO.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0f, 12f);
            var headerTint = AddThemedImage(headerGO, HeaderTintColor, ThemeSpriteSlot.None, ThemeColorSlot.WarningColor);

            var title = CreateThemedText(card.transform, "Title", "Not enough!", 40, FontStyles.Bold, ThemeBuilderSlots.Heading);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -40f, 64f);
            title.alignment = TextAlignmentOptions.Center;

            var iconGO = CreateChild(card.transform, "CurrencyIcon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 1f);
            iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.pivot = new Vector2(0.5f, 1f);
            iconRT.anchoredPosition = new Vector2(0f, -130f);
            iconRT.sizeDelta = new Vector2(140f, 140f);
            var icon = AddImage(iconGO, Color.white);
            if (theme != null) icon.sprite = theme.IconCoin;

            var message = CreateThemedText(card.transform, "Message", "You need 50 more Coins.", 26, FontStyles.Normal, ThemeBuilderSlots.Body);
            var messageRT = message.GetComponent<RectTransform>();
            messageRT.anchorMin = new Vector2(0f, 1f);
            messageRT.anchorMax = new Vector2(1f, 1f);
            messageRT.pivot = new Vector2(0.5f, 1f);
            messageRT.anchoredPosition = new Vector2(0f, -300f);
            messageRT.sizeDelta = new Vector2(-60f, 60f);
            message.alignment = TextAlignmentOptions.Center;

            var buttons = CreateChild(card.transform, "Buttons");
            var btnRT = buttons.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 0f);
            btnRT.anchorMax = new Vector2(1f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 40f);
            btnRT.sizeDelta = new Vector2(-60f, 240f);
            var btnLayout = buttons.AddComponent<VerticalLayoutGroup>();
            btnLayout.spacing = 12f;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = false;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;

            var (watchGO, watchButton, _) = CreatePrimaryButton(buttons.transform, "WatchAdButton");
            var watchLabel = CreateThemedText(watchGO.transform, "Label", "Watch ad", 26, FontStyles.Bold, ThemeBuilderSlots.PrimaryButtonLabel);
            watchLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(watchLabel.GetComponent<RectTransform>());
            ForceButtonHeight(watchGO, 72f);

            var (buyMoreGO, buyMoreButton, _) = CreateSecondaryButton(buttons.transform, "BuyMoreButton");
            var buyMoreLabel = CreateThemedText(buyMoreGO.transform, "Label", "Buy more", 26, FontStyles.Bold, ThemeBuilderSlots.SecondaryButtonLabel);
            buyMoreLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(buyMoreLabel.GetComponent<RectTransform>());
            ForceButtonHeight(buyMoreGO, 72f);

            var (declineGO, declineButton, _) = CreateSecondaryButton(buttons.transform, "DeclineButton");
            var declineLabel = CreateThemedText(declineGO.transform, "Label", "No thanks", 24, FontStyles.Normal, ThemeBuilderSlots.SecondaryButtonLabel);
            declineLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(declineLabel.GetComponent<RectTransform>());
            ForceButtonHeight(declineGO, 56f);
            declineGO.SetActive(false);

            var animator = root.AddComponent<UIAnimNotEnoughCurrencyPopup>();
            var popup = root.AddComponent<NotEnoughCurrencyPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.MessageLabel").objectReferenceValue = message;
            so.FindProperty("_refs.CurrencyIcon").objectReferenceValue = icon;
            so.FindProperty("_refs.BuyMoreLabel").objectReferenceValue = buyMoreLabel;
            so.FindProperty("_refs.WatchAdLabel").objectReferenceValue = watchLabel;
            so.FindProperty("_refs.DeclineLabel").objectReferenceValue = declineLabel;
            so.FindProperty("_refs.BuyMoreButton").objectReferenceValue = buyMoreButton;
            so.FindProperty("_refs.WatchAdButton").objectReferenceValue = watchButton;
            so.FindProperty("_refs.DeclineButton").objectReferenceValue = declineButton;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.HeaderTint").objectReferenceValue = headerTint;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<NotEnoughCurrencyPopup>(root, NotEnoughPath);
        }

        private static HUDCurrency BuildHUDCoins()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var (root, icon, label, button) = BuildHUDShell("HUDCoins", theme != null ? theme.IconCoin : null, "0");
            var hud = root.AddComponent<HUDCurrency>();
            var so = new SerializedObject(hud);
            so.FindProperty("_currency").enumValueIndex = (int)CurrencyType.Coins;
            so.FindProperty("_refs.IconImage").objectReferenceValue = icon;
            so.FindProperty("_refs.CountLabel").objectReferenceValue = label;
            so.FindProperty("_refs.ClickButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();
            return SaveAsPrefab<HUDCurrency>(root, HUDCoinsPath);
        }

        private static HUDCurrency BuildHUDGems()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var (root, icon, label, button) = BuildHUDShell("HUDGems", theme != null ? theme.IconGem : null, "0");
            var hud = root.AddComponent<HUDCurrency>();
            var so = new SerializedObject(hud);
            so.FindProperty("_currency").enumValueIndex = (int)CurrencyType.Gems;
            so.FindProperty("_refs.IconImage").objectReferenceValue = icon;
            so.FindProperty("_refs.CountLabel").objectReferenceValue = label;
            so.FindProperty("_refs.ClickButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();
            return SaveAsPrefab<HUDCurrency>(root, HUDGemsPath);
        }

        private static (GameObject root, Image icon, TMP_Text label, Button button) BuildHUDShell(string name, Sprite iconSprite, string defaultText)
        {
            var root = new GameObject(name);
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.5f, 0.5f);
            rootRT.anchorMax = new Vector2(0.5f, 0.5f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = new Vector2(220f, 80f);
            var bg = AddImage(root, HUDBackgroundColor);

            var iconGO = CreateChild(root.transform, "Icon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.anchoredPosition = new Vector2(12f, 0f);
            iconRT.sizeDelta = new Vector2(56f, 56f);
            var icon = AddImage(iconGO, Color.white);
            if (iconSprite != null) icon.sprite = iconSprite;

            var label = CreateThemedText(root.transform, "CountLabel", defaultText, 32, FontStyles.Bold, ThemeBuilderSlots.DarkBgBody);
            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0f);
            labelRT.anchorMax = new Vector2(1f, 1f);
            labelRT.offsetMin = new Vector2(76f, 0f);
            labelRT.offsetMax = new Vector2(-12f, 0f);
            label.alignment = TextAlignmentOptions.Left;

            var button = root.AddComponent<Button>();
            button.targetGraphic = bg;

            return (root, icon, label, button);
        }
    }
}
