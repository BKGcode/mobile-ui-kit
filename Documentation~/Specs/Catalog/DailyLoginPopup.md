# DailyLoginPopup

## Purpose
Modal that announces a daily login reward streak — N-day calendar (default 7), daily reward grant, optional watch-ad-to-double, persists streak across sessions. Element 1/5 of Group C. First popup to consume `ITimeService` for day-boundary detection and `IProgressionService` for streak persistence.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| D1 | Day boundary | UTC midnight via `ITimeService.GetServerTimeUtc()`. Buyer can override `DayBoundaryHour` (default `0` = midnight) in DTO. | Server-time is the only honest source for daily systems. Wallclock invites cheating. |
| D2 | Streak break | If `LastClaimUtc` is older than `2 * day`, streak resets to day 1. If exactly 1 day older, advances. If same day, popup shows already-claimed state. | Standard mid-core mobile pattern (Coin Master / Royal Match). Forgiveness window optional via `MaxStreakGapDays` (default `1`). |
| D3 | Calendar size | `RewardEntries: DailyLoginRewardEntry[]` — buyer-defined length (typical 7-30). Last entry = "big reward" (`IsBigReward = true` styling). | Avoids hardcoded 7-day. Matches what the kit's specs lock: data-driven, no magic numbers. |
| D4 | Rewards data source | DTO carries the array directly. Buyer optionally swaps via `IProgressionService.GetDailyLoginConfig()` if they want server-side config — popup is agnostic. | Decoupling. Popup shows what it's bound. Source-of-truth is buyer's concern. |
| D5 | Watch-to-double | Optional per-day flag `AllowDouble` on each entry. When `true` AND `IAdsService.IsRewardedAdReady()=true` AND **`DailyLoginState.DoubledToday=false`** (per FQ3), popup shows secondary "Watch ad to double" CTA. **Host credits 2× total post-ad — popup does NOT credit base on the doubled path.** Player taps "Watch ad to double" → popup emits `OnWatchAdRequested(day, rewards)` and dismisses (no `OnDayClaimed` fires). Host runs ad → on success calls `RewardFlow.GrantAndShow(currency, amount * 2)` once + sets `DoubledToday=true` in `IProgressionService` impl. Player taps regular "Claim" instead → popup emits `OnDayClaimed` (base path); host credits 1× via `RewardFlow.GrantAndShow(currency, amount)`. The two paths are mutually exclusive. **Already-claimed-today + AllowDouble + DoubledToday=false (FQ3)**: Watch-to-double CTA still visible in read-only D7 state (lets player retroactively double up to ad cap). Already-claimed + DoubledToday=true → CTA hidden. | Single credit-call per claim. No "bonus half" arithmetic. Buyer wires one branch, not two. Persisted state prevents re-doubling exploit. |
| D6 | Auto-trigger | Popup ships with `DailyLoginFlow.ShowIfDue(popups, services, config)` static helper. Host calls on app launch (Splash/MainMenu controller). Returns `bool` (shown/skipped). Helper queries `IProgressionService.GetDailyLoginState()` to decide. | Keeps auto-trigger logic out of popup itself (MUSTN'T #7 — no self-registration). Host owns when. |
| D7 | Already-claimed state | Popup opens in read-only state showing streak progress + countdown to next reward. Claim button replaced by disabled "Come back in HH:MM:SS" label that ticks via `ITimeService`. **Auto-transition (FQ1)**: when countdown crosses zero, popup re-queries `IProgressionService.GetDailyLoginState()` once; if `AlreadyClaimedToday=false`, advances to claim-ready state inline (no close/re-open). | Better UX than popup refusing to open — the player sees progress earned. Live transition handles the rare midnight-while-popup-open case without buyer wiring. |
| D8 | Multi-reward day | A single day's reward can be a `RewardPopupData[]` (e.g. 100 coins + 1 gem). Popup claim emits `OnDayClaimed(day, rewards[])`. Host wires to `RewardFlow.GrantAndShowSequence(rewards)`. | Reuses Group C helper. No new sequencing concept. |

## DTO

`DailyLoginPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Daily Reward"` | Header label. |
| `RewardEntries` | `DailyLoginRewardEntry[]` | `null` | Calendar entries, 1-based day index = array index + 1. |
| `CurrentDay` | `int` | `1` | 1-based day in streak. Driven by `IProgressionService` snapshot. |
| `LastClaimUtc` | `DateTime` | `default` | Last successful claim time (for already-claimed/cooldown calc). |
| `AlreadyClaimedToday` | `bool` | `false` | When `true`, shows read-only state per D7. |
| `MaxStreakGapDays` | `int` | `1` | Forgiveness window per D2. |
| `ClaimLabel` | `string` | `"Claim"` | Primary CTA label. |
| `WatchToDoubleLabel` | `string` | `"Watch ad to double"` | Secondary CTA when `AllowDouble && IsRewardedAdReady`. |
| `CloseOnBackdrop` | `bool` | `false` | Default blocker — must claim or explicitly close. |

`DailyLoginRewardEntry` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Rewards` | `RewardPopupData[]` | `null` | One or more rewards granted on claim. Multi-element triggers sequence. |
| `IsBigReward` | `bool` | `false` | Visual emphasis (larger cell, glow). |
| `AllowDouble` | `bool` | `false` | Per D5. |
| `Label` | `string` | `""` | Optional override for cell label (default = `"Day {N}"`). |

`Bind(null)` → falls back to fresh `DailyLoginPopupData()` with empty `RewardEntries`. Popup logs warning and closes immediately if `RewardEntries.Length == 0`.

## Service binding
Null-service behavior follows the "silent degrade" rule.

- `ITimeService` (REQUIRED). Reads `GetServerTimeUtc()` on Bind for cooldown countdown. Subscribes via internal `Update()` ticking once per second when popup is shown in already-claimed state (D7).
- `IAdsService` (optional). `IsRewardedAdReady()` queried on Bind to gate watch-to-double CTA visibility.
- `IProgressionService` (queried by `DailyLoginFlow.ShowIfDue`, NOT by popup). Methods: `GetDailyLoginState()` returns `(currentDay, lastClaimUtc, alreadyClaimedToday, doubledToday, maxStreakGapDays)`.
- `IUIAudioRouter` (optional). Cues: `PopupOpen` (Show), `Success` (Claim), `PopupClose` (Hide).
- No `IEconomyService` write access (matches RewardPopup R3).

## Events emitted

| Event | When |
|---|---|
| `OnDayClaimed(int day, RewardPopupData[] rewards)` | Claim button click. Host wires to `RewardFlow.GrantAndShowSequence(rewards)`. |
| `OnWatchAdRequested(int day, RewardPopupData[] rewards)` | Watch-to-double click. Mutually exclusive with `OnDayClaimed`. Host calls `IAdsService.ShowRewardedAd()` then on success calls `RewardFlow.GrantAndShow` with **2× amount** per D5. |
| `OnDismissed` | Always after hide animation completes. Fires AFTER claim/ad events. |

All events reset on `Bind(...)`.

## Animation contract
- `[RequireComponent(typeof(UIAnimDailyLoginPopup))]`. Auto-resolved.
- Show: scale pop-in + fade-in on `_card`. Style preset = `Bouncy` (celebration).
- Day cells stagger-cascade in (0.05s per cell) via `DOPunchScale` on `_dayCellContainer` children. Big-reward cell extra glow tween.
- Hide: scale-down + fade-out, then `FinalizeDismissal`.
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`.

## Theme tokens consumed
- `IconCoin` / `IconGem` / `IconEnergy` (per-cell currency icon).
- `SuccessColor` (claim button tint, current day cell highlight).
- `WarningColor` (countdown timer text in already-claimed state).
- `DefaultAnimPreset`.
- `FontHeader` / `FontBody` (typography via `ThemedText`).

## Edge cases
- **Null `RewardEntries`**: popup logs error, closes immediately. Host responsibility to bind valid data.
- **`CurrentDay > RewardEntries.Length`**: popup wraps via modulo (`(currentDay - 1) % length + 1`). Logs warning. Buyer should design data for streak loops.
- **Already-claimed today + countdown ticks past midnight**: popup auto-refreshes — closes and re-opens via `DailyLoginFlow.ShowIfDue` if buyer wires the `OnDismissed` to retry. Default: stays in read-only until manual dismiss.
- **`ITimeService.GetServerTimeUtc()` returns past time**: countdown shows `00:00:00`, claim re-enables. Trust the service.
- **Re-Show same instance**: `Bind` resets events + recomputes state.
- **Race conditions** (`IsDismissing` guard): double-claim, claim-during-ad, back-press-during-hide all guarded.
- **`AllowDouble = true` but `IAdsService = null` or `IsRewardedAdReady = false`**: secondary CTA hidden silently. No error.
- **Sequence interrupted** (host crash mid-`GrantAndShowSequence`): popup already dismissed; recovery is buyer's `IProgressionService` job.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — Day 1, fresh streak` (default 7-entry config).
2. `Show — Day 4, mid-streak`.
3. `Show — Day 7, big reward` (last cell glow).
4. `Show — Already claimed today` (read-only, ticking countdown).
5. `Show — Multi-reward day` (Day 5 = 100 coins + 1 gem, sequence on claim).
6. `Show — Watch-to-double available` (`AllowDouble=true`, ad ready).
7. `Show — Watch-to-double blocked` (ad not ready).
8. `Click — Claim` (verify `OnDayClaimed` + dismiss).
9. `Click — Watch ad to double` (verify `OnWatchAdRequested` + dismiss).
10. `Backdrop tap — CloseOnBackdrop=false` (verify NO dismiss).
11. `Stress — Spam claim 10x` (single dismiss).
12. `Auto-trigger — DailyLoginFlow.ShowIfDue` (yesterday claim, today opens; same-day claim, skips).
13. `DTO defaults assertion (FD3)` — `var data = new DailyLoginPopupData(); Assert.AreEqual("Daily Reward", data.Title); Assert.AreEqual(1, data.CurrentDay); Assert.AreEqual(false, data.AlreadyClaimedToday); Assert.AreEqual(1, data.MaxStreakGapDays); Assert.AreEqual("Claim", data.ClaimLabel); Assert.AreEqual(false, data.CloseOnBackdrop);`. Per `feedback_workflow.md` L3 — DTO defaults are part of the buyer-facing contract.
14. `Idempotency (FQ2)` — `DailyLoginFlow.ShowIfDue` called twice in same launch with `due=true` → second call returns `false` and does NOT show.
15. `Auto-transition (FQ1)` — open popup in already-claimed state; tick `ITimeService` past midnight; verify popup transitions to claim-ready inline without close/re-open.
16. `Already-claimed + Watch-to-double available (FQ3)` — `AlreadyClaimedToday=true`, `DoubledToday=false`, `AllowDouble=true`, ad ready → secondary CTA visible; click fires `OnWatchAdRequested`.
17. `Show — Null ITimeService logs error and aborts (§ 4.5)` — bind popup with `Services.Time = null`; verify `Debug.LogError` with format `"DailyLoginPopup: ITimeService not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding."` and `OnShow` aborts (popup remains closed, queue advances).
18. `DailyLoginFlow — Null IProgressionService logs error and returns false (§ 4.5)` — call `DailyLoginFlow.ShowIfDue` with `Services.Progression = null`; verify `Debug.LogError` actionable message and helper returns `false` without invoking `popups.Show`.

## Convenience helper

`DailyLoginFlow` static class — auto-trigger orchestration per D6.

```csharp
// Runtime/Catalog/Popups/DailyLogin/DailyLoginFlow.cs
public static class DailyLoginFlow
{
    // Returns true if the popup was shown, false if skipped.
    // Call on app launch (Splash/MainMenu controller).
    public static bool ShowIfDue(
        PopupManager popups,
        UIServices services,
        DailyLoginPopupData configTemplate,
        Action<int, RewardPopupData[]> onClaimed = null);
}
```

Helper queries `IProgressionService.GetDailyLoginState()`, populates `CurrentDay`/`LastClaimUtc`/`AlreadyClaimedToday` on a clone of `configTemplate`, then calls `popups.Show<DailyLoginPopup>(data)` if not already-claimed today (or if `D7` opted-in to read-only display).

**Idempotency (FQ2)**: `DailyLoginFlow` holds an internal `_shownThisLaunch: bool` flag. First `ShowIfDue` call with `due=true` sets flag and returns `true`. Subsequent calls in the same app launch return `false` without showing — even if `due` is still `true`. Flag resets on next app launch (static state lives on the static class; if buyer needs explicit reset for testing, expose `_internal_ResetForTests()`).

## Pre-flight infrastructure
Required infrastructure: `IProgressionService.GetDailyLoginState()` extension is required before delivery.

## Files
```
Runtime/Catalog/Popups/DailyLogin/
├── DailyLoginPopupData.cs
├── DailyLoginRewardEntry.cs
├── DailyLoginPopup.cs
├── DailyLoginFlow.cs
└── UIAnimDailyLoginPopup.cs
Tests/Editor/
└── DailyLoginPopupTests.cs
└── DailyLoginFlowTests.cs
```

## Status
- Code: ✅ `DailyLoginPopupData`, `DailyLoginRewardEntry`, `DailyLoginPopup`, `DailyLoginFlow`, `UIAnimDailyLoginPopup`
- Spec: ✅ this document (§ 4.5 citation applied)
- Tests: ✅ 25 EditMode tests (18 popup + 7 flow) — exceed target of 13
- Prefab: ⏳ pending (`CatalogGroupCBuilder.BuildDailyLoginPopup`)
- Demo scene entry: ⏳ pending
