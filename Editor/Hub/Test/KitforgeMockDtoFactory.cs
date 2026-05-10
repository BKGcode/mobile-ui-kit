using System;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Test
{
    public static class KitforgeMockDtoFactory
    {
        public static object Create(KitforgeCatalogEntry entry) => CreateForComponent(entry.ComponentType, entry.DisplayName);

        public static object CreateForComponent(Type componentType, string displayName)
        {
            var dataType = KitforgeCatalogSnippetBuilder.ResolveDataTypeForComponent(componentType);
            var data = CreateInstanceSafe(dataType, displayName);
            if (data != null) ApplySmartDefaults(data, displayName);
            return data;
        }

        private static object CreateInstanceSafe(Type dataType, string displayName)
        {
            if (dataType == null) return null;
            try
            {
                return Activator.CreateInstance(dataType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KitforgeMockDtoFactory] Failed to create mock data for {displayName}: {ex.Message}");
                return null;
            }
        }

        private static void ApplySmartDefaults(object data, string displayName)
        {
            var type = data.GetType();
            SetIfStringField(type, data, "Title", $"Test {displayName}");
            SetIfStringField(type, data, "Subtitle", $"Mock {displayName}");
            SetIfStringField(type, data, "Message", "Mock spawn — click any button to dismiss.");
        }

        private static void SetIfStringField(Type type, object data, string fieldName, string value)
        {
            var field = type.GetField(fieldName);
            if (field == null || field.FieldType != typeof(string)) return;
            field.SetValue(data, value);
        }
    }
}
