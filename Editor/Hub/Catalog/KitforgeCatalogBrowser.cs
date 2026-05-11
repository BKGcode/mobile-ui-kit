using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.UIKit.Editor.Hub.Catalog
{
    internal sealed class KitforgeCatalogBrowser
    {
        private const string AllGroupsToken = "";

        private readonly KitforgeHubState _state;

        private VisualElement _root;
        private VisualElement _leftPanel;
        private VisualElement _filterChipRow;
        private TextField _searchField;
        private Label _emptyResultsLabel;
        private VisualElement _grid;
        private VisualElement _detail;
        private bool _dragReady;

        public KitforgeCatalogBrowser(KitforgeHubState state)
        {
            _state = state;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.AddToClassList("kfh-catalog");
            BuildLeftPanel();
            BuildDetailPanel();
            return _root;
        }

        private void BuildLeftPanel()
        {
            _leftPanel = new VisualElement();
            _leftPanel.AddToClassList("kfh-catalog-left");
            BuildFilterBar();
            BuildGrid();
            _root.Add(_leftPanel);
        }

        private void BuildFilterBar()
        {
            var bar = new VisualElement();
            bar.AddToClassList("kfh-catalog-filter-bar");
            BuildChipRow(bar);
            BuildSearchField(bar);
            _leftPanel.Add(bar);
        }

        private void BuildChipRow(VisualElement bar)
        {
            _filterChipRow = new VisualElement();
            _filterChipRow.AddToClassList("kfh-catalog-filter-chip-row");
            AppendChip(_filterChipRow, "All", AllGroupsToken);
            foreach (var group in CollectGroups())
            {
                AppendChip(_filterChipRow, group, group);
            }
            bar.Add(_filterChipRow);
        }

        private void AppendChip(VisualElement row, string label, string groupToken)
        {
            var btn = new Button(() => OnChipClicked(groupToken)) { text = label };
            btn.AddToClassList("kfh-catalog-filter-chip");
            btn.userData = groupToken;
            UpdateChipSelection(btn, groupToken);
            row.Add(btn);
        }

        private void OnChipClicked(string groupToken)
        {
            _state.CatalogGroupFilter = groupToken;
            UpdateAllChipSelections();
            RebuildGridContents();
        }

        private void UpdateAllChipSelections()
        {
            foreach (var child in _filterChipRow.Children())
            {
                if (child is Button btn && btn.userData is string token) UpdateChipSelection(btn, token);
            }
        }

        private void UpdateChipSelection(Button btn, string groupToken)
        {
            btn.EnableInClassList("kfh-catalog-filter-chip--active", _state.CatalogGroupFilter == groupToken);
        }

        private void BuildSearchField(VisualElement bar)
        {
            _searchField = new TextField { value = _state.CatalogSearchQuery };
            _searchField.AddToClassList("kfh-catalog-filter-search");
            var input = _searchField.Q(TextField.textInputUssName);
            if (input != null) input.tooltip = "Filter by display name (case-insensitive)";
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            bar.Add(_searchField);
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _state.CatalogSearchQuery = evt.newValue ?? string.Empty;
            RebuildGridContents();
        }

        private void BuildGrid()
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("kfh-catalog-grid-scroll");
            _grid = new VisualElement();
            _grid.AddToClassList("kfh-catalog-grid");
            _emptyResultsLabel = new Label("No catalog elements match the current filter. Try clearing the search or selecting another group.");
            _emptyResultsLabel.AddToClassList("kfh-catalog-empty-results");
            scroll.Add(_grid);
            scroll.Add(_emptyResultsLabel);
            _leftPanel.Add(scroll);
            RebuildGridContents();
        }

        private void RebuildGridContents()
        {
            _grid.Clear();
            var visibleCount = AppendVisibleCells();
            _emptyResultsLabel.style.display = visibleCount == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private int AppendVisibleCells()
        {
            var count = 0;
            foreach (var entry in KitforgeCatalogRegistry.All)
            {
                if (!EntryPassesFilters(entry)) continue;
                _grid.Add(BuildCell(entry));
                count++;
            }
            return count;
        }

        private bool EntryPassesFilters(KitforgeCatalogEntry entry)
        {
            if (!string.IsNullOrEmpty(_state.CatalogGroupFilter) && entry.Group != _state.CatalogGroupFilter) return false;
            var query = _state.CatalogSearchQuery;
            if (string.IsNullOrEmpty(query)) return true;
            return entry.DisplayName.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static IEnumerable<string> CollectGroups()
        {
            var seen = new HashSet<string>();
            var ordered = new List<string>();
            foreach (var entry in KitforgeCatalogRegistry.All)
            {
                if (seen.Add(entry.Group)) ordered.Add(entry.Group);
            }
            ordered.Sort(System.StringComparer.Ordinal);
            return ordered;
        }

        private VisualElement BuildCell(KitforgeCatalogEntry entry)
        {
            var cell = new VisualElement();
            cell.AddToClassList("kfh-catalog-cell");
            cell.userData = entry;
            cell.Add(BuildCellGroupBadge(entry));
            cell.Add(BuildCellName(entry));
            cell.Add(BuildCellPattern(entry));
            cell.RegisterCallback<ClickEvent>(_ => SelectEntry(entry));
            RegisterDragCallbacks(cell, entry);
            UpdateCellSelection(cell, entry);
            return cell;
        }

        private void RegisterDragCallbacks(VisualElement cell, KitforgeCatalogEntry entry)
        {
            cell.AddToClassList("kfh-catalog-cell--draggable");
            cell.RegisterCallback<MouseDownEvent>(OnCellMouseDown);
            cell.RegisterCallback<MouseLeaveEvent>(_ => OnCellMouseLeave(entry));
            cell.RegisterCallback<MouseUpEvent>(_ => OnCellMouseUp());
        }

        private Label BuildCellGroupBadge(KitforgeCatalogEntry entry)
        {
            var label = new Label(entry.Group);
            label.AddToClassList("kfh-catalog-cell-group");
            return label;
        }

        private Label BuildCellName(KitforgeCatalogEntry entry)
        {
            var label = new Label(entry.DisplayName);
            label.AddToClassList("kfh-catalog-cell-name");
            return label;
        }

        private Label BuildCellPattern(KitforgeCatalogEntry entry)
        {
            var label = new Label(entry.Pattern.ToString());
            label.AddToClassList("kfh-catalog-cell-pattern");
            return label;
        }

        private void OnCellMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            _dragReady = true;
        }

        private void OnCellMouseLeave(KitforgeCatalogEntry entry)
        {
            if (!_dragReady) return;
            _dragReady = false;
            StartDragForEntry(entry);
        }

        private void OnCellMouseUp()
        {
            _dragReady = false;
        }

        private void StartDragForEntry(KitforgeCatalogEntry entry)
        {
            var prefab = KitforgeCatalogPrefabResolver.LoadPrefab(entry);
            if (prefab == null)
            {
                Debug.Log($"[KitforgeCatalogBrowser] Drag-to-scene for catalog entries is not available in this release. Use the snippet panel on the right to copy the Show/Push call into your script.");
                return;
            }
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new Object[] { prefab };
            DragAndDrop.StartDrag(entry.DisplayName);
        }

        private void BuildDetailPanel()
        {
            _detail = new VisualElement();
            _detail.AddToClassList("kfh-catalog-detail");
            RefreshDetail();
            _root.Add(_detail);
        }

        private void SelectEntry(KitforgeCatalogEntry entry)
        {
            _state.SelectedCatalogKey = entry.Key;
            UpdateAllCellSelections();
            RefreshDetail();
        }

        private void UpdateAllCellSelections()
        {
            foreach (var child in _grid.Children())
            {
                if (child.userData is KitforgeCatalogEntry e) UpdateCellSelection(child, e);
            }
        }

        private void UpdateCellSelection(VisualElement cell, KitforgeCatalogEntry entry)
        {
            cell.EnableInClassList("kfh-catalog-cell--selected", _state.SelectedCatalogKey == entry.Key);
        }

        private void RefreshDetail()
        {
            _detail.Clear();
            var entry = ResolveSelectedEntry();
            if (entry == null)
            {
                _detail.Add(BuildDetailHint());
                return;
            }
            _detail.Add(BuildDetailContent(entry));
        }

        private Label BuildDetailHint()
        {
            var hint = new Label("Select an element from the grid to see details, drag-to-scene, and the spawn snippet.");
            hint.AddToClassList("kfh-catalog-detail-hint");
            return hint;
        }

        private VisualElement BuildDetailContent(KitforgeCatalogEntry entry)
        {
            var pane = new VisualElement();
            pane.Add(BuildDetailTitle(entry));
            pane.Add(BuildDetailMeta(entry));
            pane.Add(BuildDetailDescription(entry));
            pane.Add(BuildDragInstruction(entry));
            pane.Add(BuildFieldList(entry));
            pane.Add(BuildSnippetField(entry));
            return pane;
        }

        private Label BuildDetailTitle(KitforgeCatalogEntry entry)
        {
            var title = new Label(entry.DisplayName);
            title.AddToClassList("kfh-catalog-detail-title");
            return title;
        }

        private Label BuildDetailMeta(KitforgeCatalogEntry entry)
        {
            var meta = new Label($"{entry.Pattern} · Group {entry.Group} · {entry.ComponentType.Name}");
            meta.AddToClassList("kfh-catalog-detail-meta");
            return meta;
        }

        private Label BuildDetailDescription(KitforgeCatalogEntry entry)
        {
            var desc = new Label(entry.Description);
            desc.AddToClassList("kfh-catalog-detail-description");
            return desc;
        }

        private Label BuildDragInstruction(KitforgeCatalogEntry entry)
        {
            var msg = entry.Pattern == KitforgeSpawnPattern.HUD
                ? BuildHUDInstruction(entry)
                : BuildSpawnInstruction(entry);
            var label = new Label(msg);
            label.AddToClassList("kfh-catalog-detail-instruction");
            return label;
        }

        private string BuildHUDInstruction(KitforgeCatalogEntry entry)
        {
            var prefab = KitforgeCatalogPrefabResolver.LoadPrefab(entry);
            if (prefab == null)
            {
                return $"Drag-to-scene unavailable — run KitforgeLabs → UI Kit → Build Group {entry.Group} Sample first.";
            }
            return "Drag the cell into KitforgeRoot/UICanvas/ScreenRoot. HUDs auto-bind to required services on enable.";
        }

        private string BuildSpawnInstruction(KitforgeCatalogEntry entry)
        {
            var call = ResolveCanonicalCall(entry.Pattern, entry.ComponentType.Name);
            var prefab = KitforgeCatalogPrefabResolver.LoadPrefab(entry);
            if (prefab == null)
            {
                return $"Use the spawn snippet below — `{call}` is the canonical path. Drag-to-scene preview unavailable: run KitforgeLabs → UI Kit → Build Group {entry.Group} Sample first.";
            }
            return $"Use the spawn snippet below — `{call}` is the canonical path. Drag-to-scene drops a static preview prefab (no DTO, no manager driving) for visual composition; delete it before Play.";
        }

        private string ResolveCanonicalCall(KitforgeSpawnPattern pattern, string typeName)
        {
            return pattern switch
            {
                KitforgeSpawnPattern.Popup => $"_popupManager.Show<{typeName}>(data)",
                KitforgeSpawnPattern.Screen => $"_uiManager.Push<{typeName}>(data)",
                KitforgeSpawnPattern.Toast => $"_toastManager.Show<{typeName}>(data)",
                _ => string.Empty,
            };
        }

        private VisualElement BuildFieldList(KitforgeCatalogEntry entry)
        {
            var box = new VisualElement();
            box.AddToClassList("kfh-catalog-detail-fields");
            box.Add(MakeFieldsHeader());
            PopulateFieldsBox(box, entry);
            return box;
        }

        private Label MakeFieldsHeader()
        {
            var header = new Label("DTO fields");
            header.AddToClassList("kfh-catalog-detail-fields-header");
            return header;
        }

        private void PopulateFieldsBox(VisualElement box, KitforgeCatalogEntry entry)
        {
            var fields = KitforgeCatalogSnippetBuilder.BuildFieldList(entry);
            if (fields.Count == 0)
            {
                box.Add(MakeFieldsEmptyLabel(entry));
                return;
            }
            for (var i = 0; i < fields.Count; i++) box.Add(MakeFieldRow(fields[i]));
        }

        private Label MakeFieldsEmptyLabel(KitforgeCatalogEntry entry)
        {
            var msg = entry.Pattern == KitforgeSpawnPattern.HUD
                ? "HUD components are driven by services, not a DTO."
                : "No DTO detected.";
            var label = new Label(msg);
            label.AddToClassList("kfh-catalog-detail-fields-empty");
            return label;
        }

        private Label MakeFieldRow(string field)
        {
            var label = new Label("· " + field);
            label.AddToClassList("kfh-catalog-detail-fields-item");
            return label;
        }

        private VisualElement BuildSnippetField(KitforgeCatalogEntry entry)
        {
            var box = new VisualElement();
            box.AddToClassList("kfh-catalog-detail-snippet-box");
            var header = new Label("Spawn snippet");
            header.AddToClassList("kfh-catalog-detail-snippet-header");
            box.Add(header);
            var snippet = new TextField { value = KitforgeCatalogSnippetBuilder.BuildSpawnSnippet(entry), isReadOnly = true, multiline = true };
            snippet.AddToClassList("kfh-catalog-detail-snippet");
            box.Add(snippet);
            return box;
        }

        private KitforgeCatalogEntry ResolveSelectedEntry()
        {
            var key = _state.SelectedCatalogKey;
            if (string.IsNullOrEmpty(key)) return null;
            foreach (var entry in KitforgeCatalogRegistry.All)
            {
                if (entry.Key == key) return entry;
            }
            return null;
        }
    }
}
