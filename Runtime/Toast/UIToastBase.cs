using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Toast
{
    public abstract class UIToastBase : MonoBehaviour
    {
        public event Action<UIToastBase> DismissRequested;

        public bool IsDismissing { get; protected set; }
        protected UIThemeConfig Theme { get; private set; }
        protected UIServices Services { get; private set; }

        public abstract float DefaultDuration { get; }
        public abstract void OnShow();

        /// <summary>
        /// Post-hide hook. Called by ToastManager AFTER auto-dismiss timer or manual dismissal.
        /// Use to skip residual animations or release transient resources — NOT to trigger the hide animation itself.
        /// </summary>
        public abstract void OnHide();

        public virtual UIAnimPreset AnimPresetOverride => null;

        private bool _presetWarningLogged;

        public virtual void Initialize(UIThemeConfig theme, UIServices services)
        {
            Theme = theme;
            Services = services;
        }

        protected UIAnimPreset ResolveAnimPreset()
        {
            var preset = AnimPresetOverride != null ? AnimPresetOverride : Theme?.DefaultAnimPreset;
            if (preset == null && !_presetWarningLogged)
            {
                _presetWarningLogged = true;
                Debug.LogWarning($"[{GetType().Name}] No animation preset resolved — toast will appear without animation. Run 'Tools/Kitforge/UI Kit/Bootstrap Defaults' or assign UIThemeConfig.DefaultAnimPreset.", this);
            }
            return preset;
        }

        protected void RaiseDismissRequested()
        {
            DismissRequested?.Invoke(this);
        }

        protected internal abstract void BindUntyped(object data);
    }
}
