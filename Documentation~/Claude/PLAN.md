# PLAN — KF_MobileUIKit (UPM package)

> Single source of truth for objective, blocks, tasks, priorities, and decisions.
> Auto-loaded by `/_start`. Auto-updated by `/_close`. Operated by `/_plan`.
> Lives at `Documentation~/Claude/PLAN.md` inside the package repo (UPM variant per `ecosystem_philosophy.md` §18).

---

## §0 Objective

- **What:** `com.kitforgelabs.mobile-ui-kit` — mobile UGUI kit for Unity hybrid-casual games. 17-element catalog (10 popups + 1 toast + 2 screens + 4 HUDs) + framework (`UIManager` / `PopupManager` / `ToastManager` / `UIThemeConfig`) + 3 themes + 8 service interfaces with Null Object defaults + Click & Play Demo Scene. Zero DI dependency in Runtime.
- **Buyer / user:** Unity hybrid-casual game developers needing pre-wired UI primitives without rebuilding popups/HUDs/screens from scratch. Buyer installs via git URL or Asset Store, opens Demo Scene, presses Play, sees all elements live in <60 seconds.
- **Success test for next tag (v1.4.0):** Asset Store listing ready — clean Unity 6000.1 install, package via git URL, Demo Scene opens + plays without warnings, 14-day smoke test on a host project shows zero regressions. LICENSE.md authored.

---

## §1 Blocks

| Block | Purpose | Status | Depends on | DoD |
|---|---|---|---|---|
| v1.0 Foundation | Framework + 17-element catalog + 3 themes + 8 service interfaces + PlayerPrefs persistence + Kitforge Hub | 🟢 | — | Tagged 2026-05-10, public API frozen |
| v1.1 Studio reorg | Namespace `KitforgeLabs.UIKit.*` + top-level `KitforgeLabs/` menu + asset root `Assets/KitforgeLabs/UI Kit/Settings/` | 🟢 | v1.0 | Tagged v1.1.0 → v1.1.3 |
| v1.2 Shipping catalog | Catalog prefabs ship with package + Null Object services + smoke test suite (`KitforgeRootSmokeTests`) | 🟢 | v1.1 | Tagged v1.2.0 |
| v1.3 Click & Play | Demo Scene + Demo services (8 in-memory impls) + Maintenance asmdef isolation (`KITFORGE_DEV_MAINTENANCE` gate) | 🟢 | v1.2 | Tagged v1.3.0 → v1.3.4 |
| v1.3.5 Demo Scene showcase | Re-bake Demo Scene as showcase with all 17 elements accessible (currently WIP at commit `70bc4de`) | 🟡 1/2 | v1.3.4 | Re-bake passes smoke + tag v1.3.5 |
| v1.4 Asset Store readiness | LICENSE.md + Documentation~/CHEATSHEET review + author-tool guards (`KitforgeCatalogWireTool.ConfirmWritable`) + store assets prep | ⚪ | v1.3.5 | Tagged v1.4.0 ready for Asset Store submission |
| Asset Store submission | Listing live at $30-60 price band on Unity Asset Store | ⚪ | v1.4 | Published listing + first external sale |

Status legend: 🟢 done · 🟡 in progress (sub-step counter `🟡 X/N`) · 🔴 blocked · ⚪ not started · ⚠ pre-pivot

---

## §2 Now (max 3 items · each <1 day)

Active work for THIS session + immediate next.

- [ ] Complete Demo Scene showcase re-bake — DoD: `KitforgeDemoScene.unity` re-baked from WIP commit `70bc4de`, all 17 catalog elements reachable from MainMenuScreen or Quick Spawn panel, no console errors on Play
- [ ] Smoke test pass on re-baked scene — DoD: `KitforgeRootSmokeTests` green on `KitforgeRoot.prefab` + manual click-through of all 17 elements in Demo Scene
- [ ] Tag v1.3.5 + bump install URL — DoD: tag on `main` + README install URL bumped from `#v1.3.4` → `#v1.3.5` + CHANGELOG `[Unreleased]` closed

---

## §3 Next (max 5 items · prioritized · ≤1 week horizon)

Queue feeding §2. Ordered by priority (top = next promotion).

1. v1.4 scope decision — block: v1.4 — est: S — Decide what makes a "ready for Asset Store" v1.4 (LICENSE + cheatsheet + author-tool guards minimum; anything more is v1.5)
2. LICENSE.md authored — block: v1.4 — est: S — Currently README says "Proprietary — KitforgeLabs. See LICENSE.md (TBD before public release)". Author per chosen license model
3. `KitforgeCatalogWireTool.ConfirmWritable` guard — block: v1.4 — est: S — Add the same read-only-package guard that `KitforgeCatalogPrefabsRegenerator` already has (parallel pattern, prevents `SaveAsPrefabAsset` ArgumentException in consumer projects)
4. `Documentation~/CHEATSHEET.md` review for v1.4 — block: v1.4 — est: M — Verify all flows still match post-v1.3.x changes (Bootstrap Defaults removed in v1.3.2, Demo Scene front door, etc.)
5. Asset Store assets prep — block: v1.4 — est: L — 5+ screenshots, 30-60s gameplay video, marketing copy (one-liner + paragraph + 7-bullet feature list)

---

## §4 Later / OUT-of-scope (max 20 items · auto-purge >60 days)

Parking. `added:` date required so `/_plan prune` knows what to archive.

- v2.0 UI Toolkit runtime support — added: 2026-05-18 — reason: README §Non-goals 7 explicitly excludes UI Toolkit; v2.0 if buyer demand materializes (UGUI is the v1.x bet)
- Data binding framework — added: 2026-05-18 — reason: README §Non-goals 1 — modules accept `Bind(TData)`, no framework
- Localization layer — added: 2026-05-18 — reason: README §Non-goals 2 — buyer brings own (Unity Localization / I2 / custom JSON); modules accept already-resolved strings
- Networking / save backend / analytics — added: 2026-05-18 — reason: README §Non-goals 3 — service interfaces are contracts, buyer implements
- Animation engine — added: 2026-05-18 — reason: README §Non-goals 5 — DOTween Pro is assumed sequencer for show/hide motion
- Visual screen-graph editor — added: 2026-05-18 — reason: README §Non-goals 6 — Inspector + prefab refs only; the Hub authors *scene state*, not graphs
- Additional theme presets beyond 3 — added: 2026-05-18 — reason: 3 presets (Default/Casual/Premium) cover hybrid-casual range; buyers create via Assets→Create→KitforgeLabs→UI Kit→Theme menu
- Public marketing campaign — added: 2026-05-18 — reason: v1.0 marketing on Asset Store listing only; broader campaign post-v1.4 if first sales validate price band
- Sample scenes beyond Demo — added: 2026-05-18 — reason: v1.1.0 explicitly removed downloadable Samples; Hub Catalog browser + Demo Scene replace them
- Tutorial walkthroughs / onboarding flows beyond Hub Setup Wizard — added: 2026-05-18 — reason: Hub already covers it (Setup wizard + Catalog browser + Theme Studio + Test launcher + inline Cheatsheet)

---

## §5 Done (this tag) — rotates to CHANGELOG on tag

Closed items with DoD evidence. Empties on tag (`/_plan tidy --post-tag`).

(Empty — current dev is unreleased after v1.3.4. The WIP commit `70bc4de` "feat(demo): redesign Demo Scene as showcase" lives in §2 Now until smoke passes; then promotes to §5 + rotates to CHANGELOG at v1.3.5 tag.)

---

## §6 Decisions log (one line + WHY)

Append-only. Every architectural decision, scope change, or rejected approach lands here with its because-clause.

- 2026-05-10 — Runtime asmdef has ZERO DI dependency — **because** buyers without VContainer must boot in one step; `UIServices` MonoBehaviour Inspector setters provide DI-free path
- 2026-05-10 — `UIManager` / `PopupManager` / `ToastManager` are `MonoBehaviour` (not singletons) — **because** one scene = one manager via Inspector references on `KitforgeRoot.prefab`
- 2026-05-10 — Screens registered as flat `UIModuleBase[]` array, resolved by `Type` — **because** Inspector authoring trivial; one prefab per concrete `UIModule` subclass
- 2026-05-10 — Popup priority = `enum` (Meta / Gameplay / Modal) — **because** three buckets cover every hybrid-casual case; richer hierarchies would explode authoring complexity
- 2026-05-10 — `PopupRecord` stores `Data` so eviction can re-enqueue with state — **because** a modal evicting a Gameplay popup must restore it with its original payload (state preservation invariant)
- 2026-05-10 — VContainer is opt-in, never a Runtime reference — **because** avoid forcing a DI container on buyers using Zenject / no DI / their own injector
- 2026-05-10 — Single `UIThemeConfig` asset feeds all three managers — **because** "skin it once" is the kit's pitch; `KitforgeThemeBinder` auto-distributes
- 2026-05-10 — Demo services live in the package (not as Samples) — **because** Demo Scene must work out-of-box without buyer downloads; lives in `Runtime/Services/Demo/`, opt-in via `DemoServicesBootstrap` MonoBehaviour
- 2026-05-11 — Studio reorganization v1.1: namespace `KitforgeLabs.UIKit.*` + top-level `KitforgeLabs/` menu + asset root `Assets/KitforgeLabs/UI Kit/Settings/` — **because** establish KitforgeLabs studio convention reused by every future product (single namespace folder keeps buyer's `Assets/` clean)
- 2026-05-11 — Removed all Package Manager Samples (9 entries) — **because** Hub replaces them (Setup wizard + Catalog browser + Theme Studio + Test launcher + inline Cheatsheet); samples maintenance cost was eating release bandwidth
- 2026-05-11 — Null Object service defaults in `Runtime/Services/Null/` — **because** kit always boots; buyers swap in production services via Inspector when ready; single warning logs at startup if Null defaults are used
- 2026-05-11 — Catalog prefabs ship with package under `Runtime/Catalog/Prefabs/` — **because** zero buyer setup on first install; `KitforgeRoot.prefab` pre-wired registries spawn any catalog element via manager APIs
- 2026-05-13 — `KITFORGE_DEV_MAINTENANCE` define gate for author tooling — **because** buyers must not see `(Dev)` menus, internal builders, or shared helpers; isolation via `KitforgeLabs.UIKit.Editor.Maintenance.asmdef` `defineConstraints`
- 2026-05-13 — Click & Play Demo Scene + Demo services — **because** "show working before asking buyer to write code" is the kit's friction-reducing pitch; baked `KitforgeDemoScene.unity` is the front door
- 2026-05-13 — Removed `Bootstrap Defaults` menu (v1.3.2) — **because** kit no longer asks buyers to run a setup tool; `UIAnimPreset_Playful.asset` ships pre-wired in 3 stock themes
- 2026-05-13 — Maintenance menus relocated to `Tools/KitforgeLabs/Test/...` — **because** top-level `KitforgeLabs/UI Kit/` menu must be buyer-facing only (Hub + Open Demo Scene); separation explicit
- 2026-05-18 — Migration to v2 planning protocol (UPM variant at `Documentation~/Claude/`) — **because** ecosystem `philosophy §18` mandates single-source PLAN.md per project; UPM convention `~`-suffixed folders excluded from consumer Unity imports keeps workspace out of buyer projects

---

## §7 Experiments (sandbox · not milestone-counted)

Throwaway exploration. If an experiment graduates, promote to §1 Blocks.

(Empty — no live experiments.)

---

<!-- INTEGRITY MARKER — do not edit by hand
last_updated: 2026-05-18T13:00Z
updated_by: migration-v1-to-v2-upm
plan_version: v1
-->
