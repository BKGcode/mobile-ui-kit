using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Animation
{
    [CreateAssetMenu(menuName = "Kitforge/UI Kit/Anim Preset", fileName = "UIAnimPreset", order = 10)]
    public class UIAnimPreset : ScriptableObject
    {
        [Header("Show")]
        [SerializeField] private float _showDuration = 0.40f;
        [SerializeField] private UIAnimEase _showEase = UIAnimEase.OutBack;

        [Header("Hide")]
        [SerializeField] private float _hideDuration = 0.20f;
        [SerializeField] private UIAnimEase _hideEase = UIAnimEase.InQuad;

        [Header("Channels")]
        [SerializeField] private bool _useScale = true;
        [SerializeField] private bool _useFade = true;
        [SerializeField] private bool _usePosition = false;
        [SerializeField] private bool _useRotation = false;

        [Header("Tuning")]
        [SerializeField] private float _scaleFrom = 0.7f;
        [SerializeField] private float _scaleOvershoot = 1.10f;
        [SerializeField] private float _hideScaleTo = 0.9f;
        [SerializeField] private Vector2 _positionOffset = new Vector2(0f, -120f);
        [SerializeField] private float _rotationFrom = -8f;

        [Header("Button Feedback")]
        [SerializeField] private float _pressedScale = 0.92f;
        [SerializeField] private float _pressedDuration = 0.08f;
        [SerializeField] private UIAnimEase _pressedEase = UIAnimEase.OutQuad;

        public float ShowDuration => _showDuration;
        public UIAnimEase ShowEase => _showEase;
        public float HideDuration => _hideDuration;
        public UIAnimEase HideEase => _hideEase;
        public bool UseScale => _useScale;
        public bool UseFade => _useFade;
        public bool UsePosition => _usePosition;
        public bool UseRotation => _useRotation;
        public float ScaleFrom => _scaleFrom;
        public float ScaleOvershoot => _scaleOvershoot;
        public float HideScaleTo => _hideScaleTo;
        public Vector2 PositionOffset => _positionOffset;
        public float RotationFrom => _rotationFrom;
        public float PressedScale => _pressedScale;
        public float PressedDuration => _pressedDuration;
        public UIAnimEase PressedEase => _pressedEase;
    }
}
