using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitforgeLabs.MobileUIKit.Editor.Inspectors
{
    [CustomEditor(typeof(UIThemeConfig))]
    public class UIThemeConfigEditor : UnityEditor.Editor
    {
        private const string StyleSheetPath =
            "Packages/com.kitforgelabs.mobile-ui-kit/Editor/Inspectors/UIThemeConfigEditor.uss";

        private static readonly string[] _colorFields =
        {
            "_primaryColor", "_secondaryColor", "_accentColor",
            "_backgroundDark", "_backgroundLight",
            "_textPrimary", "_textSecondary",
            "_successColor", "_dangerColor"
        };

        private static readonly string[] _spriteFields =
        {
            "_panelBackground", "_buttonPrimary", "_buttonSecondary",
            "_backdrop", "_divider",
            "_iconClose", "_iconBack", "_iconCheck", "_iconCoin", "_iconGem"
        };

        private static readonly string[] _audioFields =
        {
            "_audioButtonClick", "_audioPopupShow", "_audioPopupHide",
            "_audioSuccess", "_audioError", "_audioNotification"
        };

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null) root.styleSheets.Add(sheet);

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            root.Add(BuildSectionHeader("Color Preview"));
            root.Add(BuildColorRow());

            root.Add(BuildSectionHeader("Sprite Preview"));
            root.Add(BuildSpriteGrid());

            root.Add(BuildSectionHeader("Audio Preview"));
            root.Add(BuildAudioList());
            return root;
        }

        private static Label BuildSectionHeader(string text)
        {
            var label = new Label(text);
            label.AddToClassList("section-header");
            return label;
        }

        private VisualElement BuildColorRow()
        {
            var row = new VisualElement();
            row.AddToClassList("color-row");
            for (var i = 0; i < _colorFields.Length; i++)
            {
                row.Add(BuildColorSwatch(_colorFields[i]));
            }
            return row;
        }

        private VisualElement BuildColorSwatch(string field)
        {
            var prop = serializedObject.FindProperty(field);
            var swatch = new VisualElement();
            swatch.AddToClassList("color-swatch");
            swatch.tooltip = field.TrimStart('_');
            if (prop != null)
            {
                swatch.style.backgroundColor = prop.colorValue;
                swatch.TrackPropertyValue(prop, p => swatch.style.backgroundColor = p.colorValue);
            }
            return swatch;
        }

        private VisualElement BuildSpriteGrid()
        {
            var grid = new VisualElement();
            grid.AddToClassList("sprite-grid");
            for (var i = 0; i < _spriteFields.Length; i++)
            {
                grid.Add(BuildSpriteCell(_spriteFields[i]));
            }
            return grid;
        }

        private VisualElement BuildSpriteCell(string field)
        {
            var prop = serializedObject.FindProperty(field);
            var cell = new VisualElement();
            cell.AddToClassList("sprite-cell");
            var image = new Image();
            image.AddToClassList("sprite-image");
            UpdateSpriteImage(image, prop);
            if (prop != null) cell.TrackPropertyValue(prop, p => UpdateSpriteImage(image, p));
            var label = new Label(field.TrimStart('_'));
            label.AddToClassList("sprite-label");
            cell.Add(image);
            cell.Add(label);
            return cell;
        }

        private static void UpdateSpriteImage(Image image, SerializedProperty prop)
        {
            image.sprite = prop?.objectReferenceValue as Sprite;
        }

        private VisualElement BuildAudioList()
        {
            var list = new VisualElement();
            list.AddToClassList("audio-list");
            for (var i = 0; i < _audioFields.Length; i++)
            {
                list.Add(BuildAudioRow(_audioFields[i]));
            }
            return list;
        }

        private VisualElement BuildAudioRow(string field)
        {
            var prop = serializedObject.FindProperty(field);
            var row = new VisualElement();
            row.AddToClassList("audio-row");
            var label = new Label(field.TrimStart('_'));
            label.AddToClassList("audio-label");
            var play = new Button(() => AudioPreviewBridge.Play(prop?.objectReferenceValue as AudioClip)) { text = "Play" };
            play.AddToClassList("audio-play");
            var stop = new Button(AudioPreviewBridge.StopAll) { text = "Stop" };
            stop.AddToClassList("audio-play");
            row.Add(label);
            row.Add(play);
            row.Add(stop);
            return row;
        }
    }
}
