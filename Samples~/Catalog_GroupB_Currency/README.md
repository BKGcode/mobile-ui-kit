# Catalog — Group B (Currency)

Sample demonstrating the Currency layer: `RewardPopup`, `ShopPopup`, `NotEnoughCurrencyPopup`, `HUD-Coins`, `HUD-Gems`.

## What ships
- 3 in-memory service stubs (`InMemoryEconomyService`, `InMemoryShopDataProvider`, `InMemoryAdsService`).
- One `Catalog_GroupB_Demo` scene built by `Tools > KitforgeLabs > Catalog > Build Group B Sample` (Editor menu).
- `CatalogGroupBDemo` MonoBehaviour with `[ContextMenu]` triggers per scenario.

## Minimum wiring (5 steps)

```csharp
// 1. Add a UIServices container and wire 3 stubs as MonoBehaviour refs.
var services = root.AddComponent<UIServices>();
services.SetEconomy(root.AddComponent<InMemoryEconomyService>());
services.SetShopData(root.AddComponent<InMemoryShopDataProvider>());        // [RequireComponent(InMemoryEconomyService)]
services.SetAds(root.AddComponent<InMemoryAdsService>());

// 2. Spawn HUD-Coins / HUD-Gems anywhere on the canvas. They self-bind to IEconomyService.

// 3. Spawn ShopPopup. It calls IShopDataProvider.Purchase().
shop.OnPurchaseInsufficient += item => notEnough.Show(/* DTO from item.PriceCurrency + missing */);
shop.OnPurchaseCompleted += (item, result) => { /* host reacts */ };

// 4. Wire NotEnoughCurrencyPopup to ads + reward.
notEnough.OnWatchAdRequested += (currency, amount) =>
{
    services.Ads.ShowRewardedAd(success =>
    {
        if (!success) return;
        reward.Bind(new RewardPopupData { Kind = ToRewardKind(currency), Amount = amount });
        reward.Show();
    });
};

// 5. RewardPopup credits the economy at claim time (host wires — popup stays decoupled).
reward.OnClaimed += (currency, amount) =>
{
    if (currency == CurrencyType.Coins) services.Economy.AddCoins(amount);
    else if (currency == CurrencyType.Gems) services.Economy.AddGems(amount);
};
```

## End-to-end chain (Buy → NotEnough → Ad → Reward → HUD)
1. Open Shop, tap an unaffordable item → `OnPurchaseInsufficient` fires.
2. `NotEnoughCurrencyPopup` opens, tap **Watch ad** → `InMemoryAdsService` simulates 1s → calls back with `success=true`.
3. `RewardPopup` opens with the missing amount → tap **Claim**.
4. `OnClaimed` credits the economy → `OnCoinsChanged`/`OnGemsChanged` fires → HUD punches its counter.

## Helpers — NOT shipped in Group B
The kit will gain `RewardFlow.GrantAndShow`, `RewardFlow.GrantAndShowSequence`, and `ShopFlow.OpenWithPurchaseChain` in Group C, when 3+ buyer callsites justify them. See `Documentation~/Specs/Catalog/RewardPopup.md` and `ShopPopup.md` for signatures.

## Notes
- Stubs are deterministic — no network, no save, no persistence. Restart resets.
- The popups never call `IEconomyService` write methods directly. Currency mutation is always a host concern (see MUSTN'T #1 in `Documentation~/Specs/CATALOG.md`).
