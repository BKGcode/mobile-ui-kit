# Changelog

All notable changes to this package will be documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

_Last updated: 2026-05-01_

## [Unreleased]

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

### Fixed
- UIRouter.IsValidPopup: bloque corrupto por merge fallido reparado (commit 2175e6f).
- PopupManager eviction: re-encolaba con Data=null perdiendo estado del popup desalojado.

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
