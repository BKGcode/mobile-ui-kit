# Group E — Pre-flight infrastructure delta

> Cross-cutting changes Group E delivery requires BEFORE building the 2 screen elements. Modeled on `CATALOG_GroupC_DELTA.md`.

## Why this doc

`LoadingScreen` and `MainMenuScreen` each need new `UIThemeConfig` slots, a shared `Screens/` folder convention, and a Group E builder + sample. Scattering these notes across two specs hides the consolidated work.

Order of work: **Theme slots → folder convention → element specs → builder → chain demo.**

---

## 1 — `UIThemeConfig` slot additions

| Slot | Type | Used by | Default suggestion |
|---|---|---|---|
| `BackgroundMainMenu` | `Sprite` | MainMenuScreen (full-screen BG image) | Solid gradient placeholder |
| `LogoMainMenu` | `Sprite` | MainMenuScreen (`Refs.LogoImage` — optional game logo above title) | White square placeholder |
| `IconPlay` | `Sprite` | MainMenuScreen play button icon | Filled triangle (▶) |
| `IconShop` | `Sprite` | MainMenuScreen shop button icon | Cart outline |
| `IconSettings` | `Sprite` | MainMenuScreen settings button icon | Gear outline |
| `IconDaily` | `Sprite` | MainMenuScreen daily-login button icon | Calendar outline |
| `IconLoading` | `Sprite` | LoadingScreen spinner image | Circle-segment arc (placeholder) |
| `LoadingBarColor` | `Color` | LoadingScreen progress bar fill | `AccentColor` alias (default) — separate slot so buyer can override loading bar color independently |

**Acceptance**: `UIThemeConfig.cs` declares all 8 fields. Bootstrap Defaults populates with placeholder sprites/colors. Existing Theme assets (`Playful`) get the new slots auto-populated by the Bootstrap upgrade (idempotent — does not overwrite if already set).

---

## 2 — Folder convention

Screens live under `Runtime/Catalog/Screens/{Name}/` (mirrors `Popups/{Name}/` pattern):

```
Runtime/Catalog/Screens/
├── Loading/
│   └── LoadingScreen.cs
└── MainMenu/
    └── MainMenuScreen.cs
```

Animators live under the same folder as the element (same convention as popups):

```
Runtime/Catalog/Screens/
├── Loading/
│   ├── LoadingScreen.cs
│   └── UIAnim_LoadingScreen.cs
└── MainMenu/
    ├── MainMenuScreen.cs
    └── UIAnim_MainMenuScreen.cs
```

---

## 3 — Group E builder

**File:** `Editor/Generators/CatalogGroupEBuilder.cs`
**MenuItem:** `KitforgeLabs / Build Group E Sample (Screens)`

Builder responsibilities:
- Create `Samples~/Catalog_GroupE_Screens/` sample folder and asmdef.
- Instantiate full UIRoot prefab hierarchy (UIManager + PopupManager + UIRouter + ToastManager + UIServices).
- Build `LoadingScreen.prefab` with `UIAnimLoadingScreen` component + Inspector refs wired.
- Build `MainMenuScreen.prefab` with `UIAnimMainMenuScreen` component + Inspector refs wired + `HUDCurrency` slots left as null (buyer wires from their Group B/C HUD prefabs).
- Register both prefabs in `UIManager._screenPrefabs`.
- Instantiate a `GroupEDemoHost` MonoBehaviour with `[ContextMenu]` triggers for all QA scenarios.
- Wire the demo `UIServices` with stub services from Group C sample (`InMemoryProgressionService`, `InMemoryEconomyService`, `InMemoryTimeService`).
- Apply the `Playful` Theme preset.

**Sample structure:**

```
Samples~/Catalog_GroupE_Screens/
├── Scenes/
│   └── GroupE_BootDemo.unity
├── Scripts/
│   └── GroupEDemoHost.cs
└── Stubs/
    └── (no new stubs — Group C stubs cover all needed services)
```

**Demo boot flow (acceptance criteria for M3 tag):**

`Loading → MainMenu → [DailyLogin auto-triggers if due, after MainMenu show animation completes via OnShown event]`

Implemented in `GroupEDemoHost.cs` with `[ContextMenu]` triggers (`Boot Demo`, `Show MainMenu`, `Trigger DailyLogin`). Play / Pause / GameOver are demonstrated in their respective Group A (`PausePopup`) and Group C (`GameOverPopup`) sample chains — not duplicated in Group E demo. Buyers wiring full screen-flows compose Group A + Group C primitives in their own host.

---

## 4 — Null-service fallback policy (cross-element, Group E)

Screens follow the same policy as HUDs from `CATALOG_GroupC_DELTA.md` § 4.5:

| Element type | Policy | Rationale |
|---|---|---|
| **Screen** (`LoadingScreen`, `MainMenuScreen`) | **Silent degrade.** Screen renders without service-driven UI (daily dot hidden, audio skipped). No LogError for optional services. | Screens are always present — a null optional service is a valid "feature not configured" state, same as HUDs on scenes without all services wired. |

Required services (if any) follow the Popup policy (LogError + abort `OnShow`). Neither LoadingScreen nor MainMenuScreen has a required service in M3a scope.

---

## 5 — Group E pre-flight follow-ups

| ID | Concern | Resolution |
|---|---|---|
| GE1 | `UIRouter` in UIRoot architecture — kit component or buyer? | Buyer component. Kit ships `GroupEDemoHost` as the wiring example in the sample. No `UIRouter.cs` added to Runtime/ — capability-gate: adds 0 real value vs. a host MonoBehaviour wiring 4 event subscriptions. |
| GE2 | LoadingScreen `MinDisplaySeconds` enforcement — blocking or advisory? | Advisory only. Screen fires `OnMinDisplayTimeElapsed` event; buyer decides when to transition. No transition blocking in kit code. |
| GE3 | MainMenuScreen daily-dot poll frequency | 30s Update() throttle. Daily claim status changes at most once per day — 1Hz would work but 30s is correct granularity. |
| GE4 | HUD children management in MainMenuScreen | Screen owns `OnShow`/`OnHide` enable/disable on HUD child slots. Buyer drops HUD prefabs as children; screen activates them with the screen. |
