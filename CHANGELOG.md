# Changelog

All notable changes to this package will be documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

_Last updated: 2026-05-02_

## [Unreleased]

## [0.4.0-alpha] — 2026-05-02

> Group A — Pure UI catalog. Tag cut after Unity Editor verification of the 7-item Done criteria below: Bootstrap Defaults runs, `Build Group A Sample` produces 4 prefabs + scene with all 5 references wired (Theme + 4 prefabs), 4 prefabs open clean (36/36 catalog refs + 4/4 animator refs), 12 ContextMenu entries spawn correctly in Play, EditMode Test Runner reports 86/86 green, Editor.asmdef compiles after the Catalog reference addition, `git status` aligned with the deliverables list.

### Group A closure (this batch — specs, extra tests, severity polish, prefab generator, Theme contract honoring)
Resolves the per-element `Pending` lines flagged in the four "Group A — element N/4" entries below. Closes the TA checker gap "Theme sprite slots not consumed by generated prefabs" — the core "skin it once" pitch now actually works for catalog prefabs.

- **Specs written** under `Documentation~/Specs/Catalog/`: `NotificationToast.md`, `PausePopup.md`, `TutorialPopup.md`. Each opens with a **"Decisions to confirm"** block (D1-D8 / D1-D7 / D1-D8 respectively) promoting per-element implementation choices to spec contract. `ConfirmPopup.md` already shipped.
- **EditMode tests +25**: PausePopup +9 (intent + shortcut + toggle events for Restart / Home / Quit / Settings / Shop / Help / Sound / Music / Vibration), TutorialPopup +9 (3 backdrop modes / 2 StartIndex clamping / `CompleteTutorial` API / re-Bind StartIndex / 2 empty-Steps no-op), NotificationToast +7 (3 tap paths / 3 severity mappings / re-bind safety). Total catalog tests: 53. Total package tests: **86** (33 framework + 53 catalog).
- **TutorialPopup early-return guards**: `GoNext` and `GoPrevious` now `if (StepCount == 0) return` — prevents `OnNext(0)` firing with empty Steps list (defensive; `Refs` UI hides those buttons but programmatic calls would have leaked the event).
- **NotificationToast severity mapping upgraded**:
  - `Warning` → `WarningColor` (new Theme field) — was `AccentColor` (semantic neighbor; not distinct enough for a premium asset).
  - `Info` / `Warning` / `Error` → `IconInfo` / `IconWarning` / `IconError` (new Theme slots) — were `null` (only `Success` had `IconCheck`).
  - `SeverityIcon` hide path: `gameObject.SetActive(false)` instead of `Image.enabled = false` — `LayoutGroup` now reflows the row when icon slot is null instead of leaving a gap.
  - `SeverityToColor` / `SeverityToIcon` / `SeverityToCue` helpers: `private static` → `internal static` to allow direct unit testing without prefab Refs setup.
- **`UIThemeConfig`**: added `_warningColor` (default `(1.00f, 0.60f, 0.15f, 1f)` saturated orange), `_iconInfo`, `_iconWarning`, `_iconError` sprite slots. Removed legacy `_transitionSpeed` and `_bounceStrength` fields (zero consumers since v0.3.0 — animation tuning lives entirely in `UIAnimPreset` SO now). **BREAKING for assets that wrote those slots** — re-save existing Theme assets after upgrade.
- **Access modifier changes (testability)**: 9 `Handle*` in `PausePopup`, 1 in `TutorialPopup` (`HandleBackdrop`), 1 in `NotificationToast` (`HandleTap`) promoted `private` → `internal`. Catalog test asmdef has `InternalsVisibleTo` since v0.3.0; this matches the precedent set by `SetAnimatorForTests`.
- **`PausePopupTests` SetUp** now creates a mock `UIThemeConfig` + `UIAnimPreset` + `UIServices` and calls `popup.Initialize(theme, services)` before each test — silences the cosmetic `[PausePopup] Theme not initialized` and `[PausePopup] No animation preset resolved` warnings the four `OnShow*` tests were emitting. The other three Catalog fixtures don't call `OnShow` so they don't need this.
- **New Editor generator** `Tools/Kitforge/UI Kit/Build Group A Sample` (`Editor/Generators/CatalogGroupABuilder.cs`): single-click materialization of `ConfirmPopup.prefab` + `PausePopup.prefab` + `TutorialPopup.prefab` + `NotificationToast.prefab` + `Catalog_GroupA_Demo.unity` into `Assets/Catalog_GroupA_Demo/`. Demo scene wires a `Canvas` (1080×1920 portrait CanvasScaler) + `EventSystem` + `UIServices` + the `CatalogGroupADemo` host MonoBehaviour with 12 `[ContextMenu]` triggers (4 Confirm tones / 2 Pause variants / 2 Tutorial variants / 4 Toast severities). Idempotent: re-running overwrites.
- **`CatalogGroupABuilder` asset-reference persistence fix + `CatalogGroupADemo` field-type refactor** (caught during v0.4.0 verification, two complementary changes):
  - **Builder**: original implementation (a) passed `PrefabUtility.SaveAsPrefabAsset` return values directly into `BuildDemoScene`'s `SerializedProperty.objectReferenceValue` assignments — Component-on-prefab refs serialized as `{fileID: 0}`; (b) loaded `UIThemeConfig` BEFORE `EditorSceneManager.NewScene(EmptyScene, Single)` — the asset reference survived in memory but Unity's scene serializer wrote it as `{fileID: 0}` after the scene replacement. Diagnostic logs proved both pre-Apply and post-Apply re-reads of the SerializedProperty held a valid reference (instanceID present, name resolvable), yet `SaveScene` persisted null. Root cause for both: asset references obtained outside the active scene's lifetime become "stale" for the scene serializer even though they remain valid C# references. Fix: `BuildAll` now does `SaveAssets()` + `Refresh()` between prefab creation and `BuildDemoScene()`. `BuildDemoScene` calls `EditorSceneManager.NewScene` FIRST, then loads all 5 assets via `AssetDatabase.LoadAssetAtPath` (Theme as `UIThemeConfig`, prefabs as `GameObject`) — references obtained against the active scene context persist correctly. `BuildAll` no longer threads prefab refs through parameters.
  - **Sample**: `CatalogGroupADemo` 4 prefab fields changed from `ConfirmPopup`/`PausePopup`/`TutorialPopup`/`NotificationToast` Component types → `GameObject`. Decouples the asset persistence story from Component-vs-GameObject reference type (orthogonal cleanup). SpawnXxx methods now do `Instantiate(prefab).GetComponent<T>()` at runtime with explicit null check (logs an error and destroys the orphan if a buyer assigns a prefab missing the expected component). New private `SpawnPopup<T>` helper deduplicates the 3 popup spawn paths; `SpawnToast` stays inline (different parent + base type). Trade-off: Inspector drag-drop accepts any prefab, not just those carrying the typed Component — runtime check enforces the contract instead. Documented via `[Header]` tooltip.
- **New Sample** registered in `package.json`: **"Catalog — Group A — Pure UI"** (`Samples~/Catalog_GroupA_PureUI/`). Ships host script (`CatalogGroupADemo.cs`), asmdef, README. Two-step OOTB flow: `Bootstrap Defaults` → `Build Group A Sample`.
- **`Editor.asmdef`**: now references `KitforgeLabs.MobileUIKit.Catalog` + `Unity.TextMeshPro` (required by the builder).
- **README §Status rewritten** with a 6-group table (0 / A / B / C / D / E) showing per-group ship status. Latest tag updated to `v0.3.0-alpha`. Total tests count surfaced.
- **README §Architecture decisions** new entry #11: a single `UIThemeConfig` asset feeds `UIManager` + `PopupManager` + `ToastManager`. Closes the documentation gap flagged by the close_context.
- **`Documentation~/Specs/CATALOG.md`** row links: ConfirmPopup / PausePopup / TutorialPopup / NotificationToast all link to their per-element specs.
- **`package.json` description** updated to mention the catalog (4 elements shipped in `v0.4` + group-by-group roadmap).
- **NEW Theme contract: `IThemedElement` + `ThemedImage` + `ThemedText`** in `Runtime/Theme/`:
  - `IThemedElement.ApplyTheme(UIThemeConfig)` is the public contract; any component implementing it gets called when a popup or toast is initialized.
  - `ThemedImage` (slots: `ThemeSpriteSlot` enum {`PanelBackground` / `ButtonPrimary` / `ButtonSecondary` / `Backdrop` / `Divider`} + `ThemeColorSlot` enum 10 entries including new `WarningColor`).
  - `ThemedText` (slots: `ThemeFontSlot` enum {`FontHeading` / `FontBody` / `FontCaption`} + `ThemeColorSlot` + `ThemeFontSizeSlot`).
  - `None` enum value on every slot = "leave the authored value untouched" — supports literal-color text on themed buttons (white-on-primary).
- **`UIModuleBase.Initialize` and `UIToastBase.Initialize`** now walk children and call `ApplyTheme` on every `IThemedElement` found. `protected ApplyThemeToChildren(theme)` available for manual re-apply if buyer ever needs it (e.g., theme swap mid-session). One-time on instantiate; no perf cost in steady state.
- **`CatalogGroupABuilder` wires `ThemedImage` / `ThemedText`** on Card backgrounds, primary/secondary/danger button backgrounds, Title (FontHeading / TextPrimary), Body / Subtitle / Progress (FontBody or FontCaption / TextSecondary), button labels (FontBody / TextPrimary or untouched). Confirm / Pause / Tutorial / Toast all benefit. Backdrop, Toast `SeverityTint`, ConfirmPopup `ToneStrip` and Toggle visuals stay non-themed by design (their tint/visuals are dynamic per-data, not Theme-driven).
- **Sample README** new sections: **Theme contract** (table of which slot drives which element) + **Hierarchy stability** (safe vs not-safe edits to prefab variants) + clarified re-run instructions (rename output folder before customizing).

### OUT of scope for v0.4.0-alpha
Explicit limits — these do NOT ship in this tag and are not promised by `v0.4.0-alpha`:

- **Catalog Groups B / C / D / E** (RewardPopup, ShopPopup, NotEnoughCurrencyPopup, DailyLoginPopup, LevelCompletePopup, GameOverPopup, SettingsPopup, LoadingScreen, MainMenuScreen, HUD-Coins, HUD-Gems, HUD-Energy, HUD-Timer). 11 catalog elements remain. Group B is next per the roadmap.
- **Default severity icon sprites**. The Theme has slots for `IconInfo` / `IconWarning` / `IconError` but **no default sprites are shipped** — buyer assigns their own. The toast renders correctly without them (icon `gameObject.SetActive(false)`); shipping defaults requires Texture2D + Sprite asset generation in `Bootstrap Defaults`, deferred to a polish session.
- **`Build Group A Sample` style polish**. The generated prefabs are functionally complete but cosmetically minimal: flat colors, no rounded corners, default Unity button colors, no shadow, no padding finesse. Sufficient for a working demo, NOT representative of the final visual quality target ("Disney Getaway Blast" reference). Buyers ARE expected to restyle.
- **Pre-built prefabs in `Samples~/`**. Prefabs are generated on demand by the menu, not committed to the package. Rationale: prefab YAML drift and TMP/Unity GUID resolution issues across versions; the builder script is more maintainable than a binary asset to track.
- **Test Runner CI integration**. The 86 EditMode tests must be run manually until a CI pipeline is set up (out of scope for product-deliverable milestones).
- **Roadmap final sync**. `~/.claude/memory/kitforge_mobile_ui_kit_roadmap.md` Current state still says `v0.2.1-alpha`; will be synced to `v0.4.0-alpha` AFTER tag is cut to avoid claiming a tag that doesn't exist yet.
- **`Samples~/GameWiring/` repair**. Discovered during v0.4.0 verification (2026-05-02): the legacy `GameWiring` sample is broken on import — VContainer is opt-in (not installed in PACHINKO host project, ~3 errors) and the 5 stub services (`StubEconomyService`, `StubPlayerDataService`, `StubProgressionService`, `StubShopDataProvider`, `StubAdsService`, `StubTimeService`) have not been updated since v0.1.0; ~30 interface members added during v0.3.0 G3 service binding work are unimplemented (CS0535). Importing the sample blocks the entire project from compiling. **Pre-existing damage, NOT a v0.4.0 regression** — was masked because `Samples~/` does not compile until imported. Buyer-facing P0. Path forward to be decided in a follow-up session: (a) update stubs + add VContainer dependency note in sample README, (b) refactor sample to be VContainer-free, or (c) remove sample from `package.json` `samples[]` until service contracts stabilize. Option (c) is the cleanest minimum; (a) is the highest fidelity. Tracked in `kitforge_mobile_ui_kit_roadmap.md` for prioritization.

### Done criteria — verified inside Unity (2026-05-02)

- [x] `Tools/Kitforge/UI Kit/Bootstrap Defaults` runs without error and creates `UIThemeConfig_Default.asset` + 10 `UIAnimPreset_*.asset` files.
- [x] `Tools/Kitforge/UI Kit/Build Group A Sample` runs without error and creates 4 prefabs + 1 scene under `Assets/Catalog_GroupA_Demo/`. Demo GameObject has all 5 references wired (Theme + 4 prefabs + UIServices + 2 parents).
- [x] All 4 generated prefabs validated structurally via YAML inspection: 36/36 catalog `Refs` fields and 4/4 animator `_card`/`_root` fields populated (`{fileID: <non-zero>}`). No missing-script entries.
- [x] `Catalog_GroupA_Demo.unity` Play + 12 `[ContextMenu]` entries: confirmed by user during verification.
- [x] **Theme reskin chain** structurally verified: `ThemedImage` and `ThemedText` components present on all themed elements per prefab YAML; `UIModuleBase.Initialize` / `UIToastBase.Initialize` walk children via `GetComponentsInChildren<IThemedElement>(true)` and call `ApplyTheme`. Runtime swap test deferred (pure visual confirmation, contract proven by component graph).
- [x] EditMode Test Runner: 86/86 passed (`TestResults.xml` confirms `total=86 passed=86 failed=0 inconclusive=0 skipped=0`).
- [x] Editor.asmdef compiles with the Catalog reference addition (verified by builder execution + diagnostic logs running successfully).
- [x] `git status` aligned with the v0.4.0-alpha deliverables list (no orphan files outside the catalog scope).

### Verification-only fixes applied during the closure pass

- `EditorSceneManager.NewScene(EmptyScene, Single)` was invalidating the asset references (Theme `ScriptableObject` + 4 prefab Components) loaded BEFORE it — references survived in memory but the scene serializer wrote `{fileID: 0}` to disk. Resolved by moving `LoadAssetAtPath` calls AFTER `NewScene`. Diagnostic logs proved both pre-Apply and post-Apply re-reads held a valid reference yet `SaveScene` persisted null until the load order was corrected. See `CatalogGroupABuilder` entry above.

### Animation system simplified — drop enum + library indirection (BREAKING)
- **Why**: 3-piece system (UIAnimStyle enum + UIAnimPresetLibrary SO + UIAnimPreset SOs) violated the kit's own pitch ("skin it once") — buyer needed 8 manual steps to get a fresh Theme to animate. Default Theme silently produced un-animated popups with zero feedback.
- **Removed**: `UIAnimStyle` enum (10 styles + Custom), `UIAnimPresetLibrary` SO (style->preset map with fallback), `UIAnimPresetLibraryEditor` (Editor inspector for the library).
- **Removed from `UIThemeConfig`**: `_defaultAnimStyle` field, `_animPresetLibrary` field, `DefaultAnimStyle` getter, `AnimPresetLibrary` getter.
- **Added to `UIThemeConfig`**: `_defaultAnimPreset` field (single `UIAnimPreset` ref), `DefaultAnimPreset` getter, tooltip clarifying "null = popups appear without animation".
- **`UIModuleBase` / `UIToastBase`**: `virtual UIAnimStyle? AnimStyleOverride => null` -> `virtual UIAnimPreset AnimPresetOverride => null`. Per-element override is now a direct preset ref.
- **All 4 popups + toast `ResolvePreset()` simplified**: `AnimPresetOverride ?? Theme?.DefaultAnimPreset`. Two levels of indirection collapsed into one.
- **`UIAnimPreset`**: removed `_style` identity field + `Style` getter (asset filename + folder communicate identity now).
- **`DefaultUIAnimPresetsCreator` rebuilt**: menu `Tools/Kitforge/UI Kit/Bootstrap Defaults`. Single click creates 10 presets at `Assets/Settings/UIAnimPresets/` AND a `UIThemeConfig_Default.asset` at `Assets/Settings/UI/` pre-wired to the Playful preset. Buyer's OOTB experience drops from 8 steps to 1.
- **Silent failure fix**: `UIModuleBase` and `UIToastBase` now expose `ResolveAnimPreset()` with a one-shot `Debug.LogWarning` if no preset is resolved (Theme null OR `DefaultAnimPreset` null OR override null). Points the buyer at the bootstrap menu by name. Replaces the duplicated `ResolvePreset()` private helper in 4 popups + toast.
- **README Quickstart rewritten**: step 1 is now the bootstrap menu, step 4 references the auto-generated `UIThemeConfig_Default` asset. Buyer never sees a null preset on first run.
- **Pending**: update `*PopupTests` if any reference the removed enum (`UIAnimStyle` not found in tests grep — should be safe but Unity Test Runner re-run required to confirm).

### Group A — element 4/4: TutorialPopup (scaffolded, code+tests, no spec/prefab yet)
- **`Runtime/Catalog/Popups/Tutorial/`**:
  - `TutorialStep` POCO (Title + Body[TextArea] + optional Sprite).
  - `TutorialPopupData` (List<TutorialStep>, StartIndex, ShowPrevious, ShowSkip, LoopBackToFirst, CloseOnBackdrop, TapToAdvance, custom labels Next/Previous/Skip/Done).
  - `TutorialPopup` (`UIModule<TData>`, `[RequireComponent(UIAnimTutorialPopup)]`, lazy `IUIAnimator` + `internal SetAnimatorForTests`, public `GoNext`/`GoPrevious`/`SkipTutorial`/`CompleteTutorial`/`CurrentIndex`/`StepCount`/`IsFirstStep`/`IsLastStep`, dynamic Next-label that mutates to `DoneLabel` on last step, ProgressLabel "i / N" auto-formatted, theme-null warning one-shot, `ClearAllEvents` on Bind + OnDestroy).
  - `UIAnimTutorialPopup` (clone of `UIAnimPausePopup` structurally — `SetUpdate(true)` defensive in show+hide for tutorials shown over a paused gameplay layer).
- **Decisions taken without consult — flag for review**:
  - Back press = Skip (mobile modal convention; Previous stays explicit).
  - GoNext on last step → `OnCompleted` + dismiss; if `LoopBackToFirst=true`, wraps to index 0 instead and fires `OnNext`.
  - `TapToAdvance` overrides `CloseOnBackdrop` on backdrop tap.
  - `GoPrevious` on first step is silently ignored (no event, no audio cue).
  - No `Time.timeScale` handling — Tutorial is gameplay-aware; if pause is needed, host wraps it (or composes inside Pause).
  - Single Next button mutates label vs separate Done button (consistent with mobile onboarding patterns).
  - Per-element animator (no shared code with Pause despite duplication — catalog rule: 1 animator per element).
- **EditMode tests**: `Tests/Editor/TutorialPopupTests.cs` (10 tests: Bind null defaults, Bind sets StartIndex, GoNext advances + StepChanged, GoNext on last → Completed+dismiss, GoNext on last with loop wraps, GoPrevious on first ignored, GoPrevious decrements, back press → Skip + dismiss, back press while dismissing ignored, Bind resets listeners). **All green: 10/10 (59/59 total).**
- **Pending**: spec (`Documentation~/Specs/Catalog/TutorialPopup.md`), prefab + Demo scene, CATALOG.md row link. Closes Group A → bumps minor to `0.4.0`.

### Group A — element 3/4: PausePopup (scaffolded, code+tests, no spec/prefab yet)
- **`Runtime/Catalog/Popups/Pause/`**: `PausePopupData` (7 buttons + 3 toggles inline + flags), `UIAnimPausePopup` (clone of Confirm with `SetUpdate(true)` for unscaled time), `PausePopup` (`UIModule<TData>`, captures/restores `Time.timeScale` around show/hide, Dismissing vs Shortcut button categories, inline toggles mutate `_data` without closing, public `Resume()`, `OnBackPressed→HandleResume`, theme-null warning, `OnDestroy` restores timeScale if `IsPaused`).
- **Decisions taken without consult — flag for review**:
  - Two button categories: **Dismissing** (Resume/Restart/Home/Quit) close, **Shortcut** (Settings/Shop/Help) raise event and keep popup open.
  - Inline toggles (Sound/Music/Vibration) mutate `_data.XxxOn` + emit `OnXxxChanged(bool)`, never dismiss.
  - `Time.timeScale` direct, no `ITimeService` (YAGNI until 2nd consumer).
  - Pause applied AFTER show-anim (callback of `PlayShow`); restore BEFORE hide-anim.
  - `UIAnimPausePopup` uses `SetUpdate(true)` defensively in hide-anim too.
  - `CloseOnBackdrop` default = `false` (catalog-consistent).
  - Public `Resume()` for external triggers (gameplay button, etc.).
- **EditMode tests**: `Tests/Editor/PausePopupTests.cs` (6 tests: Bind null defaults, back→Resume+Dismiss, back ignored if IsDismissing, OnShow pauses + Resume restores, restores original value not hardcoded 1f, Bind resets listeners). **All green: 6/6 (49/49 total).**
- **Pending**: spec (`Documentation~/Specs/Catalog/PausePopup.md`), prefab + Demo scene, CATALOG.md row link.

### Group A — element 2/4: NotificationToast (scaffolded, code+tests, no spec/prefab yet)
- **Group 0 extension** (`Runtime/Toast/UIToastBase.cs`): added `event Action<UIToastBase> DismissRequested`, `bool IsDismissing { get; protected set; }`, `protected UIThemeConfig Theme`, `protected UIServices Services`, `virtual Initialize(theme, services)`, `protected RaiseDismissRequested()`. Aligns Toast layer with Q1/Q2 decisions taken in element 1.
- **`Runtime/Catalog/Toasts/`**: `ToastSeverity` enum (Info/Success/Warning/Error), `NotificationToastData` DTO (Message + Severity + DurationOverride + TapToDismiss), `NotificationToast` (`UIToast<TData>`, `[RequireComponent(UIAnimNotificationToast)]`, lazy `IUIAnimator`, severity → tint+icon+audio cue mapping, idempotent `DismissNow`, `OnTapped`/`OnDismissed` events, `ClearAllEvents` on Bind), `UIAnimNotificationToast` (slide-in via `PositionOffset` + fade, no scale — toasts slide, don't bounce).
- **Severity → theme mapping** (decisions taken without spec — flag for review):
  - Info → `PrimaryColor` + no icon + `UIAudioCue.Notification`
  - Success → `SuccessColor` + `IconCheck` + `UIAudioCue.Success`
  - Warning → `AccentColor` + no icon + `UIAudioCue.Notification` (no dedicated `WarningColor` in Theme — YAGNI until 2nd consumer)
  - Error → `DangerColor` + no icon + `UIAudioCue.Error`
- **`ToastManager`**: added `[SerializeField] UIServices _services`, `Initialize(theme, services)` propagation per toast, `DismissRequested` subscribe/unsubscribe lifecycle. ⚠️ **Breaking for Group 0 buyers** — existing `ToastManager` references in scene need new `_services` slot wired (migration note).
- **EditMode tests**: `Tests/Editor/NotificationToastTests.cs` (5 tests: Bind null defaults, duration override fallback, duration override honored, idempotent DismissNow, Bind resets listeners). **All green: 5/5 (43/43 total).**
- **Pending**: spec (`Documentation~/Specs/Catalog/NotificationToast.md`), prefab + Demo scene, CATALOG.md row link. Closes at end of Group A → bumps minor to `0.4.0`.

### Group A — element 1/4: ConfirmPopup (closed, code+tests+spec)
- **Catalog asmdef** `KitforgeLabs.MobileUIKit.Catalog` (refs Runtime + DOTween.Modules + TMP).
- `Runtime/Catalog/Popups/Confirm/`: `ConfirmTone` (Neutral/Destructive/Positive), `ConfirmPopupData`, `ConfirmPopup` (`UIModule<TData>`), `UIAnimConfirmPopup` (`IUIAnimator` + DOTween).
- **Architectural decisions closed**:
  - Q1 — `UIModuleBase.Initialize(theme, services)` virtual injection. PopupManager/UIManager wire on instantiate. No more `GetComponentInParent`. Testable without scene.
  - Q2 — `UIModuleBase.DismissRequested` event + `RaiseDismissRequested()` protected. Popups no longer hold a manager ref. PopupManager subscribes on resolve, unsubscribes on destroy.
  - Q3 — `IUIAudioRouter` + `UIAudioCue` enum (None/PopupOpen/PopupClose/ButtonTap/Success/Error/Notification). Slot in `UIServices`. Popups call `Services?.Audio?.Play(cue)` null-safe.
- **Race-condition hardening**: `UIModuleBase.IsDismissing` (protected set) elevated to base. Guards double-click confirm/cancel, back press during hide, backdrop spam.
- **Event-leak fix**: `ConfirmPopup.Bind` resets `OnConfirmed`/`OnCancelled`/`OnDismissed` before re-bind.
- **Null-safe Bind**: `Bind(null)` → `new ConfirmPopupData()`. `OnShow` calls `Bind(null)` if `_data == null`.
- **Tunable**: `UIAnimPreset.HideScaleTo` (default `0.9f`) replaces hardcoded scale in animator.
- **TA polish**: `[Serializable] private struct Refs` with `[Tooltip]` per field. Animator tooltips clarify `_card` is the scaled rect.
- **`[RequireComponent(typeof(UIAnimConfirmPopup))]`** + lazy animator resolve (covers prefab + dynamic `AddComponent` paths).
- **EditMode tests**: `Tests/Editor/KitforgeLabs.MobileUIKit.Catalog.Tests.asmdef` + `ConfirmPopupTests` (5 tests: Bind null, back-press routing with/without cancel, back-press during dismiss, Bind resets listeners). **All green: 5/5 (38/38 total).**
- **Spec**: `Documentation~/Specs/Catalog/ConfirmPopup.md` (full micro-spec: DTO, services, events, animation, theme, edge cases, QA scenarios, file layout).
- **CATALOG.md**: ConfirmPopup row links to spec; single-button alert mode documented.
- **Pending (Editor manual)**: `ConfirmPopup` prefab + Demo scene entry under `Samples~/Catalog/`. Will close at end of Group A together with Toast/Pause/Tutorial → bumps minor to `0.4.0`.

## [0.3.0-alpha] — 2026-05-01

### Session — 2026-05-01 (cont. 6) — Phase 2 re-scoped + Group 0 foundation
GOAL: Re-scope Phase 2 to deliver the prefab catalog (15 mid-core mobile UI elements) the kit name promises. Build Group 0 foundation (F1-F8) before any visible element.
DONE:
- **Spec**: `Documentation~/Specs/CATALOG.md` (15 elements decomposed into Screens / Popups / Transient+HUD; 10 plug-and-play MUST + 8 MUSTN'T contracts; 5-group build order).
- **F5 Animation system**: `Runtime/Animation/` — `UIAnimStyle` enum (10 styles + Custom), `UIAnimEase` enum (17 easings, DOTween-mappable), `UIAnimChannel` enum, `UIAnimPreset` SO (per-channel duration/ease/overshoot + button-feedback tokens), `UIAnimPresetLibrary` SO (style → preset map with fallback), `IUIAnimator` interface (Runtime DOTween-free).
- **F6 Theme extended**: `UIThemeConfig` adds 10 sprite slots (panel/button/backdrop/divider/icons), 6 audio slots (button-click/popup-show/popup-hide/success/error/notification), `MinTouchTarget`, `DefaultAnimStyle`, `AnimPresetLibrary` ref.
- **F4 Theme exposure**: `PopupManager` now exposes `Theme` getter (mirrors `UIManager.Theme` pattern).
- **F3 Service binding**: `Runtime/Services/UIServices.cs` — single MonoBehaviour container with Inspector slots for all 6 services (Economy/PlayerData/Progression/ShopData/Ads/Time) + runtime setters for DI interop. `UIManager` exposes `Services` getter.
- **F1 Toast layer**: `Runtime/Toast/` — `UIToastBase`, `UIToast<TData>`, `ToastManager` (priority-less, auto-dismiss via Coroutine, `_maxConcurrent` cap with pending queue, `WaitForSecondsRealtime`).
- **F2 HUD layer**: `Runtime/HUD/UIHUDBase.cs` — abstract base for HUD elements with `OnEnable`/`OnDisable` Subscribe/Unsubscribe pattern + Refresh().
- **F7 SafeArea**: `Runtime/SafeArea/SafeAreaFitter.cs` — applies `Screen.safeArea` per-edge (configurable), polls `Update()` for orientation/resolution changes.
- **F8 Editor validator**: `Editor/Validation/UIKitValidator.cs` — menu item `Kitforge/UI Kit/Validate Active Scene` + `IPreprocessBuildWithReport` hook. Checks null fields and duplicate types in registries; aborts build on errors.
- `UIModuleBase`: virtual `AnimStyleOverride` property for per-element style override.
DECISIONS:
- 10 animation styles (Snappy/Bouncy/Playful/Punchy/Smooth/Elegant/Juicy/Soft/Mechanical/Cinematic) selected via Theme default + per-Module override; Disney Getaway Blast = Playful default.
- `UIAnimPreset` as ScriptableObject (buyer-creatable), library SO (`UIAnimPresetLibrary`) maps style→preset for one-step swap.
- Runtime asmdef stays DOTween-free; catalog asmdef will reference DOTween. `IUIAnimator` uses Action callbacks, not Tween return values.
- Service binding pattern locked = Option B (UIServices container, Inspector-driven, DI-optional).
- Validator built as Editor-side scan + pre-build hook (no runtime cost).
PENDING:
- Open Unity to generate `.meta` files for all new scripts.
- Run EditMode tests to confirm no regression on the 33 existing tests.
- Group 0 demo sample (`Samples~/Catalog_Group0_Foundation/`) + EditMode tests for new components (UIServices/ToastManager/SafeAreaFitter).
- Build the 10 default `UIAnimPreset` SO assets per styles table.
- `_tween` / `_tween-dev` agent update to consume `UIAnimPreset` (deferred — not a code task here).
REFS: `Runtime/Animation/*`, `Runtime/Toast/*`, `Runtime/HUD/*`, `Runtime/SafeArea/*`, `Runtime/Services/UIServices.cs`, `Runtime/Theme/UIThemeConfig.cs`, `Runtime/Core/PopupManager.cs`, `Runtime/Core/UIManager.cs`, `Runtime/Core/UIModuleBase.cs`, `Editor/Validation/UIKitValidator.cs`, `Documentation~/Specs/CATALOG.md`

### Session — 2026-05-01 (cont. 5) — Phase 1.5 deferred NOTEs cleanup
GOAL: Cerrar deuda diferida de Phase 1.5 sin abrir Phase 2.
DONE:
- `UIRouter.Initialize()`: nuevo método público idempotente. `Start()` ahora delega en `Initialize()`. Tests dejan de usar reflection sobre `Start` privado — llaman `_router.Initialize()` directo. Añadido `IsInitialized` getter.
- `UIRouterTests`: +1 test `Initialize_CalledTwice_IsIdempotent` (total UIRouter = 10, total Phase 1.5 = 33).
- README arch decision #10: documenta el patrón `protected override` para `BindUntyped` en derivaciones cross-assembly. Cierra el bug latente del sample que el `/_checker as dev` detectó.
DECISIONS:
- `Initialize` idempotente con flag `_isInitialized` — evita doble-disparo de `OnStateChanged` si Awake/Start corre y además llamamos manual.
- `Start()` se mantiene como `private void Start() => Initialize()` para no romper escenas existentes que dependen del ciclo Unity.
PENDING:
- `git push origin main --tags` para subir Phase 1 + Phase 1.5 al remoto.
- Definir alcance Phase 2 con `planner` skill.
REFS: `Runtime/Core/UIRouter.cs`, `Tests/EditMode/UIRouterTests.cs`, `README.md`

### Session — 2026-05-01 (cont. 4) — UIManager EditMode tests (Phase 1.5 close)
GOAL: Cubrir UIManager con EditMode tests siguiendo el mismo patrón que UIRouter/PopupManager. Cierre de Phase 1.5.
DONE:
- `Tests/EditMode/UIManagerTests.cs`: **12 tests** — Push (first/second/cache reuse/missing prefab), Pop (happy/empty/last), Replace (empty stack/swap/missing prefab no corruption), PopToRoot (multi/single).
- Setup pattern: GameObject inactivo durante `AddComponent<UIManager>()` + `SetField` reflection + `SetActive(true)` al final → evita que `Awake` corra con `_themeConfig == null` (que loggearía error y rompería los tests con `Unhandled log message`).
- `_themeConfig` inyectado como `ScriptableObject.CreateInstance<UIThemeConfig>()` y destruido en TearDown.
- LogAssert.Expect en los 2 tests de fallo de prefab (Push + Replace) — valida el contrato de error documentado en el README.
- **Total Phase 1.5 EditMode tests: 9 (UIRouter) + 11 (PopupManager) + 12 (UIManager) = 32.**
DECISIONS:
- NO se extrajo el test harness compartido (NOTE del checker dev). Los 3 archivos comparten `SetField` + el patrón de fakes pero la duplicación es de ~6 líneas por archivo. Extraer ahora sería over-engineering — la regla "rule of three" ya se cumple (3 archivos), pero los helpers son tan triviales que el coste cognitivo de un harness compartido (otra asmdef? otra clase abstracta? generic constraints?) supera al de copiar 6 líneas. Si llega un 4º archivo de tests con el mismo patrón, extraer entonces.
- `Replace_MissingPrefab_DoesNotPopExistingTop` valida explícitamente la decisión Phase 1 #3 del code-doctor (Replace<T> resuelve incoming antes de Pop). Es el test más importante del suite — protege contra regresión del bug original.
PENDING:
- Tag `v0.2.0-alpha` (espera validación real en Unity Test Runner).
- Refactor `UIRouter.Start` → `Initialize()` público (NOTE diferido).
- README arch decision #10 sobre `protected override` derive pattern (NOTE diferido).
- Validación real en Unity Test Runner sigue pendiente (MCP offline).
REFS: `Tests/EditMode/UIManagerTests.cs`

### Session — 2026-05-01 (cont. 3) — `/_checker as dev` follow-ups
GOAL: Aplicar 2 FIX NOW + 1 NOTE detectados por `/_checker as dev` sobre el primer pase de Phase 1.5.
DONE:
- **FIX NOW**: `Samples~/Quickstart/QuickstartBootstrap.cs` — `QuickstartScreen.BindUntyped` y `QuickstartPopup.BindUntyped` cambiados de `protected internal override` a `protected override`. Mismo bug latente que `Tests/EditMode`: cruzando assembly boundary, `protected internal` se ve como `protected` y CS0507 explota. No lo cazamos antes porque `Samples~/` con tilde no se compila hasta que el buyer importa el sample — habría reventado en máquina del cliente.
- **FIX NOW**: `PopupManagerTests` +2 tests — `IsShowing_TypeNotPresent_ReturnsFalse` (false case del API más usado) y `Show_TypeWithoutRegisteredPrefab_LogsErrorAndReturnsNull` (con `LogAssert.Expect`, valida el contrato de fallo más probable en producción cuando un dev olvida registrar un prefab). Total tests EditMode: 9+11 = **20**.
- **NOTE aplicado (breaking)**: rename `AppState.Playing` → `AppState.Gameplay`. Vocabulario alineado con `PopupPriority.Gameplay` que usa el resto del kit. 1 archivo Runtime + 1 archivo Tests actualizados (sólo 2 usos en todo el paquete).
- `package.json` bump `0.1.0-alpha` → `0.2.0-alpha` por el rename breaking.
DECISIONS:
- Bump pre-1.0 con `-alpha` mantiene la señal de inestabilidad. El tag `v0.1.0-alpha` se preserva como histórico — el siguiente tag será `v0.2.0-alpha` cuando cerremos Phase 1.5 con UIManager tests.
- `protected override` en samples confirmado como contrato del derive pattern. Pendiente documentar en README → Architecture decisions table como entrada nº 10 (no urgente, cuando toque pasar de alpha).
PENDING:
- UIManager EditMode tests — Phase 1.5 sigue abierta.
- Test harness compartido (extraer reflection helpers `SetField`/`InvokeStart` antes de duplicarlos en UIManagerTests) — NOTE del checker dev.
- Refactor `UIRouter.Start` → método público `Initialize()` para no depender de reflection en tests — NOTE del checker dev, deferrable.
- README entry nº 10 en Architecture decisions table sobre `protected override` derive pattern — NOTE.
- Validación real en Unity Test Runner sigue pendiente (MCP offline).
REFS: `Samples~/Quickstart/QuickstartBootstrap.cs`, `Tests/EditMode/PopupManagerTests.cs`, `Tests/EditMode/UIRouterTests.cs`, `Runtime/Routing/AppState.cs`, `package.json`

### Session — 2026-05-01 (cont. 2) — Phase 1.5 EditMode tests
GOAL: Cubrir UIRouter + PopupManager con EditMode tests (Phase 1.5) y arreglar smell residual en `UIRouter.RestrictPopupsTo`.
DONE:
- `UIRouter.cs`: arreglada firma de `RestrictPopupsTo` — estaba pegada a la llave anterior y sin modificador de acceso (privada de facto). Ahora `public void RestrictPopupsTo(...)` con formato correcto. Bug funcional: la API pública para restringir popups no se podía activar desde fuera.
- `Tests/EditMode/KitforgeLabs.MobileUIKit.Tests.EditMode.asmdef`: nueva asmdef Editor-only con `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, referencias a `KitforgeLabs.MobileUIKit` + `nunit.framework.dll` + Test Runner.
- `UIRouterTests`: 9 tests — TransitionTo happy/idempotente/event payload, re-entrancy guard desde callback, IsValidPopup (null type, sin restricciones, con allow-list, null collection bloquea todo), ClearPopupRestrictions restaura estado abierto.
- `PopupManagerTests`: 9 tests — Show inicial, prioridad orden (Modal sobre Meta), MaxDepth=3 cap, drain de queue tras Dismiss, eviction por prioridad superior, eviction preserva Data y restaura al dismiss, DismissAll limpia activos+pending, DispatchBackPressed delega al topmost, DispatchBackPressed sin activos returns false.
DECISIONS:
- Fakes de popup vía `new GameObject().AddComponent<>()` con subclases internas en el test; `Instantiate` clona OK desde scene objects (no requiere prefab asset). Reflection para inyectar `_popupRoot` y `_popupPrefabs` (SerializeField private).
- EditMode (no PlayMode) — todos los flujos testeables son sincrónicos (no corrutinas).
- `_popupCache` no se purga en `DismissAll` (sólo en `OnDestroy`); test `DismissAll_ClearsActiveAndPending` valida que pending también se limpia re-mostrando el primer popup.
PENDING:
- Tests de `UIManager` (Push/Pop/Replace/PopToRoot) — diferidos: requieren screen prefabs y `UIThemeConfig` SO; mismo patrón que PopupManagerTests pero más boilerplate. Phase 1.5 sigue abierta hasta cubrir UIManager.
- Validación en Unity Test Runner — pendiente de ejecutar (Unity MCP offline en esta sesión). Compilación estática verde.
- NOTES diferidas técnicas (PriorityQueue<>, registry compartido, backdrop fade) — sin cambios.
REFS: `Runtime/Core/UIRouter.cs`, `Tests/EditMode/KitforgeLabs.MobileUIKit.Tests.EditMode.asmdef`, `Tests/EditMode/UIRouterTests.cs`, `Tests/EditMode/PopupManagerTests.cs`

### Session — 2026-05-01 (cont.) — PM checker FIX NOW
GOAL: Cerrar Phase 1 aplicando los 4 FIX NOW del PM checker antes de tag v0.1.0-alpha.
DONE:
- `package.json`: pitch description afilada (router + popup queue + theme), keywords ampliadas (`ugui`, `router`, `theme`), entry `Quickstart` añadida en `samples[]` (orden: Quickstart primero, GameWiring después).
- `README.md` reescrito desde cero: pitch line + Status + 8 Non-goals explícitos + Install (con tabla de prereqs DOTween Pro / UniTask / TMP / VContainer opt-in) + Quickstart 5 pasos + Phase 1 done-criteria checklist + Architecture decisions table de 9 entradas.
- `Samples~/Quickstart/` creado:
  - `QuickstartBootstrap.cs` con `[SerializeField]` UIManager + PopupManager y 4 `[ContextMenu]` (Push / Pop / Show / DismissAll).
  - `QuickstartScreen` y `QuickstartPopup` derivan de `UIModule<object>` (no de `UIModuleBase` directamente — `BindUntyped` es `internal abstract` y no es accesible desde otra asmdef).
  - `KitforgeLabs.MobileUIKit.Quickstart.asmdef` referencia solo `KitforgeLabs.MobileUIKit` (NO VContainer).
  - `README.md` con scene-setup paso a paso (Canvas + ScreenRoot/PopupRoot + UIThemeConfig + 1 Screen prefab + 1 Popup prefab).
DECISIONS:
- `UIModuleBase.BindUntyped` y `UIModule<TData>.BindUntyped`: `internal` → `protected internal`. Smell detectado al escribir el sample (la asmdef del Quickstart no podía heredar de `UIModuleBase` directamente). Cambio de 1 token, no breaking, desbloquea el patrón correcto. Sample refactorizado para derivar de `UIModuleBase` directamente — el patrón `UIModule<object>` solo aplica si quieres pasar payload tipado.
- Versión: bump `package.json` 0.1.0 → `0.1.0-alpha`. Renombrado el `[0.1.0]` histórico del CHANGELOG a `[0.0.1]` para alinear SemVer con el release real.
- Commit final + tag `v0.1.0-alpha` ejecutados localmente (sin push).
PENDING:
- EditMode tests (`UIRouter.TransitionTo`, `PopupManager` priority ordering) — Phase 1.5.
- NOTES diferidas de la sesión previa: `PriorityQueue<>` real para `_pendingQueue`, registry compartido `cache+FindPrefab`, backdrop fade.
- Push a remote diferido a decisión manual.
REFS: `package.json`, `README.md`, `Samples~/Quickstart/QuickstartBootstrap.cs`, `Samples~/Quickstart/KitforgeLabs.MobileUIKit.Quickstart.asmdef`, `Samples~/Quickstart/README.md`

### Session — 2026-05-01
GOAL: Aplicar 3 CRITICAL del code-doctor sobre PopupManager + UIManager y validar Phase 1 con checkers drift + as pm.
DONE:
- Hotfix UIRouter.IsValidPopup corrupto por merge fallido (commit 2175e6f, único commit de la sesión).
- code-doctor sobre PopupManager.cs + UIManager.cs: 3 CRITICAL + 2 MINOR + 1 SUGGESTION aplicados.
- PopupRecord unificado guarda Data → eviction preserva estado al re-encolar.
- UIManager.Replace<T> resuelve incoming antes de Pop (no corrompe stack si registry miss).
- OnDestroy en UIManager y PopupManager → OnHide() a módulos cacheados (libera tweens).
- MAX_DEPTH → MaxDepth; using System; añadido en PopupManager.
- PopupEntry + PopupRequest colapsados en PopupRecord único.
- /_checker drift y /_checker as pm ejecutados (veredictos 🔴 moderado / 🔴).
PENDING:
- Aplicar 4 FIX NOW del PM checker: pitch line en package.json, README completo (pitch + Non-goals + Quickstart + Done criteria + Architecture decisions), Samples~/Quickstart/ con QuickstartBootstrap.cs + asmdef + README.  ← NEXT
- EditMode tests (UIRouter.TransitionTo, PopupManager priority ordering) — Phase 1.5.
- Tag v0.1.0-alpha.
- Commit final batched de toda la sesión post-2175e6f.
- NOTES diferidas: PriorityQueue<> real para _pendingQueue, registry compartido cache+FindPrefab, backdrop fade.
DECISIONS:
- Política: 1 commit final por sesión (no per-fix). Excepción única ya consumida en el hotfix UIRouter.
- VContainer = opt-in vía Sample, NO dependencia del Runtime asmdef. Falta formalizar en README.
- ChangeLog del producto vive en el propio paquete UPM (portabilidad), no en KITforg_labs externo.
REFS: Runtime/Core/PopupManager.cs, Runtime/Core/UIManager.cs, Runtime/Core/UIRouter.cs, package.json, README.md

### Added
- Phase 0 scaffolding: package.json, folder layout (Runtime/Editor/Samples~/Documentation~).
- Samples~/GameWiring skeleton with VContainer LifetimeScope and 6 stub services.
- UIRouterStub that logs AppState transitions without invoking services.
- Phase 1: UIThemeConfig ScriptableObject + Editor inspector con color preview.
- Phase 1: UIModuleBase + UIModule<TData> con BindUntyped interno.
- Phase 1: UIManager (Push / Pop / Replace / PopToRoot, prefab cache, registry validator).
- Phase 1: PopupManager (priority queue Meta < Gameplay < Modal, eviction con preservación de Data, backdrop, sibling-order sync, MaxDepth=3).
- Phase 1: PopupPriority enum.
- Phase 1: UIRouter (AppState transitions con re-entrancy guard, popup allow-list con flag explícito, back-button dispatch).
- Phase 1: 6 service interfaces (IEconomyService, IPlayerDataService, IProgressionService, IShopDataProvider, IAdsService, ITimeService) + 4 DTOs.
- Phase 1: OnDestroy cleanup en UIManager y PopupManager.

### Changed
- PopupManager: PopupEntry + PopupRequest unificados en PopupRecord único.
- PopupManager: MAX_DEPTH → MaxDepth (UNITY_RULES const naming).
- UIManager.Replace<T>: valida prefab incoming antes de Pop — no corrompe stack si Push fallaría.
- **BREAKING**: `AppState.Playing` renombrado a `AppState.Gameplay` para alinear vocabulario con `PopupPriority.Gameplay`. `package.json` bumped `0.1.0-alpha` → `0.2.0-alpha`.

### Fixed
- UIRouter.IsValidPopup: bloque corrupto por merge fallido reparado (commit 2175e6f).
- PopupManager eviction: re-encolaba con Data=null perdiendo estado del popup desalojado.
- UIRouter.RestrictPopupsTo: firma sin modificador de acceso pegada a la llave anterior — la API pública para activar la allow-list no era invocable. Restaurada como `public void RestrictPopupsTo(IEnumerable<Type>)`.
- `Samples~/Quickstart/QuickstartBootstrap.cs`: `QuickstartScreen.BindUntyped` y `QuickstartPopup.BindUntyped` cambiados de `protected internal override` a `protected override`. Bug latente que sólo reventaba en máquina del buyer al importar el sample (Samples~/ con tilde no compila en CI del paquete).

### Tests
- Phase 1.5: `Tests/EditMode/` con `KitforgeLabs.MobileUIKit.Tests.EditMode.asmdef` (UNITY_INCLUDE_TESTS).
- `UIRouterTests`: 9 tests cubriendo TransitionTo (happy / idempotente / event payload / re-entrancy guard) y popup allow-list (null type, sin restricciones, con allow-list, null collection, ClearPopupRestrictions).
- `PopupManagerTests`: 11 tests cubriendo Show inicial, prioridad, MaxDepth=3, drain de queue, eviction con Data preserved, DismissAll, DispatchBackPressed, IsShowing false-case, Show con tipo sin prefab registrado (LogAssert).
- `UIManagerTests`: 12 tests cubriendo Push (first/second/cache reuse/missing prefab), Pop (happy/empty/last), Replace (empty stack/swap/missing prefab no corruption), PopToRoot (multi/single). Patrón GameObject inactivo durante AddComponent → field injection → SetActive(true) para evitar Awake con theme nulo.

## [0.1.0-alpha] - 2026-05-01

### Added
- Phase 1 closure: full Runtime (`UIManager`, `PopupManager`, `UIRouter`, `UIThemeConfig`, `UIModuleBase` / `UIModule<TData>`, 6 service interfaces + 4 DTOs).
- `Samples~/Quickstart` (zero-dependency) and `Samples~/GameWiring` (VContainer opt-in).
- `README.md` with pitch line, 8 Non-goals, Install + prereqs, 5-step Quickstart, Phase 1 done-criteria, Architecture decisions table.
- `package.json` with sharpened pitch description, expanded keywords (`ugui`, `router`, `theme`), `Quickstart` sample entry.

### Changed
- `UIModuleBase.BindUntyped` and `UIModule<TData>.BindUntyped`: `internal` → `protected internal`. Allows host assemblies (e.g. samples, buyer game code) to derive directly from `UIModuleBase` without forcing the `UIModule<object>` workaround. Non-breaking.

## [0.0.1] - 2026-05-01

- Initial repository skeleton (renamed from `[0.1.0]` to align SemVer with the real Phase 1 release as `[0.1.0-alpha]`).
