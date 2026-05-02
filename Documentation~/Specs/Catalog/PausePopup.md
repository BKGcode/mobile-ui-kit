# PausePopup

## Purpose
In-game pause modal with action buttons (Resume / Restart / Home / Quit), shortcut buttons (Settings / Shop / Help) that keep the popup open, and inline audio toggles (Sound / Music / Vibration). Owns `Time.timeScale` while visible. Third element of Group A.

## Decisions to confirm
The following decisions were taken during implementation without explicit alignment. Each will harden into spec contract unless modified.

| # | Decision | Rationale | Reverse if |
|---|---|---|---|
| D1 | **Two button categories** — *Dismissing* (Resume/Restart/Home/Quit) close the popup; *Shortcut* (Settings/Shop/Help) raise an event and keep the popup open. | Mirrors mobile pause-menu industry convention (Royal Match / Coin Master / Brawl Stars all keep pause open behind a settings overlay). | Buyer reports confusion that Settings dismisses Pause in their flow → add `DismissOnShortcut` flag. |
| D2 | **Inline toggles mutate `_data.XxxOn` AND emit `OnXxxChanged(bool)`** — never dismiss. | Toggles are settings-light, not actions. Mutation makes the DTO survive re-Show without losing state. Event lets host persist via `IPlayerDataService`. | If buyer wants a "settings-heavy" pause, they push `SettingsPopup` from the Settings shortcut instead. |
| D3 | **`Time.timeScale` direct, no `ITimeService` indirection** — `PausePopup` is the documented owner per CATALOG MUSTN'T §2 exception. `_pauseTimeScale` (default `0`) Inspector-tunable in `[0,1]`. | YAGNI until a 2nd consumer needs time control. Captured value is restored, not hardcoded `1f` (test-covered). | If a 2nd time-control consumer appears (e.g. slow-motion popup), promote to `ITimeService`. |
| D4 | **Pause applied AFTER show-anim** (callback of `PlayShow`); **restore BEFORE hide-anim**. | Pause-during-anim looks broken: the show tween freezes mid-flight. Restore-before-anim lets the hide tween play at real time too. | If the host needs the tween to play under the paused timeScale, set `_pauseTimeScale = 1` and drive pause via `OnPaused`/`OnResumed`. |
| D5 | **`UIAnimPausePopup.SetUpdate(true)`** on both show and hide sequences. | Tween must run while `Time.timeScale = 0`. `SetUpdate(true)` = unscaled time. Defensive in hide too because the anim runs while restoration just happened — a frame-boundary race could still see scaled time. | None — this is a correctness fix, not a preference. |
| D6 | **`CloseOnBackdrop = false` default**. When `true`, backdrop tap routes to `HandleResume` (same as back press). | Catalog-consistent (Confirm also defaults `false`). Pause is intentionally hard-to-dismiss-by-accident on mobile. | Buyer that wants a softer pause sets it `true` per-Show. |
| D7 | **Public `Resume()` API** delegating to `HandleResume()`. | External resume triggers (gameplay button, time-out, app-resume from background) need a way to close pause without simulating a back press. | None — additive, no contract risk. |
| D8 | **Lifecycle events `OnPaused` / `OnResumed`** distinct from intent event `OnResume`. | `OnResume` = "user clicked the Resume button" (button intent). `OnPaused`/`OnResumed` = "timeScale just changed" (lifecycle). Host that mutes audio on pause subscribes to lifecycle, not intent. | None — separation of concerns. |

## DTO

`PausePopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Paused"` | Plain text. Buyer is responsible for i18n. |
| `Subtitle` | `string` | `""` | Optional secondary line (e.g. level name). |
| `ShowResume` | `bool` | `true` | Visibility of Resume button. |
| `ShowRestart` | `bool` | `false` | Visibility of Restart button. |
| `ShowSettings` | `bool` | `false` | Visibility of Settings shortcut. |
| `ShowHome` | `bool` | `false` | Visibility of Home button. |
| `ShowShop` | `bool` | `false` | Visibility of Shop shortcut. |
| `ShowHelp` | `bool` | `false` | Visibility of Help shortcut. |
| `ShowQuit` | `bool` | `false` | Visibility of Quit button. |
| `ShowSoundToggle` | `bool` | `false` | Visibility of Sound inline toggle. |
| `ShowMusicToggle` | `bool` | `false` | Visibility of Music inline toggle. |
| `ShowVibrationToggle` | `bool` | `false` | Visibility of Vibration inline toggle. |
| `SoundOn` | `bool` | `true` | Initial state of Sound toggle. Mutated by user interaction. |
| `MusicOn` | `bool` | `true` | Initial state of Music toggle. Mutated by user interaction. |
| `VibrationOn` | `bool` | `true` | Initial state of Vibration toggle. Mutated by user interaction. |
| `CloseOnBackdrop` | `bool` | `false` | When `true`, backdrop tap → Resume. |

`Bind(null)` is tolerated → falls back to a fresh `PausePopupData()` instance.

## Inspector fields (popup)

| Field | Type | Default | Notes |
|---|---|---|---|
| `_pauseTimeScale` | `float` `[Range(0,1)]` | `0` | `Time.timeScale` value while popup is visible. `0` = full pause; `1` = event-driven pause (host listens to `OnPaused`/`OnResumed`). |

## Services consumed
- `IUIAudioRouter` (optional). Cues emitted: `PopupOpen` (Show), `ButtonTap` (any button, including shortcut), `PopupClose` (Hide). All calls null-safe.
- No economy / progression / time-service indirection. `Time.timeScale` accessed directly per D3.

## Events emitted

### Intent events (button click)
| Event | When |
|---|---|
| `OnResume` | Resume button click, OR backdrop tap when `CloseOnBackdrop=true`, OR back press. |
| `OnRestart` | Restart button click. |
| `OnHome` | Home button click. |
| `OnQuit` | Quit button click. |
| `OnSettings` | Settings shortcut click — popup stays open. |
| `OnShop` | Shop shortcut click — popup stays open. |
| `OnHelp` | Help shortcut click — popup stays open. |
| `OnSoundChanged(bool)` | Sound toggle value changed by user — popup stays open, `_data.SoundOn` mutated. |
| `OnMusicChanged(bool)` | Music toggle value changed by user — popup stays open, `_data.MusicOn` mutated. |
| `OnVibrationChanged(bool)` | Vibration toggle value changed by user — popup stays open, `_data.VibrationOn` mutated. |

### Lifecycle events (timeScale boundary)
| Event | When |
|---|---|
| `OnPaused` | After show-anim completes — `Time.timeScale` was just set to `_pauseTimeScale`. |
| `OnResumed` | Before hide-anim starts — `Time.timeScale` was just restored to its captured value. |
| `OnDismissed` | Always after hide animation completes — fires AFTER `OnResume`/`OnRestart`/`OnHome`/`OnQuit`. Use for cleanup. |

All thirteen events are reset on every `Bind(...)` to prevent handler accumulation across re-Show.

## Animation contract
- `[RequireComponent(typeof(UIAnimPausePopup))]`. Animator auto-resolved in `Awake` and lazily on first access.
- Show: scale pop-in + fade-in + optional position offset on `_card` (CanvasGroup `_canvasGroup` on root).
- Hide: scale-down to `UIAnimPreset.HideScaleTo` + fade-out, then `FinalizeDismissal` callback.
- **Both sequences run with `SetUpdate(true)`** (unscaled time) — required for the popup to animate while `Time.timeScale = 0` (D5).
- `ApplyPauseTimeScale` runs in the show-anim `onComplete` callback (D4).
- `RestoreTimeScale` runs synchronously before `Animator.PlayHide` is called (D4).
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`. If both are null, animator no-ops (instant show/hide), and `ApplyPauseTimeScale` runs immediately.

## Theme tokens consumed
- `DefaultAnimPreset` (animator preset, single SO ref).
- Sprites/fonts: applied at prefab authoring time from Theme slots; not re-applied at runtime.
- No tone tinting (Pause is neutral by definition — no Confirm-style `Destructive`/`Positive` variants).

## Edge cases
- **Null DTO**: `OnShow` calls `Bind(null)` → defaults applied. All buttons except Resume are hidden by default.
- **Missing Theme**: animator no-ops silently if `DefaultAnimPreset == null`. One-shot warning logged via base class. `ApplyPauseTimeScale` still runs (synchronously when no animator preset resolved).
- **Backdrop with `CloseOnBackdrop=false`**: `BackdropButton.interactable = false` → tap is consumed but does nothing.
- **Back press during hide**: ignored (`IsDismissing` guard).
- **Re-Show same instance**: `Bind` resets all 13 event listeners + `IsDismissing=false`. `OnPaused`/`OnResumed` will fire again on next `OnShow`.
- **Original `Time.timeScale != 1f`**: captured before pause and restored faithfully (test-covered). Use case: nested pauses, slow-motion power-up that pauses mid-effect.
- **`OnDestroy` while `IsPaused=true`**: `RestoreTimeScale()` runs in `OnDestroy` — prevents leaving the game in `timeScale=0` if the popup is destroyed without a normal hide.
- **Toggle mutation across re-Show**: `_data.SoundOn` etc. carry the user's interaction across the same DTO instance. Re-Bind replaces the DTO and restores the new DTO's initial values.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — Minimal` (only Resume visible, Title="Paused").
2. `Show — Full` (all buttons + all toggles visible).
3. `Show — Gameplay buttons` (Resume / Restart / Home / Quit).
4. `Show — Shortcuts only` (Settings / Shop / Help — verify popup stays open on click).
5. `Show — Toggles only` (verify `_data.XxxOn` mutates without dismiss).
6. `Show — CloseOnBackdrop=true` (tap outside dismisses as Resume).
7. `Show — Resume from external` (host calls `popup.Resume()` after 2s).
8. `Show — Original timeScale=0.5` (slow-mo context: Pause sets to 0, Resume restores to 0.5).
9. `Show — _pauseTimeScale=1` (event-driven pause: `OnPaused` fires, but `Time.timeScale` stays at 1).
10. `Stress — Spam Resume 10x` (only first dismisses; verify no re-entrant `OnResume`).
11. `Stress — Back press during hide` (no NRE, no double dismiss).
12. `Stress — Destroy while paused` (host destroys popup mid-pause; `Time.timeScale` must restore).

## EditMode coverage
`Tests/Editor/PausePopupTests.cs` — 17 tests, all green:
- `Bind_With_Null_Data_Uses_Defaults`
- `Back_Press_Triggers_Resume_Once_And_Dismisses`
- `Back_Press_While_Dismissing_Is_Ignored`
- `OnShow_Pauses_TimeScale_And_Resume_Restores`
- `Resume_Restores_Original_TimeScale_Not_Hardcoded_One`
- `Bind_Resets_Event_Listeners`
- `OnShow_Fires_OnPaused_Event_Once`
- `Resume_Fires_OnResumed_Event_Once`
- `Restart_Click_Fires_Event_And_Dismisses`
- `Home_Click_Fires_Event_And_Dismisses`
- `Quit_Click_Fires_Event_And_Dismisses`
- `Settings_Click_Fires_Event_And_Stays_Open`
- `Shop_Click_Fires_Event_And_Stays_Open`
- `Help_Click_Fires_Event_And_Stays_Open`
- `Sound_Toggle_Mutates_Data_And_Fires_Event_And_Stays_Open`
- `Music_Toggle_Mutates_Data_And_Fires_Event_And_Stays_Open`
- `Vibration_Toggle_Mutates_Data_And_Fires_Event_And_Stays_Open`

## Files
```
Runtime/Catalog/Popups/Pause/
├── PausePopupData.cs
├── PausePopup.cs
└── UIAnimPausePopup.cs
Tests/Editor/
└── PausePopupTests.cs
```

## Status
- Code: ✅ closed (Group A · element 3/4)
- Spec: ✅ this document
- Tests: ✅ 8/8 green
- Prefab: ⏳ pending (Editor manual step)
- Demo scene entry: ⏳ pending (Editor manual step)
