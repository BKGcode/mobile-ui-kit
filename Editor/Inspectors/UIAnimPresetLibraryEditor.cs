using KitforgeLabs.MobileUIKit.Animation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Inspectors
{
    [CustomEditor(typeof(UIAnimPresetLibrary))]
    public class UIAnimPresetLibraryEditor : UnityEditor.Editor
    {
        private const string StyleSheetPath =
            "Packages/com.kitforgelabs.mobile-ui-kit/Editor/Inspectors/UIAnimPresetLibraryEditor.uss";

        private static readonly (UIAnimStyle Style, string Field)[] _styleFields =
        {
            (UIAnimStyle.Snappy,     "_snappy"),
            (UIAnimStyle.Bouncy,     "_bouncy"),
            (UIAnimStyle.Playful,    "_playful"),
            (UIAnimStyle.Punchy,     "_punchy"),
            (UIAnimStyle.Smooth,     "_smooth"),
            (UIAnimStyle.Elegant,    "_elegant"),
            (UIAnimStyle.Juicy,      "_juicy"),
            (UIAnimStyle.Soft,       "_soft"),
            (UIAnimStyle.Mechanical, "_mechanical"),
            (UIAnimStyle.Cinematic,  "_cinematic")
        };

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null) root.styleSheets.Add(sheet);
            root.Add(BuildHeader());
            root.Add(BuildTable());
            root.Add(BuildFallbackRow());
            return root;
        }

        private static VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("lib-header");
            header.Add(MakeHeaderCell("Style", "lib-col-style"));
            header.Add(MakeHeaderCell("Preset", "lib-col-preset"));
            header.Add(MakeHeaderCell("Status", "lib-col-status"));
            return header;
        }

        private static Label MakeHeaderCell(string text, string columnClass)
        {
            var label = new Label(text);
            label.AddToClassList("lib-header-cell");
            label.AddToClassList(columnClass);
            return label;
        }

        private VisualElement BuildTable()
        {
            var table = new VisualElement();
            table.AddToClassList("lib-table");
            for (var i = 0; i < _styleFields.Length; i++)
            {
                table.Add(BuildRow(_styleFields[i].Style, _styleFields[i].Field));
            }
            return table;
        }

        private VisualElement BuildRow(UIAnimStyle style, string field)
        {
            var row = new VisualElement();
            row.AddToClassList("lib-row");
            var styleLabel = new Label(style.ToString());
            styleLabel.AddToClassList("lib-col-style");
            var prop = serializedObject.FindProperty(field);
            var picker = new ObjectField { objectType = typeof(UIAnimPreset) };
            picker.AddToClassList("lib-col-preset");
            picker.BindProperty(prop);
            var status = new Label();
            status.AddToClassList("lib-col-status");
            UpdateStatus(status, prop);
            if (prop != null) row.TrackPropertyValue(prop, p => UpdateStatus(status, p));
            row.Add(styleLabel);
            row.Add(picker);
            row.Add(status);
            return row;
        }

        private void UpdateStatus(Label status, SerializedProperty prop)
        {
            status.RemoveFromClassList("status-ok");
            status.RemoveFromClassList("status-fallback");
            status.RemoveFromClassList("status-missing");
            var (text, css) = ResolveStatus(prop);
            status.text = text;
            status.AddToClassList(css);
        }

        private (string Text, string Css) ResolveStatus(SerializedProperty prop)
        {
            if (prop != null && prop.objectReferenceValue != null) return ("OK", "status-ok");
            var fallback = serializedObject.FindProperty("_fallback");
            if (fallback != null && fallback.objectReferenceValue != null) return ("→ fallback", "status-fallback");
            return ("MISSING", "status-missing");
        }

        private VisualElement BuildFallbackRow()
        {
            var row = new VisualElement();
            row.AddToClassList("lib-fallback");
            var prop = serializedObject.FindProperty("_fallback");
            var label = new Label("Fallback");
            label.AddToClassList("lib-fallback-label");
            var picker = new ObjectField { objectType = typeof(UIAnimPreset) };
            picker.AddToClassList("lib-fallback-picker");
            picker.BindProperty(prop);
            row.Add(label);
            row.Add(picker);
            return row;
        }
    }
}
