# GameOverPopup

## Purpose
Modal that announces level failure / death — Continue (rewarded ad OR currency cost), Restart, optional MainMenu CTAs. Element 3/5 of Group C. Validates the **two-mode Continue** pattern (ad-or-currency-or-both) without coupling popup to ad/economy services.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| GO1 | Continue cost mode | `ContinueMode` enum with `None`, `Ad`, `Currency`, `AdOrCurrency`. Drives which Continue CTAs render. `AdOrCurrency` shows BOTH side-by-side (player picks). | Covers full mid-core spectrum. Hyper-casual = `Ad`. Premium = `Currency`. Hybrid = `AdOrCurrency`. |
| GO2 | Continues-per-session limit | DTO carries `ContinuesUsedThisSession` and `MaxContinuesPerSession` (default `1` for `Ad`, `int.MaxValue` for `Currency`). When limit reached, Continue CTAs hidden, only Restart/MainMenu shown. | Standard mobile guard against ad-spam. Buyer overrides via DTO. |
| GO3 | Currency cost source | `ContinueCurrency: CurrencyType` and `ContinueAmount: int` in DTO. Popup queries `IEconomyService.CanAfford(currency, amount)` to gate the Currency CTA — disables (grays) when can't afford. Click on disabled = no-op + emit `OnContinueAffordCheckFailed(currency, amount)` (host wires to Shop or NotEnough). | Reuses Group B affordability pattern. Decoupled — popup doesn't open NotEnough itself. |
| GO4 | Continue mutation responsibility | Popup does NOT call `IEconomyService.Spend` nor `IAdsService.ShowRewardedAd`. Emits `OnContinueWithAdRequested` / `OnContinueWithCurrencyRequested(currency, amount)`. Host wires both. | Cumple MUSTN'T #1. Test scenarios stay deterministic. Same pattern as RewardPopup R3. |
| GO5 | Auto-trigger | NOT auto-triggered by popup. Host's game controller decides when to `popups.Show<GameOverPopup>(data)`. No `GameOverFlow.ShowOnDeath()` helper — would require coupling popup to game state which is out of kit scope. | The kit is UI infrastructure, not game flow. |
| GO6 | Quit confirmation | `ShowMainMenu` CTA emits `OnMainMenuRequested` directly. Host optionally chains `popups.Show<ConfirmPopup>("Are you sure?")` before navigating. NOT chained inside GameOverPopup. | Decoupling. ConfirmPopup is the buyer's tool. |
| GO7 | Backdrop / back press | `CloseOnBackdrop=false` default. Back press routes to `OnRestartRequested` (default destructive-but-safe action). Buyer can override via `BackPressBehavior` enum (`Restart`, `MainMenu`, `Ignore`). | Game-state popup — accidental dismiss without action is wrong. |
| GO8 | Score visibility | Optional `Score` field. When `>= 0`, render score label. When `< 0`, hide score block entirely. | "Score" doesn't fit all genres. Match-3 has score; runner has distance. Buyer just doesn't bind, popup hides. |

## DTO

`GameOverPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Game Over"` | Header label. |
| `Subtitle` | `string` | `""` | Optional second line. |
| `Score` | `int` | `-1` | Per GO8. `-1` hides score block. |
| `ContinueMode` | `ContinueMode` | `Ad` | Per GO1. |
| `ContinueCurrency` | `CurrencyType` | `Gems` | Used when `ContinueMode` includes Currency. |
| `ContinueAmount` | `int` | `5` | Currency cost. |
| `ContinuesUsedThisSession` | `int` | `0` | Per GO2. |
| `MaxContinuesPerSession` | `int` | `1` | Per GO2. |
| `ContinueAdLabel` | `string` | `"Continue"` | Ad CTA label. |
| `ContinueCurrencyLabel` | `string` | `"Continue ({amount})"` | Currency CTA label (`{amount}` replaced). |
| `RestartLabel` | `string` | `"Restart"` | CTA label. |
| `MainMenuLabel` | `string` | `"Main Menu"` | CTA label. |
| `ShowRestart` | `bool` | `true` | Visibility. |
| `ShowMainMenu` | `bool` | `true` | Visibility (default `true` for GameOver — exit affordance is critical). |
| `BackPressBehavior` | `BackPressBehavior` | `Restart` | Per GO7. |
| `CloseOnBackdrop` | `bool` | `false` | Per GO7. |

`ContinueMode` enum: `None`, `Ad`, `Currency`, `AdOrCurrency`.
`BackPressBehavior` enum: `Restart`, `MainMenu`, `Ignore`.

`Bind(null)` → falls back to fresh instance.

## Service binding
Null-service behavior follows `CATALOG_GroupC_DELTA.md` § 4.5.

- `IAdsService` (optional). `IsRewardedAdReady()` queried on Bind to gate Ad CTA — disabled (grayed) when not ready.
- `IEconomyService` (optional). `CanAfford(currency, amount)` queried on Bind to gate Currency CTA per GO3.
- `IUIAudioRouter` (optional). Cues: `PopupOpen`, `Failure` (Show — distinct from Success), `ButtonTap` (any CTA), `PopupClose`.

## Events emitted

| Event | When |
|---|---|
| `OnContinueWithAdRequested` | Ad Continue CTA. Then popup dismisses. Host calls `IAdsService.ShowRewardedAd()` then on success resumes game. |
| `OnContinueWithCurrencyRequested(CurrencyType currency, int amount)` | Currency Continue CTA when affordable. Then popup dismisses. Host calls `IEconomyService.Spend(currency, amount)` then resumes. |
| `OnContinueAffordCheckFailed(CurrencyType currency, int amount)` | Currency Continue CTA click when NOT affordable (the gray-disabled state still emits this on click). Popup does NOT dismiss. Host typically calls `popups.Show<ShopPopup>()` or `popups.Show<NotEnoughCurrencyPopup>()`. |
| `OnRestartRequested` | Restart CTA OR back press (if `BackPressBehavior=Restart`). Then popup dismisses. |
| `OnMainMenuRequested` | Main Menu CTA OR back press (if `BackPressBehavior=MainMenu`). Then popup dismisses. |
| `OnDismissed` | Always after hide animation. Fires AFTER above. |

All events reset on `Bind(...)`.

## Animation contract
- `[RequireComponent(typeof(UIAnimGameOverPopup))]`. Auto-resolved.
- Show: fade-in backdrop 0.4s → scale pop-in card 0.35s → CTA cascade 0.15s stagger.
- Hide: scale-down + fade-out.
- Style preset = `Cinematic` recommended (somber, deliberate — distinct from LevelComplete `Cinematic` celebration via `Theme.FailureColor` accents instead of Success).
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`.

## Theme tokens consumed
- `FailureColor` (NEW Theme slot — added in Group C; header tint, particle accents).
- `IconCoin` / `IconGem` / `IconEnergy` (currency cost icon on Currency CTA).
- `PrimaryColor` (Continue Ad button).
- `SecondaryColor` (Continue Currency button).
- `TertiaryColor` (Restart button).
- `QuaternaryColor` or `MutedColor` (Main Menu button — least-emphasized).
- `DefaultAnimPreset`.

## Edge cases
- **`ContinueMode=None`**: only Restart + MainMenu visible. Ad + Currency CTAs hidden.
- **`ContinueMode=Ad` but `IAdsService=null` or `IsRewardedAdReady=false`**: Ad CTA visible but disabled (grayed). Click emits no event (silent). Buyer should set `ContinueMode=None` if no ad service is wired.
- **`ContinueMode=Currency` with no `IEconomyService`**: Currency CTA visible but disabled. Click emits no event.
- **`ContinuesUsedThisSession >= MaxContinuesPerSession`**: ALL Continue CTAs hidden regardless of `ContinueMode`. Falls back to Restart/MainMenu only.
- **All CTAs hidden** (limit reached + `ShowRestart=false` + `ShowMainMenu=false`): popup logs error in `Bind`, forces `ShowMainMenu=true` (foot-gun guard — player must have an exit).
- **`Score < 0`**: score block hidden silently. Not an error.
- **Re-Show same instance**: `Bind` resets events + recomputes affordability + ad readiness.
- **Race conditions** (`IsDismissing` guard): all CTAs guarded; back press during hide ignored.
- **Continue affordability changes between Bind and click**: Bind snapshots state. Click on currency CTA re-checks `CanAfford` → if changed to unaffordable, emits `OnContinueAffordCheckFailed` instead of `OnContinueWithCurrencyRequested`.
- **Ad failure path post-`OnContinueWithAdRequested` (FQ4)**: Popup is already dismissed when host calls `IAdsService.ShowRewardedAd()`. Ad failure (network error, no fill, user closed early) is the **host's concern, not the popup's**. Host's options: (a) re-show the same `GameOverPopupData` clone with `ContinueMode=Currency` (if affordable) or `None` (if not), (b) play `IUIAudioRouter.Failure` cue + show `NotificationToast` with retry CTA, (c) auto-restart level. The kit does NOT prescribe — `as user`/`as ux` validated this is buyer territory. Spec only guarantees popup behavior up to the event emission.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — Ad continue, ads ready` (default).
2. `Show — Ad continue, ads NOT ready` (gray button).
3. `Show — Currency continue, can afford` (5 gems, has 10).
4. `Show — Currency continue, can NOT afford` (5 gems, has 2 — gray button).
5. `Show — AdOrCurrency, both available`.
6. `Show — Limit reached` (`ContinuesUsedThisSession=1, Max=1`) — Continue hidden.
7. `Show — No score` (`Score=-1`) — score block hidden.
8. `Click — Continue Ad` (verify `OnContinueWithAdRequested` + dismiss).
9. `Click — Continue Currency, affordable` (verify `OnContinueWithCurrencyRequested(Gems, 5)` + dismiss).
10. `Click — Continue Currency, can't afford` (verify `OnContinueAffordCheckFailed` + NO dismiss).
11. `Click — Restart` (verify `OnRestartRequested` + dismiss).
12. `Click — Main Menu` (verify `OnMainMenuRequested` + dismiss).
13. `Back press — BackPressBehavior=Restart` (verify Restart event).
14. `Back press — BackPressBehavior=MainMenu` (verify MainMenu event).
15. `Back press — BackPressBehavior=Ignore` (verify nothing happens).
16. `Stress — Spam Continue Ad` (single event + single dismiss).
17. `Foot-gun guard` — bind with `ShowRestart=false, ShowMainMenu=false, ContinuesUsed >= Max` — verify popup forces `ShowMainMenu=true` and logs error.
18. `DTO defaults assertion (FD3)` — `var data = new GameOverPopupData(); Assert.AreEqual("Game Over", data.Title); Assert.AreEqual(-1, data.Score); Assert.AreEqual(ContinueMode.Ad, data.ContinueMode); Assert.AreEqual(CurrencyType.Gems, data.ContinueCurrency); Assert.AreEqual(5, data.ContinueAmount); Assert.AreEqual(0, data.ContinuesUsedThisSession); Assert.AreEqual(1, data.MaxContinuesPerSession); Assert.AreEqual(true, data.ShowRestart); Assert.AreEqual(true, data.ShowMainMenu); Assert.AreEqual(BackPressBehavior.Restart, data.BackPressBehavior); Assert.AreEqual(false, data.CloseOnBackdrop);`. Per `feedback_workflow.md` L3.

## Files
```
Runtime/Catalog/Popups/GameOver/
├── GameOverPopupData.cs
├── ContinueMode.cs
├── BackPressBehavior.cs
├── GameOverPopup.cs
└── UIAnimGameOverPopup.cs
Tests/Editor/
└── GameOverPopupTests.cs
```

## Status
- Code: ⏳ pending (Group C · element 3/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~12 — affordability matrix is wide)
- Prefab: ⏳ pending (`CatalogGroupCBuilder.BuildGameOverPopup`)
- Demo scene entry: ⏳ pending
