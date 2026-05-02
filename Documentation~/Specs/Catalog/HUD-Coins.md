# HUD-Coins

## Purpose
Live coin counter that lives on the gameplay screen (NOT in popup registry), reactive to `IEconomyService.OnCoinsChanged`. Element 4/5 of Group B. First concrete consumer of `UIHUDBase` from Group 0.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| HC1 | Class hierarchy | `HUDCoins : UIHUDBase` (sealed). No further subclassing — buyer customizes via Theme + Refs. | Avoids over-abstraction. Per ecosystem feedback (KF_MobileUIKit pre-Group B): "if a primitive solves it, don't abstract". UIHUDBase IS the abstraction. |
| HC2 | Animation on change | Punch-scale tween on counter label when value changes. `_changeAnimDuration` (default 0.18s) + `_changeAnimScale` (1.20) Inspector-tunable. | Standard mobile feedback ("juicy"). Style preset = `Punchy`. |
| HC3 | Number formatting | Default integer with thousand separators (`12,345`). Buyer overrides via `_formatString` Inspector field (default `"N0"`). | Plain int + `ToString(format)`. No new dependency. |
| HC4 | Initial value source | `OnEnable` calls `Refresh()` which reads `Services.Economy.GetCoins()`. No initial animation (snap to current). | Avoids "0 → 1000 ramp" on every screen load. |
| HC5 | Click behavior | `_onCoinClickEvent` `UnityEvent` exposed. Default = nothing. Host wires to "open shop" or telemetry. | Buyer pain: HUD coin counters are usually clickable to open the shop. Decoupled via UnityEvent so no service-aware logic in HUD. |
| HC6 | Missing service handling | If `Services.Economy == null` → label shows `"--"` and logs error. No NRE. | Cumple "fail loud, not silent". |

## DTO

**No DTO**. HUD elements are NOT modules — they live on the screen and are configured via Inspector + Theme. Lifecycle is `Awake`/`OnEnable`/`OnDisable`/`OnDestroy`, not `Bind/OnShow/OnHide`.

## Services consumed
- `IEconomyService` (REQUIRED via `_services` ref). Subscribes to `OnCoinsChanged` in `Subscribe()`. Reads `GetCoins()` in `Refresh()`.
- No `IUIAudioRouter` (HUD is silent — sounds are emitted by source events, not HUD).

## Events emitted

| Event | When |
|---|---|
| `_onCoinClickEvent` (UnityEvent) | `Refs.ClickButton` is non-null and clicked. Host wires this to e.g. `PopupManager.Show<ShopPopup>`. |

No C# events (`event Action`) — only the serialized UnityEvent. HUD is presentation-layer; chain wiring lives in the host scene.

## Animation contract
- NO `IUIAnimator` component required (HUD lifecycle differs from popups — no Show/Hide).
- Inline DOTween punch on counter label per HC2: stored in `_punchTween` field, killed before re-fire, `SetLink(gameObject)`.
- Punch only fires when delta != 0. Initial `Refresh()` (Subscribe-time) snaps without animation.

## Theme tokens consumed
- `IconCoin` (icon Image — applied at prefab authoring via `ThemedImage`).
- `TextPrimary` (counter label color via `ThemedText`).
- `FontBody` (counter font via `ThemedText`).

## Edge cases
- **`_services` null**: error log + label `"--"`. No subscription. `OnDisable` no-op.
- **Service set but `Economy == null`**: same as above.
- **Service swapped at runtime** (host calls `UIServices.SetEconomy(other)`): NOT auto-detected. Buyer must `Disable + Enable` the HUD GameObject. Documented limitation.
- **Negative value** (not normally possible in `IEconomyService` but defensive): formats with `-` sign per `N0`. Punch tween still fires.
- **Massive jump** (0 → 1,000,000): single punch tween, single label update. No staggered count-up animation in MVP.
- **Disable while tween active**: `OnDisable` kills `_punchTween`.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Initial value` — load scene with stub returning 100 coins. Verify label = "100".
2. `Add 50 coins` — call `IEconomyService.AddCoins(50)`. Verify label updates + punch tween.
3. `Spend 30 coins` — call `SpendCoins(30)`. Verify decrement + punch tween.
4. `Massive add` — `AddCoins(999000)`. Verify formatting (`999,100`) + tween.
5. `Click HUD` — verify UnityEvent fires (host hooks log).
6. `Disable + re-enable` — verify subscription survives.
7. `Null service` — verify `"--"` label + error log.
8. `Spam changes` (10 in 0.1s) — verify last value is correct, single tween at a time (kill-before-create).

## Files
```
Runtime/Catalog/HUD/
└── HUDCoins.cs
Tests/Editor/
└── HUDCoinsTests.cs
```

(HUD-Gems is the sibling element — see `HUD-Gems.md`.)

## Status
- Code: ⏳ pending (Group B · element 4/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~3 — limited testability without runtime tween, focuses on subscribe/unsubscribe + null service)
- Prefab: ⏳ pending (`CatalogGroupABuilder.BuildHUDCoins`)
- Demo scene entry: ⏳ pending (HUD lives on the persistent canvas, NOT spawned by `[ContextMenu]`)
