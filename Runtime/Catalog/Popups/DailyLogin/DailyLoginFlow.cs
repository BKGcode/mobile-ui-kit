using System;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.DailyLogin
{
    public static class DailyLoginFlow
    {
        private static bool _shownThisLaunch;

        public static bool ShowIfDue(
            PopupManager popups,
            UIServices services,
            DailyLoginPopupData configTemplate,
            Action<int, RewardPopupData[]> onClaimed = null)
        {
            if (!ValidateInputs(popups, services, configTemplate)) return false;
            if (_shownThisLaunch) return false;
            var state = services.Progression.GetDailyLoginState();
            if (state.AlreadyClaimedToday) return false;
            var data = BuildData(configTemplate, state);
            var popup = popups.Show<DailyLoginPopup>(data);
            if (popup == null) return false;
            if (onClaimed != null) popup.OnDayClaimed += onClaimed;
            _shownThisLaunch = true;
            return true;
        }

        internal static bool ShownThisLaunchForTests => _shownThisLaunch;
        internal static void ResetForTests() => _shownThisLaunch = false;
        internal static void ForceShownForTests() => _shownThisLaunch = true;

        private static bool ValidateInputs(PopupManager popups, UIServices services, DailyLoginPopupData template)
        {
            if (popups == null) { LogServiceMissing("PopupManager"); return false; }
            if (services == null || services.Progression == null) { LogServiceMissing("IProgressionService"); return false; }
            if (template == null) { Debug.LogError("DailyLoginFlow.ShowIfDue: configTemplate is null."); return false; }
            return true;
        }

        private static void LogServiceMissing(string serviceName)
        {
            Debug.LogError($"DailyLoginFlow.ShowIfDue: {serviceName} not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding.");
        }

        private static DailyLoginPopupData BuildData(DailyLoginPopupData src, DailyLoginState state)
        {
            var data = Clone(src);
            data.CurrentDay = state.CurrentDay;
            data.LastClaimUtc = state.LastClaimUtc;
            data.AlreadyClaimedToday = state.AlreadyClaimedToday;
            data.DoubledToday = state.DoubledToday;
            if (state.MaxStreakGapDays > 0) data.MaxStreakGapDays = state.MaxStreakGapDays;
            return data;
        }

        private static DailyLoginPopupData Clone(DailyLoginPopupData src)
        {
            return new DailyLoginPopupData
            {
                Title = src.Title,
                RewardEntries = src.RewardEntries,
                CurrentDay = src.CurrentDay,
                LastClaimUtc = src.LastClaimUtc,
                AlreadyClaimedToday = src.AlreadyClaimedToday,
                DoubledToday = src.DoubledToday,
                MaxStreakGapDays = src.MaxStreakGapDays,
                ClaimLabel = src.ClaimLabel,
                WatchToDoubleLabel = src.WatchToDoubleLabel,
                CloseOnBackdrop = src.CloseOnBackdrop,
            };
        }
    }
}
