using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Reward
{
    /// <summary>
    /// Convenience helpers that show <see cref="RewardPopup"/> instances and credit the
    /// <see cref="IEconomyService"/> when the player claims each non-sentinel reward.
    /// Item/Bundle rewards (sentinel currency <see cref="RewardPopup.ItemCurrencySentinel"/>)
    /// surface via <c>OnClaimed</c> but are NOT auto-credited — the host resolves them.
    /// <para>
    /// <see cref="GrantAndShow"/>: single reward + credit + optional <c>onClaimed</c>
    /// callback. Capability-gate re-audited Group D 2026-05-04 (`/_checker as user`):
    /// `CatalogGroupBDemo` itself repeats the equivalent pattern 3 times — shipped because
    /// silent-bug risk (forgetting the item-sentinel skip credits items as currency).
    /// </para>
    /// <para>
    /// <see cref="GrantAndShowSequence"/>: N rewards chained via <c>OnDismissed → next.Show()</c>;
    /// <c>onSequenceComplete</c> fires only after the LAST reward dismisses successfully.
    /// Validation errors (null/empty inputs) and queue-saturation early-aborts do NOT
    /// invoke it — the contract is "all rewards claimed", not "sequence attempted".
    /// Capability-gate (Group C 2026-05-04): shipped because 2 callsites in Group C
    /// elements (DailyLogin <c>OnDayClaimed</c>, LevelComplete <c>OnNextRequested</c>)
    /// repeat the same chain wiring.
    /// </para>
    /// <para>
    /// Sibling helper <c>ShopFlow.OpenWithPurchaseChain</c> remains OUT-of-scope at
    /// <c>v1.0.0-rc</c> — chain shape is opinionated (Ad-fund vs IAP-fund); buyers fork.
    /// Documented as "monetization chain pattern" recipe in M4 QUICKSTART.
    /// </para>
    /// </summary>
    public static class RewardFlow
    {
        public static void GrantAndShow(
            PopupManager popups,
            IEconomyService economy,
            RewardPopupData reward,
            Action<CurrencyType, int> onClaimed = null)
        {
            if (popups == null) { LogErrorSingle("popups argument is null. Cannot show reward."); return; }
            if (economy == null) { LogErrorSingle("IEconomyService argument is null. Cannot credit reward. See Quickstart § Service binding."); return; }
            if (reward == null) { LogErrorSingle("reward argument is null. Nothing to show."); return; }
            var popup = popups.Show<RewardPopup>(reward);
            if (popup == null) { LogErrorSingle("PopupManager.Show returned null (queue saturated). Aborting."); return; }
            popup.OnClaimed += (currency, amount) =>
            {
                if ((int)currency != RewardPopup.ItemCurrencySentinel) economy.Add(currency, amount);
                onClaimed?.Invoke(currency, amount);
            };
        }

        public static void GrantAndShowSequence(
            PopupManager popups,
            IEconomyService economy,
            IEnumerable<RewardPopupData> rewards,
            Action onSequenceComplete = null)
        {
            if (popups == null) { LogError("popups argument is null. Cannot show reward sequence."); return; }
            if (economy == null) { LogError("IEconomyService argument is null. Cannot credit rewards. See Quickstart § Service binding."); return; }
            if (rewards == null) { LogError("rewards collection is null. Nothing to show."); return; }
            var array = ToArray(rewards);
            if (array.Length == 0) { LogError("rewards collection is empty. Nothing to show."); return; }
            ShowNext(popups, economy, array, 0, onSequenceComplete);
        }

        private static void ShowNext(
            PopupManager popups,
            IEconomyService economy,
            RewardPopupData[] rewards,
            int index,
            Action onSequenceComplete)
        {
            if (index >= rewards.Length) { onSequenceComplete?.Invoke(); return; }
            var popup = popups.Show<RewardPopup>(rewards[index]);
            if (popup == null)
            {
                LogError($"PopupManager.Show returned null at index {index} (queue saturated). Aborting sequence.");
                return;
            }
            SubscribeChainHandlers(popup, economy, popups, rewards, index, onSequenceComplete);
        }

        private static void SubscribeChainHandlers(
            RewardPopup popup,
            IEconomyService economy,
            PopupManager popups,
            RewardPopupData[] rewards,
            int index,
            Action onSequenceComplete)
        {
            popup.OnClaimed += (currency, amount) =>
            {
                if ((int)currency == RewardPopup.ItemCurrencySentinel) return;
                economy.Add(currency, amount);
            };
            popup.OnDismissed += () => ShowNext(popups, economy, rewards, index + 1, onSequenceComplete);
        }

        private static RewardPopupData[] ToArray(IEnumerable<RewardPopupData> source)
        {
            if (source is RewardPopupData[] array) return array;
            var list = new List<RewardPopupData>();
            foreach (var item in source) list.Add(item);
            return list.ToArray();
        }

        private static void LogError(string body)
        {
            Debug.LogError($"RewardFlow.GrantAndShowSequence: {body}");
        }

        private static void LogErrorSingle(string body)
        {
            Debug.LogError($"RewardFlow.GrantAndShow: {body}");
        }
    }
}
