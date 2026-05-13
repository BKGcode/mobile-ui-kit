using System;
using System.Collections.Generic;
using System.Reflection;

namespace KitforgeLabs.UIKit.Editor.Hub.Catalog
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
            return BuildSpawnTemplate("KitforgeLabs.UIKit.Core", entry.ComponentType.Namespace, "PopupManager", "_popupManager", "Show", entry.ComponentType.Name, dataType);
        }

        private static string BuildScreenSnippet(KitforgeCatalogEntry entry)
        {
            var dataType = ResolveDataTypeName(entry);
            return BuildSpawnTemplate("KitforgeLabs.UIKit.Core", entry.ComponentType.Namespace, "UIManager", "_uiManager", "Push", entry.ComponentType.Name, dataType);
        }

        private static string BuildToastSnippet(KitforgeCatalogEntry entry)
        {
            var dataType = ResolveDataTypeName(entry);
            return BuildSpawnTemplate("KitforgeLabs.UIKit.Toast", entry.ComponentType.Namespace, "ToastManager", "_toastManager", "Show", entry.ComponentType.Name, dataType);
        }

        private static string BuildHUDSnippet(KitforgeCatalogEntry entry)
        {
            return $"// {entry.DisplayName} is a prefab — drag it under KitforgeRoot/UICanvas/ScreenRoot.\n// It auto-binds at runtime to the required services (see description above).\n// No code required.";
        }

        private static string BuildSpawnTemplate(string managerNamespace, string componentNamespace, string managerType, string managerField, string method, string componentName, string dataType)
        {
            return $"// 1. Add these usings at the top of your script:\n" +
                   $"using {managerNamespace};\n" +
                   $"using {componentNamespace};\n\n" +
                   $"// 2. Wire the manager once via Inspector or DI:\n" +
                   $"[SerializeField] private {managerType} {managerField};\n\n" +
                   $"// 3. Spawn:\n" +
                   $"{managerField}.{method}<{componentName}>(new {dataType}\n{{\n    // ...\n}});";
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
