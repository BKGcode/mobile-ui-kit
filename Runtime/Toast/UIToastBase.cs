using KitforgeLabs.MobileUIKit.Animation;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Toast
{
    public abstract class UIToastBase : MonoBehaviour
    {
        public abstract float DefaultDuration { get; }
        public abstract void OnShow();
        public abstract void OnHide();

        public virtual UIAnimStyle? AnimStyleOverride => null;

        protected internal abstract void BindUntyped(object data);
    }
}
