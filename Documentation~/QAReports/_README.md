# UI Kit Audit — Release Evidence

This folder holds the latest audit `_Summary.md` snapshot, committed to the package repo as release evidence for tagged versions.

## Why a separate folder

The `MirrorReportsToAssets` toggle in the audit window (Settings tab) writes to `Assets/Editor/QAReports/` of the **consumer Unity project**, which is where buyers commit their own audit evidence. The package's own developer Unity project (currently `H:/==GIT==/PACHINKO/PACHINKO/`) is not git-tracked, so its `Assets/Editor/QAReports/` cannot serve as release evidence for the package itself.

This `Documentation~/QAReports/` folder lives **inside the package repo** (`com.kitforgelabs.mobile-ui-kit`) and IS git-tracked. The `~` suffix excludes it from Unity asset processing in consumer projects, so it does not bloat the kit installation footprint.

## Pre-tag ritual

Run before every package version tag (`v0.X.Y-alpha`, `v1.0.0-rc`, etc.):

1. Open `Tools → Kitforge → UI Kit → Audit` in the developer Unity project.
2. Click `Clear All` (purges stale reports + snapshots).
3. Click `Regenerate + Audit`. Confirm the Console log reads `[UIKitAudit] RegenAndAudit complete · N/N pass · 0 fail`.
4. If green: copy `Library/UIKitAudit/Reports/_Summary.md` from the developer Unity project to this folder, overwriting `_Summary.md`. Optional: also copy per-target `*.md` and `*.json` files for granular evidence.
5. Optional but recommended: rename or copy the file to a versioned subfolder like `v1.0.0-rc/_Summary.md` before overwriting the top-level `_Summary.md`, so the package repo retains historical evidence per tag.
6. Commit the updated `_Summary.md` (and any versioned subfolder) in the package repo.
7. Tag the package version.

## Why `Library/.../Reports/_Summary.md` is the source of truth

The audit window writes reports to `Library/UIKitAudit/Reports/_Summary.md` of the developer Unity project on every `Regenerate + Audit` (or `Run Everything`). That path is ephemeral (gitignored, regenerated on each run) but is the canonical output of the audit pipeline. The `MirrorReportsToAssets` toggle copies the same file to `Assets/Editor/QAReports/` for buyer-facing evidence; the developer copies the same file here for package-side evidence.

## What `_Summary.md` proves

A green `_Summary.md` (`N/N pass · 0 fail`) at tag time proves:

- All catalog prefabs (`Catalog: N/N`) have valid `_refs` wiring, all `ThemedImage`/`ThemedText` backing fields are non-null, and spec markdown counts match runtime themed counts.
- All demo scenes (`Scenes: N/N`) load without missing scripts or broken references.
- No structural integrity regressions in the kit since the previous tag.

If the audit fails (`X/N pass · Y fail`), do NOT tag — fix findings, re-run, then tag.
