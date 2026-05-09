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

## ThemedImage helper pattern — `AddThemedImage` + `OverrideThemedImageSlot` (M4.7)

`CatalogGroupCBuilder.cs` (2026-05-06) introduced the canonical helper pair for migrating Image sites to runtime-themable. M4.7-bis.B.1 (2026-05-08) extracted to `Editor/Generators/CatalogGroupBuilderShared.cs` — `internal static class` with `using static` directive in each consumer (NOT `abstract class CatalogBuilderBase` — C# disallows static-class inheritance; Unity-idiomatic pattern matches `AssetDatabase` / `EditorGUIUtility`). 5 consumers (A+B+C+D+E builders) validate composability per ecosystem philosophy §5.

```csharp
private static Image AddThemedImage(GameObject go, Color color, ThemeColorSlot slot)
{
    var img = AddImage(go, color);                 // sets img.color = color (Editor preview)
    var themed = go.AddComponent<ThemedImage>();   // [RequireComponent(Image)] satisfied by AddImage above
    var so = new SerializedObject(themed);
    so.FindProperty("_image").objectReferenceValue = img;
    so.FindProperty("_colorSlot").enumValueIndex = (int)slot;
    so.ApplyModifiedPropertiesWithoutUndo();
    return img;
}

private static void OverrideThemedImageSlot(GameObject go, ThemeColorSlot slot)
{
    var themed = go.GetComponent<ThemedImage>();
    if (themed == null) return;
    var so = new SerializedObject(themed);
    so.FindProperty("_colorSlot").enumValueIndex = (int)slot;
    so.ApplyModifiedPropertiesWithoutUndo();
}
```

**Use case for `OverrideThemedImageSlot`:** when a button base (e.g. `CreatePrimaryButton`) wires `PrimaryColor` slot via `AddThemedImage` and a downstream call needs to change the slot (e.g. DailyLogin claim button → `SuccessColor`) without removing/re-adding the component.

**Slot-vs-hardcoded decision matrix (validated for Group C):**

| Site | Builder constant | Theme slot | Decision |
|---|---|---|---|
| Card backgrounds | `CardColor (0.97, 0.98, 1)` | `BackgroundLight (0.96, 0.97, 0.98)` | Migrate (close enough match) |
| Primary buttons | `ButtonPrimaryColor (0.20, 0.55, 0.95)` | `PrimaryColor` exact | Migrate |
| Success accents (banners, claim btn override, fill bars) | `SuccessTintColor (0.30, 0.78, 0.45)` | `SuccessColor` exact | Migrate |
| Failure accents (header tint, danger banners) | `FailureColor (0.898, 0.22, 0.21)` | `FailureColor` exact | Migrate |
| Secondary buttons | `ButtonSecondaryColor (0.85, 0.86, 0.90)` | `SecondaryColor (0.35, 0.40, 0.50)` Δ 0.5 | KEEP HARDCODED — palette mismatch, defer M4.7-bis |
| Backdrop | `BackdropColor (0, 0, 0, 0.55)` | none semantic | KEEP HARDCODED |
| HUD backgrounds | `HUDBackgroundColor (0, 0, 0, 0.45)` | none semantic | KEEP HARDCODED |
| Sprite-driven icons | `Color.white` passthrough | n/a (sprite resolves color) | KEEP HARDCODED |

Apply same matrix to each builder in M4.7-bis. New mismatches (Secondary buttons, future palette additions) need cross-theme palette decision before migrating.

**Critical companion rule:** post-migration, runtime override audit is mandatory. `image.color = X` in popup `Bind` / `OnShow` post-`ApplyTheme` will silently kill the theme value (validated 2026-05-06 — `GameOverPopup.ApplyButtonAlpha`). Before declaring a builder migration complete, grep popup runtime for `\.image\.color\s*=` patterns. See `feedback_workflow.md` → "Runtime overrides silently kill theme/contract values".

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

---

## CHANGELOG.md navigation — Grep-first, never full Read

`CHANGELOG.md` exceeds the Read tool's 25k token limit (~90k tokens at v0.9.0-alpha and growing). A naked `Read` call fails with `File content (90093 tokens) exceeds maximum allowed tokens (25000)`. File only grows with each release — constraint is permanent.

**Why:** v0.9.0-alpha onward each Unreleased block grows ~5-10k tokens with detailed Fixed entries (M4.7-bis sweep alone added ~25k). Reading the file blindly to locate a section wastes a tool call.

**How to apply:**

1. Locate version blocks first: `Grep -n "^## \[" CHANGELOG.md` returns all `## [X.Y.Z-tag]` headings with line numbers.
2. Locate the target sub-section within a block: `Grep -n "M4\.7-bis|M4 |Fixed|Added|Known issues" CHANGELOG.md`.
3. Read by `offset/limit` for the target line range. Last-updated timestamp is line 6 (long single line — Grep returns "Omitted long matching line", read by offset to inspect).
4. For appending entries: anchor on stable strings like `### Added` or unique entry titles, NOT on full block dumps.

Applies to any markdown changelog in this project that grows monotonically.
