# RewardPopup

## Purpose
Modal that announces a reward (currency, item, or bundle) and waits for the player to claim. Element 1/5 of Group B.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| R1 | Reward kinds in scope | `Coins`, `Gems`, `Item` (string Id, no resolution), `Bundle` (multi-line summary) | Covers 90% of mid-core mobile reward popups without coupling to game-specific item DBs. Item resolution is buyer responsibility. |
| R2 | Auto-claim behavior | `AutoClaimSeconds` field (default `0` = disabled). When `>0`, claim auto-fires after delay if user does not tap. | Standard pattern in Royal Match / Coin Master idle rewards. Keeps player flow if they walk away. |
| R3 | Currency mutation responsibility | Popup does **NOT** call `IEconomyService.Add*`. Emits `OnClaimed(currency, amount)` and the host wires the credit. | Cumple MUSTN'T #1 (no service mutation from popup). Test scenarios stay deterministic. |
| R4 | Multi-reward presentation | Single popup instance shows ONE `RewardPopupData` (single line OR bundle). Stacked rewards = host shows N popups in sequence via event chain. | Avoids in-popup queue complexity. Bundle covers "5 coins + 1 gem" in one card. |
| R5 | Theme icon source | `Theme.IconCoin` / `Theme.IconGem` for currency. Item kind uses `Refs.RewardIcon` set by host via `RewardPopupData.IconOverride`. | Theme already exposes both currency icons. Custom items need explicit sprite injection. |

## DTO

`RewardPopupData` (class, `[Serializable]`):

| Field | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | `"Reward!"` | Header label. |
| `Kind` | `RewardKind` | `Coins` | `Coins`, `Gems`, `Item`, `Bundle`. |
| `Amount` | `int` | `0` | Used for `Coins`/`Gems`. Ignored for `Item`/`Bundle`. |
| `ItemId` | `string` | `""` | Used for `Item`. Display name only — popup does NOT resolve. |
| `IconOverride` | `Sprite` | `null` | Used for `Item`. Falls back to Theme icon for `Coins`/`Gems`. |
| `BundleLines` | `string[]` | `null` | Used for `Bundle`. Each line shown as a row in a vertical layout. |
| `ClaimLabel` | `string` | `"Claim"` | Confirm button label. |
| `AutoClaimSeconds` | `float` | `0f` | When `>0`, claim auto-fires after delay if user does not tap. |
| `CloseOnBackdrop` | `bool` | `false` | When `true`, backdrop tap routes to claim. |

`Bind(null)` → falls back to fresh `RewardPopupData()` instance.

`RewardKind` enum: `Coins`, `Gems`, `Item`, `Bundle`.

## Services consumed
- `IUIAudioRouter` (optional). Cues: `PopupOpen` (Show), `Success` (Claim), `PopupClose` (Hide).
- No `IEconomyService` write access — see decision R3.

## Events emitted

| Event | When |
|---|---|
| `OnClaimed(CurrencyType currency, int amount)` | Claim button click, OR auto-claim timeout, OR backdrop tap when `CloseOnBackdrop=true`. For `Item`/`Bundle` kinds, `currency` is sentinel `(CurrencyType)(-1)` and `amount = 0`. |
| `OnDismissed` | Always after hide animation completes — fires AFTER `OnClaimed`. Use for cleanup. |

Both events reset on every `Bind(...)` to prevent handler accumulation across re-Show.

## Animation contract
- `[RequireComponent(typeof(UIAnimRewardPopup))]`. Animator auto-resolved in `Awake` and lazily on first access.
- Show: scale pop-in + fade-in on `_card` (CanvasGroup `_canvasGroup` on root). Style preset = `Bouncy` recommended (overshoot + elastic ease for "celebration" feel).
- Hide: scale-down + fade-out, then `FinalizeDismissal` callback.
- Preset resolved as `AnimPresetOverride ?? Theme.DefaultAnimPreset`. If both null, animator no-ops.
- `OnHide` calls `Animator.Skip()` — kill any residual tween.

## Theme tokens consumed
- `IconCoin` (Coins), `IconGem` (Gems) — applied at `Bind` time on `Refs.RewardIcon`.
- `SuccessColor` (claim button tint via `Refs.ClaimTint`).
- `DefaultAnimPreset` (animator preset).
- Sprites/fonts: applied at prefab authoring time from Theme slots; not re-applied at runtime.

## Edge cases
- **Null DTO**: `OnShow` calls `Bind(null)` → empty fields, `Coins`/0/empty bundle.
- **Missing Theme**: icon falls back to `IconOverride`; if also null, icon hidden.
- **`Bundle` with null/empty `BundleLines`**: single fallback line `"(empty bundle)"` to avoid NRE.
- **`AutoClaimSeconds > 0` + user taps before timeout**: timer cancelled in `HandleClaim`. Single fire only.
- **Re-Show same instance**: `Bind` resets event listeners + `IsDismissing=false` + cancels any pending auto-claim.
- **Race conditions** (covered by `IsDismissing` guard):
  - Double-click claim → second click ignored.
  - Auto-claim during dismiss animation → ignored (timer killed in `HandleClaim`).
  - Back press during hide → ignored.
- **Dynamic instantiation** (no prefab, `AddComponent`): supported via lazy `Animator` getter.

## QA scenarios
Triggered from a Demo scene host MonoBehaviour via `[ContextMenu]`:

1. `Show — Coins +100` (default style).
2. `Show — Gems +5` (gem icon, distinct color).
3. `Show — Item "epic_sword"` (text-only with placeholder icon override).
4. `Show — Bundle (3 lines)`.
5. `Show — Auto-claim 3s` (do not tap, verify auto-fire).
6. `Show — Auto-claim 3s, tap at 1s` (verify timer cancelled, no double fire).
7. `Show — Empty DTO` (`Bind(null)`).
8. `Show — CloseOnBackdrop=true` (tap outside dismisses as claim).
9. `Stress — Spam claim 10x` (only first dismisses, single `OnClaimed`).
10. `Stress — Back press during hide` (no NRE, no double dismiss).

## Convenience helpers (deferred — Group C decision)

**Status**: OUT of Group B implementation. Spec-only sketch. Re-evaluated when Group C lands (Daily / LevelComplete / GameOver — three more reward-granting popups). If 4-of-4 reward callsites repeat the same 4-line wiring, helpers are implemented in Group C with capability-gate justification ("removes N lines of buyer boilerplate per reward callsite").

Proposed signatures (NOT shipped in Group B):

```csharp
// Runtime/Catalog/Popups/Reward/RewardFlow.cs
public static class RewardFlow
{
    // Single reward — shows a RewardPopup and credits the economy when the player claims.
    // The popup remains decoupled (MUSTN'T #1 preserved) — the helper is the host.
    public static void GrantAndShow(
        PopupManager popups,
        IEconomyService economy,
        CurrencyType currency,
        int amount,
        Action onClaimed = null);

    // Sequenced rewards — chains N RewardPopupData instances using OnDismissed → next.
    // Buyer use case: post-level reward + daily login + birthday chest in one call.
    // Resolves R4 ("stacked rewards = host shows N popups in sequence") without coupling popups.
    public static void GrantAndShowSequence(
        PopupManager popups,
        IEconomyService economy,
        IEnumerable<RewardPopupData> rewards,
        Action onAllClaimed = null);
}
```

**Why deferred**: `as user` checker (2026-05-02) flagged buyer boilerplate × N callsites. `as pm` checker counter-flagged: Group B has only 1 reward callsite (Demo). Capability-gate fails — implementing the helper now is process complexity, not problem complexity. Decision lives in Group C where 4 callsites materialize.

**Why documented now**: contract stability. The helper depends on `OnClaimed(CurrencyType, int)` and `OnDismissed` event signatures. By spec'ing both helper variants now (single + sequence) we lock the event surface so Group C implements without refactor.

**R4 reconfirmed**: `RewardPopup` itself stays single-shot (one DTO, one popup instance). Sequencing is a host concern — either buyer wires `OnDismissed → next.Show()` manually (Group B), or uses `RewardFlow.GrantAndShowSequence` (Group C+). The popup contract does NOT grow a `Queue<RewardPopupData>` field — that would couple the popup to a multi-DTO concept the kit's other popups don't share.

## Files
```
Runtime/Catalog/Popups/Reward/
├── RewardKind.cs
├── RewardPopupData.cs
├── RewardPopup.cs
└── UIAnimRewardPopup.cs
Tests/Editor/
└── RewardPopupTests.cs
```

## Status
- Code: ⏳ pending (Group B · element 1/5)
- Spec: ✅ this document
- Tests: ⏳ pending (target ~6)
- Prefab: ⏳ pending (`CatalogGroupABuilder.BuildRewardPopup`)
- Demo scene entry: ⏳ pending (`Catalog_GroupB_Demo` scene, `[ContextMenu]` triggers on `CatalogGroupBDemo`)
