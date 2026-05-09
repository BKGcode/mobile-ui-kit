using System;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class UIKitAuditTarget
    {
        public UIKitAuditTargetKind Kind;
        public string AssetPath;
        public string Label;
        public string Group;
        public string TypeFullName;
        public string TypeShortName;
        public Type ComponentType;
        public GameObject PrefabAsset;
        public bool IsCatalog;
    }
}
