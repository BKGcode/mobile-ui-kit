# LoadingScreen

> Status: **SPEC DRAFT 2026-05-05** — L1-L8 locked. Targets M3a (`v0.8.0-alpha` BREAKING). Element 12 (Screen) of Group E.
> Infrastructure prerequisites: `CATALOG_GroupE_DELTA.md` § 1 (Theme slots) + § 2 (folder convention).

## Purpose

Full-screen loading overlay with optional progress bar and spinner. Shown during initial app boot (asset loading, service init) and between-level transitions. Buyer drives progress externally via `SetProgress(float)`; screen never auto-dismisses. Async-agnostic by design — works with UniTask, coroutines, or plain callbacks.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| **L1** | Progress update mechanism | **LOCKED — buyer-driven pull**: screen exposes `public void SetProgress(float 0-1)`. DTO carries initial state (`InitialProgress`, `ShowProgressBar`). Screen does NOT poll, own a task, or drive async internally. Buyer calls `SetProgress` from their boot sequence and calls `UIManager.Replace<MainMenuScreen>()` when done. | CATALOG § 3.1.3 typed payload + § 3.2.5 no PlayerPrefs/Save system = kit doesn't own boot logic. Progress is a game-specific async pipeline; kit provides the visual only. Any intrinsic async design (Func<float> in DTO, IProgress<float>, coroutine) forces a coupling to the buyer's async model. |
| **L2** | Auto-dismiss | **LOCKED — none.** Screen has no auto-dismiss. Buyer calls `UIManager.Replace<MainMenuScreen>(data)` when ready. Optional `OnProgressComplete` C# event fires when `SetProgress(1f)` is called — buyer can wire to transition if desired. | Buyer's loading pipeline may have post-progress work (fade-in, cinematic, startup validation) before showing MainMenu. Auto-dismiss would race those. Decoupled by default; wiring is 1 line for buyers who want it. |
| **L3** | Minimum display time | **LOCKED — advisory via event**: `_minDisplaySeconds` Inspector field (default `0f`). When > 0, fires `OnMinDisplayTimeElapsed` after `Time.unscaledTime` delta exceeds the threshold post-`OnShow`. Screen does NOT block transition. Buyer who requires minimum time waits for both `OnProgressComplete` AND `OnMinDisplayTimeElapsed` in their host. `ITimeService` NOT required — `Time.unscaledTime` suffices (loading screen is a local experience, not server-time-dependent). | Advisory is correct: kit should not block `UIManager.Replace` calls. Mobile loading screens have OS minimums (Apple App Store Review Guidelines 2.3.10) but enforcement is the game binary's concern, not a UI kit component. |
| **L4** | Spinner | **LOCKED — optional continuous rotation tween.** `Refs.SpinnerImage` (Image). `_spinnerRpm` Inspector field (default `120f`). Tween stored in `_spinnerTween`. Started in `OnShow`, killed in `OnHide` + `OnDestroy`. `SetLink(gameObject)`. Hidden when `Refs.SpinnerImage` null. Uses `Theme.IconLoading` sprite (NEW slot per `CATALOG_GroupE_DELTA.md` § 1). | Standard mobile loading pattern. Rotation tween is 1 line; no dedicated animator needed. `_spinnerRpm` lets buyer tune perceived speed without code changes. |
| **L5** | Progress bar animation | **LOCKED — inline DOTween on `fillAmount`.** `_progressTweenDuration` Inspector field (default `0.3f`). Stored in `_progressTween`. Killed before re-fire. `SetLink(gameObject)`. `_animateProgress` bool (default `true`) — false = snap instantly (useful for pre-positioned loading bars). Fill color via `Theme.LoadingBarColor` (NEW slot). | Smooth fill conveys "loading is progressing" better than jumpy snaps. DOTween handles perfectly; no reason for a separate animator component just for fillAmount. |
| **L6** | Back button | **LOCKED — blocked.** `OnBackPressed()` override is a no-op. No dismiss. No event. | Loading is not cancelable from the player's perspective. Android back press during boot would crash to OS; this is intentional behavior. Buyer who needs cancelable loading (e.g. PvP matchmaking lobby) overrides via subclass. |
| **L7** | Services | **LOCKED — none required.** `IUIAudioRouter` optional: cue `UIAudioCue.Ambient` on `OnShow` (ambient loading music), stop on `OnHide`. No `IEconomyService`, `IProgressionService`, or `ITimeService` consumed. | CATALOG § 3.2 — emit events; don't own services you don't need. Boot is pre-services-ready in many architectures; requiring services on the loading screen would create a chicken-and-egg dependency. |
| **L8** | Subtitle + live text updates | **LOCKED — DTO-initial + public setter.** `LoadingScreenData.Subtitle` sets the subtitle on Bind. `public void SetLoadingText(string title, string subtitle)` lets buyer update labels during load ("Connecting...", "Authenticating...", "Loading assets..."). Smooth label cross-fade NOT included (over-engineering for MVP; buyer adds via subclass). | Live text status is a common loading pattern (Hearthstone, League). Single public setter covers the case without DTO mutation. |

## DTO

`LoadingScreenData` (`class`, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Loading..."` | Top label. Empty string = hidden. |
| `Subtitle` | `string` | `""` | Secondary label for status text. Empty = hidden. |
| `InitialProgress` | `float` | `0f` | Clamped `[0, 1]`. Progress bar snaps to this value on Bind without tween. |
| `ShowProgressBar` | `bool` | `true` | Hides the progress bar Image entirely when false. |
| `ShowSpinner` | `bool` | `true` | Hides the spinner Image when false. |
| `MinDisplaySeconds` | `float` | `0f` | If > 0, fires `OnMinDisplayTimeElapsed` after this many seconds post-Show. Clamped ≥ 0. |

`Bind(null)` → fresh `LoadingScreenData()` (progress bar visible, spinner visible, no min time).

## Service binding

- `IUIAudioRouter` (optional). Cue: `UIAudioCue.Ambient` on `OnShow`, stop on `OnHide`. Off by default if router null.
- All other services: **none consumed**. Null `Services` ref is silently tolerated.

Null-service behavior follows `CATALOG_GroupE_DELTA.md` § 4 (Screen = silent degrade).

## Events emitted

| Event | When |
|---|---|
| `OnProgressComplete` (C# `event Action`) | `SetProgress(1f)` called (progress first reaches 1.0; does not re-fire on subsequent calls at 1.0). |
| `OnMinDisplayTimeElapsed` (C# `event Action`) | `MinDisplaySeconds > 0` and `Time.unscaledTime - _showTime >= MinDisplaySeconds`. Fires at most once per Show. |

Both events reset on `Bind(...)`.

## Animation contract

- `[RequireComponent(typeof(UIAnim_LoadingScreen))]`. Animator auto-resolved in `Awake`.
- **Show**: instant cut-in by default (loading screens replace immediately — no entry animation delay). `UIAnim_LoadingScreen` may add a brief fade-in if buyer configures it.
- **Hide**: short fade-out (buyer wires `UIManager.Replace` → UIManager calls `OnHide` → animator fade-out runs before next screen enters). Duration: 0.2s default.
- **Progress bar fill**: inline DOTween, NOT in UIAnim (data-driven visual per L5).
- **Spinner rotation**: inline DOTween loop, NOT in UIAnim (per L4).
- `OnHide` kills `_progressTween` + `_spinnerTween` before animator runs.

## Theme tokens consumed

- `BackgroundMainMenu` — NOT used by LoadingScreen (separate BG). LoadingScreen uses `PanelColor` as full-screen background.
- `IconLoading` (NEW) — spinner sprite via `ThemedImage` on `Refs.SpinnerImage`.
- `LoadingBarColor` (NEW) — progress bar fill color.
- `MutedColor` — progress bar track (empty portion).
- `TextPrimary` — title label.
- `TextSecondary` — subtitle label.
- `FontDisplay` — title.
- `FontBody` — subtitle.

## Edge cases

- **`SetProgress` called before `OnShow`**: value stored, applied on next `Bind`. No NRE.
- **`SetProgress` called after `OnHide`**: silently ignored (guard `_isShowing` flag). No tween fired on hidden object.
- **`SetProgress(1f)` then `SetProgress(0f)` (reset)**: `OnProgressComplete` fires on first 1f call. Reset does NOT re-arm `OnProgressComplete` (events reset only on `Bind`).
- **`MinDisplaySeconds` elapsed before `SetProgress(1f)`**: both events fire independently in their respective order. Host waits for both.
- **`LoadingScreenData.InitialProgress > 1f`**: clamped to 1f, no error.
- **`ShowProgressBar = false`**: `Refs.ProgressBarFill` and `Refs.ProgressBarTrack` hidden. `SetProgress` still works (internal value tracked; events still fire).
- **`SetLoadingText` while hidden**: stored, applied on next `OnShow`. Not silently discarded.
- **Multiple rapid `SetProgress` calls**: `_progressTween` killed + restarted each time. `SetLink` handles orphan cleanup.
- **UIManager.Replace called before spinner created**: `OnHide` kills null safely (`?.Kill()`).
- **`OnDestroy` during tween**: `SetLink(gameObject)` cleans automatically; explicit kill in `OnDestroy` is belt-and-braces.

## QA scenarios

Triggered from `GroupEDemoHost` via `[ContextMenu]`:

1. `Show LoadingScreen — default DTO` (progress bar + spinner visible, "Loading..." title).
2. `Show LoadingScreen — no progress bar` (`ShowProgressBar=false`, spinner only).
3. `Show LoadingScreen — no spinner` (`ShowSpinner=false`, progress bar only).
4. `Animate progress 0 → 1` (call `SetProgress` incrementally via coroutine, verify fill tween).
5. `SetProgress(1f)` — verify `OnProgressComplete` fires.
6. `MinDisplaySeconds=2f` — verify `OnMinDisplayTimeElapsed` fires at ~2s, not immediately.
7. `Live text update` — call `SetLoadingText("Connecting...", "Please wait")` mid-load.
8. `Back press during loading` — verify no dismiss.
9. `Replace → MainMenuScreen` — verify hide animation + transition.
10. `Disable + re-enable` — verify spinner restarts, progress resets.
11. `Null UIAudioRouter` — verify no NRE, loading works silently.

## Tests (≥12, EditMode)

- DTO defaults (1).
- `Bind` sets initial progress on bar (1).
- `Bind(null)` falls back to defaults (1).
- `SetProgress(0.5f)` updates internal value (1).
- `SetProgress(1f)` fires `OnProgressComplete` (1).
- `SetProgress(1f)` twice does NOT re-fire `OnProgressComplete` (1).
- `OnProgressComplete` resets on `Bind` (1).
- `MinDisplaySeconds=0` does NOT fire `OnMinDisplayTimeElapsed` (1).
- `MinDisplaySeconds>0` fires `OnMinDisplayTimeElapsed` after elapsed time (1 — via `RealTimeProviderForTests` injection).
- `SetProgress` while hidden is ignored (no tween, no NRE) (1).
- `OnBackPressed` is a no-op (1).
- `ShowProgressBar=false` hides bar elements (1).

## Out of scope

- Built-in async boot orchestration (buyer owns loading pipeline).
- Multiple sub-progress tasks with labels (buyer tracks individual task progress externally).
- Transition animations between multiple loading stages (buyer uses `SetLoadingText`).
- Cancel/abort loading (buyer subclasses + overrides `OnBackPressed` if needed).
- Download size / ETA display (platform-specific; buyer extends via Inspector refs).

## Files

```
Runtime/Catalog/Screens/Loading/
├── LoadingScreen.cs
└── UIAnim_LoadingScreen.cs
Tests/Editor/
└── LoadingScreenTests.cs
```

## Status

- Code: ✅ delivered 2026-05-05 — `Runtime/Catalog/Screens/Loading/LoadingScreen.cs` with `LoadingRefs` foldout, `SetProgress`/`SetLoadingText` public API, `TickMinDisplayCheck` 1-frame polling for min-display advisory, spinner loop tween, `RealTimeProviderForTests` seam. `Runtime/Catalog/_Internal/UIAnimScreenBase.cs` new base (shared with future MainMenuScreen). `UIAnim_LoadingScreen` trivial subclass.
- Spec: ✅ this document (2026-05-05)
- Tests: ✅ delivered 2026-05-05 — `Tests/Editor/LoadingScreenTests.cs` 14 tests covering DTO defaults / Bind null fallback / InitialProgress snap / SetProgress stores / OnProgressComplete fires at 1f / does not refire / Bind clears subscription / MinDisplaySeconds=0 silent / fires after elapsed / fires once / hidden ignored / OnBackPressed no-op / ShowProgressBar false / ShowSpinner false / SetLoadingText hides empty labels.
- Prefab: ✅ delivered — `CatalogGroupEBuilder.BuildLoadingScreen` (run `Tools/Kitforge/UI Kit/Build Group E Sample` to generate `Catalog_GroupE_Demo/Prefabs/LoadingScreen.prefab`)
- Demo scene entry: ✅ delivered — `GroupE_BootDemo.unity` built by `CatalogGroupEBuilder.BuildDemoScene`
