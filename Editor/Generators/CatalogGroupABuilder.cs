using KitforgeLabs.UIKit.Catalog.Confirm;
using KitforgeLabs.UIKit.Catalog.Pause;
using KitforgeLabs.UIKit.Catalog.Toasts;
using KitforgeLabs.UIKit.Catalog.Tutorial;
using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using KitforgeLabs.UIKit.Toast;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KitforgeLabs.UIKit.Editor.Generators.CatalogGroupBuilderShared;

namespace KitforgeLabs.UIKit.Editor.Generators
{
    internal static class CatalogGroupABuilder
    {
        private static string ConfirmPath => $"{ResolvePrefabsFolder()}/ConfirmPopup.prefab";
        private static string PausePath => $"{ResolvePrefabsFolder()}/PausePopup.prefab";
        private static string TutorialPath => $"{ResolvePrefabsFolder()}/TutorialPopup.prefab";
        private static string ToastPath => $"{ResolvePrefabsFolder()}/NotificationToast.prefab";

        private static readonly Color ToastBackgroundColor = new Color(0.18f, 0.20f, 0.24f, 0.95f);

        public static void BuildAll()
        {
            EnsurePrefabsFolder(ResolvePrefabsFolder());
            BuildConfirmPopup();
            BuildPausePopup();
            BuildTutorialPopup();
            BuildNotificationToast();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static ConfirmPopup BuildConfirmPopup()
        {
            var root = CreateRoot("ConfirmPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 420f));
            var toneStrip = CreateChild(card.transform, "ToneStrip");
            var toneStripRT = toneStrip.GetComponent<RectTransform>();
            toneStripRT.anchorMin = new Vector2(0f, 1f);
            toneStripRT.anchorMax = new Vector2(1f, 1f);
            toneStripRT.pivot = new Vector2(0.5f, 1f);
            toneStripRT.anchoredPosition = Vector2.zero;
            toneStripRT.sizeDelta = new Vector2(0f, 8f);
            var toneTint = AddImage(toneStrip, ButtonPrimaryColor);

            var title = CreateText(card.transform, "Title", "Title", 38, FontStyles.Bold);
            var titleRT = title.gameObject.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -30f);
            titleRT.sizeDelta = new Vector2(-40f, 56f);
            title.alignment = TextAlignmentOptions.Center;
            AddThemedText(title.gameObject, ThemeFontSlot.FontHeading, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);

            var message = CreateText(card.transform, "Message", "Message body goes here.", 26, FontStyles.Normal);
            var messageRT = message.gameObject.GetComponent<RectTransform>();
            messageRT.anchorMin = new Vector2(0f, 0f);
            messageRT.anchorMax = new Vector2(1f, 1f);
            messageRT.pivot = new Vector2(0.5f, 0.5f);
            messageRT.offsetMin = new Vector2(40f, 120f);
            messageRT.offsetMax = new Vector2(-40f, -110f);
            message.alignment = TextAlignmentOptions.Center;
            AddThemedText(message.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.TextSecondary, ThemeFontSizeSlot.None);

            var buttonRow = CreateChild(card.transform, "ButtonRow");
            var rowRT = buttonRow.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 0f);
            rowRT.anchorMax = new Vector2(1f, 0f);
            rowRT.pivot = new Vector2(0.5f, 0f);
            rowRT.anchoredPosition = new Vector2(0f, 30f);
            rowRT.sizeDelta = new Vector2(-60f, 80f);
            var rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;

            var (cancelGO, cancelButton, _) = CreateSecondaryButton(buttonRow.transform, "CancelButton");
            var cancelLabel = CreateText(cancelGO.transform, "Label", "Cancel", 28, FontStyles.Bold);
            cancelLabel.color = TextDarkColor;
            StretchInside(cancelLabel.GetComponent<RectTransform>());
            cancelLabel.alignment = TextAlignmentOptions.Center;
            AddThemedText(cancelLabel.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);

            var (confirmGO, confirmButton, _) = CreatePrimaryButton(buttonRow.transform, "ConfirmButton");
            var confirmLabel = CreateText(confirmGO.transform, "Label", "OK", 28, FontStyles.Bold);
            confirmLabel.color = TextLightColor;
            StretchInside(confirmLabel.GetComponent<RectTransform>());
            confirmLabel.alignment = TextAlignmentOptions.Center;
            AddThemedText(confirmLabel.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.None, ThemeFontSizeSlot.None);

            var animator = root.AddComponent<UIAnimConfirmPopup>();
            var popup = root.AddComponent<ConfirmPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.MessageLabel").objectReferenceValue = message;
            so.FindProperty("_refs.ConfirmLabel").objectReferenceValue = confirmLabel;
            so.FindProperty("_refs.CancelLabel").objectReferenceValue = cancelLabel;
            so.FindProperty("_refs.ConfirmButton").objectReferenceValue = confirmButton;
            so.FindProperty("_refs.CancelButton").objectReferenceValue = cancelButton;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.ConfirmTint").objectReferenceValue = toneTint;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<ConfirmPopup>(root, ConfirmPath);
        }

        private static PausePopup BuildPausePopup()
        {
            var root = CreateRoot("PausePopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 900f));

            var title = CreateText(card.transform, "Title", "Paused", 44, FontStyles.Bold);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 64f);
            title.alignment = TextAlignmentOptions.Center;
            AddThemedText(title.gameObject, ThemeFontSlot.FontHeading, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);

            var subtitle = CreateText(card.transform, "Subtitle", "", 22, FontStyles.Normal);
            AnchorTopOfCard(subtitle.GetComponent<RectTransform>(), -100f, 36f);
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.color = new Color(0.4f, 0.45f, 0.55f, 1f);
            AddThemedText(subtitle.gameObject, ThemeFontSlot.FontCaption, ThemeColorSlot.TextSecondary, ThemeFontSizeSlot.None);

            var buttonsColumn = CreateChild(card.transform, "Buttons");
            var colRT = buttonsColumn.GetComponent<RectTransform>();
            colRT.anchorMin = new Vector2(0f, 0f);
            colRT.anchorMax = new Vector2(1f, 1f);
            colRT.offsetMin = new Vector2(40f, 200f);
            colRT.offsetMax = new Vector2(-40f, -160f);
            var colLayout = buttonsColumn.AddComponent<VerticalLayoutGroup>();
            colLayout.spacing = 12f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childControlWidth = true;
            colLayout.childControlHeight = true;

            var (resumeGO, resumeButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "ResumeButton", "Resume", ButtonPrimaryColor, TextLightColor, ThemeSpriteSlot.ButtonPrimary, ThemeColorSlot.PrimaryColor, ThemeColorSlot.None);
            var (restartGO, restartButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "RestartButton", "Restart", ButtonSecondaryColor, TextDarkColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.SecondaryColor, ThemeColorSlot.TextPrimary);
            var (settingsGO, settingsButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "SettingsButton", "Settings", ButtonSecondaryColor, TextDarkColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.SecondaryColor, ThemeColorSlot.TextPrimary);
            var (homeGO, homeButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "HomeButton", "Home", ButtonSecondaryColor, TextDarkColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.SecondaryColor, ThemeColorSlot.TextPrimary);
            var (shopGO, shopButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "ShopButton", "Shop", ButtonSecondaryColor, TextDarkColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.SecondaryColor, ThemeColorSlot.TextPrimary);
            var (helpGO, helpButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "HelpButton", "Help", ButtonSecondaryColor, TextDarkColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.SecondaryColor, ThemeColorSlot.TextPrimary);
            var (quitGO, quitButton, _) = CreateThemedLabelledButton(buttonsColumn.transform, "QuitButton", "Quit", new Color(0.92f, 0.30f, 0.30f, 1f), TextLightColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.DangerColor, ThemeColorSlot.None);

            ForceButtonHeight(resumeGO, 80f);
            ForceButtonHeight(restartGO, 70f);
            ForceButtonHeight(settingsGO, 70f);
            ForceButtonHeight(homeGO, 70f);
            ForceButtonHeight(shopGO, 70f);
            ForceButtonHeight(helpGO, 70f);
            ForceButtonHeight(quitGO, 70f);

            var togglesRow = CreateChild(card.transform, "Toggles");
            var togglesRT = togglesRow.GetComponent<RectTransform>();
            togglesRT.anchorMin = new Vector2(0f, 0f);
            togglesRT.anchorMax = new Vector2(1f, 0f);
            togglesRT.pivot = new Vector2(0.5f, 0f);
            togglesRT.anchoredPosition = new Vector2(0f, 30f);
            togglesRT.sizeDelta = new Vector2(-60f, 60f);
            var togLayout = togglesRow.AddComponent<HorizontalLayoutGroup>();
            togLayout.spacing = 16f;
            togLayout.childForceExpandWidth = true;
            togLayout.childForceExpandHeight = true;
            togLayout.childControlWidth = true;
            togLayout.childControlHeight = true;

            var soundToggle = CreateLabelledToggle(togglesRow.transform, "SoundToggle", "Sound");
            var musicToggle = CreateLabelledToggle(togglesRow.transform, "MusicToggle", "Music");
            var vibrationToggle = CreateLabelledToggle(togglesRow.transform, "VibrationToggle", "Vibration");

            var animator = root.AddComponent<UIAnimPausePopup>();
            var popup = root.AddComponent<PausePopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.SubtitleLabel").objectReferenceValue = subtitle;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.ResumeButton").objectReferenceValue = resumeButton;
            so.FindProperty("_refs.RestartButton").objectReferenceValue = restartButton;
            so.FindProperty("_refs.HomeButton").objectReferenceValue = homeButton;
            so.FindProperty("_refs.QuitButton").objectReferenceValue = quitButton;
            so.FindProperty("_refs.SettingsButton").objectReferenceValue = settingsButton;
            so.FindProperty("_refs.ShopButton").objectReferenceValue = shopButton;
            so.FindProperty("_refs.HelpButton").objectReferenceValue = helpButton;
            so.FindProperty("_refs.SoundToggle").objectReferenceValue = soundToggle;
            so.FindProperty("_refs.MusicToggle").objectReferenceValue = musicToggle;
            so.FindProperty("_refs.VibrationToggle").objectReferenceValue = vibrationToggle;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<PausePopup>(root, PausePath);
        }

        private static TutorialPopup BuildTutorialPopup()
        {
            var root = CreateRoot("TutorialPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 720f));

            var title = CreateText(card.transform, "Title", "Title", 36, FontStyles.Bold);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 56f);
            title.alignment = TextAlignmentOptions.Center;
            AddThemedText(title.gameObject, ThemeFontSlot.FontHeading, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);

            var stepImageGO = CreateChild(card.transform, "StepImage");
            var stepImageRT = stepImageGO.GetComponent<RectTransform>();
            stepImageRT.anchorMin = new Vector2(0.5f, 1f);
            stepImageRT.anchorMax = new Vector2(0.5f, 1f);
            stepImageRT.pivot = new Vector2(0.5f, 1f);
            stepImageRT.anchoredPosition = new Vector2(0f, -110f);
            stepImageRT.sizeDelta = new Vector2(280f, 280f);
            var stepImage = AddThemedImage(stepImageGO, new Color(0.85f, 0.86f, 0.90f, 1f), ThemeSpriteSlot.None, ThemeColorSlot.BackgroundLight);

            var body = CreateText(card.transform, "Body", "Step body goes here.", 24, FontStyles.Normal);
            var bodyRT = body.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = new Vector2(40f, 180f);
            bodyRT.offsetMax = new Vector2(-40f, -420f);
            body.alignment = TextAlignmentOptions.Top;
            AddThemedText(body.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.TextSecondary, ThemeFontSizeSlot.None);

            var progress = CreateText(card.transform, "Progress", "1 / 3", 20, FontStyles.Italic);
            AnchorTopOfCard(progress.GetComponent<RectTransform>(), -86f, 32f);
            progress.alignment = TextAlignmentOptions.Center;
            progress.color = new Color(0.4f, 0.45f, 0.55f, 1f);
            AddThemedText(progress.gameObject, ThemeFontSlot.FontCaption, ThemeColorSlot.TextSecondary, ThemeFontSizeSlot.None);

            var buttonRow = CreateChild(card.transform, "ButtonRow");
            var rowRT = buttonRow.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 0f);
            rowRT.anchorMax = new Vector2(1f, 0f);
            rowRT.pivot = new Vector2(0.5f, 0f);
            rowRT.anchoredPosition = new Vector2(0f, 30f);
            rowRT.sizeDelta = new Vector2(-60f, 80f);
            var rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;

            var (prevGO, prevButton, _) = CreateSecondaryButton(buttonRow.transform, "PreviousButton");
            var prevLabel = CreateText(prevGO.transform, "Label", "Back", 24, FontStyles.Bold);
            prevLabel.color = TextDarkColor;
            StretchInside(prevLabel.GetComponent<RectTransform>());
            prevLabel.alignment = TextAlignmentOptions.Center;
            AddThemedText(prevLabel.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);

            var (skipGO, skipButton, _) = CreateSecondaryButton(buttonRow.transform, "SkipButton");
            var skipLabel = CreateText(skipGO.transform, "Label", "Skip", 24, FontStyles.Bold);
            skipLabel.color = TextDarkColor;
            StretchInside(skipLabel.GetComponent<RectTransform>());
            skipLabel.alignment = TextAlignmentOptions.Center;
            AddThemedText(skipLabel.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);

            var (nextGO, nextButton, _) = CreatePrimaryButton(buttonRow.transform, "NextButton");
            var nextLabel = CreateText(nextGO.transform, "Label", "Next", 24, FontStyles.Bold);
            nextLabel.color = TextLightColor;
            StretchInside(nextLabel.GetComponent<RectTransform>());
            nextLabel.alignment = TextAlignmentOptions.Center;
            AddThemedText(nextLabel.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.None, ThemeFontSizeSlot.None);

            var animator = root.AddComponent<UIAnimTutorialPopup>();
            var popup = root.AddComponent<TutorialPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.BodyLabel").objectReferenceValue = body;
            so.FindProperty("_refs.StepImage").objectReferenceValue = stepImage;
            so.FindProperty("_refs.ProgressLabel").objectReferenceValue = progress;
            so.FindProperty("_refs.NextButton").objectReferenceValue = nextButton;
            so.FindProperty("_refs.PreviousButton").objectReferenceValue = prevButton;
            so.FindProperty("_refs.SkipButton").objectReferenceValue = skipButton;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.NextLabel").objectReferenceValue = nextLabel;
            so.FindProperty("_refs.PreviousLabel").objectReferenceValue = prevLabel;
            so.FindProperty("_refs.SkipLabel").objectReferenceValue = skipLabel;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<TutorialPopup>(root, TutorialPath);
        }

        private static NotificationToast BuildNotificationToast()
        {
            var root = new GameObject("NotificationToast");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.5f, 1f);
            rootRT.anchorMax = new Vector2(0.5f, 1f);
            rootRT.pivot = new Vector2(0.5f, 1f);
            rootRT.anchoredPosition = new Vector2(0f, -40f);
            rootRT.sizeDelta = new Vector2(640f, 96f);
            root.AddComponent<CanvasGroup>();

            var tint = AddImage(root, ToastBackgroundColor);

            var iconGO = CreateChild(root.transform, "Icon");
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.anchoredPosition = new Vector2(20f, 0f);
            iconRT.sizeDelta = new Vector2(48f, 48f);
            var icon = AddImage(iconGO, Color.white);

            var message = CreateText(root.transform, "Message", "Toast message", 24, FontStyles.Normal);
            var messageRT = message.GetComponent<RectTransform>();
            messageRT.anchorMin = new Vector2(0f, 0f);
            messageRT.anchorMax = new Vector2(1f, 1f);
            messageRT.offsetMin = new Vector2(88f, 0f);
            messageRT.offsetMax = new Vector2(-20f, 0f);
            message.alignment = TextAlignmentOptions.Left;
            message.color = TextLightColor;
            AddThemedText(message.gameObject, ThemeFontSlot.FontBody, ThemeColorSlot.None, ThemeFontSizeSlot.None);

            var tapAreaGO = CreateChild(root.transform, "TapArea");
            var tapRT = tapAreaGO.GetComponent<RectTransform>();
            StretchInside(tapRT);
            var tapImg = AddImage(tapAreaGO, new Color(0f, 0f, 0f, 0f));
            tapImg.raycastTarget = true;
            var tapButton = tapAreaGO.AddComponent<Button>();
            tapButton.targetGraphic = tapImg;

            var animator = root.AddComponent<UIAnimNotificationToast>();
            var toast = root.AddComponent<NotificationToast>();

            var aso = new SerializedObject(animator);
            aso.FindProperty("_root").objectReferenceValue = rootRT;
            aso.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(toast);
            so.FindProperty("_refs.MessageLabel").objectReferenceValue = message;
            so.FindProperty("_refs.SeverityTint").objectReferenceValue = tint;
            so.FindProperty("_refs.SeverityIcon").objectReferenceValue = icon;
            so.FindProperty("_refs.TapArea").objectReferenceValue = tapButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<NotificationToast>(root, ToastPath);
        }


        private static (GameObject go, Button btn, Image img) CreateLabelledButton(Transform parent, string name, string label, Color buttonColor, Color textColor)
        {
            var (go, btn, img) = CreateButton(parent, name, buttonColor, ThemeSpriteSlot.None, ThemeColorSlot.None);
            var tmp = CreateText(go.transform, "Label", label, 26, FontStyles.Bold);
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            StretchInside(tmp.GetComponent<RectTransform>());
            return (go, btn, img);
        }

        private static (GameObject go, Button btn, Image img) CreateThemedLabelledButton(Transform parent, string name, string label, Color buttonColor, Color textColor, ThemeSpriteSlot spriteSlot, ThemeColorSlot bgColorSlot, ThemeColorSlot textColorSlot)
        {
            var (go, btn, img) = CreateLabelledButton(parent, name, label, buttonColor, textColor);
            AddThemedImage(go, buttonColor, spriteSlot, bgColorSlot);
            var label_tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label_tmp != null) AddThemedText(label_tmp.gameObject, ThemeFontSlot.FontBody, textColorSlot, ThemeFontSizeSlot.None);
            return (go, btn, img);
        }

        private static Toggle CreateLabelledToggle(Transform parent, string name, string label)
        {
            var go = CreateChild(parent, name);
            var bg = AddImage(go, ButtonSecondaryColor);
            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.isOn = true;

            var checkmarkGO = CreateChild(go.transform, "Checkmark");
            var checkmark = AddImage(checkmarkGO, ButtonPrimaryColor);
            var checkRT = checkmarkGO.GetComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0f, 0f);
            checkRT.anchorMax = new Vector2(0f, 1f);
            checkRT.pivot = new Vector2(0f, 0.5f);
            checkRT.anchoredPosition = new Vector2(8f, 0f);
            checkRT.sizeDelta = new Vector2(36f, -16f);
            toggle.graphic = checkmark;

            var tmp = CreateText(go.transform, "Label", label, 22, FontStyles.Bold);
            tmp.color = TextDarkColor;
            tmp.alignment = TextAlignmentOptions.Center;
            var tmpRT = tmp.GetComponent<RectTransform>();
            tmpRT.anchorMin = new Vector2(0f, 0f);
            tmpRT.anchorMax = new Vector2(1f, 1f);
            tmpRT.offsetMin = new Vector2(56f, 0f);
            tmpRT.offsetMax = new Vector2(-8f, 0f);

            return toggle;
        }
    }
}
