# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- **Hub Catalog tab filter chips** — chips now scope by spawn pattern (Popup / Toast / Screen / HUD) instead of internal group letters (A / B / C / D / E). Cell layout simplified: group badge removed, detail meta no longer mentions group. Internal `KitforgeCatalogEntry.Group` field retained for kit-author tooling but no longer surfaced in buyer-facing Hub UI. `KitforgeHubState.CatalogGroupFilter` renamed to `CatalogPatternFilter` (serialized field `_catalogGroupFilter` → `_catalogPatternFilter`; one-time reset of persisted filter on first launch after upgrade — semantic value changed from group letter to pattern name).
- **Hub Theme Studio "New Theme" button** — activated (previously placeholder, disabled). Click creates a new `UIThemeConfig` at `Assets/Settings/Themes/Theme_New.asset` (subtree auto-created if absent) with inline rename in Project window. Replaces the prior "use Assets → Create → KitforgeLabs → Theme" tooltip workaround.

## [1.2.0] — 2026-05-11

Catalog prefabs now ship with the package and the kit boots out-of-box on a clean install. Service contract reworked around Null Object defaults so buyers never see "service not registered" errors before wiring their own implementations. EditMode smoke test covers all 13 manager-driven catalog elements.

### Added

- **17 catalog prefabs shipped with the package** under `Runtime/Catalog/Prefabs/` (10 popups, 1 toast, 2 screens, 4 HUDs). The `KitforgeRoot.prefab` registries are pre-wired so spawning any catalog element via `PopupManager.Show<T>()`, `UIManager.Push<T>()` or `ToastManager.Show<T>()` works on first install with zero buyer setup.
- **Null Object service defaults** in `Runtime/Services/Null/`. `UIServices` now auto-instantiates `NullEconomyService`, `NullPlayerDataService`, `NullProgressionService`, `NullShopDataProvider`, `NullAdsService`, `NullTimeService`, `NullAudioRouter` and `NullLocalizationService` in `Awake` when the corresponding Inspector slot is empty. The kit always boots; buyers swap in production services via the Inspector when ready. A single warning logs at startup if any Null defaults are used.
- **`TryShow<T>(out T popup, out ShowFailureReason reason)` API** on `PopupManager` and `ToastManager`, plus `TryPush<T>(out T screen, out ShowFailureReason reason)` on `UIManager`. The new methods replace the ambiguous `null` return of legacy `Show<T>` / `Push<T>` with a structured failure reason (`PrefabMissing`, `Queued`, `InitializationFailed`, `ManagerUnavailable`). Legacy methods remain for backwards compatibility.
- **EditMode smoke test suite** at `Tests/Editor/Smoke/KitforgeRootSmokeTests.cs`. Spawns the `KitforgeRoot.prefab` in a temp scene and asserts every catalog popup / screen / toast opens cleanly with the Null Object defaults — no errors, no warnings. Gates v1.2.0+ releases against silent regressions.
- **Editor menus** `KitforgeLabs/UI Kit/Maintenance/Regenerate Catalog Prefabs (Dev)` and `... /Wire Catalog Into KitforgeRoot (Dev)` for the package maintainer to rebuild and re-wire the catalog when the kit evolves. Both are dev-only and refuse to write into a read-only (Git-installed) package — embed the package first.

### Changed

- **`PopupManager.Show<T>` queue path** now emits a `Debug.LogWarning` instead of silently returning `null` when the stack is full and no lower-priority popup can be evicted. Behavior unchanged otherwise.
- **`ToastManager.Show<T>` concurrent path** matches: a `Debug.LogWarning` is emitted when the request is queued because `_maxConcurrent` is reached.
- **Hub Test launcher** now dismisses prior popups before spawning the next, so cycling through the catalog never silently hits the `MaxDepth` queue. Error messages also include the inner exception stacktrace when a popup throws during `OnShow` / `BindUntyped`.
- **`KitforgeRoot.prefab` ships with empty service Inspector slots** — UIServices populates Null Object defaults at runtime. Previous behavior (pre-wired in-memory stub services) is removed: stubs were sample-only and conflicted with the "no samples" mandate.

### Removed

- **`Runtime/Services/Stubs/`** (`InMemoryEconomyService`, `InMemoryShopDataProvider`, `InMemoryAdsService`, `InMemoryTimeService`, `InMemoryProgressionService`, `InMemoryLocalizationService`). Replaced by the Null Object defaults in `Runtime/Services/Null/`. Buyers who relied on the in-memory state can either wire their own production services (the contract is unchanged) or copy a Null Object as a starting template.

## [1.1.3] — 2026-05-11

### Added

- **`IsRegistered(Type)` public API** on `PopupManager`, `UIManager` and `ToastManager`. Returns `true` when a prefab matching the requested type exists in the manager's Inspector array. Lets editor tooling and host code pre-check registration without triggering the error-logging path.

### Fixed

- **Hub Test launcher silent error spam** when buyer clicks Spawn on a popup / screen / toast whose prefab is not yet wired into the manager. The launcher now uses `IsRegistered` to pre-check before invoking the manager method; if missing, it logs a single warning explaining how to wire the prefab (instead of triggering 17 `[PopupManager] No prefab registered` errors when the buyer cycles through the catalog).
- **Test launcher banner copy** clarifies the v1.1.x contract: the kit ships catalog *components* but not prefab assets — the buyer wires their own. v1.2.0 will revisit this with package-shipped prefabs or programmatic spawn.

## [1.1.2] — 2026-05-11

### Fixed

- **Setup Wizard Step 3** previously invoked the `KitforgeLabs/UI Kit/Build Group A Sample` menu, which v1.1.0 removed. The wizard now uses Step 3 to switch to the Hub Catalog tab — the canonical flow now that samples no longer ship. Step 1 description updated to reference the v1.1.0 path `Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets/`.
- **Stray sample-builder references** in error messages cleared from `ShopPopup`, `KitforgeTestLauncher`, `KitforgeCatalogBrowser`, and per-element specs. Error guidance now points at the Hub Catalog snippet panel or the buyer's own Inspector wiring rather than at deleted menu items.

## [1.1.1] — 2026-05-11

### Fixed

- **Hub state path** now follows the v1.1.0 namespace convention. `KitforgeHubState.asset` is created at `Assets/KitforgeLabs/UI Kit/Settings/HubState.asset` (was `Assets/Settings/Kitforge/HubState.asset` — a v1.1.0 oversight). Buyers upgrading from v1.1.0 can safely delete the legacy `Assets/Settings/Kitforge/` folder; the Hub re-creates the state at the new path on next open. Tab selection / catalog filter / theme selection reset on first launch after upgrade (one-time cosmetic — no persistent state lost).

## [1.1.0] — 2026-05-11

Major studio-wide reorganization under the `KitforgeLabs` namespace and removal of downloadable Samples — the Hub editor window is now the single entry point.

### Changed

- **Top-level menu**: every entry point now lives under `KitforgeLabs/UI Kit/...` (previously `Tools/Kitforge/UI Kit/...`). The kit registers a top-level `KitforgeLabs` menu in the editor menu bar — the same convention will be used by every future KitforgeLabs product.
- **Asset folder root**: Bootstrap Defaults now writes preset assets to `Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets/` (previously `Assets/Settings/UIAnimPresets/`). The single namespace folder keeps the buyer's `Assets/` clean.
- **Namespace**: `KitforgeLabs.MobileUIKit.*` is renamed to `KitforgeLabs.UIKit.*`. Assembly definitions match (`KitforgeLabs.UIKit`, `KitforgeLabs.UIKit.Catalog`, `KitforgeLabs.UIKit.Editor`, `KitforgeLabs.UIKit.Tests.*`). Buyer code referencing the old namespace updates with a global find-and-replace.
- **Create asset menus**: `Assets → Create → Kitforge → UI Kit → Theme` and `... Anim Preset` are now `Assets → Create → KitforgeLabs → UI Kit → Theme` / `... Anim Preset`.
- **Package display name**: `Kitforge Mobile UI Kit` → `KitforgeLabs · UI Kit`.

### Removed

- **All Package Manager Samples** (9 entries). The kit no longer ships downloadable samples. The Hub (`KitforgeLabs → UI Kit → Hub`) replaces them: Setup wizard for scene wiring, Catalog browser with copy-paste code snippets for every element, Theme Studio for live preview, inline Cheatsheet for one-page reference.
- **Catalog builders** (`Editor/Generators/Catalog*Builder.cs`, `EditorSceneFactory.cs`, `CatalogAllDemoBuilder.cs`, `CatalogM41ThemeBuilder.cs`, `CatalogGroupBuilderShared.cs`). These existed solely to materialize the deleted samples.
- **`Editor/Audit/` tooling** (pre-tag QA scanner, 7 checks, snapshot pipeline). The audit was internal maintainer tooling and never belonged in the shipped package. Future buyer-facing diagnostics, if any, will live in the Hub.
- **Internal dev artifacts**: `MEMORY.md`, `Documentation~/QAReports/`, and the `CATALOG_Group*_DELTA.md` per-group delta notes. The buyer-facing `Documentation~/CHEATSHEET.md` and per-element `Documentation~/Specs/Catalog/*.md` remain.

### Migration

1. Update `Packages/manifest.json`:
   ```json
   "com.kitforgelabs.mobile-ui-kit": "https://github.com/BKGcode/mobile-ui-kit.git#v1.1.0"
   ```
2. Global find-and-replace in your code: `KitforgeLabs.MobileUIKit` → `KitforgeLabs.UIKit`.
3. If you previously imported Samples, delete `Assets/Samples/Kitforge Mobile UI Kit/` and any of the legacy `Assets/Catalog_Group*_Demo/` folders. None are needed any more — Hub replaces them.
4. Move any `Assets/Settings/UIAnimPresets/` content you authored into `Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets/`. Re-run `KitforgeLabs → UI Kit → Bootstrap Defaults` to regenerate the 10 stock presets at the new path.
5. Verify the menu bar shows the new top-level `KitforgeLabs` entry and that `KitforgeLabs → UI Kit → Hub` opens the Hub window.

## [1.0.1] — 2026-05-11

### Fixed

- **Input System package compatibility** — `KitforgeRoot.prefab` previously shipped a legacy `StandaloneInputModule` on its `EventSystem`, which threw `InvalidOperationException` for buyers with `Active Input Handling = Input System Package` (legacy disabled). A new `KitforgeInputBootstrap` runtime static class (gated by `ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER`) hooks `SceneManager.sceneLoaded` via `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` and reflectively swaps `StandaloneInputModule` for `UnityEngine.InputSystem.UI.InputSystemUIInputModule` on every `EventSystem` in the scene. No asmdef changes (reflection-based — works whether the Input System package is installed or not). No prefab modification. Zero buyer action required after upgrade.

## [1.0.0] — 2026-05-10

First stable release. Public API frozen.

Surface:

- **Framework**: `UIManager` (Push / Pop / Replace / PopToRoot with prefab cache and registry validator), `PopupManager` (priority queue with eviction and backdrop sync), `UIRouter` (AppState transitions), `UIThemeConfig` (one ScriptableObject re-skins everything), `UIServices` (DI-free service container), `KitforgeThemeBinder` (single source of truth for the active theme).
- **17-element catalog**: 10 popups (Confirm / Pause / Tutorial / Reward / Shop / NotEnoughCurrency / DailyLogin / LevelComplete / GameOver / Settings) · 1 toast (Notification) · 2 screens (Loading / MainMenu) · 4 service-bound HUDs (Coins / Gems / Energy / Timer) + 3 service-free HUDSimple variants.
- **3 theme presets**: `Theme_Default`, `Theme_Casual`, `Theme_Premium` under `Runtime/Theme/Presets/`.
- **8 service interfaces**: `IEconomyService`, `IPlayerDataService`, `IProgressionService`, `IShopDataProvider`, `IAdsService`, `ITimeService`, `IUIAudioRouter`, `IUILocalizationService`.
- **PlayerPrefs persistence**: `PlayerPrefsPlayerDataService` ships in Runtime. Eight canonical kit-side keys frozen under the `kfmui.<scope>.<name>` namespace (5 settings + 3 dailylogin).
- **Kitforge Hub** editor window: Setup wizard, Catalog browser, Theme Studio, Test launcher, inline Cheatsheet.

Pre-release alpha and rc versions are not listed here — the v1.0.0 surface is the stable baseline. Buyers upgrading from pre-release versions: pin to `#v1.1.0` directly.
