# ConfirmPopup

## Purpose
Universal modal Yes/No (or single-button alert) for confirming destructive, neutral or positive actions. First element of Group A.

## DTO

`ConfirmPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `""` | Plain text. Buyer is responsible for i18n. |
| `Message` | `string` | `""` | Plain text body. |
| `ConfirmLabel` | `string` | `"OK"` | Confirm button label. |
| `CancelLabel` | `string` | `"Cancel"` | Cancel button label. Ignored when `ShowCancel=false`. |
| `Tone` | `ConfirmTone` | `Neutral` | `Neutral` → `Theme.PrimaryColor`, `Destructive` → `DangerColor`, `Positive` → `SuccessColor`. |
| `ShowCancel` | `bool` | `true` | When `false`, popup collapses to single-button alert. |
| `CloseOnBackdrop` | `bool` | `false` | Catalog default. When `true`, backdrop tap routes to cancel (or confirm if `ShowCancel=false`). |

`Bind(null)` is tolerated → falls back to a fresh `ConfirmPopupData()` instance.

## Services consumed
- `IUIAudioRouter` (optional). Cues emitted: `PopupOpen` (Show), `ButtonTap` (Confirm/Cancel/Backdrop), `PopupClose` (Hide). All calls null-safe via `Services?.Audio?.Play(...)`.
- No economy / progression / time service. Pure UI element.

## Events emitted
| Event | When |
|---|---|
| `OnConfirmed` | Confirm button click, OR backdrop click when `ShowCancel=false` and `CloseOnBackdrop=true`, OR back press when `ShowCancel=false`. |
| `OnCancelled` | Cancel button click, OR backdrop click when `CloseOnBackdrop=true`, OR back press when `ShowCancel=true`. |
| `OnDismissed` | Always after hide animation completes — fires AFTER `OnConfirmed`/`OnCancelled`. Use for cleanup. |

All three events are reset on every `Bind(...)` to prevent handler accumulation across re-Show.

## Animation contract
- `[RequireComponent(typeof(UIAnimConfirmPopup))]`. Animator auto-resolved in `Awake` and lazily on first access.
- Show: scale pop-in + fade-in on `_card` (CanvasGroup `_canvasGroup` on root).
- Hide: scale-down to `UIAnimPreset.HideScaleTo` (default `0.9f`) + fade-out, then `FinalizeDismissal` callback.
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`. If both are null, animator no-ops (instant show/hide).
- `OnHide` calls `Animator.Skip()` — semantic = "kill any residual tween". Real hide is driven by `DismissWithAnimation` from interaction handlers.

## Theme tokens consumed
- `PrimaryColor` / `DangerColor` / `SuccessColor` (tone tint on `Refs.ConfirmTint`).
- `DefaultAnimPreset` (animator preset, single SO ref).
- Sprites/fonts: applied at prefab authoring time from Theme slots; not re-applied at runtime.

## Edge cases
- **Null DTO**: `OnShow` calls `Bind(null)` → empty fields, defaults applied.
- **Missing Theme**: `ApplyTone` no-ops; animator no-ops silently if `DefaultAnimPreset == null`.
- **Single-button alert**: `ShowCancel=false` hides cancel button; back press routes to `OnConfirmed`.
- **Backdrop with `CloseOnBackdrop=false`**: backdrop is non-interactable.
- **Re-Show same instance**: `Bind` resets event listeners + `IsDismissing=false`. No leak.
- **Race conditions** (covered by `IsDismissing` guard at `UIModuleBase`):
  - Double-click confirm/cancel → second click ignored.
  - Back press during hide animation → ignored.
  - Backdrop spam during hide → ignored.
- **Dynamic instantiation** (no prefab, `AddComponent` only): supported via lazy `Animator` getter.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — Neutral default` (Title + Message + OK/Cancel).
2. `Show — Destructive` (red tint, "Delete save?").
3. `Show — Positive` (green tint, "Claim reward?").
4. `Show — Single-button alert` (`ShowCancel=false`, "Connection lost").
5. `Show — Long text` (Title 80 chars, Message 400 chars — verify TMP overflow).
6. `Show — Empty DTO` (`Bind(null)` path).
7. `Show — CloseOnBackdrop=true` (tap outside dismisses as cancel).
8. `Show — Re-show same instance twice` (verify no duplicate event invocations).
9. `Stress — Spam confirm 10x` (only first dismisses).
10. `Stress — Back press during hide` (no NRE, no double dismiss).

## EditMode coverage
`Tests/Editor/ConfirmPopupTests.cs` — 5 tests, all green:
- `Bind_NullData_FallsBackToDefaults`
- `OnBackPressed_WithCancel_RoutesToCancelled`
- `OnBackPressed_WithoutCancel_RoutesToConfirmed`
- `OnBackPressed_WhileDismissing_IsIgnored`
- `Bind_ResetsEventListeners`

## Files
```
Runtime/Catalog/Popups/Confirm/
├── ConfirmTone.cs
├── ConfirmPopupData.cs
├── ConfirmPopup.cs
└── UIAnimConfirmPopup.cs
Tests/Editor/
└── ConfirmPopupTests.cs
```

## Status
- Code: ✅ closed (Group A · element 1/4)
- Spec: ✅ this document
- Tests: ✅ 5/5 green (38/38 total)
- Prefab: ⏳ pending (Editor manual step)
- Demo scene entry: ⏳ pending (Editor manual step)
