# ShopPopup

## Purpose
Modal grid of buyable items with prices and Buy buttons. Element 2/5 of Group B. The kit's universal in-app shop surface — works for soft/hard currency, consumables, cosmetics, bundles.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| S1 | Purchase authority | `ShopPopup` calls `IShopDataProvider.Purchase(itemId)` only. Never `IEconomyService.Spend*` directly. | The provider is the canonical buy surface. Internally it can delegate to economy + grant rewards. Cumple MUSTN'T #4 (no implementation assumption). |
| S2 | Insufficient funds handling | `Purchase` returning `InsufficientFunds` → popup emits `OnPurchaseInsufficient(item)` and stays open. Host decides whether to open `NotEnoughCurrencyPopup`. | Cumple MUSTN'T #1 (no popup chain coupling). Host wires `OnPurchaseInsufficient` → `PopupManager.Show<NotEnoughCurrencyPopup>(...)`. |
| S3 | Category filter | `ShopPopupData.Category` field (nullable `ShopCategory?`). When null → `IShopDataProvider.GetAllItems()`. When set → `GetItems(category)`. | Single popup serves both "All items" tab and "Cosmetics only" filtered shop. Category tab UI is OUT of scope (host responsibility). |
| S4 | Item cell prefab | Cell is a child of the grid container, **NOT** a separate prefab. Built inline by `CatalogGroupABuilder` and instantiated at runtime via clone-from-template pattern. | Avoids second prefab + asset path indirection. Mirrors how `TutorialPopup` builds steps inline. Buyer can replace by editing the prefab. |
| S5 | Cell layout | Vertical price+icon+name pattern, fixed 200×260 cell. Grid uses `GridLayoutGroup` with auto cells/row. | Mid-core mobile shop convention (Royal Match, Coin Master). Portrait first. |
| S6 | Empty data behavior | When `Items` is null/empty → popup shows centered "Shop is empty" placeholder text. Buy interactions disabled. | No NRE. Buyer sees friendly state during data fetch. |
| S7 | Currency display | Cell shows `Theme.IconCoin`/`Theme.IconGem` based on `ShopItemData.PriceCurrency`. | Existing Theme slots. Zero new assets. |
| S8 | Close button | Top-right `CloseButton` always present. Backdrop tap dismisses (default `true` for shop UX). | Different from Confirm/NotEnough where backdrop is opt-in — shop dismissal is expected gesture. |

## DTO

`ShopPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Shop"` | Header label. |
| `Category` | `ShopCategoryFilter` | `All` | Wrapper struct around nullable `ShopCategory` (Unity-serializable). `All` → ignore filter. |
| `CloseOnBackdrop` | `bool` | `true` | Per S8 default. |

`ShopPopup` queries `Services.ShopData.GetAllItems()` (or filtered) on `Bind` — items are NOT in the DTO to keep popup data-source-driven.

`ShopCategoryFilter` (struct, serializable wrapper): `bool UseFilter`, `ShopCategory Category`.

## Services consumed
- `IShopDataProvider` (REQUIRED). Calls: `GetAllItems()` / `GetItems(category)` on `Bind`, `Purchase(itemId)` on Buy click. Without it the popup logs an error and shows the empty-state placeholder.
- `IEconomyService` (REQUIRED for affordability check, optional for display). Calls: `CanAfford(currency, amount)` per cell to gray out unaffordable Buy buttons.
- `IUIAudioRouter` (optional). Cues: `PopupOpen` (Show), `ButtonTap` (item Buy attempt), `Success` (Purchase=Success), `Error` (Purchase=Failed/AlreadyOwned), `PopupClose` (Hide). InsufficientFunds plays `Error` once before emitting.

## Events emitted

| Event | When |
|---|---|
| `OnPurchaseCompleted(ShopItemData item, PurchaseResult result)` | After `IShopDataProvider.Purchase` returns. Fires for ALL non-null results — `Success`, `AlreadyOwned`, `Failed`, but NOT `InsufficientFunds` (that has its own event below). |
| `OnPurchaseInsufficient(ShopItemData item)` | When `Purchase` returns `InsufficientFunds`. Popup stays open. Host wires this to open `NotEnoughCurrencyPopup`. |
| `OnClosed` | Close button click, OR backdrop tap when `CloseOnBackdrop=true`, OR back press. |
| `OnDismissed` | Always after hide animation completes. Fires AFTER `OnClosed`. Use for cleanup. |

All events reset on `Bind(...)`.

## Animation contract
- `[RequireComponent(typeof(UIAnimShopPopup))]`. Auto-resolved.
- Show: scale pop-in + fade-in on `_card`. Style preset = `Snappy` recommended (decisive, short — shop is utilitarian).
- Hide: scale-down + fade-out.
- Per-cell entry: optional cascade fade-in (stagger from preset). Out of MVP scope; tracked as deferred.

## Theme tokens consumed
- `BackgroundLight` (popup card).
- `PrimaryColor` (Buy button affordable state).
- `SecondaryColor` (Buy button unaffordable / disabled).
- `IconCoin` / `IconGem` (cell currency icon).
- `IconClose` (close button).
- `DefaultAnimPreset`.

## Edge cases
- **Null DTO**: `Bind(null)` → defaults (`"Shop"`, all categories).
- **Null `IShopDataProvider`**: error log + empty-state placeholder. Popup still shows so back press works.
- **Null `IEconomyService`**: cells render with affordable=true (no gray-out). Still allows Purchase attempts.
- **Item count = 0**: empty-state placeholder.
- **Item count > 12 (overflow)**: ScrollRect on grid container. MVP includes vertical scroll.
- **Currency change mid-popup** (`OnCoinsChanged` fires while popup open): `RefreshAffordability()` recomputes Buy button states. NO full grid rebuild.
- **Re-Show same instance**: `Bind` resets events + grid (clears existing cells, rebuilds from current ShopData).
- **Race conditions** (`IsDismissing` guard):
  - Double-click Buy → second ignored.
  - Spam Buy across multiple cells → all queued through `Purchase` synchronously; if any returns `InsufficientFunds`, popup stops processing further clicks until next interaction.
  - Back press during hide → ignored.

## QA scenarios
1. `Show — All items (4 default in stub)` — verify grid populates, prices show.
2. `Show — Filtered by Currency` — verify only currency packs.
3. `Buy — Affordable item` — verify `OnPurchaseCompleted(Success)`, popup stays open, HUD-Coins updates.
4. `Buy — Unaffordable item` — verify `OnPurchaseInsufficient` fires, popup stays open.
5. `Buy — Already owned (cosmetic stub)` — verify `OnPurchaseCompleted(AlreadyOwned)`.
6. `Show — Empty shop` (stub returns 0 items) — verify placeholder.
7. `Show — Null provider` — verify error log + placeholder.
8. `Currency change mid-popup` — externally call `IEconomyService.AddCoins(1000)` and verify Buy buttons re-enable without rebuild.
9. `Close — Close button`.
10. `Close — Backdrop tap`.
11. `Close — Back press`.
12. `Stress — Spam Buy on unaffordable` — single `OnPurchaseInsufficient` fires, no double event.

## Convenience helpers (deferred — Group C decision)

**Status**: OUT of Group B implementation. Spec-only sketch. Re-evaluated when Group C ships (DailyLogin, LevelComplete, GameOver — all may chain to Shop). If 3+ callsites repeat the chain wiring, helper is implemented in Group C.

Proposed signature (NOT shipped in Group B):

```csharp
// Runtime/Catalog/Popups/Shop/ShopFlow.cs
public static class ShopFlow
{
    // Opens ShopPopup with the standard purchase chain pre-wired:
    //   ShopPopup.OnPurchaseInsufficient → opens NotEnoughCurrencyPopup
    //   NotEnoughCurrencyPopup.OnWatchAdRequested → IAdsService.ShowRewardedAd
    //     → on success: opens RewardPopup with reward data
    //   RewardPopup.OnClaimed → IEconomyService.Add(currency, amount)
    //
    // Each popup remains decoupled (MUSTN'T #1 preserved) — the helper is the host.
    public static void OpenWithPurchaseChain(
        PopupManager popups,
        UIServices services,
        ShopCategoryFilter category = default,
        Action<ShopItemData> onPurchaseSuccess = null);
}
```

**Why deferred**: capability-gate. Group B has 1 chain callsite (Demo). Group C adds 2 more (GameOver "Continue with ad" → reward, DailyLogin "Watch to double" → reward). 3 callsites = warranted helper. Group B alone does not justify it.

**Why documented now**: locks contracts. Helper requires:
- `ShopPopup.OnPurchaseInsufficient(ShopItemData)` event
- `NotEnoughCurrencyPopup.OnWatchAdRequested(CurrencyType, int)` event
- `RewardPopup.OnClaimed(CurrencyType, int)` event

These events are part of the Group B public surface. Spec'ing the helper now ensures the surface stays helper-compatible — no contract churn at Group C build time.

## Files
```
Runtime/Catalog/Popups/Shop/
├── ShopPopupData.cs
├── ShopCategoryFilter.cs
├── ShopItemView.cs           ← cell view, NOT a prefab (per S4)
├── ShopPopup.cs
└── UIAnimShopPopup.cs
Tests/Editor/
└── ShopPopupTests.cs
```

## Status
- Code: ⏳ pending (Group B · element 2/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~7)
- Prefab: ⏳ pending (`CatalogGroupABuilder.BuildShopPopup`)
- Demo scene entry: ⏳ pending
