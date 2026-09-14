---
name: project_conventions
description: Project-specific rules for KF_MobileUIKit UPM package. Read at session start. Precedence over ~/.claude/ globals ONLY when explicitly stated here.
type: reference
---

# Project Conventions — KF_MobileUIKit (UPM package)

UPM package `com.kitforgelabs.mobile-ui-kit` targeting Unity Asset Store ($30-60 price band). Global ecosystem rules in `~/.claude/memory/unity-rules` and `~/.claude/memory/ecosystem_philosophy.md` apply unless contradicted here.

## Studio brand — `KitForge Labs` (display canon) · `KitforgeLabs` (code identifiers)

Studio-wide brand decision (revised 2026-09-14, user-approved — supersedes the 2026-05-17 one-word lock for DISPLAY surfaces only): the **displayed** studio name is **`KitForge Labs`** (two words), aligned across all KitForge Labs products (techart-tools already presents this way; mobile-ui-kit now matches — separate packages, shared studio brand). Code identifiers already frozen under the one-word `KitforgeLabs` spelling stay as-is — renaming them breaks installs, buyer references, and saved assets.

**Display attribution — use `KitForge Labs` (two words):**
- `package.json` `displayName`: `KitForge Labs UI Kit`
- `package.json` `author.name`: `KitForge Labs`
- Top-level menu (buyer-facing): `KitForge Labs/UI Kit/...` (only Hub + Open Demo Scene) + the `Create → KitForge Labs → UI Kit → Theme / Anim Preset` asset menus
- README.md attribution + heading + footer, and any buyer-visible label / dialog / log text
- Hub window title

**Code identifiers — stay one-word `KitforgeLabs` (do NOT rename — breaking):**
- Package id: `com.kitforgelabs.mobile-ui-kit`
- Namespace: `KitforgeLabs.UIKit.*` (NOT `KitforgeLabs.MobileUIKit.*` — renamed in v1.1.0)
- Asset folder root in consumer projects: `Assets/KitforgeLabs/UI Kit/Settings/...`
- Kit-author maintenance menus: `Tools/KitforgeLabs/Test/...` (dev-only, `KITFORGE_DEV_MAINTENANCE`-gated; not buyer-facing, so outside the display rebrand)

`BKGcode` is the **GitHub account/organization slug only**, never used as studio attribution. Acceptable contexts: repo slug `BKGcode/mobile-ui-kit`, GitHub URLs.

**Why:** the studio ships more than one product; buyers must read techart-tools and mobile-ui-kit as the same studio. The 2026-05-17 single-referent lock predated the multi-product brand and is superseded for display surfaces only. Casing: `KitForge Labs` for display, `KitforgeLabs` (capital `K` only) for code.

**How to apply:** any new BUYER-VISIBLE surface uses `KitForge Labs`; any code identifier keeps the frozen one-word `KitforgeLabs`. Shipped 2026-09-14 (commit `3c0bb3a`).

---

## Architecture overrides (specific to KF_MobileUIKit, beyond unity-rules)

1. **Runtime asmdef has ZERO DI dependency.** Buyers without VContainer must boot in one step. Service binding lives in `UIServices` MonoBehaviour (Inspector setters); DI users wire their container resolution there at boot. Adding a DI reference to Runtime = BREAKING.
2. **Framework managers are MonoBehaviour, NOT singletons.** `UIManager` / `PopupManager` / `ToastManager` exposed via Inspector references on `KitforgeRoot.prefab`. One scene = one manager.
3. **Service interfaces (8) drive game-side data:** `IEconomyService` · `IPlayerDataService` · `IProgressionService` · `IShopDataProvider` · `IAdsService` · `ITimeService` · `IUIAudioRouter` · `IUILocalizationService`. Buyers implement; package ships Null Object defaults (always-bootable) + Demo services (live data for Demo Scene).
4. **Theme = ONE `UIThemeConfig` ScriptableObject feeds all three managers.** "Skin it once" is the kit's pitch. `KitforgeThemeBinder` auto-distributes to UIManager + PopupManager + ToastManager.
5. **Catalog prefabs ship pre-wired** in `KitforgeRoot.prefab` under `Runtime/Catalog/Prefabs/`. No buyer-side prefab wiring required (changed in v1.2.0).
6. **Maintenance tools compile-gated.** `KitforgeLabs.UIKit.Editor.Maintenance.asmdef` defines `defineConstraints: ["KITFORGE_DEV_MAINTENANCE"]`. Kit authors add the define to Player Settings; buyers never see these menus.
7. **`KitforgeRoot.prefab` is the canonical entry point.** Drag into your scene + wire `UIServices` Inspector slots + you're done. No additional bootstrap script needed in production.
8. **Popup priority is `enum` (Meta / Gameplay / Modal).** Three buckets cover every hybrid-casual case. Don't add a fourth bucket without strong justification — it explodes authoring complexity.
9. **`PopupRecord` stores `Data` so eviction can re-enqueue with state.** When a Modal evicts a Gameplay popup, the Gameplay popup must restore with its original payload on re-show. Invariant.

## Test gate

Smoke test suite at `Tests/Editor/Smoke/KitforgeRootSmokeTests.cs` — spawns `KitforgeRoot.prefab` in temp scene, asserts every catalog popup / screen / toast opens cleanly with Null Object defaults. Gates v1.2.0+ releases. NO popups should throw or warn during smoke; tolerated operational warnings are listed in `KitforgeRootSmokeTests.cs` `_expectedOperationalWarnings`.

## UPM-specific rules

- **Workspace lives at `Documentation~/Claude/`** (this folder's parent). UPM convention: `~`-suffixed folders excluded from consumer Unity imports.
- **Author-side editor tools MUST respect `ConfirmWritable`** before writing back to the package. Git-installed packages (consumer side) are read-only; tools must refuse to run there. Reference pattern: `KitforgeCatalogPrefabsRegenerator.ConfirmWritable`. **Known gap (logged in PLAN.md §3 Next):** `KitforgeCatalogWireTool.WireAll` missing this guard — `SaveAsPrefabAsset` throws `ArgumentException` in consumer projects instead of stopping with actionable message.
- **`Documentation~/` content visibility:**
  - `Documentation~/CHEATSHEET.md` — buyer quickstart reference (visible in repo, NOT auto-imported into consumer projects).
  - `Documentation~/Specs/Catalog/*.md` — 14 per-element specs (developer reference).
  - `Documentation~/Specs/Services/*.md` — 2 service interface specs.
  - `Documentation~/Screenshots/*.png` — Hub Catalog tab thumbnails + Hero banner.
  - `Documentation~/Claude/` — workspace (v2 protocol per ecosystem_philosophy §18 UPM variant).

## Versioning

SemVer. Tags `v<major>.<minor>.<patch>` on `main`. CHANGELOG.md `[Unreleased]` block opened pre-tag, closed at tag with version bump in `package.json` + README install URL.

When tagging:
1. Close CHANGELOG `[Unreleased]` → `[X.Y.Z] — YYYY-MM-DD`.
2. Bump `package.json` `version`.
3. Bump README install URL `#vX.Y.Z`.
4. Run `/_plan tidy --post-tag` to rotate PLAN.md §5 Done into CHANGELOG and empty §5.
5. `git tag vX.Y.Z && claude-push mobile-ui-kit`.

## Code style overrides (in addition to unity-rules)

None — package follows unity-rules verbatim.

## Asmdef structure

- `KitforgeLabs.UIKit` (Runtime/) — zero DI dependency.
- `KitforgeLabs.UIKit.Catalog` (Runtime/Catalog/) — catalog elements + prefab registries.
- `KitforgeLabs.UIKit.Editor` (Editor/) — Hub + buyer-facing editor tools.
- `KitforgeLabs.UIKit.Editor.Maintenance` (Editor/Maintenance/) — author-only, gated by `KITFORGE_DEV_MAINTENANCE` define.
- `KitforgeLabs.UIKit.Tests.EditMode` + `KitforgeLabs.UIKit.Catalog.Tests` (Tests/) — smoke + unit tests.
