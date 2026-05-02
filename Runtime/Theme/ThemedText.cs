using TMPro;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Theme
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ThemedText : MonoBehaviour, IThemedElement
    {
        [Tooltip("TMP label whose font/color/size is driven by the Theme. Auto-resolved if empty.")]
        [SerializeField] private TMP_Text _text;
        [Tooltip("Font slot to read from the Theme. None = leave the authored font untouched.")]
        [SerializeField] private ThemeFontSlot _fontSlot = ThemeFontSlot.None;
        [Tooltip("Color slot to read from the Theme. None = leave the authored color untouched.")]
        [SerializeField] private ThemeColorSlot _colorSlot = ThemeColorSlot.None;
        [Tooltip("Font size slot to read from the Theme. None = leave the authored size untouched.")]
        [SerializeField] private ThemeFontSizeSlot _sizeSlot = ThemeFontSizeSlot.None;

        private void Awake()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
        }

        public void ApplyTheme(UIThemeConfig theme)
        {
            if (theme == null) return;
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_text == null) return;
            var font = ResolveFont(_fontSlot, theme);
            if (font != null) _text.font = font;
            if (TryResolveColor(_colorSlot, theme, out var color)) _text.color = color;
            if (TryResolveSize(_sizeSlot, theme, out var size)) _text.fontSize = size;
        }

        private static TMP_FontAsset ResolveFont(ThemeFontSlot slot, UIThemeConfig theme)
        {
            switch (slot)
            {
                case ThemeFontSlot.FontHeading: return theme.FontHeading;
                case ThemeFontSlot.FontBody:    return theme.FontBody;
                case ThemeFontSlot.FontCaption: return theme.FontCaption;
                default:                        return null;
            }
        }

        private static bool TryResolveColor(ThemeColorSlot slot, UIThemeConfig theme, out Color color)
        {
            switch (slot)
            {
                case ThemeColorSlot.PrimaryColor:    color = theme.PrimaryColor; return true;
                case ThemeColorSlot.SecondaryColor:  color = theme.SecondaryColor; return true;
                case ThemeColorSlot.AccentColor:     color = theme.AccentColor; return true;
                case ThemeColorSlot.BackgroundDark:  color = theme.BackgroundDark; return true;
                case ThemeColorSlot.BackgroundLight: color = theme.BackgroundLight; return true;
                case ThemeColorSlot.TextPrimary:     color = theme.TextPrimary; return true;
                case ThemeColorSlot.TextSecondary:   color = theme.TextSecondary; return true;
                case ThemeColorSlot.SuccessColor:    color = theme.SuccessColor; return true;
                case ThemeColorSlot.WarningColor:    color = theme.WarningColor; return true;
                case ThemeColorSlot.DangerColor:     color = theme.DangerColor; return true;
                default:                             color = default; return false;
            }
        }

        private static bool TryResolveSize(ThemeFontSizeSlot slot, UIThemeConfig theme, out float size)
        {
            switch (slot)
            {
                case ThemeFontSizeSlot.SizeHeading: size = theme.SizeHeading; return true;
                case ThemeFontSizeSlot.SizeBody:    size = theme.SizeBody; return true;
                case ThemeFontSizeSlot.SizeCaption: size = theme.SizeCaption; return true;
                default:                            size = 0f; return false;
            }
        }
    }
}
