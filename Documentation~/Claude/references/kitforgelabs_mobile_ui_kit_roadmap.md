---
name: kitforgelabs_mobile_ui_kit_roadmap
description: Strategic roadmap for KF_MobileUIKit Asset Store package — phases, catalog elements, Group sequencing, decisions log, alignment answers.
type: project
---

# KITforge Mobile UI Kit — Roadmap & Planner Workbook

> Living document. Source of truth for product strategy, phases and Phase 2 planning.
> Lives in global memory because the project is a commercial Asset Store target — strategic context survives across workspaces.

---

## ⚠️ CURRENT PHASE — UX review (LOCKED 2026-05-09)

**Phase doc** (read FIRST): [`kitforgelabs_phase_ux_review.md`](./kitforgelabs_phase_ux_review.md). Workshop-drawer test = acceptance gate.

**One-line summary**: kit is technically resolved; this phase reorganizes what exists so the game designer hype-casual workflow (1 prototype/week, 3 hours of UI on Monday) succeeds. NO new features, NO Asset Store work, NO buyer-facing polish, NO marketing-quality screenshots. Until the user explicitly opens the next scope, the filter is: *"¿esto organiza el cajón del usuario que ya está dentro, o pinta el letrero de la tienda?"* — first wins, second deferred.

**Operative gate** for any proposal during M5/RC sessions: see [`feedback_audit_priority_tool_first.md`](./feedback_audit_priority_tool_first.md).

**7 acceptance principles** (criteria, not aspirations): single front door · show don't tell · progressive disclosure · no dead-ends · Unity-native · predictable defaults · persistent context.

**Phase exit**: 7 R-U-C clusters landed + 6 F-UX features landed + user explicitly opens next scope.

---

## Project identity

- **Package**: `com.kitforgelabs.mobile-ui-kit`
- **Path**: `H:\==GIT==\PACHINKO\PACHINKO\Packages\com.kitforgelabs.mobile-ui-kit`
- **Strategic intent**: **Asset Store premium tool** — target buyer is hybrid-casual mobile studios (small/indie, 1-5 devs), price band $30-60.
- **Pitch line**: "Opinionated UGUI router, popup queue and theme contract for hybrid-casual mobile games. Wire it in five minutes, skin it once, ship it."

## Current state (2026-05-10)

| Tag | Hito | Tests |
|---|---|---|
| `v0.1.0-alpha` | Phase 1 — Runtime + Samples + README | 0 |
| `v0.2.0-alpha` | Phase 1.5 — 32 EditMode tests | 32 |
| `v0.2.1-alpha` | Phase 1.5 cleanup — UIRouter.Initialize() + arch #10 | 33 |
| `v0.3.0-alpha` | Phase 2 Group 0 — Foundation (F1-F8) + CATALOG spec + UITK editors + animation indirection collapse (BREAKING) | 33 |
| `v0.4.0-alpha` | Phase 2 Group A — Pure UI catalog (Confirm + Pause + Tutorial + Toast) + Themed contract + builder | **86** (33 framework + 53 catalog) |
| `v0.4.1-alpha` | Hotfix — park broken `Game Wiring` sample (removed from `samples[]`, source preserved) | 86 |
| `v0.5.0-alpha` | Phase 2 Group B — Currency catalog (Reward + Shop + NotEnough + HUD-Coins + HUD-Gems) + UIAnimPopupBase consolidation + 3 in-memory stubs + `Build Group B Sample` builder + chain demo + 5 specs | **119** (33 framework + 86 catalog) |
| `v0.6.0-alpha` | Phase 2 Group C — Progression catalog (DailyLogin + LevelComplete + GameOver + HUD-Energy + HUD-Timer) + IEconomyService v2 BREAKING + HUDCurrency parameterized + 1/3 helpers (RewardFlow.GrantAndShowSequence) + Build Group C Sample builder + 6 chain ContextMenu + 2 sample stubs (InMemoryProgressionService + InMemoryTimeService) + 6 specs. Closes M1 of path to v1.0.0-rc. | **206** (33 framework + 173 catalog) |
| `v0.7.0-alpha` | Phase 2 Group D BREAKING — Player Data catalog (SettingsPopup) + IPlayerDataService rewritten (12-method primitive surface) + IUILocalizationService new (re-skin dispatch) + PlayerPrefsPlayerDataService Runtime + DailyLoginPersistence helper + DailyLogin retro-fit (InMemoryProgressionService opt-in) + RewardFlow.GrantAndShow (capability-gate re-audit) + Build Group D Sample + 2 sample stubs + Game Wiring sample REVIVED (8 stubs, VContainer-gated asmdef) + 3 specs (IPlayerDataService.md / IUILocalizationService.md / SettingsPopup.md). Closes M2 of path to v1.0.0-rc. 8 canonical kit-side keys frozen. | **261** (33 framework + 228 catalog) |
| `v0.8.0-alpha` | Phase 2 Group E BREAKING — Screens catalog (LoadingScreen + MainMenuScreen) + UIAnimScreenBase shared animator base + 8 new UIThemeConfig slots + Build Group E Sample builder + GroupE_BootDemo chain (Loading → MainMenu → DailyLogin auto-trigger via DailyLoginFlow.ShowIfDue) + GroupEDemoHost sample. Bundles **OnUpdate infra dispatch fix** (PopupManager.Update dispatches OnUpdate to active stack — closes latent gap since v0.1.0; RewardPopup.AutoClaim repaired + DailyLoginPopup workaround removed) + **Camera regression fix** (5 builders A→E now generate Main Camera + AudioListener via shared EditorSceneFactory helper) + **PopupManager._backdrop sibling-order fix** (M3.1ter sub-milestone — ApplySiblingOrder ends with SetAsFirstSibling so backdrop renders beneath every popup; closes structural blocker that locked input on Group E demo at the M3 close-out gate). Closes M3 of path to v1.0.0-rc. | **292** (33 framework + 259 catalog: +14 LoadingScreen + 17 MainMenuScreen) |
| `v0.9.0-alpha` | M4 hardening core packaging — 5 Catalog builders (A+B+C+D+E) aligned via `CatalogGroupBuilderShared` (6 constants + 17 helpers, `using static` consume) + ThemedImage runtime contract validated end-to-end across ~30 themed callsites + Theme presets (Default/Casual/Premium) reskin uniformly + QA Suite editor tool (sequential structural integrity scanner, 23 targets clean: 6 demo scenes A-E + ThemePresets + 17 prefabs; pluggable `IQAIntegrityCheck` via `TypeCache`; `Rebuild + Run All` closes regen→validate loop). Includes M4.1 (4 additive UIThemeConfig slots: TertiaryColor + MutedColor + TextOnPrimary + TextOnAccent) + M4.7 (ThemeColorSlot 16 slots, ThemedImage TryResolveColor) + M4.7-bis.A/A.bis/A.ter (3-iteration helper-alignment chapter sealed) + M4.7-bis.B.1-B.5 (Shared extract + B/D/E sweep + runtime override audit) + M4.7-bis.QA (QA Suite). Closes M4 of path to v1.0.0-rc. | **292** (no new tests — QA Suite is editor tool) |
| **M4.X cluster (post-tag, 2026-05-09)** | Audit unification Path-A clean — delete `Editor/QA/` (QA Suite, 8 files) + `Editor/QA/Catalog/` (v1 Catalog Audit prototype, 15 files, never tagged); replace with `Editor/Audit/` unified pipeline rooted at `Tools → Kitforge → UI Kit → Audit`. Pluggable `IUIKitCheck` via `TypeCache` (7 checks: PrefabRefs + PrefabThemeReactivity + PrefabSpecMarkdown order-independent + MissingReference + MissingScript + UIKitManager + **NEW `ThemedFieldsWiredCheck`** locks symmetric backing-field rule). Reports `Library/UIKitAudit/Reports/` (gitignored) + opt-in mirror `Assets/Editor/QAReports/`. Snapshots 1080×1920 PNG SHA256-hashed. Headless mode exit 0/1/2 for CI. **Stage 2 text-theming sweep B/C/D/E** — `CatalogGroupBuilderShared.CreateThemedText` helper + `ThemeBuilderSlots` (9 named tuples) + `ThemedText.TryResolveColor` extended to 16 slots + `AddThemedText` symmetry fix (now wires `_text` SerializedProperty). ~36 callsites migrated, +51 themed counts. **Headless builder pattern** — each `CatalogGroup{A,B,C,D,E}Builder` + `CatalogM41ThemeBuilder` expose `BuildAllForAudit()` silent variant; audit window adds `Regenerate + Audit` + `Regenerate Samples` + `Clear All` toolbar buttons (5-click ritual → 1). Regen sequence locked **A→B→C→D→M4.1→E** (M4.1 between D and E because E consumes M4.1's themes via `Theme_Casual.asset`/`Theme_Premium.asset` GUIDs which `DeleteAsset+CopyAsset` regenerates each run). **Follow-ups same session:** (1) HUDGems audit dedup (`UIKitAuditDiscovery.FindCatalogPrefabsFor` returns `List<string>` with filename-disambiguated labels — catalog count **16→17**, HUDCoins+HUDGems both surface as targets sharing `HUDCurrency` type); (2) `WarningText` callsite migrations via existing `ThemeColorSlot.WarningColor` (DailyLogin already-claimed countdown + HUDEnergy regen countdown — no new slot, Decision Protocol §4 Contrarian rejected speculative `WarningText` slot); (3) runtime theme swap API (`SetTheme` on `UIManager`/`PopupManager`/`ToastManager` re-Initializing cached instances) + GroupE_BootDemo `ThemeSwitcherEScreens` dropdown (E2E manual swap parity with M4.1 `ThemePresetsDemo` — diverges from M4.1's Instantiate-bypass pattern because screens are persistent in `_screenCache`); (4) `Documentation~/QAReports/` package-side release evidence (`_README.md` + baseline `_Summary.md` — buyer-facing `MirrorReportsToAssets` toggle stays orthogonal). **Final:** 23/23 pass · 0 fail · 0 warn (catalog 17 + scenes 6, was 23/23 · 2 warn pre-sweep). No tag yet — folds into `v1.0.0-rc` after M3c UX audit. See CHANGELOG `[Unreleased]`. | 292 (no new tests — audit checks are editor structural validators, not unit tests) |
| **M5.X Hub cluster (post-tag, 2026-05-10)** | Hub v1.0.0-rc baseline shipped — `Editor/Hub/` UIToolkit window rooted at `Tools → Kitforge → Hub` (5/5 functional tabs). **M5.1**: `Runtime/Bootstrap/KitforgeRoot.prefab` + `KitforgeThemeBinder` MonoBehaviour (theme single-field distributes to 3 managers Awake) + `Theme_Default.asset` shipped at `Runtime/Theme/Presets/` + `BuildPaneFor` placeholder pre-Hub-window. **M5.2.A**: `KitforgeHubWindow` UIToolkit shell + `KitforgeHubState` ScriptableObject persistence (ActiveTab/SelectedCatalogKey/SelectedThemeKey + IsPersisted) + `KitforgeHubAutoOpen` `[InitializeOnLoad]` + EditorPrefs first-open marker + Setup wizard 3-step (Initialize/AddSceneRoot/HelloWorld). **M5.3.A-D**: Catalog tab — `KitforgeCatalogEntry` POCO + `KitforgeCatalogRegistry` (17 hardcoded entries per D3 LOCK) + grid + detail pane with DTO field list + per-pattern spawn snippet TextField + drag-to-scene callbacks (HUD pattern only) + `KitforgeCatalogPrefabResolver` folder-scoped lookup with filename disambiguation (HUDCoins/HUDGems). **M5.4.A**: Theme tab — `KitforgeThemeStudio` AssetDatabase.FindAssets("t:UIThemeConfig") dropdown (themes ilimitados invariant honored — never hardcoded 3-cap). **M5.5.A-C**: Test tab — Play-mode-gated scrollview + `KitforgeMockDtoFactory` (Activator.CreateInstance + ApplySmartDefaults reflection on Title/Subtitle/Message) + reflection-based spawn (PopupManager.Show / UIManager.Push / ToastManager.Show<T> generic) + Force scenarios (DTO-override pattern preserves API freeze: 4 scenarios DailyLogin Day 1 / NotEnoughCoins 500 / GameOverWithAd / LevelComplete 3-stars new best) + `EditorWindow.ShowNotification` toast feedback per D4 LOCK + spawn-failure truthfulness (PopupManager.Show returns null on no-prefab without throwing → ternary on Invoke result). **M5.6.A-B**: Help tab — `KitforgeHelpTab` resolves `Documentation~/CHEATSHEET.md` via `PackageInfo.FindForAssembly().resolvedPath` + File.ReadAllText (LoadAssetAtPath<TextAsset> silently returns null because `~`-suffix excludes from AssetDatabase) + line-by-line markdown classifier (heading / quote / table / code-fence / paragraph) + 3-action toolbar (Open in editor / Run Audit / Build Group A Sample) + ToC chips with `ScrollView.ScrollTo` + clickable markdown links via compiled LinkRegex (relative paths → `EditorUtility.OpenWithDefaultApp` resolved against package path; http/https → `Application.OpenURL`; tooltip on hover) + selectable code blocks via TextField multiline read-only. **M5.7**: verify-all-docs gate — caught 2 hallucinations (README §Non-goal #6 "No Editor authoring window" claim contradicted by shipped Hub → reworded; 3 dead methods in HubWindow.cs `BuildPlaceholderPane`/`GetTabSubtitle`/`GetTabMilestone` with stale "Coming in M5.X" strings now unreachable → deleted, fallback LogErrors instead of lying). **API hallucinations caught + locked**: 4 across cluster — `UIManager.Show` (real = Push), `ToastManager.Show` non-generic (real = Show<T>), `PopupManager.RegisterPrefab` (no such API), README "No Editor authoring window" (Hub IS one). New memory rule: `feedback_library_api_verification.md` — grep source before any doc/code mentions a library symbol. New pattern memory: `pattern_documentation_tilde_access.md` — `~`-folder access from Editor C#. **Cluster scope NOT done**: Hub column complete (F-UX-1→F-UX-6 LANDED) BUT Runtime simples column partial — R-U-C3 HUDSimple/HUDEnergySimple/HUDTimerSimple pendiente (no shipped) + R-U-C5 multi-font slots `_titleFont`/`_bodyFont`/`_labelFont` pendiente + R-U-C7 SafeAreaFitter.cs pendiente. Phase exit criteria (`kitforgelabs_phase_ux_review.md:53`) NOT yet met → blocks `git tag v1.0.0-rc.1` until disposition. **Tag-cut deferrals captured in CHANGELOG `[Unreleased]` M5.7 entry**: bump `package.json.version` 0.9.0-alpha → 1.0.0-rc.1 + Hub mention in description + `editor`/`hub` keywords + README status row + Unity smoke test. No tag yet — folds into `v1.0.0-rc.1` after Runtime simples disposition. See CHANGELOG `[Unreleased]`. | 292 (no new tests — Hub is editor tooling) |
| `v1.0.0-rc.1` (2026-05-10) | First release candidate of v1.0 — commit `3b47e04` + tag pushed. Closes M5/RC milestone: Hub 5/5 panes (Setup wizard ✅ M5.2.A · Catalog browser ✅ M5.3.A-D · Theme Studio ✅ M5.4.A · Test launcher ✅ M5.5.A-C · Help tab ✅ M5.6.A-B) + R-U-C3 HUDSimple variant family (HUDSimple/HUDEnergySimple/HUDTimerSimple — service-free counterparts coexisting with canonical service-bound HUDs) + R-U-C5/C7 phase exit closure post-discovery (multi-font INVALIDATED — UIThemeConfig already ships 3 font slots; theme presets MOVED to `Runtime/Theme/Presets/` Theme_Casual + Theme_Premium .assets with stable GUIDs; CatalogM41ThemeBuilder refactored ~50 LOC removed; SafeAreaFitter CONFIRMED ALREADY-SHIPPED at `Runtime/SafeArea/SafeAreaFitter.cs`; README §Non-goal #7 inverted) + Catalog_All_Demo single-import master demo (9th sample, ~250 LOC host + ~350 LOC builder, workshop-drawer test pass) + Quickstart §Manual prefab registration section (closes 4th API hallucination definitively). Public API frozen between rc.N and final v1.0.0; only patch fixes. **Known issue at tag**: Bootstrap Defaults workaround for `[XPopup] No animation preset resolved` warning was claimed to auto-wire Theme_Default — empirically false (post-tag clarification, see post-RC hotfix cluster row). | 292 (no new tests — Hub + master demo are tooling) |
| **`v1.0.0` (2026-05-10, SHIPPED — tag pushed)** | First stable release. Commit `2faf0a0` + annotated tag `v1.0.0` pushed to origin. Promotion from `v1.0.0-rc.2` with NO code, API, or sample changes — release-candidate cycle (rc.1 → rc.2 → 1.0.0) closed clean. Promotion gate: 24/24 UIKit Audit (17 prefabs + 7 scenes incl. AllDemo) PASS + 303 EditMode green. Public API frozen. Install URL pin = `#v1.0.0`. Release evidence: `Documentation~/QAReports/_Summary.md` (latest) + `Documentation~/QAReports/v1.0.0/_Summary.md` (versioned). Buyers upgrading from rc.2: zero migration steps. Phase exit criteria 1+2 LANDED (7 R-U-C clusters + 6 F-UX Hub features); criterion 3 PENDING user decision to open next scope. | 303 EditMode (unchanged from rc.2) |
| `v1.0.0-rc.2` (2026-05-10, SHIPPED — tag pushed) | Post-rc.1 hotfix release — commits `c52ae67` (Bug 2 + Bug 4 + Bug 5) + `dfbd15f` (Bug 6) + `7611eb1` (README drift fix). **Bug 2 Path C**: ships `UIAnimPreset_Playful.asset` inside package at `Runtime/Animation/Presets/` (stable GUID `7adb85a485d243e09fb65e077ad33e67`) + pre-wires Theme_Default/Casual/Premium `_defaultAnimPreset` field (mirrors theme presets stable-GUID pattern) — eliminates `[XPopup] No animation preset resolved` spam OOTB without requiring Bootstrap Defaults. **Bug 4**: `Catalog_All_Demo.ShowDailyLogin` builds 7-entry calendar inline (was `RewardEntries=null` → DailyLoginPopup auto-deactivated). **Bug 5 Catalog UX**: `KitforgeCatalogBrowser` + `KitforgeHubState` extended with group-filter chips (All + A-E, dynamically enumerated from registry — future Group F adds zero code) + name search box (case-insensitive AND with chip) + universal drag-drop (was HUD-only; Popup/Screen/Toast now drop as static preview prefab with detail-panel hint clarifying semantics). 6 new USS classes (`kfh-catalog-left/filter-bar/filter-chip-row/filter-chip + --active/filter-search/empty-results`). Filter state persisted in `_catalogGroupFilter` + `_catalogSearchQuery` (survive Hub close/reopen). **Bug 6**: `CatalogAllDemoBuilder.WireManagerArrays` adds `uiSO.FindProperty("_screenRoot").objectReferenceValue = screenRoot` — pre-existing builder gap latent since rc.1 (PopupRoot/ToastRoot were wired, ScreenRoot was missing). Surfaced via user-driven Regenerate+Audit (1/24 fail), invisible to headless audit menu (which excludes `Scenes` dimension by default — parking-lot improvement). **Triple gate VERDE**: 303/303 EditMode (8.7s) + UIKit Audit 24/24 (17 prefabs + 7 scenes incl. AllDemo) + AllDemo Play smoke validated. Public API unchanged from rc.1; samples additive only. | 303 EditMode (+11 since rc.1: hub state persistence + browser filter logic + AllDemo host calendar) |
| **M5-post-RC hotfix cluster (2026-05-10) — folded into rc.2 above** | Post-rc.1 smoke test in fresh empty Unity scene surfaced 2 real bugs + 1 documentation drift, all landed in CHANGELOG `[Unreleased]`: **(1) Test tab empty-state UX** — `KitforgeTestLauncher` rendered Spawn buttons as enabled-on-Play-mode regardless of whether `KitforgeRoot` existed in scene; clicks dumped rich console error but in-Hub toast was just generic "Spawn blocked: no PopupManager" with zero recovery guidance. Fix: `DetectManagers` polls `FindAnyObjectByType<PopupManager>/UIManager/ToastManager>` per Refresh; banner now 3-state machine (no-managers `--warn` red + 3 inline CTAs [Add Scene Root via reused `KitforgeSetupWizard.AddSceneRootToActiveScene` static / Open Catalog_All_Demo if scene exists / Open Setup tab via new public `KitforgeHubWindow.SwitchToTab`] · Edit+managers neutral · Play+managers green). Per-pattern Spawn enable check (Popup/Screen/Toast each gate independently). New listeners: `EditorApplication.hierarchyChanged` + `EditorSceneManager.sceneOpened` (in addition to existing `playModeStateChanged`). 5 new USS classes. **(2) DailyLogin Force scenario crash** — `RewardEntries is null or empty` aborted both regular Spawn AND Force `DailyLogin · Day 1 first claim` because `KitforgeMockDtoFactory.ApplySmartDefaults` only handled string fields generically; Force `ConfigureDailyLoginDay1` only overrode CurrentDay/Title/etc, assuming factory pre-populated calendar. Fix: `ApplyTypeSpecificDefaults` switch-on-DTO-type populates `DailyLoginPopupData.RewardEntries` with 7-day calendar (Day N = N×100 coins, Day 7 IsBigReward+AllowDouble) and `TutorialPopupData.Steps` with 3 mock steps (preventive — Tutorial doesn't hard-error but renders blank). **(3) rc.1 Known Issue documentation drift** — `### Known issues` claim "Bootstrap Defaults auto-wires `Theme_Default.DefaultAnimPreset`" was empirically false (`DefaultUIAnimPresetsCreator.EnsureDefaultTheme` only creates the asset if missing, never assigns the preset reference). Updated `[Unreleased] ### Documentation` block with actual workaround (manual Inspector drag) + v1.0.0 fix path (ship 1+ `UIAnimPreset.asset` inside `Packages/.../Runtime/Animation/Presets/` + pre-wire 3 ship themes to it via stable GUID, mirroring theme presets pattern — eliminates anim-spam out-of-the-box). **MCP-for-Unity wired** — `.mcp.json` adds HTTP entry pointing at `http://127.0.0.1:8080`; user runs `uvx --from mcpforunityserver==9.6.8 mcp-for-unity --transport http --http-url http://127.0.0.1:8080 --project-scoped-tools` in separate terminal. Restart of Claude Code required for tool registration. New feedback rule `feedback_mcp_token_caution.md` — ALWAYS confirm with user before any `mcp__*` tool call (10-100× more expensive than native). Smoke test PARTIAL — Test tab empty-state + Force scenarios validated; Setup wizard 3-step + Catalog drag-drop + Theme dropdown + Help tab inline render still pending Unity validation loop. | 292 (no new tests — hotfix cluster is editor tooling + mock factory) |
| **M3c UX audit closure (2026-05-09)** | M3c executed pure audit (zero code, zero tag). 4 audit blocks: B1 4-roles original plan (audit history, lente reframed); B2 50 user findings as game designer hype-casual workflow (1 prototipo/semana, no técnico) → 7 P0 clusters (drop & play setup + cheat-sheet & DTO Inspector + HUDSimple sin servicios + Popup Test Launcher + Theme polish + Validation pre-Play + SafeArea built-in); B3 50 UX findings + Kitforge Hub design from scratch (single Editor window, 5 tabs UIToolkit) → **7 UX Pillars LOCKED** (P1 Single front door · P2 Show don't tell · P3 Progressive disclosure · P4 No dead-ends · P5 Unity-native · P6 Predictable defaults · P7 Persistent context) + **7 Hub features driven** (F-UX-1 Hub window · F-UX-2 Setup wizard 3-step · F-UX-3 Catalog Browser · F-UX-4 Theme Studio · F-UX-5 Popup Test Launcher · F-UX-6 Communication actionable · F-UX-7 Polish post-RC); B4 30 dev validation findings → **plan aterrizaje 7 sub-milestones M5.1-M5.7 ~10-11 sessions**. **4 critical decisions LOCKED**: D1 Hub in v1.0.0-rc · D2 UIToolkit (no IMGUI) · D3 17 kit-only catalog v1.0.0-rc + `[KitforgeCatalogEntry]` API v1.1.0 · D4 `EditorWindow.ShowNotification` text-only standard. **Opción A scope LOCKED** (full Hub + Runtime simples + safe-area + DTO refactor BREAKING). **Workshop-drawer test rule LOCKED** (`feedback_audit_priority_tool_first.md`): tool reliability + prefab practicality + reorganization first; store/demos/buyer-docs LAST until user opens scope. Full audit: `./kitforgelabs_ux_audit_2026-05.md`. | 292 (no new tests — audit) |

- Group C closed end-to-end 2026-05-04 (commit `9d2b4c5`, tag `v0.6.0-alpha` pushed): 5 catalog elements + 5 DTOs + 4 enums (CurrencyType.Energy, ContinueMode, BackPressBehavior, TimerMode) + DailyLoginRewardEntry POCO + RewardFlow helper + builder + scene + 6 chain ContextMenu + 9 individual + 6 HUD ContextMenu + 206 tests green on fresh compile + IEconomyService v1→v2 migration + HUDCurrency replaces HUDCoins/HUDGems. C3 PM Import flow validated end-to-end (`Assets/Samples/Kitforge Mobile UI Kit/0.6.0-alpha/` materialized + Library/ScriptAssemblies wiped pre-test). 96-file commit (+6387 / -440) per Group B precedent (single commit per tag).
- Group B verificado end-to-end 2026-05-02 (commit `da36a56`, tag `v0.5.0-alpha` pushed): 5 elementos catalog + 5 DTOs + 1 cell view + builder + escena + chain demo + **19 ContextMenu** triggers + 119 tests verdes + chain Shop→NotEnough→Ad→Reward funcional in-Editor.
- 4 fixes durante la verificación in-Editor (todos en `### Fixed` de CHANGELOG `[0.5.0-alpha]`): (a) CS0102 duplicate field/using en `CatalogGroupBDemo.cs`, (b) `FakeShopDataProvider` helper + `ShopPopupTests.SetUp` con `UIServices.SetShopData` injection, (c) `NotEnoughCurrencyPopupData` defaults aligned to spec N5 (`ShowDecline=false, CloseOnBackdrop=true`), (d) `[DefaultExecutionOrder(-100)]` en `UIServices` para garantizar Awake-before-OnEnable.
- `UIAnimPopupBase` extraída en `Runtime/Catalog/_Internal/`: 3 animator scripts del Group B reducidos a stubs `: UIAnimPopupBase { }`. Group A (UIAnimConfirmPopup) intacto — migrarlo requiere user approval.
- `UIHUDBase.SetServicesInternal(UIServices)` protected — elimina la reflection que HUDCoins/HUDGems usaban para inyectar servicios en tests.
- 3 helpers diferidos a Group C (RewardFlow.GrantAndShow, RewardFlow.GrantAndShowSequence, ShopFlow.OpenWithPurchaseChain) — capability-gate falló con sólo 1 callsite en Group B; contratos lockeados via specs.
- 86 EditMode tests verdes (33 framework + ConfirmPopup 5 + PausePopup 15 + TutorialPopup 19 + NotificationToast 14).
- Themed* contract canónico para Groups B-E (`IThemedElement` + `ThemedImage` + `ThemedText` + 4 slot enums; `UIModuleBase`/`UIToastBase` walk children y aplican).
- `Tools/Kitforge/UI Kit/Build Group A Sample` materializa los 4 prefabs + escena Demo bajo `Assets/Catalog_GroupA_Demo/`. Prefabs NO se shippean — el builder es el path canónico.
- Bootstrap Defaults idempotente: 10 UIAnimPreset + Theme con Playful pre-cableado en 1 click.
- `origin/main` totalmente sincronizado tras 2026-05-10 (13 tags pushed: v0.1.0-alpha → v0.8.0-alpha → v0.9.0-alpha → v1.0.0-rc.1 → v1.0.0-rc.2 → **v1.0.0**).
- **M3b sessions 1+2 REVERTED 2026-05-06 — wrong-premise patch-on-patch incident.** Both polish implementations (DailyLogin item 1/8 cell cascade + LevelComplete item 2/8 stars cascade & score rollup) were built on a misdiagnosed "DOTween spawn-frame race" premise locked in session 1. When item 2/8 first samples-green failed, escalating workarounds (warmup constant → `_polishInitialDelay` field → coroutine-deferred yield) were stacked rather than questioning fundamentals. User push-back surfaced that `_tween` ecosystem (production-tested) handles the same domain with simple `Activate/Instantiate + OnEnable Show + _initialDelay-when-needed` — no warmups, no yields. If the race were intrinsic to DOTween, `_tween` would suffer it. It does not. **Reverted scope:** `UIAnimPopupBase` 3-hook surface (`PlayPolishShow`/`ResetPolishToStart`/`SnapPolish`) + `SpawnFrameWarmup` constant + convention block; `UIAnimDailyLoginPopup` cascade impl; `UIAnimLevelCompletePopup` cascade + rollup impl; `CatalogGroupCBuilder.BuildDailyLoginPopup` 7 demo cells + helpers (`BuildDailyLoginDemoCells`, `StretchTopHalf`, `StretchBottomHalf`) + animator wiring; `CatalogGroupCBuilder.BuildLevelCompletePopup` animator wiring; `LevelCompletePopup.OnShow` SetScoreTarget seam; `CHANGELOG.md` `[0.8.1-alpha]` Unreleased content; `pattern_dotween_spawn_frame_warmup.md` marked INVALIDATED with audit trail preserved. Lesson captured in `feedback_workflow.md` → "Inherited framing requires empirical re-verification before propagation" (sibling rule to "two-failed-hypotheses → instrument" + "SCRAP-AND-RESTART for misaligned test/infra"). **Pending before re-attempt:** foundational review of kit's `UIAnimPopupBase` lifecycle vs `_tween`'s `UIAnimBase` lifecycle (PlayShow-from-OnShow vs Show-from-OnEnable, Sequence wrappings, manual SetLink vs RegisterTween auto-link); empirical Debug.Log instrumentation cold-spawn vs warm-spawn in both ecosystems; revisit polish strategy across all 8 M3b items + all 5 Catalog groups (A-E) to align with `_tween` validated patterns. State of Group A through E catalog code itself: untouched by the revert (all M1-M3 catalog work intact); only the polish layer + hook surface added in M3b was reverted.

## Diferidos / próximos hotfixes

- ~~**M4.7-bis.A reconciled (2026-05-06): Group A divergence resolved; B/D/E sweep + base extraction pending M4.7-bis.B.**~~ — **CLOSED 2026-05-08 at `v0.9.0-alpha`.** All 5 builders aligned via `CatalogGroupBuilderShared` extract (path pivot: `Shared` static utility + `using static` instead of `Base` abstract class — C# disallows static-class inheritance, idiomatic Unity editor pattern). ~30 themed callsites reskin uniformly across Default/Casual/Premium. QA Suite editor tool ships as M4.7-bis.QA (replaces manual master-samples-green ritual). Deferred to M4.X cluster (post-tag): ~~text-theming sweep (`text.color = constant` → `AddThemedText` across 5 builders, surfaces in Premium dark theme contrast catastrophe — B.3 visual finding)~~ **CLOSED 2026-05-09 (M4.X Stage 2)** via new `CreateThemedText` helper + `ThemeBuilderSlots` (9 named tuples) + `AddThemedText` symmetric fix + `ThemedFieldsWiredCheck` audit lock; ~36 callsites migrated B/C/D/E + Group A pre-existing; final 23/23 pass · 0 fail · 0 warn; HUDCurrency/HUDEnergy/HUDTimer reactive (Q2.A vote). Still open: `ActiveStateColor`/`HandleColor` slot evaluation; `LoadingBg`/`MainMenuBg` dedicated slots if `BackgroundDark` reuse turns too generic; `SliderTrack`/`ToggleBg` if `MutedColor` reuse turns too generic; add theme dropdown to `GroupE_BootDemo` mirroring `ThemePresetsDemo` (E manual swap E2E coverage); Bootstrap Defaults palette research (assign `Theme_Default._defaultAnimPreset` post-`UIAnimPreset` SO generation); `WarningText` slot for HUDCapLabel/RegenLabel (Q4 deferral); HUDGems audit dedup fix (component-type identity vs prefab-path); snapshot diff Casual↔Premium hash≠ check. **Lessons captured in `feedback_workflow.md`** (i-v from kickoff plan): post-discovery sync gate, extract-from-divergent-consumers anti-pattern (recursive across 3 layers — helper/wrapper/composer), AssetDatabase ref lifecycle around `EditorSceneManager.NewScene`, runtime overrides silently kill theme contract. **Discovery during M4.7-bis kickoff** (Read-audit pre-sweep): `CatalogGroupABuilder` was already migrated to `ThemedImage` runtime read at an earlier session, but with a divergent helper signature `AddThemedImage(go, ThemeSpriteSlot, ThemeColorSlot)` void (vs Group C's M4.7 `AddThemedImage(go, Color, ThemeColorSlot)` returns Image). M4.7-bis split into A (reconcile, this milestone) + B (sweep B/D/E + extract base, 2-3 sessions ahead) per `/_checker as triple` post-discovery. **Landed in M4.7-bis.A:** (a) canonical helper `AddThemedImage(GameObject go, Color fallbackColor, ThemeSpriteSlot spriteSlot, ThemeColorSlot colorSlot)` returns Image, with explicit `_image` SerializedObject wiring; (b) Group A + Group C builders refactored to canonical signature; (c) Group A `CreateSecondaryButton` reverted to non-themed (`slot=None`) matching Group C's deferral pattern; (d) Tutorial `stepImage` migrated to `ThemeColorSlot.BackgroundLight`; (e) Theme_Default `_backgroundLight` palette drift `(0.49, 0.49, 0.49)` reset to `(0.96, 0.97, 0.98)` — root cause: manual inspector edit pre-M4.1 (asset predates M4.1 slot additions; non-deterministic float + grayscale = slider drag pattern); (f) CHANGELOG drift fix (line 24 acknowledged Group A pre-migration). **Caveat:** Group A Pause-popup `CreateThemedLabelledButton` callsites with `ThemeColorSlot.SecondaryColor` slot (5 buttons lines 178-182) NOT reverted in M4.7-bis.A — same Secondary palette issue but uses different helper; deferred to M4.7-bis.B inline alongside D6. **Pending M4.7-bis.B (mandatory pre-tag `v0.9.0-alpha`):** (1) sweep `CatalogGroupBBuilder` / `CatalogGroupDBuilder` / `CatalogGroupEBuilder` with canonical helper; (2) extract shared helpers to `internal abstract class CatalogGroupBuilderBase` (composability §5 now validated — 2 aligned consumers); (3) decide D6 Group B header tints semantic mapping inline (ClaimTint→`SuccessColor`, NotEnough.HeaderTint→`WarningColor`); (4) audit `\.image\.color\s*=` runtime overrides post-sweep; (5) revert Pause Secondary slot or apply D6 inline-decide. **Deferred M4.X cluster:** Secondary cross-theme palette reconciliation (Δ 0.5 vs hardcoded fallback) + `Theme_Default._defaultAnimPreset` null + Bootstrap Defaults palette research + Loading/MainMenu screen backgrounds themed-or-not (D5). **Lessons to capture at `/_close`:** (i) post-discovery sync gate (memory-rot pattern when source contradicts memory); (ii) extract-from-divergent-consumers anti-pattern (philosophy §5 corollary); (iii) capability-gate revision when discovery invalidates premise; (iv) AssetDatabase ref lifecycle around `EditorSceneManager.NewScene` (already in M4.7); (v) Runtime overrides silently kill theme contract (already in M4.7).
- **Theme folder convention reconciliation pending** (v1.2.0+ post-tag, surfaced 2026-05-12 during Hub UX session) — Hub "New Theme" button hardcodes `Assets/Settings/Themes/Theme_New.asset` (kit commits `6b22285` + `9fbbc62` on `feat/v1.2.0-catalog-prefabs`). Violates `kitforgelabs_studio_conventions.md` line 54 ("Never scatter top-level folders ... `Assets/Settings` ..."). User accepted intentionally during `/_close` — *"es igual, para este proyecto usaremos Assets/Settings/Themes"*. Two resolution paths to evaluate: (a) **convention update** — add buyer-owned-content carve-out documenting `Assets/<topic>/` as acceptable for user-created assets (themes, custom configs, profile overrides) outside the kit namespace; (b) **kit-side revert** — change `DefaultThemeFolder` constant in `Editor/Hub/Theme/KitforgeThemeStudio.cs` to `Assets/KitforgeLabs/UI Kit/Themes/` mirroring `HubState.asset`. Decision pending future kit-session. Audit trail: git commit message of `9fbbc62` documents the deviation explicitly. Tag-gate: do NOT cut `v1.2.1`/`v1.3.0` without resolving — buyer-facing convention divergence is a Triple-gate static-doc finding (per `feedback_workflow.md` "Verify-all-docs gate").
- **M3b UIAnim per-element polish** (items 1-8 + cascade-helper extraction) — **DEFERRED post-`v1.0.0-rc` 2026-05-06**. Sessions 1+2 reverted: wrong-premise patch-on-patch incident. Foundational review of `UIAnimPopupBase` lifecycle vs `_tween` `UIAnimBase` lifecycle pending before any new polish work — see [`brief_kitforgelabs_uikit_polish_foundational_review.md`](./brief_kitforgelabs_uikit_polish_foundational_review.md). Tag `v0.8.1-alpha` cancelled. **Path-to-`v1.0.0-rc` re-sequenced 2026-05-06 per user directive** ("primero los core, luego polish/UX"): `M3a ✅ → M4 (hardening core packaging, tag v0.9.0-alpha) → M3c (UX audit on completed core) → M5/RC (M3c P0 disposition + final gates → tag v1.0.0-rc)`. Polish ships post-1.0 as `v1.1.0-alpha` (or later) once foundational review concludes y se locka un re-validated polish pattern. Individual M3b items pueden promocionarse a M5/RC si M3c P0 audit los flagea — capability-gate per item, no full M3b workstream.
- **Game Wiring sample revival** (post-Group D): re-añadir a `samples[]` cuando `IPlayerDataService` + `SettingsPopup` (Group D) hayan estabilizado los contratos de servicio. Source files preservados en `Samples~/GameWiring/`. Antes: actualizar los 5 stubs (~30 miembros de interfaz nuevos desde v0.1.0).
- Theme presets shipping (`Theme_Casual.asset`, `Theme_Premium.asset`) — TA scenario 2.
- Builder destination detection (overwrite/skip prompt) — TA scenario 4.
- **`CatalogBuilderBase` extraction** (M4 hardening sweep — same window as builder destination prompt above): cuando A/B/C/D builders existan post-Group D delivery, extraer base con helpers compartidos. Candidatos verbatim de `CatalogGroupBBuilder` / `CatalogGroupCBuilder`: `CreateRoot`, `CreateBackdrop`, `CreateCard`, `CreateText`, `CreatePrimaryButton`, `CreateSecondaryButton`, `StretchInside`, `AnchorTopOfCard` + transitive (`CreateChild`, `AddImage`, `CreateButton`) + prefab/animator helpers cuando cada builder los reuse (`SaveAsPrefab`, `WireAnimatorCard`, `ForceButtonHeight`). Decisión locked en B.4.1 close 2026-05-04 (`/_checker drift`) por composability criterion (philosophy #5): extraer cuando 2+ consumers PROBADOS, no anticipados. Hoy A/B taggeados + C en construcción + D/E pendientes — extraer ahora rompería Group B taggeado por blast radius post-tag. M4 sweep es la ventana natural (4 builders shipped = reuso probado). **Hard-deadline confirmado por `/_checker as dev` 2026-05-04** — sin extraction, M2 Group D builder = 4ª copia, M3 Group E = 5ª; drift acumulado.
- **Event signature uniformity en popup catalog** (CATALOG.md doc en M4): `/_checker as dev` 2026-05-04 detectó inconsistencia entre popups Group C: `DailyLogin.OnDayClaimed(int day, RewardPopupData[] rewards)` (tuple-style) vs `LevelComplete.OnNextRequested(LevelCompletePopupData data)` (DTO ride-along). Como dev añadiendo un 6º popup no sé qué patrón seguir. Documentar en CATALOG.md M4 el contrato preferido: "events emit DTO ride-along por defecto; tuple cuando event-specific args no pertenecen al DTO (ej. multi-day claim emit current day separado)". No requiere cambio de código en M1-M3 (cada popup ya está taggeado/cerrado). Solo doc.
- ~~**`sealed UIAnimXxxPopup` vs spec contradiction**~~ — **RESOLVED 2026-05-06 (M3b kickoff)**. Decisión: option (b) keep sealed + update spec. Grep confirmó 13/13 `UIAnim*Popup`/`UIAnim*Screen` shippeadas en `Runtime/Catalog/` son `sealed`. Spec original (§ 3.1 MUST 4) decía "separate component, replace the script" — internamente consistente con sealed. La deuda nació de un mal-leído del checker (asumió que inheritance era el path canonical). Fix: 1 edit en `Documentation~/Specs/CATALOG.md` § 3.1 MUST 4 explicitando los 2 paths binarios (UIAnimPreset SO override OR replace script entirely; inheritance intentionally blocked). 0 código tocado. M3b polish landing en las sealed classes mismas (override virtual hooks de `UIAnimPopupBase`/`UIAnimScreenBase`). CHANGELOG `[0.8.1-alpha]` `### Documentation` registra la resolución.
- **Theme banner color hardcoded en builders** (M3 polish window — UIAnim per-element polish): `/_checker as dev` 2026-05-04 detectó que `BuildLevelCompletePopup` hardcodea `SuccessTintColor` (local readonly) para el `NewBestBanner` Image en lugar de leer `Theme.SuccessColor` runtime. Stars-sprite SÍ son live-Theme (runtime `ApplyStars` reads Theme), pero colors quedan frozen-at-build. Inconsistencia interna: si buyer cambia `Theme.SuccessColor` post-build, banner queda desactualizado. Fix: builders deberían setear el banner color via runtime hook (e.g. `ThemedImage` component que lee Theme on enable) o the popup runtime should re-tint. Aplica a futuros banners también (`GameOver` failure tint, `LevelComplete` newBest, etc). Documentar como deuda M3 — no bloquea M1 tag (`v0.6.0-alpha` ships con caveat documentado en CHANGELOG).
- ~~**Manual prefab registration docs missing** (Quickstart M4)~~ **REWRITTEN 2026-05-10 post-M5.7 verify-all-docs gate** — original entry cited `PopupManager.RegisterPrefab(typeof(T), prefab)` API as 2nd buyer path; `feedback_library_api_verification.md` confirmed empirically that **NO `RegisterPrefab` API exists in Runtime** (3rd of 4 hallucinations caught in M5.X session). Real buyer-without-builder path: `[SerializeField] _popupPrefabs[]` Inspector array on `KitforgeRoot/PopupManager`. Item now reframed as: **document the Inspector-array path** in Quickstart § "Adding popups manually (without the builder)" — drag your popup prefab into `_popupPrefabs[]` slot on `KitforgeRoot/PopupManager` (Inspector); Show<T>(data) resolves by Type. Deliverable target: `v1.0.0-rc.1` Quickstart sweep (paired with README status row + version bump per M5.7 tag-cut deferrals). Rejected alternative: introducing a runtime `RegisterPrefab(Type, GameObject)` method to make the phantom API real — out of scope (post-1.0 evaluation; current Inspector path works, doesn't need a runtime mutation API).
- Default severity icon sprites en Bootstrap Defaults (`IconInfo` / `IconWarning` / `IconError`).
- Decisión `_tween-dev` vs `_tween` style-aware (no bloquea Group B).
- **`UIModuleBase.OnUpdate()` infra dispatch gap** (latent bug, validated 2026-05-03): `UIModuleBase.OnUpdate()` is `public virtual` but NO infra component invokes it — not `PopupManager`, not `UIManager`, not `ToastManager`. Any popup overriding `OnUpdate` for per-frame ticking is silently broken in production. **Currently affected:** `RewardPopup.AutoClaim` (silently broken since v0.5.0; tests pass because they call `AdvanceAutoClaim(float)` directly), `DailyLoginPopup` D7 countdown (workaround applied — see below). **Locality-respecting workaround for new popups needing ticking:** add `private void Update() => OnUpdate();` LOCALLY on the popup that needs it. Does not touch other popups, preserves scope-cap. Validated on `DailyLoginPopup` (2026-05-03). **Will hit again:** `HUD-Timer` (next Phase B element — requires per-frame ticking). **Real infra fix candidate:** `PopupManager.Update()` iterates `_activeStack` and calls `OnUpdate()` on each popup (single-line dispatch). Risk: doubles ticks for popups that already added the local workaround → fix-and-sweep needed if dispatch is added (audit RewardPopup + DailyLoginPopup + future HUDTimer when promoting). Defer the infra fix until a session that can do the sweep atomically.
- **Capability-gate audit residual — RESOLVED 2026-05-04 (M2 kickoff)**: re-audit using buyer-frequency lens completed. **`RewardFlow.GrantAndShow` (single) → ship M2**: `CatalogGroupBDemo.cs` itself repeats the pattern 3× (`SpawnReward` L260, `SpawnChainReward` L167, `HandleWatchAdRequested→SpawnReward` L244); same ≥2-callsite threshold that promoted `GrantAndShowSequence`; the silent-bug risk (forgetting item-sentinel skip credits items as currency) is high. **`ShopFlow.OpenWithPurchaseChain` → OUT-of-scope `v1.0.0-rc`**: 1 buyer callsite typical, chain shape is opinionated (Ad-fund vs IAP-fund), 70-line demo IS the pedagogy. Mitigation: M4 QUICKSTART ships a "**Pattern: monetization chain**" recipe section pointing at the demo's literal 70 lines. CHANGELOG line at M2 close: `ShopFlow.OpenWithPurchaseChain not shipped — chain shape is opinionated; buyers fork. Documented as "monetization chain pattern" recipe in QUICKSTART instead.`
- **Master demo scene `Samples~/Catalog_All_Demo/`** (M4 entry — added 2026-05-04 `/_checker as user` S2): single-import sample wiring all shipped catalog elements (Groups A+B+C+D+E) into one hub scene with buttons-per-popup. Replaces 4-click import + scene-switching path with 30-second buyer wow-moment. Hero screenshots derive from this scene.
- **"Add your own popup" Quickstart §extension** (M4 docs — added 2026-05-04 `/_checker as user` S3): walkthrough for buyer extending catalog with a 16th popup — `: UIModule<MyData>` + `PopupManager.RegisterPrefab` + UIAnim attach. Surfaces existing extension path; pairs with "Manual prefab registration docs missing" entry above (likely consolidated into one Quickstart subsection in M4 docs sweep).
- **Buyer fresh-import smoke test** (M4 entry — added 2026-05-04 `/_checker as user` S5): pre-tag M4 ritual — install via Package Manager URL into a clean Unity project, import Quickstart + Group A + B + C + D + E samples, press Play in each, confirm zero LogError without any wiring. Catches "first-Play UX" regressions like B.4.6 C1 (HUDTimer `--:--`) holistically rather than per-element.

## Open architecture questions (decided at Group C kickoff)

These are deferred-by-design — recorded so Group C does not rediscover them.

### Q1 — `IEconomyService` v1 → v2 migration

**Symptom seen in Group B**: HUD-Coins and HUD-Gems are sibling classes that share lifecycle (`UIHUDBase`) but bind to typed events (`OnCoinsChanged` vs `OnGemsChanged`). Adding a 3rd currency = copying a 30-line file.

**Root cause**: `IEconomyService` v1 has per-currency methods (`GetCoins`/`SpendCoins`/`AddCoins` × 2 currencies) and per-currency events. v2 would parameterize: `Get(CurrencyType)`, `Spend(CurrencyType, int)`, `event OnChanged(CurrencyType, int)`. Single `HUDCurrency` parameterized class replaces N siblings.

**Decision criteria at Group C kickoff**:
1. Does Group C add a 3rd currency (Energy)?
2. Does Asset Store competitive analysis show buyers wanting 4+ currencies?
3. Is BREAKING acceptable in `v0.6.0-alpha`?

**Default posture**: migrate to v2 at Group C if (1) is yes. Group B builds against v1 — inherits whatever contract exists at Group C kickoff.

**Spec reference**: `Documentation~/Specs/Catalog/HUD-Gems.md` § "Open architecture question".

### Q2 — Convenience helpers (`RewardFlow`, `ShopFlow`)

**Symptom seen in Group B**: per-callsite buyer boilerplate of ~4 lines for "show reward + credit economy", ~10 lines for "open shop + chain to NotEnough + chain to ad + chain to reward", and the R4 multi-reward sequence (Reward 1 dismisses → Reward 2 shows → ...) which is a third recurring pattern.

**Decision criteria at Group C kickoff**:
1. Does Group C produce 3+ reward callsites (Daily, LevelComplete, GameOver — likely yes)?
2. Does Group C produce 2+ shop-chain callsites (GameOver "Continue", DailyLogin "Watch to double" — likely yes)?
3. Does Group C produce 1+ multi-reward sequence callsite (post-level + daily + achievement, all granted on same level end — likely yes)?

**Default posture**: implement three helpers in Group C with capability-gate proof:
- `RewardFlow.GrantAndShow(...)` — single reward + credit.
- `RewardFlow.GrantAndShowSequence(...)` — N rewards chained via OnDismissed.
- `ShopFlow.OpenWithPurchaseChain(...)` — Shop → NotEnough → Ad → Reward → economy credit.

Spec sketches in Group B specs lock the event signatures so helpers compile without refactor. `RewardPopup` itself stays single-shot — sequencing is a host/helper concern, not a popup concern (preserves contract symmetry across catalog).

**Spec references**:
- `Documentation~/Specs/Catalog/RewardPopup.md` § "Convenience helpers (deferred)".
- `Documentation~/Specs/Catalog/ShopPopup.md` § "Convenience helpers (deferred)".

## Phase definition (canonical — overrides any ad-hoc mention in CHANGELOG)

| Phase | Status | Goal | Done criteria |
|---|---|---|---|
| **0** | ✅ done | Scaffolding (package.json, asmdef, folder layout) | Empty package boots in Editor |
| **1** | ✅ done (`v0.1.0-alpha`) | Runtime contracts (UIManager, PopupManager, UIRouter, Theme, Module<T>) + Samples + README | Quickstart sample plays end-to-end |
| **1.5** | ✅ done (`v0.2.1-alpha`) | EditMode tests + deferred fixes | 33 tests green, zero deuda |
| **2** | 🟡 **TO DEFINE** | TBD — see "Phase 2 candidates" below | TBD |
| **3+** | ⏳ | TBD | — |

## RE-SCOPING (2026-05-01) — Phase 2 is the catalog, not polish

**User correction**: Phase 1+1.5 built the framework (engine), but the original product intent was a **kit of ready-to-use mid-core mobile UI prefabs**. What ships today is "wire it yourself"; what was wanted is "drag and drop a SettingsPopup that already works".

**Phase 2 is therefore re-scoped**: deliver the prefab catalog the kit name promises, milestone by milestone (one element at a time), with `_tween` animations bundled per element.

**Reference product**: Doozy UI Manager — framework + prefab library. We're building a leaner, opinionated cousin.

## Catalog target (15 elements — confirmed by user)

Decomposed by Manager layer (this matters — different lifecycles, different contracts):

### Screens (UIManager — stack-based, full canvas)
- 12. **LoadingScreen** — full-screen, optional progress bar, async hand-off
- 13. **MainMenuScreen** — play/settings/shop/daily entry buttons

### Popups (PopupManager — priority queue, modal/overlay, MaxDepth=3)
- 1. **ConfirmPopup** — Yes/No, Continue/Cancel; the universal blocker
- 2. **SettingsPopup** — sound/music/vibration/language toggles
- 3. **RewardPopup** — item/coins obtained; tap-to-claim
- 4. **DailyLoginPopup** — 7-day calendar, daily reward
- 5. **ShopPopup** — grid of items, prices, buy buttons
- 6. **LevelCompletePopup** — stars, score, next/retry
- 7. **GameOverPopup** — continue (rewarded ad) / restart / quit
- 8. **PausePopup** — resume/restart/settings/quit
- 9. **NotEnoughCurrencyPopup** — offer to buy more / watch ad
- 10. **TutorialPopup** — text + character + continue/skip

### Transient/HUD (NEW LAYER — does NOT exist in Phase 1)
- 11. **NotificationToast** — non-blocking, auto-dismiss, no input, stack-able
- 14. **HUD elements** — coin counter, gem counter, energy bar, timer (live on screens, not in popup registry)

## Architecture gaps detected (blockers for catalog)

These don't exist yet in Phase 1 and MUST be designed before building catalog elements:

| # | Gap | Affects elements | Severity |
|---|---|---|---|
| G1 | **No ToastManager / NotificationCenter** — toasts are not popups (no priority, no eviction, no backdrop, multiple stack-able, auto-dismiss) | NotificationToast, indirectly all (any popup may want to spawn toast on confirm) | 🔴 |
| G2 | **No HUD layer** — HUD elements are screen-bound, not registry-bound, but need to react to global service events (currency, energy) | HUD-Coins, HUD-Gems, HUD-Energy, HUD-Timer | 🔴 |
| G3 | **No service binding pattern** — popups need IEconomyService/IAdsService/etc. but there's no documented way to inject them. Without DI, each popup invents its own wire-up. | Reward, Shop, NotEnoughCurrency, GameOver, DailyLogin, all HUD | 🔴 |
| G4 | **`PopupManager` doesn't expose `Theme`** — only `UIManager` does. Popups need theme to skin themselves. | All popups | 🟠 |
| G5 | **No "popup chain" pattern** — Shop → NotEnough → Reward must be expressible without coupling popups. Today buyer would either chain via callbacks (verbose) or call PopupManager.Show from inside Shop (couples). | Shop, NotEnough, Reward, GameOver, LevelComplete | 🟠 |
| G6 | **No auto-trigger pattern** — DailyLoginPopup must auto-open on app launch under conditions. Today no place to register "show this popup when X". | DailyLogin, Tutorial (first-run), GameOver (auto on death) | 🟠 |
| G7 | **No animation contract** — every popup needs Show/Hide/Idle animations. Without contract, each popup invents its own DOTween calls duplicating boilerplate. | All elements | 🔴 |
| G8 | **`UIThemeConfig` lacks asset slots** — only colors/fonts/sizes today. Needs sprite slots (button bg, panel bg, icons), audio slots (button SFX), prefab slots (close button) for true reskin without code. | All elements | 🟠 |
| G9 | **No safe-area helper** — README declares it Non-goal #7, but buyer prefabs MUST work on notched devices to be valuable. Need at least a `SafeAreaFitter` component. | All screens, full-canvas popups | 🟡 |
| G10 | **No Editor validation tooling** — buyer importing the kit gets red errors at runtime when prefab missing. Pre-build validator would prevent buyer tickets. | All elements (operational) | 🟡 |

## Plug-and-play contracts (MUST / MUSTN'T)

These are the rules every catalog element MUST follow so they integrate without surprises.

### MUST
1. **Single Theme source** — every visual element reads from `UIManager.Theme` (or new `IUIThemeProvider`). No hardcoded colors, fonts, sprites in prefabs.
2. **Single service source** — economy/ads/time/etc. services accessed via one documented binding pattern (TBD — see G3).
3. **Typed payload** — every element derives `UIModule<TData>` with a serializable `<Element>Data` DTO. No `object` payloads in production code.
4. **Animation per element** — every prefab ships a `UIAnim_<Element>.cs` script generated via `_tween`, attached to the prefab, exposing `PlayShow()` / `PlayHide()` / optional `PlayIdle()`. Hooked from `OnShow` / `OnHide`.
5. **Demo asset** — every element ships in a Demo scene (`Samples~/Catalog/<ElementGroup>Demo.unity`) with one-click context menu to trigger.
6. **Portrait 1080×1920 first** — primary anchor and CanvasScaler reference. Landscape is secondary.
7. **Safe-area respect** — every full-canvas element uses `SafeAreaFitter` (TBD component) so notch/home indicator don't clip critical UI.
8. **Self-contained back behavior** — every popup overrides `OnBackPressed` with its expected action (close, decline, skip), or explicitly calls base for default dismiss.
9. **Service decoupling** — element emits events (`OnConfirmed`, `OnPurchased`, `OnRewardClaimed`); the GAME wires the chain. Element does NOT call other elements directly.
10. **Zero new Runtime dependency on DOTween** — animations are an opt-in samples concern. Runtime asmdef stays DOTween-free. Catalog elements either: (a) ship in a separate asmdef that references DOTween, or (b) use define symbol guards.

### MUSTN'T
1. **No popup may call `PopupManager.Show` for another popup** — coupling. Use events.
2. **No popup may write `Time.timeScale`** — only the game/router responds to `AppState.Paused`.
3. **No popup may hold a static reference** to a service or another popup (UNITY_RULES: no Singletons, no Find).
4. **No popup may assume a specific service implementation** — only the interface (`IEconomyService`, not `MyGameEconomy`).
5. **No element may write to PlayerPrefs / SaveSystem directly** — settings persistence goes through `IPlayerDataService`.
6. **No element may load assets via `Resources.Load`** — Inspector references only (UNITY_RULES).
7. **No element may register itself in a global registry** — registries are owned by UIManager/PopupManager via Inspector.
8. **No element may include audio playback inline** — emit events; SFX is a Theme concern (button SFX clip on Theme, played by a single AudioRouter).

## Service binding pattern proposal (resolves G3)

Three options:

| Option | Pattern | Pro | Con |
|---|---|---|---|
| **A** | `[SerializeField]` MonoBehaviour ref per popup | Inspector-driven, no DI, zero config | Bind-per-popup-per-scene tedious; risk of orphan refs |
| **B** | New `UIServices` MonoBehaviour container with `[SerializeField]` refs to all 6 services. Popups query via `UIManager.Services.Economy`. | Single bind point. Mirrors UIManager.Theme pattern. No DI required. | Ties popups to UIManager (one more dependency); not testable in isolation. |
| **C** | Optional VContainer `[Inject]` attributes; if no container, fallback to `UIServices` MonoBehaviour. | Both worlds. | Two paths to maintain, complex. |

**Recommendation**: **Option B** — `UIServices` MonoBehaviour container, mirroring the proven `UIManager.Theme` pattern. Buyer sets it up once, popups consume via `Services.Economy` getter. VContainer users wrap their container resolution into `UIServices` setters at boot; no Runtime change needed.

## Building order — 5 groups by contract similarity

Don't build chronologically (1→15). Group by shared dependencies, build group by group, validate plug-and-play within group before moving on.

### Group 0 — Foundation (prerequisite, no buyer-visible elements)
- G1: ToastManager
- G2: HUD layer / `UIHUDBase` contract
- G3: UIServices container
- G4: PopupManager.Theme exposure
- G7: UIAnim contract + DOTween-guarded asmdef strategy
- G8: UIThemeConfig asset slots (sprites, audio, icons)
- G9: SafeAreaFitter component
- **Acceptance**: a "Hello Toast" + "Hello HUD coin" demo scene proves the new layers work without any popup yet.

### Group A — Pure UI (no service dependencies)
1. ConfirmPopup
2. TutorialPopup
8. PausePopup
11. NotificationToast (uses ToastManager from Group 0)
- **Acceptance**: 4 elements coexist in one demo scene. Open Pause → Settings (still future) → Confirm "Quit?". Toast appears. Back button traverses correctly.

### Group B — Currency / Economy
3. RewardPopup
5. ShopPopup
9. NotEnoughCurrencyPopup
14a. HUD-Coins, HUD-Gems
- **Acceptance**: Shop demo with stub IEconomyService. Buy item → spend coins → HUD updates. Buy unaffordable → NotEnough opens. Watch ad stub → Reward → coins added → HUD updates.

### Group C — Progression / Time
4. DailyLoginPopup
6. LevelCompletePopup
7. GameOverPopup
14b. HUD-Energy, HUD-Timer
- **Acceptance**: Demo level with timer countdown. Win → LevelComplete. Lose → GameOver → Continue (ad). Daily on app launch.

### Group D — Player Data
2. SettingsPopup
- **Acceptance**: Settings persists across play sessions via IPlayerDataService stub.

### Group E — Screens
12. LoadingScreen
13. MainMenuScreen
- **Acceptance**: Full app boot demo: Loading → MainMenu → Daily auto-popup → Play → Pause → GameOver → MainMenu.

## 4-lens stress test (DRAFT — to expand per group)

### PM lens (product manager)
- ❓ Is the catalog cohesive? **Risk**: 15 elements with different visual styles look incoherent. **Mitigation**: hard-enforce Theme single source of truth; ship 1 default theme + 2 alt themes (dark/light/colorful) to demonstrate reskin power.
- ❓ What's the buyer demo path in Asset Store screenshots? **Risk**: 15 elements is too many to showcase. **Mitigation**: 1 hero screenshot per group (5 total) + 1 master scene with all elements toggle-able.
- ❓ How does `_tween`-generated animations stay coherent across 15 elements? **Risk**: each looks different in motion. **Mitigation**: Theme exposes animation tokens (`TransitionSpeed`, `BounceStrength`); `_tween` generators must consume them.
- ❓ What's the pricing tier? **Deferred** per user.

### Dev lens (host game developer)
- ❓ How does the dev add a 16th popup not in the catalog? **Risk**: kit forces patterns that don't fit custom popups. **Mitigation**: every catalog element is a reference implementation of `UIModule<TData>` — derive your own the same way.
- ❓ How does the dev re-skin element X without touching prefab? **Critical**: must be 100% via Theme + sprite swap, never via code. Test: open ConfirmPopup prefab → assign different Theme → re-runs without diff.
- ❓ How does the dev wire the cross-element flow (Shop → NotEnough)? **Risk**: events feel verbose for trivial wirings. **Mitigation**: ship a `Samples~/Catalog/Wiring/` sample with the 5 most common chains pre-wired.
- ❓ How does the dev override an animation? **Risk**: replacing UIAnim_<X> means forking the prefab. **Mitigation**: UIAnim_<X> is a separate component on the same prefab; replacing the script doesn't touch hierarchy.

### QA lens (tester / automated)
- ❓ Can each element be tested in isolation? **Yes**: each popup is `UIModule<TData>` — instantiate, bind fake data, assert visual state. Pattern proven in current 33 EditMode tests.
- ❓ Can flows be tested? **Risk**: integration tests need PopupManager + multiple popups + service stubs. **Mitigation**: PlayMode tests under `Tests/PlayMode/` with composition root; one test per Group's acceptance scenario.
- ❓ What can break in combination? **Top risks**:
  - Pause + Settings + Confirm stacked: back button order
  - GameOver while NotEnough is open: priority eviction must NOT lose Continue prompt
  - Toast spawning while popup is dismissing: ToastManager must not depend on PopupManager state
  - Reward stacking (multiple rewards in succession): queue or merge?
  - Theme swap at runtime: do all open elements re-skin? (acceptance: yes for color/font; sprite swap requires re-instantiate, document the limit)
- ❓ How does QA replay a failure scenario? **Mitigation**: every Demo scene has `[ContextMenu]` triggers per scenario, deterministic seeds for stub services.

### User lens (end player)
- ❓ Does back button feel right? **Risk**: pressing back from Pause → resumes accidentally. **Mitigation**: every popup defines explicit OnBackPressed; default for blocker popups (Confirm, GameOver) is "do nothing", not "dismiss".
- ❓ Are animations consistent? **Risk**: each popup uses different ease/duration. **Mitigation**: Theme.TransitionSpeed governs all; _tween generators pull from Theme.
- ❓ Is the player ever blocked? **Risk**: Toast obscures CTA, popup queue starves. **Mitigation**: Toast positioned outside CTA zones; queue depth visible in DevHUD sample.
- ❓ Does the UI feel "mid-core mobile" or "indie hyper-casual"? **Subjective but critical**: define visual reference moodboard before building Group A.
- ❓ Touch targets on small devices? **Mitigation**: Theme exposes `MinTouchTarget` (default 88pt). All buttons enforce min size.

## ALIGNMENT ANSWERS (2026-05-01) — Phase 2 LOCKED

| # | Question | Answer |
|---|---|---|
| 1 | Group order | ✅ Group 0 first → A → B → C → D → E |
| 2 | Service binding | ✅ Option B — `UIServices` MonoBehaviour container |
| 3 | Animation strategy | ✅ **Style dropdown system** — see "Animation Style Catalog" below. Improves `_tween` agent too. |
| 4 | Theme depth | ✅ Cover ALL fundamentals in Group 0. Per-element specifics later. |
| 5 | DOTween policy | ✅ Follow `_tween` principles (DOTween Pro assumed; SetLink, Kill, explicit Ease, no Singleton). Catalog asmdef references DOTween directly. |
| 6 | Visual reference | ✅ **Disney Getaway Blast** — bright cartoon mid-core, snappy + playful animation, layered HUD, juicy feedback |
| 7 | Sample structure | ✅ One sample per group (`Samples~/Catalog_Group0_Foundation/`, `Samples~/Catalog_GroupA_PureUI/`, ...) |
| 8 | Versioning | ✅ Single bump at end of Phase 2 (e.g. `0.3.0`). Intermediate work in `[Unreleased]` CHANGELOG section. |

## Animation Style Catalog (proposal — pending user approval)

Driven by Disney Getaway Blast reference. Implemented as `UIAnimStyle` enum + `UIAnimPreset` ScriptableObject so:
- Buyer picks a style per element (or per Theme as default).
- `_tween` agent reads the style token and emits matching DOTween code.
- Each preset defines tokens for the 4 animation channels: **Scale**, **Fade**, **Position**, **Rotation** (duration + ease + overshoot).

### Proposed 10 styles (covering full mid-core mobile spectrum)

| # | Style | Feel | Duration | Ease (in/out) | Overshoot | Use cases |
|---|---|---|---|---|---|---|
| 1 | **Snappy** | Quick, decisive, light overshoot | 0.20s / 0.15s | OutBack / InQuad | 1.05 | Default for buttons, confirm, toast. Royal Match vibe. |
| 2 | **Bouncy** | Exaggerated elastic | 0.45s / 0.25s | OutElastic / InBack | 1.15 | Reward, level-complete celebration. Coin Master vibe. |
| 3 | **Playful** | Anticipation pull-back + bounce | 0.40s / 0.20s | Custom curve / InQuad | 1.10 | **Disney Getaway Blast core** — main popups, characters appearing |
| 4 | **Punchy** | Instant in, hold, instant out | 0.10s / 0.10s | OutQuad / InQuad | 1.20 | Damage numbers, combat feedback, score tick |
| 5 | **Smooth** | Polished, no overshoot | 0.35s / 0.25s | OutCubic / InOutSine | 1.00 | Settings, premium screens, transitions |
| 6 | **Elegant** | Slow, classy, fade-led | 0.50s / 0.40s | OutSine / InSine | 1.00 | Casino, story popups, story tutorials |
| 7 | **Juicy** | Multi-axis wiggle + scale pulse + slight rotation | 0.40s / 0.20s | OutElastic + sine wiggle | 1.15 + ±5° | Daily login, big rewards, hyper-casual hits |
| 8 | **Soft** | Gentle, long ease-in, no overshoot | 0.50s / 0.35s | InOutSine | 1.00 | Tutorial popups, onboarding, loading |
| 9 | **Mechanical** | Linear, no easing, instant snap | 0.05s / 0.05s | Linear | 1.00 | Debug HUD, dev tools, accessibility mode |
| 10 | **Cinematic** | Staggered cascade with hold beats | 0.60s / 0.30s | OutCubic + 0.05s stagger between sub-elements | 1.05 | Game over, level intro, story moments |

### Style selection levels (override hierarchy)

```
Theme.DefaultAnimStyle          ← global default (e.g. Playful for Disney Blast vibe)
  ↓ overridden by
UIModule.AnimStyleOverride      ← per-element opt-out (e.g. Punchy for damage numbers)
  ↓ overridden by
UIAnim_<Element> custom logic   ← bespoke choreography (Group-specific exceptions)
```

### `_tween` agent improvement (related)

Update `_tween` agent prompt template to:
- Accept a `style` parameter (one of the 10).
- Read `UIAnimPreset` tokens and emit DOTween code that pulls duration/ease/overshoot from the preset (not hardcoded).
- Optionally: emit a "style-agnostic" version that exposes a `[SerializeField] UIAnimPreset _preset` field instead of literals.

This way: changing the global style at runtime re-skins all element motion without re-generating scripts.

### Open sub-questions for animation styles

a. ✅ 10 styles aprobados.
b. ✅ Default = **Playful** (Disney Getaway Blast vibe).
c. ✅ `UIAnimPreset` como ScriptableObject (buyer-creatable).
d. ✅ Style afecta a TODO: Show/Hide de modules + feedback de botones (hover/pressed/disabled) + idle loops + transient (toast).

### `_tween` agent evolution (locked — execute en Group 0, paso F5)

Agente actual: `c:\Users\Joan\.claude\agents\_tween-animator.md` + comandos `_tween.md` y `_tween-dev.md`.

Cambios obligatorios cuando arranque Group 0:

1. **Aceptar parámetro `style`** (uno de los 10) en el prompt template.
2. **Leer `UIAnimPreset` SO tokens** (Scale/Fade/Pos/Rot duration+ease+overshoot) en lugar de hardcodear.
3. **Emitir un `[SerializeField] UIAnimPreset _preset` field** en el script generado, no literales.
4. **Generar feedback de botones** (hover/pressed/disabled) consistente con el style elegido.
5. **Documentar cuándo usar cada style** (matriz caso-de-uso → style recomendado).

Nota: `_tween-dev` ya existe como variante separada. Decidir si:
- (i) actualizar AMBOS (`_tween` y `_tween-dev`) para soportar styles, o
- (ii) hacer `_tween-dev` el "style-aware" y dejar `_tween` como modo legacy literal.

Pregunta para próxima sesión, no bloquea Group 0 spec.

## Documentation drift — pending fixes after this re-scoping

- **CHANGELOG**: añadir entry "Phase 2 re-scoped to catalog delivery".
- **README §Phase 1.5 done criteria**: nota "Framework complete; catalog is Phase 2".
- **`Samples~/GameWiring/README.md`**: la promesa "Phase 5" se reasigna o borra cuando Phase 2 esté definida.
- **`Documentation~/Specs/CATALOG.md`** (NEW): doc maestro buyer-facing con catálogo + contratos.

## Five lenses (Phase 2 — pre-alignment, deprecated by re-scoping above, kept for history)

### Essence
- What is Phase 2? A single proposition pending. Candidates collapse into either:
  - **"Asset Store-ready hardening"** (tests + validation + samples + docs).
  - **"Animation contract"** (new feature surface).
  - These are different products with different timelines. **Pick one before alignment.**

### Structure
- The Runtime is closed (Phase 1 + 1.5). Phase 2 either extends Runtime (animation API = breaking-by-addition) or adds Editor/Tests/Docs (zero Runtime impact).
- **Decision pivot**: does Phase 2 touch Runtime API or not?

### Experience
- Buyer's first-hour journey today: import → Quickstart sample → Play → see logs. **Friction point**: no animations, looks unfinished. Drives intent toward Phase 2 = animation contract.
- Buyer's first-error journey today: forgets to register prefab → console LogError. **Friction point**: error visible only at runtime, not at PreBuild. Drives intent toward validation rules.

### Priority
- For Asset Store launch viability: **validation + animation > more tests**. EditMode coverage at 33 is sufficient for `v1.0.0` if API stabilizes.
- For internal usage in PACHINKO: any of the above works; depends on PACHINKO's actual UI needs (unknown to me).

### Success
- "Done" for Phase 2 means: ___ (TBD with user).

## Strategic open questions (for user)

> These need answers before workbook can transition from DISCOVER → ALIGN.

1. **Asset Store launch target**: ¿hay una fecha objetivo (Q3 2026, end of 2026, sin prisa)? Define la presión sobre Phase 2 scope.
2. **PACHINKO usage**: ¿este paquete ya se está usando en PACHINKO o es independiente? Si se usa, ¿qué carencias has visto in vivo?
3. **Competition awareness**: ¿has analizado Doozy / NSPanel / otras alternativas en Asset Store? ¿Hay un gap claro que justifique nuestro pricing?
4. **Animation philosophy**: ¿el kit debe traer animaciones built-in (más completo, más opinionated) o seguir siendo "bring your own" (más ligero, más universal)?
5. **Phase 2 scope size**: ¿prefieres una Phase 2 grande (3-4 semanas, multi-feature) o pequeña y rápida (1 semana, una sola cosa)?

## Documentation drift fixed in this session

- README §Status: actualizado a Phase 1.5 + 33 tests.
- README §Install: URL git apunta a `v0.2.1-alpha`.
- README §Phase 1 done criteria: checkbox EditMode tests marcado.
- **Pendiente** (CHANGELOG split en bloques `[0.2.0-alpha]` y `[0.2.1-alpha]`): diferido al próximo tag versionado.
- **Pendiente estratégico** (`Samples~/GameWiring/README.md` menciona "Phase 5"): a resolver cuando Phase 2 esté definida — la promesa puede borrarse o mantenerse.

## Decisions log

- **2026-05-01** — Workbook vive en `~/.claude/memory/` (global), no en el paquete UPM ni en `_Develop/`. Razón: contexto estratégico es transversal a workspaces, no es entregable del producto.
- **2026-05-01** — Strategic intent confirmado: **Asset Store premium tool**, target hybrid-casual studios pequeños.

## Group C kickoff — 2026-05-02 (this session)

**Status**: Phase 0 (decisiones arquitectura) + Phase A (5 specs) entregadas. Phase B+ (DTOs/delivery/bump) diferido.

### Phase 0 — LOCKED

- **Q1 — `IEconomyService` v2 BREAKING en `v0.6.0-alpha`**: parameterized API (`Get/Spend/Add(CurrencyType, ...)` + `OnChanged(CurrencyType, int)`). `CurrencyType.Energy = 2` añadido. EQ1-EQ7 lockeados.
- **Q2 — 3 helpers shipping en Group C**: `RewardFlow.GrantAndShow` (con overload DTO, F2), `RewardFlow.GrantAndShowSequence` (callback `onSequenceComplete`, F5), `ShopFlow.OpenWithPurchaseChain`. HQ1-HQ6 lockeados.
- **`as user` + `as ux` checkers ejecutados**: 6 FIX NOW aplicadas (F1 pitch corrections, F2 overload, F3 named prefabs, F4 actionable error format, F5 rename, F6 migration table mandatoria).

### Phase A — 5 specs entregados

```
Documentation~/Specs/Catalog/
├── DailyLoginPopup.md         — D1-D8 + DailyLoginFlow.ShowIfDue helper
├── LevelCompletePopup.md      — L1-L7 + RewardFlow.GrantAndShowSequence integration
├── GameOverPopup.md           — GO1-GO8 + ContinueMode/BackPressBehavior enums
├── HUD-Energy.md              — E1-E8 + HUDEnergy : HUDCurrency subclass (E1 LOCKED Option A)
├── HUD-Timer.md               — T1-T10 + TimerMode enum (3 modes)
└── CATALOG_GroupC_DELTA.md    — pre-flight deltas: Theme slots + IProgressionService ext
```

### Open decisions resueltas en sesión

- **D5 watch-to-double**: host credits 2× total post-ad (single credit-call, no "bonus half" arithmetic). Mutually exclusive with `OnDayClaimed`.
- **E1 HUDEnergy class**: subclass `HUDEnergy : HUDCurrency` (Option A). Subclass acotada, `_currency` sealed a Energy.

### Pre-flight infrastructure required (next session opening move)

Consolidado en `Documentation~/Specs/Catalog/CATALOG_GroupC_DELTA.md`. Order of work:

1. `UIThemeConfig` slot additions: `IconEnergy`, `IconClock`, `StarFilledSprite`/`StarEmptySprite`, `FailureColor` (5 new tokens) + Bootstrap Defaults upgrade path
2. `IProgressionService` extension: `GetDailyLoginState()` + `GetEnergyRegenState()` + `DailyLoginState`/`EnergyRegenState` structs
3. `IEconomyService` v2 migration (Q1) + `CurrencyType.Energy`
4. `HUDCurrency.cs` parameterized base (replaces HUDCoins/HUDGems)
5. CHANGELOG `[0.6.0-alpha]` entry stub with migration table (F6)
6. THEN per-element DTOs + behavior + tests in spec order: DailyLogin → LevelComplete → GameOver → HUD-Energy → HUD-Timer
7. Builder `Build Group C Sample` + chain demo
8. Tag `v0.6.0-alpha` BREAKING

### Group C pre-flight follow-ups (deferred from `as qa`/`as dev` mental pass)

8 FIX-NEXT items captured in `CATALOG_GroupC_DELTA.md` § 5 (FQ1-FQ6 + FD3-FD4 + 5 NOTES). NOT blockers for next session start — each ties to concrete element delivery.

### Validation gaps for next session

- ❌ NO `as qa` / `as dev` / `as pm` checker rounds executed on Phase A specs (deferred — caps respected per L4 lesson).
- ❌ NO Phase B (DTO scaffolding) started.
- ❌ NO Theme/IProgressionService deltas applied to code yet.
- Recommend running `/_checker as dev` early next session, AFTER Theme + IProgressionService deltas land — concrete code makes contract issues visible.

### When resuming Group C delivery

1. Read `CATALOG_GroupC_DELTA.md` first — it's the runbook.
2. Apply pre-flight deltas in order (Theme → IProgressionService → IEconomyService v2 → HUDCurrency).
3. Compile clean. Run all 119 existing tests. Verify zero regression.
4. THEN open spec 1 (DailyLoginPopup) and start Phase B.
5. Apply lessons L1-L4 of `feedback_workflow.md` — L1 (run tests on fresh compile before declaring "done"), L4 (any new MonoBehaviour service container needs `[DefaultExecutionOrder]`).
6. Bump to `v0.6.0-alpha` BREAKING at end of Group C (single bump per group).

## Group C pre-flight closure — 2026-05-03 (this session)

**Status**: pre-flight infrastructure DELIVERED + VERIFIED. Phase B ready to start.

### Delivered (all 5 gates green, 119/119 tests preserved)

- **Step 1 — UIThemeConfig 5 new slots**: `IconEnergy`, `IconClock`, `StarFilledSprite`, `StarEmptySprite`, `FailureColor` (#E53935 default). Aditivo, no rompe Theme assets existentes.
- **Step 2 — IProgressionService extension**: `GetDailyLoginState()` + `GetEnergyRegenState()` + `[Serializable] DailyLoginState` (CurrentDay, LastClaimUtc, AlreadyClaimedToday, DoubledToday FQ3, MaxStreakGapDays) + `[Serializable] EnergyRegenState` (Current, Max, NextRegenUtc, IsFull). `FakeProgressionService` shipped en `Tests/Editor/Helpers/`.
- **Step 3 — IEconomyService v2 BREAKING**: typed surface (`GetCoins`/`SpendCoins`/`AddCoins`/`OnCoinsChanged` × 2 currencies) reemplazada por parameterized (`Get(CurrencyType)`, `Spend(CurrencyType, int)`, `Add(CurrencyType, int)`, `event Action<CurrencyType, int> OnChanged`). `CurrencyType.Energy = 2` añadido. 13 archivos in-compile + 3 sample files migrados (Assets/Samples imported copy + Samples~/ source).
- **Step 4 — HUDCurrency parameterized + HUDEnergy seal**: `HUDCoins.cs`/`HUDGems.cs`/`HUDCoinsTests.cs`/`HUDGemsTests.cs` BORRADOS. `HUDCurrency.cs` parameterizado con `[SerializeField] private CurrencyType _currency` + `protected virtual CurrencyType ResolveCurrency() => _currency` extension hook. `HUDEnergy.cs : HUDCurrency` minimal (sealing override `=> CurrencyType.Energy` only — Phase B extiende con regen UI). `HUDCurrencyTests.cs` parameterizado [TestCase Coins, Gems] + nuevo `OnChanged_With_Foreign_Currency_Does_Not_Trigger_ApplyValue` (FD4 contract). `CatalogGroupBBuilder.cs` migrado para producir HUDCurrency-based prefabs.
- **Step 5 — CHANGELOG `[Unreleased]` block** drafted como futuro `v0.6.0-alpha` con migration table 5-block (Block 1 sed-friendly diffs / Block 2 Why no Obsolete / Block 3 Currency extension limit / Block 4 Prefab migration / Block 5 economy v2 + HUDCurrency coupling).

### Architecture decisions locked en pre-flight (en `CATALOG_GroupC_DELTA.md`)

- **§ 4.5 Null-service fallback policy**: HUDs degrade silently / Popups LogError actionable + abort OnShow / Helpers LogError + return false. Format mandatorio: `"[ElementName]: I[ServiceName] not registered on UIServices. Wire it before opening this popup. See Quickstart § Service binding."`. Phase B specs deben citar § 4.5.
- **§ 4.6 HUDCurrency/HUDEnergy implementation specs**: tooltip mandatorio en `_currency`, `ResolveCurrency()` virtual hook como sealing pattern, Option A test strategy (parameterized [TestCase] + currency-agnostic test method names + FD4 filter test).

### Phase B ready to start (next session)

5 elementos catalog pendientes — orden spec: DailyLogin → LevelComplete → GameOver → HUD-Energy (extender HUDEnergy con regen UI + `IProgressionService` 1Hz poll per § 4.6 final spec) → HUD-Timer. Plus 3 helpers (RewardFlow.GrantAndShow + GrantAndShowSequence + ShopFlow.OpenWithPurchaseChain), `CatalogGroupCBuilder.cs`, `Samples~/Catalog_GroupC_Progression/Stubs/InMemoryProgressionService.cs` (depende de IEconomy.Add(Energy) callsites Phase B). Tag `v0.6.0-alpha` BREAKING al final.

### Notes para resuming Phase B

- Specs Phase B (`DailyLoginPopup.md`, `LevelCompletePopup.md`, `GameOverPopup.md`, `HUD-Energy.md`, `HUD-Timer.md`) ya escritas — Phase A (sesión 2026-05-02) las dejó listas. Cada spec necesita citar § 4.5 en su "Service binding" subsection (FIX NOW del checker que aún no se aplicó a las specs individuales — solo al runbook).
- Imported sample under `Assets/Samples/Kitforge Mobile UI Kit/0.5.0-alpha/...` ya migrado a v2, el chain demo Group B funciona end-to-end post-migration. Se puede usar como verificación regresión durante Phase B.
- Lecciones nuevas en `feedback_workflow.md` (BREAKING + imported samples / type-deletion grep) aplican durante Phase B cuando se haga delete de tipos legacy o cuando se introduzcan nuevos breaking changes.

## Group C Phase B element 1/5 closure — 2026-05-03 (DailyLogin)

- DailyLogin element 1/5 entregado: DTO + behavior + DailyLoginFlow helper + UIAnim shell + 26 EditMode tests + § 4.5 spec citation. **145/145 tests verificados verdes** in-Editor (119 baseline + 26 nuevos).
- Test fakes shipped: `FakeTimeService` + `FakeAdsService` (reusables por LevelComplete/GameOver).
- Workaround OnUpdate gap aplicado LOCALMENTE en DailyLoginPopup (`private void Update() => OnUpdate()` para D7 countdown). Real infra fix sigue diferido.
- DTO override `DailyLoginPopupData.DayBoundaryHour` NO implementado — UTC midnight hardcoded via `ComputeNextDayBoundaryUtc`. Buyer-side enhancement diferida.
- ContextMenu hooks NO en popup propio — diferidos a builder + demo MonoBehaviour (anti-pattern sin PopupManager + prefab).
- `DailyLoginDayCell` view component NO creado — cell rendering = buyer/builder concern.
- Cell-cascade stagger animation diferida a `_tween` session (UIAnimDailyLoginPopup ships como `: UIAnimPopupBase {}`).

## Group C Phase B element 2/5 closure — 2026-05-03 (LevelComplete)

- LevelComplete element 2/5 entregado: DTO + behavior + UIAnim shell + 16 EditMode tests + § 4.5 spec citation aplicada per-spec JIT.
- Pattern clonado de DailyLogin tras mental `as dev` round (contrato limpio, sin findings bloqueantes).
- Adaptaciones: NO `private void Update() => OnUpdate()` (LevelComplete no necesita ticking continuo); `ValidateBindOrAbort` simplificado (sin servicios required, solo foot-gun guard "all CTAs hidden").
- Animation polish (star cascade, score rollup, CTA stagger) diferida a `_tween` session (precedente Group B).
- Batch sweep § 4.5 citation aplicado a las 4 specs restantes en la misma pasada (LevelComplete + GameOver + HUD-Energy + HUD-Timer): `## Services consumed` → `## Service binding` con cite del runbook § 4.5. HUD specs añaden qualifier "(HUD = silent degrade — see Edge cases for per-service fallback)".
- Helper `RewardFlow.GrantAndShowSequence` diferido — host mockea inline en host code, no requirido por tests del popup (event arg carga el array `Rewards` intacto, test `Event_Data_Carries_Original_Rewards_For_Sequence_Helper` lo cubre).
- Pendiente verificación in-Editor: usuario debe correr suite completa para confirmar 161/161 (145 + 16 LevelComplete) sobre fresh compile (lección L1).

### Phase B progreso (5/5 elementos + 1/3 helpers)

| Elemento | Status | Tests acumulados |
|---|---|---|
| DailyLogin (1/5) | ✅ delivered + verified 145/145 | 145 |
| LevelComplete (2/5) | ✅ delivered + verified 161/161 (in-Editor 2026-05-03) | 161 |
| GameOver (3/5) | ✅ delivered + verified 179/179 (in-Editor 2026-05-03) | 179 |
| HUD-Energy (4/5) | ✅ delivered + verified 189/189 (in-Editor 2026-05-03 — fresh compile) | 189 |
| HUD-Timer (5/5) | ✅ delivered + verified 199/199 (in-Editor 2026-05-04 — fresh compile) | 199 |
| Helpers (capability-gate audit 2026-05-04) | ✅ `RewardFlow.GrantAndShowSequence` shipped (2 callsites: DailyLogin OnDayClaimed + LevelComplete OnNextRequested). ⏳ `RewardFlow.GrantAndShow` single (1 callsite — deferred Group D). ⏳ `ShopFlow.OpenWithPurchaseChain` (1 callsite — deferred Group D). | 206 |
| Builder + sample + tag | ⏳ pending (B.4 + B.5) | — |

## Group C Phase B element 3/5 closure — 2026-05-03 (GameOver)

- GameOver element 3/5 entregado: DTO + `ContinueMode`/`BackPressBehavior` enums + behavior + UIAnim shell + 18 EditMode tests. **179/179 verde** (verified in-Editor 2026-05-03).
- Two-layer affordability gate (GO3) implementado per spec: `economy=null` → `Button.interactable=false` (click muerto); service present + can't afford → `interactable=true` + visual gray (image alpha 0.5) + click re-queries `CanAfford` y emite `OnContinueAffordCheckFailed` SIN dismiss. Click handler always re-queries `CanAfford` (cubre external economy mutation entre Bind y click).
- Foot-gun guard: forces `ShowMainMenu=true` (no `ShowRetry` como LevelComplete) — game-over biases toward exit per GO7 default.
- Ad readiness gateado vía `Button.interactable=!IsRewardedAdReady` solo en Bind (no re-check on click — host owns ad-failure recovery per FQ4).
- `UIAudioCue.Failure` no existe en enum → uso `UIAudioCue.Error` como Show cue. Decisión documentada en CHANGELOG.
- Animation polish (CTA cascade, Cinematic Failure-color particle accents) deferida — landa en M3 polish window per `/_checker` triple-round mejora C (NO post-v1.0.0).

### Triple-round `/_checker` findings (as user + as pm + as dev) — 2026-05-03

9 mejoras alineadas. Text-only edits aplicadas en esta sesión; resto diferido a B.x kickoff por scope-cap discipline.

| # | Mejora | Status |
|---|---|---|
| A | Capability-gate audit en B.3 kickoff (`RewardFlow.GrantAndShow` + `ShopFlow.OpenWithPurchaseChain` callsite count). Si <3 / <2 callsites respectivamente → defer a Group D, ship sólo `GrantAndShowSequence` | deferred B.3 |
| B | `InMemoryProgressionService` default `LastClaimUtc = DateTime.MinValue` (no "today") para que DailyLogin auto-dispare en first Play del buyer (discoverability fix) | deferred B.4 |
| C | Animation polish UIAnim* (cell cascade DailyLogin, star cascade LevelComplete, CTA cascade GameOver, juicy regen tween HUD-Energy, Group B catch-ups) landa en M3, NO post-v1.0.0 | applied (this commit) |
| D | M1 estimate revisión: total **9-11.5 sesiones** (5 done + 4-6.5 remaining) vs original 5-7 — upper-bound del estimate original | applied (this commit) |
| E | Orden delivery: HUDs primero (B.1+B.2), helpers segundo (B.3), builder tercero (B.4), tag último (B.5). Razón: helpers tienen capability-gate risk; HUDs son predecibles | applied (this commit) |
| F | Comment anchor `// OnUpdate-workaround-M3-sweep` en HUDEnergy + HUDTimer `Update`; M3 entry-criteria: grep + atomic sweep antes de cualquier código M3 | deferred B.1 + B.2 |
| G | Leer `HUD-Timer.md` spec ANTES de B.2 .cs (T1-T10 + TimerMode enum) | deferred B.2 entry |
| H | Test audit post-B.3: migrar `LevelCompletePopupTests.Event_Data_Carries_Original_Rewards_For_Sequence_Helper` para invocar el helper, no sólo validar `Rewards[]` intacto | deferred B.3 closure |
| I | Chain demo: full 6 acceptance scenarios (no subset) per buyer-value criterion | deferred B.4 |

### M1 next-session order (post-2026-05-04 HUD-Timer close)

Per `/_checker` mejora E (HUDs primero por delivery risk lower):

1. ~~**B.1 HUD-Energy**~~ ✅ DONE 2026-05-03 — regen UI + 1Hz `IProgressionService.GetEnergyRegenState()` poll + auxiliary OnChanged-driven poll per E3 + OnUpdate workaround LOCAL anchored `// OnUpdate-workaround-M3-sweep` per F + 10 tests + § 4.5 silent-degrade contract proof. 189/189 verde sobre fresh compile.
2. ~~**B.2 HUD-Timer**~~ ✅ DONE 2026-05-04 — `HUDTimer` + `TimerMode` enum (CountdownToTarget / CountupSinceTarget / LocalStopwatch) + warning/expiry tweens + pause/resume + format validation + 10 tests + § 4.5 drift fix (UTC mode null Time → `"--:--"` silent) + `UIAudioCue.Error` for warning (Failure cue doesn't exist) + `RealTimeProviderForTests` Func injection. 199/199 verde sobre fresh compile.
3. ~~**B.3 Helpers**~~ ✅ DONE 2026-05-04 — capability-gate audit confirmed default expectation: ship `RewardFlow.GrantAndShowSequence` only (2 callsites: DailyLogin + LevelComplete); defer `RewardFlow.GrantAndShow` single + `ShopFlow.OpenWithPurchaseChain` to Group D (1 callsite each). 7 EditMode tests + mejora H integration test. 206/206 verde sobre fresh compile.
4. **B.4 Builder + sample + chain demo** (~2-3 sessions) — sub-divided per `/_checker` 2026-05-04:
    - ✅ **B.4.0 DONE 2026-05-04** — `Samples~/Catalog_GroupC_Progression/Stubs/InMemoryProgressionService.cs` + asmdef `KitforgeLabs.MobileUIKit.Samples.CatalogGroupC`. F1+F2+F3 fixes applied: `package.json` `samples[]` entry deliberately deferred to B.4.6 (premature exposure risk); CHANGELOG entry explicitly marked "intermediate progress, NOT tag-ready"; asmdef name locked for B.4.1+ reflection. **NOT tag-ready until B.4.1-B.4.6 lands.**
    - ✅ **B.4.1 DONE 2026-05-04** — `Editor/Generators/CatalogGroupCBuilder.cs` skeleton (~140 lines): 3 const strings (`OutputRoot` + `PrefabsFolder` + `ProgressionServiceTypeName` LOCKED reflection target) + 5 color tokens used by helpers + 8 layout helpers (`CreateRoot`/`CreateBackdrop`/`CreateCard`/`CreateText`/`CreatePrimaryButton`/`CreateSecondaryButton`/`StretchInside`/`AnchorTopOfCard`) + 3 transitive deps (`CreateChild`/`AddImage`/`CreateButton`) + `EnsureFolders` + empty `BuildAll` with `Tools/Kitforge/UI Kit/Build Group C Sample` MenuItem (logs skeleton-ready diagnostic). `/_checker drift` cap-strict applied — anticipatory paths/colors/helpers (Cell/HeaderTint/Success/HUDBackground/TextLight/Failure colors + 5 prefab paths + ScenePath + DefaultThemePath + StubAsmdef + `SaveAsPrefab`/`WireAnimatorCard`/`ForceButtonHeight`) deferred to specific B.4.2-B.4.6 sub-step that first needs them. **Helper-sharing strategy decided**: copy-paste M1-M3, extract `CatalogBuilderBase` at M4 hardening sweep when 4 builders (A/B/C/D) exist (composability criterion proven, not anticipated). Documented in "Diferidos / próximos hotfixes". **NOT tag-ready** until B.4.2-B.4.6 lands.
    - ✅ **B.4.2 DONE 2026-05-04** — `BuildDailyLoginPopup()` private generator (~70 LOC mirroring Group B `BuildRewardPopup` pattern). 900×1200 card with title + 4-col `DayCellContainer` GridLayoutGroup (cell template = buyer/runtime concern, builder ships empty container) + `CountdownLabel` hidden by default (`WarningColor` orange) + primary `ClaimButton` (`SuccessTintColor` green) + secondary `WatchToDoubleButton` hidden by default. All 8 `_refs` wired via `SerializedObject`. `BuildAll` extended: Theme load + dialog if missing (Group B precedent) + `BuildDailyLoginPopup()` + `SaveAssets()` + `Refresh()` + 1/5 progress log. Constants added incrementally per cap-strict: `DailyLoginPath` + `DefaultThemePath` + `SuccessTintColor` + `WarningColor` + `TextLightColor`. Helpers added (composability now satisfied — 4 consumers in B.4.3-B.4.5): `SaveAsPrefab<T>` + `WireAnimatorCard`. Usings added: `KitforgeLabs.MobileUIKit.Catalog.DailyLogin` + `KitforgeLabs.MobileUIKit.Theme`. **NOT tag-ready** until B.4.3-B.4.6 lands.
    - ✅ **B.4.3 DONE 2026-05-04** — `BuildLevelCompletePopup()` private generator (~120 LOC). 900×1300 card with title + optional `LevelLabel` subtitle (hidden by default) + `StarsRow` HorizontalLayoutGroup with 3×160px star Images (initial sprite = `Theme.StarEmptySprite` if available, runtime overrides) + `ScoreLabel` 64pt + `BestScoreLabel` 26pt italic + `NewBestBanner` full-width strip (`SuccessTintColor` + white "NEW BEST!" label, hidden by default) + `Buttons` VerticalLayoutGroup with 3 stacked CTAs (Next primary 96h / Retry secondary 92h / MainMenu secondary 80h hidden by default). All 13 `_refs` wired via `SerializedObject` including `StarImages` array via `arraySize=3` + per-index assignment. `BuildAll` extended: `BuildLevelCompletePopup()` + 2/5 progress log. Constants added: `LevelCompletePath` only (no new colors — existing 5 cover all needs). Helper added: `ForceButtonHeight` (Group B precedent — 3 future consumers in B.4.4-B.4.5 will reuse). Usings added: `KitforgeLabs.MobileUIKit.Catalog.LevelComplete`. **NOT tag-ready** until B.4.4-B.4.6 lands.
    - ✅ **B.4.4 DONE 2026-05-04** — `BuildGameOverPopup()` private generator (~130 LOC). 900×1300 card with HeaderTint full-width 14h strip (placeholder `FailureColor` — popup `ApplyTint` overwrites at Bind via `Theme.FailureColor`) + title "Game Over" 48pt + optional Subtitle italic (hidden default) + ScoreBlock wrapping ScoreLabel 64pt (popup hides block when `Score < 0`) + Buttons VerticalLayoutGroup with 4 stacked CTAs (Continue Ad primary 96h / Continue Currency secondary with internal absolute-positioned CurrencyIcon Image + label "Continue (5)" 96h — popup `ApplyIcon` resolves Theme icon by `_data.ContinueCurrency` runtime + `ApplyContinueCurrencyGating` sets alpha for affordability / Restart secondary 92h / MainMenu secondary 80h). All 15 `_refs` wired via `SerializedObject` (TitleLabel/SubtitleLabel/ScoreBlock/ScoreLabel/ContinueAdButton/ContinueAdLabelText/ContinueCurrencyButton/ContinueCurrencyLabelText/ContinueCurrencyIcon/RestartButton/RestartLabelText/MainMenuButton/MainMenuLabelText/BackdropButton/HeaderTint). `BuildAll` extended: `BuildGameOverPopup()` + 3/5 progress log. Constants added: `GameOverPath` + `FailureColor` (`#E5392B` placeholder mirroring Theme default). No new helpers (all existing reused). Usings added: `KitforgeLabs.MobileUIKit.Catalog.GameOver`. **Live-Theme canonical pattern validated**: HeaderTint color + CurrencyIcon sprite + label text all set runtime via popup `ApplyXxx` methods, not at build-time (contraste con `BuildLevelCompletePopup.NewBestBanner` que sí hardcodea — capturado en Diferidos § Theme banner color hardcoded). **NOT tag-ready** until B.4.5-B.4.6 lands.
    - ✅ **B.4.5 DONE 2026-05-04** — `BuildHUDEnergy()` + `BuildHUDTimer()` private generators (~140 LOC combined). HUDEnergy 280×100 shell con Background semi-transparente + Icon (`Theme.IconEnergy` placeholder) + CountLabel "3" 32pt + MaxCapLabel "/5" 24pt + RegenCountdownLabel "+1 in 04:30" 16pt `WarningColor` + EnergyBarFill horizontal Filled Image `SuccessTintColor` 0.6f preview. 7 `_refs` wired (`_currency=Energy` informativo + base `_refs.IconImage/CountLabel/ClickButton` + `_energyRefs.RegenCountdownLabel/MaxCapLabel/EnergyBarFill`). HUDTimer 220×64 shell con Background + Icon (`Theme.IconClock` placeholder) + Label "00:00" 36pt stretched. 3 `_refs` wired (`_refs.Label/IconImage/ClickButton`). Mode/format/expiry SerializeFields conservan defaults del script (`_mode=CountdownToTarget`, `_formatString="mm\\:ss"`, `_targetUtcIso=""`) — buyer configura via Inspector o `SetTarget()` runtime. `BuildAll` extended: `BuildHUDEnergy()` + `BuildHUDTimer()` + **5/5 progress log**. Constants añadidos: `HUDEnergyPath` + `HUDTimerPath` + `HUDBackgroundColor` (#00000073) + `HUDCapLabelColor` (#D9D9E5). Sin helpers nuevos. Usings añadidos: `KitforgeLabs.MobileUIKit.Catalog.HUD` + `KitforgeLabs.MobileUIKit.Services` (para `CurrencyType.Energy`). HUDs **NO llevan `IUIAnimator`** (spec — HUDs persistentes sin Show/Hide); inline tweens viven en HUD-side fields. **NOT tag-ready** until B.4.6 lands.
    - ✅ **B.4.6 DONE 2026-05-04** — `BuildDemoScene()` (~120 LOC) + `TryAttachStubServices()` (~50 LOC reflexión vs Group B + Group C asmdefs para 5 stubs) + `WireHUDServices()` (~10 LOC) en builder. **Pre-flight gap descubierto y resuelto**: `InMemoryTimeService.cs` NO existía en `Samples~/` — añadido como stub `MonoBehaviour, ITimeService` (~30 LOC) con `_offsetSeconds` Inspector + 3 ContextMenu helpers (skip hour/day/reset). `CatalogGroupCDemo.cs` (~280 LOC) en `Samples~/Catalog_GroupC_Progression/`: MonoBehaviour con 9 SerializeField refs + `Start()` initializer (post-checker C1 fix: `_hudTimer.SetTarget(UtcNow+5min)` para first-Play UX) + 9 individual ContextMenu + 6 HUD ContextMenu + **5 chain ContextMenu scenarios** (LevelComplete → Reward sequence, GameOver Continue Ad/Currency, DailyLogin auto-trigger, Energy regen tick) + 6º mejora I scenario reclassified como HUD helper post-checker C2 fix (categorización honesta: chain = popup→event→popup; timer set ≠ chain) + watch-to-double flow + RewardPopup.ItemCurrencySentinel filter. `BuildAll` extended: `BuildDemoScene()` post-SaveAssets + final user-facing dialog "5 prefabs + 1 scene generated..." mirror Group B UX. Constants añadidos: `ScenePath` + `GroupBRewardPath` + `GroupBStubAsmdef` + `GroupCStubAsmdef` + `TimeServiceTypeName` + `DemoMonoBehaviourTypeName`. Usings añadidos: `UnityEditor.SceneManagement` + `UnityEngine.SceneManagement`. `package.json` `samples[]` entry **added** — 4ª entry "Catalog — Group C — Progression" con descripción completa + dependency note Group B. CHANGELOG done-criteria checkboxes **flipped** (builder + chain demo ✅ / sample stubs ✅). **Triple-round `/_checker as user/dev/qa` ejecutado pre-tag**: 12 findings → 2 FIX NOW aplicadas (C1 first-Play HUDTimer UX + C2 multi-timer ContextMenu rename) + 8 NOTE deferred a M3/M4 (capturadas en CHANGELOG B.4.6 sección triple-round). **B.4.6 cierra Group C — solo B.5 tag pending.**
5. **B.5 Tag** (~0.5 session) — bump 0.6.0 + README install URL + verification fresh-compile + `git tag v0.6.0-alpha`.

**Total remaining**: 2.5-3.5 sessions. **M1 total tracking**: 9-11.5 sesiones (vs original 5-7 — upper-bound del estimate; 8 sesiones consumidas, 2.5-3.5 restantes).

## Path to `v1.0.0-rc` — finishing plan locked 2026-05-03

> Detailed planning artifact: `~/.claude/planning/mobile_ui_templates/_planner_workbook.md` § "Finishing plan — 2026-05-03". This section is the executive summary for `/_start` consumption.

**Compressed proposition**: ship `v1.0.0-rc` when 15-element catalog is feature-complete + 6 deferred hotfixes resolved + Runtime API frozen + buyer-facing docs final (README + CATALOG + Quickstart + MIGRATION). Asset Store submission (store page, marketing, pricing) is OUT of scope.

### Borderline scope decisions (F2)

| Item | Decision |
|---|---|
| Game Wiring sample revival | IN at M2 (Group D delivers IPlayerDataService — the blocker) |
| Hero screenshots README | IN at M4 doc-quality only (NOT marketing-quality) |
| Performance benchmarks | IN at M4 (Galaxy S10 + iPhone 11 baselines documented) |
| Localization path | OUT (Non-goal #2 — BYO; English literals ship in v1.0.0) |

### 4-milestone tree (LOCKED)

| # | Milestone | Tag | Sessions est. | Critical sequencing |
|---|---|---|---|---|
| **M1** | Group C close (3 popups + 2 HUDs + 3 helpers + builder + sample + chain demo) | `v0.6.0-alpha` BREAKING ✅ DONE 2026-05-04 | 9-11.5 (8 sessions consumed; original 5-7 — revised 2026-05-03 per `/_checker as pm` D) | OnUpdate fix MUST land at M3a start, not M1 — HUDTimer (M1) ships with local `private void Update() => OnUpdate()` workaround anchored `// OnUpdate-workaround-M3-sweep`. Helpers shrunk per capability-gate audit (1/3 shipped: `RewardFlow.GrantAndShowSequence`; residual 2/3 locked at M2 kickoff) |
| **M2** | Group D (SettingsPopup + `IPlayerDataService` PlayerPrefs-surface + `IUILocalizationService` (re-skin dispatch) + `PlayerPrefsPlayerDataService` Runtime + `InMemoryPlayerDataService` Sample stub + `InMemoryLocalizationService` Sample stub) + DailyLoginPersistence helper + `InMemoryProgressionService` opt-in retro-fit + `RewardFlow.GrantAndShow` (single, capability-gate re-audit promoted 2026-05-04) + Game Wiring sample REVIVED (8 stubs + VContainer-gated asmdef) | `v0.7.0-alpha` **BREAKING** ✅ DONE 2026-05-04 | 2-3 (delivered in 1 session) | After M1 (all 5 stubs ready). A2 validated 2026-05-04 — `IPlayerDataService` surface = method-per-type (12 methods), 0 deps. **BREAKING justified** despite "M3a is the only BREAKING tag" rule: at alpha→alpha with no buyers blast radius is zero; old `IPlayerDataService` (`GetProfile`/`AddXp`) was stub-era speculative API with zero production consumers — replacement is functionally additive in scope. 8 canonical keys frozen at `v1.0.0-rc`. 261 tests green (+55 from 206). |
| **M3a** | Group E (LoadingScreen + MainMenuScreen) + OnUpdate infra dispatch fix + DailyLogin workaround sweep + RewardPopup.AutoClaim repair + **M3.1ter PopupManager._backdrop sibling-order fix** (sub-milestone: deferred tag-cut at M3 gate when samples-green failed on Group E demo input — fix is 1 line in `ApplySiblingOrder`; gate refined to require button-click + popup-dismiss verification, not only visual render) | `v0.8.0-alpha` BREAKING ✅ DONE 2026-05-06 | 2-3 (delivered in 2 sessions: M3 in-flight + M3.1ter close-out) | M3a starts WITH OnUpdate fix (sweep DailyLogin workaround atomically); HUDTimer in M1 inherits the new dispatch. **Split from original M3** per `/_checker as pm` 2026-05-04 S3 (kitchen-sink mitigation: separates buyer-facing screens + infra fix from polish workstream). 292 tests green (+31 from 261). |
| **M3b** | ~~UIAnim per-element polish (Groups B + C cascades + rollup + stagger + juice)~~ | ~~`v0.8.1-alpha` additive~~ | ~~1-2~~ | **DEFERRED post-`v1.0.0-rc` 2026-05-06** — wrong-premise patch-on-patch incident (sessions 1+2 reverted). Foundational review pending — see [`brief_kitforgelabs_uikit_polish_foundational_review.md`](./brief_kitforgelabs_uikit_polish_foundational_review.md). Path-to-v1.0.0-rc skips M3b; polish ships as `v1.1.0-alpha` (or later) after foundational review concludes |
| **M4** | Hardening core packaging — Theme presets (Casual + Premium), severity icon defaults (Info/Warning/Error), CatalogBuilderBase extraction, master demo scene `Catalog_All_Demo`, ThemedImage runtime read, MIGRATION.md cumulative, README + CATALOG + QUICKSTART final + recipe §§ ("Add your own popup", "Pattern: monetization chain", manual prefab registration, event signature uniformity), perf bench, 6 hero screenshots, buyer fresh-import smoke test, API freeze gate (`CATALOG.md § Public API surface v1.0.0-rc`). **Orientation toggle MOVED to M3c.** | `v0.9.0-alpha` (last alpha pre-rc) | 6-10 (sub-divided M4.1-M4.N atomic, capability-gate per sub-step) | **After M3a** (re-sequenced 2026-05-06 per user directive: *"primero los core, luego polish/UX"* — completar packaging core ANTES de UX audit). All M4 work lives in `[Unreleased]` block until close (no intermediate version bump). Sub-divide atomic per "Pre-flight checker on multi-session subdivisions" rule (Phase 0 done in this session) |
| **M3c** | Deep UX audit del kit completo (post-M4 core packaging) — `as user` + `as ta` + `as pm` cross-group sobre catalog completo + theme presets + master demo + buyer fresh-import journey. **Orientation portrait/landscape decision absorbed here** (path 1/2/3 documented). Output: `kitforgelabs_ux_audit_2026-05.md` con findings P0/P1/P2 + disposition (apply pre-RC / defer post-1.0 / drop). **No code, no tag** — auditoría pura | (no tag — feeds M5/RC or polish) | 1 | **After M4** (re-sequenced 2026-05-06). Audit runs on COMPLETED core packaging — findings de buyer-onboarding, accessibility, orientation evalúan kit final, no kit en construcción |
| **M5 / RC-tag** | Sub-divided M5.1-M5.7 (post-M3c 2026-05-09). M5.1 Premises Runtime (KitforgeRoot.prefab + KitforgeThemeBinder + DTO refactor BREAKING 17 files + multi-font Theme + Theme presets move a Runtime) → M5.2 Hub shell + Setup tab (UIToolkit window + 5-tab sidebar + state model + auto-open + Setup wizard 3-step + Build Everything) → M5.3 Catalog Browser tab (visual grid + thumbnails + drag-to-scene + side panel preview/snippet) → M5.4 Theme Studio tab (live preview Path B + categorías + presets dropdown + Capture All + ThemeConsistencyCheck audit) → M5.5 Test tab + Communication + audit checks (Popup Test Launcher + DTO mock-fill + Force scenarios + LogError actionable + ShowNotification + health badge + UIServicesRefsCheck + ThemeReskinDeltaCheck) → M5.6 HUDSimple + SafeAreaFitter + Help tab + cheat-sheet + Tools menu jerarquía + legacy soft-deprecate → M5.7 Final triple gate + RC tag | `v1.0.0-rc` | 10-11 (sub-divided M5.1-M5.7) | After M3c (audit closed 2026-05-09 with Opción A LOCKED full scope). Original 1-2 sessions estimate revised post-M3c due to Hub Editor window + DTO refactor BREAKING + HUDSimple + SafeAreaFitter + KitforgeRoot prefab additions surfaced by audit. M3b polish workstream sigue deferred post-1.0 hasta foundational review |

**Total estimate** (updated 2026-05-09 post-M3c): **21-22 sessions to `v1.0.0-rc`** (M1+M2+M3a+M4 done = 11 sessions consumed; M3c audit done = 1 session 2026-05-09; **10-11 sessions remaining** sub-divided M5.1-M5.7). Original M5/RC 1-2 estimate revised to 10-11 due to Hub Editor window scope + DTO refactor BREAKING + HUDSimple + SafeAreaFitter + KitforgeRoot prefab additions surfaced by M3c audit (Opción A LOCKED full scope). M3b polish (1-2 sessions) deferred post-`v1.0.0-rc` pending foundational review; individual M3b items pueden promocionarse a M5/RC si user pide.

### Hardening hotfixes — placement (F3 collapse)

Original 6-hotfix list redistributed (no dedicated M4 hardening sweep):

| Hotfix | Where | Why |
|---|---|---|
| OnUpdate infra dispatch fix | **M3a start** | Blocks HUDTimer; sweep atomically removes DailyLogin workaround |
| RewardPopup.AutoClaim repair | M3a (auto-fixed by OnUpdate sweep) | Latent since v0.5.0; sweep restores production correctness |
| UIAnim* per-element polish (Group B + C cascades, rollup, stagger, juice) | **M3b** (additive polish tag — isolates from M3a BREAKING) | Buyer-facing value — Phase 2 promised "skin it once, ship it"; without polish demo looks "wire it yourself" (per `/_checker` 2026-05-03 mejora C). NO post-v1.0.0. M3 split per `/_checker as pm` 2026-05-04 S3: polish goes to additive tag so buyer migration cost is paid only at M3a (BREAKING) |
| `_tween` agent style-aware | post-v1.0.0 (independent) | NOT runtime, NOT buyer-facing; ecosystem tooling — distinct from UIAnim per-element polish above |
| Theme presets shipping (Casual + Premium) | M4 | Doc-quality requires alt-themes to PROVE reskin contract |
| Builder destination overwrite/skip prompt | M4 | UX polish (~1-2 hours) |
| Default severity icon sprites | M4 | Bootstrap Defaults completeness (NotificationToast loses without these) |

### Per-milestone "Done when…" — see workbook § F4 for full criteria

Quick refs:
- **M1**: 5/5 elements + 3 helpers + builder + sample + chain demo + ≥190 tests + tag
- **M2**: SettingsPopup + `IPlayerDataService` (method-per-type primitive surface) + `IUILocalizationService` (re-skin dispatch, 4 members) + `PlayerPrefsPlayerDataService` (Runtime default) + `InMemoryPlayerDataService` (Sample stub) + `InMemoryLocalizationService` (Sample stub) + key namespace `kfmui.<scope>.<name>` documented + **8 canonical keys frozen** (5 settings + 3 dailylogin) + DailyLogin retro-fit via `DailyLoginPersistence` Runtime helper + `InMemoryProgressionService` opt-in `_playerDataServiceRef` (graceful degradation: in-memory if not wired) + cross-session demos PASS for both Settings AND DailyLogin (player launches game day 2 → streak preserved) + language switch demo PASS (change picker → re-skin handler fires globally) + `RewardFlow.GrantAndShow` (single, capability-gate re-audit Group D 2026-05-04 — buyer-facing API; demo retains manual `SpawnPopup` pedagogy with M4 QUICKSTART pattern doc cross-reference) + Game Wiring revival + CHANGELOG line locking `ShopFlow.OpenWithPurchaseChain` as `v1.0.0-rc` OUT + ≥235 tests (≥210 baseline + 16 IPlayerDataService + 10 IUILocalizationService + ≥18 SettingsPopup + ≥5 DailyLoginPersistence + ≥5 RewardFlow.GrantAndShow) + tag
- **M3a**: 2 screens + full app boot demo (Loading→MainMenu→Daily→Play→Pause→GameOver→Main) + OnUpdate infra dispatch fix + DailyLogin workaround sweep + RewardPopup.AutoClaim repair + ≥240 tests + BREAKING tag `v0.8.0-alpha`
- **M3b**: UIAnim per-element polish for Groups B/C + visual QA + ≥292 tests (no regressions; new tests not expected — polish is visual) + additive tag `v0.8.1-alpha`. **IN scope (LOCKED 2026-05-06)**:
  - Group C polish: cell cascade DailyLogin · star cascade + score rollup LevelComplete · CTA cascade GameOver · regen juice HUD-Energy · ticking pulse HUD-Timer
  - Group B catch-ups (enumerated): RewardPopup entry stagger on reward icons · ShopPopup card cascade on offer entries · NotEnoughCurrencyPopup CTA pulse on primary action. **OUT of M3b catch-ups**: HUDCoins/HUDGems (numbers-only, no card layout to stagger) · ConfirmPopup/PausePopup/TutorialPopup/NotificationToast (Group A — frozen at `v0.4.0-alpha`, polish only on user request)
  - Canvas reference: `1080×1920 portrait` (per CATALOG.md § 3.1 MUST 6); fps-floor 30fps; target = Editor + 1 dispositivo físico genérico (cualquiera). Below 30fps → polish degrada gracefully (DOTween's frame-skip behavior is acceptable; no manual skip-cascade path en M3b)
  - Cascade-helper extraction criterion: si ≥3 elements comparten el mismo `Sequence` stagger pattern, extraer `UIAnimStaggerSequence` a `Runtime/Catalog/_Internal/`; si <3, mantener inline (philosophy §5 — composability cuando hay reuso probado, no anticipado). Decisión se aplica al cierre de M3b, no al kickoff
- **M3b — Out of scope (LOCKED 2026-05-06)**:
  - Theme banner color hardcoded en builders (línea 56 Diferidos) → **defer M4 hardening sweep**. Fix runtime requiere `ThemedImage` componente runtime + edits en builders A/B/C/D; no encaja en additive text-only/visual-only de M3b
  - Inheritance-based extension de UIAnim* → sealed canonical confirmado (línea 55 Diferidos resuelta). NO se unsea
  - Orientation portrait/landscape toggle → **defer M3c (deep UX audit)**, NO se absorbe a M3b. Cambio de orientation es contrato de canvas (BREAKING-light), no polish
  - Master demo scene `Catalog_All_Demo/` → M4 (línea 62 Diferidos)
  - Hero screenshots → M4 (post-polish para evitar reshoot)
- **M4** (re-sequenced 2026-05-06 — runs BEFORE M3c): 2 themes shipped + 4 icon sprites + CatalogBuilderBase extraction + ThemedImage runtime + perf bench docs + 6 hero screenshots + **master demo scene `Samples~/Catalog_All_Demo/`** + **buyer fresh-import smoke test passes** + 4 final docs (README + CATALOG + QUICKSTART incl. **"Add your own popup" §** + **"Pattern: monetization chain" §** pointing at `CatalogGroupBDemo.SpawnChainShop` 70-line literal + manual prefab registration § + event signature uniformity § + MIGRATION) + **API freeze documented in `CATALOG.md § Public API surface v1.0.0-rc`** + tag `v0.9.0-alpha`. **Orientation toggle decision MOVED to M3c** (was M4 conditional). All work in `[Unreleased]` block until close — no intermediate version bump
- **M3c** (re-sequenced 2026-05-06 — runs AFTER M4): deep UX audit del kit completo (post-M4 core packaging) — **Done when**:
  - `kitforgelabs_ux_audit_2026-05.md` written en `~/.claude/memory/` con findings de 3 roles (`as user` + `as ta` + `as pm`) cross-group (A→E + Foundation + theme presets + master demo)
  - Findings priorizados P0 / P1 / P2 con disposición explícita (apply pre-RC / defer post-1.0 / drop)
  - **Orientation portrait/landscape toggle decision documentada** (path 1/2/3 elegido — absorbida desde M4 en re-secuencia 2026-05-06)
  - Buyer fresh-import journey simulado paso a paso (UPM URL → import sample → press Play) con time-to-value medido en minutos
  - Mejoras alineadas table (convergent findings ≥2 roles ranked first) → input directo a M5/RC disposition
  - Zero código tocado · zero tag · output = 1 markdown file en memory/ + entry en CHANGELOG `[Unreleased]` `### Audit` referenciando el findings file
- **M5 / RC-tag**: M3c findings disposition + final triple gate + tag — **Done when**:
  - P0 findings de M3c aplicados (text-only o code per finding; capability-gate per item)
  - P1+P2 deferred a post-1.0 documentados en CHANGELOG `### Deferred post-1.0`
  - Verify-all-docs gate green (README + CATALOG + QUICKSTART + CHANGELOG + MIGRATION + package.json end-to-end read)
  - Verify-all-samples gate green (todos los samples del `samples[]` array: import + builder + Play + first-button-click + popup-dismiss + ContextMenu trigger)
  - Verify-all-tests gate green (suite EditMode completa on fresh compile, `Library/ScriptAssemblies` borrado pre-test)
  - Tag `v1.0.0-rc` + push origin/main

### Surviving assumptions (load-bearing, unvalidated)

- A1: OnUpdate infra fix = single session (verify at M3a start by auditing `PopupManager.cs` dispatch wiring complexity)
- A2: `IPlayerDataService` design = 1-session spec. ✅ **VALIDATED 2026-05-04** at M2 kickoff via 30-min surface sketch + `/_checker as dev`. Outcome: method-per-type primitive surface (`GetInt/SetInt/GetFloat/SetFloat/GetString/SetString/GetBool/SetBool/Has/Delete/Save/Reload`) — mirrors `PlayerPrefs`, 0 deps (no Newtonsoft), no JSON layer, no versioning sub-system. Sprawl risks (Newtonsoft dep + `Get<T>` boxing + cross-version key migration) eliminated by primitive-typed contract + frozen kit-side keys at `v1.0.0-rc` + `kf_<scope>.<name>` namespace convention. M2 estimate (2-3 sessions) sticks. Buyer with custom SaveSystem implements 12 trivial methods.
- A3: Game Wiring revival = ~30 min update of 5 stubs (validate at M2 mid-point)
- A4: Asset Store reviewers accept English-only at v1.0.0-rc (NOT validatable until submission — shipped as Non-goal #2)
- A5: Galaxy S10 + iPhone 11 = sufficient device coverage (validate via Asset Store competitor research before M4)

## M3c UX audit summary — 2026-05-09 (audit closed)

Full audit: `./kitforgelabs_ux_audit_2026-05.md` (4 blocks, 130 findings total). Highlights below.

### 7 UX Pillars LOCKED (rectores permanentes del package)

| # | Pilar | Test rápido |
|---|---|---|
| **P1** | Single front door | "¿Una sola ventana hace todo el lunes?" |
| **P2** | Show, don't tell | "¿VEO o tengo que LEER?" |
| **P3** | Progressive disclosure | "¿No-coder se asusta al primer abrir?" |
| **P4** | No dead-ends | "¿Hay siempre next step obvio?" |
| **P5** | Unity-native | "¿Se siente como Unity?" |
| **P6** | Predictable defaults | "¿Lo obvio funciona sin configurar?" |
| **P7** | Persistent context | "¿Vuelvo el martes y sigo donde estaba?" |

### 7 Hub features driven (deliverable central v1.0.0-rc)

- **F-UX-1** Kitforge Hub Editor window (UIToolkit, 5 tabs vertical sidebar: Setup / Catalog / Theme / Test / Help, dockable, auto-open on import).
- **F-UX-2** Setup wizard 3-step persistente (Initialize Project → Add Scene Root → Hello World).
- **F-UX-3** Catalog Browser tab (visual grid 17 prefabs + thumbnails via PrefabSnapshotCapture + drag-to-scene + side panel preview/snippet/DTO).
- **F-UX-4** Theme Studio tab (live preview mock popup + 6 slot categorías colapsables + presets dropdown + Capture All Snapshots).
- **F-UX-5** Popup Test Launcher tab (Play-mode-only · 17 popups DTO mock-fill · Force scenarios panel: Force Day N, Coins=0, Skip Hour).
- **F-UX-6** Communication actionable (LogError next-step format + `EditorWindow.ShowNotification` toasts + health badge header).
- **F-UX-7** Polish post-RC (P1) — search global · shortcuts · iconos · USS unified · `[KitforgeCatalogEntry]` API.

### 5 Premises Runtime non-Hub (M5.1 critical path)

- **R-U-C1** `Runtime/Bootstrap/KitforgeRoot.prefab` + `KitforgeThemeBinder` MonoBehaviour (theme single-field distribuye a 3 managers Awake).
- ~~**R-U-C2** DTO refactor BREAKING 17 files (constructor-style → Inspector-friendly `[Serializable]` + [SerializeField] fields).~~ **INVALIDATED 2026-05-09 (M5.1 post-discovery)** — empirical audit de los 13 DTOs encontró que ya están en formato target; refactor cancelado. R-U-C2 cheat-sheet + Catalog Browser snippets sub-items siguen válidos (M5.2 / M5.3).
- **R-U-C3** `HUDSimple` + `HUDEnergySimple` + `HUDTimerSimple` prefabs/scripts (sin servicios; `SetValue(int)` API directa).
- **R-U-C5** Theme presets move (Default/Casual/Premium) de sample M4.1 a `Runtime/Theme/Presets/` + multi-font (`_titleFont` + `_bodyFont` + `_labelFont` slots).
- **R-U-C7** `Runtime/Mobile/SafeAreaFitter.cs` MonoBehaviour minimal (aplica `Screen.safeArea` a RectTransform en OnEnable). **Inverts Non-goal #7** — README updated.

### 4 Decisions LOCKED 2026-05-09

- **D1** Hub scope: v1.0.0-rc (split por sub-milestones internos M5.1-M5.7). Razón: Hub define identidad del kit; sin ella, P1 single-front-door violado y 50 findings re-emergen.
- **D2** Implementation: **UIToolkit** (no IMGUI). Razón: Unity 6 dirección oficial + USS theming + dockable nativo + future-proof live preview/drag-drop.
- **D3** Catalog auto-discovery: **17 kit-only en v1.0.0-rc**; `[KitforgeCatalogEntry]` attribute API en v1.1.0. Razón: kit-only es atomic + testeable; attribute API requiere PrefabSnapshotCapture API freeze.
- **D4** In-Editor toasts: **`EditorWindow.ShowNotification` API standard** (text-only + Hub auto-foreground). Razón: Unity-native (P5); custom widget = scope creep sin valor. RE-LOCKED post-Bloque 4: ShowNotification no soporta button → action delivery vía Hub auto-foreground, no in-toast button.

### Workshop-drawer test rule LOCKED

`~/.claude/memory/feedback_audit_priority_tool_first.md` (HARD RULE for KF MobileUIKit + future commercial tools). Tool reliability + prefab practicality + reorganization come FIRST. Asset Store / demos / buyer-facing docs are LAST until user explicitly opens that scope. Apply to all subsequent audits/recommendations on this kit.

### Top 3 riesgos técnicos M5/RC (Bloque 4)

1. ~~**R-U-C2 DTO refactor BREAKING 17 archivos** — migration en consumer-imported samples bajo `Assets/Samples/`. Mitigation: scripted migration tool + clear MIGRATION block en CHANGELOG. Impact alto, prob media.~~ **RESOLVED 2026-05-09 — INVALIDATED, no refactor needed.** Risk #1 slot now open; promote next actual risk if any surfaces during M5.2-M5.7.
2. **D-D.1 Theme live preview Path B latency** — 100-200ms re-render perceptible. Mitigation: Path A (RenderTexture in-memory) reservado v1.1.0 si test real revela problema. Impact medio, prob baja.
3. **D-A.2 Auto-open on import timing** — domain reload race. Mitigation: `EditorApplication.delayCall` + EditorPrefs first-time marker + exhaustive testing fresh project install. Impact medio, prob media.

---

### Out of scope (explicitly deferred, LOCKED)

Asset Store submission (store page, marketing-quality screenshots, demo video, pricing lock) · localization runtime · multi-orientation auto-handling · UI Toolkit support · Editor authoring window · DOTween Pro redistribution · Runtime DI container · `ShopFlow.OpenWithPurchaseChain` helper (chain shape opinionated; QUICKSTART monetization-chain pattern recipe ships instead — locked 2026-05-04 capability-gate re-audit).
