using System;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IPlayerDataService
    {
        PlayerProfile GetProfile();
        void SaveProfile(PlayerProfile profile);
        void AddXp(int amount);

        event Action<PlayerProfile> OnProfileChanged;
    }
}
