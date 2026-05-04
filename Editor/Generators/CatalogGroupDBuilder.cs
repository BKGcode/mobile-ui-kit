using KitforgeLabs.MobileUIKit.Catalog.Settings;
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
    public static class CatalogGroupDBuilder
    {
        private const string OutputRoot = "Assets/Catalog_GroupD_Demo";
        private const string PrefabsFolder = OutputRoot + "/Prefabs";
        private const string SettingsPath = PrefabsFolder + "/SettingsPopup.prefab";
        private const string ScenePath = OutputRoot + "/Catalog_GroupD_Demo.unity";
        private const string DefaultThemePath = "Assets/Settings/UI/UIThemeConfig_Default.asset";
        private const string GroupDStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupD";
        private const string LocalizationServiceTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupD.InMemoryLocalizationService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupD";
        private const string DemoMonoBehaviourTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupD.CatalogGroupDDemo, KitforgeLabs.MobileUIKit.Samples.CatalogGroupD";

        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color CardColor = new Color(0.97f, 0.98f, 1f, 1f);
        private static readonly Color RowDividerColor = new Color(0.90f, 0.91f, 0.94f, 1f);
        private static readonly Color ButtonPrimaryColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color SliderTrackColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        private static readonly Color SliderFillColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color SliderHandleColor = Color.white;
        private static readonly Color ToggleBgColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        private static readonly Color ToggleCheckColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color DropdownBgColor = new Color(0.93f, 0.94f, 0.96f, 1f);
        private static readonly Color TextDarkColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        private static readonly Color TextLightColor = Color.white;

        [MenuItem("Tools/Kitforge/UI Kit/Build Group D Sample")]
        public static void BuildAll()
        {
            EnsureFolders();
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath) == null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Bootstrap Defaults missing",
                    $"No Theme found at {DefaultThemePath}.\n\nRun 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first, or proceed without a Theme reference.",
                    "Proceed", "Cancel")) return;
            }
            BuildSettingsPopup();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildDemoScene();
            EditorUtility.DisplayDialog(
                "Kitforge UI Kit",
                $"Group D built at {OutputRoot}.\n\n1 prefab + 1 scene generated. Open the scene, press Play, right-click the Demo GameObject and pick a Context Menu scenario. Try 'Settings — Show all', then 'PlayerData — Print all kfmui.settings.* values' to see the persisted state.",
                "OK");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) AssetDatabase.CreateFolder("Assets", "Catalog_GroupD_Demo");
            if (!AssetDatabase.IsValidFolder(PrefabsFolder)) AssetDatabase.CreateFolder(OutputRoot, "Prefabs");
        }

        private static SettingsPopup BuildSettingsPopup()
        {
            var root = CreateRoot("SettingsPopup");
            var backdrop = CreateBackdrop(root.transform);
            var card = CreateCard(root.transform, new Vector2(720f, 1100f));

            var title = CreateText(card.transform, "Title", "Settings", 44, FontStyles.Bold);
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
            closeImg.color = ButtonPrimaryColor;
            var closeLabel = CreateText(closeGO.transform, "Label", "Close", 30, FontStyles.Bold);
            closeLabel.color = TextLightColor;
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
            AddImage(bg, SliderTrackColor);

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
            AddImage(fill, SliderFillColor);

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
            var bgImg = AddImage(bg, ToggleBgColor);

            var checkGO = CreateChild(bg.transform, "Checkmark");
            var checkRT = checkGO.GetComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0.5f, 0.5f);
            checkRT.anchorMax = new Vector2(0.5f, 0.5f);
            checkRT.pivot = new Vector2(0.5f, 0.5f);
            checkRT.sizeDelta = new Vector2(28f, 28f);
            var checkImg = AddImage(checkGO, ToggleCheckColor);

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
            var bgImg = AddImage(dropdownGO, DropdownBgColor);

            var label = CreateText(dropdownGO.transform, "Label", "English", 24, FontStyles.Normal);
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
            AddImage(template, DropdownBgColor);
            template.AddComponent<ScrollRect>();

            var viewport = CreateChild(template.transform, "Viewport");
            var viewportRT = viewport.GetComponent<RectTransform>();
            StretchInside(viewportRT);
            AddImage(viewport, Color.white).color = new Color(1f, 1f, 1f, 0.01f);
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
            AddImage(itemBg, DropdownBgColor);
            itemLabel = CreateText(item.transform, "Item Label", "Option", 24, FontStyles.Normal);
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
            var label = CreateText(row.transform, "Label", labelText, 28, FontStyles.Normal);
            var labelLE = label.gameObject.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 220f;
            label.alignment = TextAlignmentOptions.Left;
            return row;
        }

        private static void BuildDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPath);
            if (settingsPrefab == null)
            {
                Debug.LogError($"[CatalogGroupDBuilder] Failed to load Settings prefab at {SettingsPath}.");
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

            var eventSystemGO = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystemGO, scene);
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var servicesGO = new GameObject("UIServices");
            SceneManager.MoveGameObjectToScene(servicesGO, scene);
            var services = servicesGO.AddComponent<UIServices>();
            var playerData = servicesGO.AddComponent<PlayerPrefsPlayerDataService>();
            TryAttachStubServices(servicesGO, services, playerData);

            var demoGO = new GameObject("Demo");
            SceneManager.MoveGameObjectToScene(demoGO, scene);
            var demoType = System.Type.GetType(DemoMonoBehaviourTypeName);
            if (demoType == null)
            {
                Debug.LogWarning("[CatalogGroupDBuilder] CatalogGroupDDemo type not found. Make sure the Group D sample is imported (Package Manager → Samples) so its asmdef compiles.");
            }
            else
            {
                var demo = demoGO.AddComponent(demoType);
                var so = new SerializedObject(demo);
                so.FindProperty("_popupParent").objectReferenceValue = popupParent.GetComponent<RectTransform>();
                so.FindProperty("_theme").objectReferenceValue = theme;
                so.FindProperty("_services").objectReferenceValue = services;
                so.FindProperty("_settingsPrefab").objectReferenceValue = settingsPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void TryAttachStubServices(GameObject host, UIServices services, PlayerPrefsPlayerDataService playerData)
        {
            var localizationType = System.Type.GetType(LocalizationServiceTypeName);
            if (localizationType == null)
            {
                Debug.LogWarning("[CatalogGroupDBuilder] Group D Localization stub not found. Import 'Catalog — Group D — Player Data' sample (Package Manager → Samples) so its asmdef compiles.");
            }
            var localization = localizationType != null ? (MonoBehaviour)host.AddComponent(localizationType) : null;

            var so = new SerializedObject(services);
            so.FindProperty("_playerDataServiceRef").objectReferenceValue = playerData;
            if (localization != null) so.FindProperty("_localizationServiceRef").objectReferenceValue = localization;
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static (GameObject go, Button btn, Image img) CreatePrimaryButton(Transform parent, string name)
        {
            var go = CreateChild(parent, name);
            var img = AddImage(go, ButtonPrimaryColor);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return (go, btn, img);
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
