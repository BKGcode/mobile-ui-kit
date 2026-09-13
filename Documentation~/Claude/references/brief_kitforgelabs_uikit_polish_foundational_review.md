---
name: brief_kitforgelabs_uikit_polish_foundational_review
description: KF MobileUIKit M3b UIAnim polish layer blocked pending foundational review of kit's UIAnimPopupBase lifecycle vs _tween's UIAnimBase lifecycle. Two failed attempts (M3b sessions 1+2) reverted 2026-05-06 due to wrong-premise patch-on-patch incident.
type: project
status: active
---

# Brief — KF MobileUIKit polish foundational review

## Goal

Identify the actual lifecycle / pattern delta between the kit's `UIAnimPopupBase` (PlayShow-from-OnShow-from-PopupManager) and `_tween`'s `UIAnimBase` (Show-from-OnEnable, RegisterTween auto-link). Validate empirically why `_tween` patterns work in production with simple `Activate/Instantiate + OnEnable Show + _initialDelay-when-needed` while the kit's first M3b polish attempt (DailyLogin item 1/8) only "appeared to work" via accidental cascade-total-exceeds-spike, and second attempt (LevelComplete item 2/8) failed and triggered three escalating workarounds (warmup → init delay → coroutine yield) over the same wrong premise. Once delta is identified, lock a re-validated polish pattern that aligns with `_tween` philosophy across all 5 catalog groups (A-E) and all 8 M3b polish items.

## Scope

**Foundational pieces to review:**
- `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Catalog/_Internal/UIAnimPopupBase.cs` — abstract base for popup animators
- `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Catalog/_Internal/UIAnimScreenBase.cs` — abstract base for screen animators (mirrors popup pattern)
- `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Core/PopupManager.cs` — popup lifecycle (Instantiate → SetActive → Bind → OnShow chain)
- `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Core/UIManager.cs` — screen lifecycle equivalent
- `~/.claude/memory/tween_project_conventions.md` — `_tween` `UIAnimBase` template + Inspector philosophy + Dev API contract
- `~/.claude/memory/tween_patterns_library.md` — `_tween` validated patterns (panel-slide, popup-scale, button-feedback, stagger-list, hud-element)

**Polish items in scope (all blocked):**
- Item 1/8 DailyLogin cell cascade
- Item 2/8 LevelComplete stars cascade + score rollup
- Item 3/8 GameOver CTA cascade
- Item 4/8 RewardPopup icon stagger
- Item 5/8 ShopPopup card cascade
- Item 6/8 HUD-Energy regen juice
- Item 7/8 HUD-Timer ticking pulse
- Item 8/8 NotEnoughCurrency CTA pulse (also has spec-clarify open)

**Catalog groups potentially affected:** A (Confirm/Pause/Tutorial/Toast — frozen `v0.4.0-alpha`, polish only on user request) · B (Reward/Shop/NotEnough/HUD-Coins/HUD-Gems) · C (DailyLogin/LevelComplete/GameOver/HUD-Energy/HUD-Timer) · D (Settings) · E (Loading/MainMenu screens).

## Entry criteria (must be true before resuming polish work)

1. Foundational review session run — minimum 1 dedicated session, NOT mixed with feature work.
2. Empirical Debug.Log instrumentation captured for both ecosystems on cold spawn AND warm spawn, with timestamps for: tween creation, first DOTween Update tick, OnComplete. Compare apples-to-apples.
3. The premise of the original "DOTween spawn-frame race" rejected, modified, or re-confirmed under current evidence — documented as a finding, not as a vibe.
4. New polish pattern explicitly compared against `_tween`'s validated patterns; if patterns diverge, the divergence is justified by a kit-specific constraint (lifecycle, sealed canonicals, `IUIAnimator` contract) and documented.
5. Pattern validated on a single item (recommend item 1/8 DailyLogin OR item 2/8 LevelComplete — both have known reverted state to compare against) with samples-green visual gate on cold spawn AND warm spawn before generalizing.

## Exit criteria (foundational review can close when)

- Empirical evidence captured + analyzed.
- New polish pattern locked, documented in `~/.claude/memory/` (NEW pattern file or extension to `tween_patterns_library.md` if it generalizes).
- Single-item validation green (samples-green on cold + warm spawn).
- Items 2-8 strategy mapped to the new pattern (re-evaluate which items need cascade vs pulse vs no-polish).
- M4 sequencing decision: insert polish session BEFORE M4 hardening, OR ship `v1.0.0-rc` without polish and add it post-1.0 as `v1.1.0-alpha`.

## Resources / cross-references

- `~/.claude/memory/feedback_workflow.md` → "Inherited framing requires empirical re-verification before propagation" (rule capturing the lesson from this incident — sibling to "two-failed-hypotheses → instrument" + "SCRAP-AND-RESTART for misaligned test/infra")
- `~/.claude/memory/pattern_dotween_spawn_frame_warmup.md` — INVALIDATED 2026-05-06; audit trail preserved at the bottom of the file (DO NOT apply the pattern; read only as historical context)
- `./kitforgelabs_mobile_ui_kit_roadmap.md` § Diferidos — M3b polish deferral entry
- Reverted commits' diff (in-flight, never committed) reconstructable from `Packages/com.kitforgelabs.mobile-ui-kit/CHANGELOG.md` `[0.8.1-alpha]` Unreleased revert summary

## Background — incident summary

M3b session 1 (2026-05-06, DailyLogin item 1/8): documented "DOTween spawn-frame race" as root cause of cells fast-forwarding to scale 1 in one frame. Locked workaround as convention in `UIAnimPopupBase` — standalone tweens + `.SetDelay() + .SetLink(KillOnDisable) + .SetUpdate(true) + SpawnFrameWarmup = 1.0f protected const`. Item 1/8 declared "landed" because cascade visually appeared to work — likely accidental, the 7-cell cascade total ~1.6s exceeded the spike, masking the issue.

M3b session 2 (2026-05-06, LevelComplete item 2/8): 3-star cascade (~0.91s total) was too short to mask any spike; first samples-green showed stars instant on cold spawn, correct on subsequent. Three escalating workarounds applied over the same premise — increase warmup → add `_polishInitialDelay` field (per `_tween` convention misread) → coroutine-deferred `yield return null`. Patch-on-patch.

User push-back surfaced the smoking gun: `_tween` ecosystem (multi-project production-tested) handles popup/screen animations on Activate/Instantiate + OnEnable + `_initialDelay`-when-needed without any of these workarounds. If the alleged race were intrinsic to DOTween, `_tween` would suffer it equally. It does not. → original race diagnosis was almost certainly wrong; subsequent patches built on a wrong premise.

Full revert executed 2026-05-06 (this brief written same day): all M3b session 1+2 work reverted to v0.8.0-alpha state, including 3-hook surface in base, builder demo cells, animator wiring. Lesson captured in `feedback_workflow.md`. Pattern doc INVALIDATED with audit trail. Roadmap M3b sessions 1+2 bullets replaced with single incident summary.

**Until this brief is closed, no M3b polish work proceeds.** The kit ships `v1.0.0-rc` without M3b polish, OR resumes M3b polish AFTER foundational review concludes — to be decided when this brief is picked up.
