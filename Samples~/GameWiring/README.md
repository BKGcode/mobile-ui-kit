# Game Wiring — Sample

> **Status:** Revived at `v0.7.0-alpha` (M2 close, 2026-05-04). Compiles against the 8 service interfaces shipped through Group D.

This sample is the integration entry point for a host game project that uses **VContainer** for dependency injection. Drop it in, replace each `Stub*` registration with your real implementation, and you're wired.

> **Not using VContainer?** Use the `UIServices` MonoBehaviour container (Inspector-driven) instead — see Group A/B/C/D Sample Demos for that pattern. This sample is for VContainer-first projects only.

## What ships

| File | Purpose |
|---|---|
| `GameWiringLifetimeScope.cs` | VContainer `LifetimeScope` that registers the 8 services and the UI router. |
| `StubServices.cs` | In-memory implementations of the 8 service interfaces. Compile-time scaffolding so the sample boots out of the box. Replace with your real implementations. |
| `UIRouterStub.cs` | Minimal router that logs `AppState` transitions without invoking services. |

## The 8 service interfaces (`KitforgeLabs.MobileUIKit.Services`)

1. `IEconomyService` — `Get/Spend/Add(CurrencyType)` + `OnChanged` event. Wires currencies (Coins / Gems / Energy).
2. `IPlayerDataService` — primitive key-value persistence (12-method `PlayerPrefs`-shaped surface). `kfmui.<scope>.<name>` namespace reserved for kit-side keys.
3. `IProgressionService` — level state, daily-login state, energy regen state, `OnLevelCompleted` / `OnLevelUnlocked` events.
4. `IShopDataProvider` — shop catalog, prices, IAP product mapping (`GetItems` / `Purchase`).
5. `IAdsService` — rewarded / interstitial ad availability and playback.
6. `ITimeService` — authoritative time source (`GetServerTimeUtc`).
7. `IUIAudioRouter` — UI audio cue playback (`Play(UIAudioCue)`).
8. `IUILocalizationService` — active language + `OnLanguageChanged` dispatch (kit-side re-skin trigger; BYO translation table).

## Bridging VContainer → `UIServices` MonoBehaviour

Kit popups consume services via the `UIServices` MonoBehaviour container (Inspector-driven, `[DefaultExecutionOrder(-100)]`). To bridge VContainer-resolved services to `UIServices`, add a binder MonoBehaviour to your scene:

```csharp
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;
using VContainer;

public sealed class UIServicesBinder : MonoBehaviour
{
    [SerializeField] private UIServices _uiServices;

    [Inject] private IEconomyService _economy;
    [Inject] private IPlayerDataService _playerData;
    [Inject] private IProgressionService _progression;
    [Inject] private IShopDataProvider _shopData;
    [Inject] private IAdsService _ads;
    [Inject] private ITimeService _time;
    [Inject] private IUIAudioRouter _audio;
    [Inject] private IUILocalizationService _localization;

    private void Awake()
    {
        _uiServices.SetEconomy(_economy);
        _uiServices.SetPlayerData(_playerData);
        _uiServices.SetProgression(_progression);
        _uiServices.SetShopData(_shopData);
        _uiServices.SetAds(_ads);
        _uiServices.SetTime(_time);
        _uiServices.SetAudio(_audio);
        _uiServices.SetLocalization(_localization);
    }
}
```

Place the binder GameObject *under* the VContainer `LifetimeScope` so it gets injected before kit popups read services in their `OnEnable`.

## Migration notes (from v0.4.1-alpha parked → v0.7.0-alpha revived)

- `IEconomyService` v1 → v2: per-currency methods (`GetCoins/SpendCoins/...`) replaced with parameterized `(CurrencyType, ...)` form. `CurrencyType.Energy` added in Group C.
- `IPlayerDataService`: rewritten from speculative profile/XP API to 12-method primitive surface (M2 BREAKING).
- 2 new service interfaces shipped: `IUIAudioRouter` (Group 0) + `IUILocalizationService` (M2).
- All 8 service interfaces have shipped surface — `Stub*` impls are no longer empty placeholders.
