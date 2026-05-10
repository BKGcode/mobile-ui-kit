using System;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Test
{
    public sealed class KitforgeForceScenario
    {
        public string Label { get; }
        public string Description { get; }
        public Type ComponentType { get; }
        public KitforgeSpawnPattern Pattern { get; }
        public Action<object> ConfigureMock { get; }

        public KitforgeForceScenario(string label, string description, Type componentType, KitforgeSpawnPattern pattern, Action<object> configureMock)
        {
            Label = label;
            Description = description;
            ComponentType = componentType;
            Pattern = pattern;
            ConfigureMock = configureMock;
        }
    }
}
