using System;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;
using KitforgeLabs.MobileUIKit.Editor.Hub.Help;
using KitforgeLabs.MobileUIKit.Editor.Hub.Setup;
using KitforgeLabs.MobileUIKit.Editor.Hub.Test;
using KitforgeLabs.MobileUIKit.Editor.Hub.Theme;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Hub
{
    public sealed class KitforgeHubWindow : EditorWindow
    {
        private const string UssPath = "Packages/com.kitforgelabs.mobile-ui-kit/Editor/Hub/KitforgeHubWindow.uss";

        private KitforgeHubState _state;
        private VisualElement _sidebar;
        private VisualElement _content;

        [MenuItem("Tools/Kitforge/Hub")]
        public static void Open()
        {
            var w = GetWindow<KitforgeHubWindow>("Kitforge Hub");
            w.minSize = new Vector2(900f, 560f);
        }

        private void CreateGUI()
        {
            _state = KitforgeHubState.GetOrCreate();
            BuildLayout();
            RenderActiveTab();
        }

        private void BuildLayout()
        {
            ApplyStylesheet();
            rootVisualElement.AddToClassList("kfh-root");
            BuildHeader();
            BuildPersistenceBanner();
            var body = new VisualElement();
            body.AddToClassList("kfh-body");
            BuildSidebar(body);
            BuildContentArea(body);
            rootVisualElement.Add(body);
        }

        private void ApplyStylesheet()
        {
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (ss == null)
            {
                Debug.LogWarning($"[KitforgeHub] StyleSheet not found at '{UssPath}'. Window will render unstyled. Verify package install integrity.");
                return;
            }
            rootVisualElement.styleSheets.Add(ss);
        }

        private void BuildHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("kfh-header");
            var title = new Label("Kitforge Hub");
            title.AddToClassList("kfh-header-title");
            header.Add(title);
            var sub = new Label("Single front door for the kit — Setup · Catalog · Theme · Test · Help.");
            sub.AddToClassList("kfh-header-subtitle");
            header.Add(sub);
            rootVisualElement.Add(header);
        }

        private void BuildPersistenceBanner()
        {
            if (_state.IsPersisted) return;
            var banner = new VisualElement();
            banner.AddToClassList("kfh-banner");
            var msg = new Label($"Hub state is in-memory only — could not write {KitforgeHubState.AssetPath}. Verify Assets/Settings/Kitforge/ is writable. Tab choice will reset on Editor restart.");
            msg.AddToClassList("kfh-banner-message");
            banner.Add(msg);
            rootVisualElement.Add(banner);
        }

        private void BuildSidebar(VisualElement parent)
        {
            _sidebar = new VisualElement();
            _sidebar.AddToClassList("kfh-sidebar");
            foreach (KitforgeHubState.HubTab tab in Enum.GetValues(typeof(KitforgeHubState.HubTab)))
            {
                _sidebar.Add(MakeTabButton(tab));
            }
            parent.Add(_sidebar);
        }

        private VisualElement MakeTabButton(KitforgeHubState.HubTab tab)
        {
            var btn = new Label(GetTabLabel(tab));
            btn.AddToClassList("kfh-sidebar-tab");
            btn.userData = tab;
            btn.RegisterCallback<ClickEvent>(_ => SwitchTab(tab));
            return btn;
        }

        private void BuildContentArea(VisualElement parent)
        {
            _content = new VisualElement();
            _content.AddToClassList("kfh-content");
            parent.Add(_content);
        }

        private void SwitchTab(KitforgeHubState.HubTab tab)
        {
            _state.ActiveTab = tab;
            RenderActiveTab();
        }

        private void RenderActiveTab()
        {
            UpdateSidebarSelection();
            _content.Clear();
            _content.Add(BuildPaneFor(_state.ActiveTab));
        }

        private void UpdateSidebarSelection()
        {
            foreach (var child in _sidebar.Children())
            {
                if (child.userData is not KitforgeHubState.HubTab tab) continue;
                child.EnableInClassList("kfh-sidebar-tab--active", tab == _state.ActiveTab);
            }
        }

        private VisualElement BuildPaneFor(KitforgeHubState.HubTab tab)
        {
            if (tab == KitforgeHubState.HubTab.Setup) return new KitforgeSetupWizard().Build();
            if (tab == KitforgeHubState.HubTab.Catalog) return new KitforgeCatalogBrowser(_state).Build();
            if (tab == KitforgeHubState.HubTab.Theme) return new KitforgeThemeStudio(_state).Build();
            if (tab == KitforgeHubState.HubTab.Test) return new KitforgeTestLauncher(_state, this).Build();
            if (tab == KitforgeHubState.HubTab.Help) return new KitforgeHelpTab().Build();
            Debug.LogError($"[KitforgeHub] Unknown tab '{tab}' — BuildPaneFor needs a wire-up. This is a kit-author bug; reinstall the package or report the missing branch.");
            return new VisualElement();
        }

        private string GetTabLabel(KitforgeHubState.HubTab tab) => tab switch
        {
            KitforgeHubState.HubTab.Setup => "Setup",
            KitforgeHubState.HubTab.Catalog => "Catalog",
            KitforgeHubState.HubTab.Theme => "Theme",
            KitforgeHubState.HubTab.Test => "Test",
            KitforgeHubState.HubTab.Help => "Help",
            _ => tab.ToString(),
        };
    }
}
