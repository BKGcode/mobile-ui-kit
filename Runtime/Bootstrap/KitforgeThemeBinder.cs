using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.Theme;
using KitforgeLabs.UIKit.Toast;
using UnityEngine;

namespace KitforgeLabs.UIKit.Bootstrap
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class KitforgeThemeBinder : MonoBehaviour
    {
        [SerializeField] private UIThemeConfig _theme;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PopupManager _popupManager;
        [SerializeField] private ToastManager _toastManager;

        public UIThemeConfig Theme => _theme;

        private void Awake()
        {
            DistributeTheme();
        }

        public void SetTheme(UIThemeConfig theme)
        {
            _theme = theme;
            DistributeTheme();
        }

        private void DistributeTheme()
        {
            if (_theme == null) return;
            if (_uiManager != null) _uiManager.SetTheme(_theme);
            if (_popupManager != null) _popupManager.SetTheme(_theme);
            if (_toastManager != null) _toastManager.SetTheme(_theme);
        }
    }
}
