using KitforgeLabs.MobileUIKit.Animation;
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
        [SerializeField] private Color _warningColor = new Color(1.00f, 0.60f, 0.15f, 1f);
        [SerializeField] private Color _dangerColor = new Color(0.92f, 0.30f, 0.30f, 1f);
        [SerializeField] private Color _failureColor = new Color(0.898f, 0.224f, 0.208f, 1f);

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
        [SerializeField] private float _minTouchTarget = 88f;

        [Header("Sprites")]
        [SerializeField] private Sprite _panelBackground;
        [SerializeField] private Sprite _buttonPrimary;
        [SerializeField] private Sprite _buttonSecondary;
        [SerializeField] private Sprite _backdrop;
        [SerializeField] private Sprite _divider;
        [SerializeField] private Sprite _iconClose;
        [SerializeField] private Sprite _iconBack;
        [SerializeField] private Sprite _iconCheck;
        [SerializeField] private Sprite _iconInfo;
        [SerializeField] private Sprite _iconWarning;
        [SerializeField] private Sprite _iconError;
        [SerializeField] private Sprite _iconCoin;
        [SerializeField] private Sprite _iconGem;
        [SerializeField] private Sprite _iconEnergy;
        [SerializeField] private Sprite _iconClock;
        [SerializeField] private Sprite _starFilledSprite;
        [SerializeField] private Sprite _starEmptySprite;

        [Header("Audio")]
        [SerializeField] private AudioClip _audioButtonClick;
        [SerializeField] private AudioClip _audioPopupShow;
        [SerializeField] private AudioClip _audioPopupHide;
        [SerializeField] private AudioClip _audioSuccess;
        [SerializeField] private AudioClip _audioError;
        [SerializeField] private AudioClip _audioNotification;

        [Header("Animation")]
        [Tooltip("Animation preset used by all kit popups/toasts unless overridden per-element. If null, popups appear without animation.")]
        [SerializeField] private UIAnimPreset _defaultAnimPreset;

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
        public Color AccentColor => _accentColor;
        public Color BackgroundDark => _backgroundDark;
        public Color BackgroundLight => _backgroundLight;
        public Color TextPrimary => _textPrimary;
        public Color TextSecondary => _textSecondary;
        public Color SuccessColor => _successColor;
        public Color WarningColor => _warningColor;
        public Color DangerColor => _dangerColor;
        public Color FailureColor => _failureColor;

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
        public float MinTouchTarget => _minTouchTarget;

        public Sprite PanelBackground => _panelBackground;
        public Sprite ButtonPrimary => _buttonPrimary;
        public Sprite ButtonSecondary => _buttonSecondary;
        public Sprite Backdrop => _backdrop;
        public Sprite Divider => _divider;
        public Sprite IconClose => _iconClose;
        public Sprite IconBack => _iconBack;
        public Sprite IconCheck => _iconCheck;
        public Sprite IconInfo => _iconInfo;
        public Sprite IconWarning => _iconWarning;
        public Sprite IconError => _iconError;
        public Sprite IconCoin => _iconCoin;
        public Sprite IconGem => _iconGem;
        public Sprite IconEnergy => _iconEnergy;
        public Sprite IconClock => _iconClock;
        public Sprite StarFilledSprite => _starFilledSprite;
        public Sprite StarEmptySprite => _starEmptySprite;

        public AudioClip AudioButtonClick => _audioButtonClick;
        public AudioClip AudioPopupShow => _audioPopupShow;
        public AudioClip AudioPopupHide => _audioPopupHide;
        public AudioClip AudioSuccess => _audioSuccess;
        public AudioClip AudioError => _audioError;
        public AudioClip AudioNotification => _audioNotification;

        public UIAnimPreset DefaultAnimPreset => _defaultAnimPreset;
    }
}
