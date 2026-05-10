using System;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Catalog
{
    public enum KitforgeSpawnPattern
    {
        Popup,
        Toast,
        Screen,
        HUD,
    }

    public sealed class KitforgeCatalogEntry
    {
        public Type ComponentType { get; }
        public string DisplayName { get; }
        public string Group { get; }
        public KitforgeSpawnPattern Pattern { get; }
        public string Description { get; }

        public KitforgeCatalogEntry(Type componentType, string displayName, string group, KitforgeSpawnPattern pattern, string description)
        {
            ComponentType = componentType;
            DisplayName = displayName;
            Group = group;
            Pattern = pattern;
            Description = description;
        }

        public string Key => $"{ComponentType.FullName}|{DisplayName}";
    }
}
