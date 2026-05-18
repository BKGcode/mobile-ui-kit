---
manifest_version: v2
project: kf-mobile-ui-kit
project_root: C:\Users\Joan\mobile-ui-kit-dev
timestamp: 2026-05-18T13:00Z
commit_ref: ""
scope: PROJECT — C:\Users\Joan\mobile-ui-kit-dev
session_status: in progress
plan_md_path: Documentation~/Claude/PLAN.md
---

# Context manifest — kf-mobile-ui-kit — 2026-05-18

(Migration session — initial v2 manifest. UPM variant per `~/.claude/memory/ecosystem_philosophy.md` §18: workspace lives at `Documentation~/Claude/` instead of `Assets/_Project/Claude/` because there is no `Assets/` folder in a UPM package. Same v2 schema. This file is the session-level delta, overwritten on next `/_close`.)

## Entry-points (read at next session start)
- [required] PLAN.md — current §0 + §2
- [required] memory/conventions.md — KitforgeLabs brand + 9 architecture overrides + UPM-specific rules
- [optional] CHANGELOG.md `[Unreleased]` block (package root) — only when actively working toward next tag

## Recent activity (last 3 sessions)
- 2026-05-18 (this, quizzical-panini): Initial v2 migration. Created `Documentation~/Claude/` with PLAN.md + README + memory/conventions + memory/close_context_latest. Ecosystem updated in parallel to support UPM detection (`/_plan init` walks for `package.json` with `unity` field when no `Assets/` found; `/_align`/`/_close`/`/_start`/`/_ecosystem` route to `Documentation~/Claude/` for UPM packages). KF_MobileUIKit is the first UPM package on v2 protocol.
- 2026-05-13 (pre-v1.3.4): Demo Scene Quick Spawn fix + Play Mode guard on Open Demo Scene + Hub Catalog thumbnails + Main Camera bake. Tagged v1.3.4.
- 2026-05-13 (v1.3.0 → v1.3.3 batch): Click & Play feature shipped — Demo Scene + Demo services + Maintenance asmdef isolation + Setup Wizard simplification.

## Pending threads
- Demo Scene showcase re-bake (WIP commit `70bc4de`) → smoke test pass → tag v1.3.5
- v1.4 scope decision (LICENSE.md + author-tool guards + cheatsheet review + store assets)
- `KitforgeCatalogWireTool.WireAll` missing `ConfirmWritable` guard (parallels `KitforgeCatalogPrefabsRegenerator` pattern)
- Asset Store submission scoping ($30-60 price band; v1.4 readiness gate)
