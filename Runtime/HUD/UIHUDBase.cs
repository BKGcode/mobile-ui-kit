using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.HUD
{
    public abstract class UIHUDBase : MonoBehaviour
    {
        [SerializeField] private UIServices _services;

        protected UIServices Services => _services;

        protected void SetServicesInternal(UIServices services) => _services = services;

        protected virtual void OnEnable()
        {
            if (_services == null)
            {
                Debug.LogError($"[{GetType().Name}] UIServices reference is null on '{name}'.", this);
                return;
            }
            Subscribe();
            Refresh();
        }

        protected virtual void OnDisable()
        {
            if (_services == null) return;
            Unsubscribe();
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract void Refresh();
    }
}
