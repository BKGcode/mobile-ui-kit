# Quickstart — Sample

> Zero dependencies. No DI. Two `[ContextMenu]` entries. Drop the prefab in a scene and press Play.

This sample shows the smallest viable wiring of `UIManager` + `PopupManager`. A VContainer-driven setup with service stubs is parked under `Samples~/GameWiring/` (not exposed in Package Manager) until service contracts stabilize at the end of Group D.

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

## Manual prefab registration (canonical path)

There is **no `PopupManager.RegisterPrefab(Type, GameObject)` runtime API.** Prefab registration in this kit is **Inspector-driven**: drop your popup / screen / toast prefab into the corresponding `[SerializeField] _popupPrefabs[]` / `_screenPrefabs[]` / `_toastPrefabs[]` array on the manager component, and the resolver in `PopupManager.Show<T>()` (and `UIManager.Push<T>()` / `ToastManager.Show<T>()`) walks the array looking for the entry whose root `GetType() == typeof(T)`.

**Why Inspector-only?** Because the kit's contract is "wire it once at design time, ship it." Runtime mutation of the prefab map is out of scope (no `RegisterPrefab` / `UnregisterPrefab` API surface, no event for "prefabs changed"). If a buyer needs runtime registration (e.g. a DLC system loading new popups at runtime), they hold a reference to the manager's `_popupPrefabs` field via reflection or extend the manager — out of scope for v1.0.

**Step-by-step for adding a new popup type:**

1. Create your popup class: `public sealed class MyPopup : UIModule<MyPopupData> { ... }` plus `[Serializable] public class MyPopupData { ... }`.
2. Create the prefab: a `RectTransform` child of `PopupRoot` with `MyPopup` component on the root + your visual children.
3. Drag from the Hierarchy into Project to make it a prefab; delete the scene instance.
4. **On the `PopupManager` component in your scene, expand `_popupPrefabs` in the Inspector and drag `MyPopup.prefab` into a new array slot.**
5. Call from your gameplay code: `_popupManager.Show<MyPopup>(new MyPopupData { ... });`

The same flow applies to:
- **Screens** → drag prefab to `UIManager._screenPrefabs[]`, call `_uiManager.Push<MyScreen>(data)`.
- **Toasts** → drag prefab to `ToastManager._toastPrefabs[]`, call `_toastManager.Show<MyToast>(data)`.

**Validation check:** `[ContextMenu("Validate")]` on `UIServices` walks each slot and warns about nulls. The `_popupPrefabs[]` array does NOT have a built-in null-slot check — drop only valid prefabs and the resolver will silently return null for `Show<T>` when `T` is not in the array (the caller can inspect the return value and act on null).

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
- Not a router demo. `UIRouter` is exercised by host games via `UIServices` setters; the parked GameWiring sample carries an example.
- Not a popup priority demo. Both context-menu calls use `PopupPriority.Gameplay`. Override the call site to test eviction (`Modal` over `Gameplay`).
- Not a pattern reference. Real screens with payload derive from `UIModule<TData>`; the samples here use `UIModuleBase` directly because they have no data to bind.

## Next step

Once this runs, read the package `README.md` (`Architecture decisions` table) and import the `Catalog — Group A — Pure UI` sample to materialize the 4 catalog prefabs + Demo scene.
