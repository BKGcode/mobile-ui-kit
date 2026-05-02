# HUD-Gems

## Purpose
Live gem counter, reactive to `IEconomyService.OnGemsChanged`. Element 5/5 of Group B. Sibling of HUD-Coins. Validates that `UIHUDBase` supports a second concrete use-case without contract changes.

## Why two HUD elements (vs one parameterized)

Reality check (2026-05-02): `IEconomyService` exposes **typed events** (`OnCoinsChanged(int)` / `OnGemsChanged(int)`), NOT a unified `OnCurrencyChanged(CurrencyType, int)`. `UIHUDBase` is a template-method base with no `CurrencyType` parameter.

→ HUD-Coins and HUD-Gems are NOT a parameterized abstraction validation — they are two concrete subclasses sharing the **lifecycle** (UIHUDBase) but binding to **different typed events**.

Building both proves UIHUDBase generalizes correctly across distinct currency channels without forcing the kit to invent a `Currency<T>` abstraction. If a third currency (e.g. Energy) joins later via Group C, it follows the same pattern.

## Decisions to confirm

Same as HUD-Coins (HC1-HC6). No new decisions specific to gems beyond:

| # | Decision | Proposal |
|---|---|---|
| HG1 | Theme color | Counter label uses `TextPrimary` (same as Coins). Premium tint deferred — Theme could add `GemTint` slot later. |
| HG2 | Default format | `"N0"` same as Coins. Buyer overrides via Inspector. |
| HG3 | Click behavior | Same UnityEvent pattern. Default unwired. |

## DTO
None — see HUD-Coins spec.

## Services consumed
- `IEconomyService.OnGemsChanged` / `GetGems()`. Same `_services` ref pattern.

## Events emitted
- `_onGemClickEvent` (UnityEvent).

## Animation contract
- Same as HUD-Coins (punch tween, kill-before-create, SetLink, no IUIAnimator).

## Theme tokens consumed
- `IconGem` (icon Image — `ThemedImage`).
- `TextPrimary` / `FontBody` (counter — `ThemedText`).

## Edge cases
Same as HUD-Coins. The two implementations differ ONLY in:
1. Event subscribed (`OnGemsChanged` vs `OnCoinsChanged`).
2. Read method (`GetGems()` vs `GetCoins()`).
3. Default theme icon (`IconGem` vs `IconCoin`).

If a third currency arrives (Group C — likely Energy), the implementation is a 30-line copy with three lines changed. **This is acceptable duplication** because the alternative (introducing `Currency<T>` parameterization) breaks the existing `IEconomyService` contract and introduces a generic constraint that infects the entire economy surface.

## QA scenarios
Same as HUD-Coins, swap `Coins` → `Gems` in stub calls. Demo scene MUST cover both HUD elements simultaneously to validate they don't fight each other (separate Subscribe paths, separate punch tweens).

## Open architecture question (Group C decision — before HUD-Energy lands)

**Question**: Should `IEconomyService` migrate to a parameterized contract before Group C builds HUD-Energy?

**Current contract** (Group B builds against this):
```csharp
public interface IEconomyService
{
    int GetCoins();           int GetGems();
    bool SpendCoins(int);     bool SpendGems(int);
    void AddCoins(int);       void AddGems(int);
    event Action<int> OnCoinsChanged;
    event Action<int> OnGemsChanged;
    bool CanAfford(CurrencyType currency, int amount);
}
```

**Proposed v2** (Group C if accepted):
```csharp
public interface IEconomyService
{
    int Get(CurrencyType currency);
    bool Spend(CurrencyType currency, int amount);
    void Add(CurrencyType currency, int amount);
    bool CanAfford(CurrencyType currency, int amount);
    event Action<CurrencyType, int> OnChanged;
}
```

**Tradeoff**:

| Axis | v1 (current) | v2 (proposed) |
|---|---|---|
| HUD per currency | New class per currency (HUD-Coins, HUD-Gems, HUD-Energy = 3 classes) | Single `HUDCurrency` parameterized by enum (1 class) |
| Migration effort | Zero | BREAKING — every buyer impl rewrites |
| Adding 4th currency | Add `GetXxx`/`SpendXxx`/`AddXxx`/`OnXxxChanged` to interface (BREAKING) + new HUD class | Add enum value (NOT breaking the interface) + reuse `HUDCurrency` |
| Type safety | Compiler catches `economy.SpendCoins(x)` typos | Runtime — `Spend(CurrencyType.Coins, x)` requires param check |
| Group A/B existing code | Compiles unchanged | Refactor `IShopDataProvider`, all popups, all tests |

**Decision criteria** (apply at Group C kickoff, NOT now):
1. Does Group C add a 3rd currency (Energy)? → +1 evidence for v2.
2. Does the Asset Store competitive analysis show buyers asking for 4+ currencies (premium / event / soft / hard)? → +1 for v2.
3. Is BREAKING acceptable in `v0.6.0-alpha`? → required for v2 path.

**Recommended posture**: v2 is the cleaner long-term contract. Migration timing is the only question. Most likely adopted at Group C kickoff (where Energy joins). Group B does NOT block the migration — it inherits whatever contract exists.

**Why documented now**: HUD-Coins/HUD-Gems duplication this spec acknowledges is a *symptom* of v1, not a Group B architecture flaw. Recording the question prevents Group C from rediscovering it.

## Files
```
Runtime/Catalog/HUD/
└── HUDGems.cs
Tests/Editor/
└── HUDGemsTests.cs
```

## Status
- Code: ⏳ pending (Group B · element 5/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~3, mirror HUD-Coins)
- Prefab: ⏳ pending (`CatalogGroupABuilder.BuildHUDGems`)
- Demo scene entry: ⏳ pending
