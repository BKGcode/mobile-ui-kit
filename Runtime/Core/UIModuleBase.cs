using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Core
{
    public abstract class UIModuleBase : MonoBehaviour
    {
        public abstract void OnShow();
        public abstract void OnHide();
        public virtual void OnUpdate() { }
        public virtual void OnBackPressed() { }

        internal abstract void BindUntyped(object data);
    }
}
