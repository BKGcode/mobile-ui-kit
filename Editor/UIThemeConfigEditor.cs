using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor
{
    [CustomEditor(typeof(UIThemeConfig))]
    public class UIThemeConfigEditor : UnityEditor.Editor
    {
        private static readonly string[] _colorFieldNames =
        {
            "_primaryColor", "_secondaryColor", "_accentColor",
            "_backgroundDark", "_backgroundLight",
            "_textPrimary", "_textSecondary",
            "_successColor", "_dangerColor"
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawColorPreview();
        }

        private void DrawColorPreview()
        {
            EditorGUILayout.LabelField("Color Preview", EditorStyles.boldLabel);
            var rowRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
            DrawColorRow(rowRect);
        }

        private void DrawColorRow(Rect rect)
        {
            var swatchWidth = rect.width / _colorFieldNames.Length;
            for (var i = 0; i < _colorFieldNames.Length; i++)
            {
                var prop = serializedObject.FindProperty(_colorFieldNames[i]);
                if (prop == null) continue;
                var swatch = new Rect(rect.x + i * swatchWidth, rect.y, swatchWidth, rect.height);
                EditorGUI.DrawRect(swatch, prop.colorValue);
            }
        }
    }
}
