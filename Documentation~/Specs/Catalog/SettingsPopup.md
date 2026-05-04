# SettingsPopup

> Status: **CONFIRMED 2026-05-04** — S1-S12 locked.
> Targets M2 (`v0.7.0-alpha`). Element 1/1 of Group D (Player Data).
> Consumes [IPlayerDataService](../Services/IPlayerDataService.md) + [IUILocalizationService](../Services/IUILocalizationService.md).

## Purpose

Modal popup with 5 standard player settings (music volume, sfx volume, language picker, notifications toggle, haptics toggle) backed by `IPlayerDataService` for cross-session persistence and `IUILocalizationService` for live re-skin trigger on language change. Element 1/1 of Group D.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| S1 | Settings surface | 5 standard fields: music volume slider (float 0-1), sfx volume slider (float 0-1), language picker (string ISO 639-1), notifications toggle (bool), haptics toggle (bool) | Mobile-game standard surface. Royal Match / Candy Crush / Supercell titles ship the same 5 ± rare additions (per `/_checker as user` market scan 2026-05-04). Covers buyer expectation without scope creep. |
| S2 | Visual layout | Vertical stack, label-left + control-right, single column, centered modal ~70% of screen | Standard mobile-settings pattern. No horizontal scroll, no tabs (5 items don't justify tab indirection). Comfortable for player thumb reach. |
| S3 | DTO | `SettingsPopupData` with 5 `Show<X>` bool flags (default all true) + `Title`/`CloseLabel` string + optional `LanguageOptions[]` | Buyer hides per-setting visibility without forking the prefab (e.g. hyper-casual buyer hides `language` + `haptics`). Default-all-true matches "ship complete" expectation. |
| S4 | Save semantics | `Set*` on every control interaction (in-memory PlayerData write); single `Save()` flush in `OnHide` (belt-and-braces — covers force-close-via-back-button shutdown edge that bypasses Unity's auto-flush) | Standard mobile pattern — settings persist invisibly, no "Save" button visible to player. Manual `Save()` in `OnHide` keeps PlayerPrefs lifecycle robust against atypical close paths. |
| S5 | Audio preview while dragging | Slider drag raises `OnMusicVolumeChanged(float)` / `OnSfxVolumeChanged(float)` in real-time on every value change → buyer's `IUIAudioRouter` applies live → player hears volume change immediately during drag | Standard mobile pattern — every game with audio slider previews live. Without it, player can't tell what value sounds right without releasing the slider repeatedly. |
| S6 | Language change trigger | Language picker change calls `_localization.SetLanguage(code)` (which raises global `OnLanguageChanged` per IUILocalizationService L7) AND writes `kfmui.settings.language` to PlayerData | Single source-of-truth dispatch. Buyer's re-skin handler subscribes globally to `IUILocalizationService.OnLanguageChanged` once and re-skins all visible content atomically. SettingsPopup is one trigger source among potentially many (some games change language from MainMenu too). |
| S7 | Backdrop tap behavior | Dismisses (calls `Save()` then plays hide animation) | Standard mobile modal — backdrop tap closes. No "unsaved changes" prompt because settings auto-persist on every change (per S4). |
| S8 | Back button behavior | Dismisses (same as backdrop tap) | Mobile standard. No special handling — just close + Save. |
| S9 | Animation style | `Smooth` preset (per Animation Style Catalog #5) — polished/premium feel | Settings is a "calm, polished" context, not bouncy/playful. `Smooth` (no overshoot, OutCubic in / InOutSine out) matches mobile-settings genre expectation. |
| S10 | Bind contract | `Bind(SettingsPopupData)` reads ALL canonical PlayerData keys + IUILocalizationService current language and binds to controls. Re-binding mid-Show is supported (resets event listeners). | Standard `UIModule<TData>` pattern. Bind = "snapshot current state into controls". Save happens incrementally on interaction + final flush on Hide. |
| S11 | Required services | `IPlayerDataService` (required — null = error); `IUILocalizationService` (required IF `ShowLanguagePicker=true`, else null tolerated); `IUIAudioRouter` (optional — cues only) | Fail-loud on missing PlayerData (popup is meaningless without it). Tolerate null Localization when picker hidden (hyper-casual buyer single-language). |
| S12 | Restore defaults | OUT of `v1.0.0-rc` — see [IPlayerDataService.md D9](../Services/IPlayerDataService.md). Buyer adds via QUICKSTART recipe (5 lines: `Delete` × N + `Save` + `Reload`) | Mobile market research: top-grossing games with ≤5 settings ship NO reset button. Reset is UX hazard at small surfaces (accidental tap nukes prefs). |

## DTO

`SettingsPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Settings"` | Header label. |
| `CloseLabel` | `string` | `"Close"` | Bottom button label. |
| `ShowMusicSlider` | `bool` | `true` | Hide entire row (label + slider) when false. |
| `ShowSfxSlider` | `bool` | `true` | Hide entire row when false. |
| `ShowLanguagePicker` | `bool` | `true` | Hide entire row when false. Also auto-hidden if `LanguageOptions` is null/empty. |
| `ShowNotificationsToggle` | `bool` | `true` | Hide entire row when false. |
| `ShowHapticsToggle` | `bool` | `true` | Hide entire row when false. |
| `LanguageOptions` | `LanguageOption[]` | `null` | Buyer-supplied (code, displayName) pairs for the picker. Null/empty hides picker even if `ShowLanguagePicker=true`. |

`Bind(null)` → falls back to fresh `SettingsPopupData()` instance (all show, English defaults).

`LanguageOption` POCO:

| Field | Type | Notes |
|---|---|---|
| `Code` | `string` | ISO 639-1 (`"en"`, `"es"`, `"fr"`, …). Must exist in `IUILocalizationService.AvailableLanguages`. |
| `DisplayName` | `string` | UI label as the player will see it (`"English"`, `"Español"`). |

## Services consumed

- `IPlayerDataService` (required) — reads/writes the 5 canonical `kfmui.settings.*` keys.
- `IUILocalizationService` (conditionally required) — drives language picker; `SetLanguage()` raises global re-skin event.
- `IUIAudioRouter` (optional). Cues: `PopupOpen` (Show), `Click` (toggle/dropdown change), `PopupClose` (Hide).

## Events emitted

| Event | When |
|---|---|
| `OnMusicVolumeChanged(float)` | Slider value change (real-time during drag, every value tick). |
| `OnSfxVolumeChanged(float)` | Slider value change (real-time during drag, every value tick). |
| `OnLanguageChanged(string code)` | After `IUILocalizationService.SetLanguage(code)` succeeds. Convenience local event — buyer typically subscribes globally on the service instead. |
| `OnNotificationsChanged(bool)` | Toggle flip. |
| `OnHapticsChanged(bool)` | Toggle flip. |
| `OnDismissed` | After hide animation completes. Fires AFTER `Save()`. Use for cleanup. |

All events reset on every `Bind(...)` to prevent handler accumulation across re-Show.

## Animation contract
- `[RequireComponent(typeof(UIAnimSettingsPopup))]`. Animator auto-resolved in `Awake` and lazily on first access.
- Show: scale-in (0.95→1.0) + fade-in on `_card` (CanvasGroup `_canvasGroup` on root). Style preset = `Smooth` recommended (OutCubic in / InOutSine out — polished, no overshoot).
- Hide: scale-out (1.0→0.95) + fade-out, then `FinalizeDismissal` callback.
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`. If both null, animator no-ops.
- `OnHide` calls `Animator.Skip()` — kill any residual tween.

## Theme tokens consumed

- `AccentColor` — slider fill color, toggle ON state, dropdown selection highlight.
- `MutedColor` — toggle OFF state, slider track empty portion.
- `PanelColor` — dropdown panel background, popup card background.
- `TitleFont` / `BodyFont` — header / labels.
- `DefaultAnimPreset` — animator preset.
- `IconClose` (optional) — close button icon if present.
- `ButtonClickAudio` — audio cue on toggle/dropdown change.

Sprites/fonts: applied at prefab authoring time from Theme slots; not re-applied at runtime.

## Edge cases

- **Null DTO**: `OnShow` calls `Bind(null)` → fresh defaults (all show, English title).
- **`IPlayerDataService` null at Show**: `LogError` describing how to wire `UIServices.PlayerData`; popup shows with all controls disabled.
- **`ShowLanguagePicker=true` but `IUILocalizationService` null**: `LogError` once, hide picker row gracefully (don't crash).
- **`LanguageOptions` empty/null with `ShowLanguagePicker=true`**: hide picker (no error — buyer may legitimately omit).
- **Saved language code not in `LanguageOptions`**: fall back to first entry of `LanguageOptions`; log warning ("saved language X not in supplied options, falling back to Y").
- **Saved language code not in `IUILocalizationService.AvailableLanguages`**: fall back to `IUILocalizationService.CurrentLanguage`; log warning.
- **Slider drag during dismiss animation**: `IsDismissing` guard rejects new value changes (event suppressed). Same for toggle/dropdown.
- **Re-`Show` same instance**: `Bind` resets event listeners + `IsDismissing=false` + re-reads PlayerData (fresh snapshot).
- **`Save()` in `OnHide` while PlayerData impl throws**: log error; popup still dismisses (don't trap player on a failing flush).
- **Race conditions** (covered by `IsDismissing` guard):
  - Double-click close → second click ignored.
  - Back press during hide → ignored.
  - Slider drag during hide → ignored (event suppressed).
- **Dynamic instantiation** (no prefab, `AddComponent`): supported via lazy `Animator` getter.

## QA scenarios

Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — All settings visible (default DTO)`.
2. `Show — Audio sliders only` (hide language/notif/haptics).
3. `Show — No language picker` (hyper-casual buyer scenario).
4. `Drag music slider` — verify `OnMusicVolumeChanged` fires per frame; buyer's audio router previews live.
5. `Change language` — verify `_localization.SetLanguage` called once + global `OnLanguageChanged` raised + PlayerData written.
6. `Toggle notifications` — verify `OnNotificationsChanged` fires + PlayerData written + state persists across re-Show.
7. `Close + reopen` — verify all values persist (PlayerPrefs round-trip via `PlayerPrefsPlayerDataService`; in-memory round-trip via `InMemoryPlayerDataService`).
8. `Backdrop tap` — verify `Save()` called before hide animation starts.

## Tests (≥18, EditMode)

- DTO defaults (1).
- Bind reads all 5 canonical keys from PlayerData (5 — one per field).
- Bind hides each row when `Show<X>=false` (5).
- Slider drag raises event with current value (2 — music + sfx).
- Toggle change raises event + writes PlayerData (2 — notif + haptics).
- Language picker change calls `_localization.SetLanguage` + writes PlayerData (1).
- `OnHide` calls PlayerData.Save() exactly once (1).
- Re-Show resets event listeners (1).
- Saved language not in options falls back gracefully + logs warning (1+).

All tests use `InMemoryPlayerDataService` + `InMemoryLocalizationService` stubs (test seam — hermetic).

## Out of scope

- Account / login section (buyer territory).
- Privacy / GDPR consent (buyer territory; legal varies per market).
- Restore defaults button (per S12 — buyer adds via QUICKSTART recipe).
- Per-music-track or per-channel volume sliders (advanced — buyer extends).
- Audio device selection (platform-specific — buyer territory).
- Display brightness / graphics quality (mid-core/console-port territory; out of mobile-casual scope).
- "Apply changes" button (settings auto-persist per S4; explicit Apply button rejected as redundant + worse UX).
- Settings export/import (mid-core territory).
