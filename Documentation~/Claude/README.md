# Claude workspace — KF_MobileUIKit (UPM package)

This folder is the Claude-side workspace for the package. Travels with the repo via `git push`. Collaborators clone and their Claude reads this folder.

Path: `Documentation~/Claude/` — UPM convention: `~`-suffixed folders are excluded from consumer Unity project imports (Unity ignores them, does not generate `.meta` files for their contents). Workspace stays out of buyers' projects but is tracked in git and visible in any file explorer or IDE.

This is the **UPM package variant** of the v2 planning protocol per `~/.claude/memory/ecosystem_philosophy.md` §18. The Unity-project variant uses `Assets/_Project/Claude/`. Same schema, same tiered scaffolding, same three non-negotiable rules — only the path differs.

## Layout (tiered)

### Tier 1 — mandatory
- `PLAN.md` — master plan (objective, blocks, tasks, decisions). Source of truth.
- `README.md` — this file.
- `memory/conventions.md` — package rules over global ecosystem.
- `memory/close_context_latest.md` — last session manifest (auto-written by `/_close`).

### Tier 2 — add when needed
- `memory/decisions.md` — append-only decision log (deeper mirror of PLAN.md §6).
- `memory/learnings.md` — package-specific feedback.
- `references/` — design references (architecture diagrams, contracts).

### Tier 3 — multi-month projects
- `references/contracts/` — interface contracts (e.g., service interface specs).
- `references/specs/` — per-feature specs.
- `tasks/briefs/` — milestone walk scripts (none currently — package iterates by version, not milestone).
- `discussions/closed/` — archived resolved discussions.
- `guides/` — onboarding / quickstart.

## How to work with it

| Action | Command |
|---|---|
| Session bootstrap | `/_start` (auto-loads PLAN.md + runs `/_align`) |
| See current plan status | `/_plan` |
| Add / done a task | `/_plan add "..."` / `/_plan done "..." --evidence "..."` |
| Update a block status | `/_plan block <name> --status 🟡 --step 3/4` |
| Append a decision | `/_plan decision "phrase · because reason"` |
| Alignment / drift check | `/_align` |
| Session close | `/_close` (updates PLAN.md as exit gate) |

## Non-negotiable rules (per `~/.claude/memory/ecosystem_philosophy.md` §18)

1. **Mandatory load:** `/_start` reads PLAN.md if it exists.
2. **Alignment gate:** no Edit/Write on package code if `/_align` verdict is not 🟢 or explicitly acknowledged.
3. **Commit-updates-PLAN:** any commit whose diff touches §1 Blocks or closes a §2 Now item MUST update PLAN.md in the same commit.

## What does NOT live here

- Global ecosystem rules (`~/.claude/memory/ecosystem_philosophy.md`, `unity-rules` skill, etc.) — stay in `~/.claude/`.
- Universal learnings (apply across projects) — go to `~/.claude/memory/feedback_*.md`.
- Anything specific to ONE package lives HERE, never in `~/.claude/`.

## Package-side references (outside this folder)

- `README.md` (package root) — canonical product spec for buyers (what's in the box, catalog, install, services).
- `CHANGELOG.md` (package root) — Keep-a-Changelog format, version-grouped (v1.0.0 → v1.3.4 currently).
- `Documentation~/CHEATSHEET.md` — buyer quickstart reference (visible to package consumers via Documentation~ access — NOT auto-imported, but readable in the repo).
- `Documentation~/Specs/Catalog/*.md` — 14 per-element specs (developer reference).
- `Documentation~/Specs/Services/*.md` — service interface specs.
- `Documentation~/Screenshots/*.png` — Hub thumbnails + Hero banner (baked from Demo Scene via `KitforgeCatalogScreenshotBaker`).
- `package.json` — UPM manifest (version, displayName, unity, dependencies, testables).
- `Runtime/` — runtime asmdef + catalog + services + framework.
- `Editor/` — Hub + maintenance tools (compile-gated by `KITFORGE_DEV_MAINTENANCE`).
- `Tests/Editor/Smoke/KitforgeRootSmokeTests.cs` — smoke test suite (gates v1.2.0+ releases).
