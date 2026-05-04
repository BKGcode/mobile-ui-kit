# HUD-Timer

## Purpose
Live timer label — three modes: countdown to UTC target, countup since UTC start, or local stopwatch (Time.time-based). Element 5/5 of Group C. The transversal time-display HUD reused by level timers, event countdowns, energy-regen overlays, daily-reset clocks, etc. NOT bound to any currency.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| **T1** | Class strategy | Single `HUDTimer : UIHUDBase` class with `TimerMode` enum (`CountdownToTarget` / `CountupSinceTarget` / `LocalStopwatch`) and `[SerializeField]` source fields per mode. Inspector hides irrelevant fields via custom drawer? **NO — plain primitive solves it** per `feedback_workflow.md` editor-tooling threshold. All fields visible, mode comment in tooltip. **Mode immutability post-OnEnable (FQ6)**: `_mode` is read in `OnEnable` and used to wire tick source / event guards. Changing `_mode` while enabled has UNDEFINED behavior. Buyer who needs to swap modes: `gameObject.SetActive(false); _mode = newMode; SetTarget(...); gameObject.SetActive(true);`. No `SetMode()` runtime helper in MVP — capability-gate failure (1 callsite hypothetical, vs disable/enable cycle which is 2 lines). | Plain. Mirrors HUDCurrency parameterized pattern. Custom drawer for mode-conditional visibility = over-engineering for 3 enum values. Immutable mode keeps tick logic single-pass. |
| T2 | Tick rate | 1Hz default for `CountdownToTarget` and `CountupSinceTarget` (server-time computed — sub-second precision is meaningless). 30Hz for `LocalStopwatch` (millisecond precision matters for levels). Inspector field `_tickRateHz` (default 1f for first two, 30f for stopwatch). | Match precision to use case. Cheap. |
| T3 | Format string | Default `"mm:ss"` for `CountdownToTarget` / `CountupSinceTarget`. Default `"mm:ss.ff"` for `LocalStopwatch`. Buyer overrides via `_formatString` Inspector field. | Standard `TimeSpan.ToString` format. Buyer can localize. |
| T4 | Target source | `CountdownToTarget` / `CountupSinceTarget`: `_targetUtc: DateTime` field. Buyer sets via public `SetTarget(DateTime utc)` method (no Inspector field for runtime-only sources like "now + 5 min"). For static targets (event end), Inspector accepts ISO string field `_targetUtcIso` parsed in Awake. `LocalStopwatch`: starts on `OnEnable`, resettable via public `Reset()`. | Two paths: code-driven and Inspector-driven. Buyer picks. |
| T5 | Expiry handling | When `CountdownToTarget` reaches zero: emits `OnExpired` event, then optionally hides label OR shows `_expiredLabel` (default `"00:00"`) per `_hideOnExpire` flag. Tick stops on expiry. | Configurable. Hyper-casual hides; mid-core event timers show `"Ended"`. |
| T6 | Warning threshold | Optional `_warningThresholdSeconds: float` (default `0f` = disabled; TimeSpan can't be SerializeField). When countdown drops below threshold, label tints with `_warningColor` SerializeField (default ≈ `Theme.WarningColor`; HUDTimer does NOT read Theme directly — consistency with HUDEnergy decision to keep HUDs decoupled from `UIThemeConfig`). Pulse tween (`_warningTween`) fires as YoYo color loop. | Standard "last 10s flash red" pattern. SerializeField color keeps the HUD self-contained — buyer can theme via Inspector or runtime assignment without coupling to `IUIThemeProvider`. |
| T7 | Pause / Resume | Public `SetPaused(bool)` method. When paused, `LocalStopwatch` accumulates pause time, `CountdownToTarget` does NOT (server time keeps ticking — pausing a real-time event is buyer's concern). Inspector flag `_pausesWithTimeScale` (default `false`) makes `LocalStopwatch` respect `Time.timeScale`. | Stopwatch is local — host can pause. UTC modes are absolute — host can't pause real time. |
| T8 | Click behavior | Inherits UnityEvent pattern from base. `_onTimerClickEvent` exposed. Default unwired. Buyer typically wires to "open shop" (event timer) or telemetry. | Same pattern as HUDCurrency. |
| T9 | Initial value source | `OnEnable` calls `Refresh()` which computes initial elapsed/remaining and snaps without animation. Subsequent ticks animate (warning pulse, expiry flash). | Avoid jarring "00:00 → 04:32" ramp on screen load. |
| T10 | Multiple timers per scene | Supported. Each `HUDTimer` instance is independent (own `_targetUtc`, own tween). No shared state. | Composability. |

## DTO
**No DTO**. Inspector-configured + service-driven. Same as HUDCoins/HUDGems/HUDEnergy.

## Service binding
Null-service behavior follows `CATALOG_GroupC_DELTA.md` § 4.5 (HUD = silent degrade — see Edge cases for per-service fallback).

- `ITimeService` (REQUIRED via `_services` ref) for `CountdownToTarget` / `CountupSinceTarget` modes. Reads `GetServerTimeUtc()` per tick.
- `IUIAudioRouter` (optional). Cue: `Error` (warning threshold crossed inbound — once; reused as the closest semantic match — `UIAudioCue.Failure` does NOT exist in the enum, drift fix vs older spec wording, same precedent as GameOver), `Success` (expiry — once). Off by default to avoid annoying gameplay HUD.
- `LocalStopwatch` mode does NOT require `ITimeService` — uses `Time.unscaledTime` (or `Time.time` if `_pausesWithTimeScale=true`).

## Events emitted

| Event | When |
|---|---|
| `OnExpired` (C# `event Action`) | `CountdownToTarget` mode only. Fires once when remaining time crosses zero. |
| `OnWarningEntered` (C# `event Action`) | `CountdownToTarget` mode only. Fires once when remaining drops below `_warningThreshold`. |
| `_onTimerClickEvent` (UnityEvent) | `Refs.ClickButton` non-null and clicked. |

C# events reset on `SetTarget(...)` or `Reset()`.

## Animation contract
- NO `IUIAnimator` component (HUD lifecycle).
- Inline DOTween:
  - Warning pulse on label using `Theme.WarningColor` (color + scale punch). Stored in `_warningTween`. Fires on `OnWarningEntered`. Loops while in warning zone (`SetLoops(-1, LoopType.Yoyo)`). Killed on expiry or warning exit.
  - Expiry flash using `Theme.SuccessColor` (or `FailureColor` per `_expiryStyle` flag). Stored in `_expiryTween`. Fires once on `OnExpired`.
- All tweens `SetLink(gameObject)`, killed in `OnDisable`/`OnDestroy`.

## Theme tokens consumed
- `TextPrimary` (default label color via `ThemedText`).
- `WarningColor` (warning pulse + warning-zone tint).
- `SuccessColor` / `FailureColor` (expiry flash, picked by `_expiryStyle`).
- `FontBody` or `FontDisplay` (Inspector-selectable).
- `IconClock` (NEW Theme slot — optional icon left of label, applied via `ThemedImage` if `Refs.IconImage` non-null).

## Edge cases
- **`_services` null OR `Services.Time` null in UTC mode**: label shows `"--:--"` SILENTLY, no LogError. Drift fix vs older "logs error" wording — `CATALOG_GroupC_DELTA.md` § 4.5 (HUD = silent degrade) wins; HUDs live on screens and a missing time service is a valid "feature not configured" state during scene-edit iteration. Stopwatch mode unaffected (uses `Time.unscaledTime` / `Time.time` per `_pausesWithTimeScale`).
- **`_targetUtc` default (`DateTime.MinValue`)**: countdown shows huge negative remaining → clamped to "00:00" + immediate expiry. Buyer must call `SetTarget` before `OnEnable` OR set `_targetUtcIso` in Inspector.
- **Server clock drift** (`ITimeService.GetServerTimeUtc()` jumps backwards): tick recomputes from new now. Possible visual glitch (timer "rewinds"). Buyer's `ITimeService` impl should clamp.
- **Format string invalid** (e.g. typo): falls back to `"mm:ss"` and logs error once.
- **Warning threshold > target duration**: warning fires immediately on Bind. Pulse loop runs entire countdown.
- **Stopwatch with `_pausesWithTimeScale=true` and `Time.timeScale=0`**: stopwatch frozen. Resumes on `Time.timeScale > 0`.
- **`SetPaused(true)` then disable then enable**: stopwatch resumes from paused value. UTC modes ignore pause state.
- **Multiple `HUDTimer` instances ticking simultaneously**: independent, no contention. Tested in QA scenario 11.
- **`OnExpired` with no listener**: silent. Label still flashes per `_hideOnExpire` flag.
- **Negative `_warningThreshold`**: silently treated as `TimeSpan.Zero` (disabled).

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Countdown — 5 minutes` (UTC target).
2. `Countdown — expired` (target in past, verify immediate `OnExpired` + flash + `"00:00"` or hidden per flag).
3. `Countdown — warning at 10s` (`_warningThreshold=00:00:10`, verify pulse fires + tint).
4. `Countup — since 30 seconds ago` (UTC start).
5. `Stopwatch — running` (verify mm:ss.ff format + 30Hz).
6. `Stopwatch — pause + resume` (`SetPaused(true)`, wait 5s, `SetPaused(false)`, verify no jump).
7. `Stopwatch — Time.timeScale=0` with `_pausesWithTimeScale=true` (verify freeze).
8. `Stopwatch — Reset()` (verify back to 00:00.00).
9. `Click HUD` (verify UnityEvent fires).
10. `Disable + re-enable` (verify subscription survives, no double-tick).
11. `Multi-timer — 3 instances` (different modes, verify independent).
12. `Format override — "HH:mm:ss"` (verify long-form).
13. `Null ITimeService in countdown mode` (verify `"--:--"` + error log, no NRE).
14. `Server time jump backward` (verify clamp + log).

## Files
```
Runtime/Catalog/HUD/
├── HUDTimer.cs
└── TimerMode.cs                ← enum: CountdownToTarget, CountupSinceTarget, LocalStopwatch
Tests/Editor/
└── HUDTimerTests.cs
```

## Status
- Code: ✅ delivered 2026-05-04 — `Runtime/Catalog/HUD/HUDTimer.cs` + `Runtime/Catalog/HUD/TimerMode.cs`. Single class with `TimerMode` enum (CountdownToTarget / CountupSinceTarget / LocalStopwatch), `[Serializable] private struct TimerRefs`, format/target/expiry/warning/pause SerializeFields, `RealTimeProviderForTests` Func injection, `Update() → OnUpdate() → tick rate throttle` chain (anchor `// OnUpdate-workaround-M3-sweep` per mejora F), `OnEnable` bypasses `base.OnEnable` when `Services==null && _mode==LocalStopwatch` (stopwatch doesn't need services), public `SetTarget`/`Reset`/`SetPaused` API, C# events `OnExpired`/`OnWarningEntered` cleared via `ResetTransientFlags` in `SetTarget`/`Reset`, format-string validation with try/catch FormatException + fallback to `"mm\\:ss"` + LogError once.
- Spec: ✅ this document
- Tests: ✅ delivered 2026-05-04 — `Tests/Editor/HUDTimerTests.cs` 10 tests covering countdown initial/tick/expiry/warning/hide-on-expire / countup initial / stopwatch zero/pause/reset / UTC null-time silent (§ 4.5 drift fix). **199/199 tests verde sobre fresh compile.**
- Prefab: ⏳ pending (`CatalogGroupCBuilder.BuildHUDTimer` — one prefab per mode, three presets total)
- Demo scene entry: ⏳ pending (HUD on persistent canvas)
