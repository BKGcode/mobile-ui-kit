using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Theme
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class ThemedImage : MonoBehaviour, IThemedElement
    {
        [Tooltip("Image whose sprite/color is driven by the Theme. Auto-resolved if empty.")]
        [SerializeField] private Image _image;
        [Tooltip("Sprite slot to read from the Theme. None = leave the Image's authored sprite untouched.")]
        [SerializeField] private ThemeSpriteSlot _spriteSlot = ThemeSpriteSlot.None;
        [Tooltip("Color slot to read from the Theme. None = leave the Image's authored color untouched.")]
        [SerializeField] private ThemeColorSlot _colorSlot = ThemeColorSlot.None;

        private void Awake()
        {
            if (_image == null) _image = GetComponent<Image>();
        }

        public void ApplyTheme(UIThemeConfig theme)
        {
            if (theme == null) return;
            if (_image == null) _image = GetComponent<Image>();
            if (_image == null) return;
            var sprite = ResolveSprite(_spriteSlot, theme);
            if (sprite != null) _image.sprite = sprite;
            if (TryResolveColor(_colorSlot, theme, out var color)) _image.color = color;
        }

        private static Sprite ResolveSprite(ThemeSpriteSlot slot, UIThemeConfig theme)
        {
            switch (slot)
            {
                case ThemeSpriteSlot.PanelBackground: return theme.PanelBackground;
                case ThemeSpriteSlot.ButtonPrimary:   return theme.ButtonPrimary;
                case ThemeSpriteSlot.ButtonSecondary: return theme.ButtonSecondary;
                case ThemeSpriteSlot.Backdrop:        return theme.Backdrop;
                case ThemeSpriteSlot.Divider:         return theme.Divider;
                default:                              return null;
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
    }
}
