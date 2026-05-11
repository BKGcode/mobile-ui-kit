# MainMenuScreen

> Infrastructure prerequisites: see the Theme slots and folder convention notes.

## Purpose

Entry-point screen displayed after the boot sequence. Exposes Play, Settings, Shop, and DailyLogin action buttons, each individually hide-able. Each action emits a C# event; the host wires chains to `UIManager.Push` / `PopupManager.Show` per `CATALOG.md` § 3.2.1 (no screen calls PopupManager directly). Hosts optional HUD child slots (Coins, Gems, Energy) activated with the screen.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| **MM1** | Button surface | **LOCKED — 4 standard buttons**: Play, Settings, Shop, DailyLogin. Each individually hide-able via DTO bool. No "Leaderboard", "Friends", "Profile" buttons in MVP — capability-gate: each adds a new popup dependency and a new service interface. Buyer adds extra buttons via subclass or Inspector extension. | Royal Match / Homescapes / Candy Crush MainMenu: 1 giant play button + 2-4 secondary meta buttons. This surfaces the 90% of mid-core mobile games without bloating the base. |
| **MM2** | Event model | **LOCKED — C# `event Action`** (not UnityEvent). `OnPlayRequested`, `OnSettingsRequested`, `OnShopRequested`, `OnDailyRequested`. Host wires to `UIManager.Push<GameScreen>()` / `PopupManager.Show<SettingsPopup>()` etc. Events reset on every `Bind(...)`. | CATALOG § 3.2.1 + § 3.1.9. C# events (not UnityEvents) for per-Show cleanup without UnityEvent serialization noise. Same approach as DailyLoginPopup. |
| **MM3** | Daily indicator dot | **LOCKED — optional badge on DailyLogin button.** `Refs.DailyIndicatorDot` (GameObject). Shown when `IProgressionService.GetDailyLoginState().AlreadyClaimedToday == false`. Polled: on `OnShow` + 30s throttle via `private void Update()`. If `IProgressionService` null: dot hidden silently (per the "silent degrade" rule silent degrade). | Standard mobile meta pattern (coin-master style daily dot). 30s poll is correct granularity — daily claim status changes at most once per UTC day. 1Hz would be technically correct but wastes battery/CPU for a static state. |
| **MM4** | HUD integration | **LOCKED — buyer-configures-as-children.** Screen has Inspector refs `_coinHudSlot` / `_gemHudSlot` / `_energyHudSlot` (GameObject, nullable). On `OnShow`, activates non-null slots; on `OnHide`, deactivates. Buyer drops `HUDCurrency` prefabs as children and wires refs. Screen does NOT instantiate or configure HUDs. | CATALOG § 3.2.6 (no Resources.Load). Screen doesn't know which HUD flavors buyer ships. Inspector-ref pattern keeps DI-free and prefab-configureable. Buyer wires once in prefab editor. |
| **MM5** | Back button (Android) | **LOCKED — emits `OnBackRequested` C# event. Default behavior: no-op.** Buyer wires to `Application.Quit()` or `PopupManager.Show<ConfirmPopup>()` confirm-before-quit. | CATALOG § 3.1.8 — self-contained back behavior. For a root screen, the kit cannot know if buyer wants quit, confirm dialog, or something else (some games go to a "quit?" popup, some just quit). Event is the correct abstraction. |
| **MM6** | Game logo / title display | **LOCKED — dual support**: `Refs.LogoImage` (Image, optional) shows `Theme.LogoMainMenu` sprite (NEW slot). `Refs.TitleLabel` (TMP_Text, optional) shows `LoadingScreenData.Title`. If both assigned, both show. If neither assigned, top area is empty. Buyer uses whichever matches their game's visual identity. | Most mid-core games use a logo sprite, not a text label. Text label is a fast placeholder. Both slots live on the prefab; buyer enables/hides as needed. |
| **MM7** | DailyLogin auto-trigger | **NOT in this screen.** The chain `OnDailyRequested` → host checks `GetDailyLoginState().AlreadyClaimedToday` → `PopupManager.Show<DailyLoginPopup>()` is wired in the host demo. An auto-trigger of DailyLogin on screen enter is a host decision (see `DailyLoginFlow.ShowIfDue` per `DailyLoginPopup.md`). | CATALOG § 3.2.1. If this screen auto-triggered popups, it would violate the rule. `DailyLoginFlow.ShowIfDue` is the helper the host uses. Demo wires this in `GroupEDemoHost.Start()`. |
| **MM8** | Animation style | **LOCKED — `PlayfulEntry` preset.** Show: panel slides in from bottom (Y offset → 0) + stagger button entries (0.05s delay per button from bottom up). Hide: panel slides out to bottom. Duration: show 0.45s, hide 0.25s. Matches Disney Getaway Blast MainMenu energy (bouncy, not formal). | Main menu is the most-seen screen in a mobile game — it must feel alive and welcoming. `Smooth` (SettingsPopup preset) would feel corporate. `Playful` slide-in-from-bottom is the industry-standard mobile-game main menu entry. |
| **MM9** | Services | **LOCKED.** `IProgressionService` (optional — daily dot only). `IUIAudioRouter` (optional — ambient music on Show / stop on Hide). No `IEconomyService` (currency display is HUD territory, not screen territory). | Screens don't read economy state directly — that's HUDs' single responsibility. |

## DTO

`MainMenuScreenData` (`class`, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `""` | Game title text. Empty = `Refs.TitleLabel` hidden. |
| `ShowPlayButton` | `bool` | `true` | Hides Play button row entirely when false. |
| `ShowSettingsButton` | `bool` | `true` | Hides Settings button when false. |
| `ShowShopButton` | `bool` | `true` | Hides Shop button when false. |
| `ShowDailyButton` | `bool` | `true` | Hides DailyLogin button when false. Dot also hidden. |

`Bind(null)` → fresh `MainMenuScreenData()` (all buttons visible, no title).

## Service binding

- `IProgressionService` (optional) — polls `GetDailyLoginState()` for daily dot. Null = dot hidden silently.
- `IUIAudioRouter` (optional) — cues `UIAudioCue.Ambient` on `OnShow`, stop on `OnHide`.

Null-service behavior follows the "silent degrade" rule (Screen = silent degrade).

## Events emitted

| Event | When |
|---|---|
| `OnPlayRequested` (C# `event Action`) | Play button tapped. |
| `OnSettingsRequested` (C# `event Action`) | Settings button tapped. |
| `OnShopRequested` (C# `event Action`) | Shop button tapped. |
| `OnDailyRequested` (C# `event Action`) | DailyLogin button tapped. |
| `OnBackRequested` (C# `event Action`) | Android back button pressed (MM5). |

All events reset on every `Bind(...)` to prevent handler accumulation across re-Show.

No C# events carry data — all are bare `Action`. Host inspects state from `IProgressionService`/`IEconomyService` on its side when reacting to events.

## Animation contract

- `[RequireComponent(typeof(UIAnim_MainMenuScreen))]`. Animator auto-resolved in `Awake`.
- **Show**: panel (`_panelRoot`) slides in from bottom (Y −80 → 0, `Ease.OutBack`) + button stagger (0.05s delay × index from bottom). Full duration ~0.45s.
- **Hide**: panel slides out to bottom (Y 0 → −80, `Ease.InCubic`). Duration 0.25s. `UIManager.Pop` waits for `OnHide` callback; animator calls `FinalizeDismissal` when done.
- `OnHide` calls `Animator.Skip()` — kills all running tweens.
- Stagger driven by button visibility (hidden buttons skipped in stagger sequence).

## Theme tokens consumed

- `BackgroundMainMenu` (NEW) — full-screen background sprite via `ThemedImage` on `Refs.BackgroundImage`.
- `LogoMainMenu` (NEW) — game logo sprite via `ThemedImage` on `Refs.LogoImage` (optional).
- `IconPlay` / `IconSettings` / `IconShop` / `IconDaily` (NEW) — button icon sprites.
- `AccentColor` — Play button background (primary CTA).
- `MutedColor` — secondary button backgrounds (Settings, Shop, Daily).
- `TextOnAccent` — Play button label color.
- `TextPrimary` — secondary button labels.
- `TitleFont` — game title label.
- `FontBody` — button labels.
- `MinTouchTarget` — minimum button height (`≥ Theme.MinTouchTarget`, default 88pt).
- `DefaultAnimPreset` — animator preset.
- `ButtonClickAudio` — audio cue on any button tap (via `IUIAudioRouter`).

## Edge cases

- **`ShowPlayButton=false`**: Play button hidden. `OnPlayRequested` still subscribable but never fires.
- **`ShowDailyButton=false`**: Daily dot also hidden regardless of `IProgressionService` state.
- **`IProgressionService` null**: dot hidden silently, no LogError. Screen still fully functional.
- **`GetDailyLoginState()` throws**: caught, dot hidden, LogError once with service name.
- **Daily state changes while screen is open** (UTC midnight crossing, daily auto-granted from another system): 30s poll catches it. Next poll cycle applies the change.
- **Button tapped during hide animation**: `_isDismissing` guard rejects tap events silently (same as SettingsPopup S9 pattern).
- **`_coinHudSlot` null**: silently skipped in `OnShow`/`OnHide`. No error — buyer may not use all HUD slots.
- **HUD activation when `UIServices` null**: HUDs handle their own null-service degrade; MainMenuScreen does not inspect HUD state.
- **Re-`Show` same instance**: `Bind` resets event listeners + `_isDismissing=false` + re-reads progression state for dot.
- **`OnBackRequested` with no subscriber**: silent. No app-quit in kit code. Buyer MUST wire this on Android builds to prevent OS back-stack confusion.
- **`ConfirmPopup` wired to `OnBackRequested` (common pattern)**: works per CATALOG § 3.2.1 — host wires, screen emits.
- **Multiple `UIManager.Push<MainMenuScreen>()` calls**: UIManager resolves from cache (instantiated once); same instance re-Shown with new Bind data.

## QA scenarios

Triggered from `GroupEDemoHost` via `[ContextMenu]`:

1. `Show MainMenu — all buttons visible` (default DTO).
2. `Show MainMenu — play button only` (shop/settings/daily hidden).
3. `Show MainMenu — title "My Game"` (verify TitleLabel visible).
4. `Daily dot visible` (IProgressionService state: `AlreadyClaimedToday=false`).
5. `Daily dot hidden` (IProgressionService state: `AlreadyClaimedToday=true`).
6. `Tap Play` — verify `OnPlayRequested` fires.
7. `Tap Settings` — verify `OnSettingsRequested` fires, host shows SettingsPopup.
8. `Tap Shop` — verify `OnShopRequested` fires, host shows ShopPopup.
9. `Tap Daily` — verify `OnDailyRequested` fires, host shows DailyLoginPopup via `DailyLoginFlow.ShowIfDue`.
10. `Back press` — verify `OnBackRequested` fires (no crash).
11. `Null IProgressionService` — verify dot hidden, screen fully operational.
12. `Re-Show same instance` — verify event listeners reset, dot state refreshed.
13. `HUD slots active on Show, inactive on Hide` (verify via Coins HUD slot enable state).
14. `DTO defaults assertion` — `Bind(null)` renders all 4 buttons.
15. `Full boot chain`: Loading → Show MainMenu → DailyLogin auto-trigger via `DailyLoginFlow.ShowIfDue`.

## Tests (≥16, EditMode)

- DTO defaults (1).
- `Bind` hides each button when `Show<X>=false` (4).
- `Bind` shows TitleLabel when Title non-empty (1).
- `Bind` hides TitleLabel when Title empty (1).
- `Bind` resets event listeners (1).
- Play button tap fires `OnPlayRequested` (1).
- Settings button tap fires `OnSettingsRequested` (1).
- `ShowDailyButton=false` hides dot regardless of progression state (1).
- `IProgressionService` null: dot hidden, no NRE (1).
- `GetDailyLoginState().AlreadyClaimedToday=false` → dot visible (1).
- `GetDailyLoginState().AlreadyClaimedToday=true` → dot hidden (1).
- `OnBackPressed` triggers `OnBackRequested` event (1).

## Out of scope

- Leaderboard / friends / profile buttons (per MM1 — buyer extends).
- Player name / avatar display (buyer adds via Inspector refs + `OnShow` binding).
- Store badge / "New" indicator on Shop button (buyer adds via Inspector hook on `OnShopRequested`).
- Level progress bar / chapter display (mid-core territory; buyer adds via Inspector refs).
- DailyLogin auto-trigger on screen entry (host responsibility via `DailyLoginFlow.ShowIfDue`).
- Landscape layout (CATALOG § 3.1.6 — portrait first; buyer re-anchors for landscape).

## Files

```
Runtime/Catalog/Screens/MainMenu/
├── MainMenuScreen.cs
└── UIAnim_MainMenuScreen.cs
Tests/Editor/
└── MainMenuScreenTests.cs
```

## Status

- Code: ✅ `Runtime/Catalog/Screens/MainMenu/MainMenuScreen.cs` with `MainMenuRefs` + `HudSlots` foldouts, 5 C# events (OnPlayRequested/OnSettingsRequested/OnShopRequested/OnDailyRequested/OnBackRequested), `RefreshDailyDot` with `IProgressionService` optional + silent degrade, 30s `Update()` poll throttle, `SetHudSlotsActive` on OnShow/OnHide. `UIAnim_MainMenuScreen` with panel slide-from-bottom + `_staggerItems` entry array (captures base positions in Awake, skips inactive items).
- Spec: ✅ this document (2026-05-05)
- Tests: ✅ `Tests/Editor/MainMenuScreenTests.cs` 17 tests covering DTO defaults / Bind null / each button hide-able / title label show+hide / Bind clears subscriptions / Play+Settings events / daily dot ShowDailyButton=false / null progression / AlreadyClaimedToday false→dot visible / true→dot hidden / OnBackPressed fires event / OnBackPressed while hidden no-op.
- Demo scene entry: ✅ delivered — `GroupE_BootDemo.unity` built by `CatalogGroupEBuilder.BuildDemoScene`
