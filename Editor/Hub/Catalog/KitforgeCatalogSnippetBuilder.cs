using System;
using System.Collections.Generic;
using System.Reflection;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Catalog
{
    public static class KitforgeCatalogSnippetBuilder
    {
        public static string BuildSpawnSnippet(KitforgeCatalogEntry entry)
        {
            switch (entry.Pattern)
            {
                case KitforgeSpawnPattern.Popup: return BuildPopupSnippet(entry);
                case KitforgeSpawnPattern.Screen: return BuildScreenSnippet(entry);
                case KitforgeSpawnPattern.Toast: return BuildToastSnippet(entry);
                case KitforgeSpawnPattern.HUD: return BuildHUDSnippet(entry);
                default: return string.Empty;
            }
        }

        public static IReadOnlyList<string> BuildFieldList(KitforgeCatalogEntry entry)
        {
            var dataType = ResolveDataType(entry);
            if (dataType == null) return Array.Empty<string>();
            var fields = dataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<string>(fields.Length);
            for (var i = 0; i < fields.Length; i++)
            {
                list.Add($"{fields[i].Name} : {GetReadableTypeName(fields[i].FieldType)}");
            }
            return list;
        }

        public static Type ResolveDataType(KitforgeCatalogEntry entry) => ResolveDataTypeForComponent(entry.ComponentType);

        public static Type ResolveDataTypeForComponent(Type componentType)
        {
            var dataTypeName = componentType.Name + "Data";
            var asm = componentType.Assembly;
            foreach (var t in asm.GetTypes())
            {
                if (t.Name == dataTypeName && t.Namespace == componentType.Namespace) return t;
            }
            return null;
        }

        private static string BuildPopupSnippet(KitforgeCatalogEntry entry)
        {
            var dataType = ResolveDataTypeName(entry);
            return $"_popupManager.Show<{entry.ComponentType.Name}>(new {dataType}\n{{\n    // ...\n}});";
        }

        private static string BuildScreenSnippet(KitforgeCatalogEntry entry)
        {
            var dataType = ResolveDataTypeName(entry);
            return $"_uiManager.Push<{entry.ComponentType.Name}>(new {dataType}\n{{\n    // ...\n}});";
        }

        private static string BuildToastSnippet(KitforgeCatalogEntry entry)
        {
            var dataType = ResolveDataTypeName(entry);
            return $"_toastManager.Show<{entry.ComponentType.Name}>(new {dataType}\n{{\n    // ...\n}});";
        }

        private static string BuildHUDSnippet(KitforgeCatalogEntry entry)
        {
            return $"// {entry.DisplayName} is a prefab. Drag the catalog cell into KitforgeRoot/UICanvas/ScreenRoot.\n// It auto-binds to the required services at runtime (see description above).";
        }

        private static string ResolveDataTypeName(KitforgeCatalogEntry entry)
        {
            var resolved = ResolveDataType(entry);
            return resolved != null ? resolved.Name : entry.ComponentType.Name + "Data";
        }

        private static string GetReadableTypeName(Type t)
        {
            if (t == typeof(string)) return "string";
            if (t == typeof(int)) return "int";
            if (t == typeof(float)) return "float";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(DateTime)) return "DateTime";
            if (t.IsArray) return $"{GetReadableTypeName(t.GetElementType())}[]";
            return t.Name;
        }
    }
}
