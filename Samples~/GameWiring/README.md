# Game Wiring — Sample

> **Status:** Phase 0 placeholder. Full integration walkthrough lands in Phase 5.

This sample is the integration entry point for a host game project. Drop it in, replace each `Stub*` registration with your real implementation, and you're wired.

## What ships

| File | Purpose |
|---|---|
| `GameWiringLifetimeScope.cs` | VContainer `LifetimeScope` that registers the 6 services and the UI router. |
| `StubServices.cs` | No-op implementations of the 6 service interfaces. Used as compile-time scaffolding so the package boots out of the box. |
| `UIRouterStub.cs` | Minimal router that logs `AppState` transitions without invoking services. Replaced by the real `UIRouter` in Phase 1. |

## The 6 service interfaces you must implement in your game project

Each interface lives in `KitforgeLabs.MobileUIKit.Services`:

1. `IEconomyService` — coins, gems, premium currency reads/writes.
2. `IPlayerDataService` — persistent player profile (name, avatar, prefs).
3. `IProgressionService` — level state, stars, unlocks, progress.
4. `IShopDataProvider` — shop catalog, prices, IAP product mapping.
5. `IAdsService` — rewarded / interstitial ad availability and playback.
6. `ITimeService` — authoritative time source (server time, daily reward windows).

> The interfaces are intentionally empty in Phase 0. Method signatures are added in Phase 1. Your real implementations go in your **game** assembly, not in this package.

## 10-step integration (TBD — Phase 5)

A full step-by-step guide will replace this section once Phase 5 ships.
