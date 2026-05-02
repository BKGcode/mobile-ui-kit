# Memory — KF_MobileUIKit

Project-local knowledge that survives across Claude Code sessions on this UPM package.

---

## Themed* contract is canonical for ALL catalog elements

`Runtime/Theme/IThemedElement` + `ThemedImage` + `ThemedText` + `ThemeSpriteSlot` / `ThemeColorSlot` / `ThemeFontSlot` / `ThemeFontSizeSlot` enums are the kit's first-class theming contract. `UIModuleBase.Initialize` and `UIToastBase.Initialize` walk children and call `ApplyTheme` on every `IThemedElement` found.

**Why:** v0.4.0-alpha session (2026-05-02) shipped Group A with literal hardcoded colors in the builder. The TA `/_checker` caught that the `Theme.PanelBackground` / `ButtonPrimary` / `ButtonSecondary` slots were not consumed by generated prefabs. The "skin it once" pitch was broken without the buyer noticing — until they tried to reskin and discovered they had to edit each prefab manually. Themed* contract closes the gap retroactively for Group A.

**How to apply (Groups B-E):**
- Every Card background → `ThemedImage(PanelBackground, BackgroundLight)`
- Every primary button → `ThemedImage(ButtonPrimary, PrimaryColor)`
- Every secondary button → `ThemedImage(ButtonSecondary, SecondaryColor)`
- Every danger button (Quit, Delete, etc.) → `ThemedImage(ButtonSecondary, DangerColor)`
- Every Title TMP → `ThemedText(FontHeading, TextPrimary)`
- Every body / message TMP → `ThemedText(FontBody, TextSecondary)`
- Every caption / progress TMP → `ThemedText(FontCaption, TextSecondary)`
- Button labels (white-on-primary) → `ThemedText(FontBody, None)` — None preserves the literal white

What stays NON-themed by design (do not retro-fit):
- Backdrop dim (universal black 55%)
- ConfirmPopup `ToneStrip` (driven by data via `ApplyTone`)
- Toast `SeverityTint` and `SeverityIcon` (driven by data via severity mapping)

Wire at builder-time, never as a post-checker mejora — the v0.4.0 cascade cost ~250 lines of refactor for what should have been native from day 1.

---

## Editor builder pattern as bridge for catalog prefab generation

`Editor/Generators/CatalogGroupABuilder.cs` (MenuItem `Tools/Kitforge/UI Kit/Build Group A Sample`) materializes 4 prefabs + 1 Demo scene at `Assets/Catalog_GroupA_Demo/`. Output is idempotent — re-running overwrites. Companion sample at `Samples~/Catalog_GroupA_PureUI/` ships the host MonoBehaviour + asmdef + README.

**Why:** v0.4.0 session was authored without Unity Editor present. Shipping prefabs as binary assets (`.prefab`) requires Unity to resolve TMP / Image / GUID references. The builder pattern decouples authoring (Claude in CLI) from materialization (Juan in Editor). Side benefit: prefabs are regeneratable across kit versions without GUID drift.

**How to apply (Groups B-E):**
- Extend `CatalogGroupABuilder` with new `Build*Popup` / `Build*Screen` private methods. Do NOT fork into `CatalogGroupBBuilder` etc. — single class with one MenuItem per group entry point keeps maintenance localized.
- Always wire `ThemedImage` / `ThemedText` at build time (see `AddThemedImage` / `AddThemedText` / `CreatePrimaryButton` / `CreateSecondaryButton` / `CreateThemedLabelledButton` helpers).
- Output path `Assets/Catalog_Group{X}_Demo/` per group; stays consistent.
- Demo scene host MonoBehaviour lives in `Samples~/Catalog_Group{X}_*/`; one `[ContextMenu]` per scenario.

Limitation: builder writes to `Assets/`, not `Packages/.../Samples~/`. Juan's pre-tag flow: run menu → manually copy outputs into `Samples~/Catalog_Group{X}_*/` → commit. Buyer's flow: import sample → run menu → outputs land in their project. Both paths supported.

---

## Hierarchy stability — buyer-facing disclosure for catalog prefab variants

Catalog elements expose `private struct Refs` with direct child references (TitleLabel, BackdropButton, ConfirmTint, etc.). Re-parenting or deleting a referenced child in a prefab variant **silently breaks** the references at runtime — Unity does not warn at edit time.

**Why:** Sample README documents the contract (`Samples~/Catalog_GroupA_PureUI/README.md` → "Hierarchy stability" section). Without it, TAs reskin freely, ship the variant, and discover the bug at QA. The README disclosure is the canonical buyer-facing template.

**How to apply (Groups B-E):**
- Copy the "Hierarchy stability" section verbatim into every new sample README.
- Safe edits: change colors / sprites / fonts via Theme, resize, re-anchor, add new sibling decorative elements, restyle Toggles.
- NOT safe: re-parenting or deleting referenced children, renaming `Refs` struct fields.
- Buyer needing structural changes forks the prefab + derives from the popup class (their concrete `Refs` struct).

This is a kit-wide design constraint — applies to every catalog element with a Refs struct, not just Group A.
