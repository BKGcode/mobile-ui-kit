# Catalog Specification — Phase 2

> **Status**: Pre-alignment draft (2026-05-01). Subject to change before any element is built.
> **Source of truth** for what ships under `Runtime/Catalog/` and `Samples~/Catalog/` once Phase 2 begins.

This document defines **what** the kit catalog will contain and **the contracts** every element must honor so they integrate without coupling. The roadmap and stress tests live in the planner workbook (`~/.claude/memory/kitforge_mobile_ui_kit_roadmap.md`).

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
| 3 | RewardPopup | Gameplay | Item/coins obtained — tap to claim |
| 4 | DailyLoginPopup | Meta | 7-day calendar reward |
| 5 | ShopPopup | Modal | Grid of items with prices + buy buttons |
| 6 | LevelCompletePopup | Gameplay | Stars / score / next / retry |
| 7 | GameOverPopup | Modal | Continue (rewarded ad) / restart / quit |
| 8 | PausePopup | Modal | Resume / restart / settings / quit |
| 9 | NotEnoughCurrencyPopup | Modal | Offer to buy more / watch ad |
| 10 | TutorialPopup | Gameplay | Text + character + continue/skip |

### Transient / HUD (NEW LAYER — not in Phase 1)
| # | Element | Manager | Purpose |
|---|---|---|---|
| 11 | NotificationToast | `ToastManager` (new) | Non-blocking, auto-dismiss, stack-able |
| 14a | HUD-Coins | none (binds to screen) | Live coin counter, reacts to economy events |
| 14b | HUD-Gems | none (binds to screen) | Live gem counter |
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
4. **Animation per element** — ship `UIAnim_<Element>.cs` (generated via `_tween`) as a separate component on the prefab. Hook from `OnShow` / `OnHide`.
5. **Demo asset** — every element appears in at least one Demo scene under `Samples~/Catalog/` with `[ContextMenu]` triggers.
6. **Portrait 1080×1920 first** — primary CanvasScaler reference. Landscape is secondary, not denied.
7. **Safe-area respect** — every full-canvas element uses `SafeAreaFitter`.
8. **Self-contained back behavior** — every popup overrides `OnBackPressed` explicitly.
9. **Service decoupling** — emit events (`OnConfirmed`, `OnPurchased`, `OnRewardClaimed`); host game wires chains.
10. **Min touch target** — every interactive element ≥ `Theme.MinTouchTarget` (default 88pt / ~44dp).

### 3.2 MUSTN'T

1. **No popup may call `PopupManager.Show` for another popup** — emit an event; host wires the chain.
2. **No popup may write `Time.timeScale`** — only the game/router responds to `AppState.Paused`.
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

| Group | Members | Acceptance |
|---|---|---|
| **0 — Foundation** | F1-F8 | Hello-Toast + Hello-HUD-Coin demo works |
| **A — Pure UI** | Confirm, Tutorial, Pause, Toast | 4 elements coexist; back button traverses correctly |
| **B — Currency** | Reward, Shop, NotEnough, HUD-Coins, HUD-Gems | Buy → spend → HUD updates → unaffordable → NotEnough → ad → reward |
| **C — Progression / Time** | DailyLogin, LevelComplete, GameOver, HUD-Energy, HUD-Timer | Full level loop with Continue (ad) |
| **D — Player Data** | Settings | Persists across sessions |
| **E — Screens** | Loading, MainMenu | Full app boot demo end-to-end |

Each closed group = `package.json` minor bump (e.g. `0.3.0`, `0.4.0`, ...).

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

## 7. Open questions (block Phase 2 kickoff)

1. Group order confirmation (Group 0 first or jump to a visible group?).
2. Service binding pattern confirmed (Option B = `UIServices` container)?
3. Animation moodboard (vibey / snappy / punchy / elegant) before generating first `UIAnim_<X>`?
4. Theme depth: extend slots in Group 0 or on-demand in Group A?
5. DOTween Pro: hard requirement or `#if DOTWEEN` guards?
6. Visual reference target (Royal Match, Coin Master, Gardenscapes, ...)?
7. Sample structure (single `Samples~/Catalog/` or per-group)?
8. Versioning policy (per-group bump or single Phase 2 bump)?

These get answered in chat before any code is written.

---

## 8. Out of scope for Phase 2

The following are explicitly NOT delivered by Phase 2 catalog:

- Localization (still buyer responsibility per README Non-goal #2).
- Real economy / ads / save backends — only stub services.
- Custom shaders / VFX (UI Particles, glows beyond what UGUI does).
- Editor authoring window (visual screen graph).
- UI Toolkit equivalents — UGUI only.
- Multi-orientation auto-handling — portrait first; landscape requires re-anchoring per element.
