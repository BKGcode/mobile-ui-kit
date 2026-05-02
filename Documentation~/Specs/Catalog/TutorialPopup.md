# TutorialPopup

## Purpose
Multi-step tutorial overlay with title + body + optional illustration, navigation (Next/Previous/Skip), progress indicator and dynamic Next-label mutation on the last step. Fourth element of Group A.

## Decisions to confirm
The following decisions were taken during implementation without explicit alignment. Each will harden into spec contract unless modified.

| # | Decision | Rationale | Reverse if |
|---|---|---|---|
| D1 | **Back press routes to Skip** (not Previous, not Done). | Industry convention on mobile: back from a tutorial = "I want out", not "go back one step". Previous is an explicit forward affordance via its own button. | Buyer reports player confusion → add `BackBehavior` enum (`Skip` / `Previous` / `Disable`). |
| D2 | **`GoNext` on last step**: if `LoopBackToFirst=false` → fires `OnCompleted` + dismiss. If `LoopBackToFirst=true` → wraps to index 0, fires `OnNext(0)`, popup stays open. | Two distinct intents: completion (one-shot tutorial) vs cycling (loopable hint, e.g. controls reminder during gameplay). One flag covers both without inventing a separate `HintPopup`. | If buyers want both completion AND loop on the same instance, split into two configurable callbacks. |
| D3 | **`TapToAdvance` overrides `CloseOnBackdrop` on backdrop tap** — both true → tap advances; only `CloseOnBackdrop` true → tap skips; both false → tap consumed (no-op). | TapToAdvance is the more specific intent ("tutorial wants taps to drive progress"). CloseOnBackdrop is the generic dismiss affordance. Specific beats generic. | Buyer reports confusion → add `BackdropMode` enum (`Disabled` / `Skip` / `Advance`). |
| D4 | **`GoPrevious` on first step silently ignored** — no event, no audio, no warning. | Mobile convention: pressing a disabled-looking action is non-event, not error. Previous button is hidden on first step (`ApplyPreviousVisibility`) so this only fires for programmatic calls. | None — defensive behavior. |
| D5 | **No `Time.timeScale` handling** — Tutorial is gameplay-aware. If pause is needed, host wraps the show or composes it inside `PausePopup`. | Tutorials commonly run during gameplay (controls reminder, hint). Hardcoding pause would force every host to opt out. | None — composable design. |
| D6 | **Single Next button with mutating label** (vs separate Done button) — `NextLabel`/`DoneLabel` swap on last step driven by `ApplyNextLabel()`. | Mobile onboarding convention (Duolingo / Royal Match / Notion onboarding all do this). Less hierarchy noise, less prefab variants. | Buyer reports a11y issue (screen reader announces "Next" instead of "Done") → expose dedicated `DoneButton` slot with explicit label. |
| D7 | **Per-element animator class `UIAnimTutorialPopup`** despite structural duplication with `UIAnimPausePopup`. | Catalog-wide rule: 1 animator per element. Keeps replacing the script independent per element (buyer can fork TutorialPopup's anim without touching Pause). Also lets per-element animators diverge later (Tutorial could grow step-transition micro-anim that Pause doesn't need). | If 4+ animators are byte-identical at v0.5 review, extract `UIAnimSimpleCard` shared base. Defer until concrete pressure. |

## DTO

### `TutorialStep` (class, `[Serializable]`)

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `""` | Step heading. Plain text. |
| `Body` | `string` (`[TextArea(2,5)]`) | `""` | Step body. Multi-line allowed. |
| `Image` | `Sprite` | `null` | Optional step illustration. Hidden when `null`. |

### `TutorialPopupData` (class, `[Serializable]`)

| Field | Type | Default | Notes |
|---|---|---|---|
| `Steps` | `List<TutorialStep>` | empty | The tutorial content. Empty list is tolerated (`StepCount=0`, no advance possible). |
| `StartIndex` | `int` | `0` | Index the popup opens on. Clamped to `[0, Steps.Count - 1]`. Out-of-bounds clamped silently. |
| `ShowPrevious` | `bool` | `true` | Visibility of Previous button. Always hidden on first step regardless. |
| `ShowSkip` | `bool` | `true` | Visibility of Skip button. |
| `LoopBackToFirst` | `bool` | `false` | When `true`, `GoNext` on last step wraps to `0` instead of completing. |
| `CloseOnBackdrop` | `bool` | `false` | When `true`, backdrop tap routes to Skip (unless `TapToAdvance=true`). |
| `TapToAdvance` | `bool` | `false` | When `true`, backdrop tap routes to `GoNext`. Overrides `CloseOnBackdrop` on backdrop tap. |
| `NextLabel` | `string` | `"Next"` | Next button label on intermediate steps. |
| `PreviousLabel` | `string` | `"Back"` | Previous button label. |
| `SkipLabel` | `string` | `"Skip"` | Skip button label. |
| `DoneLabel` | `string` | `"Got it!"` | Next button label on the last step (when `LoopBackToFirst=false`). |

`Bind(null)` is tolerated → falls back to a fresh `TutorialPopupData()` instance (empty Steps, defaults).

## Services consumed
- `IUIAudioRouter` (optional). Cues emitted: `PopupOpen` (Show), `ButtonTap` (GoNext / GoPrevious / Skip), `PopupClose` (Hide). All calls null-safe.
- No economy / progression / time-service indirection.

## Public API

### Methods
| Method | Behavior |
|---|---|
| `GoNext()` | Advance one step. On last step + `LoopBackToFirst=false` → `CompleteAndDismiss`. On last step + `LoopBackToFirst=true` → wraps to `0`. Ignored while `IsDismissing` or `StepCount == 0`. |
| `GoPrevious()` | Decrement one step. Ignored on first step (D4), while `IsDismissing`, or `StepCount == 0`. |
| `SkipTutorial()` | Equivalent to back press / Skip button. Always dismisses. |
| `CompleteTutorial()` | Forces completion regardless of current step. Fires `OnCompleted` + dismisses. |

### Read-only properties
| Property | Behavior |
|---|---|
| `CurrentIndex` | Current step index (0-based). |
| `StepCount` | `_data.Steps.Count` or `0` if data null. |
| `IsFirstStep` | `CurrentIndex <= 0`. |
| `IsLastStep` | `StepCount > 0 && CurrentIndex >= StepCount - 1`. Empty Steps → always `false`. |

## Events emitted

| Event | When |
|---|---|
| `OnStepChanged(int)` | Step index changed via `GoNext` / `GoPrevious` / loop wrap. Payload = new index. NOT fired on initial Bind. |
| `OnNext(int)` | `GoNext` succeeded (advanced or wrapped). Payload = new index. NOT fired on completion. |
| `OnPrevious(int)` | `GoPrevious` succeeded. Payload = new index. |
| `OnSkip` | Skip button click, back press, or backdrop tap when `CloseOnBackdrop=true && TapToAdvance=false`. |
| `OnCompleted` | `GoNext` on last step with `LoopBackToFirst=false`, OR explicit `CompleteTutorial()` call. NOT fired when looping. |
| `OnDismissed` | Always after hide animation completes. Fires AFTER `OnSkip` / `OnCompleted`. Use for cleanup. |

All six events are reset on every `Bind(...)` to prevent handler accumulation across re-Show.

## Animation contract
- `[RequireComponent(typeof(UIAnimTutorialPopup))]`. Animator auto-resolved in `Awake` and lazily on first access.
- Show: scale pop-in + fade-in + optional position offset on `_card` (CanvasGroup `_canvasGroup` on root).
- Hide: scale-down to `UIAnimPreset.HideScaleTo` + fade-out, then `FinalizeDismissal` callback.
- **`SetUpdate(true)`** on both sequences (defensive — tutorials may show over a paused gameplay layer).
- Step transitions are NOT animated by `UIAnimTutorialPopup` — labels swap synchronously in `ApplyStep`. Future enhancement: per-step crossfade animation contract.
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`. If both are null, animator no-ops (instant show/hide).

## Theme tokens consumed
- `DefaultAnimPreset` (animator preset, single SO ref).
- Sprites/fonts: applied at prefab authoring time from Theme slots; not re-applied at runtime.
- No tone tinting (Tutorial is neutral by definition).

## Edge cases
- **Empty Steps list**: `StepCount=0`, `CurrentIndex=0`, `IsFirstStep=true`, `IsLastStep=false`. `GoNext`/`GoPrevious` are explicit no-ops (early-return guard `StepCount == 0`) — no events fired. Title/Body labels show empty strings, ProgressLabel is empty.
- **Single step (`StepCount=1`)**: `IsFirstStep=true && IsLastStep=true`. ProgressLabel is empty (UX choice: 1/1 is noise). `GoNext` triggers completion immediately. Previous never visible.
- **`StartIndex` out of bounds**: clamped silently to `[0, StepCount-1]`. No warning.
- **Null DTO**: `OnShow` calls `Bind(null)` → empty Steps, defaults applied.
- **Missing Theme**: animator no-ops silently if `DefaultAnimPreset == null`. One-shot warning logged via base class.
- **Backdrop with `CloseOnBackdrop=false && TapToAdvance=false`**: tap consumed, no event.
- **Backdrop with both `true`**: TapToAdvance wins (D3) — tap advances, never skips.
- **Step image null on a step**: `_refs.StepImage.gameObject.SetActive(false)`. No layout shift if Image slot uses LayoutElement with `ignoreLayout` toggled.
- **Re-Show same instance**: `Bind` resets all 6 event listeners + `IsDismissing=false` + `CurrentIndex=ClampedStartIndex`. No leak.
- **Race conditions**:
  - `GoNext` spam during step transition → safe (`SetIndex` is synchronous).
  - `GoNext` on last step during dismissal → ignored (`IsDismissing` guard).
  - `SkipTutorial` after `CompleteTutorial` → ignored.
  - Back press during hide animation → ignored.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — 3-step linear` (default flow, click Next 3 times → completion).
2. `Show — 3-step with Skip visible` (click Skip → fires `OnSkip` mid-flow).
3. `Show — 3-step with Previous visible` (Next, Next, Previous → step 1).
4. `Show — Single step` (verify Done label, no Previous, no progress).
5. `Show — 5 steps with images` (verify image swap per step + null-image step hides slot).
6. `Show — Loop` (`LoopBackToFirst=true`, Next on last wraps to 0).
7. `Show — TapToAdvance` (tap backdrop = advance, not skip).
8. `Show — CloseOnBackdrop` (tap backdrop = skip).
9. `Show — Both backdrop flags` (TapToAdvance wins).
10. `Show — StartIndex=2 of 5` (resume from middle).
11. `Show — Empty Steps` (verify no NRE, popup shows empty fields, GoNext no-ops).
12. `Show — Custom labels` (Spanish: "Siguiente"/"Atrás"/"Saltar"/"¡Listo!").
13. `Stress — Spam Next 10x on last step` (only first triggers completion, rest ignored).
14. `Stress — Back press during hide` (no NRE, no double dismiss).
15. `Stress — Re-Show same instance with new Steps` (verify state reset).

## EditMode coverage
`Tests/Editor/TutorialPopupTests.cs` — 19 tests, all green:
- `Bind_With_Null_Data_Uses_Defaults_Without_Errors`
- `Bind_With_Steps_Sets_Start_Index_And_Counts`
- `GoNext_Advances_Index_And_Fires_StepChanged`
- `GoNext_On_Last_Step_Fires_Completed_And_Dismisses`
- `GoNext_On_Last_Step_With_Loop_Wraps_To_First`
- `GoPrevious_On_First_Step_Is_Ignored`
- `GoPrevious_Decrements_Index`
- `Back_Press_Triggers_Skip_And_Dismisses`
- `Back_Press_While_Dismissing_Is_Ignored`
- `Bind_Resets_Event_Listeners`
- `Backdrop_With_TapToAdvance_Calls_GoNext`
- `Backdrop_With_CloseOnBackdrop_Calls_Skip`
- `Backdrop_With_Both_Flags_TapToAdvance_Wins`
- `StartIndex_Negative_Is_Clamped_To_Zero`
- `StartIndex_Beyond_Bounds_Is_Clamped_To_Last`
- `CompleteTutorial_Public_Api_Forces_Completion_Mid_Flow`
- `Rebind_Resets_CurrentIndex_From_New_StartIndex`
- `GoNext_With_Empty_Steps_Is_NoOp`
- `GoPrevious_With_Empty_Steps_Is_NoOp`

## Files
```
Runtime/Catalog/Popups/Tutorial/
├── TutorialPopupData.cs   (also defines TutorialStep)
├── TutorialPopup.cs
└── UIAnimTutorialPopup.cs
Tests/Editor/
└── TutorialPopupTests.cs
```

## Status
- Code: ✅ closed (Group A · element 4/4)
- Spec: ✅ this document
- Tests: ✅ 10/10 green
- Prefab: ⏳ pending (Editor manual step)
- Demo scene entry: ⏳ pending (Editor manual step)
