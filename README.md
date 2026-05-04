# Kitforge Mobile UI Kit

> Opinionated UGUI router, popup queue and theme contract for hybrid-casual mobile games. Wire it in five minutes, skin it once, ship it.

`com.kitforgelabs.mobile-ui-kit` gives you the three pieces every hybrid-casual game re-implements badly:

- **`UIManager`** — stack-based screen flow (`Push` / `Pop` / `Replace` / `PopToRoot`) with prefab cache and registry validator.
- **`PopupManager`** — priority queue (`Meta < Gameplay < Modal`), eviction with state preservation, backdrop sync, depth cap.
- **`UIThemeConfig`** — one `ScriptableObject` to skin every screen and popup. Inspector preview included.

Everything else (router app states, service interfaces) is optional and lives behind opt-in samples.

---

## Status

**Phase 2 in progress — alpha.** Phase 1 framework (UIManager / PopupManager / UIRouter / Theme) shipped at `v0.2.1-alpha` with 33 EditMode tests. Phase 2 adds the catalog of ready-to-use mid-core mobile UI elements promised by the kit name — 15 elements grouped by contract similarity, delivered group-by-group.

| Group | Elements | Status |
|---|---|---|
| **0 — Foundation** | ToastManager, UIServices, HUD layer, SafeArea, animation contract, validator | ✅ shipped at `v0.3.0-alpha` |
| **A — Pure UI** | ConfirmPopup, PausePopup, TutorialPopup, NotificationToast | ✅ shipped at `v0.4.0-alpha` (+ hotfix `v0.4.1-alpha`) — `Build Group A Sample` menu materializes the 4 prefabs + Demo scene |
| **B — Currency** | RewardPopup, ShopPopup, NotEnoughCurrencyPopup, HUD-Coins → `HUDCurrency`, HUD-Gems → `HUDCurrency` | ✅ shipped at `v0.5.0-alpha` — `Build Group B Sample` chain demo (Shop → NotEnough → Ad → Reward) playable in-Editor |
| **C — Progression** | DailyLogin, LevelComplete, GameOver, HUD-Energy, HUD-Timer | ✅ shipped at `v0.6.0-alpha` BREAKING — `IEconomyService` v2 + `HUDCurrency` parameterized + `IProgressionService` extended; 5 catalog elements + `RewardFlow.GrantAndShowSequence` helper; `Build Group C Sample` builder + 5 prefabs + demo scene + 6 chain `[ContextMenu]` scenarios + 2 sample stubs (`InMemoryProgressionService` + `InMemoryTimeService`). Buyers upgrading from `v0.5.0-alpha`: see [`CHANGELOG.md`](./CHANGELOG.md) § migration blocks 1-5. |
| **D — Player Data** | SettingsPopup | ✅ shipped at `v0.7.0-alpha` BREAKING — `IPlayerDataService` rewritten (12-method primitive surface) + new `IUILocalizationService` (re-skin dispatch) + `PlayerPrefsPlayerDataService` Runtime impl + `RewardFlow.GrantAndShow` (single, capability-gate re-audit promoted from Group C deferral) + DailyLogin persistence retro-fit via `DailyLoginPersistence` helper + Game Wiring sample revival (8 stubs + VContainer-gated asmdef). 8 canonical kit-side keys frozen at `v1.0.0-rc` (5 settings + 3 dailylogin) under `kfmui.<scope>.<name>` namespace. Buyers upgrading from `v0.6.0-alpha`: see [`CHANGELOG.md`](./CHANGELOG.md) § migration. |
| **E — Screens** | LoadingScreen, MainMenuScreen | ⏳ tag `v0.8.0-alpha` BREAKING (bundles OnUpdate infra dispatch fix) |

Total tests: **261 EditMode** (33 framework + 228 catalog: Group A 53 + Group B 33 + DailyLogin 26 + LevelComplete 16 + GameOver 18 + HUD-Energy 10 + HUD-Timer 10 + RewardFlow 12 + IPlayerDataService 16 + IUILocalizationService 10 + SettingsPopup 19 + DailyLoginPersistence 5). Public API may still move before `v1.0.0` — track BREAKING entries in [`CHANGELOG.md`](./CHANGELOG.md).

Latest tag: `v0.7.0-alpha` BREAKING (Group D player data catalog — closes M2). Next: `v0.8.0-alpha` BREAKING (Group E — LoadingScreen + MainMenuScreen + OnUpdate infra dispatch fix).

After Group E ships, the package enters a hardening + documentation pass (Theme presets, severity icons, performance benchmarks, hero screenshots, cumulative migration guide, API freeze) and reaches **`v1.0.0-rc`** — Asset Store release candidate. See [`CHANGELOG.md`](./CHANGELOG.md) § "Pending" for the running list.

---

## Non-goals

This package will **not** grow to cover the items below. If you need them, this is not the kit for you.

1. **No data binding framework.** No MVVM, no observable properties, no `INotifyPropertyChanged`. Modules expose `Bind(TData)` and you call it.
2. **No localization.** Bring your own (`I2 Localization`, Unity Localization, etc.). Modules accept already-resolved strings.
3. **No networking, save system or analytics.** Service interfaces in samples are stubs — your game implements them.
4. **No DI container in Runtime.** `KitforgeLabs.MobileUIKit` has zero dependency on VContainer / Zenject. DI is a sample, opt-in.
5. **No animation engine.** DOTween Pro is the assumed sequencer for module show/hide, but the package does not redistribute or wrap it.
6. **No Editor authoring window.** No visual screen graph, no drag-and-drop registry. Inspector + prefab references only.
7. **No automatic safe-area / notch handling beyond `RectTransform` anchors.** Use a dedicated package for that.
8. **No UI Toolkit support.** UGUI only. UI Toolkit lives in a different problem space and a different (future) package.

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
"com.kitforgelabs.mobile-ui-kit": "https://github.com/BKGcode/mobile-ui-kit.git#v0.7.0-alpha"
```

Or via Package Manager → **Add package from git URL…**

### Import a sample

In Package Manager, select **Kitforge Mobile UI Kit → Samples** and import:

- **Quickstart** — zero dependencies, two `[ContextMenu]` entries, ready to play.
- **Catalog — Group A — Pure UI** — Confirm / Pause / Tutorial / NotificationToast. After importing, run `Tools → Kitforge → UI Kit → Build Group A Sample` to materialize the 4 prefabs and a Demo scene under `Assets/Catalog_GroupA_Demo/`.
- **Catalog — Group B — Currency** — Reward / Shop / NotEnoughCurrency / HUDCoins / HUDGems + 3 in-memory stubs (Economy / Shop / Ads). After importing, run `Tools → Kitforge → UI Kit → Build Group B Sample` to materialize 5 prefabs + demo scene with a `Chain — Shop → NotEnough → Ad → Reward` `[ContextMenu]` trigger under `Assets/Catalog_GroupB_Demo/`.
- **Catalog — Group C — Progression** — DailyLogin / LevelComplete / GameOver / HUDEnergy / HUDTimer + 2 in-memory stubs (Time / Progression — wires into Group B's Economy / Shop / Ads). Requires Group B sample for the LevelComplete → Reward sequence chain. After importing, run `Tools → Kitforge → UI Kit → Build Group C Sample` to materialize 5 prefabs + demo scene with 6 chain `[ContextMenu]` scenarios under `Assets/Catalog_GroupC_Demo/`.
- **Catalog — Group D — Player Data** — SettingsPopup (5 controls: music / sfx volume sliders + language picker + notifications / haptics toggles) backed by `IPlayerDataService` for cross-session persistence + `IUILocalizationService` for live re-skin trigger + 2 in-memory stubs (`InMemoryPlayerDataService` + `InMemoryLocalizationService`). After importing, run `Tools → Kitforge → UI Kit → Build Group D Sample` to materialize the SettingsPopup prefab + demo scene with 9 `[ContextMenu]` scenarios under `Assets/Catalog_GroupD_Demo/`.
- **Game Wiring (VContainer)** — VContainer `LifetimeScope` + 8 in-memory `Stub*` impls of every kit service interface. Reference for VContainer-first host projects. Asmdef gated behind `KFMUI_HAS_VCONTAINER` define — silently excluded if VContainer is not installed in the host project.

---

## Quickstart (5 steps)

1. **Bootstrap the defaults**: top menu → **Tools → Kitforge → UI Kit → Bootstrap Defaults**. Generates 10 `UIAnimPreset` SOs at `Assets/Settings/UIAnimPresets/` and a pre-wired `UIThemeConfig_Default.asset` at `Assets/Settings/UI/`. The Theme is auto-selected and pinged when the dialog closes.
2. **Import the `Quickstart` sample** from Package Manager. Files land under `Assets/Samples/Kitforge Mobile UI Kit/<version>/Quickstart/`.
3. **Set up the scene**: open the `Quickstart` README and follow scene-setup — one `Canvas`, two empty `RectTransform` roots (`ScreenRoot`, `PopupRoot`), drop the `QuickstartBootstrap` prefab.
4. **Wire the Theme**: drag `UIThemeConfig_Default` into `UIManager._themeConfig` and `PopupManager._themeConfig`. Create one screen prefab and one popup prefab (deriving from `UIModule<TData>` or `UIModuleBase`) and assign them to the bootstrap arrays.
5. **Press Play**, then right-click `QuickstartBootstrap` in the Hierarchy → run `Push Quickstart Screen`, `Show Quickstart Popup`, `Pop Screen`. Watch the stack and popup queue react.

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
