using System;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Reward
{
    [Serializable]
    public class RewardPopupData
    {
        public string Title = "Reward!";
        public RewardKind Kind = RewardKind.Coins;
        public int Amount;
        public string ItemId = string.Empty;
        public Sprite IconOverride;
        public string[] BundleLines;
        public string ClaimLabel = "Claim";
        public float AutoClaimSeconds;
        public bool CloseOnBackdrop;
    }
}
