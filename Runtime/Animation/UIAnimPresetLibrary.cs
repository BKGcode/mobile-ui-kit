using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Animation
{
    [CreateAssetMenu(menuName = "Kitforge/UI Kit/Anim Preset Library", fileName = "UIAnimPresetLibrary", order = 11)]
    public class UIAnimPresetLibrary : ScriptableObject
    {
        [SerializeField] private UIAnimPreset _snappy;
        [SerializeField] private UIAnimPreset _bouncy;
        [SerializeField] private UIAnimPreset _playful;
        [SerializeField] private UIAnimPreset _punchy;
        [SerializeField] private UIAnimPreset _smooth;
        [SerializeField] private UIAnimPreset _elegant;
        [SerializeField] private UIAnimPreset _juicy;
        [SerializeField] private UIAnimPreset _soft;
        [SerializeField] private UIAnimPreset _mechanical;
        [SerializeField] private UIAnimPreset _cinematic;
        [SerializeField] private UIAnimPreset _fallback;

        public UIAnimPreset Resolve(UIAnimStyle style)
        {
            var preset = ResolveInternal(style);
            if (preset != null) return preset;
            if (_fallback != null) return _fallback;
            Debug.LogError($"[UIAnimPresetLibrary] No preset for {style} and no fallback assigned on '{name}'.", this);
            return null;
        }

        private UIAnimPreset ResolveInternal(UIAnimStyle style)
        {
            switch (style)
            {
                case UIAnimStyle.Snappy: return _snappy;
                case UIAnimStyle.Bouncy: return _bouncy;
                case UIAnimStyle.Playful: return _playful;
                case UIAnimStyle.Punchy: return _punchy;
                case UIAnimStyle.Smooth: return _smooth;
                case UIAnimStyle.Elegant: return _elegant;
                case UIAnimStyle.Juicy: return _juicy;
                case UIAnimStyle.Soft: return _soft;
                case UIAnimStyle.Mechanical: return _mechanical;
                case UIAnimStyle.Cinematic: return _cinematic;
                default: return _fallback;
            }
        }
    }
}
