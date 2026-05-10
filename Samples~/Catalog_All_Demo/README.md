# Catalog — All — Single-import master demo

Single-scene master demo that proves the kit's "wire it in five minutes, skin it once, ship it" claim end-to-end. Wires every shipped catalog element (10 popups + 1 toast + 2 screens + 5 HUDs) into ONE scene with buttons-per-element + a theme dropdown.

This is the workshop-drawer demo: import this sample, run the builder, press Play, click any button — the corresponding popup / toast / screen spawns and re-skins live across 3 themes (Default / Casual / Premium) without touching prefabs or code.

## Prerequisites

**Step 0 — Bootstrap Defaults (one-time, removes "No animation preset resolved" warnings):**

```
Tools → Kitforge → UI Kit → Bootstrap Defaults
```

This generates 10 `UIAnimPreset` ScriptableObjects under `Assets/Settings/UIAnimPresets/` (entry / exit / punch / shake / etc) and wires the default one onto `Theme_Default.asset` so popups animate on Show. **Skipping this step is harmless** — popups still spawn correctly, but each `OnShow` logs a `[XPopup] No animation preset resolved — popup will appear without animation` warning. Animation presets live under `Assets/` (not `Packages/`) so the package cannot pre-wire them; this is a one-time setup.

**Then the 5 catalog group samples + M4.1 Theme Presets sample MUST be imported and built** — the master demo loads their prefabs by path and reuses their in-memory service stubs.

In Package Manager → **Kitforge Mobile UI Kit → Samples**, import (in order):

1. **Catalog — Group A — Pure UI** → run `Tools → Kitforge → UI Kit → Build Group A Sample`.
2. **Catalog — Group B — Currency** → run `Tools → Kitforge → UI Kit → Build Group B Sample`.
3. **Catalog — Group C — Progression** → run `Tools → Kitforge → UI Kit → Build Group C Sample`.
4. **Catalog — Group D — Player Data** → run `Tools → Kitforge → UI Kit → Build Group D Sample`.
5. **Catalog — Group E — Screens** → run `Tools → Kitforge → UI Kit → Build Group E Sample`.
6. **Catalog — M4.1 — Theme Presets** → run `Tools → Kitforge → UI Kit → Build M4.1 — Theme Presets`.

Each group builder materializes the popup prefabs + a per-group demo scene under `Assets/Catalog_Group<X>_Demo/`. The master demo reuses those prefabs.

## Build

After all prerequisites are imported and built, run:

```
Tools → Kitforge → UI Kit → Build Catalog_All_Demo
```

The builder generates `Assets/Catalog_All_Demo/AllDemo.unity` — a wired scene with:

- 1 KitforgeRoot prefab (Canvas + UIManager + PopupManager + ToastManager + EventSystem)
- 1 UIServices GameObject with 8 in-memory service stubs auto-attached (Economy / PlayerData / Progression / ShopData / Ads / Time / Audio / Localization)
- A right-edge sidebar of buttons grouped by category (Group A / B / C / D / E + HUD test)
- A top-of-canvas Theme dropdown (Default / Casual / Premium) wired to live re-skin via `UIManager/PopupManager/ToastManager.SetTheme()`

## Validate the contract

1. Open `Assets/Catalog_All_Demo/AllDemo.unity`. Press **Play**.
2. Click any popup button — the popup spawns with Default theme palette.
3. Use the **Theme** dropdown (top of canvas) — pick `Casual`. Click the same popup button again. **Same prefab, different palette.** Repeat with `Premium`.
4. Click `Loading Screen` — the screen pushes onto the UIManager stack. Click `Main Menu Screen` to replace.
5. HUD test buttons (Add Coins / Add Gems / Spend Coins) trigger `IEconomyService` mutations — any `HUDCurrency` (Coins / Gems) prefab dropped into the scene reflects the value live.

If at least 8 of the 13 catalog buttons spawn cleanly across all 3 themes, the master "skin it once, ship it" contract is validated.

## How the host wires popups

`CatalogAllDemoHost.cs` is the canonical demo of the buyer path. Each click handler is one line:

```csharp
_popupManager.Show<RewardPopup>(new RewardPopupData
{
    Title = "Reward!",
    Kind = RewardKind.Coins,
    Amount = 100,
});
```

`PopupManager.Show<T>(dto)` resolves the prefab from the `_popupPrefabs[]` Inspector array (see Quickstart § Manual prefab registration) and runs the full lifecycle (`Initialize` → `Bind(dto)` → `Show()`). No reflection, no boilerplate.

For toasts: `_toastManager.Show<NotificationToast>(dto)`. For screens: `_uiManager.Push<LoadingScreen>(dto)`.

## Limitations

- **Live re-skin on an open popup** — when the theme changes, currently-open popups capture the new theme on their next `Initialize` (next `Show` call). Open popups dismiss + respawn on theme change in this demo. Documented kit limitation.
- **NotificationToast is the only Toast type shipped.** ToastSeverity Info / Warning / Error variants are dto-driven.
- **Service stubs are in-memory.** All currencies / settings / time / progression state resets on Play stop. This demo is intentionally non-persistent — it validates the UI contract, not save state. The Game Wiring sample shows VContainer-driven persistent stubs.
