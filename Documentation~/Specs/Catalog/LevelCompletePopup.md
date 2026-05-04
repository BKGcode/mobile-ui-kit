# LevelCompletePopup

## Purpose
Modal that announces a completed level — stars (0-3), score, optional new-best banner, optional reward sequence, and Next/Retry/MainMenu CTAs. Element 2/5 of Group C. Validates `RewardFlow.GrantAndShowSequence` integration as the primary post-popup chain.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| L1 | Star representation | `int Stars` (0-3) in DTO. Popup renders 3 star slots with `Refs.StarImages[3]` array. Slots 0..Stars-1 use `Theme.StarFilledSprite`, rest use `Theme.StarEmptySprite`. Cap `Stars` to `[0, 3]` defensively. | Plain primitive solves it. Matches `feedback_workflow.md` editor-tooling threshold — no custom drawer needed. |
| L2 | Star reveal animation | Cascade-in with stagger (0.15s per star) on Show. Each filled star punch-scales + plays optional `_starParticle`. Empty stars fade in only. | Disney Getaway Blast vibe. Tunable via Theme animation tokens. |
| L3 | Reward integration | Popup does NOT show rewards inline. DTO carries `Rewards: RewardPopupData[]`. On `OnNextRequested` (or any CTA) AFTER popup dismisses, host calls `RewardFlow.GrantAndShowSequence(rewards, onSequenceComplete: openNextLevel)`. | Cumple MUSTN'T #1 (popup doesn't open another popup) and reuses Group C helper. Single responsibility. |
| L4 | New-best detection | DTO carries explicit `IsNewBest: bool`. Host computes (host owns score history). Popup shows "NEW BEST!" banner with Theme.SuccessColor when `true`. | Avoids popup querying `IProgressionService` (decoupling). Host knows. |
| L5 | CTA visibility | `ShowNext` / `ShowRetry` / `ShowMainMenu` flags. Defaults: `Next=true`, `Retry=true`, `MainMenu=false`. Last level of campaign sets `Next=false`. | Mirrors NotEnoughCurrencyPopup CTA pattern. |
| L6 | Score animation | Roll-up tween from 0 to `Score` over 0.5s using DOTween `DOInt`. Optional `BestScore` displayed alongside (no animation, just label). | Standard mobile feedback. Tunable via `_scoreRollupDuration`. |
| L7 | Backdrop / back press | `CloseOnBackdrop=false` default — modal blocker. Back press routes to `OnRetryRequested` (single-tap "try again" affordance). | Game-state popup, accidental dismiss is destructive. |

## DTO

`LevelCompletePopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Level Complete!"` | Header label. |
| `LevelLabel` | `string` | `""` | Optional subtitle (e.g. `"Level 7"`). |
| `Stars` | `int` | `0` | Clamped to `[0, 3]` on Bind. |
| `Score` | `int` | `0` | Final score (rolled up via tween). |
| `BestScore` | `int` | `0` | Personal best (static label). |
| `IsNewBest` | `bool` | `false` | Per L4. Drives banner visibility. |
| `Rewards` | `RewardPopupData[]` | `null` | Per L3. Empty/null = no post-popup reward chain. |
| `NextLabel` | `string` | `"Next"` | CTA label. |
| `RetryLabel` | `string` | `"Retry"` | CTA label. |
| `MainMenuLabel` | `string` | `"Main Menu"` | CTA label. |
| `ShowNext` | `bool` | `true` | Per L5. |
| `ShowRetry` | `bool` | `true` | Per L5. |
| `ShowMainMenu` | `bool` | `false` | Per L5. |
| `CloseOnBackdrop` | `bool` | `false` | Per L7. |

`Bind(null)` → falls back to fresh instance.

## Service binding
Null-service behavior follows `CATALOG_GroupC_DELTA.md` § 4.5.

- `IUIAudioRouter` (optional). Cues: `PopupOpen`, `Success` (per star reveal + new-best banner), `ButtonTap` (any CTA), `PopupClose`.
- No `IEconomyService` access — see L3.
- No `IProgressionService` access — see L4.

## Events emitted

| Event | When |
|---|---|
| `OnNextRequested(LevelCompletePopupData data)` | Next CTA. Then popup dismisses. Host typically calls `RewardFlow.GrantAndShowSequence(data.Rewards, onSequenceComplete: loadNextLevel)`. |
| `OnRetryRequested(LevelCompletePopupData data)` | Retry CTA OR back press. Then popup dismisses. Host typically restarts level (rewards skipped — design choice per buyer). |
| `OnMainMenuRequested(LevelCompletePopupData data)` | Main Menu CTA. Then popup dismisses. |
| `OnDismissed` | Always after hide animation. Fires AFTER above. |

DTO passed back in event args lets host route to reward sequencing without re-querying.

All events reset on `Bind(...)`.

## Animation contract
- `[RequireComponent(typeof(UIAnimLevelCompletePopup))]`. Auto-resolved.
- Show: scale pop-in + fade-in on `_card`, THEN star cascade (per L2), THEN score rollup (per L6), THEN CTA fade-in (0.2s after score lands).
- Hide: scale-down + fade-out, then `FinalizeDismissal`.
- Style preset = `Cinematic` recommended (staggered cascade matches the multi-stage reveal).
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`.

## Theme tokens consumed
- `StarFilledSprite` / `StarEmptySprite` (NEW Theme slots — added in Group C).
- `SuccessColor` (new-best banner tint, star particle color).
- `PrimaryColor` (Next button — primary action).
- `SecondaryColor` (Retry button).
- `TertiaryColor` (Main Menu button).
- `DefaultAnimPreset`.
- `FontHeader` / `FontDisplay` (score label uses display font).

## Edge cases
- **`Stars` out of range** (negative or >3): clamped to `[0, 3]` in `Bind` with warning log.
- **`Rewards = null` or empty**: Next CTA still works; host receives empty array, skips reward sequence.
- **`IsNewBest=true` but `BestScore == 0`**: banner shows anyway — host's truth.
- **All CTAs hidden** (`ShowNext=false`, `ShowRetry=false`, `ShowMainMenu=false`): popup logs error in `Bind`, falls back to showing Retry only (foot-gun guard, mirrors NotEnoughCurrencyPopup pattern).
- **Re-Show same instance**: `Bind` resets events + restarts star cascade.
- **Back press during reveal sequence**: skip-to-end (all stars/score snap to final values), then dismiss as Retry per L7.
- **Score = 0**: rollup tween skipped (no animation on no-change).
- **Race conditions** (`IsDismissing` guard): all CTAs guarded against double-fire; back press during hide ignored.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — 3 stars, new best, +100 coins reward`.
2. `Show — 1 star, no rewards, no new-best`.
3. `Show — 0 stars` (failure-to-perfect-fallback path).
4. `Show — Last level (ShowNext=false)`.
5. `Show — With main menu CTA (ShowMainMenu=true)`.
6. `Show — Score rollup 0 → 999,999` (verify tween, formatting).
7. `Click — Next` (verify `OnNextRequested` + dismiss + DTO in args).
8. `Click — Retry` (verify `OnRetryRequested` + dismiss).
9. `Back press` (verify `OnRetryRequested` per L7).
10. `Reward chain` — Next click → host calls `RewardFlow.GrantAndShowSequence(data.Rewards)` → 2 reward popups in sequence → `onSequenceComplete` fires.
11. `Stress — Spam Next` (single dismiss).
12. `Re-Show — Stars 1 → 3` (verify stale stars cleared, fresh cascade).
13. `DTO defaults assertion (FD3)` — `var data = new LevelCompletePopupData(); Assert.AreEqual("Level Complete!", data.Title); Assert.AreEqual(0, data.Stars); Assert.AreEqual(0, data.Score); Assert.AreEqual(0, data.BestScore); Assert.AreEqual(false, data.IsNewBest); Assert.AreEqual("Next", data.NextLabel); Assert.AreEqual(true, data.ShowNext); Assert.AreEqual(true, data.ShowRetry); Assert.AreEqual(false, data.ShowMainMenu); Assert.AreEqual(false, data.CloseOnBackdrop);`. Per `feedback_workflow.md` L3.

## Files
```
Runtime/Catalog/Popups/LevelComplete/
├── LevelCompletePopupData.cs
├── LevelCompletePopup.cs
└── UIAnimLevelCompletePopup.cs
Tests/Editor/
└── LevelCompletePopupTests.cs
```

## Status
- Code: ⏳ pending (Group C · element 2/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~8)
- Prefab: ⏳ pending (`CatalogGroupCBuilder.BuildLevelCompletePopup`)
- Demo scene entry: ⏳ pending
