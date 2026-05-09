using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    internal static class CatalogGroupBuilderShared
    {
        internal static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);
        internal static readonly Color CardColor = new Color(0.97f, 0.98f, 1f, 1f);
        internal static readonly Color ButtonPrimaryColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        internal static readonly Color ButtonSecondaryColor = new Color(0.85f, 0.86f, 0.90f, 1f);
        internal static readonly Color TextDarkColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        internal static readonly Color TextLightColor = Color.white;
        internal static readonly Color WarningColor = new Color(1.00f, 0.60f, 0.15f, 1f);

        internal static void EnsureFolders(string demoFolder)
        {
            var outputRoot = "Assets/" + demoFolder;
            var prefabsFolder = outputRoot + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(outputRoot)) AssetDatabase.CreateFolder("Assets", demoFolder);
            if (!AssetDatabase.IsValidFolder(prefabsFolder)) AssetDatabase.CreateFolder(outputRoot, "Prefabs");
        }

        internal static GameObject CreateRoot(string name)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            StretchInside(rt);
            go.AddComponent<CanvasGroup>();
            return go;
        }

        internal static Button CreateBackdrop(Transform parent)
        {
            var go = CreateChild(parent, "Backdrop");
            var img = AddImage(go, BackdropColor);
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }

        internal static GameObject CreateCard(Transform parent, Vector2 size)
        {
            var go = CreateChild(parent, "Card");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            AddThemedImage(go, CardColor, ThemeSpriteSlot.PanelBackground, ThemeColorSlot.BackgroundLight);
            return go;
        }

        internal static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        internal static Image AddImage(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        internal static Image AddThemedImage(GameObject go, Color fallbackColor, ThemeSpriteSlot spriteSlot, ThemeColorSlot colorSlot)
        {
            var img = go.GetComponent<Image>();
            if (img == null) img = AddImage(go, fallbackColor);
            else img.color = fallbackColor;
            var themed = go.AddComponent<ThemedImage>();
            var so = new SerializedObject(themed);
            so.FindProperty("_image").objectReferenceValue = img;
            so.FindProperty("_spriteSlot").enumValueIndex = (int)spriteSlot;
            so.FindProperty("_colorSlot").enumValueIndex = (int)colorSlot;
            so.ApplyModifiedPropertiesWithoutUndo();
            return img;
        }

        internal static void AddThemedText(GameObject go, ThemeFontSlot fontSlot, ThemeColorSlot colorSlot, ThemeFontSizeSlot sizeSlot)
        {
            var themed = go.AddComponent<ThemedText>();
            var so = new SerializedObject(themed);
            so.FindProperty("_text").objectReferenceValue = go.GetComponent<TMP_Text>();
            so.FindProperty("_fontSlot").enumValueIndex = (int)fontSlot;
            so.FindProperty("_colorSlot").enumValueIndex = (int)colorSlot;
            so.FindProperty("_sizeSlot").enumValueIndex = (int)sizeSlot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, FontStyles style)
        {
            var go = CreateChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = TextDarkColor;
            return tmp;
        }

        internal static TextMeshProUGUI CreateThemedText(Transform parent, string name, string text, int size, FontStyles style, TextThemeSlot slot)
        {
            var tmp = CreateText(parent, name, text, size, style);
            if (slot.Color == ThemeColorSlot.TextOnPrimary || slot.Color == ThemeColorSlot.TextOnAccent) tmp.color = TextLightColor;
            else if (slot.Color == ThemeColorSlot.WarningColor) tmp.color = WarningColor;
            AddThemedText(tmp.gameObject, slot.Font, slot.Color, slot.Size);
            return tmp;
        }

        internal static (GameObject go, Button btn, Image img) CreateButton(Transform parent, string name, Color color, ThemeSpriteSlot spriteSlot, ThemeColorSlot colorSlot)
        {
            var go = CreateChild(parent, name);
            var themed = spriteSlot != ThemeSpriteSlot.None || colorSlot != ThemeColorSlot.None;
            var img = themed ? AddThemedImage(go, color, spriteSlot, colorSlot) : AddImage(go, color);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return (go, btn, img);
        }

        internal static (GameObject go, Button btn, Image img) CreatePrimaryButton(Transform parent, string name)
        {
            return CreateButton(parent, name, ButtonPrimaryColor, ThemeSpriteSlot.ButtonPrimary, ThemeColorSlot.PrimaryColor);
        }

        internal static (GameObject go, Button btn, Image img) CreateSecondaryButton(Transform parent, string name)
        {
            return CreateButton(parent, name, ButtonSecondaryColor, ThemeSpriteSlot.ButtonSecondary, ThemeColorSlot.SecondaryColor);
        }

        internal static void StretchInside(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        internal static void AnchorTopOfCard(RectTransform rt, float y, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, height);
        }

        internal static void ForceButtonHeight(GameObject buttonGO, float height)
        {
            var le = buttonGO.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        internal static void WireAnimatorCard(MonoBehaviour animator, RectTransform card)
        {
            var so = new SerializedObject(animator);
            var prop = so.FindProperty("_card");
            if (prop != null)
            {
                prop.objectReferenceValue = card;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        internal static T SaveAsPrefab<T>(GameObject root, string path) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<T>();
        }
    }

    internal readonly struct TextThemeSlot
    {
        public readonly ThemeFontSlot Font;
        public readonly ThemeColorSlot Color;
        public readonly ThemeFontSizeSlot Size;

        public TextThemeSlot(ThemeFontSlot font, ThemeColorSlot color, ThemeFontSizeSlot size)
        {
            Font = font;
            Color = color;
            Size = size;
        }
    }

    internal static class ThemeBuilderSlots
    {
        public static readonly TextThemeSlot Heading = new(ThemeFontSlot.FontHeading, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot Body = new(ThemeFontSlot.FontBody, ThemeColorSlot.TextSecondary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot BodyPrimary = new(ThemeFontSlot.FontBody, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot Caption = new(ThemeFontSlot.FontCaption, ThemeColorSlot.TextSecondary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot PrimaryButtonLabel = new(ThemeFontSlot.FontBody, ThemeColorSlot.TextOnPrimary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot SecondaryButtonLabel = new(ThemeFontSlot.FontBody, ThemeColorSlot.TextPrimary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot BannerLabel = new(ThemeFontSlot.FontBody, ThemeColorSlot.TextOnAccent, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot DarkBgHeading = new(ThemeFontSlot.FontHeading, ThemeColorSlot.TextOnPrimary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot DarkBgBody = new(ThemeFontSlot.FontBody, ThemeColorSlot.TextOnPrimary, ThemeFontSizeSlot.None);
        public static readonly TextThemeSlot WarningBody = new(ThemeFontSlot.FontBody, ThemeColorSlot.WarningColor, ThemeFontSizeSlot.None);
    }
}
