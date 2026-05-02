# NotificationToast

## Purpose
Non-blocking transient message with severity (Info / Success / Warning / Error), auto-dismiss timer, optional tap-to-dismiss, and theme-driven color/icon/audio mapping. Stack-able via `ToastManager`. Second element of Group A (first non-popup element).

## Layer
**Toast** — derives `UIToast<TData>`, NOT `UIModule<TData>`. Differences from popups:
- No `OnBackPressed` — toasts do not intercept back press.
- No priority queue — `ToastManager` stacks multiple toasts up to `_maxConcurrent`.
- Auto-dismiss timer (`ToastManager.WaitForSecondsRealtime(toast.DefaultDuration)`).
- No backdrop / no modal blocking.

## Decisions to confirm
The following decisions were taken during implementation without explicit alignment. Each will harden into spec contract unless modified.

| # | Decision | Rationale | Reverse if |
|---|---|---|---|
| D1 | **Severity → Color mapping**: Info=`PrimaryColor`, Success=`SuccessColor`, Warning=`WarningColor`, Error=`DangerColor`. | Premium-tier asset must distinguish all 4 severities visually. `WarningColor` (default saturated orange `#FF9926`) added to Theme in v0.4.0. | If buyer reports Warning/Error indistinct on certain device gamuts → expose Inspector tweakable defaults per severity. |
| D2 | **Severity → Icon mapping**: Info=`IconInfo`, Success=`IconCheck`, Warning=`IconWarning`, Error=`IconError`. | Premium-tier asset must distinguish all 4 severities at a glance (icon + color redundant cue, helps colorblind users). 3 new sprite slots added to Theme in v0.4.0. | If buyer prefers minimalist toasts (no icons), they leave Theme slots null → `gameObject.SetActive(false)` hides the slot and LayoutGroup reflows. |
| D3 | **Severity → Audio cue mapping**: Info=`Notification`, Success=`Success`, Warning=`Notification`, Error=`Error`. | YAGNI: no `Warning` audio cue in `UIAudioCue` enum. Notification is generic-attention; sufficient for warnings. | Add `UIAudioCue.Warning` if buyers want audible distinction. |
| D4 | **`TapToDismiss=false`**: tap area set to `interactable=false`. Tap is consumed (no event, no dismiss). | Prevents accidental dismissal of error toasts that the player must read. Consistent with mobile system toasts (Android transient errors are non-tappable). | None — explicit data flag, buyer can opt in/out per toast. |
| D5 | **`DismissNow` and tap are both `IsDismissing`-guarded**. Calling either after a dismiss starts is a no-op. | Race-condition hardening. Toast might tap-dismiss while `ToastManager` simultaneously fires its timer expiry → second dismiss attempt must be silent. | None — correctness fix. |
| D6 | **Slide-in animation only** (`PositionOffset` + `Fade`, no `Scale`, no `Rotation`). | Toasts slide; popups bounce. Industry convention (Android Snackbar, iOS toast). Scale animation makes toasts look like miniature popups, breaks visual hierarchy. | If a buyer specifically needs a scale-in toast, they swap `UIAnimNotificationToast` for a custom animator. The contract is `IUIAnimator`. |
| D7 | **`_defaultDuration = 3f` Inspector-tunable** on the toast prefab; per-show override via `NotificationToastData.DurationOverride > 0`. Negative or zero override falls back to default. | Default mirrors Material Snackbar (`SHORT=2s`, `LONG=3.5s`); 3s splits the difference. Per-show override needed for "this error must stay 8s" cases. | None — additive, no contract risk. |
| D8 | **`OnTapped` fires BEFORE the hide animation starts**. Host receives the event with `IsDismissing=true` already set. | Host that wants to chain "Tap toast → open detail screen" needs the event before the toast disappears. Receiving it after `OnDismissed` would race with the host's transition. | None — ordering is intentional. |

## DTO

`NotificationToastData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Message` | `string` | `""` | Plain text body. Toasts have no title. |
| `Severity` | `ToastSeverity` | `Info` | `Info` / `Success` / `Warning` / `Error`. Drives color/icon/audio. |
| `DurationOverride` | `float` | `-1f` | Seconds on screen. `<= 0` → fall back to prefab `_defaultDuration`. |
| `TapToDismiss` | `bool` | `true` | When `false`, tap area is non-interactable (D4). |

`Bind(null)` is tolerated → falls back to a fresh `NotificationToastData()` instance.

## Inspector fields (toast)

| Field | Type | Default | Notes |
|---|---|---|---|
| `_defaultDuration` | `float` | `3f` | Seconds the toast stays on screen when `DurationOverride <= 0`. |

## Services consumed
- `IUIAudioRouter` (optional). Cue emitted on Show is determined by `Severity` (D3). Cue emitted on tap is `ButtonTap`. All calls null-safe.
- No economy / progression / time-service indirection.

## Public API

| Method / Property | Behavior |
|---|---|
| `Bind(data)` | Inherited from `UIToast<TData>`. Resets events, applies message + severity + tap-area state. |
| `OnShow()` | Logs theme warning if missing. Plays severity-mapped audio cue. Triggers show animation. |
| `OnHide()` | Calls `Animator.Skip()`. Real hide is driven by `DismissWithAnimation`. |
| `DismissNow()` | Idempotent — first call dismisses, subsequent calls are silent (D5). |
| `DefaultDuration` | Override of `UIToastBase.DefaultDuration` getter — returns `DurationOverride` if positive, else `_defaultDuration`. Read by `ToastManager` to schedule auto-dismiss. |

## Events emitted

| Event | When |
|---|---|
| `OnTapped` | Tap area click while `TapToDismiss=true` and `!IsDismissing`. Fires BEFORE hide animation (D8). |
| `OnDismissed` | After hide animation completes. Fires for ALL dismiss paths: tap, `DismissNow()`, and `ToastManager` auto-timer expiry. |

Both events are reset on every `Bind(...)` to prevent handler accumulation across re-show.

## Animation contract
- `[RequireComponent(typeof(UIAnimNotificationToast))]`. Animator auto-resolved in `Awake` and lazily on first access.
- Show: position offset (slide-in from `PositionOffset`) + fade-in. No scale, no rotation (D6).
- Hide: position offset (slide-out) + fade-out, then `FinalizeDismissal` callback.
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`. If both are null, animator no-ops (instant show/hide).

## Theme tokens consumed
Driven by `ToastSeverity`:

| Severity | `SeverityTint.color` | `SeverityIcon.sprite` | Audio cue |
|---|---|---|---|
| Info | `PrimaryColor` | `IconInfo` | `Notification` |
| Success | `SuccessColor` | `IconCheck` | `Success` |
| Warning | `WarningColor` | `IconWarning` | `Notification` |
| Error | `DangerColor` | `IconError` | `Error` |

When a Theme icon slot is null, the icon `GameObject.SetActive(false)` so `LayoutGroup` reflows the row (no empty gap).

Plus:
- `DefaultAnimPreset` (animator preset, single SO ref).
- Sprites/fonts: applied at prefab authoring time from Theme slots; not re-applied at runtime.

## ToastManager interaction
- `NotificationToast` does NOT spawn itself — host calls `ToastManager.Show(data)`.
- `ToastManager` instantiates the toast prefab, calls `Initialize(theme, services)` then `Bind(data)`, then `OnShow()`.
- `ToastManager` schedules `WaitForSecondsRealtime(toast.DefaultDuration)` and calls `DismissNow()` at expiry.
- `ToastManager` subscribes to `DismissRequested` (raised by `RaiseDismissRequested()` in `FinalizeDismissal`) to clean up the slot.
- `_maxConcurrent` cap on `ToastManager` queues excess toasts; pending toasts wait for a slot.

## Edge cases
- **Null DTO**: `OnShow` calls `Bind(null)` → empty message, Info severity, default duration, tap-to-dismiss enabled.
- **Missing Theme**: `ApplySeverity` no-ops (returns early on `Theme == null`). One-shot warning logged via base class. Audio cue still plays through `Services.Audio` (theme-independent).
- **`TapToDismiss=false` + Theme missing**: tap area set to `interactable=false`; tap consumed silently.
- **`DurationOverride=0`**: falls back to `_defaultDuration` (zero is treated as "not set").
- **`DurationOverride=0.001f`**: honored — toast effectively flashes. Use case: visual confirmation pulse.
- **Tap during hide animation**: `IsDismissing=true` already → tap is no-op (D5).
- **`DismissNow` after tap**: `IsDismissing=true` already → second dismiss is no-op (D5).
- **`DismissNow` while ToastManager timer is also expiring**: race-safe via D5 — whoever wins, `OnDismissed` fires once.
- **Re-Show same instance**: `Bind` resets events + `IsDismissing=false`. Re-applies severity (in case the new DTO has different severity).
- **`SeverityIcon` ref null in prefab**: silent no-op in `ApplySeverity` for icon path. Tint still applied.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — Info` (default severity, "Saved.").
2. `Show — Success` ("Reward claimed!" with check icon).
3. `Show — Warning` ("Connection unstable.").
4. `Show — Error` ("Purchase failed.").
5. `Show — Long message` (3 lines, verify TMP wrap).
6. `Show — DurationOverride=8s` (Error toast that lingers).
7. `Show — DurationOverride=0.3s` (flash pulse).
8. `Show — TapToDismiss=false` (Error, verify tap consumed silently).
9. `Show — TapToDismiss=true` (Info, verify tap fires `OnTapped` + dismiss).
10. `Stack — Spawn 5 toasts at once` (verify `ToastManager._maxConcurrent` queueing).
11. `Stack — Spawn 1 every 0.5s for 5s` (verify auto-dismiss order matches spawn order).
12. `Stress — Spam DismissNow 10x` (only first dismisses).
13. `Stress — Spam tap during hide anim` (no NRE, no double dismiss).
14. `Stress — Re-Show same instance with different severity` (verify color/icon/audio swap correctly).

## EditMode coverage
`Tests/Editor/NotificationToastTests.cs` — 12 tests, all green:
- `Bind_With_Null_Data_Uses_Defaults`
- `Default_Duration_Falls_Back_When_Override_Not_Set`
- `Default_Duration_Honors_Override_When_Positive`
- `Dismiss_Now_Is_Idempotent`
- `Bind_Resets_Event_Listeners`
- `Tap_With_TapToDismiss_True_Fires_Event_And_Dismisses`
- `Tap_With_TapToDismiss_False_Is_NoOp`
- `Tap_During_Dismiss_Is_Ignored`
- `SeverityToColor_Mapping_Is_Correct`
- `SeverityToIcon_Mapping_Is_Correct`
- `SeverityToCue_Mapping_Is_Correct`
- `Rebind_With_Different_Severity_Does_Not_Throw`

## Files
```
Runtime/Catalog/Toasts/
├── ToastSeverity.cs
├── NotificationToastData.cs
├── NotificationToast.cs
└── UIAnimNotificationToast.cs
Tests/Editor/
└── NotificationToastTests.cs
```

## Status
- Code: ✅ closed (Group A · element 2/4)
- Spec: ✅ this document
- Tests: ✅ 5/5 green
- Prefab: ⏳ pending (Editor manual step)
- Demo scene entry: ⏳ pending (Editor manual step)
