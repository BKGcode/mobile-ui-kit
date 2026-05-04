using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.HUD;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
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
    public static class CatalogGroupCBuilder
    {
        private const string OutputRoot = "Assets/Catalog_GroupC_Demo";
        private const string PrefabsFolder = OutputRoot + "/Prefabs";
        private const string DailyLoginPath = PrefabsFolder + "/DailyLoginPopup.prefab";
        private const string LevelCompletePath = PrefabsFolder + "/LevelCompletePopup.prefab";
        private const string GameOverPath = PrefabsFolder + "/GameOverPopup.prefab";
        private const string HUDEnergyPath = PrefabsFolder + "/HUDEnergy.prefab";
        private const string HUDTimerPath = PrefabsFolder + "/HUDTimer.prefab";
        private const string ScenePath = OutputRoot + "/Catalog_GroupC_Demo.unity";
        private const string GroupBRewardPath = "Assets/Catalog_GroupB_Demo/Prefabs/RewardPopup.prefab";
        private const string DefaultThemePath = "Assets/Settings/UI/UIThemeConfig_Default.asset";
        private const string GroupBStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupB";
        private const string GroupCStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string ProgressionServiceTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryProgressionService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string TimeServiceTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryTimeService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string DemoMonoBehaviourTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.CatalogGroupCDemo, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";

        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color CardColor = new Color(0.97f, 0.98f, 1f, 1f);
        private static readonly Color ButtonPrimaryColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color ButtonSecondaryColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        private static readonly Color SuccessTintColor = new Color(0.30f, 0.78f, 0.45f, 1f);
        private static readonly Color WarningColor = new Color(0.95f, 0.62f, 0.10f, 1f);
        private static readonly Color FailureColor = new Color(0.898f, 0.224f, 0.208f, 1f);
        private static readonly Color HUDBackgroundColor = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color HUDCapLabelColor = new Color(0.85f, 0.85f, 0.90f, 1f);
        private static readonly Color TextDarkColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        private static readonly Color TextLightColor = Color.white;

        [MenuItem("Tools/Kitforge/UI Kit/Build Group C Sample")]
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
            BuildDailyLoginPopup();
            BuildLevelCompletePopup();
            BuildGameOverPopup();
            BuildHUDEnergy();
            BuildHUDTimer();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildDemoScene();
            EditorUtility.DisplayDialog(
                "Kitforge UI Kit",
                $"Group C built at {OutputRoot}.\n\n5 prefabs + 1 scene generated. Open the scene, press Play, right-click the Demo GameObject and pick a Context Menu scenario (try 'Chain — LevelComplete → Reward sequence (3 rewards)' to see the post-level chain).",
                "OK");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) AssetDatabase.CreateFolder("Assets", "Catalog_GroupC_Demo");
            if (!AssetDatabase.IsValidFolder(PrefabsFolder)) AssetDatabase.CreateFolder(OutputRoot, "Prefabs");
        }

        private static DailyLoginPopup BuildDailyLoginPopup()
        {
            var root = CreateRoot("DailyLoginPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(900f, 1200f));

            var title = CreateText(card.transform, "Title", "Daily Reward", 44, FontStyles.Bold);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 72f);
            title.alignment = TextAlignmentOptions.Center;

            var dayCellContainer = CreateChild(card.transform, "DayCellContainer");
            var cellRT = dayCellContainer.GetComponent<RectTransform>();
            cellRT.anchorMin = new Vector2(0f, 1f);
            cellRT.anchorMax = new Vector2(1f, 1f);
            cellRT.pivot = new Vector2(0.5f, 1f);
            cellRT.anchoredPosition = new Vector2(0f, -120f);
            cellRT.sizeDelta = new Vector2(-60f, 600f);
            var cellLayout = dayCellContainer.AddComponent<GridLayoutGroup>();
            cellLayout.cellSize = new Vector2(180f, 180f);
            cellLayout.spacing = new Vector2(12f, 12f);
            cellLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cellLayout.constraintCount = 4;
            cellLayout.childAlignment = TextAnchor.UpperCenter;

            var countdown = CreateText(card.transform, "CountdownLabel", "00:00:00", 32, FontStyles.Bold);
            var countdownRT = countdown.GetComponent<RectTransform>();
            countdownRT.anchorMin = new Vector2(0.5f, 0f);
            countdownRT.anchorMax = new Vector2(0.5f, 0f);
            countdownRT.pivot = new Vector2(0.5f, 0f);
            countdownRT.anchoredPosition = new Vector2(0f, 240f);
            countdownRT.sizeDelta = new Vector2(420f, 60f);
            countdown.alignment = TextAlignmentOptions.Center;
            countdown.color = WarningColor;
            countdown.gameObject.SetActive(false);

            var (claimGO, claimButton, claimImg) = CreatePrimaryButton(card.transform, "ClaimButton");
            var claimRT = claimGO.GetComponent<RectTransform>();
            claimRT.anchorMin = new Vector2(0.5f, 0f);
            claimRT.anchorMax = new Vector2(0.5f, 0f);
            claimRT.pivot = new Vector2(0.5f, 0f);
            claimRT.anchoredPosition = new Vector2(0f, 130f);
            claimRT.sizeDelta = new Vector2(440f, 96f);
            claimImg.color = SuccessTintColor;
            var claimLabel = CreateText(claimGO.transform, "Label", "Claim", 30, FontStyles.Bold);
            claimLabel.color = TextLightColor;
            claimLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(claimLabel.GetComponent<RectTransform>());

            var (watchGO, watchButton, _) = CreateSecondaryButton(card.transform, "WatchToDoubleButton");
            var watchRT = watchGO.GetComponent<RectTransform>();
            watchRT.anchorMin = new Vector2(0.5f, 0f);
            watchRT.anchorMax = new Vector2(0.5f, 0f);
            watchRT.pivot = new Vector2(0.5f, 0f);
            watchRT.anchoredPosition = new Vector2(0f, 30f);
            watchRT.sizeDelta = new Vector2(380f, 72f);
            var watchLabel = CreateText(watchGO.transform, "Label", "Watch ad to double", 24, FontStyles.Bold);
            watchLabel.color = TextDarkColor;
            watchLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(watchLabel.GetComponent<RectTransform>());
            watchGO.SetActive(false);

            var animator = root.AddComponent<UIAnimDailyLoginPopup>();
            var popup = root.AddComponent<DailyLoginPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.ClaimButton").objectReferenceValue = claimButton;
            so.FindProperty("_refs.ClaimLabel").objectReferenceValue = claimLabel;
            so.FindProperty("_refs.CountdownLabel").objectReferenceValue = countdown;
            so.FindProperty("_refs.WatchToDoubleButton").objectReferenceValue = watchButton;
            so.FindProperty("_refs.WatchToDoubleLabel").objectReferenceValue = watchLabel;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.DayCellContainer").objectReferenceValue = cellRT;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<DailyLoginPopup>(root, DailyLoginPath);
        }

        private static LevelCompletePopup BuildLevelCompletePopup()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = CreateRoot("LevelCompletePopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(900f, 1300f));

            var title = CreateText(card.transform, "Title", "Level Complete!", 48, FontStyles.Bold);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 80f);
            title.alignment = TextAlignmentOptions.Center;

            var levelLabel = CreateText(card.transform, "LevelLabel", "", 28, FontStyles.Italic);
            AnchorTopOfCard(levelLabel.GetComponent<RectTransform>(), -120f, 48f);
            levelLabel.alignment = TextAlignmentOptions.Center;
            levelLabel.gameObject.SetActive(false);

            var starsRow = CreateChild(card.transform, "StarsRow");
            var starsRT = starsRow.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(0f, 1f);
            starsRT.anchorMax = new Vector2(1f, 1f);
            starsRT.pivot = new Vector2(0.5f, 1f);
            starsRT.anchoredPosition = new Vector2(0f, -200f);
            starsRT.sizeDelta = new Vector2(-60f, 200f);
            var starsLayout = starsRow.AddComponent<HorizontalLayoutGroup>();
            starsLayout.spacing = 20f;
            starsLayout.childAlignment = TextAnchor.MiddleCenter;
            starsLayout.childForceExpandWidth = false;
            starsLayout.childForceExpandHeight = false;
            starsLayout.childControlWidth = false;
            starsLayout.childControlHeight = false;

            var starImages = new Image[3];
            for (var i = 0; i < 3; i++)
            {
                var starGO = CreateChild(starsRow.transform, $"Star{i}");
                starGO.GetComponent<RectTransform>().sizeDelta = new Vector2(160f, 160f);
                var starImg = AddImage(starGO, Color.white);
                if (theme != null && theme.StarEmptySprite != null) starImg.sprite = theme.StarEmptySprite;
                starImages[i] = starImg;
            }

            var score = CreateText(card.transform, "ScoreLabel", "0", 64, FontStyles.Bold);
            var scoreRT = score.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.5f, 1f);
            scoreRT.anchorMax = new Vector2(0.5f, 1f);
            scoreRT.pivot = new Vector2(0.5f, 1f);
            scoreRT.anchoredPosition = new Vector2(0f, -440f);
            scoreRT.sizeDelta = new Vector2(700f, 90f);
            score.alignment = TextAlignmentOptions.Center;

            var bestScore = CreateText(card.transform, "BestScoreLabel", "Best: 0", 26, FontStyles.Italic);
            var bestRT = bestScore.GetComponent<RectTransform>();
            bestRT.anchorMin = new Vector2(0.5f, 1f);
            bestRT.anchorMax = new Vector2(0.5f, 1f);
            bestRT.pivot = new Vector2(0.5f, 1f);
            bestRT.anchoredPosition = new Vector2(0f, -540f);
            bestRT.sizeDelta = new Vector2(500f, 40f);
            bestScore.alignment = TextAlignmentOptions.Center;
            bestScore.color = new Color(0.40f, 0.42f, 0.48f, 1f);

            var banner = CreateChild(card.transform, "NewBestBanner");
            var bannerRT = banner.GetComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0f, 1f);
            bannerRT.anchorMax = new Vector2(1f, 1f);
            bannerRT.pivot = new Vector2(0.5f, 1f);
            bannerRT.anchoredPosition = new Vector2(0f, -610f);
            bannerRT.sizeDelta = new Vector2(-100f, 64f);
            AddImage(banner, SuccessTintColor);
            var bannerLabel = CreateText(banner.transform, "Label", "NEW BEST!", 30, FontStyles.Bold);
            bannerLabel.color = TextLightColor;
            bannerLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(bannerLabel.GetComponent<RectTransform>());
            banner.SetActive(false);

            var buttons = CreateChild(card.transform, "Buttons");
            var btnRT = buttons.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 0f);
            btnRT.anchorMax = new Vector2(1f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 40f);
            btnRT.sizeDelta = new Vector2(-60f, 320f);
            var btnLayout = buttons.AddComponent<VerticalLayoutGroup>();
            btnLayout.spacing = 12f;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = false;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;

            var (nextGO, nextButton, _) = CreatePrimaryButton(buttons.transform, "NextButton");
            var nextLabel = CreateText(nextGO.transform, "Label", "Next", 30, FontStyles.Bold);
            nextLabel.color = TextLightColor;
            nextLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(nextLabel.GetComponent<RectTransform>());
            ForceButtonHeight(nextGO, 96f);

            var (retryGO, retryButton, _) = CreateSecondaryButton(buttons.transform, "RetryButton");
            var retryLabel = CreateText(retryGO.transform, "Label", "Retry", 28, FontStyles.Bold);
            retryLabel.color = TextDarkColor;
            retryLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(retryLabel.GetComponent<RectTransform>());
            ForceButtonHeight(retryGO, 92f);

            var (mainMenuGO, mainMenuButton, _) = CreateSecondaryButton(buttons.transform, "MainMenuButton");
            var mainMenuLabel = CreateText(mainMenuGO.transform, "Label", "Main Menu", 26, FontStyles.Bold);
            mainMenuLabel.color = TextDarkColor;
            mainMenuLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(mainMenuLabel.GetComponent<RectTransform>());
            ForceButtonHeight(mainMenuGO, 80f);
            mainMenuGO.SetActive(false);

            var animator = root.AddComponent<UIAnimLevelCompletePopup>();
            var popup = root.AddComponent<LevelCompletePopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.LevelLabel").objectReferenceValue = levelLabel;
            var starArrayProp = so.FindProperty("_refs.StarImages");
            starArrayProp.arraySize = 3;
            for (var i = 0; i < 3; i++) starArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];
            so.FindProperty("_refs.ScoreLabel").objectReferenceValue = score;
            so.FindProperty("_refs.BestScoreLabel").objectReferenceValue = bestScore;
            so.FindProperty("_refs.NewBestBanner").objectReferenceValue = banner;
            so.FindProperty("_refs.NextButton").objectReferenceValue = nextButton;
            so.FindProperty("_refs.NextLabelText").objectReferenceValue = nextLabel;
            so.FindProperty("_refs.RetryButton").objectReferenceValue = retryButton;
            so.FindProperty("_refs.RetryLabelText").objectReferenceValue = retryLabel;
            so.FindProperty("_refs.MainMenuButton").objectReferenceValue = mainMenuButton;
            so.FindProperty("_refs.MainMenuLabelText").objectReferenceValue = mainMenuLabel;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<LevelCompletePopup>(root, LevelCompletePath);
        }

        private static GameOverPopup BuildGameOverPopup()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = CreateRoot("GameOverPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(900f, 1300f));

            var headerTintGO = CreateChild(card.transform, "HeaderTint");
            var headerRT = headerTintGO.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0f, 14f);
            var headerTint = AddImage(headerTintGO, FailureColor);

            var title = CreateText(card.transform, "Title", "Game Over", 48, FontStyles.Bold);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -40f, 80f);
            title.alignment = TextAlignmentOptions.Center;

            var subtitle = CreateText(card.transform, "Subtitle", "", 28, FontStyles.Italic);
            AnchorTopOfCard(subtitle.GetComponent<RectTransform>(), -130f, 48f);
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.gameObject.SetActive(false);

            var scoreBlock = CreateChild(card.transform, "ScoreBlock");
            var scoreBlockRT = scoreBlock.GetComponent<RectTransform>();
            scoreBlockRT.anchorMin = new Vector2(0.5f, 1f);
            scoreBlockRT.anchorMax = new Vector2(0.5f, 1f);
            scoreBlockRT.pivot = new Vector2(0.5f, 1f);
            scoreBlockRT.anchoredPosition = new Vector2(0f, -260f);
            scoreBlockRT.sizeDelta = new Vector2(700f, 110f);
            var scoreLabel = CreateText(scoreBlock.transform, "ScoreLabel", "0", 64, FontStyles.Bold);
            StretchInside(scoreLabel.GetComponent<RectTransform>());
            scoreLabel.alignment = TextAlignmentOptions.Center;

            var buttons = CreateChild(card.transform, "Buttons");
            var btnRT = buttons.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 0f);
            btnRT.anchorMax = new Vector2(1f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 40f);
            btnRT.sizeDelta = new Vector2(-60f, 480f);
            var btnLayout = buttons.AddComponent<VerticalLayoutGroup>();
            btnLayout.spacing = 12f;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = false;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;

            var (continueAdGO, continueAdButton, _) = CreatePrimaryButton(buttons.transform, "ContinueAdButton");
            var continueAdLabel = CreateText(continueAdGO.transform, "Label", "Continue", 30, FontStyles.Bold);
            continueAdLabel.color = TextLightColor;
            continueAdLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(continueAdLabel.GetComponent<RectTransform>());
            ForceButtonHeight(continueAdGO, 96f);

            var (continueCurrGO, continueCurrButton, _) = CreateSecondaryButton(buttons.transform, "ContinueCurrencyButton");
            var continueCurrIconGO = CreateChild(continueCurrGO.transform, "CurrencyIcon");
            var continueCurrIconRT = continueCurrIconGO.GetComponent<RectTransform>();
            continueCurrIconRT.anchorMin = new Vector2(0f, 0.5f);
            continueCurrIconRT.anchorMax = new Vector2(0f, 0.5f);
            continueCurrIconRT.pivot = new Vector2(0f, 0.5f);
            continueCurrIconRT.anchoredPosition = new Vector2(40f, 0f);
            continueCurrIconRT.sizeDelta = new Vector2(48f, 48f);
            var continueCurrIcon = AddImage(continueCurrIconGO, Color.white);
            if (theme != null && theme.IconGem != null) continueCurrIcon.sprite = theme.IconGem;
            var continueCurrLabel = CreateText(continueCurrGO.transform, "Label", "Continue (5)", 26, FontStyles.Bold);
            continueCurrLabel.color = TextDarkColor;
            continueCurrLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(continueCurrLabel.GetComponent<RectTransform>());
            ForceButtonHeight(continueCurrGO, 96f);

            var (restartGO, restartButton, _) = CreateSecondaryButton(buttons.transform, "RestartButton");
            var restartLabel = CreateText(restartGO.transform, "Label", "Restart", 28, FontStyles.Bold);
            restartLabel.color = TextDarkColor;
            restartLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(restartLabel.GetComponent<RectTransform>());
            ForceButtonHeight(restartGO, 92f);

            var (mainMenuGO, mainMenuButton, _) = CreateSecondaryButton(buttons.transform, "MainMenuButton");
            var mainMenuLabel = CreateText(mainMenuGO.transform, "Label", "Main Menu", 26, FontStyles.Bold);
            mainMenuLabel.color = TextDarkColor;
            mainMenuLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(mainMenuLabel.GetComponent<RectTransform>());
            ForceButtonHeight(mainMenuGO, 80f);

            var animator = root.AddComponent<UIAnimGameOverPopup>();
            var popup = root.AddComponent<GameOverPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.SubtitleLabel").objectReferenceValue = subtitle;
            so.FindProperty("_refs.ScoreBlock").objectReferenceValue = scoreBlock;
            so.FindProperty("_refs.ScoreLabel").objectReferenceValue = scoreLabel;
            so.FindProperty("_refs.ContinueAdButton").objectReferenceValue = continueAdButton;
            so.FindProperty("_refs.ContinueAdLabelText").objectReferenceValue = continueAdLabel;
            so.FindProperty("_refs.ContinueCurrencyButton").objectReferenceValue = continueCurrButton;
            so.FindProperty("_refs.ContinueCurrencyLabelText").objectReferenceValue = continueCurrLabel;
            so.FindProperty("_refs.ContinueCurrencyIcon").objectReferenceValue = continueCurrIcon;
            so.FindProperty("_refs.RestartButton").objectReferenceValue = restartButton;
            so.FindProperty("_refs.RestartLabelText").objectReferenceValue = restartLabel;
            so.FindProperty("_refs.MainMenuButton").objectReferenceValue = mainMenuButton;
            so.FindProperty("_refs.MainMenuLabelText").objectReferenceValue = mainMenuLabel;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.HeaderTint").objectReferenceValue = headerTint;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<GameOverPopup>(root, GameOverPath);
        }

        private static HUDEnergy BuildHUDEnergy()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = new GameObject("HUDEnergy");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.5f, 0.5f);
            rootRT.anchorMax = new Vector2(0.5f, 0.5f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = new Vector2(280f, 100f);
            var bg = AddImage(root, HUDBackgroundColor);
            var clickButton = root.AddComponent<Button>();
            clickButton.targetGraphic = bg;

            var iconGO = CreateChild(root.transform, "Icon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.anchoredPosition = new Vector2(14f, 0f);
            iconRT.sizeDelta = new Vector2(64f, 64f);
            var icon = AddImage(iconGO, Color.white);
            if (theme != null && theme.IconEnergy != null) icon.sprite = theme.IconEnergy;

            var countLabel = CreateText(root.transform, "CountLabel", "3", 32, FontStyles.Bold);
            countLabel.color = TextLightColor;
            var countRT = countLabel.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0f, 1f);
            countRT.anchorMax = new Vector2(0f, 1f);
            countRT.pivot = new Vector2(0f, 1f);
            countRT.anchoredPosition = new Vector2(90f, -10f);
            countRT.sizeDelta = new Vector2(80f, 44f);
            countLabel.alignment = TextAlignmentOptions.Left;

            var capLabel = CreateText(root.transform, "MaxCapLabel", "/5", 24, FontStyles.Bold);
            capLabel.color = HUDCapLabelColor;
            var capRT = capLabel.GetComponent<RectTransform>();
            capRT.anchorMin = new Vector2(0f, 1f);
            capRT.anchorMax = new Vector2(0f, 1f);
            capRT.pivot = new Vector2(0f, 1f);
            capRT.anchoredPosition = new Vector2(150f, -16f);
            capRT.sizeDelta = new Vector2(60f, 36f);
            capLabel.alignment = TextAlignmentOptions.Left;

            var regenLabel = CreateText(root.transform, "RegenCountdownLabel", "+1 in 04:30", 16, FontStyles.Normal);
            regenLabel.color = WarningColor;
            var regenRT = regenLabel.GetComponent<RectTransform>();
            regenRT.anchorMin = new Vector2(0f, 0f);
            regenRT.anchorMax = new Vector2(1f, 0f);
            regenRT.pivot = new Vector2(0.5f, 0f);
            regenRT.anchoredPosition = new Vector2(45f, 22f);
            regenRT.sizeDelta = new Vector2(-100f, 22f);
            regenLabel.alignment = TextAlignmentOptions.Left;

            var barFillGO = CreateChild(root.transform, "EnergyBarFill");
            var barRT = barFillGO.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(1f, 0f);
            barRT.pivot = new Vector2(0.5f, 0f);
            barRT.anchoredPosition = new Vector2(45f, 8f);
            barRT.sizeDelta = new Vector2(-100f, 8f);
            var barFill = AddImage(barFillGO, SuccessTintColor);
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillAmount = 0.6f;

            var hud = root.AddComponent<HUDEnergy>();
            var so = new SerializedObject(hud);
            so.FindProperty("_currency").enumValueIndex = (int)CurrencyType.Energy;
            so.FindProperty("_refs.IconImage").objectReferenceValue = icon;
            so.FindProperty("_refs.CountLabel").objectReferenceValue = countLabel;
            so.FindProperty("_refs.ClickButton").objectReferenceValue = clickButton;
            so.FindProperty("_energyRefs.RegenCountdownLabel").objectReferenceValue = regenLabel;
            so.FindProperty("_energyRefs.MaxCapLabel").objectReferenceValue = capLabel;
            so.FindProperty("_energyRefs.EnergyBarFill").objectReferenceValue = barFill;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<HUDEnergy>(root, HUDEnergyPath);
        }

        private static HUDTimer BuildHUDTimer()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var root = new GameObject("HUDTimer");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.5f, 0.5f);
            rootRT.anchorMax = new Vector2(0.5f, 0.5f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = new Vector2(220f, 64f);
            var bg = AddImage(root, HUDBackgroundColor);
            var clickButton = root.AddComponent<Button>();
            clickButton.targetGraphic = bg;

            var iconGO = CreateChild(root.transform, "Icon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.anchoredPosition = new Vector2(12f, 0f);
            iconRT.sizeDelta = new Vector2(40f, 40f);
            var icon = AddImage(iconGO, Color.white);
            if (theme != null && theme.IconClock != null) icon.sprite = theme.IconClock;

            var label = CreateText(root.transform, "Label", "00:00", 36, FontStyles.Bold);
            label.color = TextLightColor;
            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0f);
            labelRT.anchorMax = new Vector2(1f, 1f);
            labelRT.offsetMin = new Vector2(60f, 0f);
            labelRT.offsetMax = new Vector2(-12f, 0f);
            label.alignment = TextAlignmentOptions.Left;

            var hud = root.AddComponent<HUDTimer>();
            var so = new SerializedObject(hud);
            so.FindProperty("_refs.Label").objectReferenceValue = label;
            so.FindProperty("_refs.IconImage").objectReferenceValue = icon;
            so.FindProperty("_refs.ClickButton").objectReferenceValue = clickButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<HUDTimer>(root, HUDTimerPath);
        }

        private static void BuildDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var dailyLoginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DailyLoginPath);
            var levelCompletePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelCompletePath);
            var gameOverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameOverPath);
            var hudEnergyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUDEnergyPath);
            var hudTimerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HUDTimerPath);
            var rewardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GroupBRewardPath);
            if (theme == null)
            {
                Debug.LogError($"[CatalogGroupCBuilder] Theme not found at {DefaultThemePath}. Run 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first.");
            }
            if (dailyLoginPrefab == null || levelCompletePrefab == null || gameOverPrefab == null || hudEnergyPrefab == null || hudTimerPrefab == null)
            {
                Debug.LogError($"[CatalogGroupCBuilder] Failed to load Group C prefabs. dailyLogin={dailyLoginPrefab != null} levelComplete={levelCompletePrefab != null} gameOver={gameOverPrefab != null} hudEnergy={hudEnergyPrefab != null} hudTimer={hudTimerPrefab != null}");
            }
            if (rewardPrefab == null)
            {
                Debug.LogWarning($"[CatalogGroupCBuilder] RewardPopup prefab not found at {GroupBRewardPath}. The 'LevelComplete → Reward sequence' chain demo requires it. Run 'Tools/Kitforge/UI Kit/Build Group B Sample' first to generate it.");
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
            hudRT.sizeDelta = new Vector2(300f, 240f);
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
            TryAttachStubServices(servicesGO, services);

            HUDEnergy hudEnergyInstance = null;
            HUDTimer hudTimerInstance = null;
            if (hudEnergyPrefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(hudEnergyPrefab, hudParent.transform);
                hudEnergyInstance = instance.GetComponent<HUDEnergy>();
                if (hudEnergyInstance != null) WireHUDServices(hudEnergyInstance, services);
            }
            if (hudTimerPrefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(hudTimerPrefab, hudParent.transform);
                hudTimerInstance = instance.GetComponent<HUDTimer>();
                if (hudTimerInstance != null) WireHUDServices(hudTimerInstance, services);
            }

            var demoGO = new GameObject("Demo");
            SceneManager.MoveGameObjectToScene(demoGO, scene);
            var demoType = System.Type.GetType(DemoMonoBehaviourTypeName);
            if (demoType == null)
            {
                Debug.LogWarning("[CatalogGroupCBuilder] CatalogGroupCDemo type not found. Make sure the Group C sample is imported (Package Manager → Samples) so its asmdef compiles.");
            }
            else
            {
                var demo = demoGO.AddComponent(demoType);
                var so = new SerializedObject(demo);
                so.FindProperty("_popupParent").objectReferenceValue = popupParent.GetComponent<RectTransform>();
                so.FindProperty("_theme").objectReferenceValue = theme;
                so.FindProperty("_services").objectReferenceValue = services;
                so.FindProperty("_dailyLoginPrefab").objectReferenceValue = dailyLoginPrefab;
                so.FindProperty("_levelCompletePrefab").objectReferenceValue = levelCompletePrefab;
                so.FindProperty("_gameOverPrefab").objectReferenceValue = gameOverPrefab;
                so.FindProperty("_rewardPrefab").objectReferenceValue = rewardPrefab;
                so.FindProperty("_hudEnergy").objectReferenceValue = hudEnergyInstance;
                so.FindProperty("_hudTimer").objectReferenceValue = hudTimerInstance;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void TryAttachStubServices(GameObject host, UIServices services)
        {
            var economyType = System.Type.GetType($"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryEconomyService, {GroupBStubAsmdef}");
            var shopType = System.Type.GetType($"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryShopDataProvider, {GroupBStubAsmdef}");
            var adsType = System.Type.GetType($"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryAdsService, {GroupBStubAsmdef}");
            var timeType = System.Type.GetType(TimeServiceTypeName);
            var progressionType = System.Type.GetType(ProgressionServiceTypeName);
            if (economyType == null || shopType == null || adsType == null)
            {
                Debug.LogWarning("[CatalogGroupCBuilder] Group B stubs not found. Import 'Catalog — Group B — Currency' sample (Package Manager → Samples) so its asmdef compiles, then re-run this builder.");
            }
            if (timeType == null || progressionType == null)
            {
                Debug.LogWarning("[CatalogGroupCBuilder] Group C stubs not found. Import 'Catalog — Group C — Progression' sample (Package Manager → Samples) so its asmdef compiles, then re-run this builder.");
            }

            var economy = economyType != null ? (MonoBehaviour)host.AddComponent(economyType) : null;
            var shop = shopType != null ? (MonoBehaviour)host.AddComponent(shopType) : null;
            var ads = adsType != null ? (MonoBehaviour)host.AddComponent(adsType) : null;
            var time = timeType != null ? (MonoBehaviour)host.AddComponent(timeType) : null;
            var progression = progressionType != null ? (MonoBehaviour)host.AddComponent(progressionType) : null;

            if (progression != null && economy != null)
            {
                var progressionSO = new SerializedObject(progression);
                progressionSO.FindProperty("_economyServiceRef").objectReferenceValue = economy;
                progressionSO.ApplyModifiedPropertiesWithoutUndo();
            }

            var so = new SerializedObject(services);
            if (economy != null) so.FindProperty("_economyServiceRef").objectReferenceValue = economy;
            if (shop != null) so.FindProperty("_shopDataProviderRef").objectReferenceValue = shop;
            if (ads != null) so.FindProperty("_adsServiceRef").objectReferenceValue = ads;
            if (time != null) so.FindProperty("_timeServiceRef").objectReferenceValue = time;
            if (progression != null) so.FindProperty("_progressionServiceRef").objectReferenceValue = progression;
            so.ApplyModifiedPropertiesWithoutUndo();
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
