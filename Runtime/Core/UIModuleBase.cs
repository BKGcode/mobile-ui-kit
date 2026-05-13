using System;
using KitforgeLabs.UIKit.Animation;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.UIKit.Core
{
    public abstract class UIModuleBase : MonoBehaviour
    {
        public event Action<UIModuleBase> DismissRequested;

        public bool IsDismissing { get; protected set; }
        protected UIThemeConfig Theme { get; private set; }
        protected UIServices Services { get; private set; }

        public virtual UIAnimPreset AnimPresetOverride => null;

        private bool _presetWarningLogged;

        public virtual void Initialize(UIThemeConfig theme, UIServices services)
        {
            Theme = theme;
            Services = services;
            ApplyThemeToChildren(theme);
        }

        protected void ApplyThemeToChildren(UIThemeConfig theme)
        {
            if (theme == null) return;
            var themed = GetComponentsInChildren<IThemedElement>(true);
            for (var i = 0; i < themed.Length; i++) themed[i].ApplyTheme(theme);
        }

        protected UIAnimPreset ResolveAnimPreset()
        {
            var preset = AnimPresetOverride != null ? AnimPresetOverride : Theme?.DefaultAnimPreset;
            if (preset == null && !_presetWarningLogged)
            {
                _presetWarningLogged = true;
                Debug.LogWarning($"[{GetType().Name}] No animation preset resolved — popup will appear without animation. Assign a UIAnimPreset asset to UIThemeConfig.DefaultAnimPreset (Theme_Default ships pre-wired to UIAnimPreset_Playful).", this);
            }
            return preset;
        }

        public abstract void OnShow();

        /// <summary>
        /// Post-hide hook. Called by the manager AFTER the module is removed from the active stack.
        /// Use this to skip residual animations or release transient resources — NOT to trigger the hide animation.
        /// The hide animation is owned by the module and triggered from interaction handlers via the
        /// DismissWithAnimation pattern, which finalizes by calling RaiseDismissRequested().
        /// The manager then invokes OnHide() as a final cleanup pass.
        /// </summary>
        public abstract void OnHide();

        public virtual void OnUpdate() { }
        public virtual void OnBackPressed() { }

        protected void RaiseDismissRequested()
        {
            DismissRequested?.Invoke(this);
        }

        protected internal abstract void BindUntyped(object data);
    }
}
