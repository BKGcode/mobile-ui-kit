using System;
using System.Collections.Generic;
using System.IO;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Theme
{
    internal sealed class KitforgeThemeStudio
    {
        private const string EmptyDropdownPlaceholder = "(no themes found)";

        private readonly KitforgeHubState _state;
        private readonly List<string> _themePaths = new();

        private VisualElement _root;
        private DropdownField _dropdown;
        private VisualElement _detail;

        public KitforgeThemeStudio(KitforgeHubState state)
        {
            _state = state;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.AddToClassList("kfh-theme");
            DiscoverThemes();
            BuildToolbar();
            BuildDetailPanel();
            return _root;
        }

        private void BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("kfh-theme-toolbar");
            _dropdown = BuildDropdown();
            toolbar.Add(_dropdown);
            toolbar.Add(BuildPlaceholderToolbarButton("New Theme", "Coming in M5.4.D (delegates to Assets → Create → Kitforge → Theme)."));
            toolbar.Add(BuildPlaceholderToolbarButton("Capture All Snapshots", "Coming in M5.4.D (delegates to PrefabSnapshotCapture pipeline)."));
            _root.Add(toolbar);
        }

        private DropdownField BuildDropdown()
        {
            var labels = BuildDropdownLabels();
            var hasThemes = _themePaths.Count > 0;
            var displayLabels = hasThemes ? labels : new List<string> { EmptyDropdownPlaceholder };
            var dd = new DropdownField("Theme", displayLabels, ResolveDropdownIndex());
            dd.AddToClassList("kfh-theme-dropdown");
            dd.RegisterValueChangedCallback(OnThemeChanged);
            dd.SetEnabled(hasThemes);
            return dd;
        }

        private List<string> BuildDropdownLabels()
        {
            var labels = new List<string>(_themePaths.Count);
            for (var i = 0; i < _themePaths.Count; i++)
            {
                labels.Add(Path.GetFileNameWithoutExtension(_themePaths[i]));
            }
            return labels;
        }

        private int ResolveDropdownIndex()
        {
            if (_themePaths.Count == 0) return 0;
            var path = _state.SelectedThemeKey;
            if (string.IsNullOrEmpty(path)) return 0;
            for (var i = 0; i < _themePaths.Count; i++)
            {
                if (_themePaths[i] == path) return i;
            }
            return 0;
        }

        private Button BuildPlaceholderToolbarButton(string text, string milestone)
        {
            var btn = new Button(() => Debug.Log($"[KitforgeThemeStudio] {text}: {milestone}")) { text = text };
            btn.AddToClassList("kfh-theme-toolbar-button");
            btn.SetEnabled(false);
            btn.tooltip = milestone;
            return btn;
        }

        private void DiscoverThemes()
        {
            _themePaths.Clear();
            var guids = AssetDatabase.FindAssets("t:" + nameof(UIThemeConfig));
            for (var i = 0; i < guids.Length; i++)
            {
                _themePaths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            }
            _themePaths.Sort(StringComparer.OrdinalIgnoreCase);
        }

        private void OnThemeChanged(ChangeEvent<string> evt)
        {
            var index = _dropdown.index;
            if (index < 0 || index >= _themePaths.Count) return;
            _state.SelectedThemeKey = _themePaths[index];
            RefreshDetail();
        }

        private void BuildDetailPanel()
        {
            _detail = new VisualElement();
            _detail.AddToClassList("kfh-theme-detail");
            RefreshDetail();
            _root.Add(_detail);
        }

        private void RefreshDetail()
        {
            _detail.Clear();
            var theme = ResolveSelectedTheme();
            if (theme == null)
            {
                _detail.Add(BuildEmptyHint());
                return;
            }
            _detail.Add(BuildThemeSummary(theme));
            _detail.Add(BuildPlaceholderStatus());
        }

        private Label BuildEmptyHint()
        {
            var label = new Label("No UIThemeConfig assets found. Run Tools → Kitforge → UI Kit → Bootstrap Defaults to create Theme_Default, or right-click in Project → Create → Kitforge → Theme to create your own.");
            label.AddToClassList("kfh-theme-detail-hint");
            return label;
        }

        private VisualElement BuildThemeSummary(UIThemeConfig theme)
        {
            var box = new VisualElement();
            box.AddToClassList("kfh-theme-summary");
            var title = new Label(theme.name);
            title.AddToClassList("kfh-theme-summary-title");
            box.Add(title);
            var meta = new Label($"Asset: {AssetDatabase.GetAssetPath(theme)}");
            meta.AddToClassList("kfh-theme-summary-meta");
            box.Add(meta);
            return box;
        }

        private Label BuildPlaceholderStatus()
        {
            var label = new Label("Live preview · slot editing · Capture All Snapshots — coming in M5.4.B / .C / .D.");
            label.AddToClassList("kfh-theme-detail-status");
            return label;
        }

        private UIThemeConfig ResolveSelectedTheme()
        {
            var path = _state.SelectedThemeKey;
            if (string.IsNullOrEmpty(path) && _themePaths.Count > 0) path = _themePaths[0];
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<UIThemeConfig>(path);
        }
    }
}
