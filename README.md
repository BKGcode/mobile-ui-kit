# Kitforge Mobile UI Kit

> Opinionated UGUI router, popup queue and theme contract for hybrid-casual mobile games. Wire it in five minutes, skin it once, ship it.

`com.kitforgelabs.mobile-ui-kit` gives you the three pieces every hybrid-casual game re-implements badly:

- **`UIManager`** — stack-based screen flow (`Push` / `Pop` / `Replace` / `PopToRoot`) with prefab cache and registry validator.
- **`PopupManager`** — priority queue (`Meta < Gameplay < Modal`), eviction with state preservation, backdrop sync, depth cap.
- **`UIThemeConfig`** — one `ScriptableObject` to skin every screen and popup. Inspector preview included.

Everything else (router app states, service interfaces) is optional and lives behind opt-in samples.

---

## Status

**v1.0.0 — stable.** Phase 1 framework (UIManager / PopupManager / UIRouter / Theme) + Phase 2 catalog of 17 mid-core mobile UI elements (10 popups + 1 toast + 2 screens + 4 HUDs + 3 HUDSimple variants) + Kitforge Hub Editor window (`Tools → Kitforge → Hub`, UI Toolkit) shipped end-to-end. 303 EditMode tests + UIKit Audit 24/24 (17 prefabs + 7 scenes) green at release.

| Group | Elements | Status |
|---|---|---|
| **0 — Foundation** | ToastManager, UIServices, HUD layer, SafeArea, animation contract, validator | ✅ shipped at `v0.3.0-alpha` |
| **A — Pure UI** | ConfirmPopup, PausePopup, TutorialPopup, NotificationToast | ✅ shipped at `v0.4.0-alpha` (+ hotfix `v0.4.1-alpha`) — `Build Group A Sample` menu materializes the 4 prefabs + Demo scene |
| **B — Currency** | RewardPopup, ShopPopup, NotEnoughCurrencyPopup, HUD-Coins → `HUDCurrency`, HUD-Gems → `HUDCurrency` | ✅ shipped at `v0.5.0-alpha` — `Build Group B Sample` chain demo (Shop → NotEnough → Ad → Reward) playable in-Editor |
| **C — Progression** | DailyLogin, LevelComplete, GameOver, HUD-Energy, HUD-Timer | ✅ shipped at `v0.6.0-alpha` BREAKING — `IEconomyService` v2 + `HUDCurrency` parameterized + `IProgressionService` extended; 5 catalog elements + `RewardFlow.GrantAndShowSequence` helper; `Build Group C Sample` builder + 5 prefabs + demo scene + 6 chain `[ContextMenu]` scenarios + 2 sample stubs (`InMemoryProgressionService` + `InMemoryTimeService`). Buyers upgrading from `v0.5.0-alpha`: see [`CHANGELOG.md`](./CHANGELOG.md) § migration blocks 1-5. |
| **D — Player Data** | SettingsPopup | ✅ shipped at `v0.7.0-alpha` BREAKING — `IPlayerDataService` rewritten (12-method primitive surface) + new `IUILocalizationService` (re-skin dispatch) + `PlayerPrefsPlayerDataService` Runtime impl + `RewardFlow.GrantAndShow` (single, capability-gate re-audit promoted from Group C deferral) + DailyLogin persistence retro-fit via `DailyLoginPersistence` helper + Game Wiring sample revival (8 stubs + VContainer-gated asmdef). 8 canonical kit-side keys frozen at `v1.0.0-rc` (5 settings + 3 dailylogin) under `kfmui.<scope>.<name>` namespace. Buyers upgrading from `v0.6.0-alpha`: see [`CHANGELOG.md`](./CHANGELOG.md) § migration. |
| **E — Screens** | LoadingScreen, MainMenuScreen | ✅ shipped at `v0.8.0-alpha` BREAKING — `LoadingScreen` (buyer-driven `SetProgress`, optional spinner + bar, `OnProgressComplete` / `OnMinDisplayTimeElapsed` events) + `MainMenuScreen` (4 hide-able actions, daily-dot 30s poll, optional HUD child slots) + `UIAnimScreenBase` shared animator base + 8 new `UIThemeConfig` slots; `Build Group E Sample` builder + `GroupE_BootDemo.unity` chain (Loading → MainMenu → DailyLogin auto-trigger via `DailyLoginFlow.ShowIfDue`). Bundles **OnUpdate infra dispatch fix** — `PopupManager.Update()` now dispatches `OnUpdate()` to active popups (closes latent gap since `v0.1.0`); fixes `RewardPopup.AutoClaim` (silently broken since `v0.5.0`) + `DailyLoginPopup` countdown (workaround removed). |

Total tests: **303 EditMode** (Group + framework breakdown matches v0.8.0-alpha catalog baseline; +11 since rc.1 across post-tag fixes — DailyLogin/Tutorial mock factory + KitforgeRoot binder + Hub state). Public API frozen at `v1.0.0` — BREAKING entries in [`CHANGELOG.md`](./CHANGELOG.md) belong to pre-1.0 releases.

Latest tag: `v1.0.0` (2026-05-10 — first stable release). Promotion from `v1.0.0-rc.2` with no code, API, or sample changes — release-candidate cycle (rc.1 → rc.2 → 1.0.0) closed without further hotfixes. Promotion gate: 24/24 UIKit Audit (17 prefabs + 7 scenes) PASS + 303 EditMode tests green. Public API frozen; buyers upgrading from rc.2 have zero migration steps.

Previous tag: `v1.0.0-rc.2` (2026-05-10 — post-rc.1 hotfix release). Bundles 4 buyer-facing fixes from in-Editor "fresh install" smoke test + user-driven Regenerate+Audit: **Bug 2** ships `UIAnimPreset_Playful.asset` inside the package + pre-wires the 3 ship themes so OOTB popups animate without requiring `Bootstrap Defaults`; **Bug 4** populates `Catalog_All_Demo.ShowDailyLogin`'s 7-day calendar (no more `RewardEntries = null` console warning + auto-deactivation); **Bug 5** Catalog tab gains group-filter chips (`All / A / B / C / D / E`) + name search box + universal drag-drop preview (Popups/Screens/Toasts can now drop as static preview prefab for visual composition; HUDs unchanged — auto-bind to services on enable); **Bug 6** `CatalogAllDemoBuilder` now wires `UIManager._screenRoot` (pre-existing builder gap latent since rc.1 — `PushLoadingScreen` + `PushMainMenuScreen` would parent screens to scene root). Triple gate VERDE: 303/303 EditMode tests + UIKit Audit 24/24 (17 prefabs + 7 scenes) PASS + AllDemo Play smoke validated. Public API unchanged from rc.1; samples additive only.

Previous tag: `v1.0.0-rc.1` (2026-05-10 — first release candidate of v1.0). Hub 5/5 panes shipped (`Tools → Kitforge → Hub` — Setup wizard, Catalog browser, Theme Studio, Test launcher, inline Cheatsheet) via M5.1-M5.7. Theme presets (Default + Casual + Premium) ship under `Runtime/Theme/Presets/`. R-U-C3 HUDSimple variants (HUDSimple / HUDEnergySimple / HUDTimerSimple) coexist with the canonical service-bound HUDs for the no-services buyer path. Catalog_All_Demo single-import master demo (9th sample) wires every catalog element into one buttons-per-element scene with theme dropdown — workshop-drawer test pass. R-U-C5 multi-font + R-U-C7 SafeAreaFitter component INVALIDATED post-discovery (already shipped pre-M5.7). Public API frozen at v1.0.0-rc; only patch fixes between rc.N and final v1.0.0.

The path to `v1.0.0-rc` was re-sequenced 2026-05-06 per the rule "complete core packaging BEFORE auditing UX": auditing UX on incomplete packaging (no theme presets, no master demo, no MIGRATION) forces re-audit, so M4 ships hardening first, then M3c audits the completed core, then M5 dispositions findings + tags. M3b per-element polish is **deferred post-`v1.0.0-rc`** pending foundational review (wrong-premise patch-on-patch incident 2026-05-06; see [`CHANGELOG.md`](./CHANGELOG.md) `[0.8.1-alpha]` block).

---

## Non-goals

This package will **not** grow to cover the items below. If you need them, this is not the kit for you.

1. **No data binding framework.** No MVVM, no observable properties, no `INotifyPropertyChanged`. Modules expose `Bind(TData)` and you call it.
2. **No localization.** Bring your own (`I2 Localization`, Unity Localization, etc.). Modules accept already-resolved strings.
3. **No networking, save system or analytics.** Service interfaces in samples are stubs — your game implements them.
4. **No DI container in Runtime.** `KitforgeLabs.MobileUIKit` has zero dependency on VContainer / Zenject. DI is a sample, opt-in.
5. **No animation engine.** DOTween Pro is the assumed sequencer for module show/hide, but the package does not redistribute or wrap it.
6. **No visual screen-graph editor.** No node-based flow authoring, no drag-and-drop screen registry. Inspector + prefab references only — but the kit ships a single-front-door **Kitforge Hub** (`Tools → Kitforge → Hub`, UI Toolkit) covering Setup wizard / Catalog browser / Theme Studio / Test launcher / inline Cheatsheet. The Hub authors *scene state* (drop `KitforgeRoot.prefab`, scan themes) — not screen graphs.
7. **No UI Toolkit support.** UGUI only. UI Toolkit lives in a different problem space and a different (future) package.

---

## Install

### Prerequisites

| Dependency | Required by | How to install |
|---|---|---|
| Unity 6000.1 LTS or newer | Runtime | Editor |
| TextMeshPro | Runtime (`KitforgeLabs.MobileUIKit.asmdef` references it) | Package Manager (built-in) |
| DOTween Pro | Recommended for screen/popup show & hide animations | [Asset Store](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416) |
| UniTask | Recommended for `async` flows in your game code that drive `Push` / `Show` | [OpenUPM](https://openupm.com/packages/com.cysharp.unitask/) |
| VContainer | **Opt-in only.** Runtime asmdef does not reference it. Wire your container to `UIServices` setters at boot. | [OpenUPM](https://openupm.com/packages/jp.hadashikick.vcontainer/) |

### Add the package

Add to `Packages/manifest.json`:

```json
"com.kitforgelabs.mobile-ui-kit": "https://github.com/BKGcode/mobile-ui-kit.git#v1.0.0"
```

Or via Package Manager → **Add package from git URL…**

### Import a sample

In Package Manager, select **Kitforge Mobile UI Kit → Samples** and import:

- **Quickstart** — zero dependencies, two `[ContextMenu]` entries, ready to play.
- **Catalog — Group A — Pure UI** — Confirm / Pause / Tutorial / NotificationToast. After importing, run `Tools → Kitforge → UI Kit → Build Group A Sample` to materialize the 4 prefabs and a Demo scene under `Assets/Catalog_GroupA_Demo/`.
- **Catalog — Group B — Currency** — Reward / Shop / NotEnoughCurrency / HUDCoins / HUDGems + 3 in-memory stubs (Economy / Shop / Ads). After importing, run `Tools → Kitforge → UI Kit → Build Group B Sample` to materialize 5 prefabs + demo scene with a `Chain — Shop → NotEnough → Ad → Reward` `[ContextMenu]` trigger under `Assets/Catalog_GroupB_Demo/`.
- **Catalog — Group C — Progression** — DailyLogin / LevelComplete / GameOver / HUDEnergy / HUDTimer + 2 in-memory stubs (Time / Progression — wires into Group B's Economy / Shop / Ads). Requires Group B sample for the LevelComplete → Reward sequence chain. After importing, run `Tools → Kitforge → UI Kit → Build Group C Sample` to materialize 5 prefabs + demo scene with 6 chain `[ContextMenu]` scenarios under `Assets/Catalog_GroupC_Demo/`.
- **Catalog — Group D — Player Data** — SettingsPopup (5 controls: music / sfx volume sliders + language picker + notifications / haptics toggles) backed by `IPlayerDataService` for cross-session persistence + `IUILocalizationService` for live re-skin trigger + 2 in-memory stubs (`InMemoryPlayerDataService` + `InMemoryLocalizationService`). After importing, run `Tools → Kitforge → UI Kit → Build Group D Sample` to materialize the SettingsPopup prefab + demo scene with 9 `[ContextMenu]` scenarios under `Assets/Catalog_GroupD_Demo/`.
- **Catalog — Group E — Screens** — LoadingScreen + MainMenuScreen, the two entry-point screens of the boot flow. `GroupEDemoHost` drives the full chain: Loading (simulated progress) → Replace to MainMenu → DailyLogin auto-trigger via `DailyLoginFlow.ShowIfDue`. **Requires Group C sample** (DailyLoginPopup prefab + `InMemoryProgressionService`); the builder aborts with a clear LogError if Group C is not built first. After importing, run `Tools → Kitforge → UI Kit → Build Group E Sample` to materialize the 2 prefabs + `GroupE_BootDemo.unity` under `Assets/Catalog_GroupE_Demo/`.
- **Catalog — M4.1 — Theme Presets** — runtime dropdown reskinning `GameOver` + `LevelComplete` popups across the 3 themes shipped in the package (`Theme_Default` + `Theme_Casual` + `Theme_Premium` under `Runtime/Theme/Presets/`). **Requires Group B + Group C samples** (Economy / Ads / Time / Progression stubs + popup prefabs); the builder aborts with a clear LogError if either is missing. After importing, run `Tools → Kitforge → UI Kit → Build M4.1 — Theme Presets` to generate `ThemePresetsDemo.unity` under `Assets/Catalog_M4_ThemePresets_Demo/` wired to the package presets — no `.asset` cloning at build time.
- **Catalog — All — Single-import master demo** — workshop-drawer demo: ONE scene wiring every catalog popup (10 + 1 toast + 2 screens) with buttons-per-element + Theme dropdown re-skinning live. **Requires the 5 catalog group samples + M4.1 Theme Presets imported and built first** — the builder aborts with a missing-dependency list. After all prerequisites are built, run `Tools → Kitforge → UI Kit → Build Catalog_All_Demo` to materialize `Assets/Catalog_All_Demo/AllDemo.unity` — open the scene, press Play, click any button to spawn the corresponding element. Demonstrates the canonical `_popupManager.Show<T>(dto)` / `_uiManager.Push<T>(dto)` / `_toastManager.Show<T>(dto)` pattern documented in Quickstart §Manual prefab registration.
- **Game Wiring (VContainer)** — VContainer `LifetimeScope` + 8 in-memory `Stub*` impls of every kit service interface. Reference for VContainer-first host projects. Asmdef gated behind `KFMUI_HAS_VCONTAINER` define — silently excluded if VContainer is not installed in the host project.

---

## Quickstart (5 steps)

1. **Bootstrap the defaults**: top menu → **Tools → Kitforge → UI Kit → Bootstrap Defaults**. Generates 10 `UIAnimPreset` SOs at `Assets/Settings/UIAnimPresets/`. The package itself ships `Theme_Default.asset` at `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset` — no buyer-side Theme generation required.
2. **Drop `KitforgeRoot.prefab` into your scene**: drag `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab` into the Hierarchy. The prefab ships pre-wired with `UIManager` + `PopupManager` + `ToastManager` + `UIServices` + `Canvas` + 3 RectTransform roots + `EventSystem` + a global `PopupBackdrop`. The `KitforgeThemeBinder` component on the root distributes `Theme_Default` to all 3 managers in `Awake` via a single Inspector field — swap themes by changing `_theme`, never touch the manager fields.
3. **Import the `Quickstart` sample** from Package Manager. Files land under `Assets/Samples/Kitforge Mobile UI Kit/<version>/Quickstart/`.
4. **Register your prefabs**: select the `UIManager` / `PopupManager` GameObjects under `KitforgeRoot` and assign your screen / popup prefabs (deriving from `UIModule<TData>` or `UIModuleBase`) to the `_screenPrefabs` / `_popupPrefabs` arrays.
5. **Press Play**, then right-click your bootstrap host in the Hierarchy → run `Push <YourScreen>`, `Show <YourPopup>`, `Pop`. Watch the stack and popup queue react.

---

## Phase 1 — done criteria

The work below is complete. Tick what's shipped, not what's planned.

- [x] `UIManager`: `Push<T>` / `Pop` / `Replace<T>` / `PopToRoot`, prefab cache, registry validator, `OnDestroy` cleanup.
- [x] `PopupManager`: priority queue (`Meta < Gameplay < Modal`), eviction with `Data` preserved, backdrop sync, sibling-order sync, `MaxDepth = 3`, `OnDestroy` cleanup.
- [x] `UIRouter`: `AppState` transitions with re-entrancy guard, popup allow-list with explicit flag, back-button dispatch.
- [x] `UIThemeConfig` ScriptableObject + custom Editor with color preview.
- [x] `UIModuleBase` + generic `UIModule<TData>` with `protected internal` `BindUntyped` (host assemblies can derive directly).
- [x] 8 service interfaces (`IEconomyService`, `IPlayerDataService`, `IProgressionService`, `IShopDataProvider`, `IAdsService`, `ITimeService`, `IUIAudioRouter`, `IUILocalizationService`) — `IUIAudioRouter` shipped Group 0, `IUILocalizationService` shipped M2.
- [x] `Samples~/Quickstart`: zero-dependency bootstrap.
- [x] `package.json` + `README.md` aligned with PM checker FIX NOW list.
- [x] EditMode tests — **Phase 1.5 closed**: 10 `UIRouter` + 11 `PopupManager` + 12 `UIManager` = **33 tests**, all green. Tagged `v0.2.0-alpha` (suite) → `v0.2.1-alpha` (cleanup: `UIRouter.Initialize()` + arch decision #10).

---

## Architecture decisions

| # | Decision | Rationale | Implication |
|---|---|---|---|
| 1 | Runtime asmdef has **zero DI dependency** | Buyers without VContainer must boot in one step. | Service binding lives in `UIServices` MonoBehaviour container (Inspector setters); DI users wrap their container resolution into those setters at boot. |
| 2 | `UIManager` is a `MonoBehaviour`, not a singleton | UNITY_RULES forbid singletons; one scene = one manager via Inspector references. | Multiple `UIManager` instances per scene are valid (rare but supported). |
| 3 | Screens registered as a flat **`UIModuleBase[]` registry**, resolved by `Type` | Keeps Inspector authoring trivial; no `[CreateAssetMenu]` graph asset to maintain. | One prefab per concrete `UIModule` subclass. Duplicate types caught by `ValidateRegistry`. |
| 4 | Popup priority is an `enum`, not a numeric weight | Three buckets (Meta / Gameplay / Modal) cover every hybrid-casual case we surveyed. | Adding a fourth bucket is a breaking change — done deliberately. |
| 5 | `PopupRecord` stores `Data` so eviction can re-enqueue with state | A modal evicting a Gameplay popup must restore the gameplay popup with its original payload. | `PopupRecord` is a `struct` to avoid GC churn; size kept ≤ 32 bytes. |
| 6 | `_pendingQueue` is a `List<PopupRecord>` with insertion sort, not `PriorityQueue<>` | List churn is bounded by `MaxDepth = 3`; `PriorityQueue<>` adds an allocation per `Show`. | Promoted to `PriorityQueue<>` only if a future module justifies it. |
| 7 | `ChangeLog` ships **inside** the UPM package | Buyer reads release notes from Package Manager UI. | Studio-side `_Develop/KF_*_dev/ChangeLog.md` is for internal session log only. |
| 8 | VContainer is **opt-in**, never a Runtime reference | Avoids forcing a DI container on buyers who use Zenject, no DI, or their own injector. | DI users wire their container into the `UIServices` MonoBehaviour setters at boot. A VContainer-driven `GameWiring` sample is parked under `Samples~/` (not exposed in Package Manager) until service contracts stabilize at the end of Group D — see CHANGELOG `[0.4.1-alpha]`. |
| 9 | No comments, ≤ 15-line methods, `var` everywhere | UNITY_RULES, applied without exception. | Readability via naming, not prose. |
| 10 | `BindUntyped` is `protected override` (not `protected internal`) when host assemblies derive `UIModule<TData>` | Cross-assembly subclasses cannot widen accessibility from `protected internal` — the C# compiler emits CS0507. Buyers deriving in their own asmdef would hit this on first compile. | The base method stays `protected internal` so internal samples can call it; derivations declare `protected override`. Validated by `Samples~/Quickstart` and EditMode test fakes. |
| 11 | A single `UIThemeConfig` asset feeds **all three managers** (`UIManager._themeConfig`, `PopupManager._themeConfig`, `ToastManager._themeConfig`) | "Skin it once" is the kit's pitch. Multiple Theme assets per scene = inconsistent visuals across screens / popups / toasts; defeats the value proposition. | Buyer assigns the same `UIThemeConfig_Default` ref into all three managers' Inspector slots. Bootstrap Defaults menu auto-creates this asset; the demo scene built by `Build Group A Sample` wires it automatically. |

---

## License

Proprietary — Kitforge Labs. See `LICENSE.md` (TBD before public release).

---

## Changelog

See [`CHANGELOG.md`](./CHANGELOG.md). Versioning follows [SemVer](https://semver.org/).
