# HUD-Energy

## Purpose
Live energy counter — same lifecycle as HUDCoins/HUDGems (now `HUDCurrency` per Phase 0 lock), PLUS regen-tick countdown ("+1 in 04:32") and explicit max-cap display ("3/5"). Element 4/5 of Group C. Validates that the HUDCurrency parameterized base extends without bloating Coins/Gems.

## Architectural reconciliation (vs Phase 0)

Phase 0 EQ4 locked `HUDCurrency` as a single parameterized class replacing `HUDCoins`/`HUDGems`. Energy adds **two UX concerns** Coins/Gems don't have:

1. **Regen countdown** — "next +1 in HH:MM:SS" ticking label.
2. **Max-cap display** — "3/5" instead of "3" (bounded resource).

Bloating `HUDCurrency` with optional `_showRegen` / `_showCap` Inspector fields infects Coins/Gems prefabs with unused UI. **Decision E1 below** is the resolution. Recommended: `HUDEnergy : HUDCurrency` subclass.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| **E1** | HUDEnergy class strategy | **LOCKED — Option A**: `HUDEnergy : HUDCurrency` subclass. `_currency` defaulted to `Energy` and not Inspector-editable (override the field via constructor or hide via custom drawer / `[HideInInspector]`). Adds regen + cap UX as new fields. Coins/Gems prefabs use plain `HUDCurrency`. (Option B = bloat HUDCurrency, rejected. Option C = composability via 2 components, rejected — doubles buyer wiring.) | Capability-gate winner: zero bloat in Coins/Gems, regen logic isolated, single hierarchy, one prefab one component. |
| E2 | Regen source | `IEconomyService` does NOT model regen (kept plain per Phase 0 EQ6). Regen is a **buyer-side game system** that periodically calls `economy.Add(Energy, 1)`. HUD subscribes to `OnChanged(CurrencyType, int)` for the value, AND to `IProgressionService.GetEnergyRegenState()` for the countdown clock. **Subscribe override semantics (FQ5)**: `HUDEnergy.Subscribe()` calls `base.Subscribe()` (which subscribes to `OnChanged` filtered by `_currency = Energy`) THEN registers the 1Hz polling tick for regen state. `HUDEnergy.Unsubscribe()` reverses both in opposite order (kill polling, then `base.Unsubscribe()`). Filter logic stays in base — subclass adds, never duplicates. | Decoupling. The kit doesn't own regen rules (cap, rate, refill cost, ad-skip). Buyer's `IProgressionService` exposes the snapshot the HUD needs. Subscribe layering keeps HUDCurrency simple. |
| E3 | `IProgressionService.GetEnergyRegenState()` contract | Returns struct `EnergyRegenState { int Current, int Max, DateTime NextRegenUtc, bool IsFull }`. Polled on Bind + on every `OnChanged` event + every second when popup is visible (countdown tick). | Snapshot pattern, not push. Polling 1Hz is cheap, simpler than event subscription for a clock. |
| E4 | Cap display format | Default `"{Current}/{Max}"` (e.g. `"3/5"`). Buyer overrides via `_capFormatString` with placeholders `{0}=current, {1}=max`. When `Max <= 0` or `IsFull`, cap label shown without slash (`"3"`). | Plain string format. No new dependency. |
| E5 | Regen countdown format | Default `"+1 in {0}"` (single placeholder; `{0}` is replaced with HH:MM:SS computed from `state.NextRegenUtc - Time.GetServerTimeUtc()`). Buyer overrides via `_regenFormatString`. When `IsFull`, regen label hidden entirely. When `NextRegenUtc <= now`, label shows `_regenReadyText` (default `"+1 ready"`). | Standard mobile pattern. Single-placeholder format keeps `string.Format` simple and locale-friendly; HH:MM:SS rendering is fixed (matches DailyLogin countdown precedent). |
| E6 | Click behavior | Inherits the agnostic `_onClickEvent` UnityEvent from `HUDCurrency` base — no rename in HUDEnergy (the rename suggestion was a spec drift; HUDCurrency post-FD4 has a single agnostic `_onClickEvent` field). Default unwired. Buyer typically wires to `popups.Show<ShopPopup>(category=Energy)` or to `popups.Show<NotEnoughCurrencyPopup>(currency=Energy, missing=1)`. | Decoupling. Buyer wires the chain. Reusing the base's agnostic event keeps the click contract identical across Coins/Gems/Energy HUDs — buyer doesn't learn 3 different field names. |
| E7 | At-max animation | When `Current` reaches `Max` from below, optional flash tween on cap label using `Theme.SuccessColor`. Tunable via `_atMaxFlashDuration` (default `0.3f`). | Juicy feedback for "fully restored". Inspector-toggleable. |
| E8 | Empty (zero) animation | When `Current` reaches `0`, optional pulse tween using `Theme.WarningColor` to nudge "go refill". Tunable. | Optional juicy feedback. Off by default to avoid annoying the player. |

## DTO
**No DTO**. HUD elements are Inspector + Theme + service-driven, NOT modules with `Bind/OnShow/OnHide`. Same as HUDCoins/HUDGems (now HUDCurrency).

## Service binding
Null-service behavior follows `CATALOG_GroupC_DELTA.md` § 4.5 (HUD = silent degrade — see Edge cases for per-service fallback).

- `IEconomyService` (REQUIRED via `_services` ref). Subscribes to `OnChanged(CurrencyType, int)` filtered to `Energy`. Reads `Get(CurrencyType.Energy)` in `Refresh()`.
- `IProgressionService` (REQUIRED via `_services` ref — new requirement vs Coins/Gems). Polled per E3.
- `ITimeService` (REQUIRED via `_services` ref). Used for countdown computation (`NextRegenUtc - GetServerTimeUtc()`).
- No `IUIAudioRouter` (HUD silent, like Coins/Gems).

## Events emitted

| Event | When |
|---|---|
| `_onEnergyClickEvent` (UnityEvent) | `Refs.ClickButton` non-null and clicked. Per E6. |

No C# events.

## Animation contract
- NO `IUIAnimator` component (HUD lifecycle, no Show/Hide).
- Inline DOTween punch on counter label (inherited from `HUDCurrency` base).
- Additional inline tweens per E7/E8 (at-max flash, empty pulse) stored in `_capFlashTween` field, killed before re-fire, `SetLink(gameObject)`.
- Regen countdown updates via `Update()` (1Hz throttle — recompute every `Time.unscaledTime - _lastTickTime > 1f`).

## Theme tokens consumed
- `IconEnergy` (NEW Theme slot — added in Group C; lightning bolt sprite via `ThemedImage`).
- `TextPrimary` (current value — `ThemedText`).
- `TextSecondary` (cap "/5" suffix — `ThemedText`).
- `WarningColor` (regen countdown text + empty pulse).
- `SuccessColor` (at-max flash).
- `FontBody` (counter font via `ThemedText`).

## Edge cases
- **`_services` null**: error log + label `"--/--"`. No subscription. Inherits HUDCurrency null-handling.
- **`IProgressionService` null**: regen + cap labels hidden, falls back to plain `HUDCurrency` display (just current value).
- **`ITimeService` null**: regen countdown freezes at last computed value, logs error once. Cap still updates.
- **`NextRegenUtc` in the past**: label shows `"+1 ready"` until next `OnChanged` event arrives (buyer's regen system should fire `economy.Add` to reset the clock).
- **`Max <= 0`**: cap suffix hidden, displays current only. Logs warning.
- **`Current > Max`** (overcap from temporary buffs): displays as-is (e.g. `"7/5"`). Cap label tinted with `Theme.AccentColor` (or fallback to `TextPrimary`). No clamp — host's truth.
- **Energy goes from 5/5 → 4/5**: at-max flash NOT replayed. Flash only fires on `Current == Max && previousCurrent < Max`.
- **Energy goes from 1 → 0**: empty pulse fires once. Subsequent reads at 0 do NOT re-fire.
- **Service swapped at runtime**: same as HUDCoins — buyer must Disable + Enable. Documented limitation.
- **Disable while tweens active**: `OnDisable` kills all stored tweens (punch, capFlash, emptyPulse).

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Initial value — 3/5, regen in 04:30` (default state).
2. `Initial value — 5/5, full` (regen label hidden).
3. `Initial value — 0/5, regen in 04:30` (empty pulse fires).
4. `Add energy — 3 → 4` (verify counter punch + regen clock unchanged).
5. `Add energy — 4 → 5` (verify at-max flash fires).
6. `Spend energy — 5 → 4` (verify counter + regen clock starts).
7. `Spend energy — 1 → 0` (verify empty pulse).
8. `Tick — 1 second` (verify regen label updates).
9. `Tick — until ready` (verify `"+1 ready"` label).
10. `Click HUD` (verify UnityEvent fires).
11. `Disable + re-enable` (verify subscription survives).
12. `Null IProgressionService` (verify regen/cap labels hidden, plain counter remains).
13. `Overcap — 7/5` (verify display + tint).

## Pre-flight infrastructure
See `CATALOG_GroupC_DELTA.md` — `IProgressionService.GetEnergyRegenState()` extension, `EnergyRegenState` struct, `IconEnergy` Theme slot, `CurrencyType.Energy` enum value, and `HUDCurrency` parameterized base class are all required before delivery.

## Files
```
Runtime/Catalog/HUD/
└── HUDEnergy.cs            ← subclasses HUDCurrency per E1
Runtime/Services/
└── IProgressionService.cs  ← extended with GetEnergyRegenState() + EnergyRegenState struct (struct lives in same file, not a separate .cs — Phase 0 decision)
Tests/Editor/
└── HUDEnergyTests.cs
```

(`HUDCurrency.cs` itself is delivered alongside Q1 v2 migration — see Phase 0 lock.)

## Status
- Code: ✅ delivered 2026-05-03 — `Runtime/Catalog/HUD/HUDEnergy.cs` extends base with `EnergyRefs` (RegenCountdownLabel + MaxCapLabel + EnergyBarFill), `_capFormatString`/`_regenFormatString`/`_regenReadyText`, at-max flash (`_atMaxFlashColor`/`_atMaxFlashDuration`/`_atMaxFlashEnabled`), 1Hz `IProgressionService.GetEnergyRegenState()` poll via `Update() → OnUpdate() → TickRegenPoll()` (anchor `// OnUpdate-workaround-M3-sweep` per mejora F), Subscribe/Unsubscribe override registers auxiliary `HandleEconomyChangedForRegen` for spec E3 "polled on every OnChanged event". Empty pulse (E8) and overcap tint deferred to M3 polish.
- Spec: ✅ this document
- Tests: ✅ delivered 2026-05-03 — `Tests/Editor/HUDEnergyTests.cs` 10 tests covering initial state read / null-progression silent degrade (§ 4.5) / cap suffix bounded / cap hidden when IsFull / cap hidden when Max≤0 / regen "+1 ready" when overdue / regen hidden when IsFull / bar fill amount / E1 sealing (Inspector field ignored) / OnChanged(Energy) triggers regen poll. **189/189 tests verde sobre fresh compile.**
- Prefab: ⏳ pending (`CatalogGroupCBuilder.BuildHUDEnergy` — preset with `_currency=Energy` sealed)
- Demo scene entry: ⏳ pending (HUD lives on persistent canvas, not spawned by ContextMenu)
