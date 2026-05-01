using KitforgeLabs.MobileUIKit.Core;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Quickstart
{
    [DisallowMultipleComponent]
    public sealed class QuickstartBootstrap : MonoBehaviour
    {
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PopupManager _popupManager;

        private void Awake()
        {
            if (_uiManager == null)
            {
                Debug.LogError("[QuickstartBootstrap] UIManager reference missing.", this);
            }
            if (_popupManager == null)
            {
                Debug.LogError("[QuickstartBootstrap] PopupManager reference missing.", this);
            }
        }

        [ContextMenu("Push Quickstart Screen")]
        private void PushScreen()
        {
            if (_uiManager == null) return;
            _uiManager.Push<QuickstartScreen>();
        }

        [ContextMenu("Pop Screen")]
        private void PopScreen()
        {
            if (_uiManager == null) return;
            _uiManager.Pop();
        }

        [ContextMenu("Show Quickstart Popup")]
        private void ShowPopup()
        {
            if (_popupManager == null) return;
            _popupManager.Show<QuickstartPopup>(null, PopupPriority.Gameplay);
        }

        [ContextMenu("Dismiss All Popups")]
        private void DismissAllPopups()
        {
            if (_popupManager == null) return;
            _popupManager.DismissAll();
        }
    }

    public sealed class QuickstartScreen : UIModuleBase
    {
        public override void OnShow()
        {
            Debug.Log($"[QuickstartScreen] OnShow on '{name}'.", this);
        }

        public override void OnHide()
        {
            Debug.Log($"[QuickstartScreen] OnHide on '{name}'.", this);
        }

        protected override void BindUntyped(object data) { }
    }

    public sealed class QuickstartPopup : UIModuleBase
    {
        public override void OnShow()
        {
            Debug.Log($"[QuickstartPopup] OnShow on '{name}'.", this);
        }

        public override void OnHide()
        {
            Debug.Log($"[QuickstartPopup] OnHide on '{name}'.", this);
        }

        public override void OnBackPressed()
        {
            Debug.Log($"[QuickstartPopup] OnBackPressed on '{name}'.", this);
        }

        protected override void BindUntyped(object data) { }
    }
}
