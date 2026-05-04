# Group C — Pre-flight infrastructure delta

> Cross-cutting changes Group C delivery requires BEFORE building the 5 catalog elements. Without these, first compile after migration to v0.6.0-alpha will fail with "method not found" / "Theme slot missing" errors.

## Why this doc

Each Group C spec (DailyLogin, LevelComplete, GameOver, HUD-Energy, HUD-Timer) references services and Theme slots that do not yet exist. Scattering the deltas across specs hides the consolidated work. This doc is the single source.

Order of work in next delivery session: **Theme slots → IProgressionService extension → IEconomyService v2 migration → HUDCurrency base → element specs → builder.**

---

## 1 — `UIThemeConfig` slot additions

| Slot | Type | Used by | Default suggestion |
|---|---|---|---|
| `IconEnergy` | `Sprite` | HUD-Energy, DailyLogin (per-day reward icon), GameOver (Continue Currency icon when `ContinueCurrency = Energy`) | Lightning bolt outline (placeholder) |
| `IconClock` | `Sprite` | HUD-Timer (optional left-of-label icon) | Clock face outline (placeholder) |
| `StarFilledSprite` | `Sprite` | LevelComplete (3-star reveal) | Solid yellow star |
| `StarEmptySprite` | `Sprite` | LevelComplete (empty star slots) | Outline star, dimmed |
| `FailureColor` | `Color` | GameOver (header tint, particle accents), HUD-Timer (`_expiryStyle = Failure`) | `#E53935` (red 600) |

**Acceptance**: `UIThemeConfig.cs` declares all 5 fields. Bootstrap Defaults populates with placeholder sprites/colors. Existing Theme assets (Playful) get the new slots auto-populated by the Bootstrap upgrade path (idempotent — does not overwrite if already set).

**Capability-gate axis 1 (capabilities preserved)**: ✓ all existing Theme tokens unchanged.

---

## 2 — `IProgressionService` extension

Current `IProgressionService` (Group B state, `Runtime/Services/IProgressionService.cs`) does NOT include daily-login or energy-regen surface. Group C adds two methods + two value-types.

### Proposed signature

```csharp
namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IProgressionService
    {
        // ... existing members preserved ...

        DailyLoginState GetDailyLoginState();
        EnergyRegenState GetEnergyRegenState();
    }

    [Serializable]
    public struct DailyLoginState
    {
        public int CurrentDay;            // 1-based streak day
        public DateTime LastClaimUtc;     // UTC timestamp of last successful claim
        public bool AlreadyClaimedToday;  // computed by impl using ITimeService boundary
        public bool DoubledToday;         // FQ3 — true once player consumed the watch-to-double path for today's reward
        public int MaxStreakGapDays;      // forgiveness window before streak resets
    }

    [Serializable]
    public struct EnergyRegenState
    {
        public int Current;            // current energy value (mirror of IEconomyService.Get(Energy))
        public int Max;                // configured cap
        public DateTime NextRegenUtc;  // when next +1 ticks (or DateTime.MinValue if full)
        public bool IsFull;            // Current >= Max
    }
}
```

### Stub impact — `InMemoryProgressionService` SHIPPING

Group B precedent (verified 2026-05-02 against repo): 3 in-memory stubs ship in `Samples~/Catalog_GroupB_Currency/Stubs/` (`InMemoryEconomyService` / `InMemoryShopDataProvider` / `InMemoryAdsService`). They live SAMPLE-side, not Runtime, so Runtime asmdef stays free of demo-only code. The Group B builder (`Editor/Generators/CatalogGroupBBuilder.cs`) instantiates them on the demo `UIServices` GameObject when `Build Group B Sample` runs. Group C MUST mirror this: ship `InMemoryProgressionService` in `Samples~/Catalog_GroupC_Progression/Stubs/` and have `CatalogGroupCBuilder` wire it into the demo. Buyer who imports the Group C sample gets a zero-config demo; buyer building from scratch wires their own service (same expectation as Group B).

**File:** `Samples~/Catalog_GroupC_Progression/Stubs/InMemoryProgressionService.cs`
**Asmdef:** sample-local (no Runtime dependency in reverse direction; sample asmdef references Runtime as Group B does)

Required implementation:

- Implements `IProgressionService` in full (existing members + new `GetDailyLoginState` / `GetEnergyRegenState`).
- `[SerializeField] private DailyLoginState _dailyLoginState` — Inspector-editable for demo.
- `[SerializeField] private EnergyRegenState _energyRegenState` — Inspector-editable, plus `[SerializeField] private float _regenSeconds = 60f`.
- `[ContextMenu]` triggers: "Reset Daily To Day 1", "Set Daily To Day 7 Ready", "Mark Today Claimed", "Set Energy To Zero", "Refill Energy".
- Update loop ticks `_energyRegenState.NextRegenUtc`; on regen tick calls `_economy.Add(CurrencyType.Energy, 1)` via injected `IEconomyService` reference (settable via `SetEconomyForTests` for editor tests).
- `[DefaultExecutionOrder(-100)]` — same lesson as `UIServices` (L4): Awake-before-OnEnable for HUDs that read state in their `OnEnable`.
- Defaults: day 1, never claimed, energy = max — "fresh first run".

**Builder wiring (`Build Group C Sample`):** the builder MUST instantiate `InMemoryProgressionService` on a child of the demo `UIServices` GameObject and assign it to the `Progression` slot. Same pattern Group B used for `FakeEconomy` / `FakeShopData` / `FakeAds`.

**Test fakes** in `Tests/Editor/Helpers/` extend the existing fake hierarchy with `FakeProgressionService` (mirrors stub but without Update loop — tests advance state explicitly).

### Acceptance

- `IProgressionService.cs` compiles with new methods.
- `Runtime/Services/Stubs/InMemoryProgressionService.cs` exists, implements interface in full, has `[DefaultExecutionOrder(-100)]`.
- `Build Group C Sample` builder wires the stub into the demo scene's `UIServices.Progression` slot.
- `Tests/Editor/Helpers/FakeProgressionService.cs` exists for unit tests.
- DailyLogin spec D6/D7 + HUD-Energy spec E2/E3 reference these signatures verbatim.
- Zero-config "import + open Build Group C Sample + press Play" runs end-to-end without buyer code.

---

## 2.5 — `HUDCurrency` event filtering (FD4)

`HUDCurrency` parameterized base subscribes to `IEconomyService.OnChanged(CurrencyType currency, int newValue)` and MUST filter inside the handler:

```csharp
private void HandleEconomyChanged(CurrencyType currency, int newValue)
{
    if (currency != _currency) return;            // foreign currency — ignore silently
    // proceed with ApplyValue(newValue, animate: true)
}
```

The sentinel `(CurrencyType)(-1)` used by `RewardPopup` for `Item`/`Bundle` reward kinds (Phase 0 EQ2) is naturally filtered out — no `HUDCurrency` instance has `_currency = -1`, so the equality check rejects it without special-casing. Same applies to any future enum extension (e.g. `Tickets = 3`) — only HUDs configured for that currency will react. No global event multiplexing required.

This contract MUST be documented in `HUDCurrency.cs` class XML doc + included in `HUDCurrency`'s currency-filter test (single test asserting `OnChanged(otherCurrency, _)` does NOT trigger a punch tween). HUDCurrency has no DTO; the test is the only contract enforcement layer beyond the XML doc.

---

## 3 — `IEconomyService` v2 + `CurrencyType.Energy`

Already locked in Phase 0 (Q1). Re-stated here for completeness — this delta blocks BOTH HUD-Energy AND GameOver Continue-with-Currency-as-Energy.

```csharp
public enum CurrencyType
{
    Coins  = 0,
    Gems   = 1,
    Energy = 2,   // NEW
}

public interface IEconomyService
{
    int Get(CurrencyType currency);
    bool Spend(CurrencyType currency, int amount);
    void Add(CurrencyType currency, int amount);
    bool CanAfford(CurrencyType currency, int amount);
    event Action<CurrencyType, int> OnChanged;
}
```

### Migration table requirements (CHANGELOG `[0.6.0-alpha]` § Breaking)

Per Phase 0 F6 the migration table is mandatory. Quality bar — table MUST include the four blocks below. Enumerating method renames without snippets is INSUFFICIENT.

**Block 1 — Before/after code snippets per renamed/removed method.** Diff format so buyer can `sed`/regex on their codebase:

```diff
- economy.GetCoins()
+ economy.Get(CurrencyType.Coins)

- economy.GetGems()
+ economy.Get(CurrencyType.Gems)

- economy.SpendCoins(amount)
+ economy.Spend(CurrencyType.Coins, amount)

- economy.SpendGems(amount)
+ economy.Spend(CurrencyType.Gems, amount)

- economy.AddCoins(amount)
+ economy.Add(CurrencyType.Coins, amount)

- economy.AddGems(amount)
+ economy.Add(CurrencyType.Gems, amount)

- economy.OnCoinsChanged += handler
+ economy.OnChanged += (currency, value) => { if (currency == CurrencyType.Coins) handler(value); }

- economy.OnGemsChanged += handler
+ economy.OnChanged += (currency, value) => { if (currency == CurrencyType.Gems) handler(value); }
```

**Block 2 — Section "Why no Obsolete shim".** Explicit rationale for the hard cut: no transitional `[Obsolete] event OnCoinsChanged` forwarder is shipped. Reason: parameterized API IS the contract; shipping shims preserves the v1 surface and undermines the v2 buyer experience. Buyer pays migration cost once at v0.6.0; we don't carry duplicate API surface into v0.7+. This is a documented choice, not an oversight.

**Block 3 — Section "Currency extension limit".** Explicit acknowledgement that `CurrencyType` enum is closed to outside-package extension. Buyer with custom currencies (Tickets, Stars, Hearts, etc.) has three options: (a) fork the package, (b) wait for an extension mechanism in a future version, (c) reuse `Coins`/`Gems`/`Energy` as semantic aliases for their domain. Document honestly so buyer doesn't discover this at runtime when their `(CurrencyType)42` cast silently fails.

**Block 4 — Prefab migration row.** `HUDCoins.prefab` / `HUDGems.prefab` from the Group B builder are REPLACED by the new builder output (HUDCoins / HUDGems as named prefabs of `HUDCurrency` parameterized base, `_currency` pre-set, no dropdown — Phase 0 F3). Buyer steps: delete the Group B demo folder, re-run `Build Group C Sample`. Existing scenes that reference the old prefabs by GUID will break — buyer re-wires references manually (prefab GUIDs change because path changes).

**Block 5 — Coupling note: economy v2 + HUDCurrency are an inseparable migration.** v0.6.0-alpha ships two breaking changes that cannot be reverted independently: (a) `IEconomyService` v1 → v2, and (b) `HUDCoins`/`HUDGems` classes → `HUDCurrency` parameterized. A buyer who reverts (b) by restoring `HUDCoins.cs`/`HUDGems.cs` from git will get compile errors — the restored files reference v1 surface (`OnCoinsChanged`, `GetCoins`, ...) which v2 deleted. Reverting requires either (1) reverting both v0.6.0 migrations together, or (2) manually rewriting the restored HUDs to v2 (`OnChanged + filter`). Do not assume `git restore HUDCoins.cs` is enough. Document the coupling honestly in CHANGELOG so the buyer knows both pieces move as a unit.

**Stub services migration row.** Buyers using the Group B `Fake*` stubs as starter implementations must extend their `IEconomyService` impl with the v2 surface. The shipped `Runtime/Services/Stubs/InMemoryEconomyService.cs` is updated in v0.6.0 — buyer can diff against it as a reference.

---

## 4 — Q2 helpers (Phase 0 lock — restated)

```csharp
// Runtime/Catalog/Popups/Reward/RewardFlow.cs
public static class RewardFlow
{
    public static void GrantAndShow(
        PopupManager popups, IEconomyService economy,
        CurrencyType currency, int amount,
        Action onClaimed = null);

    public static void GrantAndShow(   // F2 overload
        PopupManager popups, IEconomyService economy,
        RewardPopupData data,
        Action onClaimed = null);

    public static void GrantAndShowSequence(
        PopupManager popups, IEconomyService economy,
        IEnumerable<RewardPopupData> rewards,
        Action onSequenceComplete = null);
}

// Runtime/Catalog/Popups/Shop/ShopFlow.cs
public static class ShopFlow
{
    public static void OpenWithPurchaseChain(
        PopupManager popups, UIServices services,
        ShopCategoryFilter category = default,
        Action<ShopItemData> onPurchaseSuccess = null);
}
```

---

## 4.5 — Null-service fallback policy (cross-element)

When a Group C element queries `IProgressionService` (DailyLogin, HUDEnergy) or `IEconomyService` (HUDCurrency base, GameOver Continue-with-Currency) and the service is `null` on `UIServices` at the moment of binding, the element MUST follow this uniform policy. No per-spec invention.

| Element type | Policy | Rationale |
|---|---|---|
| **HUD** (`HUDCurrency`, `HUDEnergy`, `HUDTimer`) | **Silent degrade.** Element renders the plain value (or last-known) without service-driven UI (regen countdown, expiry style, etc.). No console output. | HUDs live on screens and a null service is a valid "feature not configured" state. Buyer may bring HUDEnergy into a scene that has no progression yet. Forcing LogError would spam during scene-edit iteration. Already documented for HUDEnergy E2/E3 — extended here to all HUD types. |
| **Popup** (`DailyLoginPopup`, `LevelCompletePopup`, `GameOverPopup`) | **`Debug.LogError` actionable + abort `OnShow`.** Popup remains closed; PopupManager queue advances. | A popup with missing data is buyer misconfiguration, not a degraded state. Same bar as v0.5.0-alpha fix (d) `[DefaultExecutionOrder]` actionable LogError. Half-bound popups are the worst UX — silently broken CTAs. |
| **Helper** (`RewardFlow`, `ShopFlow`, `DailyLoginFlow`) | **`Debug.LogError` actionable + return `false` (or no-op).** Helper does NOT call into PopupManager. | Helpers compose multiple services; null on any one means the chain is unbuildable. Surfacing at the helper boundary is cheaper than null-ref deep inside a popup. |

**Error message format (mandatory):**

```
[ElementName]: I[ServiceName] not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding.
```

Concrete examples:

```
DailyLoginPopup: IProgressionService not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding.
GameOverPopup: IAdsService not registered on UIServices (required for ContinueMode.Ad). Wire it before opening this popup. See Quickstart § Service binding.
RewardFlow.GrantAndShow: IEconomyService not registered on UIServices. Cannot credit reward. See Quickstart § Service binding.
```

**Acceptance per Phase B element spec:**

- Each spec MUST cite this section in its "Service binding" subsection (one line: "Null-service behavior follows `CATALOG_GroupC_DELTA.md` § 4.5").
- Each popup spec MUST list a QA scenario `Show_With_Null_Required_Service_Logs_Error_And_Aborts` (one per required service the popup consumes).
- Each HUD spec MUST list a QA scenario `Bind_With_Null_Service_Renders_Plain_Value_Silently`.
- Helpers tested at the helper boundary, not via popup.

**Out of scope:** runtime-swap of service from null → bound while popup is open. Buyer responsibility — popup binds at Show, not continuously.

---

## 4.6 — Step 4 implementation specs (HUDCurrency + HUDEnergy)

Locked details before opening any HUD `.cs` file. Validated by `as dev` checker on Step 4 plan.

### `HUDCurrency.cs` (new — replaces HUDCoins/HUDGems classes)

- `[SerializeField] private CurrencyType _currency` MUST carry tooltip:
  `[Tooltip("Currency this HUD displays. Set at prefab time only — runtime changes do not re-subscribe and will silently leave the HUD bound to the previous currency.")]`
- Currency filter mandatory inside `HandleEconomyChanged`: `if (currency != ResolveCurrency()) return;`
- Sealing extension hook: `protected virtual CurrencyType ResolveCurrency() => _currency;` — subclasses override to seal currency unconditionally.
- Class XML doc: document the FD4 currency-filter contract (foreign-currency events ignored silently, sentinel `(CurrencyType)(-1)` naturally excluded).
- Subscribe/Refresh/Unsubscribe override the `UIHUDBase` abstract contract (mirror Group B HUDCoins.cs structure with v2 OnChanged subscription).

### `HUDEnergy.cs : HUDCurrency` (new — E1 Option A subclass)

- Override sealing: `protected override CurrencyType ResolveCurrency() => CurrencyType.Energy;`
- The inherited `_currency` Inspector field is IGNORED by HUDEnergy (read-once-via-virtual-method pattern). `HUDEnergy.prefab` sets it to Energy for Inspector clarity, but `ResolveCurrency()` returns Energy unconditionally — buyer cannot break it via Inspector.
- Class XML doc: explain the seal so future maintainer doesn't think `_currency` field is wired.
- Adds (in `[Serializable] private struct EnergyRefs` for foldout grouping): `RegenCountdownLabel` (TMP_Text), `MaxCapLabel` (TMP_Text), `EnergyBarFill` (Image, type Filled).
- `Subscribe()` override: `base.Subscribe(); SubscribeProgression();` — adds 1Hz `IProgressionService.GetEnergyRegenState()` poll.
- `Unsubscribe()` override: reverse order — `UnsubscribeProgression(); base.Unsubscribe();`.
- Null-progression policy: per § 4.5 (HUD = silent degrade). If `Services.Progression == null`, regen UI hidden, base HUDCurrency display works as plain energy counter.

### Tests strategy (Option A — parameterized, locked)

- DELETE `Tests/Editor/HUDCoinsTests.cs` + `Tests/Editor/HUDGemsTests.cs`.
- CREATE `Tests/Editor/HUDCurrencyTests.cs` with `[TestCase(CurrencyType.Coins)]` + `[TestCase(CurrencyType.Gems)]` per applicable test.
- Test method renames (currency-agnostic):
  - `HandleCoinsChanged_Updates_Last_Value` → `HandleEconomyChanged_Updates_Last_Value`
  - `Refresh_With_Service_Reads_Initial_Value_Without_Subscribe_Side_Effects` → unchanged (already agnostic)
  - `Default_Format_Uses_Thousand_Separators_For_Large_Values` → unchanged (already agnostic)
  - `Refresh_With_Null_Service_Logs_Error_And_Does_Not_Throw` → unchanged
- ADD new test: `OnChanged_With_Foreign_Currency_Does_Not_Trigger_ApplyValue` (FD4 currency-filter contract — single test, not parameterized; explicitly fires OnChanged for "the other" currency and asserts no punch tween / no value change).
- `HUDEnergy` gets its OWN file `Tests/Editor/HUDEnergyTests.cs` (regen + cap behaviors that base HUDCurrency does NOT have — tested separately).

---

## 5 — Group C pre-flight follow-ups (`as qa` + `as dev` mental pass)

**APPLIED (2026-05-02 same session)** — all 8 FIX NOW absorbed into the relevant specs. Table preserved as audit trail.

| ID | Spec | Concern | Status | Where applied |
|---|---|---|---|---|
| FQ1 | DailyLogin | D7 read-only crossing UTC midnight should auto-transition | ✅ APPLIED | `DailyLoginPopup.md` D7 + QA scenario 15 |
| FQ2 | DailyLogin | `DailyLoginFlow.ShowIfDue` idempotency | ✅ APPLIED | `DailyLoginPopup.md` Convenience helper § + QA scenario 14 |
| FQ3 | DailyLogin | Watch-to-double + already-claimed-today (`DoubledToday` field) | ✅ APPLIED | `DailyLoginPopup.md` D5 + QA scenario 16. **Requires `DailyLoginState.DoubledToday: bool` extension in § 2 above — added to acceptance.** |
| FQ4 | GameOver | Ad failure path = host responsibility | ✅ APPLIED | `GameOverPopup.md` Edge cases § |
| FQ5 | HUD-Energy | Subscribe override semantics | ✅ APPLIED | `HUD-Energy.md` E2 |
| FQ6 | HUD-Timer | Mode immutability post-OnEnable | ✅ APPLIED | `HUD-Timer.md` T1 |
| FD3 | DailyLogin / LevelComplete / GameOver | DTO defaults assertion missing from QA scenarios | ✅ APPLIED | QA scenarios 13/13/18 in respective specs (HUDs have no DTO so excluded) |
| FD4 | HUDCurrency (Phase 0) | Sentinel/foreign-currency event filtering | ✅ APPLIED | `CATALOG_GroupC_DELTA.md` § 2.5 (above) |

| ID | Concern | Defer reason |
|---|---|---|
| NQ1 | Multiple HUDTimer × 30Hz performance | Negligible (~150 updates/sec for 5 timers). Note in code, no spec change. |
| NQ2 | GameOver `ContinueCurrencyLabel` placeholder fallback when buyer override has no `{amount}` | Buyer-side concern, doc only. |
| NQ3 | HUD-Energy Update() optimization when full | Micro-opt. Skip-when-full inside class, no spec change. |
| ND1 | DailyLogin config as ScriptableObject | Future buyer pattern, not blocker. |
| ND2 | GameOver `ContinueMode` enum vs split bools | Trade-off accepted — enum keeps invalid state unrepresentable. |

---

## Acceptance criteria for Group C delivery start

Before opening DTO files in next session:

- [ ] `UIThemeConfig.cs` updated with 5 new slots
- [ ] Bootstrap Defaults updated with placeholder sprites/colors for new slots
- [ ] `IProgressionService.cs` extended with `GetDailyLoginState` + `GetEnergyRegenState`
- [ ] `Samples~/Catalog_GroupC_Progression/Stubs/InMemoryProgressionService.cs` shipped (full impl, `[DefaultExecutionOrder(-100)]`, ContextMenu triggers) — sample-side, mirrors Group B pattern
- [ ] `Editor/Generators/CatalogGroupCBuilder.cs` wires the stub into the demo scene's `UIServices.Progression` slot
- [ ] `Tests/Editor/Helpers/FakeProgressionService.cs` shipped
- [ ] `IEconomyService.cs` v2 (per Phase 0 lock)
- [ ] `CurrencyType.Energy` value added
- [ ] `HUDCurrency.cs` parameterized base shipped per § 4.6 specs (replaces HUDCoins/HUDGems classes)
- [ ] `HUDEnergy.cs : HUDCurrency` shipped per § 4.6 sealing pattern
- [ ] `HUDCurrencyTests.cs` parameterized + `HUDEnergyTests.cs` separate (Option A locked)
- [ ] CHANGELOG `[0.6.0-alpha]` entry stub created with migration table containing all 4 mandatory blocks (§ 3 above)
- [ ] Null-service fallback policy (§ 4.5) referenced in all 5 Phase B specs' "Service binding" subsection
- [ ] All 119 existing tests pass on fresh compile (delete `Library/ScriptAssemblies` before run — L1 lesson)

Then proceed to per-element DTO + behavior + tests in spec order: DailyLogin → LevelComplete → GameOver → HUD-Energy → HUD-Timer.
