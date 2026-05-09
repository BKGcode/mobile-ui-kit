# Catalog — M4.1 — Theme Presets

Validates the kit's "skin it once" claim by reskinning **GameOver** + **LevelComplete** popups across 3 themes (Default / Casual / Premium) without touching prefabs or code.

## Prerequisites

- `Theme_Default` shipped in the package at `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset` (no Bootstrap Defaults required — it ships pre-wired).
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
- **`ThemedImage` runtime read is currently wired only on Group C catalog (`DailyLogin`, `LevelComplete`, `GameOver`, `HUD-Energy`, `HUD-Timer`).** This sample shows `GameOverPopup` + `LevelCompletePopup`, so the "skin it once" contract is validated end-to-end here. If you extend the sample with Group A / B / D / E popups, or build a scene that uses them, the dropdown swap will NOT visibly reskin those popups — their builders still hardcode `Image.color`. The sweep to A / B / D / E lands in M4.7-bis (next sub-step in milestone M4, pre-tag `v0.9.0-alpha`). Until then, treat Group C reskin as the canonical demo of the contract.
- **Secondary buttons (Restart / Main Menu / Watch ad / Retry) do not reskin in M4.7.** `Theme_Default._secondaryColor` and the builder's `ButtonSecondaryColor` constant differ by Δ 0.5 — migrating Secondary buttons to the slot today would change the Editor preview into a darker palette with poor text contrast. Reconciliation requires a cross-theme palette decision and lands in M4.7-bis. HUD backgrounds and sprite icons stay hardcoded by design (no semantic slot for translucent overlays; sprites are sprite-driven via `ThemeSpriteSlot`, not color-driven).
