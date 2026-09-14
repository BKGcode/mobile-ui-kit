---
manifest_version: v2
project: kf-mobile-ui-kit
project_root: C:\Users\Joan\mobile-ui-kit-dev
timestamp: 2026-09-14T00:00:00Z
commit_ref: 3c0bb3a
scope: PROJECT — C:\Users\Joan\mobile-ui-kit-dev
session_status: done
plan_md_path: Documentation~/Claude/PLAN.md
---

# Context manifest — kf-mobile-ui-kit — 2026-09-14

(Cross-repo session: opened in the techart-tools package (F:) but all work landed here in mobile-ui-kit. Branding-only, no functional change. This file is the session-level delta, overwritten on next `/_close`.)

## Entry-points (read at next session start)
- [required] PLAN.md — §0 + §2 (active work is still the Demo Scene re-bake → v1.3.5, untouched this session)
- [required] memory/conventions.md — §Studio brand REVISED this session (display `KitForge Labs` vs frozen code-id `KitforgeLabs`) + 9 architecture overrides + UPM rules
- [optional] CHANGELOG.md `[Unreleased]` — now holds the brand change; closes at v1.3.5 tag

## Recent activity (last 3 sessions)
- 2026-09-14 (this): Studio brand DISPLAY unified to `KitForge Labs` across all buyer-visible surfaces (displayName/author/README/Hub-title/top-level+Create menus); code ids frozen (namespace, package id, `Assets/KitforgeLabs/…`, `Tools/KitforgeLabs/Test`). `conventions.md` §Studio brand revised (supersedes 2026-05-17 one-word lock for display only). CHANGELOG `[Unreleased]` + PLAN §5/§6 updated. A merge into techart-tools was evaluated and REJECTED (runtime-lib ≠ editor-suite; unify via dependency, not absorption). Commit `3c0bb3a` (code, pushed) + doc-sync commit.
- 2026-05-18 (quizzical-panini): Initial v2 migration. Created `Documentation~/Claude/` (PLAN + README + conventions + close_context). First UPM package on v2 protocol.
- 2026-05-13 (pre-v1.3.4): Demo Scene Quick Spawn fix + Play Mode guard + Hub thumbnails + Main Camera bake. Tagged v1.3.4.

## Pending threads
- Demo Scene showcase re-bake (WIP commit `70bc4de`) → smoke test pass → tag v1.3.5 (the brand change in CHANGELOG `[Unreleased]` ships with this tag — no separate version bump)
- v1.4 scope decision (LICENSE.md + author-tool guards + cheatsheet review + store assets)
- `KitforgeCatalogWireTool.WireAll` missing `ConfirmWritable` guard (parallels `KitforgeCatalogPrefabsRegenerator`)
- Asset Store submission scoping ($30-60 price band; v1.4 readiness gate)
