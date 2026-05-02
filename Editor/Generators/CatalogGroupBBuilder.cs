using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.NotEnough;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Shop;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    public static class CatalogGroupBBuilder
    {
        private const string OutputRoot = "Assets/Catalog_GroupB_Demo";
        private const string PrefabsFolder = OutputRoot + "/Prefabs";
        private const string ScenePath = OutputRoot + "/Catalog_GroupB_Demo.unity";
        private const string RewardPath = PrefabsFolder + "/RewardPopup.prefab";
        private const string ShopPath = PrefabsFolder + "/ShopPopup.prefab";
        private const string NotEnoughPath = PrefabsFolder + "/NotEnoughCurrencyPopup.prefab";
        private const string HUDCoinsPath = PrefabsFolder + "/HUDCoins.prefab";
        private const string HUDGemsPath = PrefabsFolder + "/HUDGems.prefab";
        private const string DefaultThemePath = "Assets/Settings/UI/UIThemeConfig_Default.asset";

        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color CardColor = new Color(0.97f, 0.98f, 1f, 1f);
        private static readonly Color CellColor = new Color(0.94f, 0.95f, 0.98f, 1f);
        private static readonly Color HeaderTintColor = new Color(1.00f, 0.60f, 0.15f, 1f);
        private static readonly Color ButtonPrimaryColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color ButtonSecondaryColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        private static readonly Color SuccessTintColor = new Color(0.30f, 0.78f, 0.45f, 1f);
        private static readonly Color HUDBackgroundColor = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color TextDarkColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        private static readonly Color TextLightColor = Color.white;

        [MenuItem("Tools/Kitforge/UI Kit/Build Group B Sample")]
        public static void BuildAll()
        {
            EnsureFolders();
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath) == null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Bootstrap Defaults missing",
                    $"No Theme found at {DefaultThemePath}.\n\nRun 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first, or proceed without a Theme reference (icons will be missing at runtime).",
                    "Proceed", "Cancel")) return;
            }
            BuildRewardPopup();
            BuildShopPopup();
            BuildNotEnoughCurrencyPopup();
            BuildHUDCoins();
            BuildHUDGems();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildDemoScene();
            EditorUtility.DisplayDialog(
                "Kitforge UI Kit",
                $"Group B built at {OutputRoot}.\n\n5 prefabs + 1 scene generated. Open the scene, press Play, right-click the Demo GameObject and pick a Context Menu scenario (try 'Chain — Shop → NotEnough → Ad → Reward' to see the full monetization loop).",
                "OK");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) AssetDatabase.CreateFolder("Assets", "Catalog_GroupB_Demo");
            if (!AssetDatabase.IsValidFolder(PrefabsFolder)) AssetDatabase.CreateFolder(OutputRoot, "Prefabs");
        }

        private static RewardPopup BuildRewardPopup()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = CreateRoot("RewardPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 720f));

            var title = CreateText(card.transform, "Title", "Reward!", 40, FontStyles.Bold);
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

            var amount = CreateText(card.transform, "AmountLabel", "+100", 56, FontStyles.Bold);
            var amountRT = amount.GetComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.5f, 0.5f);
            amountRT.anchorMax = new Vector2(0.5f, 0.5f);
            amountRT.pivot = new Vector2(0.5f, 0.5f);
            amountRT.anchoredPosition = new Vector2(0f, -90f);
            amountRT.sizeDelta = new Vector2(600f, 80f);
            amount.alignment = TextAlignmentOptions.Center;

            var item = CreateText(card.transform, "ItemLabel", "", 32, FontStyles.Italic);
            var itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0.5f, 0.5f);
            itemRT.anchorMax = new Vector2(0.5f, 0.5f);
            itemRT.pivot = new Vector2(0.5f, 0.5f);
            itemRT.anchoredPosition = new Vector2(0f, -160f);
            itemRT.sizeDelta = new Vector2(600f, 60f);
            item.alignment = TextAlignmentOptions.Center;
            item.gameObject.SetActive(false);

            var (claimGO, claimButton, claimImg) = CreatePrimaryButton(card.transform, "ClaimButton");
            var claimRT = claimGO.GetComponent<RectTransform>();
            claimRT.anchorMin = new Vector2(0.5f, 0f);
            claimRT.anchorMax = new Vector2(0.5f, 0f);
            claimRT.pivot = new Vector2(0.5f, 0f);
            claimRT.anchoredPosition = new Vector2(0f, 40f);
            claimRT.sizeDelta = new Vector2(360f, 96f);
            claimImg.color = SuccessTintColor;
            var claimLabel = CreateText(claimGO.transform, "Label", "Claim", 30, FontStyles.Bold);
            claimLabel.color = TextLightColor;
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

            var title = CreateText(card.transform, "Title", "Shop", 44, FontStyles.Bold);
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
            var emptyText = CreateText(emptyGO.transform, "Label", "Shop is empty.", 28, FontStyles.Italic);
            StretchInside(emptyText.GetComponent<RectTransform>());
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.color = new Color(0.5f, 0.5f, 0.55f, 1f);
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

            var nameText = CreateText(cellGO.transform, "DisplayName", "Item", 22, FontStyles.Bold);
            var nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 1f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.pivot = new Vector2(0.5f, 1f);
            nameRT.anchoredPosition = new Vector2(0f, -10f);
            nameRT.sizeDelta = new Vector2(-12f, 50f);
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = TextDarkColor;

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

            var priceText = CreateText(priceRow.transform, "Price", "100", 24, FontStyles.Bold);
            priceText.color = TextDarkColor;
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
            var buyLabel = CreateText(buyGO.transform, "Label", "Buy", 22, FontStyles.Bold);
            buyLabel.color = TextLightColor;
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
            var headerTint = AddImage(headerGO, HeaderTintColor);

            var title = CreateText(card.transform, "Title", "Not enough!", 40, FontStyles.Bold);
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

            var message = CreateText(card.transform, "Message", "You need 50 more Coins.", 26, FontStyles.Normal);
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
            var watchLabel = CreateText(watchGO.transform, "Label", "Watch ad", 26, FontStyles.Bold);
            watchLabel.color = TextLightColor;
            watchLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(watchLabel.GetComponent<RectTransform>());
            ForceButtonHeight(watchGO, 72f);

            var (buyMoreGO, buyMoreButton, _) = CreateSecondaryButton(buttons.transform, "BuyMoreButton");
            var buyMoreLabel = CreateText(buyMoreGO.transform, "Label", "Buy more", 26, FontStyles.Bold);
            buyMoreLabel.color = TextDarkColor;
            buyMoreLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(buyMoreLabel.GetComponent<RectTransform>());
            ForceButtonHeight(buyMoreGO, 72f);

            var (declineGO, declineButton, _) = CreateSecondaryButton(buttons.transform, "DeclineButton");
            var declineLabel = CreateText(declineGO.transform, "Label", "No thanks", 24, FontStyles.Normal);
            declineLabel.color = TextDarkColor;
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

        private static HUDCoins BuildHUDCoins()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var (root, icon, label, button) = BuildHUDShell("HUDCoins", theme != null ? theme.IconCoin : null, "0");
            var hud = root.AddComponent<HUDCoins>();
            var so = new SerializedObject(hud);
            so.FindProperty("_refs.IconImage").objectReferenceValue = icon;
            so.FindProperty("_refs.CountLabel").objectReferenceValue = label;
            so.FindProperty("_refs.ClickButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();
            return SaveAsPrefab<HUDCoins>(root, HUDCoinsPath);
        }

        private static HUDGems BuildHUDGems()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var (root, icon, label, button) = BuildHUDShell("HUDGems", theme != null ? theme.IconGem : null, "0");
            var hud = root.AddComponent<HUDGems>();
            var so = new SerializedObject(hud);
            so.FindProperty("_refs.IconImage").objectReferenceValue = icon;
            so.FindProperty("_refs.CountLabel").objectReferenceValue = label;
            so.FindProperty("_refs.ClickButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();
            return SaveAsPrefab<HUDGems>(root, HUDGemsPath);
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

            var label = CreateText(root.transform, "CountLabel", defaultText, 32, FontStyles.Bold);
            label.color = TextLightColor;
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

        private static void BuildDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var rewardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RewardPath);
            var shopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopPath);
            var notEnoughPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NotEnoughPath);
            var hudCoinsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUDCoinsPath);
            var hudGemsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUDGemsPath);
            if (theme == null)
            {
                Debug.LogError($"[CatalogGroupBBuilder] Theme not found at {DefaultThemePath}. Run 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first.");
            }
            if (rewardPrefab == null || shopPrefab == null || notEnoughPrefab == null || hudCoinsPrefab == null || hudGemsPrefab == null)
            {
                Debug.LogError($"[CatalogGroupBBuilder] Failed to load one or more prefabs. reward={rewardPrefab != null} shop={shopPrefab != null} notEnough={notEnoughPrefab != null} hudCoins={hudCoinsPrefab != null} hudGems={hudGemsPrefab != null}");
            }

            var canvasGO = new GameObject("UICanvas");
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var popupParent = CreateChild(canvasGO.transform, "Popups");
            StretchInside(popupParent.GetComponent<RectTransform>());
            var hudParent = CreateChild(canvasGO.transform, "HUD");
            var hudRT = hudParent.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(1f, 1f);
            hudRT.anchorMax = new Vector2(1f, 1f);
            hudRT.pivot = new Vector2(1f, 1f);
            hudRT.anchoredPosition = new Vector2(-30f, -30f);
            hudRT.sizeDelta = new Vector2(240f, 200f);
            var hudLayout = hudParent.AddComponent<VerticalLayoutGroup>();
            hudLayout.spacing = 12f;
            hudLayout.childAlignment = TextAnchor.UpperRight;
            hudLayout.childForceExpandWidth = false;
            hudLayout.childForceExpandHeight = false;
            hudLayout.childControlWidth = false;
            hudLayout.childControlHeight = false;

            var eventSystemGO = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystemGO, scene);
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var servicesGO = new GameObject("UIServices");
            SceneManager.MoveGameObjectToScene(servicesGO, scene);
            var services = servicesGO.AddComponent<UIServices>();
            var (economyImpl, shopImpl, adsImpl) = TryAttachStubServices(servicesGO, services);

            HUDCoins hudCoinsInstance = null;
            HUDGems hudGemsInstance = null;
            if (hudCoinsPrefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(hudCoinsPrefab, hudParent.transform);
                hudCoinsInstance = instance.GetComponent<HUDCoins>();
                if (hudCoinsInstance != null) WireHUDServices(hudCoinsInstance, services);
            }
            if (hudGemsPrefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(hudGemsPrefab, hudParent.transform);
                hudGemsInstance = instance.GetComponent<HUDGems>();
                if (hudGemsInstance != null) WireHUDServices(hudGemsInstance, services);
            }

            var demoGO = new GameObject("Demo");
            SceneManager.MoveGameObjectToScene(demoGO, scene);
            var demoTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.CatalogGroupBDemo";
            var demoType = System.Type.GetType($"{demoTypeName}, KitforgeLabs.MobileUIKit.Samples.CatalogGroupB");
            if (demoType == null)
            {
                Debug.LogWarning("[CatalogGroupBBuilder] CatalogGroupBDemo type not found. Make sure the Group B sample is imported (Package Manager → Samples) so its asmdef compiles.");
            }
            else
            {
                var demo = demoGO.AddComponent(demoType);
                var so = new SerializedObject(demo);
                so.FindProperty("_popupParent").objectReferenceValue = popupParent.GetComponent<RectTransform>();
                so.FindProperty("_theme").objectReferenceValue = theme;
                so.FindProperty("_services").objectReferenceValue = services;
                so.FindProperty("_rewardPrefab").objectReferenceValue = rewardPrefab;
                so.FindProperty("_shopPrefab").objectReferenceValue = shopPrefab;
                so.FindProperty("_notEnoughPrefab").objectReferenceValue = notEnoughPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static (MonoBehaviour economy, MonoBehaviour shop, MonoBehaviour ads) TryAttachStubServices(GameObject host, UIServices services)
        {
            var stubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupB";
            var economyType = System.Type.GetType($"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryEconomyService, {stubAsmdef}");
            var shopType = System.Type.GetType($"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryShopDataProvider, {stubAsmdef}");
            var adsType = System.Type.GetType($"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryAdsService, {stubAsmdef}");
            if (economyType == null || shopType == null || adsType == null)
            {
                Debug.LogWarning("[CatalogGroupBBuilder] In-memory stubs not found. Import 'Catalog — Group B — Currency' sample (Package Manager → Samples) so its asmdef compiles, then re-run this builder.");
                return (null, null, null);
            }
            var economy = (MonoBehaviour)host.AddComponent(economyType);
            var shop = (MonoBehaviour)host.AddComponent(shopType);
            var ads = (MonoBehaviour)host.AddComponent(adsType);

            var so = new SerializedObject(services);
            so.FindProperty("_economyServiceRef").objectReferenceValue = economy;
            so.FindProperty("_shopDataProviderRef").objectReferenceValue = shop;
            so.FindProperty("_adsServiceRef").objectReferenceValue = ads;
            so.ApplyModifiedPropertiesWithoutUndo();
            return (economy, shop, ads);
        }

        private static void WireHUDServices(MonoBehaviour hud, UIServices services)
        {
            var so = new SerializedObject(hud);
            var prop = so.FindProperty("_services");
            if (prop != null) { prop.objectReferenceValue = services; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static GameObject CreateRoot(string name)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            StretchInside(rt);
            go.AddComponent<CanvasGroup>();
            return go;
        }

        private static Button CreateBackdrop(Transform parent)
        {
            var go = CreateChild(parent, "Backdrop");
            var img = AddImage(go, BackdropColor);
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }

        private static GameObject CreateCard(Transform parent, Vector2 size)
        {
            var go = CreateChild(parent, "Card");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            AddImage(go, CardColor);
            return go;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image AddImage(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, FontStyles style)
        {
            var go = CreateChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = TextDarkColor;
            return tmp;
        }

        private static (GameObject go, Button btn, Image img) CreateButton(Transform parent, string name, Color color)
        {
            var go = CreateChild(parent, name);
            var img = AddImage(go, color);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return (go, btn, img);
        }

        private static (GameObject go, Button btn, Image img) CreatePrimaryButton(Transform parent, string name)
        {
            return CreateButton(parent, name, ButtonPrimaryColor);
        }

        private static (GameObject go, Button btn, Image img) CreateSecondaryButton(Transform parent, string name)
        {
            return CreateButton(parent, name, ButtonSecondaryColor);
        }

        private static void StretchInside(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AnchorTopOfCard(RectTransform rt, float y, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, height);
        }

        private static void ForceButtonHeight(GameObject buttonGO, float height)
        {
            var le = buttonGO.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        private static void WireAnimatorCard(MonoBehaviour animator, RectTransform card)
        {
            var so = new SerializedObject(animator);
            var prop = so.FindProperty("_card");
            if (prop != null)
            {
                prop.objectReferenceValue = card;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static T SaveAsPrefab<T>(GameObject root, string path) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<T>();
        }
    }
}
