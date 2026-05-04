using KitforgeLabs.MobileUIKit.Catalog.NotEnough;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Shop;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupB
{
    public sealed class CatalogGroupBDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform _popupParent;
        [SerializeField] private UIThemeConfig _theme;
        [SerializeField] private UIServices _services;

        [Header("Catalog prefabs (assigned by Build Group B Sample). Drop a prefab whose root has the corresponding catalog component.")]
        [SerializeField] private GameObject _rewardPrefab;
        [SerializeField] private GameObject _shopPrefab;
        [SerializeField] private GameObject _notEnoughPrefab;

        [ContextMenu("HUD — Add 100 coins")]
        private void DebugAddCoins() => _services?.Economy?.Add(CurrencyType.Coins, 100);

        [ContextMenu("HUD — Spend 30 coins")]
        private void DebugSpendCoins() => _services?.Economy?.Spend(CurrencyType.Coins, 30);

        [ContextMenu("HUD — Add 999,000 coins")]
        private void DebugAddManyCoins() => _services?.Economy?.Add(CurrencyType.Coins, 999_000);

        [ContextMenu("HUD — Add 5 gems")]
        private void DebugAddGems() => _services?.Economy?.Add(CurrencyType.Gems, 5);

        [ContextMenu("HUD — Spend 2 gems")]
        private void DebugSpendGems() => _services?.Economy?.Spend(CurrencyType.Gems, 2);

        [ContextMenu("Reward — Coins +100")]
        private void ShowRewardCoins() => SpawnReward(new RewardPopupData
        {
            Title = "Reward!",
            Kind = RewardKind.Coins,
            Amount = 100
        });

        [ContextMenu("Reward — Gems +5")]
        private void ShowRewardGems() => SpawnReward(new RewardPopupData
        {
            Title = "Reward!",
            Kind = RewardKind.Gems,
            Amount = 5
        });

        [ContextMenu("Reward — Item")]
        private void ShowRewardItem() => SpawnReward(new RewardPopupData
        {
            Title = "Item unlocked",
            Kind = RewardKind.Item,
            ItemId = "epic_sword"
        });

        [ContextMenu("Reward — Bundle (3 lines)")]
        private void ShowRewardBundle() => SpawnReward(new RewardPopupData
        {
            Title = "Daily bundle",
            Kind = RewardKind.Bundle,
            BundleLines = new[] { "+100 Coins", "+1 Gem", "+1 XP Boost" }
        });

        [ContextMenu("Reward — Auto-claim 3s")]
        private void ShowRewardAuto() => SpawnReward(new RewardPopupData
        {
            Title = "You won!",
            Kind = RewardKind.Coins,
            Amount = 50,
            AutoClaimSeconds = 3f
        });

        [ContextMenu("Reward — Empty (null DTO)")]
        private void ShowRewardEmpty() => SpawnReward(null);

        [ContextMenu("Reward — Backdrop tap = claim")]
        private void ShowRewardBackdrop() => SpawnReward(new RewardPopupData
        {
            Title = "Reward!",
            Kind = RewardKind.Coins,
            Amount = 25,
            CloseOnBackdrop = true
        });

        [ContextMenu("Chain — Shop → NotEnough → Ad → Reward")]
        private void ShowChainShop() => SpawnChainShop();

        [ContextMenu("Shop — All items")]
        private void ShowShopAll() => SpawnShop(new ShopPopupData
        {
            Title = "Shop"
        });

        [ContextMenu("Shop — Currency only")]
        private void ShowShopCurrency() => SpawnShop(new ShopPopupData
        {
            Title = "Currency",
            Category = ShopCategoryFilter.Of(ShopCategory.Currency)
        });

        [ContextMenu("Shop — Cosmetics only")]
        private void ShowShopCosmetics() => SpawnShop(new ShopPopupData
        {
            Title = "Cosmetics",
            Category = ShopCategoryFilter.Of(ShopCategory.Cosmetic)
        });

        private void SpawnChainShop()
        {
            if (!ChainPrefabsAssigned()) return;
            var instance = SpawnPopup<ShopPopup>(_shopPrefab, "ShopPopup");
            if (instance == null) return;
            instance.Bind(new ShopPopupData { Title = "Shop (chained)" });
            instance.OnPurchaseCompleted += (item, result) => Debug.Log($"[Demo] Chain — Purchase: {item.Id} -> {result}");
            instance.OnPurchaseInsufficient += SpawnChainNotEnough;
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private bool ChainPrefabsAssigned()
        {
            if (_shopPrefab != null && _notEnoughPrefab != null && _rewardPrefab != null) return true;
            Debug.LogError("[CatalogGroupBDemo] Chain requires _shopPrefab, _notEnoughPrefab AND _rewardPrefab. Assign all three before triggering the chain. Run 'Tools/Kitforge/UI Kit/Build Group B Sample' to regenerate the demo references.", this);
            return false;
        }

        private void SpawnChainNotEnough(ShopItemData item)
        {
            var instance = SpawnPopup<NotEnoughCurrencyPopup>(_notEnoughPrefab, "NotEnoughCurrencyPopup");
            if (instance == null) return;
            var missing = ResolveMissingAmount(item);
            instance.Bind(new NotEnoughCurrencyPopupData
            {
                Currency = item.PriceCurrency,
                Required = item.PriceAmount,
                Missing = missing
            });
            instance.OnWatchAdRequested += SpawnChainAd;
            instance.OnBuyMoreRequested += (currency, amount) => Debug.Log($"[Demo] Chain — Buy more {currency} ({amount} missing). Wire shop here.");
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private int ResolveMissingAmount(ShopItemData item)
        {
            if (_services == null || _services.Economy == null) return item.PriceAmount;
            var current = _services.Economy.Get(item.PriceCurrency);
            return Mathf.Max(0, item.PriceAmount - current);
        }

        private void SpawnChainAd(CurrencyType currency, int missing)
        {
            var ads = _services != null ? _services.Ads : null;
            if (ads == null) { Debug.LogError("[Demo] Chain — IAdsService not available. Add InMemoryAdsService and assign it on UIServices."); return; }
            ads.ShowRewardedAd(success =>
            {
                if (!success) { Debug.Log("[Demo] Chain — Ad not completed; reward skipped."); return; }
                SpawnChainReward(currency, missing);
            });
        }

        private void SpawnChainReward(CurrencyType currency, int amount)
        {
            var instance = SpawnPopup<RewardPopup>(_rewardPrefab, "RewardPopup");
            if (instance == null) return;
            instance.Bind(new RewardPopupData
            {
                Title = "Ad reward!",
                Kind = currency == CurrencyType.Coins ? RewardKind.Coins : RewardKind.Gems,
                Amount = amount
            });
            instance.OnClaimed += HandleRewardClaimed;
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private void SpawnShop(ShopPopupData data)
        {
            var instance = SpawnPopup<ShopPopup>(_shopPrefab, "ShopPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnPurchaseCompleted += (item, result) => Debug.Log($"[Demo] Purchase: {item.Id} -> {result}");
            instance.OnPurchaseInsufficient += HandleShopInsufficient;
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        [ContextMenu("NotEnough — Coins missing 50")]
        private void ShowNotEnoughCoins() => SpawnNotEnough(new NotEnoughCurrencyPopupData
        {
            Currency = CurrencyType.Coins,
            Required = 200,
            Missing = 50
        });

        [ContextMenu("NotEnough — Gems missing 5")]
        private void ShowNotEnoughGems() => SpawnNotEnough(new NotEnoughCurrencyPopupData
        {
            Currency = CurrencyType.Gems,
            Required = 8,
            Missing = 5
        });

        [ContextMenu("NotEnough — Decline only")]
        private void ShowNotEnoughDeclineOnly() => SpawnNotEnough(new NotEnoughCurrencyPopupData
        {
            Currency = CurrencyType.Coins,
            Missing = 100,
            ShowBuyMore = false,
            ShowWatchAd = false,
            ShowDecline = true
        });

        private void SpawnNotEnough(NotEnoughCurrencyPopupData data)
        {
            var instance = SpawnPopup<NotEnoughCurrencyPopup>(_notEnoughPrefab, "NotEnoughCurrencyPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnBuyMoreRequested += (currency, missing) => Debug.Log($"[Demo] Buy more {currency} (missing {missing}). Host could open ShopPopup here.");
            instance.OnWatchAdRequested += HandleWatchAdRequested;
            instance.OnDeclined += () => Debug.Log("[Demo] User declined the offer.");
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private void HandleShopInsufficient(ShopItemData item)
        {
            var economy = _services?.Economy;
            var owned = economy != null ? economy.Get(item.PriceCurrency) : 0;
            var missing = Mathf.Max(item.PriceAmount - owned, 0);
            SpawnNotEnough(new NotEnoughCurrencyPopupData
            {
                Currency = item.PriceCurrency,
                Required = item.PriceAmount,
                Missing = missing
            });
        }

        private void HandleWatchAdRequested(CurrencyType currency, int missing)
        {
            var ads = _services?.Ads;
            if (ads == null) { Debug.LogWarning("[Demo] No IAdsService — cannot fulfil ad request."); return; }
            ads.ShowRewardedAd(success =>
            {
                if (!success) { Debug.Log("[Demo] Ad cancelled — no reward."); return; }
                SpawnReward(new RewardPopupData
                {
                    Title = "Earned!",
                    Kind = currency == CurrencyType.Coins ? RewardKind.Coins : RewardKind.Gems,
                    Amount = missing
                });
            });
        }

        private void SpawnReward(RewardPopupData data)
        {
            var instance = SpawnPopup<RewardPopup>(_rewardPrefab, "RewardPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnClaimed += HandleRewardClaimed;
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private void HandleRewardClaimed(CurrencyType currency, int amount)
        {
            if (_services == null || _services.Economy == null) return;
            if ((int)currency == RewardPopup.ItemCurrencySentinel) return;
            _services.Economy.Add(currency, amount);
        }

        private T SpawnPopup<T>(GameObject prefab, string name) where T : UIModuleBase
        {
            if (prefab == null) { Debug.LogError($"[CatalogGroupBDemo] {name} prefab not assigned. Run 'Tools/Kitforge/UI Kit/Build Group B Sample' to generate it.", this); return null; }
            var go = Instantiate(prefab, _popupParent, false);
            var instance = go.GetComponent<T>();
            if (instance == null) { Debug.LogError($"[CatalogGroupBDemo] Spawned prefab is missing the {name} component on its root.", this); Destroy(go); return null; }
            instance.Initialize(_theme, _services);
            return instance;
        }
    }
}
