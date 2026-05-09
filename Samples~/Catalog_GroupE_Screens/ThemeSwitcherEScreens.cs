using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Theme;
using KitforgeLabs.MobileUIKit.Toast;
using TMPro;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupE
{
    public class ThemeSwitcherEScreens : MonoBehaviour
    {
        [Serializable]
        private struct ThemeOption
        {
            public string Name;
            public UIThemeConfig Theme;
        }

        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PopupManager _popupManager;
        [SerializeField] private ToastManager _toastManager;
        [SerializeField] private TMP_Dropdown _themeDropdown;
        [SerializeField] private List<ThemeOption> _themes = new List<ThemeOption>();

        private void Start()
        {
            if (_themes.Count == 0)
            {
                Debug.LogWarning("[ThemeSwitcherEScreens] No themes assigned. Run 'Tools/Kitforge/UI Kit/Build Group E Sample' (and 'Build M4.1 — Theme Presets' first for full Casual/Premium coverage).", this);
                if (_themeDropdown != null) _themeDropdown.interactable = false;
                return;
            }
            PopulateDropdown();
        }

        private void PopulateDropdown()
        {
            if (_themeDropdown == null) return;
            _themeDropdown.ClearOptions();
            var options = new List<string>();
            foreach (var t in _themes) options.Add(t.Name);
            _themeDropdown.AddOptions(options);
            _themeDropdown.onValueChanged.AddListener(OnDropdownChanged);
            _themeDropdown.value = 0;
        }

        private void OnDropdownChanged(int index)
        {
            if (index < 0 || index >= _themes.Count) return;
            var theme = _themes[index].Theme;
            if (theme == null) return;
            if (_uiManager != null) _uiManager.SetTheme(theme);
            if (_popupManager != null) _popupManager.SetTheme(theme);
            if (_toastManager != null) _toastManager.SetTheme(theme);
        }
    }
}
