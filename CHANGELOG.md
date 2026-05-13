# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.3.3] — 2026-05-13

### Added

- **Hub Catalog tab thumbnails** — every cell now renders a 110px thumbnail of the catalog element, loaded directly from `Documentation~/Screenshots/` via `File.ReadAllBytes` (no asset duplication). The detail panel adds a 280px preview between meta and description. HUD entries fall back to a textual placeholder with a hint to open the Demo Scene for live rendering. New file `Editor/Hub/Catalog/KitforgeCatalogThumbnails.cs` caches textures and disposes them on assembly reload.
- **Main Camera in baked Demo Scene** — `KitforgeDemoSceneBaker` now creates a `Main Camera` GameObject (`tag=MainCamera`, AudioListener, dark background `rgb(0.12, 0.13, 0.17)`, perspective 60°) before instantiating `KitforgeRoot.prefab`. GameView no longer renders black and the "Display 1 No cameras rendering" warning is gone.

### Changed

- **`KitforgeCatalogScreenshotBaker` now wires demo services** into the capture rig: a temporary `UIServices` MonoBehaviour holds `DemoEconomyService` / `DemoShopDataProvider` / `DemoPlayerDataService` / etc., passed as the `services` argument to `module.Initialize(theme, services)`. `ShopPopup` now bakes with the item grid populated, `SettingsPopup` with sliders/toggles bound, `RewardPopup` with currency icons. No more `LogError` during EditMode bakes.
- **`Runtime/Demo/KitforgeDemoScene.unity` rebaked** to include the Main Camera.
- **`Documentation~/Screenshots/06_ShopPopup.png`, `12_GameOverPopup.png`, `15_SettingsPopup.png` rebaked** with service-driven content visible.

## [1.3.2] — 2026-05-13

### Removed

- **`KitforgeLabs/UI Kit/Bootstrap Defaults` menu** and its backing class `DefaultUIAnimPresetsCreator`. The kit no longer asks buyers to run a setup tool before animations work. `UIAnimPreset_Playful.asset` ships in `Runtime/Animation/Presets/` and the 3 stock themes (`Theme_Default`, `Theme_Casual`, `Theme_Premium`) are pre-wired to it. Buyers who want extra presets create them via `Assets → Create → KitforgeLabs → UI Kit → Anim Preset` (existing menu, unchanged).
- **Setup Wizard Step 1 ("Bootstrap Defaults")** — wizard now has 2 steps: Add Scene Root, Browse Catalog. The recommended-path Demo Scene banner is unchanged.

### Changed

- **Buyer-visible warnings** in `UIModuleBase.ResolveAnimPreset` and `UIToastBase.ResolveAnimPreset` no longer reference the deleted Bootstrap Defaults menu — they point at `UIThemeConfig.DefaultAnimPreset` directly.
- **Hub Theme Studio empty-hint** updated: instead of "Run Bootstrap Defaults" it points at "Reinstall the package (3 themes ship in Runtime/Theme/Presets/)" or the Create menu.
- **`Documentation~/CHEATSHEET.md`** friction table + "Going deeper" section updated to reflect the new flow (no Bootstrap Defaults).

## [1.3.1] — 2026-05-13

### Changed

- **Maintenance menus relocated** from `KitforgeLabs/UI Kit/Maintenance/...` to `Tools/KitforgeLabs/Test/...`. The kit's top-level `KitforgeLabs/UI Kit/` menu is buyer-facing only (Hub · Open Demo Scene). Kit-author tooling (Bake Demo Scene · Bake Catalog Screenshots · Regenerate Catalog Prefabs · Wire Catalog Into KitforgeRoot) now lives under `Tools/KitforgeLabs/Test/` to make the separation explicit. `(Dev)` suffix removed from labels — the path already conveys it. Compile-gate remains `KITFORGE_DEV_MAINTENANCE`.

## [1.3.0] — 2026-05-13

Click & Play: the kit now ships a ready-to-run demo scene with mock services for every contract, so buyers see all 17 catalog elements working live before writing any code. The Setup Wizard surfaces the demo scene as the recommended path; the 3-step wizard remains for blank-scene workflows. Spawn snippets now include `using` imports and field declarations.

### Added

- **`Runtime/Demo/KitforgeDemoScene.unity`** — a single-scene showcase wiring the 17-element catalog, three themes and `DemoServicesBootstrap`. Open via top menu `KitforgeLabs → UI Kit → Open Demo Scene`. The scene boots into `MainMenuScreen` with HUDs showing live values (1250 coins / 80 gems / 5 energy), a quick-spawn side panel for popups not reachable from the main menu, and a top-right theme cycle button.
- **`Runtime/Services/Demo/`** — 8 demo implementations of every service contract (`DemoEconomyService`, `DemoProgressionService`, `DemoShopDataProvider`, `DemoAdsService`, `DemoTimeService`, `DemoAudioRouter`, `DemoLocalizationService`, `DemoPlayerDataService` — in-memory, no PlayerPrefs writes). Plus `DemoServicesBootstrap` MonoBehaviour (`[DefaultExecutionOrder(-200)]`) that wires them into `UIServices` before any consumer's `OnEnable`. Kit ships with Null Object defaults for production, Demo Object defaults for the showcase, your impl for your game.
- **`Runtime/Demo/DemoMenuController.cs`** — runtime controller that pushes `MainMenuScreen`, wires its events to the catalog popups, builds the quick-spawn side panel, and exposes a theme cycle button. Builds the overlay panel programmatically — no extra prefabs required.
- **`Editor/Hub/KitforgeDemoLauncher.cs`** — top menu `KitforgeLabs → UI Kit → Open Demo Scene` (priority 10) opens the demo scene in Single mode after prompting to save modified scenes.
- **`Editor/Maintenance/KitforgeDemoSceneBaker.cs`** — `(Dev)` tool that rebuilds `KitforgeDemoScene.unity` from `KitforgeRoot.prefab` + the three theme presets. Refuses to write into a read-only (Git-installed) package — embed the package first via `file:` in `manifest.json`.
- **`Editor/Maintenance/KitforgeCatalogScreenshotBaker.cs`** — `(Dev)` tool that bakes catalog PNGs into `Documentation~/Screenshots/` while the demo scene runs in Play Mode. Captures one frame per element, then dismisses and moves on.
- **Hub Setup Wizard "Recommended path" banner** — surfaces `Open Demo Scene` as the primary action above the 3-step blank-scene wizard. The wizard becomes the advanced path; the demo is the front door.
- **Spawn snippet enrichment** — the Hub Catalog tab now emits the full block: `using` imports for the manager and the element namespace, `[SerializeField] private XManager _xManager;` declaration, and the `Show<T>(new XData { ... });` call. Buyers paste once into a script and it compiles.

### Changed

- **`Editor/Maintenance/` asmdef** — `KitforgeLabs.UIKit.Editor.Maintenance.asmdef` introduced with `defineConstraints: ["KITFORGE_DEV_MAINTENANCE"]`. All kit-author tooling (Catalog prefabs regenerator, Wire-into-KitforgeRoot tool, Demo Scene baker, Screenshot baker, plus the internal `CatalogGroup{A,B,C,D,E}Builder` + `CatalogGroupBuilderShared`) now lives there and only compiles when the kit author defines `KITFORGE_DEV_MAINTENANCE` in Player Settings. Buyers no longer see `Maintenance/.../(Dev)` menus, internal builders or shared helpers. `Editor/Generators/` retains only `DefaultUIAnimPresetsCreator` (buyer-facing Bootstrap Defaults).
- **Hub Setup Wizard Step 3 ("Browse Catalog")** — now correctly marks Done once the buyer clicks the action button (persists via `EditorPrefs` key `kf.hub.setup.step3.catalog_visited`). `AllStepsDone()` now includes Step 3; the summary text updates accordingly. `IsStep1Done()` tightened from `>= 1` to `>= 10` so a single stray `UIAnimPreset` no longer false-positives.
- **README rewritten for v1.3.0** — hero banner image (`Documentation~/Screenshots/HeroBanner.png`), Click & Play quickstart reduced to 2 steps (install + Open Demo Scene), Non-goals moved to the end, added "What's in the box", "Catalog" (with required services column) and "Services" sections, install URL pinned to `#v1.3.0`.
- **Documentation~/CHEATSHEET.md "Quick start"** — rewritten around `Open Demo Scene` as Step 1, `Press Play` as Step 2, `Drag KitforgeRoot into your own scene` as Step 3.

### Removed

- **`Editor/Generators/CatalogGroup{A,B,C,D,E}Builder.cs` + `CatalogGroupBuilderShared.cs`** — moved (not deleted) to `Editor/Maintenance/`. CHANGELOG v1.1.0 had declared these "removed"; v1.2.0 silently restored them. v1.3.0 keeps them but isolates them behind the `KITFORGE_DEV_MAINTENANCE` define so buyers no longer see them in any code path.
- **`Editor/Generators/KitforgeCatalogPrefabsRegenerator.cs` + `KitforgeCatalogWireTool.cs`** — moved to `Editor/Maintenance/` for the same reason.

### Upgrade notes

- **From v1.2.x**: no buyer action required. The demo scene and demo services are additive; existing wired scenes still work as-is. If you upgraded via Package Manager with the version pinned to `#v1.2.0`, bump the manifest tag to `#v1.3.0`.
- **Kit author / contributor only**: to access the regenerate / wire / bake tools, add `KITFORGE_DEV_MAINTENANCE` to `Edit → Project Settings → Player → Other Settings → Scripting Define Symbols` in your host project.

## [1.2.0] — 2026-05-11

Catalog prefabs now ship with the package and the kit boots out-of-box on a clean install. Service contract reworked around Null Object defaults so buyers never see "service not registered" errors before wiring their own implementations. EditMode smoke test covers all 13 manager-driven catalog elements.

### Added

- **17 catalog prefabs shipped with the package** under `Runtime/Catalog/Prefabs/` (10 popups, 1 toast, 2 screens, 4 HUDs). The `KitforgeRoot.prefab` registries are pre-wired so spawning any catalog element via `PopupManager.Show<T>()`, `UIManager.Push<T>()` or `ToastManager.Show<T>()` works on first install with zero buyer setup.
- **Null Object service defaults** in `Runtime/Services/Null/`. `UIServices` now auto-instantiates `NullEconomyService`, `NullPlayerDataService`, `NullProgressionService`, `NullShopDataProvider`, `NullAdsService`, `NullTimeService`, `NullAudioRouter` and `NullLocalizationService` in `Awake` when the corresponding Inspector slot is empty. The kit always boots; buyers swap in production services via the Inspector when ready. A single warning logs at startup if any Null defaults are used.
- **`TryShow<T>(out T popup, out ShowFailureReason reason)` API** on `PopupManager` and `ToastManager`, plus `TryPush<T>(out T screen, out ShowFailureReason reason)` on `UIManager`. The new methods replace the ambiguous `null` return of legacy `Show<T>` / `Push<T>` with a structured failure reason (`PrefabMissing`, `Queued`, `InitializationFailed`, `ManagerUnavailable`). Legacy methods remain for backwards compatibility.
- **EditMode smoke test suite** at `Tests/Editor/Smoke/KitforgeRootSmokeTests.cs`. Spawns the `KitforgeRoot.prefab` in a temp scene and asserts every catalog popup / screen / toast opens cleanly with the Null Object defaults — no errors, no warnings. Gates v1.2.0+ releases against silent regressions.
- **Editor menus** `Tools/KitforgeLabs/Test/Regenerate Catalog Prefabs` and `... /Wire Catalog Into KitforgeRoot` for the package maintainer to rebuild and re-wire the catalog when the kit evolves. Both are dev-only (compile-gated by `KITFORGE_DEV_MAINTENANCE`) and refuse to write into a read-only (Git-installed) package — embed the package first.

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
