using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Catalog
{
    internal sealed class KitforgeCatalogBrowser
    {
        private readonly KitforgeHubState _state;

        private VisualElement _root;
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
            BuildGrid();
            BuildDetailPanel();
            return _root;
        }

        private void BuildGrid()
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("kfh-catalog-grid-scroll");
            _grid = new VisualElement();
            _grid.AddToClassList("kfh-catalog-grid");
            foreach (var entry in KitforgeCatalogRegistry.All)
            {
                _grid.Add(BuildCell(entry));
            }
            scroll.Add(_grid);
            _root.Add(scroll);
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
            if (entry.Pattern == KitforgeSpawnPattern.HUD) RegisterDragCallbacks(cell, entry);
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
                Debug.LogWarning($"[KitforgeCatalogBrowser] Prefab for '{entry.DisplayName}' not found. Run Tools → Kitforge → UI Kit → Build Group {entry.Group} Sample to enable drag-to-scene.");
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
                return $"Drag-to-scene unavailable — run Tools → Kitforge → UI Kit → Build Group {entry.Group} Sample first.";
            }
            return "Drag the cell into KitforgeRoot/UICanvas/ScreenRoot. HUDs auto-bind to required services on enable.";
        }

        private string BuildSpawnInstruction(KitforgeCatalogEntry entry)
        {
            var call = ResolveCanonicalCall(entry.Pattern, entry.ComponentType.Name);
            return $"Use the spawn snippet below — `{call}` is the canonical path. Manual scene drop creates a dormant prefab not driven by the manager.";
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
