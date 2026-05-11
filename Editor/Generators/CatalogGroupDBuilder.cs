using KitforgeLabs.UIKit.Catalog.Settings;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KitforgeLabs.UIKit.Editor.Generators.CatalogGroupBuilderShared;

namespace KitforgeLabs.UIKit.Editor.Generators
{
    internal static class CatalogGroupDBuilder
    {
        private const string DefaultThemePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset";

        private static string SettingsPath => $"{ResolvePrefabsFolder()}/SettingsPopup.prefab";

        private static readonly Color SliderTrackColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        private static readonly Color SliderFillColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color SliderHandleColor = Color.white;
        private static readonly Color ToggleBgColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        private static readonly Color ToggleCheckColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color DropdownBgColor = new Color(0.93f, 0.94f, 0.96f, 1f);

        public static void BuildAll()
        {
            EnsurePrefabsFolder(ResolvePrefabsFolder());
            BuildSettingsPopup();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static SettingsPopup BuildSettingsPopup()
        {
            var root = CreateRoot("SettingsPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 1100f));

            var title = CreateThemedText(card.transform, "Title", "Settings", 44, FontStyles.Bold, ThemeBuilderSlots.Heading);
            AnchorTopOfCard(title.GetComponent<RectTransform>(), -30f, 72f);
            title.alignment = TextAlignmentOptions.Center;

            var rows = CreateChild(card.transform, "Rows");
            var rowsRT = rows.GetComponent<RectTransform>();
            rowsRT.anchorMin = new Vector2(0f, 1f);
            rowsRT.anchorMax = new Vector2(1f, 1f);
            rowsRT.pivot = new Vector2(0.5f, 1f);
            rowsRT.anchoredPosition = new Vector2(0f, -130f);
            rowsRT.sizeDelta = new Vector2(-60f, 800f);
            var rowsLayout = rows.AddComponent<VerticalLayoutGroup>();
            rowsLayout.spacing = 14f;
            rowsLayout.childAlignment = TextAnchor.UpperLeft;
            rowsLayout.childForceExpandHeight = false;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = false;

            var (musicRow, musicSlider) = CreateSliderRow(rows.transform, "MusicRow", "Music");
            var (sfxRow, sfxSlider) = CreateSliderRow(rows.transform, "SfxRow", "SFX");
            var (languageRow, languageDropdown) = CreateDropdownRow(rows.transform, "LanguageRow", "Language");
            var (notificationsRow, notificationsToggle) = CreateToggleRow(rows.transform, "NotificationsRow", "Notifications");
            var (hapticsRow, hapticsToggle) = CreateToggleRow(rows.transform, "HapticsRow", "Haptics");

            var (closeGO, closeButton, closeImg) = CreatePrimaryButton(card.transform, "CloseButton");
            var closeRT = closeGO.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.5f, 0f);
            closeRT.anchorMax = new Vector2(0.5f, 0f);
            closeRT.pivot = new Vector2(0.5f, 0f);
            closeRT.anchoredPosition = new Vector2(0f, 50f);
            closeRT.sizeDelta = new Vector2(420f, 88f);
            var closeLabel = CreateThemedText(closeGO.transform, "Label", "Close", 30, FontStyles.Bold, ThemeBuilderSlots.PrimaryButtonLabel);
            closeLabel.alignment = TextAlignmentOptions.Center;
            StretchInside(closeLabel.GetComponent<RectTransform>());

            var animator = root.AddComponent<UIAnimSettingsPopup>();
            var popup = root.AddComponent<SettingsPopup>();
            WireAnimatorCard(animator, card.GetComponent<RectTransform>());

            var so = new SerializedObject(popup);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.BackdropButton").objectReferenceValue = backdrop;
            so.FindProperty("_refs.CloseButton").objectReferenceValue = closeButton;
            so.FindProperty("_refs.CloseLabelText").objectReferenceValue = closeLabel;
            so.FindProperty("_refs.MusicRow").objectReferenceValue = musicRow;
            so.FindProperty("_refs.SfxRow").objectReferenceValue = sfxRow;
            so.FindProperty("_refs.LanguageRow").objectReferenceValue = languageRow;
            so.FindProperty("_refs.NotificationsRow").objectReferenceValue = notificationsRow;
            so.FindProperty("_refs.HapticsRow").objectReferenceValue = hapticsRow;
            so.FindProperty("_refs.MusicSlider").objectReferenceValue = musicSlider;
            so.FindProperty("_refs.SfxSlider").objectReferenceValue = sfxSlider;
            so.FindProperty("_refs.LanguageDropdown").objectReferenceValue = languageDropdown;
            so.FindProperty("_refs.NotificationsToggle").objectReferenceValue = notificationsToggle;
            so.FindProperty("_refs.HapticsToggle").objectReferenceValue = hapticsToggle;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SaveAsPrefab<SettingsPopup>(root, SettingsPath);
        }

        private static (GameObject row, Slider slider) CreateSliderRow(Transform parent, string rowName, string labelText)
        {
            var row = CreateRow(parent, rowName, labelText);
            var sliderGO = CreateChild(row.transform, "Slider");
            var sliderLE = sliderGO.AddComponent<LayoutElement>();
            sliderLE.preferredWidth = 360f;
            sliderLE.minHeight = 44f;
            BuildSliderHierarchy(sliderGO, out var slider);
            return (row, slider);
        }

        private static void BuildSliderHierarchy(GameObject sliderGO, out Slider slider)
        {
            var bg = CreateChild(sliderGO.transform, "Background");
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.4f);
            bgRT.anchorMax = new Vector2(1f, 0.6f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            AddThemedImage(bg, SliderTrackColor, ThemeSpriteSlot.None, ThemeColorSlot.MutedColor);

            var fillArea = CreateChild(sliderGO.transform, "Fill Area");
            var fillAreaRT = fillArea.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.4f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.6f);
            fillAreaRT.offsetMin = new Vector2(8f, 0f);
            fillAreaRT.offsetMax = new Vector2(-8f, 0f);

            var fill = CreateChild(fillArea.transform, "Fill");
            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            AddThemedImage(fill, SliderFillColor, ThemeSpriteSlot.None, ThemeColorSlot.PrimaryColor);

            var handleArea = CreateChild(sliderGO.transform, "Handle Slide Area");
            var handleAreaRT = handleArea.GetComponent<RectTransform>();
            handleAreaRT.anchorMin = new Vector2(0f, 0f);
            handleAreaRT.anchorMax = new Vector2(1f, 1f);
            handleAreaRT.offsetMin = new Vector2(8f, 0f);
            handleAreaRT.offsetMax = new Vector2(-8f, 0f);

            var handle = CreateChild(handleArea.transform, "Handle");
            var handleRT = handle.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(28f, 0f);
            var handleImg = AddImage(handle, SliderHandleColor);

            slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }

        private static (GameObject row, Toggle toggle) CreateToggleRow(Transform parent, string rowName, string labelText)
        {
            var row = CreateRow(parent, rowName, labelText);
            var toggleGO = CreateChild(row.transform, "Toggle");
            var toggleLE = toggleGO.AddComponent<LayoutElement>();
            toggleLE.preferredWidth = 80f;
            toggleLE.preferredHeight = 44f;
            var bg = CreateChild(toggleGO.transform, "Background");
            var bgRT = bg.GetComponent<RectTransform>();
            StretchInside(bgRT);
            var bgImg = AddThemedImage(bg, ToggleBgColor, ThemeSpriteSlot.None, ThemeColorSlot.MutedColor);

            var checkGO = CreateChild(bg.transform, "Checkmark");
            var checkRT = checkGO.GetComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0.5f, 0.5f);
            checkRT.anchorMax = new Vector2(0.5f, 0.5f);
            checkRT.pivot = new Vector2(0.5f, 0.5f);
            checkRT.sizeDelta = new Vector2(28f, 28f);
            var checkImg = AddThemedImage(checkGO, ToggleCheckColor, ThemeSpriteSlot.None, ThemeColorSlot.PrimaryColor);

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = true;
            return (row, toggle);
        }

        private static (GameObject row, TMP_Dropdown dropdown) CreateDropdownRow(Transform parent, string rowName, string labelText)
        {
            var row = CreateRow(parent, rowName, labelText);
            var dropdownGO = CreateChild(row.transform, "Dropdown");
            var dropdownLE = dropdownGO.AddComponent<LayoutElement>();
            dropdownLE.preferredWidth = 280f;
            dropdownLE.preferredHeight = 56f;
            var bgImg = AddThemedImage(dropdownGO, DropdownBgColor, ThemeSpriteSlot.None, ThemeColorSlot.MutedColor);

            var label = CreateThemedText(dropdownGO.transform, "Label", "English", 24, FontStyles.Normal, ThemeBuilderSlots.BodyPrimary);
            label.alignment = TextAlignmentOptions.Left;
            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0f);
            labelRT.anchorMax = new Vector2(1f, 1f);
            labelRT.offsetMin = new Vector2(16f, 0f);
            labelRT.offsetMax = new Vector2(-32f, 0f);

            BuildDropdownTemplate(dropdownGO, out var template, out var itemLabel);

            var dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = bgImg;
            dropdown.template = template;
            dropdown.captionText = label;
            dropdown.itemText = itemLabel;
            return (row, dropdown);
        }

        private static void BuildDropdownTemplate(GameObject dropdownGO, out RectTransform templateRT, out TextMeshProUGUI itemLabel)
        {
            var template = CreateChild(dropdownGO.transform, "Template");
            templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0f, 0f);
            templateRT.anchorMax = new Vector2(1f, 0f);
            templateRT.pivot = new Vector2(0.5f, 1f);
            templateRT.anchoredPosition = new Vector2(0f, 0f);
            templateRT.sizeDelta = new Vector2(0f, 200f);
            AddThemedImage(template, DropdownBgColor, ThemeSpriteSlot.None, ThemeColorSlot.MutedColor);
            template.AddComponent<ScrollRect>();

            var viewport = CreateChild(template.transform, "Viewport");
            var viewportRT = viewport.GetComponent<RectTransform>();
            StretchInside(viewportRT);
            AddImage(viewport, new Color(1f, 1f, 1f, 0.01f));
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateChild(viewport.transform, "Content");
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0f, 56f);

            var item = CreateChild(content.transform, "Item");
            var itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0f, 0.5f);
            itemRT.anchorMax = new Vector2(1f, 0.5f);
            itemRT.sizeDelta = new Vector2(0f, 56f);
            item.AddComponent<Toggle>();
            var itemBg = CreateChild(item.transform, "Item Background");
            StretchInside(itemBg.GetComponent<RectTransform>());
            AddThemedImage(itemBg, DropdownBgColor, ThemeSpriteSlot.None, ThemeColorSlot.MutedColor);
            itemLabel = CreateThemedText(item.transform, "Item Label", "Option", 24, FontStyles.Normal, ThemeBuilderSlots.BodyPrimary);
            itemLabel.alignment = TextAlignmentOptions.Left;
            var itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = new Vector2(0f, 0f);
            itemLabelRT.anchorMax = new Vector2(1f, 1f);
            itemLabelRT.offsetMin = new Vector2(16f, 0f);
            itemLabelRT.offsetMax = new Vector2(-16f, 0f);

            template.SetActive(false);
        }

        private static GameObject CreateRow(Transform parent, string rowName, string labelText)
        {
            var row = CreateChild(parent, rowName);
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.minHeight = 56f;
            rowLE.preferredHeight = 56f;
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = false;
            var label = CreateThemedText(row.transform, "Label", labelText, 28, FontStyles.Normal, ThemeBuilderSlots.BodyPrimary);
            var labelLE = label.gameObject.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 220f;
            label.alignment = TextAlignmentOptions.Left;
            return row;
        }



    }
}
