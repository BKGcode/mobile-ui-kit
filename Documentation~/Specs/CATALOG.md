# Catalog Specification

> **Source of truth** for what ships under `Runtime/Catalog/` and the contracts every catalog element honors. Per-element specs live under `Documentation~/Specs/Catalog/`.

---

## 1. Catalog scope

The kit ships **17 ready-to-use mobile UI elements** in three layers:

### Screens (managed by `UIManager`)

| Element | Purpose |
|---|---|
| LoadingScreen | Full-screen loading with optional progress bar; async-friendly |
| MainMenuScreen | Entry point — play / settings / shop / daily buttons |

### Popups (managed by `PopupManager`)

| Element | Priority | Purpose |
|---|---|---|
| ConfirmPopup | Modal | Yes/No, Continue/Cancel — universal blocker. `ShowCancel=false` collapses to single-button alert. |
| SettingsPopup | Modal | Music / SFX / language / notifications / haptics. |
| RewardPopup | Gameplay | Item / coins obtained — tap to claim. |
| DailyLoginPopup | Meta | 7-day calendar reward. |
| ShopPopup | Modal | Grid of items with prices + buy buttons. |
| LevelCompletePopup | Gameplay | Stars / score / next / retry. |
| GameOverPopup | Modal | Continue (rewarded ad) / restart / quit. |
| PausePopup | Modal | Resume / restart / settings / quit. Owns `Time.timeScale` while visible. |
| NotEnoughCurrencyPopup | Modal | Offer to buy more / watch ad. |
| TutorialPopup | Gameplay | Multi-step tutorial with Next/Previous/Skip + dynamic Done label + loop wrap. |

### Transient / HUD

| Element | Manager | Purpose |
|---|---|---|
| NotificationToast | `ToastManager` | Non-blocking, auto-dismiss, severity-tinted, tap-to-dismiss. |
| HUDCoins / HUDGems | none (binds to screen) | Live currency counter, reacts to economy events. |
| HUDEnergy | none | Energy bar with regen timer. |
| HUDTimer | none | Countdown / count-up timer. |

---

## 2. Foundation services

| ID | Component | Resolves |
|---|---|---|
| F1 | `ToastManager` MonoBehaviour | NotificationToast lifecycle |
| F2 | `UIHUDBase` abstract class + HUD layer convention | Shared HUD lifecycle distinct from popups |
| F3 | `UIServices` MonoBehaviour container | DI-free service binding (Inspector setters) |
| F4 | `UIThemeConfig` ScriptableObject | All visual tokens (colors / sprites / fonts / audio cues / anim preset / safe-area) |
| F5 | `IUIAnimator` contract + `UIAnim_<X>.cs` | DOTween-backed show/hide motion per element |
| F6 | `SafeAreaFitter` component | Notch / home-indicator survival |

---

## 3. Plug-and-play contracts

Every catalog element honors these contracts so combinations work without surprises.

### 3.1 MUST

1. **Single Theme source** — read all visual tokens from `UIManager.Theme`. No hardcoded colors / fonts / sprites in prefabs.
2. **Single service source** — access economy / ads / time / progression / player-data / localization / audio / shop via the `UIServices` container only. Never resolve services manually.
3. **Typed payload** — derive `UIModule<TData>` with a serializable `<Element>Data` DTO. No `object` payloads.
4. **Animation per element** — ship a `UIAnim_<Element>` component on the prefab. Hook from `OnShow` / `OnHide`. Animator classes are `sealed by design`; buyers extend via two canonical paths only: (a) override the `UIAnimPreset` SO assigned to `AnimPresetOverride` (data-driven, no code), or (b) replace the component entirely (remove the kit component, add a new MonoBehaviour implementing `IUIAnimator`). Inheritance is intentionally blocked to keep the extension surface binary and reviewable.
5. **Portrait 1080×1920 first** — primary CanvasScaler reference. Landscape is secondary, not denied.
6. **Safe-area respect** — every full-canvas element uses `SafeAreaFitter`.
7. **Self-contained back behavior** — every popup overrides `OnBackPressed` explicitly.
8. **Service decoupling** — emit events (`OnConfirmed`, `OnPurchased`, `OnRewardClaimed`); the host game wires chains.
9. **Min touch target** — every interactive element ≥ `Theme.MinTouchTarget` (default 88 pt / ~44 dp).

### 3.2 MUSTN'T

1. **No popup may call `PopupManager.Show` for another popup** — emit an event; the host wires the chain.
2. **No popup may write `Time.timeScale`** — only the game/router responds to `AppState.Paused`. **Exception: `PausePopup`** is the documented opt-in owner of `Time.timeScale` while visible (industry-standard mobile pattern).
3. **No element may hold a static reference** to a service or another element.
4. **No element may assume a specific service implementation** — only the interface.
5. **No element may persist via `PlayerPrefs` / SaveSystem directly** — go through `IPlayerDataService`.
6. **No element may load assets via `Resources.Load`** — Inspector references only.
7. **No element may register itself globally** — registries are owned by managers via Inspector.
8. **No element may play audio inline** — emit events; SFX is a Theme concern.

---

## 4. Service binding pattern

```text
KitforgeRoot (GameObject)
├── UIManager
├── PopupManager
├── ToastManager
├── UIServices
│   ├── _economyService   (MonoBehaviour ref — buyer's impl)
│   ├── _playerService    (MonoBehaviour ref)
│   ├── _adsService       (MonoBehaviour ref)
│   ├── _timeService      (MonoBehaviour ref)
│   ├── _progressionSvc   (MonoBehaviour ref)
│   ├── _shopProvider     (MonoBehaviour ref)
│   ├── _localization     (MonoBehaviour ref)
│   └── _audioRouter      (MonoBehaviour ref)
└── KitforgeThemeBinder._theme
```

Popups query: `UIManager.Services.Economy.GetCoins()`.

VContainer / Zenject users wrap their container resolution into `UIServices` setters at boot. The Runtime asmdef stays DI-free.

---

## 5. Per-element specs

Each catalog element has a markdown spec under `Documentation~/Specs/Catalog/<Element>.md`:

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
Empty data, missing service, multi-instance, etc.
```

---

## 6. Out of scope

- Localization (BYO — Non-goal #2 in README).
- Real economy / ads / save backends — interfaces only.
- Custom shaders / VFX.
- Visual screen-graph editor.
- UI Toolkit runtime equivalents — UGUI only.
- Multi-orientation auto-handling — portrait first.
