using TMPro;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Theme
{
    [CreateAssetMenu(menuName = "Kitforge/UI Kit/Theme Config", fileName = "UIThemeConfig", order = 0)]
    public class UIThemeConfig : ScriptableObject
    {
        [Header("Colors")]
        [SerializeField] private Color _primaryColor = new Color(0.20f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color _secondaryColor = new Color(0.35f, 0.40f, 0.50f, 1f);
        [SerializeField] private Color _accentColor = new Color(1.00f, 0.75f, 0.20f, 1f);
        [SerializeField] private Color _backgroundDark = new Color(0.10f, 0.12f, 0.16f, 1f);
        [SerializeField] private Color _backgroundLight = new Color(0.96f, 0.97f, 0.98f, 1f);
        [SerializeField] private Color _textPrimary = new Color(0.10f, 0.10f, 0.12f, 1f);
        [SerializeField] private Color _textSecondary = new Color(0.45f, 0.48f, 0.55f, 1f);
        [SerializeField] private Color _successColor = new Color(0.30f, 0.78f, 0.45f, 1f);
        [SerializeField] private Color _dangerColor = new Color(0.92f, 0.30f, 0.30f, 1f);

        [Header("Typography")]
        [SerializeField] private TMP_FontAsset _fontHeading;
        [SerializeField] private TMP_FontAsset _fontBody;
        [SerializeField] private TMP_FontAsset _fontCaption;
        [SerializeField] private float _sizeHeading = 36f;
        [SerializeField] private float _sizeBody = 24f;
        [SerializeField] private float _sizeCaption = 18f;

        [Header("Spacing")]
        [SerializeField] private float _spacingS = 8f;
        [SerializeField] private float _spacingM = 16f;
        [SerializeField] private float _spacingL = 32f;

        [Header("Shape")]
        [SerializeField] private float _cornerRadius = 16f;

        [Header("Animation")]
        [SerializeField] private float _transitionSpeed = 0.3f;
        [SerializeField] private float _bounceStrength = 1.1f;

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
        public Color AccentColor => _accentColor;
        public Color BackgroundDark => _backgroundDark;
        public Color BackgroundLight => _backgroundLight;
        public Color TextPrimary => _textPrimary;
        public Color TextSecondary => _textSecondary;
        public Color SuccessColor => _successColor;
        public Color DangerColor => _dangerColor;

        public TMP_FontAsset FontHeading => _fontHeading;
        public TMP_FontAsset FontBody => _fontBody;
        public TMP_FontAsset FontCaption => _fontCaption;
        public float SizeHeading => _sizeHeading;
        public float SizeBody => _sizeBody;
        public float SizeCaption => _sizeCaption;

        public float SpacingS => _spacingS;
        public float SpacingM => _spacingM;
        public float SpacingL => _spacingL;

        public float CornerRadius => _cornerRadius;

        public float TransitionSpeed => _transitionSpeed;
        public float BounceStrength => _bounceStrength;
    }
}
