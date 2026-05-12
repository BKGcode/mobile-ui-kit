using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Hub
{
    public sealed class KitforgeHubState : ScriptableObject
    {
        public enum HubTab
        {
            Setup,
            Catalog,
            Theme,
            Test,
            Help,
        }

        public const string AssetFolder = "Assets/KitforgeLabs/UI Kit/Settings";
        public const string AssetPath = AssetFolder + "/HubState.asset";

        [SerializeField] private HubTab _activeTab = HubTab.Setup;
        [SerializeField] private string _selectedCatalogKey = string.Empty;
        [SerializeField] private string _selectedThemeKey = string.Empty;
        [SerializeField] private string _catalogPatternFilter = string.Empty;
        [SerializeField] private string _catalogSearchQuery = string.Empty;

        [System.NonSerialized] private bool _isPersisted;

        public HubTab ActiveTab
        {
            get => _activeTab;
            set
            {
                if (_activeTab == value) return;
                _activeTab = value;
                Persist();
            }
        }

        public string SelectedCatalogKey
        {
            get => _selectedCatalogKey;
            set
            {
                if (_selectedCatalogKey == value) return;
                _selectedCatalogKey = value ?? string.Empty;
                Persist();
            }
        }

        public string SelectedThemeKey
        {
            get => _selectedThemeKey;
            set
            {
                if (_selectedThemeKey == value) return;
                _selectedThemeKey = value ?? string.Empty;
                Persist();
            }
        }

        public string CatalogPatternFilter
        {
            get => _catalogPatternFilter;
            set
            {
                var normalized = value ?? string.Empty;
                if (_catalogPatternFilter == normalized) return;
                _catalogPatternFilter = normalized;
                Persist();
            }
        }

        public string CatalogSearchQuery
        {
            get => _catalogSearchQuery;
            set
            {
                var normalized = value ?? string.Empty;
                if (_catalogSearchQuery == normalized) return;
                _catalogSearchQuery = normalized;
                Persist();
            }
        }

        public bool IsPersisted => _isPersisted;

        private void Persist()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public static KitforgeHubState GetOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<KitforgeHubState>(AssetPath);
            if (existing != null) return MarkPersisted(existing);
            return TryCreateOrFallback();
        }

        private static KitforgeHubState MarkPersisted(KitforgeHubState state)
        {
            state._isPersisted = true;
            return state;
        }

        private static KitforgeHubState TryCreateOrFallback()
        {
            EnsureFolders();
            var created = CreateInstance<KitforgeHubState>();
            AssetDatabase.CreateAsset(created, AssetPath);
            AssetDatabase.SaveAssets();
            var verified = AssetDatabase.LoadAssetAtPath<KitforgeHubState>(AssetPath);
            if (verified != null) return MarkPersisted(verified);
            Debug.LogError($"[KitforgeHubState] Could not persist state at '{AssetPath}'. Verify '{AssetFolder}' is writable (not under VCS lock). Hub will operate in-memory until permissions allow asset write.");
            return created;
        }

        private static void EnsureFolders()
        {
            if (AssetDatabase.IsValidFolder(AssetFolder)) return;
            Directory.CreateDirectory(AssetFolder);
            AssetDatabase.Refresh();
        }
    }
}
