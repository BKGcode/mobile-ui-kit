# Changelog

All notable changes to this package will be documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

_Last updated: 2026-05-04_

## [0.6.0-alpha] — 2026-05-04 BREAKING

> Group C — Progression catalog. Adds 5 catalog elements (DailyLogin / LevelComplete / GameOver popups + HUD-Energy / HUD-Timer), 1 of 3 planned helpers (`RewardFlow.GrantAndShowSequence` — sibling helpers deferred to Group D per capability-gate), `Build Group C Sample` builder + 5 prefabs + demo scene + 6 chain `[ContextMenu]` scenarios, 2 in-memory service stubs (`InMemoryProgressionService` + `InMemoryTimeService`), `IEconomyService` v1→v2 BREAKING migration with `CurrencyType.Energy` added, and `HUDCurrency` parameterized base replacing the `HUDCoins`/`HUDGems` sibling classes. **206 EditMode tests green** on fresh compile. Closes Milestone M1 of the path to `v1.0.0-rc`.

### Breaking changes — migration guide

**Block 1 — `IEconomyService` v1 → v2 method renames (sed/regex friendly):**

```diff
- economy.GetCoins()
+ economy.Get(CurrencyType.Coins)

- economy.GetGems()
+ economy.Get(CurrencyType.Gems)

- economy.SpendCoins(amount)
+ economy.Spend(CurrencyType.Coins, amount)

- economy.SpendGems(amount)
+ economy.Spend(CurrencyType.Gems, amount)

- economy.AddCoins(amount)
+ economy.Add(CurrencyType.Coins, amount)

- economy.AddGems(amount)
+ economy.Add(CurrencyType.Gems, amount)

- economy.OnCoinsChanged += handler
+ economy.OnChanged += (currency, value) => { if (currency == CurrencyType.Coins) handler(value); }

- economy.OnGemsChanged += handler
+ economy.OnChanged += (currency, value) => { if (currency == CurrencyType.Gems) handler(value); }
```

**Block 2 — Why no `[Obsolete]` shim.** `IEconomyService` v1 is removed cleanly — no transitional forwarders ship. Rationale: the parameterized API IS the contract; preserving v1 alongside v2 doubles the surface, masks the migration intent, and would bleed into v0.7+. Buyer pays the migration cost once at v0.6.0.

**Block 3 — Currency extension limit.** `CurrencyType` is a closed `public enum` with three sealed values (`Coins = 0`, `Gems = 1`, `Energy = 2`). Buyers cannot extend the enum from outside the package. Buyers with custom currencies (Tickets, Stars, Hearts) have three options: (a) fork the package and add enum values, (b) wait for an extension mechanism in a future version (no roadmap commitment), (c) reuse `Coins` / `Gems` / `Energy` as semantic aliases for their domain. A `(CurrencyType)42` cast at runtime returns 0 from `Get` / `CanAfford` and is a silent no-op for `Spend` / `Add` (no exception). Documented honestly so the buyer doesn't discover this in production.

**Block 4 — Prefab migration.** `HUDCoins.prefab` / `HUDGems.prefab` from the v0.5.0 Group B builder reference deleted classes (`HUDCoins`, `HUDGems`). After upgrade, the imported sample prefabs show "Missing Mono Script" in Inspector. Buyer steps:

1. Delete the Group B demo folder (`Assets/Catalog_GroupB_Demo/` if generated locally; `Assets/Samples/Kitforge Mobile UI Kit/0.5.0-alpha/` if imported via Package Manager).
2. Re-run `Tools/Kitforge/UI Kit/Build Group B Sample` (transitional — the Group B builder is updated in this release to produce `HUDCurrency`-based prefabs) OR `Tools/Kitforge/UI Kit/Build Group C Sample` (Phase B deliverable, ships with the v0.6.0-alpha tag).
3. Existing scenes referencing the old prefabs by GUID will break — buyer re-wires references manually (prefab GUIDs change because file paths change).

**Block 5 — Coupling: economy v2 + HUDCurrency are an inseparable migration.** v0.6.0-alpha bundles two breaking changes that cannot be reverted independently: (a) `IEconomyService` v1 → v2, and (b) `HUDCoins`/`HUDGems` classes → `HUDCurrency` parameterized base. A buyer who reverts (b) by restoring `HUDCoins.cs`/`HUDGems.cs` from git will get compile errors — the restored files reference v1 surface (`OnCoinsChanged`, `GetCoins`, ...) which v2 deleted. Reverting requires either reverting BOTH migrations together, or manually rewriting the restored HUDs to v2 (`OnChanged + filter`). Do NOT assume `git restore HUDCoins.cs` is enough.

### Added (Phase B — DailyLogin element 1/5)

- **`Runtime/Catalog/Popups/DailyLogin/`** — 5 files:
  - `DailyLoginPopupData.cs` — DTO with calendar-driven streak state. Fields: `Title` ("Daily Reward"), `RewardEntries: DailyLoginRewardEntry[]`, `CurrentDay` (1-based), `LastClaimUtc`, `AlreadyClaimedToday`, `DoubledToday` (FQ3), `MaxStreakGapDays` (1), `ClaimLabel` ("Claim"), `WatchToDoubleLabel` ("Watch ad to double"), `CloseOnBackdrop` (false). DTO defaults asserted in `DTO_Defaults_Match_Spec_Contract` test (FD3).
  - `DailyLoginRewardEntry.cs` — per-day calendar slot. `Rewards: RewardPopupData[]` (multi-reward day support per D8), `IsBigReward`, `AllowDouble`, `Label`.
  - `DailyLoginPopup.cs` — `UIModule<DailyLoginPopupData>`. Two CTAs (`Claim`, `Watch ad to double`) mutually exclusive per D5; back-press = silent dismiss; backdrop tap honors `CloseOnBackdrop`. Already-claimed state (D7) renders a HH:MM:SS countdown to UTC midnight, ticking via `Services.Time` once per `OnUpdate`. FQ1 auto-transition: when countdown crosses zero, popup re-queries `Services.Progression.GetDailyLoginState()` and flips to claim-ready inline (no close/re-open). FQ3: watch-to-double CTA stays visible in already-claimed state when `DoubledToday=false` and ad is ready. CurrentDay-beyond-RewardEntries.Length wraps modulo per D2/spec edge cases. Service binding follows § 4.5: `ITimeService` REQUIRED → null logs actionable LogError + aborts OnShow; `IAdsService` optional → null hides watch-to-double silently; `IProgressionService` optional (FQ1 auto-transition only) → null skips transition silently. Empty/null RewardEntries also aborts OnShow with actionable error.
  - `DailyLoginFlow.cs` — static helper `ShowIfDue(popups, services, configTemplate, onClaimed = null)` for D6 auto-trigger. Validates `PopupManager`/`IProgressionService`/`configTemplate` (null on any → § 4.5 LogError + return false). Queries `IProgressionService.GetDailyLoginState()`; returns false when `AlreadyClaimedToday=true`. Idempotent within app launch (FQ2): internal `_shownThisLaunch` flag prevents re-showing in same session even when due. Test helpers `ShownThisLaunchForTests` / `ResetForTests` / `ForceShownForTests` exposed via `internal` for unit coverage.
  - `UIAnimDailyLoginPopup.cs` — `: UIAnimPopupBase {}` shell (Group B precedent). Cell-cascade stagger animation deferred to builder/`_tween` polish session.
- **Test helpers** under `Tests/Editor/Helpers/`:
  - `FakeTimeService.cs` — `ITimeService` fake with `SetNow` / `Advance` setters. Default 2026-01-15 12:00 UTC.
  - `FakeAdsService.cs` — `IAdsService` fake with `SetRewardedReady` / `SetInterstitialReady` setters; `ShowRewardedAd` invokes callback synchronously with current ready state.
- **EditMode tests +26** under `Tests/Editor/`:
  - `DailyLoginPopupTests.cs` (19 tests) — Bind defaults / DTO contract (FD3) / Claim+Watch+Backdrop+Back paths / spam guard / already-claimed lockout / FQ1 auto-transition / FQ3 watch-double in already-claimed state / null-time service abort (§ 4.5) / empty-entries abort / countdown tick math / modulo wrapping.
  - `DailyLoginFlowTests.cs` (7 tests) — null-popups + null-services + null-progression + null-template § 4.5 paths / already-claimed short-circuit / FQ2 idempotency / test helper round-trip.
- **Spec update** — `Documentation~/Specs/Catalog/DailyLoginPopup.md`: `Services consumed` section renamed to `Service binding` and cites `CATALOG_GroupC_DELTA.md` § 4.5; QA scenarios 17-18 added covering null-required-service abort (popup) and null-progression return-false (helper).

### Added (Phase B — LevelComplete element 2/5)

- **`Runtime/Catalog/Popups/LevelComplete/`** — 3 files:
  - `LevelCompletePopupData.cs` — DTO with stars/score/best-score/CTA visibility flags and a `Rewards: RewardPopupData[]` array consumed by host via `RewardFlow.GrantAndShowSequence` post-dismiss (L3). Fields: `Title` ("Level Complete!"), `LevelLabel` (""), `Stars` (0, clamped `[0,3]` on Bind), `Score` (0), `BestScore` (0), `IsNewBest` (false), `Rewards` (null), `NextLabel` ("Next"), `RetryLabel` ("Retry"), `MainMenuLabel` ("Main Menu"), `ShowNext` (true), `ShowRetry` (true), `ShowMainMenu` (false), `CloseOnBackdrop` (false). Defaults asserted in `DTO_Defaults_Match_Spec_Contract` test (FD3).
  - `LevelCompletePopup.cs` — `UIModule<LevelCompletePopupData>`. Three CTAs (`Next`/`Retry`/`MainMenu`) gated by their `Show*` flags; back-press routes to retry per L7; backdrop-tap routes to retry when `CloseOnBackdrop=true`. `IsDismissing` guard on every CTA path. Star reveal via direct sprite swap from `Theme.StarFilledSprite`/`StarEmptySprite` (Theme slots from pre-flight); cascade tween + score rollup tween deferred to `_tween` polish session per Group B precedent. `IsNewBest=true` plays `UIAudioCue.Success` on Show. No required services — only optional `IUIAudioRouter`. Foot-gun guard: all-CTAs-hidden bind logs error and forces `ShowRetry=true` (mirrors NotEnoughCurrencyPopup pattern). Stars-out-of-range warning + clamp to `[0,3]`. Service binding follows § 4.5 (no required service to validate; LogError pattern unused but cited in spec).
  - `UIAnimLevelCompletePopup.cs` — `: UIAnimPopupBase {}` shell (Group B precedent). Star cascade + CTA stagger animation deferred to `_tween` polish session.
- **EditMode tests +16** under `Tests/Editor/LevelCompletePopupTests.cs` — Bind defaults / DTO contract (FD3) / Next+Retry+MainMenu+Backdrop+Back paths / per-CTA `Show*=false` lockout / spam guard / stars-clamp warnings (positive + negative) / all-CTAs-hidden foot-gun guard / event listener reset on re-Bind / event-arg DTO carries `Rewards` array intact for `RewardFlow.GrantAndShowSequence` integration.
- **Spec update** — `Documentation~/Specs/Catalog/LevelCompletePopup.md`: `Services consumed` section renamed to `Service binding` and cites `CATALOG_GroupC_DELTA.md` § 4.5.

### Added (Phase B — GameOver element 3/5)

- **`Runtime/Catalog/Popups/GameOver/`** — 5 files:
  - `GameOverPopupData.cs` — DTO for level-failure announcement with two-mode Continue (ad / currency / both) decoupled from ad / economy services. Fields: `Title` ("Game Over"), `Subtitle` (""), `Score` (-1, GO8 sentinel hides score block), `ContinueMode` (`Ad`), `ContinueCurrency` (`Gems`), `ContinueAmount` (5), `ContinuesUsedThisSession` (0), `MaxContinuesPerSession` (1), `ContinueAdLabel` ("Continue"), `ContinueCurrencyLabel` ("Continue ({amount})"), `RestartLabel` ("Restart"), `MainMenuLabel` ("Main Menu"), `ShowRestart` (true), `ShowMainMenu` (true), `BackPressBehavior` (`Restart`), `CloseOnBackdrop` (false). Defaults asserted in `DTO_Defaults_Match_Spec_Contract` test (FD3).
  - `ContinueMode.cs` — enum `None`/`Ad`/`Currency`/`AdOrCurrency` (GO1). Drives Continue CTA visibility; `AdOrCurrency` shows both side-by-side (player picks).
  - `BackPressBehavior.cs` — enum `Restart`/`MainMenu`/`Ignore` (GO7). Drives back-button routing.
  - `GameOverPopup.cs` — `UIModule<GameOverPopupData>`. Five CTAs (`ContinueAd` / `ContinueCurrency` / `Restart` / `MainMenu` / `Backdrop`) with strict service decoupling per GO4 — popup never calls `IEconomyService.Spend` nor `IAdsService.ShowRewardedAd`; host wires via emitted events. **Affordability gate (GO3) is a TWO-LAYER design**: (a) `IEconomyService=null` → currency button `interactable=false` (click is a no-op, matches spec edge case "Click emits no event"); (b) service present + can't afford → button stays interactable AND visually grayed via `image.color.a=0.5f`, click re-queries `CanAfford` and emits `OnContinueAffordCheckFailed(currency, amount)` WITHOUT dismissing — host wires to `ShopPopup` / `NotEnoughCurrencyPopup`. Click handler always re-queries `CanAfford` even when the Bind snapshot was true, covering external economy mutation between Bind and click. Ad readiness gated via `Button.interactable = !IsRewardedAdReady` set at Bind only (no re-check on click — host owns ad-failure recovery per FQ4). **Continue session limit** (`ContinuesUsedThisSession >= MaxContinuesPerSession`) hides BOTH Continue CTAs regardless of `ContinueMode` (GO2). **Foot-gun guard**: when Continue is hidden by limit AND `ShowRestart=false` AND `ShowMainMenu=false`, `Bind` logs error and forces `ShowMainMenu=true` (vs LevelComplete's `ShowRetry=true` — game-over biases toward exit, GO7 default). Score block hides silently when `Score < 0` (GO8). Header tint applies `Theme.FailureColor` (Group C pre-flight slot). Currency icon resolved from `Theme.IconCoin`/`IconGem`/`IconEnergy` by `ContinueCurrency`. `ContinueCurrencyLabel` placeholder `{amount}` replaced with `ContinueAmount.ToString()`. Service binding follows § 4.5 (Ads, Economy, Audio all OPTIONAL for GameOver — degrade visible+disabled, no abort). On Show plays `UIAudioCue.PopupOpen` then `UIAudioCue.Error` — no `Failure` cue exists in the enum, `Error` is the closest semantic match for the failure-feedback cue called for in spec; a dedicated `Failure` cue would expand cross-cutting audio surface beyond element-3 scope cap.
  - `UIAnimGameOverPopup.cs` — `: UIAnimPopupBase {}` shell (Group B precedent). CTA cascade stagger + Cinematic-preset Failure-color particle accents deferred to `_tween` polish session.
- **EditMode tests +18** under `Tests/Editor/GameOverPopupTests.cs` — Bind defaults / DTO contract (FD3) / ContinueAd happy path + spam guard / ContinueCurrency afford → success event + dismiss / ContinueCurrency NOT-afford → `OnContinueAffordCheckFailed` without dismiss (GO3 contract assertion) / ContinueCurrency with null `IEconomyService` → no-op (Bind sets interactable=false, handler also early-returns at `economy == null`) / Restart + MainMenu CTAs / `BackPressBehavior` Restart + MainMenu + Ignore branches (GO7) / session limit hides Continue (direct click invocations no-op) / `ContinueMode=None` hides Continue / foot-gun guard forces `ShowMainMenu=true` with error log assertion / Bind resets event listeners / backdrop click routes to `BackPressBehavior` when `CloseOnBackdrop=true`, ignored when false. Test target was ~16 from spec; delivered 18 covering the affordability matrix + back-press matrix + DTO defaults + backdrop edge cases. Visual-state setup scenarios (QA 1-5, 7) deferred to in-Editor demo verification — not testable in EditMode without prefab Refs assignments.
- **Spec citation** — `Documentation~/Specs/Catalog/GameOverPopup.md`: § 4.5 citation already applied in 2026-05-03 batch sweep; this delivery did not touch the spec.

### Added (Phase B — HUD-Energy element 4/5)

- **`Runtime/Catalog/HUD/HUDEnergy.cs`** extended from sealing-only stub to full element delivery. Adds `[Serializable] private struct EnergyRefs` foldout (`RegenCountdownLabel: TMP_Text`, `MaxCapLabel: TMP_Text`, `EnergyBarFill: Image` type Filled), serialized format strings (`_capFormatString = "/{0}"`, `_regenFormatString = "+1 in {0}"`, `_regenReadyText = "+1 ready"`), and at-max flash configuration (`_atMaxFlashEnabled` + `_atMaxFlashColor` defaulting to ≈SuccessColor + `_atMaxFlashDuration = 0.30f`). Subscribe override registers an auxiliary `HandleEconomyChangedForRegen` filtered to Energy that triggers `PollAndApplyRegenState()` on every economy event (spec E3 — "polled on every OnChanged event"); base.Subscribe still handles the value/punch via `HandleEconomyChanged`. 1Hz polling driven by a local `Update() → OnUpdate() → TickRegenPoll()` chain (workaround for `UIModuleBase.OnUpdate()` infra dispatch gap, anchored with literal `// OnUpdate-workaround-M3-sweep` per `/_checker` mejora F — M3 entry-criteria greps the anchor and sweeps atomically when infra dispatch lands). MaxCapLabel renders the `/{Max}` suffix only (CountLabel inherited from HUDCurrency carries Current); when `IsFull` or `Max <= 0` the suffix label is hidden via `SetActive(false)`. RegenCountdownLabel hidden when `IsFull`; shows `_regenReadyText` when `NextRegenUtc <= ITimeService.GetServerTimeUtc()`; otherwise `string.Format(_regenFormatString, "HH:MM:SS")` (manual TimeSpan format, DailyLogin precedent). EnergyBarFill `fillAmount = Mathf.Clamp01((float)Current / Max)`; hidden when `Max <= 0`. At-max flash (E7) fires when Current crosses up to Max via DOTween `Sequence().Append(label.DOColor(_atMaxFlashColor, half).SetEase(Ease.OutQuad)).Append(label.DOColor(_capLabelOriginalColor, half).SetEase(Ease.InQuad)).SetLink(gameObject)`, original color captured once on first OnEnable. Service binding follows § 4.5: `IProgressionService=null` → cap+regen+bar `SetActive(false)` silently (no LogError); `ITimeService=null` → regen label shows `_regenReadyText` (no countdown computation); `IEconomyService=null` → base `HUDCurrency.Refresh` logs actionable error and label shows `"--"`. Empty pulse (E8) and overcap tint deferred to M3 polish — both are nice-to-have feedback that don't block buyer demo, and overcap tint requires Theme access not currently wired into `HUDCurrency`.
- **EditMode tests +10** under `Tests/Editor/HUDEnergyTests.cs` — `Refresh_With_Service_Reads_Initial_Energy_State`, `Refresh_With_Null_ProgressionService_Hides_Energy_UI_Silently` (§ 4.5 silent-degrade contract — assert no LogError + all 3 energy UI elements `activeSelf=false`), `Cap_Label_Shows_Suffix_When_Bounded`, `Cap_Label_Hides_When_IsFull` (E4), `Cap_Label_Hides_When_Max_Is_Zero` (E4), `Regen_Label_Shows_Ready_Text_When_NextRegen_In_Past` (E5), `Regen_Label_Hides_When_IsFull` (E5), `EnergyBarFill_Sets_FillAmount_From_Current_Max` (3/5 → 0.6), `Sealed_To_Energy_Even_If_Inspector_Currency_Set_To_Coins` (E1 LOCKED Option A — ResolveCurrency seal proof), `OnChanged_Energy_Triggers_Regen_Poll_Refresh` (auxiliary handler proof — Max change picked up immediately, not on next 1Hz tick). Internal test API additions: `LastEnergyValueForTests`, `CapLabelTextForTests`, `CapLabelVisibleForTests`, `RegenLabelTextForTests`, `RegenLabelVisibleForTests`, `BarFillAmountForTests`, `BarFillVisibleForTests`, `SetEnergyRefsForTests`, `ForceRegenPollForTests`. Total tests: **189/189 verde sobre fresh compile** (179 baseline + 10 HUDEnergy).
- **Spec update** — `Documentation~/Specs/Catalog/HUD-Energy.md`: § Status flipped Code/Tests from `⏳ pending` to `✅ delivered 2026-05-03`; § E5 placeholder format corrected from `"{hh:mm:ss}"` to single-placeholder `"+1 in {0}"`; § E6 click behavior corrected (no rename — inherits agnostic `_onClickEvent` from FD4-cleaned base); § Files corrected (no separate `EnergyRegenState.cs` — struct lives inside `IProgressionService.cs`).

### Added (Phase B — HUD-Timer element 5/5)

- **`Runtime/Catalog/HUD/TimerMode.cs`** — `public enum TimerMode { CountdownToTarget = 0, CountupSinceTarget = 1, LocalStopwatch = 2 }`. Explicit values for stable serialization across upgrades.
- **`Runtime/Catalog/HUD/HUDTimer.cs`** — `[DisallowMultipleComponent] public sealed class HUDTimer : UIHUDBase`. Single class parameterized by `TimerMode`; mode is immutable post-OnEnable per FQ6 (buyer swaps modes via disable→assign→enable cycle, no `SetMode()` runtime helper). `[Serializable] private struct TimerRefs` foldout (`Label: TMP_Text`, optional `IconImage: Image`, optional `ClickButton: Button`). SerializeFields cover the entire surface: `_mode`, `_tickRateHz` (default 1f, buyer raises to 30f for stopwatch precision), `_formatString` (default `"mm\\:ss"`, validated in OnEnable with try/catch FormatException + LogError once + fallback), `_targetUtcIso` (optional ISO 8601 string parsed in OnEnable via `DateTime.TryParse` with `DateTimeStyles.AssumeUniversal | AdjustToUniversal`), `_expiredLabel` (default `"00:00"`), `_hideOnExpire` (default false), `_warningThresholdSeconds` (float, default 0 = disabled — TimeSpan can't be SerializeField), `_pausesWithTimeScale` (LocalStopwatch only; UTC modes ignore it), `_expiryStyle` (private enum `Success`/`Failure`, picks expiry flash color), `_warningColor`/`_expirySuccessColor`/`_expiryFailureColor` (defaults approximate Theme.WarningColor/SuccessColor/FailureColor — HUDTimer does NOT couple to `UIThemeConfig`, same precedent as HUDEnergy at-max flash decision), `_warningTweenDuration` (0.40s half-cycle), `_expiryFlashDuration` (0.30s in+out), `_onTimerClickEvent: UnityEvent`. Public C# events `OnExpired` and `OnWarningEntered` (cleared via `ResetTransientFlags` on `SetTarget(...)` and `Reset()` per spec § Events). Public methods `SetTarget(DateTime utc)` (resets expiry + warning state, ticks once force=true), `Reset()` (zeroes stopwatch state + clears events + ticks), `SetPaused(bool)` (LocalStopwatch only — early-returns for UTC modes; pause accumulates `_stopwatchAccumulated += CurrentRealTime() - _stopwatchStartRealTime`, unpause sets new start). `OnEnable` bypasses `base.OnEnable` when `Services == null && _mode == LocalStopwatch` (stopwatch doesn't need services — unique to HUDTimer; HUDCurrency/HUDEnergy always require Economy). 1Hz polling via local `Update() → OnUpdate() → tick rate throttle` chain (anchor `// OnUpdate-workaround-M3-sweep` per `/_checker` mejora F — same M3 sweep target as HUDEnergy + DailyLogin). Warning pulse: `_warningTween = label.DOColor(_labelOriginalColor, dur).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(gameObject)` — base color set to `_warningColor` on entry, restored to original on exit (re-target moves out of warning zone). Expiry flash: `_expiryTween = DOTween.Sequence().Append(label.DOColor(color, half)).Append(label.DOColor(originalColor, half)).SetLink(gameObject)` — kills warning tween first. Service binding follows § 4.5: `ITimeService = null` in UTC mode → label shows `"--:--"` SILENTLY, **drift fix vs older spec § Edge cases wording** (`"logs error"`); `IUIAudioRouter = null` → audio cues no-op. Stopwatch mode operates with zero services. Audio cues: warning enters `UIAudioCue.Error` (drift fix — `UIAudioCue.Failure` doesn't exist in enum, `Error` is closest semantic match per GameOver precedent); expiry plays `UIAudioCue.Success`. Internal `RealTimeProviderForTests: Func<float>` allows EditMode tests to inject a controllable clock without depending on `Time.unscaledTime` (prevents flaky tests). Production fallback: `_pausesWithTimeScale ? Time.time : Time.unscaledTime`.
- **EditMode tests +10** under `Tests/Editor/HUDTimerTests.cs` — `Countdown_Initial_Refresh_Computes_Remaining_From_Target` (5min target → `"05:00"`), `Countdown_Tick_Decrements_Remaining` (advance 30s → `"04:30"`), `Countdown_Expired_Fires_Event_Once_And_Stops_Ticking` (target in past → fireCount=1 + label=`"00:00"` + IsExpired=true; subsequent ticks within cycle do NOT re-fire), `Countdown_Warning_Threshold_Fires_Event_Once_When_Crossed` (target=now+15s, threshold=10s, advance 6s → fireCount=1; re-tick within zone does NOT re-fire), `Countdown_Hide_On_Expire_Hides_Label` (T5 — `_hideOnExpire=true`), `Countup_Initial_Refresh_Computes_Elapsed` (start=now-30s → `"00:30"`), `Stopwatch_OnEnable_Starts_From_Zero` (clock injected, format `"mm\\:ss\\.ff"` → `"00:00.00"`), `Stopwatch_SetPaused_Freezes_Counter` (clock at 0→5→pause→10→unpause→13 → label progression `00:05`→`00:05`→`00:08`), `Stopwatch_Reset_Sets_To_Zero` (advance to 10s, Reset → `"00:00"`), `Mode_UTC_Without_TimeService_Shows_Dashes_Silently` (§ 4.5 drift fix proof — `_services.SetTime(null)`, label = `"--:--"`, no LogError). Total tests: **199/199 verde sobre fresh compile** (189 baseline + 10 HUDTimer).
- **Spec update** — `Documentation~/Specs/Catalog/HUD-Timer.md`: § Status flipped Code/Tests from `⏳ pending` to `✅ delivered 2026-05-04`; § Edge cases UTC null-time wording aligned to § 4.5 silent (drift fix); § T6 warning tint clarified as `_warningColor` SerializeField (HUDTimer does NOT read Theme directly — consistency with HUDEnergy); § Service binding audio cue corrected from `Failure` to `Error` (drift fix — `UIAudioCue.Failure` does not exist).

### Added (Phase B — Helpers, capability-gate verdict 2026-05-04)

- **`Runtime/Catalog/Popups/Reward/RewardFlow.cs`** — `public static class RewardFlow` with the single shipped helper `GrantAndShowSequence(PopupManager popups, IEconomyService economy, IEnumerable<RewardPopupData> rewards, Action onSequenceComplete = null)`. Chains N `RewardPopup` instances via `OnDismissed → next.Show()` and credits `IEconomyService.Add(currency, amount)` on each non-sentinel claim. Item/Bundle rewards (sentinel `(CurrencyType)(-1)` per `RewardPopup.ItemCurrencySentinel`) surface via `OnClaimed` but are NOT auto-credited — host resolves them. Validation per `CATALOG_GroupC_DELTA.md` § 4.5 helper policy: null `popups` / null `economy` / null `rewards` / empty `rewards` → `Debug.LogError` with mandatory format + early no-op return. `onSequenceComplete` fires only after the LAST reward dismisses successfully — validation errors and queue-saturation early-aborts do NOT invoke it (contract is "all rewards claimed", not "sequence attempted"). Snapshots `IEnumerable` to `RewardPopupData[]` once on entry to insulate against buyer-side post-call mutation. Internal recursion via `ShowNext(index)` + `SubscribeChainHandlers` keeps each function ≤15 lines per UNITY_RULES. No static state cross-call (each invocation independent — distinct from sibling `DailyLoginFlow._shownThisLaunch` idempotent pattern).
- **EditMode tests +7** under `Tests/Editor/RewardFlowTests.cs` — 4 validation paths (null popups, null economy, null rewards, empty collection — each asserts LogError + `onSequenceComplete` does NOT fire) + 3 happy paths (single Coins reward credits + completes / 3 mixed rewards chain crediting Coins+Gems+Coins=150+5 then completes once / 3 mixed including Item + Bundle sentinels filters correctly crediting only the Coins). Test infra uses real `PopupManager` + `RewardPopup` prefab + reflection on private `_popupPrefabs`/`_popupRoot` SerializeFields (Group B PopupManagerTests precedent) + `PreWarmRewardPopupWithNullAnimator` to inject NullAnimator into the cached popup instance before the helper invokes it (UIAnimRewardPopup short-circuits on `_card == null` so this is defensive — both real animator + NullAnimator complete the chain synchronously in EditMode).
- **`Tests/Editor/LevelCompletePopupTests.Event_Data_Carries_Original_Rewards_For_Sequence_Helper`** migrated per `/_checker` mejora H — renamed to `Event_Data_Carries_Original_Rewards_And_RewardFlow_Credits_Economy_Per_Reward`. Original assertion (`Assert.AreSame(rewards, d.Rewards)` for L3 array carry-through contract) preserved; test now ALSO sets up local `PopupManager` + `RewardPopup` prefab + `FakeEconomyService` and invokes `RewardFlow.GrantAndShowSequence(manager, economy, d.Rewards, () => sequenceCompleteCount++)` from inside the `OnNextRequested` handler. Two `dummy.OnBackPressed()` calls drive the chain through both rewards (Coins=100 + Gems=5). Asserts economy credited correctly + `sequenceCompleteCount == 1`. Closes the "test verifies intent but not integration" gap flagged in the post-GameOver triple `/_checker` round.
- **Capability-gate verdict (Group C 2026-05-04)** — sibling helpers `RewardFlow.GrantAndShow` (single) and `ShopFlow.OpenWithPurchaseChain` deferred to Group D. Counted callsites: `GrantAndShow` single = 1 (DailyLogin OnWatchAdRequested 2× amount; host can use `GrantAndShowSequence` with single-element collection in the meantime), `ShopFlow` chain = 1 (Group B demo trigger; GameOver does NOT use this chain — its continues are direct Ad/Currency, not Shop-routed). Spec preserved in `RewardPopup.md` and `ShopPopup.md` § Convenience helpers — contract-stable, no event surface churn at Group D build time.
- **Spec updates** — `Documentation~/Specs/Catalog/RewardPopup.md` § Convenience helpers and `Documentation~/Specs/Catalog/ShopPopup.md` § Convenience helpers: status flipped from "deferred — Group C decision" to capability-gate verdict (Sequence shipped, single + Shop deferred Group D).

### Added (B.4.0 — sample stub infrastructure prep, NOT tag-ready)

> **Intermediate progress entry.** B.4.0 ships ONLY the in-memory progression stub + sample asmdef. No prefabs, no builder, no demo scene. **`v0.6.0-alpha` is NOT tag-ready** until B.4.1 (builder skeleton) → B.4.2-B.4.5 (popup/HUD prefab generators) → B.4.6 (demo scene + chain demo MonoBehaviour) all land. `package.json` `samples[]` entry deliberately NOT added in B.4.0 — adding it now would expose a half-finished sample to buyers via Package Manager UI (`/_checker` 2026-05-04 mejora F1: shipping the entry premature breaks the buyer-facing completeness expectation set by Group A and Group B). The samples[] entry lands at B.4.6 closure together with the buyer-facing demo. Done criteria checkbox for the helper line above is the LAST flip before tag — all subsequent B.4 sub-steps must land before flipping it.

- **`Samples~/Catalog_GroupC_Progression/Stubs/InMemoryProgressionService.cs`** — `[DisallowMultipleComponent] [DefaultExecutionOrder(-100)]` (Awake-before-OnEnable per L4 lesson) `MonoBehaviour, IProgressionService`. Inspector-editable `_dailyLoginState` and `_energyRegenState` SerializeFields with documented defaults: `_dailyLoginState.LastClaimUtc = default` (DateTime.MinValue per `/_checker` mejora B — DailyLogin auto-trigger fires on buyer's first Play because today != MinValue.Date), `_dailyLoginState.CurrentDay = 1` / `MaxStreakGapDays = 1`, `_energyRegenState.Current = Max = 5` / `IsFull = true` / `NextRegenUtc = default` ("fresh first run"). `_regenSeconds = 60f` Inspector-tunable. `_levels` seeded with 5 LevelData entries (Id 0 Available, Ids 1-4 Locked) on first Awake. Economy injection via `[SerializeField] MonoBehaviour _economyServiceRef` cast to `IEconomyService` in Awake — no cross-sample `RequireComponent` dependency (buyer can wire any `IEconomyService` impl, not only Group B's `InMemoryEconomyService`). Public `SetEconomyForTests(IEconomyService)` for test/builder direct injection. `Update()` ticks regen via `DateTime.UtcNow` (not via `ITimeService` — keeps stub self-contained, mirrors mobile-game convention; buyer can fork stub for server time): when not full and `UtcNow >= NextRegenUtc` → `Current++` (capped at Max), `IsFull` recomputed, `NextRegenUtc = UtcNow + _regenSeconds` (or `default` if full), then `_economy?.Add(CurrencyType.Energy, 1)` so HUD-Energy's `IEconomyService.OnChanged(Energy, _)` subscription stays in sync without extra wiring. `GetDailyLoginState()` returns a struct copy with `AlreadyClaimedToday` computed at call time (`LastClaimUtc.Date == DateTime.UtcNow.Date`) — the SerializeField stores the raw timestamp, the computed bool is never written back. Public state-mutation API for buyer host code: `MarkDailyLoginClaimed()` (sets `LastClaimUtc=UtcNow` + increments `CurrentDay` + clears `DoubledToday`), `MarkDailyLoginDoubled()`, `SetEnergyValue(int)` (clamped 0..Max with NextRegenUtc recomputed). Five `[ContextMenu]` debug triggers per `CATALOG_GroupC_DELTA.md` § 2 acceptance: "Reset Daily To Day 1", "Set Daily To Day 7 Ready", "Mark Today Claimed", "Set Energy To Zero", "Refill Energy". `IProgressionService.CompleteLevel` clamps stars to `[0,3]`; `UnlockLevel` returns false if already unlocked. All public functions ≤15 lines per UNITY_RULES.
- **`Samples~/Catalog_GroupC_Progression/KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.asmdef`** — sample-side asmdef mirroring Group B precedent (`KitforgeLabs.MobileUIKit.Samples.CatalogGroupB`). References: `KitforgeLabs.MobileUIKit`, `KitforgeLabs.MobileUIKit.Catalog`, `Unity.TextMeshPro`. Asmdef name LOCKED per `/_checker` mejora F3 — B.4.1+ builder will resolve the stub via `System.Type.GetType("...InMemoryProgressionService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC")` reflection (Group B precedent). Naming consistency keeps the future builder code symmetrical to `CatalogGroupBBuilder.TryAttachStubServices`.

### Added (B.4.1 — builder skeleton, NOT tag-ready)

> **Intermediate progress entry.** B.4.1 ships the `CatalogGroupCBuilder` static class scaffold with the `Tools/Kitforge/UI Kit/Build Group C Sample` MenuItem wired and `EnsureFolders` creating the `Assets/Catalog_GroupC_Demo/Prefabs/` output tree. **No prefabs are generated yet** — `BuildAll` only logs a "skeleton ready" diagnostic and exits. **`v0.6.0-alpha` is NOT tag-ready** until B.4.2-B.4.5 (per-element `BuildXxx` generators) + B.4.6 (demo scene + chain demo + `samples[]` entry) all land. `package.json` `samples[]` entry deliberately NOT added in this sub-step — same rationale as B.4.0 (premature buyer-facing exposure of a half-finished sample).

- **`Editor/Generators/CatalogGroupCBuilder.cs`** (~140 lines) — `static class CatalogGroupCBuilder` mirroring `CatalogGroupBBuilder` precedent. Includes:
  - 3 const strings: `OutputRoot = "Assets/Catalog_GroupC_Demo"`, `PrefabsFolder = OutputRoot + "/Prefabs"`, and `ProgressionServiceTypeName` (LOCKED reflection target string for B.4.6: `"KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryProgressionService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC"` — typo-proofs the future `Type.GetType` call against the asmdef name LOCKED in B.4.0).
  - 5 color tokens used by the helpers (`BackdropColor`, `CardColor`, `ButtonPrimaryColor`, `ButtonSecondaryColor`, `TextDarkColor`) — additional palette tokens (Cell / HeaderTint / Success / HUDBackground / TextLight / Failure) deferred to the specific `BuildXxx` sub-step that first needs them, per scope-cap discipline (avoids `CS0414` unused-readonly warnings + keeps the file growing in lockstep with capability delivery).
  - 8 layout helpers + 3 transitive deps copied verbatim from `CatalogGroupBBuilder` (decision: copy-paste — see "Helper-sharing strategy" below): `CreateRoot`, `CreateBackdrop`, `CreateCard`, `CreateChild`, `AddImage`, `CreateText`, `CreateButton`, `CreatePrimaryButton`, `CreateSecondaryButton`, `StretchInside`, `AnchorTopOfCard`. Prefab-saving + animator-wiring helpers (`SaveAsPrefab`, `WireAnimatorCard`, `ForceButtonHeight`) deferred to the specific sub-step that first needs them.
  - `[MenuItem("Tools/Kitforge/UI Kit/Build Group C Sample")] public static void BuildAll()` body: `EnsureFolders(); Debug.Log(skeleton-ready diagnostic that cites the locked reflection target).` No prefab generation, no scene generation, no service stub attachment. Output the user can verify: a console log line + the empty `Assets/Catalog_GroupC_Demo/Prefabs/` folder appearing in the Project view.
- **Helper-sharing strategy decision** (drift checker `/_checker drift` 2026-05-04, after evaluating 3 options): **copy-paste during M1-M3**, extract `CatalogBuilderBase` at M4 hardening sweep. Rationale: composability criterion (philosophy #5) requires 2+ consumers PROVEN, not anticipated. Touching `CatalogGroupBBuilder.cs` (taggeado en `v0.5.0-alpha`) for an extraction now opens BREAKING blast radius post-tag and falls outside the B.4.1 cap. M4 already has "builder destination overwrite/skip prompt" hotfix scheduled — extraction lands in the same sweep when 4 builders (A/B/C/D) exist. Documented in roadmap "Diferidos / próximos hotfixes".

### Added (B.4.2 — DailyLoginPopup prefab generator, NOT tag-ready)

> **Intermediate progress entry.** B.4.2 ships the `BuildDailyLoginPopup` private generator + `DailyLoginPopup.prefab` materialized at `Assets/Catalog_GroupC_Demo/Prefabs/DailyLoginPopup.prefab`. **`v0.6.0-alpha` is NOT tag-ready** until B.4.3 (LevelComplete) → B.4.4 (GameOver) → B.4.5 (HUD-Energy + HUD-Timer) → B.4.6 (DemoScene + chain demo + `samples[]` entry) all land.

- **`Editor/Generators/CatalogGroupCBuilder.cs`** — `BuildDailyLoginPopup()` private static added (~70 LOC mirroring Group B `BuildRewardPopup` pattern). Generates a 900×1200 card with: title (TMP, top-anchored), 4-column `GridLayoutGroup` `DayCellContainer` (cell template authoring is buyer/runtime concern per spec — builder ships the empty container only), `CountdownLabel` (TMP, hidden by default — popup `ApplyVisibility` toggles based on `AlreadyClaimedToday`; color = local `WarningColor` orange, buyer can attach `ThemedText` for theme override), primary `ClaimButton` (`SuccessTintColor` green tint + white "Claim" label), secondary `WatchToDoubleButton` (gray + dark "Watch ad to double" label, hidden by default — popup activates when `ShouldShowWatchToDouble()` evaluates `AllowDouble` + `IsRewardedAdReady` + `!DoubledToday`). All 8 `_refs` wired via `SerializedObject.FindProperty("_refs.XxxLabel/Button")` matching `DailyLoginPopup.Refs` private struct.
- **Animator wiring**: `root.AddComponent<UIAnimDailyLoginPopup>()` first (mirroring Group B AddComponent ordering), then `root.AddComponent<DailyLoginPopup>()` (auto-adds animator via `[RequireComponent]` if absent — explicit-first prevents the redundant fallback path), then `WireAnimatorCard(animator, card)` sets `_card` SerializeField. `UIAnimDailyLoginPopup` is the v0.5.0 Group B precedent stub `: UIAnimPopupBase {}`.
- **`BuildAll` updated**: now loads `UIThemeConfig_Default.asset` + opens "Bootstrap Defaults missing" dialog if absent (mirroring Group B), calls `BuildDailyLoginPopup()` + `AssetDatabase.SaveAssets()` + `Refresh()`, logs "Group C — 1/5 prefabs generated. Pending: B.4.3 / B.4.4 / B.4.5 / B.4.6".
- **Constants/colors added incrementally per cap-strict discipline**: `DailyLoginPath` const + `DefaultThemePath` const + `SuccessTintColor` + `WarningColor` + `TextLightColor` static readonly. Anticipatory tokens for B.4.3-B.4.5 (LevelComplete star colors, GameOver header tint, HUD background) still deferred — added when each `BuildXxx` first uses them. Helpers `SaveAsPrefab<T>` + `WireAnimatorCard` added (composability criterion now satisfied — 4 future consumers in B.4.3-B.4.5 will reuse).
- **Usings added**: `KitforgeLabs.MobileUIKit.Catalog.DailyLogin` + `KitforgeLabs.MobileUIKit.Theme`.

### Added (B.4.3 — LevelCompletePopup prefab generator, NOT tag-ready)

> **Intermediate progress entry.** B.4.3 ships the `BuildLevelCompletePopup` private generator + `LevelCompletePopup.prefab` materialized at `Assets/Catalog_GroupC_Demo/Prefabs/LevelCompletePopup.prefab`. **`v0.6.0-alpha` is NOT tag-ready** until B.4.4 (GameOver) → B.4.5 (HUD-Energy + HUD-Timer) → B.4.6 (DemoScene + chain demo + `samples[]` entry) all land.

- **`Editor/Generators/CatalogGroupCBuilder.cs`** — `BuildLevelCompletePopup()` private static added (~120 LOC). Generates a 900×1300 card with: title (TMP "Level Complete!" 48pt, top-anchored), optional `LevelLabel` subtitle (28pt italic, hidden when empty per popup `ApplyTexts`), `StarsRow` (`HorizontalLayoutGroup` with 3 child `Image` slots 160×160 — initial sprite = `Theme.StarEmptySprite` if Theme present; runtime overrides via `ApplyStars`), `ScoreLabel` (64pt bold center), `BestScoreLabel` (26pt italic gray below score), `NewBestBanner` (full-width strip with `SuccessTintColor` background + white "NEW BEST!" label, hidden by default — popup `ApplyVisibility` toggles via `IsNewBest`), `Buttons` container with `VerticalLayoutGroup` holding 3 stacked CTAs: `NextButton` (primary blue, "Next" label, 96h), `RetryButton` (secondary gray, "Retry" label, 92h), `MainMenuButton` (secondary gray, "Main Menu" label, 80h, hidden by default per `ShowMainMenu=false`). All 13 `_refs` wired via `SerializedObject.FindProperty`, including the `StarImages` array via `arraySize=3` + per-index `objectReferenceValue` assignment.
- **Animator wiring**: `root.AddComponent<UIAnimLevelCompletePopup>()` first, then `root.AddComponent<LevelCompletePopup>()`, then `WireAnimatorCard(animator, card)`. `UIAnimLevelCompletePopup` is the v0.5.0 Group B precedent stub `: UIAnimPopupBase {}` (Group C precedent for stub-only animator scripts).
- **`BuildAll` updated**: `BuildLevelCompletePopup()` call added after `BuildDailyLoginPopup()`. Log message updated to "2/5 prefabs generated. Pending: B.4.4 / B.4.5 / B.4.6".
- **Constants added incrementally per cap-strict discipline**: `LevelCompletePath` const. No new color tokens needed (existing `SuccessTintColor` covers banner; `ButtonPrimaryColor`/`ButtonSecondaryColor`/`TextDarkColor`/`TextLightColor` cover CTAs; star colors come from `Theme.StarFilledSprite`/`StarEmptySprite` at runtime). Helper `ForceButtonHeight` added (Group B precedent — 3 future consumers in B.4.4-B.4.5 will reuse for stacked CTA layouts).
- **Usings added**: `KitforgeLabs.MobileUIKit.Catalog.LevelComplete`.

### Added (B.4.4 — GameOverPopup prefab generator, NOT tag-ready)

> **Intermediate progress entry.** B.4.4 ships the `BuildGameOverPopup` private generator + `GameOverPopup.prefab` materialized at `Assets/Catalog_GroupC_Demo/Prefabs/GameOverPopup.prefab`. **`v0.6.0-alpha` is NOT tag-ready** until B.4.5 (HUD-Energy + HUD-Timer) → B.4.6 (DemoScene + chain demo + `samples[]` entry) all land.

- **`Editor/Generators/CatalogGroupCBuilder.cs`** — `BuildGameOverPopup()` private static added (~130 LOC). Generates a 900×1300 card with: `HeaderTint` strip (full-width 14h band at top with `FailureColor` placeholder — popup `ApplyTint` overwrites runtime via `Theme.FailureColor` at Bind time), title (TMP "Game Over" 48pt bold), optional `Subtitle` (28pt italic, hidden when DTO Subtitle empty per `ApplySubtitle`), `ScoreBlock` GameObject wrapping `ScoreLabel` (64pt bold, popup hides block when `Score < 0` per GO8), `Buttons` container with `VerticalLayoutGroup` holding 4 stacked CTAs: `ContinueAdButton` (primary blue, "Continue" white 30pt, 96h), `ContinueCurrencyButton` (secondary gray, internal absolute-positioned `CurrencyIcon` Image + "Continue (5)" label — popup `ApplyIcon` resolves `Theme.IconCoin/Gem/Energy` runtime by `_data.ContinueCurrency`; popup `ApplyContinueCurrencyGating` sets alpha 0.5 when can't afford), `RestartButton` (secondary gray, "Restart" 28pt, 92h), `MainMenuButton` (secondary gray, "Main Menu" 26pt, 80h). All 15 `_refs` wired via `SerializedObject.FindProperty` including `HeaderTint` and `ScoreBlock`.
- **Animator wiring**: `root.AddComponent<UIAnimGameOverPopup>()` first, then `root.AddComponent<GameOverPopup>()`, then `WireAnimatorCard`. `UIAnimGameOverPopup` is the v0.5.0 Group B precedent stub `: UIAnimPopupBase {}`.
- **`BuildAll` updated**: `BuildGameOverPopup()` call added after `BuildLevelCompletePopup()`. Log message updated to "3/5 prefabs generated. Pending: B.4.5 / B.4.6".
- **Constants added incrementally per cap-strict discipline**: `GameOverPath` const + `FailureColor` static readonly (`#E5392B` mirroring `Theme.FailureColor` default for placeholder fidelity in Prefab Mode; popup runtime overwrites). No new helpers needed (all existing helpers reused).
- **Usings added**: `KitforgeLabs.MobileUIKit.Catalog.GameOver`.
- **Theme runtime hook validation**: `BuildGameOverPopup` deliberately leaves `HeaderTint.color`, `ContinueCurrencyIcon.sprite`, and `ContinueCurrencyLabel.text` at placeholder values — popup `ApplyTint`/`ApplyIcon`/`ApplyTexts` overwrite at every `Bind` call from current `Theme` and DTO. This is the **canonical pattern for live-Theme reskin** (contrast with `BuildLevelCompletePopup.NewBestBanner` which hardcodes `SuccessTintColor` build-time — captured as M3 polish deuda in `kitforge_mobile_ui_kit_roadmap.md` § Diferidos).

### Added (B.4.5 — HUDEnergy + HUDTimer prefab generators, NOT tag-ready)

> **Intermediate progress entry.** B.4.5 ships `BuildHUDEnergy()` + `BuildHUDTimer()` — the final 2 of the 5 Group C catalog prefabs. Materializes `Assets/Catalog_GroupC_Demo/Prefabs/HUDEnergy.prefab` and `Assets/Catalog_GroupC_Demo/Prefabs/HUDTimer.prefab`. **All 5 catalog prefabs now generated by the builder** (DailyLogin / LevelComplete / GameOver popups + HUDEnergy + HUDTimer HUDs). **`v0.6.0-alpha` is NOT tag-ready** until B.4.6 (DemoScene + chain demo `CatalogGroupCDemo` MonoBehaviour wiring `UIServices` + 5 prefabs + `[ContextMenu]` triggers per mejora I + `package.json` `samples[]` entry) lands.

- **`Editor/Generators/CatalogGroupCBuilder.cs`** — Two new private static functions added (~140 LOC combined):
  - `BuildHUDEnergy()` (~70 LOC) — generates a 280×100 horizontal HUD shell: `HUDBackgroundColor` semi-transparent panel as Button targetGraphic, left-anchored Icon Image (64×64, sprite = `Theme.IconEnergy` if available), `CountLabel` ("3" 32pt white bold) + `MaxCapLabel` ("/5" 24pt gray bold) inline at top, `RegenCountdownLabel` ("+1 in 04:30" 16pt `WarningColor` orange) at bottom-left, `EnergyBarFill` Image (Filled/Horizontal type, `SuccessTintColor` green, fillAmount=0.6f preview) as thin 8h strip at bottom edge. Wires 7 SerializedObject refs: `_currency=Energy` (informational — `HUDEnergy.ResolveCurrency()` seals via virtual override regardless), `_refs.IconImage`, `_refs.CountLabel`, `_refs.ClickButton`, `_energyRefs.RegenCountdownLabel`, `_energyRefs.MaxCapLabel`, `_energyRefs.EnergyBarFill`. **No `_services` wiring at build time** — buyer or `BuildDemoScene` (B.4.6) attaches `UIServices` reference at runtime via `WireHUDServices` precedent.
  - `BuildHUDTimer()` (~50 LOC) — generates a 220×64 horizontal HUD shell: `HUDBackgroundColor` panel as Button targetGraphic, left-anchored Icon Image (40×40, sprite = `Theme.IconClock` if available), `Label` ("00:00" 36pt white bold) stretched right of icon. Wires 3 SerializedObject refs: `_refs.Label`, `_refs.IconImage`, `_refs.ClickButton`. Mode/format/expiry SerializeFields keep their script-level defaults (`_mode=CountdownToTarget`, `_formatString="mm\\:ss"`, `_targetUtcIso=""`) — buyer configures per-instance via Inspector or `SetTarget(DateTime)` runtime API.
- **`BuildAll` updated**: added `BuildHUDEnergy()` + `BuildHUDTimer()` calls after `BuildGameOverPopup()`. Log message updated to **"5/5 prefabs generated. Pending: B.4.6 (DemoScene + chain demo + samples[] entry)"** — final builder closure milestone.
- **Constants added incrementally per cap-strict discipline**: `HUDEnergyPath` const + `HUDTimerPath` const + `HUDBackgroundColor` static readonly (`#00000073` semi-transparent black, mirrors Group B precedent for HUD shells) + `HUDCapLabelColor` static readonly (`#D9D9E5` light gray for "/5" suffix readability against background). No new helpers needed (HUDs are simpler than popups — no card, no backdrop, no animator, just RectTransform + Image bg + Button + child labels/icon).
- **Usings added**: `KitforgeLabs.MobileUIKit.Catalog.HUD` (for `HUDEnergy`/`HUDTimer` types) + `KitforgeLabs.MobileUIKit.Services` (for `CurrencyType.Energy` enum reference in `_currency` SerializedObject assignment).
- **HUDs do NOT have `IUIAnimator` components** (per spec — HUDs are persistent UI without Show/Hide lifecycle). Builder consequently skips `WireAnimatorCard` for these two prefabs. Inline tweens for at-max flash, empty pulse, warning pulse, expiry flash are stored in HUD-side `Tween` fields (`HUDEnergy._capFlashTween`, `HUDTimer._warningTween`/`_expiryTween`) with `SetLink(gameObject)` per UNITY_RULES.

### Added (B.4.6 — Demo scene + chain demo + samples[] entry → tag-ready)

> **Tag-ready closure.** B.4.6 ships the buyer-facing closure of Group C: `BuildDemoScene` materializes `Catalog_GroupC_Demo.unity` with Canvas + EventSystem + UIServices container + 5 wired in-memory stubs (Economy / Shop / Ads from Group B asmdef + Time / Progression from Group C asmdef via reflection) + 2 HUD instances + Demo MonoBehaviour wired to all prefabs. `CatalogGroupCDemo.cs` host script ships with 6 chain `[ContextMenu]` scenarios per mejora I (`/_checker as user/pm/dev` 2026-05-03 GameOver close) + 9 individual element triggers + multi-timer reset per HUD-Timer QA scenario 11. `package.json` `samples[]` entry added making the Group C sample discoverable in Package Manager UI. CHANGELOG done-criteria checkboxes flipped — **only `v0.6.0-alpha` BREAKING tag remains** (B.5: bump version + README install URL + fresh-compile final verification + `git tag`).

- **`Samples~/Catalog_GroupC_Progression/Stubs/InMemoryTimeService.cs`** (NEW, ~30 LOC) — `[DisallowMultipleComponent] [DefaultExecutionOrder(-100)]` `MonoBehaviour, ITimeService`. Returns `DateTime.UtcNow + _offsetSeconds`. `_offsetSeconds` is Inspector-editable for QA testing (skip 1 hour / 1 day / reset ContextMenu helpers). Wired into UIServices' `_timeServiceRef` field by `TryAttachStubServices`. Required dependency for `DailyLoginPopup` (countdown + day-boundary computation) and `HUDTimer` (UTC modes). Discovered as a gap by pre-flight finding ("InMemoryTimeService NOT exists in Samples~/") — included in B.4.6 cap per Option A approval.
- **`Samples~/Catalog_GroupC_Progression/CatalogGroupCDemo.cs`** (NEW, ~280 LOC) — host MonoBehaviour with 9 `[SerializeField]` refs (`_popupParent` RectTransform, `_theme` UIThemeConfig, `_services` UIServices, 3 popup prefabs + RewardPopup prefab from Group B + 2 HUD scene instances). Mirrors `CatalogGroupBDemo` precedent: instantiates popups via `SpawnPopup<T>` helper (Instantiate + GetComponent + Initialize(theme, services)), subscribes to popup events, dismisses by destroying the GameObject on `OnDismissed`. Includes:
  - **`Start()` initializer** (post-checker C1 fix 2026-05-04): seeds `_hudTimer.SetTarget(DateTime.UtcNow.AddMinutes(5))` so first-Play shows a live 5-minute countdown instead of the default `_targetUtc = MinValue` that would expire immediately and display "00:00" out-of-the-box. Buyer's first impression is a working ticking timer.
  - **9 individual ContextMenu triggers**: DailyLogin (day 1 fresh / day 5 watch-to-double / already claimed); LevelComplete (3 stars new best / last level Next-hidden); GameOver (Ad / Currency / AdOrCurrency).
  - **6 HUD ContextMenu triggers**: Add 1 energy / Spend 1 energy / Empty energy (set to 0); Set timer 5min / Set timer 10s warning zone / Set timer 3min countdown (with multi-timer setup hint).
  - **5 chain ContextMenu scenarios per mejora I + honest categorization**: LevelComplete → Reward sequence (3 chained rewards via `OnDismissed`-based recursion in `SpawnNextRewardInSequence`); GameOver Continue Ad (subscribes `OnContinueWithAdRequested` → `_services.Ads.ShowRewardedAd`); GameOver Continue Currency (subscribes → `_services.Economy.Spend` + log); DailyLogin auto-trigger (manual SpawnDailyLogin since the MonoBehaviour-less `DailyLoginFlow.ShowIfDue` requires `PopupManager` which the demo deliberately doesn't use, mirroring Group B's `Instantiate`-based pattern); Energy regen tick (manual +1 add, HUD reflects via `OnChanged` subscription). **Sixth mejora I scenario (multi-timer) reclassified** post-checker C2 fix as a HUD ContextMenu helper rather than a Chain scenario — it sets a single timer plus logs the manual-instantiation guidance for QA scenario 11; "chain" is reserved for true popup → event → another-popup flows. The 6 mejora I scenarios are still all addressable; only the categorization changed for honest naming.
  - **Watch-to-double flow** (`HandleAdThenDoubledRewards`): subscribes to `OnWatchAdRequested(day, rewards)` → calls `_services.Ads.ShowRewardedAd` → on success, doubles first reward amount per spec D5 → spawns single doubled reward via `SpawnRewardSequence`.
  - **`HandleRewardClaimed`** filters out `RewardPopup.ItemCurrencySentinel` (Item/Bundle kinds) before crediting economy, matching Group B precedent.
- **`Editor/Generators/CatalogGroupCBuilder.cs`** — Three new private static functions added (~210 LOC combined):
  - `BuildDemoScene()` — creates new empty scene, loads 5 Group C prefabs + Group B `RewardPopup` prefab + Theme, builds Canvas (ScreenSpaceOverlay, ScaleWithScreenSize 1080×1920, matchWidthOrHeight 0.5), `Popups` child (full stretch) + `HUD` child (top-right anchored 300×240 with `VerticalLayoutGroup`), `EventSystem` GameObject, `UIServices` GameObject, calls `TryAttachStubServices(servicesGO, services)`, instantiates HUDEnergy + HUDTimer prefabs into HUD parent and calls `WireHUDServices` on each, instantiates Demo GameObject with `CatalogGroupCDemo` component (resolved via reflection against `DemoMonoBehaviourTypeName` — `LogWarning` if Group C sample asmdef not compiled), wires all 9 SerializedObject fields on Demo, saves scene to `Catalog_GroupC_Demo/Catalog_GroupC_Demo.unity`. Logs `LogError` for missing critical prefabs (theme + 5 Group C prefabs), `LogWarning` for missing optional `RewardPopup` prefab (chain partial without it but other scenarios still work).
  - `TryAttachStubServices(host, services)` — reflects against TWO asmdefs (`GroupBStubAsmdef` for Economy/Shop/Ads + `GroupCStubAsmdef` for Time/Progression). Adds 5 stub MonoBehaviours to the UIServices host GameObject, wires `InMemoryProgressionService._economyServiceRef` to the Economy stub instance (so regen tick can call `_economy.Add(Energy, 1)`), then wires all 5 service refs on UIServices (`_economyServiceRef`, `_shopDataProviderRef`, `_adsServiceRef`, `_timeServiceRef`, `_progressionServiceRef`). Logs separate warnings per missing asmdef so buyer knows whether Group B or Group C sample needs importing.
  - `WireHUDServices(hud, services)` — sets `_services` SerializedObject field on HUD instance (mirrors Group B precedent verbatim).
- **`BuildAll` updated**: now calls `BuildDemoScene()` after `SaveAssets()` + `Refresh()` (mirroring Group B precedent — scene generation requires prefabs already saved on disk for `LoadAssetAtPath`). Final user-facing dialog replaces the prior diagnostic Debug.Log: "Group C built at Assets/Catalog_GroupC_Demo. 5 prefabs + 1 scene generated. Open the scene, press Play, right-click the Demo GameObject and pick a Context Menu scenario (try 'Chain — LevelComplete → Reward sequence (3 rewards)' to see the post-level chain)." Same UX pattern as `Build Group B Sample`.
- **Constants added incrementally**: `ScenePath` const + `GroupBRewardPath` const + `GroupBStubAsmdef` const + `GroupCStubAsmdef` const + `TimeServiceTypeName` const + `DemoMonoBehaviourTypeName` const. The 4 reflection-target consts typo-proof every `Type.GetType` call against the exact namespace + asmdef strings (locked at B.4.0 + B.4.6).
- **Usings added**: `UnityEditor.SceneManagement` (`EditorSceneManager.NewScene` / `SaveScene`) + `UnityEngine.SceneManagement` (`SceneManager.MoveGameObjectToScene`).
- **`package.json` `samples[]` entry added**: 4th entry for `"Catalog — Group C — Progression"` describing the 5 catalog elements + 2 stubs + Group B dependency for the chain demo. Discoverable in Package Manager UI under Samples after upgrade. **This entry is the buyer-facing closure of Group C** — it's what makes the kit promise "drag and drop a popup that already works" tangible for the catalog's 5 progression elements.

### Triple-round `/_checker as user/dev/qa` (pre-tag audit) — 2026-05-04

Pre-tag audit on B.4.0 → B.4.6 + 5 prefabs + demo scene + samples[] entry. 12 findings collapsed via mental-pass to 2 FIX NOW (C1 + C2 — text-only edits applied this commit) + 8 NOTE deferred to M3/M4 polish (already captured in `kitforge_mobile_ui_kit_roadmap.md` § Diferidos or this CHANGELOG).

- **C1 — HUDTimer first-Play UX bug** (rol `as user` U2 + `as qa` Q2): default `_targetUtc = MinValue` made HUDTimer expire immediately on Play → "00:00" out-of-the-box. Fixed by adding `Start()` initializer in `CatalogGroupCDemo` that seeds 5-minute countdown so first impression shows a live ticking timer.
- **C2 — Multi-timer ContextMenu misleading name** (rol `as user` U3 + `as qa` Q5): "Chain — Multi-timer reset (3 min countdown)" was categorized as Chain but only set a single timer; spec QA 11 expects 3 instances of HUDTimer. Fixed by renaming + recategorizing as `"HUD Timer — Set 3min countdown (multi-timer requires extra prefab instances)"` with log message guiding manual instantiation. Honest naming aligned with what the helper actually does. The 6th mejora I scenario is still addressable — buyer can read the log and add extra HUDTimer prefab instances; the kit's responsibility ends at the contract documentation.

**8 NOTE deferred** (captured for `/_close` → memoria, no changes this session): N1 2-step build dependency (documented), N2 build dialog Group B prerequisite hint (M3), N4 cross-sample scene reference fragility (documented), N5 60s regen tick QA-slow (M3 ContextMenu helper), N7 DailyLoginFlow.ShowIfDue not in Play demo (M3), N8 InMemoryProgressionService stub-economy wiring fragility (documented), N9 demo no anti-spam guard (Group B precedent), N10 watch-to-double doubles only `rewards[0]` (limitation documented in spec D5).

### Spec citations (Phase B § 4.5 batch sweep)

- **`Documentation~/Specs/Catalog/GameOverPopup.md`** — `Services consumed` renamed to `Service binding`, cites § 4.5.
- **`Documentation~/Specs/Catalog/HUD-Energy.md`** — `Services consumed` renamed to `Service binding`, cites § 4.5 with HUD-degrade qualifier.
- **`Documentation~/Specs/Catalog/HUD-Timer.md`** — `Services consumed` renamed to `Service binding`, cites § 4.5 with HUD-degrade qualifier.

### Added (pre-flight infrastructure)

- **`HUDCurrency.cs`** under `Runtime/Catalog/HUD/` — parameterized HUD displaying a single currency from `IEconomyService`, replaces the v0.5.0 `HUDCoins`/`HUDGems` sibling classes. `[SerializeField] private CurrencyType _currency` set at prefab time only (tooltip warns against runtime changes — the HUD does not re-subscribe). Subscribes to `IEconomyService.OnChanged` and filters foreign currencies via `if (currency != ResolveCurrency()) return` (FD4 contract — sentinel `(CurrencyType)(-1)` used by RewardPopup for Item/Bundle is naturally excluded). `protected virtual CurrencyType ResolveCurrency() => _currency` — extension hook for subclass sealing.
- **`HUDEnergy.cs : HUDCurrency`** — sealed subclass with `protected override CurrencyType ResolveCurrency() => CurrencyType.Energy`. The inherited `_currency` Inspector field has no effect (read-once-via-virtual-method pattern); buyer cannot break the energy binding via Inspector mistake. Pre-flight ships the **sealing scaffold only** — Phase B HUD-Energy element delivery extends with regen countdown UI, max-cap label, energy bar fill, and 1Hz `IProgressionService.GetEnergyRegenState()` poll.
- **`IProgressionService` extension** in `Runtime/Services/IProgressionService.cs`: 2 new methods `GetDailyLoginState()` / `GetEnergyRegenState()` + 2 supporting `[Serializable]` structs. `DailyLoginState` carries `CurrentDay`, `LastClaimUtc`, `AlreadyClaimedToday`, `DoubledToday` (FQ3), `MaxStreakGapDays`. `EnergyRegenState` carries `Current`, `Max`, `NextRegenUtc`, `IsFull`. Backward compatible — existing level-progression members preserved.
- **`UIThemeConfig` slots (5 new)**: `IconEnergy`, `IconClock`, `StarFilledSprite`, `StarEmptySprite` (sprites under `[Header("Sprites")]`) + `FailureColor` (color under `[Header("Colors")]`, default `#E53935` red 600). Used by Phase B elements (HUD-Energy, HUD-Timer, LevelComplete star reveal, GameOver header tint). Existing Theme assets get the new slots auto-populated as null/default — buyer fills them via Bootstrap Defaults Editor menu or manually.
- **`Tests/Editor/Helpers/FakeProgressionService.cs`** — minimal `IProgressionService` test fake. Setters: `SetCurrentLevelIndex`, `SetDailyLoginState`, `SetEnergyRegenState`. Phase B element tests will use this for behavior coverage.
- **`Tests/Editor/HUDCurrencyTests.cs`** — replaces `HUDCoinsTests` + `HUDGemsTests` with parameterized `[TestCase(CurrencyType.Coins)]` + `[TestCase(CurrencyType.Gems)]` covering the 3 currency-relevant paths (initial value, change handler, large-value formatting). Adds standalone `OnChanged_With_Foreign_Currency_Does_Not_Trigger_ApplyValue` (FD4 currency-filter contract test). Null-service path keeps its dedicated test.
- **Documentation**: `Documentation~/Specs/Catalog/CATALOG_GroupC_DELTA.md` runbook with 8 sections — Theme slots / IProgressionService extension / HUDCurrency event filtering (FD4) / IEconomyService v2 / Q2 helpers contract / null-service fallback policy (§ 4.5) / HUDCurrency+HUDEnergy implementation specs (§ 4.6) / pre-flight follow-ups + 4-block migration table spec (§ 3) + 8 FIX-NEXT items absorbed from `as qa`/`as dev` mental pass.

### Changed (BREAKING)

- **`IEconomyService` v2** in `Runtime/Services/IEconomyService.cs` — typed surface removed (`GetCoins`, `GetGems`, `SpendCoins`, `SpendGems`, `AddCoins`, `AddGems`, `OnCoinsChanged`, `OnGemsChanged`). Replaced by parameterized: `int Get(CurrencyType)`, `bool Spend(CurrencyType, int)`, `void Add(CurrencyType, int)`, `bool CanAfford(CurrencyType, int)` (already parameterized in v1, signature unchanged), `event Action<CurrencyType, int> OnChanged`. See migration block 1 above for buyer-side renames.
- **`CurrencyType` enum** in `Runtime/Services/DTO/ShopItemData.cs`: `Energy = 2` value added; existing `Coins = 0`, `Gems = 1` preserved with explicit values for stable serialization.
- **`Runtime/Catalog/Popups/Shop/ShopPopup.cs`** — single `economy.OnChanged` subscription replaces previous double subscription to `OnCoinsChanged` + `OnGemsChanged`. Handler signature: `(CurrencyType _, int __) => RefreshAffordability()` — affordability check is currency-agnostic and fires once per any currency change.
- **`Tests/Editor/Helpers/FakeEconomyService.cs`** — implements v2 surface; `SetCoins` / `SetGems` / `AddCoins` / `AddGems` retained as **test-only helpers** internally calling `Add` / `SetCurrency` (now public). HUDCurrencyTests + non-economy fixtures do not need to change.
- **`Samples~/Catalog_GroupB_Currency/Stubs/InMemoryEconomyService.cs`** + the imported copy under `Assets/Samples/Kitforge Mobile UI Kit/0.5.0-alpha/...` — both rewritten to v2. Adds `_energy = 30` field + `Debug — Add 50 energy` ContextMenu so the Group B sample remains runnable post-upgrade for buyers who stay on Group B prefabs.
- **`Samples~/Catalog_GroupB_Currency/Stubs/InMemoryShopDataProvider.cs`** + imported copy — `_economy.SpendCoins/SpendGems` collapsed to `_economy.Spend(item.PriceCurrency, item.PriceAmount)`; `_economy.AddCoins(N)` → `_economy.Add(CurrencyType.Coins, N)`.
- **`Samples~/Catalog_GroupB_Currency/CatalogGroupBDemo.cs`** + imported copy — 9 v1 callsites migrated to v2. `HandleRewardClaimed` simplified from typed if/else chain to single `_services.Economy.Add(currency, amount)` (sentinel still filtered before the call).
- **`Editor/Generators/CatalogGroupBBuilder.cs`** — `BuildHUDCoins`/`BuildHUDGems` produce `HUDCurrency`-typed prefabs with `_currency` set via `SerializedObject.FindProperty("_currency").enumValueIndex`. Demo scene wiring uses `GetComponent<HUDCurrency>()`. `Build Group B Sample` continues to work post-migration — the menu item is preserved as a transitional path until `Build Group C Sample` ships in Phase B.

### Removed (BREAKING)

- **`Runtime/Catalog/HUD/HUDCoins.cs`** + `.meta` — replaced by `HUDCurrency` parameterized base.
- **`Runtime/Catalog/HUD/HUDGems.cs`** + `.meta` — same.
- **`Tests/Editor/HUDCoinsTests.cs`** + `.meta` — replaced by parameterized `HUDCurrencyTests` + FD4 filter test.
- **`Tests/Editor/HUDGemsTests.cs`** + `.meta` — same.
- **`IEconomyService` v1 surface**: 8 typed members removed (`GetCoins`, `GetGems`, `SpendCoins`, `SpendGems`, `AddCoins`, `AddGems`, `OnCoinsChanged`, `OnGemsChanged`). See Changed `IEconomyService v2` and migration block 1 above.

### Architecture decisions (locked in pre-flight)

- **Null-service fallback policy** (`CATALOG_GroupC_DELTA.md` § 4.5): HUDs degrade silently (render plain value or last-known); popups `Debug.LogError` actionable + abort `OnShow` so PopupManager queue advances; helpers `Debug.LogError` + return false. Mandatory error format: `"[ElementName]: I[ServiceName] not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding."`. Phase B element specs cite § 4.5 in their "Service binding" subsection. QA scenarios `Show_With_Null_Required_Service_Logs_Error_And_Aborts` per popup; `Bind_With_Null_Service_Renders_Plain_Value_Silently` per HUD.
- **Sealing pattern for HUD subclasses** (`CATALOG_GroupC_DELTA.md` § 4.6): `protected virtual CurrencyType ResolveCurrency() => _currency` in base; subclasses override to seal currency unconditionally. Prevents Inspector-mistake breakage. Validated on HUDEnergy.

### Pending (Phase B — required for `v0.6.0-alpha` tag cut)

- **5 catalog elements** under `Runtime/Catalog/`: `Popups/DailyLogin/`, `Popups/LevelComplete/`, `Popups/GameOver/`, `HUD/HUDEnergy.cs` extension (regen UI + IProgressionService poll), `HUD/HUDTimer.cs`. Specs already locked in `Documentation~/Specs/Catalog/`.
- **3 helpers** (Q2 lock — capability-gated for Group C): `RewardFlow.GrantAndShow` (+ DTO overload F2), `RewardFlow.GrantAndShowSequence` (with `onSequenceComplete` callback F5), `ShopFlow.OpenWithPurchaseChain`.
- **`Editor/Generators/CatalogGroupCBuilder.cs`** — skeleton DONE in B.4.1 + 5 `BuildXxx` generators DONE in B.4.2-B.4.5 + `BuildDemoScene` + `TryAttachStubServices` + `WireHUDServices` DONE in B.4.6 (above). **Builder complete** — `Build Group C Sample` MenuItem now generates 5 prefabs + demo scene end-to-end.
- **`Samples~/Catalog_GroupC_Progression/CatalogGroupCDemo.cs`** (B.4.6) — chain demo with 6 `[ContextMenu]` scenarios (LevelComplete → Reward sequence, GameOver Continue paths, DailyLogin auto-trigger, Energy regen tick, multi-timer per HUD-Timer QA scenario 11) + builder-wired prefab references.
- **`package.json` `samples[]` entry** for "Catalog — Group C — Progression" (deliberately NOT added in B.4.0 per `/_checker` mejora F1 — adding it without B.4.6 demo would expose a half-finished sample to buyers; lands at B.4.6 closure).
- **`Samples~/Catalog_GroupC_Progression/`** stub + asmdef shipped in B.4.0 (above) — buyer-facing demo wiring lands at B.4.6.

### Done criteria (for `v0.6.0-alpha` tag)

Pre-flight (this batch — done 2026-05-03):
- [x] `UIThemeConfig` 5 new slots compile; gate green
- [x] `IProgressionService` extended; `FakeProgressionService` ships; gate green
- [x] `IEconomyService` v2 BREAKING migration applied (Runtime + Tests + Samples~ source + imported `Assets/Samples` copy); gate green
- [x] `HUDCurrency` parameterized base + `HUDEnergy` minimal sealing subclass shipped; `HUDCurrencyTests` covers parameterized cases + FD4 filter test; gate green
- [x] `CatalogGroupBBuilder` migrated to produce HUDCurrency-based prefabs (back-compat for Group B sample post-upgrade)
- [x] All 119 baseline tests pass on fresh compile after each delta
- [x] CHANGELOG migration table present with 5 mandatory blocks (this section)

Phase B (in progress):
- [x] DailyLogin element 1/5 — DTO + behavior + Flow helper + UIAnim wrapper + 25 EditMode tests + § 4.5 spec citation
- [x] LevelComplete element 2/5 — DTO + behavior + UIAnim wrapper + 16 EditMode tests + § 4.5 spec citation. Animation polish (star cascade, score rollup, CTA stagger) deferred to `_tween` session.
- [x] GameOver element 3/5 — DTO + `ContinueMode`/`BackPressBehavior` enums + behavior + UIAnim wrapper + 18 EditMode tests + § 4.5 spec citation. Animation polish (CTA cascade, Cinematic preset Failure-color accents) deferred to `_tween` session.
- [x] HUD-Energy element 4/5 — extended with regen UI (countdown + cap suffix + bar fill) + 1Hz `IProgressionService` poll per § 4.6 + auxiliary OnChanged-driven poll per E3 + 10 EditMode tests + § 4.5 silent-degrade contract proof + anchor `// OnUpdate-workaround-M3-sweep` per mejora F. Empty pulse (E8) + overcap tint deferred to M3 polish.
- [x] HUD-Timer element 5/5 — `HUDTimer` + `TimerMode` enum (3 modes) + warning/expiry tween infra + pause/resume + format validation + 10 EditMode tests + § 4.5 silent-degrade drift fix (UTC null-Time → `"--:--"` silently) + `UIAudioCue.Error` for warning (Failure cue doesn't exist, drift fix) + anchor `// OnUpdate-workaround-M3-sweep` per mejora F. Multi-timer test (QA scenario 11) + server-time-jump-backward (QA 14) deferred to B.4 builder/demo coverage.
- [x] Helpers — capability-gate verdict 2026-05-04: ✅ `RewardFlow.GrantAndShowSequence` shipped (2 callsites); ⏳ `RewardFlow.GrantAndShow` single + ⏳ `ShopFlow.OpenWithPurchaseChain` deferred Group D (1 callsite each — capability-gate failed). 7 EditMode tests + 1 integration test (LevelCompletePopupTests via mejora H).
- [x] `Build Group C Sample` builder + chain demo (B.4.1-B.4.6 closed 2026-05-04 — 5 prefabs + demo scene + 6 chain ContextMenu scenarios + `samples[]` entry)
- [x] `InMemoryProgressionService` + `InMemoryTimeService` sample stubs shipped under `Samples~/Catalog_GroupC_Progression/Stubs/` (B.4.0 + B.4.6 closed 2026-05-04)
- [x] Tag `v0.6.0-alpha` BREAKING (single bump per group) — closed 2026-05-04, M1 done

## [0.5.0-alpha] — 2026-05-02

> Group B — Currency catalog. Adds 3 popups (Reward / Shop / NotEnoughCurrency), 2 HUD elements (Coins / Gems), 3 in-memory service stubs (Economy / Shop / Ads), one demo scene chaining the full monetization loop, and an `UIAnimPopupBase` consolidating the per-popup animator boilerplate.

### Added
- **5 catalog elements** under `Runtime/Catalog/`:
  - `Popups/Reward/` — `RewardKind`, `RewardPopupData`, `RewardPopup`, `UIAnimRewardPopup`. Supports `Coins`/`Gems`/`Item`/`Bundle` reward kinds, optional `AutoClaimSeconds`, optional backdrop-tap-to-claim. Emits `OnClaimed(CurrencyType, int)` + `OnDismissed`. Item/Bundle use sentinel `(CurrencyType)(-1)` to signal "host-resolved" amount. Popup never mutates economy — host wires the credit.
  - `Popups/Shop/` — `ShopPopupData`, `ShopCategoryFilter`, `ShopItemView` (cell), `ShopPopup`, `UIAnimShopPopup`. Clone-from-template grid; subscribes to `IEconomyService.OnCoinsChanged`/`OnGemsChanged` and re-evaluates affordability without rebuild. Calls `IShopDataProvider.Purchase(itemId)` only — never `IEconomyService.Spend*`. Disables the cell of an item that returned `InsufficientFunds` until the next currency event refreshes it (prevents repeat-fire spam).
  - `Popups/NotEnough/` — `NotEnoughCurrencyPopupData`, `NotEnoughCurrencyPopup`, `UIAnimNotEnoughCurrencyPopup`. Three CTAs: BuyMore / WatchAd / Decline. Queries `IAdsService.IsRewardedAdReady()` on Bind to gray out Watch Ad when no ad is available. Emits `OnBuyMoreRequested(CurrencyType, int)` / `OnWatchAdRequested(CurrencyType, int)` / `OnDeclined`. Logs warning if all CTAs hidden + backdrop disabled (foot-gun guard for iOS-only builds without hardware back).
  - `HUD/HUDCoins.cs` + `HUD/HUDGems.cs` — Live counters reactive to `IEconomyService` typed events. Punch-scale tween on every change (kill-before-create, `SetLink(gameObject)`). Default format `"N0"` (thousand separators). Optional `UnityEvent` click hook for "open shop" wiring. `"--"` fallback + actionable error log when `IEconomyService` is unavailable.
- **`UIAnimPopupBase`** under `Runtime/Catalog/_Internal/`. Consolidates the show / hide / preset / sequence / Snap / ResetToShowStart logic that was previously duplicated across each popup's animator. New popups now inherit and add zero code (e.g., `public sealed class UIAnimRewardPopup : UIAnimPopupBase { }`). Group B animator files reduced from 3 × 110 lines to 1 base + 3 × 3-line stubs.
- **3 in-memory stubs** under `Samples~/Catalog_GroupB_Currency/Stubs/`: `InMemoryEconomyService` (250 coins / 5 gems seed, debug ContextMenus), `InMemoryShopDataProvider` (4 hardcoded items spanning Currency / Consumable / Cosmetic categories, delegates `Spend` to the economy service via `[RequireComponent]`), `InMemoryAdsService` (1s simulated rewarded ad). All deterministic — no network, no save, no persistence.
- **`Build Group B Sample` editor menu** (`CatalogGroupBBuilder.cs`): generates 5 prefabs + 1 demo scene under `Assets/Catalog_GroupB_Demo/`. Pre-flight check warns if Theme or stubs are missing. Wires UIServices + 3 stubs + HUD instances + Demo MonoBehaviour with all prefab references in one click.
- **Demo MonoBehaviour** (`CatalogGroupBDemo.cs`) with 19 `[ContextMenu]` triggers across 5 sections: HUD debug (5 — Add/Spend coins/gems), Reward variations (7 — Coins/Gems/Item/Bundle/Auto/Empty/Backdrop), Shop variations (3 — All/Currency/Cosmetics), NotEnough variations (3 — Coins/Gems/Decline-only), and 1 **`Chain — Shop → NotEnough → Ad → Reward`** end-to-end demonstration. Chain trigger pre-flight-checks all three popup prefabs are assigned before opening the Shop.
- **Specs** under `Documentation~/Specs/Catalog/`: `RewardPopup.md`, `ShopPopup.md`, `NotEnoughCurrencyPopup.md`, `HUD-Coins.md`, `HUD-Gems.md`. Each opens with a "Decisions to confirm" table promoting per-element implementation choices to spec contract.
- **EditMode tests +37**: 8 RewardPopup, 9 ShopPopup (including `InsufficientFunds_Disables_The_Matching_Cell_Until_Affordability_Refreshes`), 10 NotEnoughCurrencyPopup, 4 HUDCoins, 4 HUDGems (including `Coins_Changed_Does_NOT_Update_Gems_HUD` for typed-event isolation). Total catalog tests: 90+.
- **Test helpers** under `Tests/Editor/Helpers/`: `NullAnimator.cs` and `FakeEconomyService.cs` shared across the 5 new test fixtures. Replaced 3× duplicate `NullAnimator` and 2× duplicate `FakeEconomyService` private nested classes.

### Changed
- **`UIHUDBase`**: added `protected void SetServicesInternal(UIServices)` to enable test injection without reflection. HUDCoins/HUDGems `SetServicesForTests` now a one-line forward instead of `BindingFlags.NonPublic` field hacking.
- **Sample registration**: `package.json` `samples[]` now lists 3 entries (Quickstart, Catalog Group A, Catalog Group B).
- **`NotEnoughCurrencyPopupData` defaults aligned to spec N5**: `ShowDecline` `true` → **`false`** (decline button is opt-in; backdrop/back already cover the implicit decline path). `CloseOnBackdrop` `false` → **`true`** (forgiving UX; tap-out cancels the offer). DTO was internally inconsistent with `Documentation~/Specs/Catalog/NotEnoughCurrencyPopup.md` § N5; tests `Default_DTO_Hides_Decline_Button` and `CloseOnBackdrop_True_By_Default` codified the spec contract. **No buyer-visible regression** (no tagged version shipped the previous defaults).

### Fixed
- **`Samples~/Catalog_GroupB_Currency/CatalogGroupBDemo.cs`**: removed duplicate `using KitforgeLabs.MobileUIKit.Catalog.NotEnough` directive and duplicate `[SerializeField] private GameObject _notEnoughPrefab` field declaration (CS0102 compile error). Group B sample asmdef failed to compile, which silently cascaded — `Build Group B Sample` resolved demo + stub types via `System.Type.GetType` and got `null` from a non-compiling assembly, producing a demo scene with no `CatalogGroupBDemo` component, no in-memory stubs, and no HUD service wiring. The 19 ContextMenu triggers were not exercisable until this fix landed.
- **`Tests/Editor/Helpers/FakeShopDataProvider.cs`** (new — mirrors `FakeEconomyService` pattern): minimal `IShopDataProvider` test fake exposing `SetItems` / `QueuePurchaseResult`. `ShopPopupTests.SetUp` now creates a `UIServices` + injects the fake via `services.SetShopData(_shopData)` + calls `_popup.Initialize(null, services)`. Without this, 7 of 8 ShopPopup tests false-failed on a fresh test run because `ShopPopup.Bind` → `RebuildGrid` → `ResolveItems` emits `[ShopPopup] No IShopDataProvider available` `LogError`, and Unity's Test Runner treats unexpected `LogError` as a failure. **Symptom only — no runtime change**; the LogError is intentional buyer-facing diagnostic when wiring is forgotten.
- **`UIServices`**: added `[DefaultExecutionOrder(-100)]` so its `Awake` (where serialized service refs are resolved into typed properties: `Economy`, `ShopData`, `Ads`, …) is guaranteed to run before any consumer's `OnEnable`. Without this, Unity does NOT guarantee Awake-before-OnEnable across GameObjects in the same scene; HUDCoins/HUDGems `OnEnable → Refresh` could read `Services.Economy == null` and emit the actionable buyer-facing `LogError` even when wiring was correct in the Inspector. Caught only during the in-Editor Group B chain demo verification — the 119 EditMode tests injected services manually via `SetServicesForTests` and could not exercise this code path. **Buyer impact:** silently fixes any UIServices+HUD scene where script execution order was previously undefined.

### Deferred (Group C)
- `RewardFlow.GrantAndShow` / `RewardFlow.GrantAndShowSequence` / `ShopFlow.OpenWithPurchaseChain` — capability-gate fails for Group B (1 chain callsite). Will land when 3+ callsites materialize at Group C kickoff (DailyLogin / LevelComplete / GameOver). Spec stub written in `RewardPopup.md` and `ShopPopup.md` to lock the event surface that the helpers will consume.
- `IEconomyService` v1 (typed `OnCoinsChanged`/`OnGemsChanged`) → v2 (parameterized `OnChanged(CurrencyType, int)`) migration — open question for Group C kickoff before HUD-Energy lands. Tradeoff table documented in `HUD-Gems.md` and `kitforge_mobile_ui_kit_roadmap.md`.

### Done criteria
- [x] Group B 5 elements + tests + specs + Build Group B Sample editor menu + demo scene + chain trigger.
- [x] All popups respect MUSTN'T #1 (no service mutation, no popup-to-popup spawn calls).
- [x] HUD-Coins and HUD-Gems coexist in the same scene with isolated punch tweens (no cross-channel leakage).
- [x] Chain demo wires the full Shop → NotEnough → Ad → Reward → HUD update loop using only public events.

## [0.4.1-alpha] — 2026-05-02

### Removed
- **`Game Wiring` sample parked**: removed from `package.json` `samples[]` so Package Manager no longer offers it for import. Source files stay under `Samples~/GameWiring/` (`GameWiringLifetimeScope.cs`, `StubServices.cs`, `UIRouterStub.cs`, `KitforgeLabs.MobileUIKit.GameWiring.asmdef`, `README.md`) for revival once service contracts stabilize at end of Group D. **Why**: pre-existing buyer-facing P0 caught during v0.4.0 verification — sample required VContainer (opt-in, not installed in PACHINKO host) and the 5 stub services had ~30 unimplemented interface members added during v0.3.0 G3 work. Importing the sample blocked the entire host project from compiling. Path to revival = update stubs after Group D ships `IPlayerDataService` + `SettingsPopup`, then re-add the entry to `samples[]`.

### Changed
- **README**: VContainer prereq note rewritten — no longer mentions `Samples~/GameWiring`; instead points buyers to `UIServices` MonoBehaviour setters as the DI integration surface. Architecture decision #1 + #8 updated. Phase 1 done-criteria checkbox for `Samples~/GameWiring` removed (no longer shipped). Install URL bumped to `v0.4.1-alpha`.
- **Quickstart README**: 3 cross-references to `Game Wiring` rewritten as "parked under `Samples~/`, returns once Group D ships".
- **Catalog Group A README**: "See also" link to GameWiring annotated as parked.

### Done criteria
- [x] `package.json` `samples[]` lists Quickstart + Catalog Group A only (2 entries).
- [x] No README references guide buyers to import a broken sample.
- [x] `Samples~/GameWiring/` source preserved on disk for future revival.

## [0.4.0-alpha] — 2026-05-02

> Group A — Pure UI catalog. Tag cut after Unity Editor verification of the 7-item Done criteria below: Bootstrap Defaults runs, `Build Group A Sample` produces 4 prefabs + scene with all 5 references wired (Theme + 4 prefabs), 4 prefabs open clean (36/36 catalog refs + 4/4 animator refs), 12 ContextMenu entries spawn correctly in Play, EditMode Test Runner reports 86/86 green, Editor.asmdef compiles after the Catalog reference addition, `git status` aligned with the deliverables list.

### Group A closure (this batch — specs, extra tests, severity polish, prefab generator, Theme contract honoring)
Resolves the per-element `Pending` lines flagged in the four "Group A — element N/4" entries below. Closes the TA checker gap "Theme sprite slots not consumed by generated prefabs" — the core "skin it once" pitch now actually works for catalog prefabs.

- **Specs written** under `Documentation~/Specs/Catalog/`: `NotificationToast.md`, `PausePopup.md`, `TutorialPopup.md`. Each opens with a **"Decisions to confirm"** block (D1-D8 / D1-D7 / D1-D8 respectively) promoting per-element implementation choices to spec contract. `ConfirmPopup.md` already shipped.
- **EditMode tests +25**: PausePopup +9 (intent + shortcut + toggle events for Restart / Home / Quit / Settings / Shop / Help / Sound / Music / Vibration), TutorialPopup +9 (3 backdrop modes / 2 StartIndex clamping / `CompleteTutorial` API / re-Bind StartIndex / 2 empty-Steps no-op), NotificationToast +7 (3 tap paths / 3 severity mappings / re-bind safety). Total catalog tests: 53. Total package tests: **86** (33 framework + 53 catalog).
- **TutorialPopup early-return guards**: `GoNext` and `GoPrevious` now `if (StepCount == 0) return` — prevents `OnNext(0)` firing with empty Steps list (defensive; `Refs` UI hides those buttons but programmatic calls would have leaked the event).
- **NotificationToast severity mapping upgraded**:
  - `Warning` → `WarningColor` (new Theme field) — was `AccentColor` (semantic neighbor; not distinct enough for a premium asset).
  - `Info` / `Warning` / `Error` → `IconInfo` / `IconWarning` / `IconError` (new Theme slots) — were `null` (only `Success` had `IconCheck`).
  - `SeverityIcon` hide path: `gameObject.SetActive(false)` instead of `Image.enabled = false` — `LayoutGroup` now reflows the row when icon slot is null instead of leaving a gap.
  - `SeverityToColor` / `SeverityToIcon` / `SeverityToCue` helpers: `private static` → `internal static` to allow direct unit testing without prefab Refs setup.
- **`UIThemeConfig`**: added `_warningColor` (default `(1.00f, 0.60f, 0.15f, 1f)` saturated orange), `_iconInfo`, `_iconWarning`, `_iconError` sprite slots. Removed legacy `_transitionSpeed` and `_bounceStrength` fields (zero consumers since v0.3.0 — animation tuning lives entirely in `UIAnimPreset` SO now). **BREAKING for assets that wrote those slots** — re-save existing Theme assets after upgrade.
- **Access modifier changes (testability)**: 9 `Handle*` in `PausePopup`, 1 in `TutorialPopup` (`HandleBackdrop`), 1 in `NotificationToast` (`HandleTap`) promoted `private` → `internal`. Catalog test asmdef has `InternalsVisibleTo` since v0.3.0; this matches the precedent set by `SetAnimatorForTests`.
- **`PausePopupTests` SetUp** now creates a mock `UIThemeConfig` + `UIAnimPreset` + `UIServices` and calls `popup.Initialize(theme, services)` before each test — silences the cosmetic `[PausePopup] Theme not initialized` and `[PausePopup] No animation preset resolved` warnings the four `OnShow*` tests were emitting. The other three Catalog fixtures don't call `OnShow` so they don't need this.
- **New Editor generator** `Tools/Kitforge/UI Kit/Build Group A Sample` (`Editor/Generators/CatalogGroupABuilder.cs`): single-click materialization of `ConfirmPopup.prefab` + `PausePopup.prefab` + `TutorialPopup.prefab` + `NotificationToast.prefab` + `Catalog_GroupA_Demo.unity` into `Assets/Catalog_GroupA_Demo/`. Demo scene wires a `Canvas` (1080×1920 portrait CanvasScaler) + `EventSystem` + `UIServices` + the `CatalogGroupADemo` host MonoBehaviour with 12 `[ContextMenu]` triggers (4 Confirm tones / 2 Pause variants / 2 Tutorial variants / 4 Toast severities). Idempotent: re-running overwrites.
- **`CatalogGroupABuilder` asset-reference persistence fix + `CatalogGroupADemo` field-type refactor** (caught during v0.4.0 verification, two complementary changes):
  - **Builder**: original implementation (a) passed `PrefabUtility.SaveAsPrefabAsset` return values directly into `BuildDemoScene`'s `SerializedProperty.objectReferenceValue` assignments — Component-on-prefab refs serialized as `{fileID: 0}`; (b) loaded `UIThemeConfig` BEFORE `EditorSceneManager.NewScene(EmptyScene, Single)` — the asset reference survived in memory but Unity's scene serializer wrote it as `{fileID: 0}` after the scene replacement. Diagnostic logs proved both pre-Apply and post-Apply re-reads of the SerializedProperty held a valid reference (instanceID present, name resolvable), yet `SaveScene` persisted null. Root cause for both: asset references obtained outside the active scene's lifetime become "stale" for the scene serializer even though they remain valid C# references. Fix: `BuildAll` now does `SaveAssets()` + `Refresh()` between prefab creation and `BuildDemoScene()`. `BuildDemoScene` calls `EditorSceneManager.NewScene` FIRST, then loads all 5 assets via `AssetDatabase.LoadAssetAtPath` (Theme as `UIThemeConfig`, prefabs as `GameObject`) — references obtained against the active scene context persist correctly. `BuildAll` no longer threads prefab refs through parameters.
  - **Sample**: `CatalogGroupADemo` 4 prefab fields changed from `ConfirmPopup`/`PausePopup`/`TutorialPopup`/`NotificationToast` Component types → `GameObject`. Decouples the asset persistence story from Component-vs-GameObject reference type (orthogonal cleanup). SpawnXxx methods now do `Instantiate(prefab).GetComponent<T>()` at runtime with explicit null check (logs an error and destroys the orphan if a buyer assigns a prefab missing the expected component). New private `SpawnPopup<T>` helper deduplicates the 3 popup spawn paths; `SpawnToast` stays inline (different parent + base type). Trade-off: Inspector drag-drop accepts any prefab, not just those carrying the typed Component — runtime check enforces the contract instead. Documented via `[Header]` tooltip.
- **New Sample** registered in `package.json`: **"Catalog — Group A — Pure UI"** (`Samples~/Catalog_GroupA_PureUI/`). Ships host script (`CatalogGroupADemo.cs`), asmdef, README. Two-step OOTB flow: `Bootstrap Defaults` → `Build Group A Sample`.
- **`Editor.asmdef`**: now references `KitforgeLabs.MobileUIKit.Catalog` + `Unity.TextMeshPro` (required by the builder).
- **README §Status rewritten** with a 6-group table (0 / A / B / C / D / E) showing per-group ship status. Latest tag updated to `v0.3.0-alpha`. Total tests count surfaced.
- **README §Architecture decisions** new entry #11: a single `UIThemeConfig` asset feeds `UIManager` + `PopupManager` + `ToastManager`. Closes the documentation gap flagged by the close_context.
- **`Documentation~/Specs/CATALOG.md`** row links: ConfirmPopup / PausePopup / TutorialPopup / NotificationToast all link to their per-element specs.
- **`package.json` description** updated to mention the catalog (4 elements shipped in `v0.4` + group-by-group roadmap).
- **NEW Theme contract: `IThemedElement` + `ThemedImage` + `ThemedText`** in `Runtime/Theme/`:
  - `IThemedElement.ApplyTheme(UIThemeConfig)` is the public contract; any component implementing it gets called when a popup or toast is initialized.
  - `ThemedImage` (slots: `ThemeSpriteSlot` enum {`PanelBackground` / `ButtonPrimary` / `ButtonSecondary` / `Backdrop` / `Divider`} + `ThemeColorSlot` enum 10 entries including new `WarningColor`).
  - `ThemedText` (slots: `ThemeFontSlot` enum {`FontHeading` / `FontBody` / `FontCaption`} + `ThemeColorSlot` + `ThemeFontSizeSlot`).
  - `None` enum value on every slot = "leave the authored value untouched" — supports literal-color text on themed buttons (white-on-primary).
- **`UIModuleBase.Initialize` and `UIToastBase.Initialize`** now walk children and call `ApplyTheme` on every `IThemedElement` found. `protected ApplyThemeToChildren(theme)` available for manual re-apply if buyer ever needs it (e.g., theme swap mid-session). One-time on instantiate; no perf cost in steady state.
- **`CatalogGroupABuilder` wires `ThemedImage` / `ThemedText`** on Card backgrounds, primary/secondary/danger button backgrounds, Title (FontHeading / TextPrimary), Body / Subtitle / Progress (FontBody or FontCaption / TextSecondary), button labels (FontBody / TextPrimary or untouched). Confirm / Pause / Tutorial / Toast all benefit. Backdrop, Toast `SeverityTint`, ConfirmPopup `ToneStrip` and Toggle visuals stay non-themed by design (their tint/visuals are dynamic per-data, not Theme-driven).
- **Sample README** new sections: **Theme contract** (table of which slot drives which element) + **Hierarchy stability** (safe vs not-safe edits to prefab variants) + clarified re-run instructions (rename output folder before customizing).

### OUT of scope for v0.4.0-alpha
Explicit limits — these do NOT ship in this tag and are not promised by `v0.4.0-alpha`:

- **Catalog Groups B / C / D / E** (RewardPopup, ShopPopup, NotEnoughCurrencyPopup, DailyLoginPopup, LevelCompletePopup, GameOverPopup, SettingsPopup, LoadingScreen, MainMenuScreen, HUD-Coins, HUD-Gems, HUD-Energy, HUD-Timer). 11 catalog elements remain. Group B is next per the roadmap.
- **Default severity icon sprites**. The Theme has slots for `IconInfo` / `IconWarning` / `IconError` but **no default sprites are shipped** — buyer assigns their own. The toast renders correctly without them (icon `gameObject.SetActive(false)`); shipping defaults requires Texture2D + Sprite asset generation in `Bootstrap Defaults`, deferred to a polish session.
- **`Build Group A Sample` style polish**. The generated prefabs are functionally complete but cosmetically minimal: flat colors, no rounded corners, default Unity button colors, no shadow, no padding finesse. Sufficient for a working demo, NOT representative of the final visual quality target ("Disney Getaway Blast" reference). Buyers ARE expected to restyle.
- **Pre-built prefabs in `Samples~/`**. Prefabs are generated on demand by the menu, not committed to the package. Rationale: prefab YAML drift and TMP/Unity GUID resolution issues across versions; the builder script is more maintainable than a binary asset to track.
- **Test Runner CI integration**. The 86 EditMode tests must be run manually until a CI pipeline is set up (out of scope for product-deliverable milestones).
- **Roadmap final sync**. Studio-internal roadmap will be synced to `v0.4.0-alpha` AFTER the tag is cut to avoid claiming a tag that doesn't exist yet.
- **`Samples~/GameWiring/` repair**. Discovered during v0.4.0 verification (2026-05-02): the legacy `GameWiring` sample is broken on import — VContainer is opt-in (not installed in PACHINKO host project, ~3 errors) and the 5 stub services (`StubEconomyService`, `StubPlayerDataService`, `StubProgressionService`, `StubShopDataProvider`, `StubAdsService`, `StubTimeService`) have not been updated since v0.1.0; ~30 interface members added during v0.3.0 G3 service binding work are unimplemented (CS0535). Importing the sample blocks the entire project from compiling. **Pre-existing damage, NOT a v0.4.0 regression** — was masked because `Samples~/` does not compile until imported. Buyer-facing P0. Path forward to be decided in a follow-up session: (a) update stubs + add VContainer dependency note in sample README, (b) refactor sample to be VContainer-free, or (c) remove sample from `package.json` `samples[]` until service contracts stabilize. Option (c) is the cleanest minimum; (a) is the highest fidelity. Tracked in `kitforge_mobile_ui_kit_roadmap.md` for prioritization.

### Done criteria — verified inside Unity (2026-05-02)

- [x] `Tools/Kitforge/UI Kit/Bootstrap Defaults` runs without error and creates `UIThemeConfig_Default.asset` + 10 `UIAnimPreset_*.asset` files.
- [x] `Tools/Kitforge/UI Kit/Build Group A Sample` runs without error and creates 4 prefabs + 1 scene under `Assets/Catalog_GroupA_Demo/`. Demo GameObject has all 5 references wired (Theme + 4 prefabs + UIServices + 2 parents).
- [x] All 4 generated prefabs validated structurally via YAML inspection: 36/36 catalog `Refs` fields and 4/4 animator `_card`/`_root` fields populated (`{fileID: <non-zero>}`). No missing-script entries.
- [x] `Catalog_GroupA_Demo.unity` Play + 12 `[ContextMenu]` entries: confirmed by user during verification.
- [x] **Theme reskin chain** structurally verified: `ThemedImage` and `ThemedText` components present on all themed elements per prefab YAML; `UIModuleBase.Initialize` / `UIToastBase.Initialize` walk children via `GetComponentsInChildren<IThemedElement>(true)` and call `ApplyTheme`. Runtime swap test deferred (pure visual confirmation, contract proven by component graph).
- [x] EditMode Test Runner: 86/86 passed (`TestResults.xml` confirms `total=86 passed=86 failed=0 inconclusive=0 skipped=0`).
- [x] Editor.asmdef compiles with the Catalog reference addition (verified by builder execution + diagnostic logs running successfully).
- [x] `git status` aligned with the v0.4.0-alpha deliverables list (no orphan files outside the catalog scope).

### Verification-only fixes applied during the closure pass

- `EditorSceneManager.NewScene(EmptyScene, Single)` was invalidating the asset references (Theme `ScriptableObject` + 4 prefab Components) loaded BEFORE it — references survived in memory but the scene serializer wrote `{fileID: 0}` to disk. Resolved by moving `LoadAssetAtPath` calls AFTER `NewScene`. Diagnostic logs proved both pre-Apply and post-Apply re-reads held a valid reference yet `SaveScene` persisted null until the load order was corrected. See `CatalogGroupABuilder` entry above.

### Animation system simplified — drop enum + library indirection (BREAKING)
- **Why**: 3-piece system (UIAnimStyle enum + UIAnimPresetLibrary SO + UIAnimPreset SOs) violated the kit's own pitch ("skin it once") — buyer needed 8 manual steps to get a fresh Theme to animate. Default Theme silently produced un-animated popups with zero feedback.
- **Removed**: `UIAnimStyle` enum (10 styles + Custom), `UIAnimPresetLibrary` SO (style->preset map with fallback), `UIAnimPresetLibraryEditor` (Editor inspector for the library).
- **Removed from `UIThemeConfig`**: `_defaultAnimStyle` field, `_animPresetLibrary` field, `DefaultAnimStyle` getter, `AnimPresetLibrary` getter.
- **Added to `UIThemeConfig`**: `_defaultAnimPreset` field (single `UIAnimPreset` ref), `DefaultAnimPreset` getter, tooltip clarifying "null = popups appear without animation".
- **`UIModuleBase` / `UIToastBase`**: `virtual UIAnimStyle? AnimStyleOverride => null` -> `virtual UIAnimPreset AnimPresetOverride => null`. Per-element override is now a direct preset ref.
- **All 4 popups + toast `ResolvePreset()` simplified**: `AnimPresetOverride ?? Theme?.DefaultAnimPreset`. Two levels of indirection collapsed into one.
- **`UIAnimPreset`**: removed `_style` identity field + `Style` getter (asset filename + folder communicate identity now).
- **`DefaultUIAnimPresetsCreator` rebuilt**: menu `Tools/Kitforge/UI Kit/Bootstrap Defaults`. Single click creates 10 presets at `Assets/Settings/UIAnimPresets/` AND a `UIThemeConfig_Default.asset` at `Assets/Settings/UI/` pre-wired to the Playful preset. Buyer's OOTB experience drops from 8 steps to 1.
- **Silent failure fix**: `UIModuleBase` and `UIToastBase` now expose `ResolveAnimPreset()` with a one-shot `Debug.LogWarning` if no preset is resolved (Theme null OR `DefaultAnimPreset` null OR override null). Points the buyer at the bootstrap menu by name. Replaces the duplicated `ResolvePreset()` private helper in 4 popups + toast.
- **README Quickstart rewritten**: step 1 is now the bootstrap menu, step 4 references the auto-generated `UIThemeConfig_Default` asset. Buyer never sees a null preset on first run.
- **Pending**: update `*PopupTests` if any reference the removed enum (`UIAnimStyle` not found in tests grep — should be safe but Unity Test Runner re-run required to confirm).

### Group A — element 4/4: TutorialPopup (scaffolded, code+tests, no spec/prefab yet)
- **`Runtime/Catalog/Popups/Tutorial/`**:
  - `TutorialStep` POCO (Title + Body[TextArea] + optional Sprite).
  - `TutorialPopupData` (List<TutorialStep>, StartIndex, ShowPrevious, ShowSkip, LoopBackToFirst, CloseOnBackdrop, TapToAdvance, custom labels Next/Previous/Skip/Done).
  - `TutorialPopup` (`UIModule<TData>`, `[RequireComponent(UIAnimTutorialPopup)]`, lazy `IUIAnimator` + `internal SetAnimatorForTests`, public `GoNext`/`GoPrevious`/`SkipTutorial`/`CompleteTutorial`/`CurrentIndex`/`StepCount`/`IsFirstStep`/`IsLastStep`, dynamic Next-label that mutates to `DoneLabel` on last step, ProgressLabel "i / N" auto-formatted, theme-null warning one-shot, `ClearAllEvents` on Bind + OnDestroy).
  - `UIAnimTutorialPopup` (clone of `UIAnimPausePopup` structurally — `SetUpdate(true)` defensive in show+hide for tutorials shown over a paused gameplay layer).
- **Decisions taken without consult — flag for review**:
  - Back press = Skip (mobile modal convention; Previous stays explicit).
  - GoNext on last step → `OnCompleted` + dismiss; if `LoopBackToFirst=true`, wraps to index 0 instead and fires `OnNext`.
  - `TapToAdvance` overrides `CloseOnBackdrop` on backdrop tap.
  - `GoPrevious` on first step is silently ignored (no event, no audio cue).
  - No `Time.timeScale` handling — Tutorial is gameplay-aware; if pause is needed, host wraps it (or composes inside Pause).
  - Single Next button mutates label vs separate Done button (consistent with mobile onboarding patterns).
  - Per-element animator (no shared code with Pause despite duplication — catalog rule: 1 animator per element).
- **EditMode tests**: `Tests/Editor/TutorialPopupTests.cs` (10 tests: Bind null defaults, Bind sets StartIndex, GoNext advances + StepChanged, GoNext on last → Completed+dismiss, GoNext on last with loop wraps, GoPrevious on first ignored, GoPrevious decrements, back press → Skip + dismiss, back press while dismissing ignored, Bind resets listeners). **All green: 10/10 (59/59 total).**
- **Pending**: spec (`Documentation~/Specs/Catalog/TutorialPopup.md`), prefab + Demo scene, CATALOG.md row link. Closes Group A → bumps minor to `0.4.0`.

### Group A — element 3/4: PausePopup (scaffolded, code+tests, no spec/prefab yet)
- **`Runtime/Catalog/Popups/Pause/`**: `PausePopupData` (7 buttons + 3 toggles inline + flags), `UIAnimPausePopup` (clone of Confirm with `SetUpdate(true)` for unscaled time), `PausePopup` (`UIModule<TData>`, captures/restores `Time.timeScale` around show/hide, Dismissing vs Shortcut button categories, inline toggles mutate `_data` without closing, public `Resume()`, `OnBackPressed→HandleResume`, theme-null warning, `OnDestroy` restores timeScale if `IsPaused`).
- **Decisions taken without consult — flag for review**:
  - Two button categories: **Dismissing** (Resume/Restart/Home/Quit) close, **Shortcut** (Settings/Shop/Help) raise event and keep popup open.
  - Inline toggles (Sound/Music/Vibration) mutate `_data.XxxOn` + emit `OnXxxChanged(bool)`, never dismiss.
  - `Time.timeScale` direct, no `ITimeService` (YAGNI until 2nd consumer).
  - Pause applied AFTER show-anim (callback of `PlayShow`); restore BEFORE hide-anim.
  - `UIAnimPausePopup` uses `SetUpdate(true)` defensively in hide-anim too.
  - `CloseOnBackdrop` default = `false` (catalog-consistent).
  - Public `Resume()` for external triggers (gameplay button, etc.).
- **EditMode tests**: `Tests/Editor/PausePopupTests.cs` (6 tests: Bind null defaults, back→Resume+Dismiss, back ignored if IsDismissing, OnShow pauses + Resume restores, restores original value not hardcoded 1f, Bind resets listeners). **All green: 6/6 (49/49 total).**
- **Pending**: spec (`Documentation~/Specs/Catalog/PausePopup.md`), prefab + Demo scene, CATALOG.md row link.

### Group A — element 2/4: NotificationToast (scaffolded, code+tests, no spec/prefab yet)
- **Group 0 extension** (`Runtime/Toast/UIToastBase.cs`): added `event Action<UIToastBase> DismissRequested`, `bool IsDismissing { get; protected set; }`, `protected UIThemeConfig Theme`, `protected UIServices Services`, `virtual Initialize(theme, services)`, `protected RaiseDismissRequested()`. Aligns Toast layer with Q1/Q2 decisions taken in element 1.
- **`Runtime/Catalog/Toasts/`**: `ToastSeverity` enum (Info/Success/Warning/Error), `NotificationToastData` DTO (Message + Severity + DurationOverride + TapToDismiss), `NotificationToast` (`UIToast<TData>`, `[RequireComponent(UIAnimNotificationToast)]`, lazy `IUIAnimator`, severity → tint+icon+audio cue mapping, idempotent `DismissNow`, `OnTapped`/`OnDismissed` events, `ClearAllEvents` on Bind), `UIAnimNotificationToast` (slide-in via `PositionOffset` + fade, no scale — toasts slide, don't bounce).
- **Severity → theme mapping** (decisions taken without spec — flag for review):
  - Info → `PrimaryColor` + no icon + `UIAudioCue.Notification`
  - Success → `SuccessColor` + `IconCheck` + `UIAudioCue.Success`
  - Warning → `AccentColor` + no icon + `UIAudioCue.Notification` (no dedicated `WarningColor` in Theme — YAGNI until 2nd consumer)
  - Error → `DangerColor` + no icon + `UIAudioCue.Error`
- **`ToastManager`**: added `[SerializeField] UIServices _services`, `Initialize(theme, services)` propagation per toast, `DismissRequested` subscribe/unsubscribe lifecycle. ⚠️ **Breaking for Group 0 buyers** — existing `ToastManager` references in scene need new `_services` slot wired (migration note).
- **EditMode tests**: `Tests/Editor/NotificationToastTests.cs` (5 tests: Bind null defaults, duration override fallback, duration override honored, idempotent DismissNow, Bind resets listeners). **All green: 5/5 (43/43 total).**
- **Pending**: spec (`Documentation~/Specs/Catalog/NotificationToast.md`), prefab + Demo scene, CATALOG.md row link. Closes at end of Group A → bumps minor to `0.4.0`.

### Group A — element 1/4: ConfirmPopup (closed, code+tests+spec)
- **Catalog asmdef** `KitforgeLabs.MobileUIKit.Catalog` (refs Runtime + DOTween.Modules + TMP).
- `Runtime/Catalog/Popups/Confirm/`: `ConfirmTone` (Neutral/Destructive/Positive), `ConfirmPopupData`, `ConfirmPopup` (`UIModule<TData>`), `UIAnimConfirmPopup` (`IUIAnimator` + DOTween).
- **Architectural decisions closed**:
  - Q1 — `UIModuleBase.Initialize(theme, services)` virtual injection. PopupManager/UIManager wire on instantiate. No more `GetComponentInParent`. Testable without scene.
  - Q2 — `UIModuleBase.DismissRequested` event + `RaiseDismissRequested()` protected. Popups no longer hold a manager ref. PopupManager subscribes on resolve, unsubscribes on destroy.
  - Q3 — `IUIAudioRouter` + `UIAudioCue` enum (None/PopupOpen/PopupClose/ButtonTap/Success/Error/Notification). Slot in `UIServices`. Popups call `Services?.Audio?.Play(cue)` null-safe.
- **Race-condition hardening**: `UIModuleBase.IsDismissing` (protected set) elevated to base. Guards double-click confirm/cancel, back press during hide, backdrop spam.
- **Event-leak fix**: `ConfirmPopup.Bind` resets `OnConfirmed`/`OnCancelled`/`OnDismissed` before re-bind.
- **Null-safe Bind**: `Bind(null)` → `new ConfirmPopupData()`. `OnShow` calls `Bind(null)` if `_data == null`.
- **Tunable**: `UIAnimPreset.HideScaleTo` (default `0.9f`) replaces hardcoded scale in animator.
- **TA polish**: `[Serializable] private struct Refs` with `[Tooltip]` per field. Animator tooltips clarify `_card` is the scaled rect.
- **`[RequireComponent(typeof(UIAnimConfirmPopup))]`** + lazy animator resolve (covers prefab + dynamic `AddComponent` paths).
- **EditMode tests**: `Tests/Editor/KitforgeLabs.MobileUIKit.Catalog.Tests.asmdef` + `ConfirmPopupTests` (5 tests: Bind null, back-press routing with/without cancel, back-press during dismiss, Bind resets listeners). **All green: 5/5 (38/38 total).**
- **Spec**: `Documentation~/Specs/Catalog/ConfirmPopup.md` (full micro-spec: DTO, services, events, animation, theme, edge cases, QA scenarios, file layout).
- **CATALOG.md**: ConfirmPopup row links to spec; single-button alert mode documented.
- **Pending (Editor manual)**: `ConfirmPopup` prefab + Demo scene entry under `Samples~/Catalog/`. Will close at end of Group A together with Toast/Pause/Tutorial → bumps minor to `0.4.0`.

## [0.3.0-alpha] — 2026-05-01

### Session — 2026-05-01 (cont. 6) — Phase 2 re-scoped + Group 0 foundation
GOAL: Re-scope Phase 2 to deliver the prefab catalog (15 mid-core mobile UI elements) the kit name promises. Build Group 0 foundation (F1-F8) before any visible element.
DONE:
- **Spec**: `Documentation~/Specs/CATALOG.md` (15 elements decomposed into Screens / Popups / Transient+HUD; 10 plug-and-play MUST + 8 MUSTN'T contracts; 5-group build order).
- **F5 Animation system**: `Runtime/Animation/` — `UIAnimStyle` enum (10 styles + Custom), `UIAnimEase` enum (17 easings, DOTween-mappable), `UIAnimChannel` enum, `UIAnimPreset` SO (per-channel duration/ease/overshoot + button-feedback tokens), `UIAnimPresetLibrary` SO (style → preset map with fallback), `IUIAnimator` interface (Runtime DOTween-free).
- **F6 Theme extended**: `UIThemeConfig` adds 10 sprite slots (panel/button/backdrop/divider/icons), 6 audio slots (button-click/popup-show/popup-hide/success/error/notification), `MinTouchTarget`, `DefaultAnimStyle`, `AnimPresetLibrary` ref.
- **F4 Theme exposure**: `PopupManager` now exposes `Theme` getter (mirrors `UIManager.Theme` pattern).
- **F3 Service binding**: `Runtime/Services/UIServices.cs` — single MonoBehaviour container with Inspector slots for all 6 services (Economy/PlayerData/Progression/ShopData/Ads/Time) + runtime setters for DI interop. `UIManager` exposes `Services` getter.
- **F1 Toast layer**: `Runtime/Toast/` — `UIToastBase`, `UIToast<TData>`, `ToastManager` (priority-less, auto-dismiss via Coroutine, `_maxConcurrent` cap with pending queue, `WaitForSecondsRealtime`).
- **F2 HUD layer**: `Runtime/HUD/UIHUDBase.cs` — abstract base for HUD elements with `OnEnable`/`OnDisable` Subscribe/Unsubscribe pattern + Refresh().
- **F7 SafeArea**: `Runtime/SafeArea/SafeAreaFitter.cs` — applies `Screen.safeArea` per-edge (configurable), polls `Update()` for orientation/resolution changes.
- **F8 Editor validator**: `Editor/Validation/UIKitValidator.cs` — menu item `Kitforge/UI Kit/Validate Active Scene` + `IPreprocessBuildWithReport` hook. Checks null fields and duplicate types in registries; aborts build on errors.
- `UIModuleBase`: virtual `AnimStyleOverride` property for per-element style override.
DECISIONS:
- 10 animation styles (Snappy/Bouncy/Playful/Punchy/Smooth/Elegant/Juicy/Soft/Mechanical/Cinematic) selected via Theme default + per-Module override; Disney Getaway Blast = Playful default.
- `UIAnimPreset` as ScriptableObject (buyer-creatable), library SO (`UIAnimPresetLibrary`) maps style→preset for one-step swap.
- Runtime asmdef stays DOTween-free; catalog asmdef will reference DOTween. `IUIAnimator` uses Action callbacks, not Tween return values.
- Service binding pattern locked = Option B (UIServices container, Inspector-driven, DI-optional).
- Validator built as Editor-side scan + pre-build hook (no runtime cost).
PENDING:
- Open Unity to generate `.meta` files for all new scripts.
- Run EditMode tests to confirm no regression on the 33 existing tests.
- Group 0 demo sample (`Samples~/Catalog_Group0_Foundation/`) + EditMode tests for new components (UIServices/ToastManager/SafeAreaFitter).
- Build the 10 default `UIAnimPreset` SO assets per styles table.
- `_tween` / `_tween-dev` agent update to consume `UIAnimPreset` (deferred — not a code task here).
REFS: `Runtime/Animation/*`, `Runtime/Toast/*`, `Runtime/HUD/*`, `Runtime/SafeArea/*`, `Runtime/Services/UIServices.cs`, `Runtime/Theme/UIThemeConfig.cs`, `Runtime/Core/PopupManager.cs`, `Runtime/Core/UIManager.cs`, `Runtime/Core/UIModuleBase.cs`, `Editor/Validation/UIKitValidator.cs`, `Documentation~/Specs/CATALOG.md`

### Session — 2026-05-01 (cont. 5) — Phase 1.5 deferred NOTEs cleanup
GOAL: Cerrar deuda diferida de Phase 1.5 sin abrir Phase 2.
DONE:
- `UIRouter.Initialize()`: nuevo método público idempotente. `Start()` ahora delega en `Initialize()`. Tests dejan de usar reflection sobre `Start` privado — llaman `_router.Initialize()` directo. Añadido `IsInitialized` getter.
- `UIRouterTests`: +1 test `Initialize_CalledTwice_IsIdempotent` (total UIRouter = 10, total Phase 1.5 = 33).
- README arch decision #10: documenta el patrón `protected override` para `BindUntyped` en derivaciones cross-assembly. Cierra el bug latente del sample que el `/_checker as dev` detectó.
DECISIONS:
- `Initialize` idempotente con flag `_isInitialized` — evita doble-disparo de `OnStateChanged` si Awake/Start corre y además llamamos manual.
- `Start()` se mantiene como `private void Start() => Initialize()` para no romper escenas existentes que dependen del ciclo Unity.
PENDING:
- `git push origin main --tags` para subir Phase 1 + Phase 1.5 al remoto.
- Definir alcance Phase 2 con `planner` skill.
REFS: `Runtime/Core/UIRouter.cs`, `Tests/EditMode/UIRouterTests.cs`, `README.md`

### Session — 2026-05-01 (cont. 4) — UIManager EditMode tests (Phase 1.5 close)
GOAL: Cubrir UIManager con EditMode tests siguiendo el mismo patrón que UIRouter/PopupManager. Cierre de Phase 1.5.
DONE:
- `Tests/EditMode/UIManagerTests.cs`: **12 tests** — Push (first/second/cache reuse/missing prefab), Pop (happy/empty/last), Replace (empty stack/swap/missing prefab no corruption), PopToRoot (multi/single).
- Setup pattern: GameObject inactivo durante `AddComponent<UIManager>()` + `SetField` reflection + `SetActive(true)` al final → evita que `Awake` corra con `_themeConfig == null` (que loggearía error y rompería los tests con `Unhandled log message`).
- `_themeConfig` inyectado como `ScriptableObject.CreateInstance<UIThemeConfig>()` y destruido en TearDown.
- LogAssert.Expect en los 2 tests de fallo de prefab (Push + Replace) — valida el contrato de error documentado en el README.
- **Total Phase 1.5 EditMode tests: 9 (UIRouter) + 11 (PopupManager) + 12 (UIManager) = 32.**
DECISIONS:
- NO se extrajo el test harness compartido (NOTE del checker dev). Los 3 archivos comparten `SetField` + el patrón de fakes pero la duplicación es de ~6 líneas por archivo. Extraer ahora sería over-engineering — la regla "rule of three" ya se cumple (3 archivos), pero los helpers son tan triviales que el coste cognitivo de un harness compartido (otra asmdef? otra clase abstracta? generic constraints?) supera al de copiar 6 líneas. Si llega un 4º archivo de tests con el mismo patrón, extraer entonces.
- `Replace_MissingPrefab_DoesNotPopExistingTop` valida explícitamente la decisión Phase 1 #3 del code-doctor (Replace<T> resuelve incoming antes de Pop). Es el test más importante del suite — protege contra regresión del bug original.
PENDING:
- Tag `v0.2.0-alpha` (espera validación real en Unity Test Runner).
- Refactor `UIRouter.Start` → `Initialize()` público (NOTE diferido).
- README arch decision #10 sobre `protected override` derive pattern (NOTE diferido).
- Validación real en Unity Test Runner sigue pendiente (MCP offline).
REFS: `Tests/EditMode/UIManagerTests.cs`

### Session — 2026-05-01 (cont. 3) — `/_checker as dev` follow-ups
GOAL: Aplicar 2 FIX NOW + 1 NOTE detectados por `/_checker as dev` sobre el primer pase de Phase 1.5.
DONE:
- **FIX NOW**: `Samples~/Quickstart/QuickstartBootstrap.cs` — `QuickstartScreen.BindUntyped` y `QuickstartPopup.BindUntyped` cambiados de `protected internal override` a `protected override`. Mismo bug latente que `Tests/EditMode`: cruzando assembly boundary, `protected internal` se ve como `protected` y CS0507 explota. No lo cazamos antes porque `Samples~/` con tilde no se compila hasta que el buyer importa el sample — habría reventado en máquina del cliente.
- **FIX NOW**: `PopupManagerTests` +2 tests — `IsShowing_TypeNotPresent_ReturnsFalse` (false case del API más usado) y `Show_TypeWithoutRegisteredPrefab_LogsErrorAndReturnsNull` (con `LogAssert.Expect`, valida el contrato de fallo más probable en producción cuando un dev olvida registrar un prefab). Total tests EditMode: 9+11 = **20**.
- **NOTE aplicado (breaking)**: rename `AppState.Playing` → `AppState.Gameplay`. Vocabulario alineado con `PopupPriority.Gameplay` que usa el resto del kit. 1 archivo Runtime + 1 archivo Tests actualizados (sólo 2 usos en todo el paquete).
- `package.json` bump `0.1.0-alpha` → `0.2.0-alpha` por el rename breaking.
DECISIONS:
- Bump pre-1.0 con `-alpha` mantiene la señal de inestabilidad. El tag `v0.1.0-alpha` se preserva como histórico — el siguiente tag será `v0.2.0-alpha` cuando cerremos Phase 1.5 con UIManager tests.
- `protected override` en samples confirmado como contrato del derive pattern. Pendiente documentar en README → Architecture decisions table como entrada nº 10 (no urgente, cuando toque pasar de alpha).
PENDING:
- UIManager EditMode tests — Phase 1.5 sigue abierta.
- Test harness compartido (extraer reflection helpers `SetField`/`InvokeStart` antes de duplicarlos en UIManagerTests) — NOTE del checker dev.
- Refactor `UIRouter.Start` → método público `Initialize()` para no depender de reflection en tests — NOTE del checker dev, deferrable.
- README entry nº 10 en Architecture decisions table sobre `protected override` derive pattern — NOTE.
- Validación real en Unity Test Runner sigue pendiente (MCP offline).
REFS: `Samples~/Quickstart/QuickstartBootstrap.cs`, `Tests/EditMode/PopupManagerTests.cs`, `Tests/EditMode/UIRouterTests.cs`, `Runtime/Routing/AppState.cs`, `package.json`

### Session — 2026-05-01 (cont. 2) — Phase 1.5 EditMode tests
GOAL: Cubrir UIRouter + PopupManager con EditMode tests (Phase 1.5) y arreglar smell residual en `UIRouter.RestrictPopupsTo`.
DONE:
- `UIRouter.cs`: arreglada firma de `RestrictPopupsTo` — estaba pegada a la llave anterior y sin modificador de acceso (privada de facto). Ahora `public void RestrictPopupsTo(...)` con formato correcto. Bug funcional: la API pública para restringir popups no se podía activar desde fuera.
- `Tests/EditMode/KitforgeLabs.MobileUIKit.Tests.EditMode.asmdef`: nueva asmdef Editor-only con `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, referencias a `KitforgeLabs.MobileUIKit` + `nunit.framework.dll` + Test Runner.
- `UIRouterTests`: 9 tests — TransitionTo happy/idempotente/event payload, re-entrancy guard desde callback, IsValidPopup (null type, sin restricciones, con allow-list, null collection bloquea todo), ClearPopupRestrictions restaura estado abierto.
- `PopupManagerTests`: 9 tests — Show inicial, prioridad orden (Modal sobre Meta), MaxDepth=3 cap, drain de queue tras Dismiss, eviction por prioridad superior, eviction preserva Data y restaura al dismiss, DismissAll limpia activos+pending, DispatchBackPressed delega al topmost, DispatchBackPressed sin activos returns false.
DECISIONS:
- Fakes de popup vía `new GameObject().AddComponent<>()` con subclases internas en el test; `Instantiate` clona OK desde scene objects (no requiere prefab asset). Reflection para inyectar `_popupRoot` y `_popupPrefabs` (SerializeField private).
- EditMode (no PlayMode) — todos los flujos testeables son sincrónicos (no corrutinas).
- `_popupCache` no se purga en `DismissAll` (sólo en `OnDestroy`); test `DismissAll_ClearsActiveAndPending` valida que pending también se limpia re-mostrando el primer popup.
PENDING:
- Tests de `UIManager` (Push/Pop/Replace/PopToRoot) — diferidos: requieren screen prefabs y `UIThemeConfig` SO; mismo patrón que PopupManagerTests pero más boilerplate. Phase 1.5 sigue abierta hasta cubrir UIManager.
- Validación en Unity Test Runner — pendiente de ejecutar (Unity MCP offline en esta sesión). Compilación estática verde.
- NOTES diferidas técnicas (PriorityQueue<>, registry compartido, backdrop fade) — sin cambios.
REFS: `Runtime/Core/UIRouter.cs`, `Tests/EditMode/KitforgeLabs.MobileUIKit.Tests.EditMode.asmdef`, `Tests/EditMode/UIRouterTests.cs`, `Tests/EditMode/PopupManagerTests.cs`

### Session — 2026-05-01 (cont.) — PM checker FIX NOW
GOAL: Cerrar Phase 1 aplicando los 4 FIX NOW del PM checker antes de tag v0.1.0-alpha.
DONE:
- `package.json`: pitch description afilada (router + popup queue + theme), keywords ampliadas (`ugui`, `router`, `theme`), entry `Quickstart` añadida en `samples[]` (orden: Quickstart primero, GameWiring después).
- `README.md` reescrito desde cero: pitch line + Status + 8 Non-goals explícitos + Install (con tabla de prereqs DOTween Pro / UniTask / TMP / VContainer opt-in) + Quickstart 5 pasos + Phase 1 done-criteria checklist + Architecture decisions table de 9 entradas.
- `Samples~/Quickstart/` creado:
  - `QuickstartBootstrap.cs` con `[SerializeField]` UIManager + PopupManager y 4 `[ContextMenu]` (Push / Pop / Show / DismissAll).
  - `QuickstartScreen` y `QuickstartPopup` derivan de `UIModule<object>` (no de `UIModuleBase` directamente — `BindUntyped` es `internal abstract` y no es accesible desde otra asmdef).
  - `KitforgeLabs.MobileUIKit.Quickstart.asmdef` referencia solo `KitforgeLabs.MobileUIKit` (NO VContainer).
  - `README.md` con scene-setup paso a paso (Canvas + ScreenRoot/PopupRoot + UIThemeConfig + 1 Screen prefab + 1 Popup prefab).
DECISIONS:
- `UIModuleBase.BindUntyped` y `UIModule<TData>.BindUntyped`: `internal` → `protected internal`. Smell detectado al escribir el sample (la asmdef del Quickstart no podía heredar de `UIModuleBase` directamente). Cambio de 1 token, no breaking, desbloquea el patrón correcto. Sample refactorizado para derivar de `UIModuleBase` directamente — el patrón `UIModule<object>` solo aplica si quieres pasar payload tipado.
- Versión: bump `package.json` 0.1.0 → `0.1.0-alpha`. Renombrado el `[0.1.0]` histórico del CHANGELOG a `[0.0.1]` para alinear SemVer con el release real.
- Commit final + tag `v0.1.0-alpha` ejecutados localmente (sin push).
PENDING:
- EditMode tests (`UIRouter.TransitionTo`, `PopupManager` priority ordering) — Phase 1.5.
- NOTES diferidas de la sesión previa: `PriorityQueue<>` real para `_pendingQueue`, registry compartido `cache+FindPrefab`, backdrop fade.
- Push a remote diferido a decisión manual.
REFS: `package.json`, `README.md`, `Samples~/Quickstart/QuickstartBootstrap.cs`, `Samples~/Quickstart/KitforgeLabs.MobileUIKit.Quickstart.asmdef`, `Samples~/Quickstart/README.md`

### Session — 2026-05-01
GOAL: Aplicar 3 CRITICAL del code-doctor sobre PopupManager + UIManager y validar Phase 1 con checkers drift + as pm.
DONE:
- Hotfix UIRouter.IsValidPopup corrupto por merge fallido (commit 2175e6f, único commit de la sesión).
- code-doctor sobre PopupManager.cs + UIManager.cs: 3 CRITICAL + 2 MINOR + 1 SUGGESTION aplicados.
- PopupRecord unificado guarda Data → eviction preserva estado al re-encolar.
- UIManager.Replace<T> resuelve incoming antes de Pop (no corrompe stack si registry miss).
- OnDestroy en UIManager y PopupManager → OnHide() a módulos cacheados (libera tweens).
- MAX_DEPTH → MaxDepth; using System; añadido en PopupManager.
- PopupEntry + PopupRequest colapsados en PopupRecord único.
- /_checker drift y /_checker as pm ejecutados (veredictos 🔴 moderado / 🔴).
PENDING:
- Aplicar 4 FIX NOW del PM checker: pitch line en package.json, README completo (pitch + Non-goals + Quickstart + Done criteria + Architecture decisions), Samples~/Quickstart/ con QuickstartBootstrap.cs + asmdef + README.  ← NEXT
- EditMode tests (UIRouter.TransitionTo, PopupManager priority ordering) — Phase 1.5.
- Tag v0.1.0-alpha.
- Commit final batched de toda la sesión post-2175e6f.
- NOTES diferidas: PriorityQueue<> real para _pendingQueue, registry compartido cache+FindPrefab, backdrop fade.
DECISIONS:
- Política: 1 commit final por sesión (no per-fix). Excepción única ya consumida en el hotfix UIRouter.
- VContainer = opt-in vía Sample, NO dependencia del Runtime asmdef. Falta formalizar en README.
- ChangeLog del producto vive en el propio paquete UPM (portabilidad), no en KITforg_labs externo.
REFS: Runtime/Core/PopupManager.cs, Runtime/Core/UIManager.cs, Runtime/Core/UIRouter.cs, package.json, README.md

### Added
- Phase 0 scaffolding: package.json, folder layout (Runtime/Editor/Samples~/Documentation~).
- Samples~/GameWiring skeleton with VContainer LifetimeScope and 6 stub services.
- UIRouterStub that logs AppState transitions without invoking services.
- Phase 1: UIThemeConfig ScriptableObject + Editor inspector con color preview.
- Phase 1: UIModuleBase + UIModule<TData> con BindUntyped interno.
- Phase 1: UIManager (Push / Pop / Replace / PopToRoot, prefab cache, registry validator).
- Phase 1: PopupManager (priority queue Meta < Gameplay < Modal, eviction con preservación de Data, backdrop, sibling-order sync, MaxDepth=3).
- Phase 1: PopupPriority enum.
- Phase 1: UIRouter (AppState transitions con re-entrancy guard, popup allow-list con flag explícito, back-button dispatch).
- Phase 1: 6 service interfaces (IEconomyService, IPlayerDataService, IProgressionService, IShopDataProvider, IAdsService, ITimeService) + 4 DTOs.
- Phase 1: OnDestroy cleanup en UIManager y PopupManager.

### Changed
- PopupManager: PopupEntry + PopupRequest unificados en PopupRecord único.
- PopupManager: MAX_DEPTH → MaxDepth (UNITY_RULES const naming).
- UIManager.Replace<T>: valida prefab incoming antes de Pop — no corrompe stack si Push fallaría.
- **BREAKING**: `AppState.Playing` renombrado a `AppState.Gameplay` para alinear vocabulario con `PopupPriority.Gameplay`. `package.json` bumped `0.1.0-alpha` → `0.2.0-alpha`.

### Fixed
- UIRouter.IsValidPopup: bloque corrupto por merge fallido reparado (commit 2175e6f).
- PopupManager eviction: re-encolaba con Data=null perdiendo estado del popup desalojado.
- UIRouter.RestrictPopupsTo: firma sin modificador de acceso pegada a la llave anterior — la API pública para activar la allow-list no era invocable. Restaurada como `public void RestrictPopupsTo(IEnumerable<Type>)`.
- `Samples~/Quickstart/QuickstartBootstrap.cs`: `QuickstartScreen.BindUntyped` y `QuickstartPopup.BindUntyped` cambiados de `protected internal override` a `protected override`. Bug latente que sólo reventaba en máquina del buyer al importar el sample (Samples~/ con tilde no compila en CI del paquete).

### Tests
- Phase 1.5: `Tests/EditMode/` con `KitforgeLabs.MobileUIKit.Tests.EditMode.asmdef` (UNITY_INCLUDE_TESTS).
- `UIRouterTests`: 9 tests cubriendo TransitionTo (happy / idempotente / event payload / re-entrancy guard) y popup allow-list (null type, sin restricciones, con allow-list, null collection, ClearPopupRestrictions).
- `PopupManagerTests`: 11 tests cubriendo Show inicial, prioridad, MaxDepth=3, drain de queue, eviction con Data preserved, DismissAll, DispatchBackPressed, IsShowing false-case, Show con tipo sin prefab registrado (LogAssert).
- `UIManagerTests`: 12 tests cubriendo Push (first/second/cache reuse/missing prefab), Pop (happy/empty/last), Replace (empty stack/swap/missing prefab no corruption), PopToRoot (multi/single). Patrón GameObject inactivo durante AddComponent → field injection → SetActive(true) para evitar Awake con theme nulo.

## [0.1.0-alpha] - 2026-05-01

### Added
- Phase 1 closure: full Runtime (`UIManager`, `PopupManager`, `UIRouter`, `UIThemeConfig`, `UIModuleBase` / `UIModule<TData>`, 6 service interfaces + 4 DTOs).
- `Samples~/Quickstart` (zero-dependency) and `Samples~/GameWiring` (VContainer opt-in).
- `README.md` with pitch line, 8 Non-goals, Install + prereqs, 5-step Quickstart, Phase 1 done-criteria, Architecture decisions table.
- `package.json` with sharpened pitch description, expanded keywords (`ugui`, `router`, `theme`), `Quickstart` sample entry.

### Changed
- `UIModuleBase.BindUntyped` and `UIModule<TData>.BindUntyped`: `internal` → `protected internal`. Allows host assemblies (e.g. samples, buyer game code) to derive directly from `UIModuleBase` without forcing the `UIModule<object>` workaround. Non-breaking.

## [0.0.1] - 2026-05-01

- Initial repository skeleton (renamed from `[0.1.0]` to align SemVer with the real Phase 1 release as `[0.1.0-alpha]`).
