# KitforgeLabs · UI Kit

> Mobile UGUI kit for hybrid-casual games. Wire it in five minutes, skin it once, ship it.

`com.kitforgelabs.mobile-ui-kit` gives you the three pieces every hybrid-casual game re-implements badly, plus a catalog of 17 ready-to-use UI elements and a single-front-door Hub editor window:

- **`UIManager`** — stack-based screen flow (`Push` / `Pop` / `Replace` / `PopToRoot`) with prefab cache and registry validator.
- **`PopupManager`** — priority queue (`Meta < Gameplay < Modal`), eviction with state preservation, backdrop sync, depth cap.
- **`UIThemeConfig`** — one `ScriptableObject` that re-skins every screen, popup and HUD.
- **17-element catalog** — Confirm / Pause / Tutorial / NotificationToast / Reward / Shop / NotEnoughCurrency / DailyLogin / LevelComplete / GameOver / Settings popups · LoadingScreen + MainMenuScreen · HUDCoins / HUDGems / HUDEnergy / HUDTimer + simple HUD variants.
- **KitforgeLabs Hub** (`KitforgeLabs → UI Kit → Hub`) — Setup wizard · Catalog browser · Theme Studio · Test launcher · inline Cheatsheet.

Service binding (economy, ads, time, progression, player data, localization, audio, shop) is buyer-implemented through 8 interfaces; the runtime asmdef has zero DI dependency.

---

## Status

**v1.1.0 — stable.** Public API frozen at `v1.0.0`. v1.1.0 reorganizes everything under the `KitforgeLabs/UI Kit` namespace and removes downloadable samples — the Hub is the single entry point.

---

## Non-goals

1. **No data binding framework.** Modules expose `Bind(TData)` and you call it.
2. **No localization layer.** Bring your own (Unity Localization / I2 / custom JSON). Modules accept already-resolved strings.
3. **No networking, save backend or analytics.** Service interfaces are contracts — your game implements them.
4. **No DI container in Runtime.** `KitforgeLabs.UIKit` has zero reference to VContainer / Zenject / etc. DI users wire their container resolution into the kit's `UIServices` MonoBehaviour setters at boot.
5. **No animation engine.** DOTween Pro is the assumed sequencer for show/hide motion.
6. **No visual screen-graph editor.** Inspector + prefab references only — the Hub authors *scene state*, not screen graphs.
7. **No UI Toolkit runtime support.** UGUI only.

---

## Install

### Prerequisites

| Dependency | Required by | How to install |
|---|---|---|
| Unity 6000.1 LTS or newer | Runtime | Editor |
| TextMeshPro | Runtime (the asmdef references it) | Package Manager (built-in) |
| DOTween Pro | Recommended for screen/popup show & hide animations | [Asset Store](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416) |
| UniTask | Recommended for `async` flows in your game code | [OpenUPM](https://openupm.com/packages/com.cysharp.unitask/) |
| VContainer (or any DI) | **Opt-in only.** Runtime asmdef does not reference it. Wire your container resolution into `UIServices` setters at boot. | [OpenUPM](https://openupm.com/packages/jp.hadashikick.vcontainer/) |

### Add the package

Add to `Packages/manifest.json`:

```json
"com.kitforgelabs.mobile-ui-kit": "https://github.com/BKGcode/mobile-ui-kit.git#v1.1.0"
```

Or via Package Manager → **Add package from git URL…**

---

## Quickstart

1. Open **`KitforgeLabs → UI Kit → Bootstrap Defaults`**. Generates 10 `UIAnimPreset` assets at `Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets/`. The package itself ships `Theme_Default.asset` (plus `Theme_Casual` and `Theme_Premium`) under `Runtime/Theme/Presets/` — no buyer-side theme generation required.
2. Drag `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab` into your scene. The prefab ships pre-wired with `UIManager` + `PopupManager` + `ToastManager` + `UIServices` + `Canvas` + 3 RectTransform roots + `EventSystem` + a global `PopupBackdrop`. The `KitforgeThemeBinder` component on the root distributes the active theme to all three managers in `Awake` via a single Inspector field.
3. Open **`KitforgeLabs → UI Kit → Hub`**. Use the Setup wizard to confirm the scene is wired, browse the Catalog tab to copy `Show<T>(dto)` / `Push<T>(dto)` snippets straight into your scripts, and run Theme Studio to swap between `Theme_Default` / `Theme_Casual` / `Theme_Premium`.
4. Register your custom screens / popups on the `UIManager` / `PopupManager` Inspector arrays. Catalog popup types are already known by the kit — wire prefab instances of them in the same arrays.
5. Press **Play**, drive the flow from your game code, watch the stack and popup queue react.

---

## Spawn patterns

```csharp
// Popup (modal / gameplay / meta) — PopupManager
var popup = _popupManager.Show<ConfirmPopup>(new ConfirmPopupData {
    Title = "Quit?", Message = "Progress will be lost.",
    ConfirmLabel = "Yes", CancelLabel = "Stay"
});
popup.OnConfirmed += () => QuitToMenu();

// Screen (full-canvas, stack-based) — UIManager
_uiManager.Push<MainMenuScreen>(new MainMenuScreenData { Title = "MyGame" });

// Toast (non-blocking, auto-dismiss) — ToastManager
_toastManager.Show<NotificationToast>(new NotificationToastData {
    Message = "Saved!", Severity = ToastSeverity.Success
});
```

The Hub's Catalog tab generates these snippets for every catalog element — search by name, copy, paste.

---

## Theme

`UIThemeConfig` is a `ScriptableObject` with 16 colors · sprite slots · audio cues · `_defaultAnimPreset` · safe-area config. Three presets ship in `Runtime/Theme/Presets/`:

- `Theme_Default` — neutral baseline.
- `Theme_Casual` — bright, saturated.
- `Theme_Premium` — dark, desaturated.

Swap at design time: assign any `UIThemeConfig.asset` to `KitforgeRoot/KitforgeThemeBinder._theme`.

Swap at runtime: `_themeBinder.SetTheme(myTheme)` re-distributes to the three managers and re-Initializes cached instances.

Create your own: **Assets → Create → KitforgeLabs → UI Kit → Theme** (or duplicate an existing preset).

---

## Architecture decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | Runtime asmdef has zero DI dependency | Buyers without VContainer must boot in one step. Service binding lives in `UIServices` MonoBehaviour (Inspector setters); DI users wire their container resolution there at boot. |
| 2 | `UIManager` is a `MonoBehaviour`, not a singleton | One scene = one manager via Inspector references. |
| 3 | Screens registered as a flat `UIModuleBase[]` registry, resolved by `Type` | Inspector authoring trivial. One prefab per concrete `UIModule` subclass. |
| 4 | Popup priority is an `enum` (Meta / Gameplay / Modal) | Three buckets cover every hybrid-casual case. |
| 5 | `PopupRecord` stores `Data` so eviction can re-enqueue with state | A modal evicting a Gameplay popup must restore it with its original payload. |
| 6 | `_pendingQueue` is a `List<PopupRecord>` with insertion sort | Bounded by `MaxDepth = 3`. |
| 7 | VContainer is opt-in, never a Runtime reference | Avoids forcing a DI container on buyers who use Zenject, no DI, or their own injector. |
| 8 | A single `UIThemeConfig` asset feeds all three managers | "Skin it once" is the kit's pitch. Bootstrap Defaults + `KitforgeThemeBinder` auto-wire this. |

---

## License

Proprietary — KitforgeLabs. See `LICENSE.md` (TBD before public release).

---

## Changelog

See [`CHANGELOG.md`](./CHANGELOG.md). Versioning follows [SemVer](https://semver.org/).
