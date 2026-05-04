using System;
using System.Globalization;

namespace KitforgeLabs.MobileUIKit.Services
{
    public static class DailyLoginPersistence
    {
        public const string KeyCurrentDay = "kfmui.dailylogin.currentDay";
        public const string KeyLastClaimUtc = "kfmui.dailylogin.lastClaimUtc";
        public const string KeyDoubledToday = "kfmui.dailylogin.doubledToday";

        public static void Load(IPlayerDataService pd, ref DailyLoginState state)
        {
            if (pd == null) return;
            state.CurrentDay = pd.GetInt(KeyCurrentDay, state.CurrentDay);
            state.LastClaimUtc = ParseIsoOrDefault(pd.GetString(KeyLastClaimUtc, ""));
            state.DoubledToday = pd.GetBool(KeyDoubledToday, state.DoubledToday);
        }

        public static void Save(IPlayerDataService pd, DailyLoginState state)
        {
            if (pd == null) return;
            pd.SetInt(KeyCurrentDay, state.CurrentDay);
            pd.SetString(KeyLastClaimUtc, FormatIso(state.LastClaimUtc));
            pd.SetBool(KeyDoubledToday, state.DoubledToday);
        }

        private static DateTime ParseIsoOrDefault(string iso)
        {
            if (string.IsNullOrEmpty(iso)) return default;
            if (DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)) return dt;
            return default;
        }

        private static string FormatIso(DateTime dt)
        {
            return dt == default ? "" : dt.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
