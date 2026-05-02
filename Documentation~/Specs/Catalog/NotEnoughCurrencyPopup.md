# NotEnoughCurrencyPopup

## Purpose
Modal that fires when the player attempts a purchase they can't afford. Two CTAs: "Buy more" (open shop) and "Watch ad" (rewarded ad path). Element 3/5 of Group B. The monetization-loop closer.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| N1 | CTA visibility | Both CTAs visible by default. `ShowBuyMore` / `ShowWatchAd` flags allow either to be hidden. Hiding both = "decline only" mode (rare but supported). | Mirror of ConfirmPopup's `ShowCancel` flag. |
| N2 | Ad availability check | Popup queries `IAdsService.IsRewardedAdReady()` on Bind. If false, "Watch ad" button disabled with grayed-out style. | Buyer doesn't need to gate visibility manually. Decoupled from popup logic. |
| N3 | Currency mutation responsibility | Popup does NOT call any service mutation. Emits `OnBuyMoreRequested(currency, missing)` / `OnWatchAdRequested(currency, missing)` / `OnDeclined`. Host wires the response. | Cumple MUSTN'T #1 + R3 from RewardPopup spec. |
| N4 | Auto-dismiss after CTA | Popup dismisses after either CTA event. Host receives event + dismiss callback. | Standard mobile UX — user expects this popup gone after action. |
| N5 | Default close behavior | Backdrop tap = `OnDeclined`. Back press = `OnDeclined`. Optional explicit "No thanks" button hidden by default but available via `ShowDecline=true`. | Decline path is the implicit case; no extra button needed for MVP. |

## DTO

`NotEnoughCurrencyPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Currency` | `CurrencyType` | `Coins` | Which currency the player is short on. Drives icon + tint. |
| `Required` | `int` | `0` | The price the player tried to pay. |
| `Missing` | `int` | `0` | The shortfall. Displayed prominently. |
| `Title` | `string` | `"Not enough!"` | Header. |
| `Message` | `string` | `""` | Optional body text. When empty, falls back to `"You need {Missing} more {Currency}."`. |
| `BuyMoreLabel` | `string` | `"Buy more"` | CTA label. |
| `WatchAdLabel` | `string` | `"Watch ad"` | CTA label. |
| `DeclineLabel` | `string` | `"No thanks"` | Optional decline button. |
| `ShowBuyMore` | `bool` | `true` | Hide via `false`. |
| `ShowWatchAd` | `bool` | `true` | Hide via `false`. |
| `ShowDecline` | `bool` | `false` | Optional explicit decline button. |
| `CloseOnBackdrop` | `bool` | `true` | Per N5. |

`Bind(null)` → falls back to fresh instance.

## Services consumed
- `IAdsService` (optional). Calls: `IsRewardedAdReady()` on Bind. Without service, "Watch ad" button enabled — host validates on event.
- `IUIAudioRouter` (optional). Cues: `PopupOpen` (Show), `ButtonTap` (any CTA), `PopupClose` (Hide).
- No `IEconomyService` access — popup is dumb about totals.

## Events emitted

| Event | When |
|---|---|
| `OnBuyMoreRequested(CurrencyType currency, int missing)` | Buy more click. Then popup dismisses. |
| `OnWatchAdRequested(CurrencyType currency, int missing)` | Watch ad click. Then popup dismisses. |
| `OnDeclined` | Decline button OR backdrop OR back press. Then popup dismisses. |
| `OnDismissed` | Always after hide animation. Fires AFTER above. |

All events reset on `Bind(...)`.

## Animation contract
- `[RequireComponent(typeof(UIAnimNotEnoughCurrencyPopup))]`. Auto-resolved.
- Show: scale pop-in + fade-in. Style preset = `Snappy` (urgent, attention-grabbing without celebration).
- Hide: scale-down + fade-out.
- Optional shake on Show if `Theme.WarningColor` is set — deferred, not MVP.

## Theme tokens consumed
- `WarningColor` (header tint via `Refs.HeaderTint`).
- `IconCoin` / `IconGem` (currency icon).
- `PrimaryColor` (Watch ad button — primary action).
- `SecondaryColor` (Buy more button — secondary action).
- `DefaultAnimPreset`.

## Edge cases
- **Null DTO**: defaults to `Coins`, 0 required, 0 missing. Useless visually but no NRE.
- **Both CTAs hidden**: only decline path remains. Popup degrades to "you can't afford this" alert.
- **`IAdsService` returns `IsRewardedAdReady()=false`**: Watch ad button disabled (grayed). Click on disabled button = no-op.
- **Empty `Message`**: fallback string `"You need {Missing} more {Currency}."` rendered.
- **`Currency` = sentinel out-of-range**: icon hidden, title only.
- **Re-Show same instance**: `Bind` resets events + recomputes ad button state.
- **Race conditions** (`IsDismissing` guard):
  - Double-click any CTA → second click ignored.
  - Click Buy more then Watch ad fast → only first wins.
  - Back press during hide → ignored.

## QA scenarios
1. `Show — Coins, missing 50` (default both CTAs visible).
2. `Show — Gems, missing 5` (gem icon).
3. `Show — Decline only` (`ShowBuyMore=false, ShowWatchAd=false`).
4. `Show — Ads not ready` (stub returns false) — verify Watch ad button disabled.
5. `Show — Empty message` — verify fallback string.
6. `Click — Buy more` — verify `OnBuyMoreRequested` + dismiss.
7. `Click — Watch ad` — verify `OnWatchAdRequested` + dismiss.
8. `Click — Decline (`ShowDecline=true`)` — verify `OnDeclined` + dismiss.
9. `Backdrop tap` — verify `OnDeclined` + dismiss.
10. `Back press` — verify `OnDeclined` + dismiss.
11. `Stress — Spam CTAs` — single event fires, single dismiss.

## Files
```
Runtime/Catalog/Popups/NotEnough/
├── NotEnoughCurrencyPopupData.cs
├── NotEnoughCurrencyPopup.cs
└── UIAnimNotEnoughCurrencyPopup.cs
Tests/Editor/
└── NotEnoughCurrencyPopupTests.cs
```

## Status
- Code: ⏳ pending (Group B · element 3/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~6)
- Prefab: ⏳ pending (`CatalogGroupABuilder.BuildNotEnoughCurrencyPopup`)
- Demo scene entry: ⏳ pending
