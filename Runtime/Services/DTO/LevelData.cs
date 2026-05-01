namespace KitforgeLabs.MobileUIKit.Services
{
    public enum LevelState
    {
        Locked,
        Available,
        Complete
    }

    [System.Serializable]
    public struct LevelData
    {
        public int Id;
        public LevelState State;
        public int Stars;
        public int UnlockCost;
    }
}
