# Changelog

All notable changes to this package will be documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

_Last updated: 2026-05-01_

## [Unreleased]

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
