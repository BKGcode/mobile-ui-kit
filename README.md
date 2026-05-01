# Kitforge Mobile UI Kit

> Opinionated UGUI router, popup queue and theme contract for hybrid-casual mobile games. Wire it in five minutes, skin it once, ship it.

`com.kitforgelabs.mobile-ui-kit` gives you the three pieces every hybrid-casual game re-implements badly:

- **`UIManager`** — stack-based screen flow (`Push` / `Pop` / `Replace` / `PopToRoot`) with prefab cache and registry validator.
- **`PopupManager`** — priority queue (`Meta < Gameplay < Modal`), eviction with state preservation, backdrop sync, depth cap.
- **`UIThemeConfig`** — one `ScriptableObject` to skin every screen and popup. Inspector preview included.

Everything else (router app states, service interfaces) is optional and lives behind opt-in samples.

---

## Status

**Phase 1 — alpha.** Core managers, theme and routing primitives shipped. Not yet hardened with tests. Public API may move before `v0.2.0`.

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
| VContainer | **Opt-in only.** Required by `Samples~/GameWiring`. Runtime asmdef does not reference it. | [OpenUPM](https://openupm.com/packages/jp.hadashikick.vcontainer/) |

### Add the package

Add to `Packages/manifest.json`:

```json
"com.kitforgelabs.mobile-ui-kit": "https://github.com/BKGcode/com.kitforgelabs.mobile-ui-kit.git#v0.1.0-alpha"
```

Or via Package Manager → **Add package from git URL…**

### Import a sample

In Package Manager, select **Kitforge Mobile UI Kit → Samples** and import:

- **Quickstart** — zero dependencies, two `[ContextMenu]` entries, ready to play.
- **Game Wiring** — VContainer `LifetimeScope` + 6 service stubs.

---

## Quickstart (5 steps)

1. **Import the `Quickstart` sample** from Package Manager. Files land under `Assets/Samples/Kitforge Mobile UI Kit/0.1.0/Quickstart/`.
2. **Open the `Quickstart` sample README** and follow the scene-setup section: create one `Canvas`, two empty `RectTransform` roots (`ScreenRoot`, `PopupRoot`), drop the `QuickstartBootstrap` prefab.
3. **Create one screen prefab and one popup prefab.** Each is a `MonoBehaviour` deriving from `UIModule<TData>` (or `UIModuleBase`), placed under the matching root. Assign them to the `_screenPrefabs` / `_popupPrefabs` arrays on the bootstrap.
4. **Assign a `UIThemeConfig` asset** to `UIManager._themeConfig`. Right-click in Project → **Create → Kitforge Labs → UI Theme Config**.
5. **Press Play, then right-click `QuickstartBootstrap` in the Hierarchy** → run `Push Quickstart Screen`, `Show Quickstart Popup`, `Pop Screen`. Watch the stack and popup queue react.

---

## Phase 1 — done criteria

The work below is complete. Tick what's shipped, not what's planned.

- [x] `UIManager`: `Push<T>` / `Pop` / `Replace<T>` / `PopToRoot`, prefab cache, registry validator, `OnDestroy` cleanup.
- [x] `PopupManager`: priority queue (`Meta < Gameplay < Modal`), eviction with `Data` preserved, backdrop sync, sibling-order sync, `MaxDepth = 3`, `OnDestroy` cleanup.
- [x] `UIRouter`: `AppState` transitions with re-entrancy guard, popup allow-list with explicit flag, back-button dispatch.
- [x] `UIThemeConfig` ScriptableObject + custom Editor with color preview.
- [x] `UIModuleBase` + generic `UIModule<TData>` with `protected internal` `BindUntyped` (host assemblies can derive directly).
- [x] 6 service interfaces (`IEconomyService`, `IPlayerDataService`, `IProgressionService`, `IShopDataProvider`, `IAdsService`, `ITimeService`) + 4 DTOs.
- [x] `Samples~/GameWiring`: VContainer `LifetimeScope` + stub services.
- [x] `Samples~/Quickstart`: zero-dependency bootstrap.
- [x] `package.json` + `README.md` aligned with PM checker FIX NOW list.
- [ ] EditMode tests (`UIRouter.TransitionTo`, `PopupManager` priority ordering) — **deferred to Phase 1.5**.

---

## Architecture decisions

| # | Decision | Rationale | Implication |
|---|---|---|---|
| 1 | Runtime asmdef has **zero DI dependency** | Buyers without VContainer must boot in one step. | DI lives in `Samples~/GameWiring`, not in `Runtime/`. |
| 2 | `UIManager` is a `MonoBehaviour`, not a singleton | UNITY_RULES forbid singletons; one scene = one manager via Inspector references. | Multiple `UIManager` instances per scene are valid (rare but supported). |
| 3 | Screens registered as a flat **`UIModuleBase[]` registry**, resolved by `Type` | Keeps Inspector authoring trivial; no `[CreateAssetMenu]` graph asset to maintain. | One prefab per concrete `UIModule` subclass. Duplicate types caught by `ValidateRegistry`. |
| 4 | Popup priority is an `enum`, not a numeric weight | Three buckets (Meta / Gameplay / Modal) cover every hybrid-casual case we surveyed. | Adding a fourth bucket is a breaking change — done deliberately. |
| 5 | `PopupRecord` stores `Data` so eviction can re-enqueue with state | A modal evicting a Gameplay popup must restore the gameplay popup with its original payload. | `PopupRecord` is a `struct` to avoid GC churn; size kept ≤ 32 bytes. |
| 6 | `_pendingQueue` is a `List<PopupRecord>` with insertion sort, not `PriorityQueue<>` | List churn is bounded by `MaxDepth = 3`; `PriorityQueue<>` adds an allocation per `Show`. | Promoted to `PriorityQueue<>` only if a future module justifies it. |
| 7 | `ChangeLog` ships **inside** the UPM package | Buyer reads release notes from Package Manager UI. | Studio-side `_Develop/KF_*_dev/ChangeLog.md` is for internal session log only. |
| 8 | VContainer is **opt-in via Sample**, never a Runtime reference | Avoids forcing a DI container on buyers who use Zenject, no DI, or their own injector. | Two samples coexist: `Quickstart` (no DI) and `GameWiring` (VContainer). |
| 9 | No comments, ≤ 15-line methods, `var` everywhere | UNITY_RULES, applied without exception. | Readability via naming, not prose. |
| 10 | `BindUntyped` is `protected override` (not `protected internal`) when host assemblies derive `UIModule<TData>` | Cross-assembly subclasses cannot widen accessibility from `protected internal` — the C# compiler emits CS0507. Buyers deriving in their own asmdef would hit this on first compile. | The base method stays `protected internal` so internal samples can call it; derivations declare `protected override`. Validated by `Samples~/Quickstart` and EditMode test fakes. |

---

## License

Proprietary — Kitforge Labs. See `LICENSE.md` (TBD before public release).

---

## Changelog

See [`CHANGELOG.md`](./CHANGELOG.md). Versioning follows [SemVer](https://semver.org/).
