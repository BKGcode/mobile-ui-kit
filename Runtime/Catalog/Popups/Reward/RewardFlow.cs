using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Catalog.Reward
{
    /// <summary>
    /// Convenience helper that shows N <see cref="RewardPopup"/> instances in sequence —
    /// chained via <c>OnDismissed → next.Show()</c> — and credits the
    /// <see cref="IEconomyService"/> when the player claims each non-sentinel reward.
    /// Item/Bundle rewards (sentinel currency <see cref="RewardPopup.ItemCurrencySentinel"/>)
    /// surface via <c>OnClaimed</c> but are NOT auto-credited — the host resolves them.
    /// <para>
    /// <c>onSequenceComplete</c> fires only after the LAST reward dismisses successfully.
    /// Validation errors (null/empty inputs) and queue-saturation early-aborts do NOT
    /// invoke it — the contract is "all rewards claimed", not "sequence attempted".
    /// </para>
    /// <para>
    /// Capability-gate (Group C 2026-05-04): shipped because 2 callsites in Group C
    /// elements (DailyLogin <c>OnDayClaimed</c>, LevelComplete <c>OnNextRequested</c>)
    /// repeat the same chain wiring. Sibling helpers <c>RewardFlow.GrantAndShow</c>
    /// (single) and <c>ShopFlow.OpenWithPurchaseChain</c> deferred to Group D
    /// (1 callsite each — capability-gate failed).
    /// </para>
    /// </summary>
    public static class RewardFlow
    {
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
    }
}
