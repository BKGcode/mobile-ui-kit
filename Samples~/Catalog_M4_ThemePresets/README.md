# Catalog — M4.1 — Theme Presets

Validates the kit's "skin it once" claim by reskinning **GameOver** + **LevelComplete** popups across 3 themes (Default / Casual / Premium) without touching prefabs or code.

## Prerequisites

- Bootstrap Defaults run (`Tools → Kitforge → UI Kit → Bootstrap Defaults` — generates `Assets/Settings/UI/UIThemeConfig_Default.asset`).
- Group B sample imported + built (`Build Group B Sample` — provides `InMemoryEconomyService` / `InMemoryAdsService` stubs).
- Group C sample imported + built (`Build Group C Sample` — provides `GameOverPopup.prefab` + `LevelCompletePopup.prefab` + `InMemoryProgressionService` / `InMemoryTimeService`).

## Build

After importing this sample, run:

```
Tools → Kitforge → UI Kit → Build M4.1 — Theme Presets
```

The builder generates under `Assets/Catalog_M4_ThemePresets_Demo/`:

- `Themes/Theme_Casual.asset` — bright/saturated palette (Royal Match / Disney Getaway Blast vibe). Sprites/fonts/audio cloned from `UIThemeConfig_Default`; only colors differ.
- `Themes/Theme_Premium.asset` — dark/desaturated palette (Hades / Genshin menu vibe). Same clone strategy.
- `ThemePresetsDemo.unity` — wired scene with `ThemeSwitcherSample` MonoBehaviour, top-of-canvas dropdown (3 options) + `Show GameOver` / `Show LevelComplete` buttons.

## Validate the contract

1. Open `ThemePresetsDemo.unity`. Press **Play**.
2. Click **Show GameOver**. Note the visual identity (button colors, header tint, card panel).
3. Use the **Theme** dropdown — pick `Casual` then `Premium`. The popup automatically closes and reopens with the new palette. **Same prefab + 3 themes = 3 distinct visual identities, zero code or prefab edit.**
4. Click **Show LevelComplete**. Repeat the dropdown switch. Star sprites stay the same (cloned from Default); colors drive the reskin.

If at least 2 of the 3 themes produce visibly different button hierarchy + header accents on both popups, the "skin it once" contract is validated.

## Add your own theme

The dropdown reads from a `List<ThemeOption>` on `ThemeSwitcherSample` (Inspector). To add a 4th theme:

1. `Create → Kitforge → UI Kit → Theme Config` → save as `Theme_<YourBrand>.asset`.
2. Open `ThemePresetsDemo.unity` → select the `Demo` GameObject.
3. In the `Themes` list, add a new row: `Name = "<YourBrand>"`, `Theme = <your asset>`.
4. Press Play. The dropdown picks up the new option automatically.

No script edits required.

## Limitations

- **Live re-skin on an open popup is not supported.** Catalog popups capture `Theme` at `Initialize` time; changing the theme dropdown while a popup is open dismisses + respawns it. This is the documented sample pattern.
- **Sprites/fonts/audio are shared across the 3 themes** in this sample. Casual/Premium clone Default's non-color refs at build time. If you want sprite-level reskin (e.g. cartoon icons in Casual, minimal-line icons in Premium), edit the generated `.asset` files and assign your own slot values — the kit reads them transparently.
- **CloseOnBackdrop is false on the demo GameOver** so you can't dismiss by tapping the backdrop while exploring (use Restart / MainMenu / Continue or the Back button instead).
