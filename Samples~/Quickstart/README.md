# Quickstart — Sample

> Zero dependencies. No DI. Two `[ContextMenu]` entries. Drop the prefab in a scene and press Play.

This sample shows the smallest viable wiring of `UIManager` + `PopupManager`. If you need a VContainer-driven setup with service stubs, import the `Game Wiring` sample instead.

## What ships

| File | Purpose |
|---|---|
| `QuickstartBootstrap.cs` | One `MonoBehaviour` with `[SerializeField]` references to `UIManager` and `PopupManager`. Exposes four `[ContextMenu]` entries: `Push Quickstart Screen`, `Pop Screen`, `Show Quickstart Popup`, `Dismiss All Popups`. |
| `QuickstartScreen` (in same .cs) | Trivial `UIModuleBase` that logs `OnShow` / `OnHide`. Attach to your screen prefab. |
| `QuickstartPopup` (in same .cs) | Trivial `UIModuleBase` that logs `OnShow` / `OnHide` / `OnBackPressed`. Attach to your popup prefab. |
| `KitforgeLabs.MobileUIKit.Quickstart.asmdef` | Asmdef referencing `KitforgeLabs.MobileUIKit` only. **No VContainer**, no UniTask, no DOTween. |

## Scene setup (5 minutes)

1. **Create a Canvas.**
   `GameObject → UI → Canvas`. Set Render Mode to `Screen Space - Overlay`. Add a `CanvasScaler` set to `Scale With Screen Size` (1080×1920 reference).

2. **Create two empty roots under the Canvas.**
   - `ScreenRoot` (RectTransform stretched to parent)
   - `PopupRoot` (RectTransform stretched to parent, sibling **after** `ScreenRoot` so popups render on top)

3. **Create a `UIThemeConfig` asset.**
   Project window → right-click → `Create → Kitforge Labs → UI Theme Config`. Name it `QuickstartTheme`.

4. **Add managers and bootstrap.**
   On the Canvas (or a dedicated `_UI` GameObject under it), add three components on the same GameObject (or separate children, your choice):
   - `UIManager`  → assign `_themeConfig` = `QuickstartTheme`, `_screenRoot` = `ScreenRoot`.
   - `PopupManager` → assign `_popupRoot` = `PopupRoot`. Leave `_backdrop` empty for now (or wire a fullscreen `Image` GameObject if you want the dimmer).
   - `QuickstartBootstrap` → assign `_uiManager` and `_popupManager`.

5. **Build one Screen prefab and one Popup prefab.**

   **Screen prefab:**
   - Create a child of `ScreenRoot` named `QuickstartScreenPrefab`.
   - Add a fullscreen `Image` (e.g. solid colour) so you can see it on screen.
   - Add the `QuickstartScreen` component.
   - Drag it from the Hierarchy into the Project window to make it a prefab. Delete the scene instance.
   - On `UIManager`, add the prefab to `_screenPrefabs`.

   **Popup prefab:**
   - Create a child of `PopupRoot` named `QuickstartPopupPrefab`.
   - Add a centred `Image` (e.g. 600×400) so you can see it.
   - Add the `QuickstartPopup` component.
   - Drag to Project, delete scene instance.
   - On `PopupManager`, add the prefab to `_popupPrefabs`.

## Run it

1. Press **Play**.
2. Right-click `QuickstartBootstrap` in the Hierarchy → context menu:
   - `Push Quickstart Screen` → screen instantiates, activates, logs `OnShow`.
   - `Show Quickstart Popup` → popup instantiates on top, logs `OnShow`.
   - `Pop Screen` → screen deactivates, logs `OnHide`.
   - `Dismiss All Popups` → popup deactivates, logs `OnHide`.
3. Watch the Console — every transition is logged.

## What this sample is **not**

- Not a production setup. No animations, no theming applied to UI elements, no input wiring.
- Not a router demo. `UIRouter` is exercised in the `Game Wiring` sample, not here.
- Not a popup priority demo. Both context-menu calls use `PopupPriority.Gameplay`. Override the call site to test eviction (`Modal` over `Gameplay`).
- Not a pattern reference. Real screens with payload derive from `UIModule<TData>`; the samples here use `UIModuleBase` directly because they have no data to bind.

## Next step

Once this runs, read the package `README.md` (`Architecture decisions` table) and import the `Game Wiring` sample for the VContainer-driven version.
