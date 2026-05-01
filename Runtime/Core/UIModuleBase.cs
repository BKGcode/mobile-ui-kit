using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Core
{
    public abstract class UIModuleBase : MonoBehaviour
    {
        public event Action<UIModuleBase> DismissRequested;

        public bool IsDismissing { get; protected set; }
        protected UIThemeConfig Theme { get; private set; }
        protected UIServices Services { get; private set; }

        public virtual UIAnimStyle? AnimStyleOverride => null;

        public virtual void Initialize(UIThemeConfig theme, UIServices services)
        {
            Theme = theme;
            Services = services;
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
