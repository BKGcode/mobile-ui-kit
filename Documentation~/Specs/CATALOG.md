# Catalog Specification — Phase 2

> **Status**: Phase 2 in progress (2026-05-06). Group 0 + A + B + C + D + E shipped. M3 closed at `v0.8.0-alpha` BREAKING (OnUpdate infra dispatch fix + Group E catalog) — **292 EditMode tests green** on fresh compile. Path to `v1.0.0-rc` locked — see § 9 below. Next: M4 hardening + docs final.
> **Source of truth** for what ships under `Runtime/Catalog/` and `Samples~/Catalog/`.

This document defines **what** the kit catalog contains and **the contracts** every element honors so they integrate without coupling. Per-element specs live under `Documentation~/Specs/Catalog/`. Cross-cutting deltas per group live alongside (e.g. `Documentation~/Specs/Catalog/CATALOG_GroupC_DELTA.md`).

---

## 1. Catalog scope

The kit ships **15 ready-to-use mid-core mobile UI elements** in three layers:

### Screens (managed by `UIManager`)
| # | Element | Purpose |
|---|---|---|
| 12 | LoadingScreen | Full-screen loading with optional progress bar; async-friendly |
| 13 | MainMenuScreen | Entry point — play / settings / shop / daily buttons |

### Popups (managed by `PopupManager`)
| # | Element | Priority | Purpose |
|---|---|---|---|
| 1 | ConfirmPopup | Modal | Yes/No, Continue/Cancel — universal blocker. `ShowCancel=false` collapses to single-button alert (no separate AlertPopup). See [Specs/Catalog/ConfirmPopup.md](Catalog/ConfirmPopup.md). |
| 2 | SettingsPopup | Modal | Sound / music / vibration / language toggles |
| 3 | RewardPopup | Gameplay | Item/coins obtained — tap to claim. See [Specs/Catalog/RewardPopup.md](Catalog/RewardPopup.md). |
| 4 | DailyLoginPopup | Meta | 7-day calendar reward |
| 5 | ShopPopup | Modal | Grid of items with prices + buy buttons. See [Specs/Catalog/ShopPopup.md](Catalog/ShopPopup.md). |
| 6 | LevelCompletePopup | Gameplay | Stars / score / next / retry |
| 7 | GameOverPopup | Modal | Continue (rewarded ad) / restart / quit |
| 8 | PausePopup | Modal | Resume / restart / settings / quit. Owns `Time.timeScale` while visible. See [Specs/Catalog/PausePopup.md](Catalog/PausePopup.md). |
| 9 | NotEnoughCurrencyPopup | Modal | Offer to buy more / watch ad. See [Specs/Catalog/NotEnoughCurrencyPopup.md](Catalog/NotEnoughCurrencyPopup.md). |
| 10 | TutorialPopup | Gameplay | Multi-step tutorial with Next/Previous/Skip + dynamic Done label + loop wrap. See [Specs/Catalog/TutorialPopup.md](Catalog/TutorialPopup.md). |

### Transient / HUD (NEW LAYER — not in Phase 1)
| # | Element | Manager | Purpose |
|---|---|---|---|
| 11 | NotificationToast | `ToastManager` | Non-blocking, auto-dismiss, severity-tinted (Info / Success / Warning / Error), tap-to-dismiss. See [Specs/Catalog/NotificationToast.md](Catalog/NotificationToast.md). |
| 14a | HUD-Coins | none (binds to screen) | Live coin counter, reacts to economy events. See [Specs/Catalog/HUD-Coins.md](Catalog/HUD-Coins.md). |
| 14b | HUD-Gems | none (binds to screen) | Live gem counter. See [Specs/Catalog/HUD-Gems.md](Catalog/HUD-Gems.md). |
| 14c | HUD-Energy | none (binds to screen) | Energy bar with regen timer |
| 14d | HUD-Timer | none (binds to screen) | Countdown / count-up timer |

---

## 2. Foundation work (Group 0 — built before any visible element)

The following are architectural prerequisites with no direct buyer-visible output. Without them, every catalog element would re-invent the same plumbing differently.

| ID | Component | Resolves |
|---|---|---|
| F1 | `ToastManager` MonoBehaviour | NotificationToast lifecycle |
| F2 | `UIHUDBase` abstract class + HUD layer convention | HUD elements need a shared lifecycle distinct from popups |
| F3 | `UIServices` MonoBehaviour container | Popups need IEconomyService etc. without DI dependency |
| F4 | `PopupManager.Theme` getter (proxy to UIManager) | Popups can read theme |
| F5 | `IUIAnimator` contract + `UIAnim_<X>` script convention (DOTween-guarded) | All elements have show/hide animations |
| F6 | `UIThemeConfig` extended with sprite/audio/icon slots | True reskin without code |
| F7 | `SafeAreaFitter` component | Notch/home-indicator survival |
| F8 | Editor pre-build validator | Catch missing prefabs at build, not at runtime |

---

## 3. Plug-and-play contracts (cross-element)

Every catalog element MUST satisfy these contracts so combinations work without surprises.

### 3.1 MUST

1. **Single Theme source** — read all visual tokens from `UIManager.Theme` (or via `IUIThemeProvider` indirection). No hardcoded colors/fonts/sprites in prefabs.
2. **Single service source** — access economy / ads / time / progression via `UIServices` container only. Never resolve services manually.
3. **Typed payload** — derive `UIModule<TData>` with serializable `<Element>Data` DTO. No `object` payloads.
4. **Animation per element** — ship `UIAnim_<Element>.cs` (generated via `_tween`) as a separate component on the prefab. Hook from `OnShow` / `OnHide`. Animator classes are `sealed by design` — buyers extend animations via two canonical paths only: (a) override the `UIAnimPreset` SO assigned to `AnimPresetOverride` (data-driven, no code), or (b) replace the script entirely (remove the kit component, add a new MonoBehaviour implementing `IUIAnimator`). Inheritance is intentionally blocked to keep the extension surface binary and reviewable; the kit owns the animator class, the buyer owns the preset OR the replacement.
5. **Demo asset** — every element appears in at least one Demo scene under `Samples~/Catalog/` with `[ContextMenu]` triggers. The scene MUST open in Play mode and reach the happy-path state without manual fixup — `Main Camera`, `EventSystem`, services, theme, and required prefab refs all wired by the builder.
6. **Portrait 1080×1920 first** — primary CanvasScaler reference. Landscape is secondary, not denied.
7. **Safe-area respect** — every full-canvas element uses `SafeAreaFitter`.
8. **Self-contained back behavior** — every popup overrides `OnBackPressed` explicitly.
9. **Service decoupling** — emit events (`OnConfirmed`, `OnPurchased`, `OnRewardClaimed`); host game wires chains.
10. **Min touch target** — every interactive element ≥ `Theme.MinTouchTarget` (default 88pt / ~44dp).

### 3.2 MUSTN'T

1. **No popup may call `PopupManager.Show` for another popup** — emit an event; host wires the chain.
2. **No popup may write `Time.timeScale`** — only the game/router responds to `AppState.Paused`. **Exception: `PausePopup`** is the documented opt-in owner of `Time.timeScale` while visible (industry-standard mobile pattern). Captures the value in `OnShow`, restores in `OnHide`/`OnDestroy`, and exposes `_pauseTimeScale` (default `0`) in the Inspector. Any host that prefers an event-driven model can set `_pauseTimeScale = 1` and consume `OnPaused`/`OnResumed` instead.
3. **No element may hold a static reference** to a service or another element.
4. **No element may assume a specific service implementation** — only the interface.
5. **No element may persist via PlayerPrefs / SaveSystem directly** — go through `IPlayerDataService`.
6. **No element may load assets via `Resources.Load`** — Inspector references only.
7. **No element may register itself globally** — registries are owned by managers via Inspector.
8. **No element may play audio inline** — emit events; SFX is a Theme concern.

---

## 4. Service binding pattern (resolves G3)

**Decision**: `UIServices` MonoBehaviour container.

```text
UIRoot (GameObject)
├── UIManager (has _themeConfig)
├── PopupManager
├── UIRouter
├── ToastManager
└── UIServices            ← NEW
    ├── _economyService   (MonoBehaviour ref — buyer's impl)
    ├── _playerService    (MonoBehaviour ref)
    ├── _adsService       (MonoBehaviour ref)
    ├── _timeService      (MonoBehaviour ref)
    ├── _progressionSvc   (MonoBehaviour ref)
    └── _shopProvider     (MonoBehaviour ref)
```

Popups query: `UIManager.Services.Economy.GetCoins()`.

VContainer/Zenject users wrap their container resolution into `UIServices` setters at boot. Runtime asmdef stays DI-free.

---

## 5. Build groups (priority order)

| Group | Members | Status | Tag | Acceptance |
|---|---|---|---|---|
| **0 — Foundation** | F1-F8 | ✅ shipped | `v0.3.0-alpha` | Hello-Toast + Hello-HUD-Coin demo works |
| **A — Pure UI** | Confirm, Tutorial, Pause, Toast | ✅ shipped | `v0.4.0-alpha` (+ hotfix `v0.4.1-alpha`) | 4 elements coexist; back button traverses correctly |
| **B — Currency** | Reward, Shop, NotEnough, HUD-Coins → `HUDCurrency`, HUD-Gems → `HUDCurrency` | ✅ shipped | `v0.5.0-alpha` | Buy → spend → HUD updates → unaffordable → NotEnough → ad → reward. Chain demo playable in-Editor. |
| **C — Progression / Time** | DailyLogin, LevelComplete, GameOver, HUD-Energy, HUD-Timer | ✅ shipped | `v0.6.0-alpha` BREAKING | Full level loop with Continue (ad) + DailyLogin auto-trigger + Energy regen across UTC midnight |
| **D — Player Data** | Settings + Game Wiring sample revival | ✅ shipped | `v0.7.0-alpha` BREAKING | Settings persist across sessions; Game Wiring sample re-imports + plays |
| **E — Screens** | Loading, MainMenu | ✅ shipped (closes M3 — bundles OnUpdate infra dispatch fix) | `v0.8.0-alpha` BREAKING | Demo scene plays happy path on Play without manual fixup (Loading → MainMenu → DailyLogin auto-trigger); zero LogErrors in Console |
| **Hardening + Docs final** | Theme presets, severity icons, perf bench, hero screenshots, MIGRATION.md, API freeze | ⏳ | `v1.0.0-rc` | Buyer-facing docs final; API stable |

Each closed group = `package.json` minor bump. Tag detail and per-milestone "Done when…" criteria: see roadmap `Path to v1.0.0-rc` § F4.

**Per-element specs**: Catalog elements ship with a markdown spec under `Documentation~/Specs/Catalog/`. Group C specs additionally cite the cross-cutting null-service policy in `CATALOG_GroupC_DELTA.md` § 4.5.

---

## 6. Per-element micro-spec template

Every element will get its own spec file under `Documentation~/Specs/Catalog/<Element>.md` once its group starts. Template:

```markdown
# <Element>

## Purpose
One sentence.

## DTO
`<Element>Data` fields, types, defaults.

## Services consumed
List of interfaces (or "none").

## Events emitted
Public events the host can subscribe to.

## Animation contract
Show / Hide / Idle behavior; tokens consumed from Theme.

## Theme tokens consumed
Colors / fonts / sprites / audio used.

## Edge cases
What happens with empty data, missing service, multi-instance, etc.

## QA scenarios
List of [ContextMenu] triggers in the Demo scene.
```

---

## 7. Open questions — RESOLVED 2026-05-01 / 2026-05-03

All Phase 2 kickoff questions answered (locked in roadmap Decisions Log + planner workbook):

| # | Question | Answer |
|---|---|---|
| 1 | Group order | ✅ Group 0 → A → B → C → D → E |
| 2 | Service binding | ✅ Option B — `UIServices` MonoBehaviour container |
| 3 | Animation moodboard | ✅ Style dropdown (10 styles) + `UIAnimPreset` SO; default = Playful (Disney Getaway Blast vibe) |
| 4 | Theme depth | ✅ Group 0 covers all fundamentals; per-element slots added on-demand at group kickoff (e.g. Group C added `IconEnergy`/`IconClock`/`StarFilledSprite`/`StarEmptySprite`/`FailureColor`) |
| 5 | DOTween Pro | ✅ Hard requirement (assumed). Catalog asmdef references DOTween directly. Runtime asmdef DOTween-free per Non-goal #5. |
| 6 | Visual reference | ✅ Disney Getaway Blast (bright cartoon mid-core, snappy + playful) |
| 7 | Sample structure | ✅ One sample per group: `Samples~/Catalog_Group{Letter}_{Theme}/` |
| 8 | Versioning policy | ✅ Per-group minor bump; single `v1.0.0-rc` tag at end of hardening + docs final |

---

## 8. Out of scope for Phase 2

The following are explicitly NOT delivered by Phase 2 catalog:

- Localization (still buyer responsibility per README Non-goal #2).
- Real economy / ads / save backends — only stub services.
- Custom shaders / VFX (UI Particles, glows beyond what UGUI does).
- Editor authoring window (visual screen graph).
- UI Toolkit equivalents — UGUI only.
- Multi-orientation auto-handling — portrait first; landscape requires re-anchoring per element.

---

## 9. Path to `v1.0.0-rc`

The catalog reaches buyer-ready release-candidate state via 7 milestones (re-sequenced 2026-05-06 per the rule "complete core packaging BEFORE auditing UX" — auditing on incomplete packaging forces re-audit):

| # | Milestone | Tag |
|---|---|---|
| M1 | Group C close (3 popups + 2 HUDs + helpers + builder + sample + chain demo) | `v0.6.0-alpha` BREAKING ✅ |
| M2 | Group D (SettingsPopup + `IPlayerDataService` + `IUILocalizationService`) + Game Wiring revival | `v0.7.0-alpha` BREAKING ✅ |
| M3a | Group E (Loading + MainMenu) + OnUpdate infra dispatch fix + `_backdrop` sibling-order fix | `v0.8.0-alpha` BREAKING ✅ |
| M3b | UIAnim per-element polish (cell cascade, score rollup, juice) | **DEFERRED post-`v1.0.0-rc`** (wrong-premise revert 2026-05-06; foundational review pending) |
| M4 | Hardening core packaging — Theme presets, severity icons, `CatalogBuilderBase` extraction, ThemedImage runtime, master demo, MIGRATION.md, README + CATALOG + QUICKSTART final, perf bench, hero screenshots, buyer fresh-import smoke test, API freeze | `v0.9.0-alpha` (last alpha pre-rc) |
| M3c | Deep UX audit del kit completo (post-M4 core packaging) — `as user` + `as ta` + `as pm` cross-group · output = `kitforge_ux_audit_2026-05.md` findings file with P0 / P1 / P2 disposition. Orientation portrait/landscape decision absorbed here | (no tag — feeds M5/RC) |
| M5 / RC-tag | Disposition M3c findings (apply P0; defer P1+P2 post-1.0) + verify-all-docs + verify-all-samples + verify-all-tests on fresh compile | `v1.0.0-rc` |

Asset Store submission (store page text, marketing-quality screenshots, demo video, pricing lock) is **explicitly deferred** to a post-`v1.0.0-rc` session — not part of finishing plan scope.
