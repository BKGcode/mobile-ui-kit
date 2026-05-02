# Catalog — Group A — Pure UI

The first 4 catalog elements with no service dependencies: ConfirmPopup, PausePopup, TutorialPopup, NotificationToast.

## Two-step setup

This sample ships the host script (`CatalogGroupADemo.cs`) and asmdef. Prefabs and the Demo scene are **generated on demand** by an Editor menu so they integrate cleanly with whatever Theme you have.

1. Run `Tools/Kitforge/UI Kit/Bootstrap Defaults` once (creates `UIThemeConfig_Default` + 10 anim presets at `Assets/Settings/`).
2. Run `Tools/Kitforge/UI Kit/Build Group A Sample`. This creates 4 prefabs + 1 Demo scene under `Assets/Catalog_GroupA_Demo/`.
3. Open `Catalog_GroupA_Demo.unity`, press Play.
4. Right-click the `Demo` GameObject in the scene Hierarchy → `Context Menu` → pick a scenario.

## What you get

| Element | Trigger context-menu | Demonstrates |
|---|---|---|
| `ConfirmPopup` | `Confirm — Neutral`, `Destructive`, `Positive`, `SingleButton` | Tone tinting + back-press routing + single-button alert collapse |
| `PausePopup` | `Pause — Default`, `Pause — All Buttons` | Time.timeScale capture/restore, Dismissing vs Shortcut button categories, inline toggles |
| `TutorialPopup` | `Tutorial — 3 Steps`, `Tutorial — Loop` | Multi-step navigation, Done-label mutation, loop wrap, backdrop modes |
| `NotificationToast` | `Toast — Info`, `Success`, `Warning`, `Error` | Severity → color/icon/audio mapping, tap-to-dismiss, slide-in animation |

## Where the prefabs live

`Assets/Catalog_GroupA_Demo/Prefabs/`. The generated prefabs reference your project's TMP_Essentials and the active Theme. Reskinning is done via the Theme; do NOT modify the prefabs directly unless you intend to fork.

## Theme contract — "skin it once" works like this

Each generated prefab carries `ThemedImage` and `ThemedText` components on the elements that should respond to your `UIThemeConfig`. When `Initialize(theme, services)` runs (via `PopupManager` / `ToastManager` / the Demo host), the popup walks its child hierarchy and applies the Theme to every `IThemedElement` it finds. You can replace any `Sprite` / `Color` / `Font` slot in your Theme asset and ALL four prefabs pick it up the next time they're instantiated.

| Element | Themed surface |
|---|---|
| `Card` (popup background) | `Sprite=PanelBackground`, `Color=BackgroundLight` |
| Primary button (Confirm / Resume / Next) | `Sprite=ButtonPrimary`, `Color=PrimaryColor` |
| Secondary button (Cancel / Restart / Settings / Skip / Back / Home / Shop / Help) | `Sprite=ButtonSecondary`, `Color=SecondaryColor` |
| Quit button | `Sprite=ButtonSecondary`, `Color=DangerColor` |
| Title labels | `Font=FontHeading`, `Color=TextPrimary` |
| Body / Subtitle / Step body | `Font=FontBody` (or `FontCaption` for subtitle), `Color=TextSecondary` |
| Toast message | `Font=FontBody` (color stays light-on-tint) |

What the Theme does NOT drive (intentional):
- Backdrop dim (universal black 55%).
- ConfirmPopup ToneStrip — driven by `ConfirmPopupData.Tone` via `Theme.PrimaryColor` / `DangerColor` / `SuccessColor` at runtime, not a fixed Theme slot.
- Toast `SeverityTint` and `SeverityIcon` — driven by `NotificationToastData.Severity` via the severity → color/icon mapping in `NotificationToast`, not the static Theme slots.
- Toggle visuals (Sound / Music / Vibration) — kept minimal; reskin via prefab if you ship them.

Limitation: theme changes after a popup is already on screen do NOT auto-re-apply. The popup reads the Theme on `Initialize`, which runs once at instantiate.

## Hierarchy stability — what you can and can't move

The popups expose a private `Refs` struct that holds direct references to specific child Transforms (TitleLabel, BackdropButton, ConfirmTint, etc). Re-parenting or deleting those children in a prefab variant **silently breaks** the references at runtime — Unity does not warn at edit time.

Safe edits inside a prefab variant:
- Change colors, sprites, fonts via `ThemedImage` / `ThemedText` slots or by replacing the Theme asset.
- Resize / re-anchor existing elements (their `RectTransform` is yours to lay out).
- Add new sibling elements (decorative icons, particles, secondary text) — they don't interfere with `Refs`.
- Restyle Toggle visuals (the kit ships minimal toggles; bring your own).

NOT safe (will break Refs):
- Re-parenting an existing referenced child to a different parent in the hierarchy.
- Deleting a referenced child (TitleLabel, ConfirmButton, etc).
- Renaming the `Refs` struct fields by editing source — the prefab serializes by SerializedProperty path.

If you need a structural change, fork the prefab into your project and update the `Refs` struct in a derived popup class. The kit's contract is the popup behavior + DTO + events — not the prefab hierarchy.

## Re-running the builder

`Build Group A Sample` is idempotent — running it again overwrites any existing prefabs/scene at the output path. If you've customized the prefabs and want to keep your changes, **rename the output folder** (`Assets/Catalog_GroupA_Demo/` → `Assets/MyGame_Popups/`) before re-running, or move the prefabs out of the default path. The builder always targets `Assets/Catalog_GroupA_Demo/`.

## See also

- Specs: `Packages/com.kitforgelabs.mobile-ui-kit/Documentation~/Specs/Catalog/{ConfirmPopup,PausePopup,TutorialPopup,NotificationToast}.md`
- Quickstart sample (Phase 1 framework, no catalog elements): `Samples~/Quickstart/`
- Game wiring with VContainer: `Samples~/GameWiring/`
