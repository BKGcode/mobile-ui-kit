---
name: kitforgelabs_ux_audit_2026-05_bloque4
description: M3c UX audit — Bloque 4 (as dev · técnica + plan aterrizaje M5.X). Split from kitforgelabs_ux_audit_2026-05.md (monolith >800). Index + Bloque 2 in the index file; Bloque 3 + Decisiones LOCKED in kitforgelabs_ux_audit_2026-05_bloque3.md.
type: project
status: active
---

## Bloque 4 (2026-05-09) — `as dev` técnica + plan aterrizaje M5.X

### Persona LOCKED — "el senior Unity dev del studio"

**Quién**: senior Unity dev (5+ años Unity, conoce UIToolkit + IMGUI + AssetDatabase + Editor APIs profundo). Lee el blueprint UX del Bloque 3 (Pillars + 7 features Hub) y valida viabilidad técnica antes del kickoff M5/RC. Su pregunta de oro: *"¿hay alguna trampa técnica que invalide el plan UX o que duplique el coste de implementación?"*.

**Mandato**: encontrar tradeoffs y riesgos REAL técnicos que podrían reventar fechas o degradar UX. Sin pretensión de auditar todo el kit; solo **lo que la Hub tocará** + **lo que los Pillars exigen**.

---

### 30 findings técnicos

> Format: `[D-X.N]` **Título**. UX intent → tech reality → recomendación + scope estimate.

#### Sección A — Hub window architecture (5)

**[D-A.1] UIToolkit no tiene tabbed view nativo — custom impl**
*UX intent*: 5 tabs verticales (Setup/Catalog/Theme/Test/Help).
*Tech reality*: UIToolkit no expone `TabView` element built-in en Unity 6.0/6.1 (hay [Unity Forum proposal](https://forum.unity.com/threads/tabview-uitoolkit) pero no shipping). Hay que implementar custom: VisualElement sidebar + content area + state machine para active tab.
*Tradeoff*: ~150 LOC custom. Riesgo bajo, patrón conocido. Existen libs comunidad (e.g. `UnityToolbarExtender`) pero no alineadas con UIToolkit.
*Recomendación*: **GO**. Custom tabs (~150 LOC) + USS for active state. **Scope: ~3h**.

**[D-A.2] Auto-open on import — PackageManager event timing es traicionero**
*UX intent*: Hub auto-opens primer importar kit (UX-B.8 P0).
*Tech reality*: `UnityEditor.PackageManager.Events.registeredPackages` se dispara *durante domain reload* — `EditorWindow.GetWindow` puede fallar o crear ventana fantasma. Workaround conocido: usar `[InitializeOnLoadMethod]` + `EditorApplication.delayCall` para deferir hasta after-domain-reload-stable.
*Tradeoff*: 1ª-vez detection requires marker (EditorPrefs key `kitforge.hub.first_open_done`) para no re-disparar en cada Editor restart.
*Recomendación*: **GO** con patrón delayCall + EditorPrefs marker. **Scope: ~1h**.

**[D-A.3] Dockable EditorWindow + state persistence post Domain Reload**
*UX intent*: Hub recuerda last tab + setup progress (P7).
*Tech reality*: Standard EditorWindow es dockable nativo ✅. Pero state survival on Domain Reload requiere `[SerializeField]` fields persisted via `[OnEnable]` rehydration desde EditorPrefs. UIToolkit `VisualElement` state se pierde — hay que serializar el state model, no la VisualElement.
*Tradeoff*: hay que separar State (serializable POCO) de View (VisualElement tree). Patrón MVVM-light.
*Recomendación*: **GO** con `KitforgeHubState` POCO + `KitforgeHubWindow` view. State persists via `EditorJsonUtility` + EditorPrefs key. **Scope: ~3h** patrón base reusable.

**[D-A.4] Settings storage: EditorPrefs vs SessionState vs ProjectSettings**
*UX intent*: Settings (auto-open, show advanced, etc.) persistentes per-user.
*Tech reality*: 3 stores Unity:
- `EditorPrefs` = global per-user-per-machine (todos los projects). Apt for "show advanced" toggle.
- `SessionState` = per-Editor-session, no survive restart. Apt for "current tab" durante session.
- `ProjectSettings/KitforgeUIKit.asset` = per-project, version-control friendly. Apt for studio team-shared settings.
*Tradeoff*: usar 3 stores apropiadamente o sobre-simplificar. Mezclar stores = confusión.
*Recomendación*: **GO**. Settings UI toggles → `EditorPrefs`; current tab + scroll position → `SessionState`; shared studio defaults → `ProjectSettings/KitforgeUIKit.asset` ScriptableSingleton. **Scope: ~2h** decision + impl.

**[D-A.5] Hub size: UIToolkit + USS + 17 thumbnails — package footprint impact**
*UX intent*: Catalog Browser thumbnails 17 prefabs visibles.
*Tech reality*: thumbnails son SHA256-hashed PNG snapshots generados via `PrefabSnapshotCapture`. **No shipped in package** — generados on-demand en consumer project a `Library/UIKitAudit/Snapshots/` (gitignored). Hub thumbnails son `Texture2D` cargados desde esos paths. Package no engorda.
*Tradeoff*: si user no corre `Capture All` aún → thumbnails missing → fallback "no preview" placeholder.
*Recomendación*: **GO**. Hub muestra placeholder + button "Capture All to populate previews" si missing. **Scope: ~1h** UI fallback.

#### Sección B — Setup wizard (4)

**[D-B.1] Initialize Project idempotency: AssetDatabase race conditions**
*UX intent*: re-run safe (UX-C.10 P0).
*Tech reality*: `AssetDatabase.CreateFolder` + `CreateAsset` chained — si interrupted (Editor crash mid-create) → state inconsistent. Necesita `AssetDatabase.StartAssetEditing()` + `StopAssetEditing()` para atomicity. + check-existence-first per asset.
*Tradeoff*: complica el code, pero es estándar Editor pattern. Sin esto: re-run puede crear duplicates.
*Recomendación*: **GO**. Wrap en StartAssetEditing/StopAssetEditing block. **Scope: ~1h**.

**[D-B.2] Add Scene Root detection: KitforgeRoot via component vs GUID**
*UX intent*: detect KitforgeRoot ya en escena → ping en lugar de duplicate (UX-C.9 P0).
*Tech reality*: detect via `FindObjectOfType<KitforgeThemeBinder>()` (component-based) > GUID-based. Component survive prefab variants, GUID-check rompe en variants.
*Tradeoff*: `FindObjectOfType` slow en scenes grandes (~1k+ GameObjects); aceptable para Setup ocasional.
*Recomendación*: **GO** component-based. **Scope: ~30min**.

**[D-B.3] Build Everything orchestration: mid-fail handling**
*UX intent*: 1 click ejecuta A→B→C→D→M4.1→E (UX-C.8 P0).
*Tech reality*: 6 builders chained. Si Group C falla mid-execution → state parcial (Group A+B prefabs creados, Group C+D+E missing). Audit detecta el desorden post-facto, pero el user queda con scene corrupta.
*Tradeoff*: rollback completo es expensive (mucho AssetDatabase work). Mejor: **fail-fast con clear error** + button "Retry from where you stopped" en Hub.
*Recomendación*: **GO** con per-builder try/catch + Hub state remembers last-successful-step + retry resume. **Scope: ~3h**.

**[D-B.4] Hello World requires Play mode — gating + state**
*UX intent*: button disabled si no Play (UX-C.5 P0).
*Tech reality*: trivial: `Application.isPlaying` check on button enable. Pero "Run Hello World" en Play mode requiere PopupManager presente en scene. Si user no hizo Step 2 (Add Scene Root) → no PopupManager → crash.
*Tradeoff*: forzar orden secuencial Step 2 → Step 3 vs permitir click pero showing actionable error.
*Recomendación*: **GO**. Step 3 button disabled hasta Step 2 done (state machine). + Tooltip "Complete Step 2 first". **Scope: ~30min** (parte del state model en D-A.3).

#### Sección C — Catalog Browser (4)

**[D-C.1] Thumbnail render: PrefabSnapshotCapture API binding**
*UX intent*: visual grid 17 prefabs con thumbnails.
*Tech reality*: `PrefabSnapshotCapture` ya existe en `Editor/Audit/Snapshots/`. Genera 1080×1920 PNG. Para Hub grid (~200×300px cards) hay que escalar — Unity `Texture2D.Resize` + reimport, o load full-size + render escalado en VisualElement Image (UIToolkit hace el resize automático).
*Tradeoff*: load full-size 1080×1920 × 17 thumbnails = ~50MB RAM peak. Aceptable para Editor. Para mobile build sería problema; aquí no aplica.
*Recomendación*: **GO** con UIToolkit Image escalado. Si memoria preocupa: lazy-load on tab open + Texture2D.Compress post-load. **Scope: ~2h**.

**[D-C.2] Drag-to-scene: DragAndDrop API + auto-register**
*UX intent*: drag thumbnail al Hierarchy → instancia + register en PopupManager (UX-D.3 P0).
*Tech reality*: UIToolkit drag-and-drop API requiere `RegisterCallback<MouseDownEvent>` + `DragAndDrop.PrepareStartDrag` + `DragAndDrop.StartDrag`. Drop al Hierarchy es Unity-native — Hierarchy interpreta el `DragAndDrop.objectReferences` automáticamente y instancia. Auto-register en PopupManager requiere hook post-drop, vía `EditorApplication.hierarchyChanged` event + detect new instance.
*Tradeoff*: hierarchyChanged dispara para *cualquier* cambio — overhead. Mejor: subscribe only durante drag operation, unsubscribe on drop.
*Recomendación*: **GO** con subscription temporal. **Scope: ~3h** + edge cases (drop fuera de Hierarchy → silent fail OK).

**[D-C.3] Side panel slide animation — UIToolkit transitions vs simple show/hide**
*UX intent*: panel desliza desde derecha (UX-D.2 nice).
*Tech reality*: UIToolkit USS transitions (`transition: translate 200ms ease-out`) son nativas y fluidas. Trivial.
*Tradeoff*: ninguno relevante.
*Recomendación*: **GO** USS transition. **Scope: ~30min**.

**[D-C.4] Custom popup auto-discovery API (post-RC v1.1.0)**
*UX intent*: `[KitforgeCatalogEntry]` attribute (D3 LOCKED diferido).
*Tech reality*: requires attribute + `TypeCache.GetTypesWithAttribute` scan + thumbnail generation API exposed para custom prefabs. Thumbnail generation depende de `PrefabSnapshotCapture` siendo Editor-API estable.
*Tradeoff*: API surface freeze en `PrefabSnapshotCapture` para que buyer use el mismo path. Pre-RC = API en flux; freeze blockea iteración.
*Recomendación*: **DEFER v1.1.0**. v1.0.0-rc ships con kit-only catalog. **Scope post-RC: ~5h** API design + impl.

#### Sección D — Theme Studio (4)

**[D-D.1] Live preview render: RenderTexture vs reuse existing prefab + capture**
*UX intent*: live preview mock popup re-renders al cambiar slot (UX-E.1 P0).
*Tech reality*: 2 paths:
- **Path A**: dedicar un mock popup MonoBehaviour rendered en RenderTexture, asignado a UIToolkit Image. Pros: live, sin disk I/O. Cons: render off-screen UGUI requires camera + RT setup, ~80 LOC.
- **Path B**: re-trigger `PrefabSnapshotCapture` on slot change → load PNG → Image. Pros: reuses existing Editor pipeline. Cons: disk I/O per change, latency ~100-200ms (perceptible).
*Tradeoff*: A es más reactivo (instant), B reusa código probado.
*Recomendación*: **SPLIT** — v1.0.0-rc usa **Path B** (simple, reuses); v1.1.0 considera Path A si latency probada como problema. **Scope v1.0.0-rc: ~2h**.

**[D-D.2] Slot category foldouts — UIToolkit Foldout state persistence**
*UX intent*: 6 categorías colapsables (UX-E.2 P0). Default Brand colors expanded.
*Tech reality*: UIToolkit `Foldout` element nativo ✅. State (expanded/collapsed) persiste vía `value` binding a SessionState.
*Recomendación*: **GO**. **Scope: ~1h**.

**[D-D.3] Theme presets dropdown — detect Theme assets en `Runtime/Theme/Presets/`**
*UX intent*: dropdown Default / Casual / Premium / + New (UX-E.4 P0).
*Tech reality*: `AssetDatabase.FindAssets("t:UIThemeConfig", new[] { "Runtime/Theme/Presets" })` para enumerar. + watch para new themes via `AssetPostprocessor`.
*Tradeoff*: Theme assets en `Runtime/` shipped al consumer — requiere R-U-C5 (move presets de sample M4.1 al Runtime). LOCKED.
*Recomendación*: **GO**. Premise = R-U-C5 ejecutado primero (move presets). **Scope: ~1h** Hub + ~1h move presets.

**[D-D.4] Theme drift detection: ThemeConsistencyCheck audit**
*UX intent*: flag drift entre 3 managers theme refs (R-C2 Bloque 1).
*Tech reality*: post-KitforgeThemeBinder (R-U-C1) los 3 managers reciben theme single-source → drift IMPOSIBLE arquitectónicamente. Pero defense-in-depth:check audita `UIManager._themeConfig == PopupManager._themeConfig == ToastManager._themeConfig` por demo scenes.
*Tradeoff*: redundante post-KitforgeThemeBinder, pero cheap (~30 LOC audit check).
*Recomendación*: **GO** defense-in-depth. **Scope: ~1h**.

#### Sección E — Test Launcher + Communication (4)

**[D-E.1] Play mode gating + DTO mock-fill UI auto-generation**
*UX intent*: Test tab list popups + DTO mock fields editables (UX-E.7 P0).
*Tech reality*: DTO mock fields auto-generation via `SerializedObject` introspection sobre wrapper ScriptableObject que contenga el DTO instance. Trick: cada DTO needs to be `[Serializable]` (R-U-C2 ya P0). Then create temporary SO at runtime + render via UIToolkit `PropertyField`.
*Tradeoff*: requires R-U-C2 (DTO Inspector-friendly) ya hecho. Premise crítica.
*Recomendación*: **GO**. Premise = R-U-C2 lockada como BREAKING en M5/RC. Sin ella, Test Launcher dégrada a "click Show with hardcoded defaults". **Scope: ~4h** UI + ~separate work R-U-C2.

**[D-E.2] Force scenarios — ContextMenu reflection invocation on stubs**
*UX intent*: checkboxes "Day N daily login", "Coins=0", etc. (UX-E.8 P0).
*Tech reality*: stubs (`InMemoryProgressionService`, `InMemoryEconomyService`, etc.) deben exponer methods o ContextMenu items invocables. Hub finds them via `FindObjectsOfType<MonoBehaviour>` + reflection on `[KitforgeForceScenario]` attribute. Premise = stubs annotate methods con attribute.
*Tradeoff*: requires stubs to be in Play scene. Si user usa real services en Play → no stubs disponibles → "Force scenarios" panel muestra "no in-memory stubs detected — connect a stub service or use Inspector ContextMenu directly".
*Recomendación*: **GO**. Annotate stubs con `[KitforgeForceScenario("Force Day {0}", typeof(int))]`. **Scope: ~3h** Hub + ~1h stub annotations.

**[D-E.3] LogError actionable — Console no soporta clickeable buttons**
*UX intent*: error message con "Click to fix" button (UX-E.9 P0).
*Tech reality*: Unity Console renders LogError plain text. **NO** soporta buttons inline. Workaround: log message con clear "Open Hub: <action>" instruction + `[OpenHubFromLogContextMenu]` static method registered as `Object` context-click target via `LinkClickedEventArgs`.
*Tradeoff*: clickable links en Console son posible (`<a href="...">` tags) pero no buttons.
*Recomendación*: **RE-WORK plan**. Opciones:
  - **(a)** LogError con `<a href="kitforge://open-hub-setup">` link (Unity Console lo hace clickable, opens browser pero con custom protocol handler interno = posible).
  - **(b)** LogError plain + complementar con in-Editor toast (D-E.4 abajo) que sí soporta button.
  - **(c)** Aceptar limitación: LogError text-only + "Open Kitforge Hub for fix" instruction. Toast hace el actionable lift.
*Recomendación final*: **(b)+(c)** — LogError instructional + toast con button para fixes en running Editor session. **Scope: ~2h**.

**[D-E.4] In-Editor toasts — `EditorWindow.ShowNotification` API limits**
*UX intent*: toast con button "Wire it" (UX-E.10 P0).
*Tech reality*: `EditorWindow.ShowNotification(GUIContent, double fadeoutWait)` muestra notification BUT solo texto + icon, NO button. Para action: el patrón Unity usa `SceneView.AddOverlayToActiveView` o custom `EditorWindow` modal.
*Tradeoff*: D4 LOCKED dijo "ShowNotification standard". Reality: ShowNotification NO basta para "actionable" UX intent. Requiere RE-WORK D4.
*Recomendación*: **RE-WORK D4**. Opciones reales:
  - **(a)** ShowNotification (texto-only) + auto-dock/foreground Hub Setup tab para que user vea fix instruction. Cero buttons in-toast.
  - **(b)** Custom popup overlay (no es ShowNotification) con button. ~150 LOC + USS. Less Unity-native pero más actionable.
*Recomendación final D4 RE-LOCK*: **(a)** ShowNotification + Hub auto-foreground si Hub abierto. Si Hub cerrado: notification con texto "Open Kitforge Hub → Setup → wire IEconomyService". User abre Hub manual. **Scope: ~2h**.

#### Sección F — Cross-cutting / arquitectura (5)

**[D-F.1] Hub Editor folder layout — discoverable structure**
*UX intent*: implementación organizada para mantener sin caos.
*Tech reality*: propuesta:
```
Editor/
  Hub/
    KitforgeHubWindow.cs          (entry, [MenuItem])
    KitforgeHubState.cs            (POCO state)
    Tabs/
      SetupTab.cs · CatalogTab.cs · ThemeTab.cs · TestTab.cs · HelpTab.cs
    Components/
      HealthBadge.cs · TabSidebar.cs · ToolBar.cs
    Resources/
      KitforgeHub.uxml · KitforgeHub.uss · icons/*.png
```
*Recomendación*: **GO** layout above. **Scope: trivial estructura**.

**[D-F.2] Hub asmdef — separate from `Editor/` root or in-place?**
*UX intent*: build separation, isolation.
*Tech reality*: 2 opciones:
- (a) Hub vive en mismo asmdef `KitforgeLabs.MobileUIKit.Editor` como Audit + Generators.
- (b) Hub asmdef separado `KitforgeLabs.MobileUIKit.Editor.Hub` referencing Audit + Generators.
*Tradeoff*: (b) compile-time isolation pero overhead asmdef + dependencies wiring. (a) simple, todo Editor en un compile unit.
*Recomendación*: **GO** opción (a) — single Editor asmdef. Solo splitea si compile times se vuelven problema (>5s recompile). **Scope: 0**.

**[D-F.3] Test coverage — cómo testear UIToolkit windows**
*UX intent*: tests automatizados para Hub features (P1 R-C7 Bloque 1).
*Tech reality*: UIToolkit Editor windows son testeable via `EditorWindow.GetWindow<T>()` + query VisualElement tree con `rootVisualElement.Q<T>(name)` + simulate clicks via `VisualElement.SendEvent`. EditMode tests funcionan.
*Tradeoff*: integration tests son lentos (open window + close + repeat). Mejor: test state model (`KitforgeHubState` POCO) directly en EditMode.
*Recomendación*: **GO** EditMode unit tests sobre `KitforgeHubState` (state machine, idempotency, persistence). Integration tests mínimos sobre window open/close/tab-switch. **Scope: ~3h** test suite.

**[D-F.4] Migration path: legacy menu items deprecated o hard-removed?**
*UX intent*: 9 menu items legacy (Bootstrap Defaults, Build Group A-E, etc.) movidos a Developer submenu post-Hub.
*Tech reality*: opciones:
- (a) Hard-remove: legacy menu items deleted. Usuarios con muscle memory rompen. **BREAKING UX**.
- (b) Soft-deprecate: legacy menu items siguen funcionales pero mueven a `Tools → Kitforge → UI Kit → Developer (legacy) → ...` con tooltip "Deprecated; use Hub instead". v1.0.0-rc → 1 release con ambos. v2.0.0 → remove.
*Tradeoff*: (a) cleaner, (b) friendly migration.
*Recomendación*: **GO** opción (b). Legacy queda en Developer submenu por v1.x. **Scope: ~1h** menu reorganization.

**[D-F.5] Domain Reload survival — Hub state persists?**
*UX intent*: P7 persistent context — Hub recuerda donde dejaste.
*Tech reality*: Domain Reload destruye toda VisualElement tree. `EditorWindow` is recreated. State POCO debe estar `[Serializable]` + `[SerializeField]` field en window class para survive.
*Tradeoff*: complica state model pero estándar Unity Editor pattern. Sin esto: Domain Reload reset wizard progress = P7 violado.
*Recomendación*: **GO**. Cubierto por D-A.3 (state POCO serializable). **Scope: included en D-A.3**.

#### Sección G — Scope estimate global (3)

**[D-G.1] Total scope Hub + 7 features — sesiones estimate**
*Análisis acumulado*:
- Hub shell + 5 tabs base + state model + dockable + auto-open: **~12h** (~2 sesiones).
- Setup wizard 3-step + idempotency + Build Everything orchestration: **~6h** (~1 sesión).
- Catalog Browser grid + thumbnails + drag-to-scene + side panel: **~9h** (~1.5 sesiones).
- Theme Studio live preview (Path B) + categorías + presets dropdown: **~6h** (~1 sesión).
- Test Launcher + DTO mock-fill (premise R-U-C2 done) + Force scenarios: **~8h** (~1.5 sesiones).
- Communication: LogError actionable + ShowNotification + health badge: **~4h** (~0.5 sesión).
- Tests EditMode Hub state: **~3h** (~0.5 sesión).
- Premises (no Hub work but blockers): R-U-C1 KitforgeRoot prefab (~3h) + R-U-C2 DTO refactor BREAKING (~6h, 17 archivos) + R-U-C3 HUDSimple (~4h) + R-U-C5 Theme presets move + multi-font (~4h) + R-U-C7 SafeAreaFitter (~2h).
- **Total Hub work**: ~48h = ~8 sesiones.
- **Total premises Runtime**: ~19h = ~3 sesiones.
- **Total v1.0.0-rc estimate**: **~67h = ~10-11 sesiones**.

*Recomendación*: SPLIT en 7 sub-milestones M5.X (abajo).

**[D-G.2] Critical path: cuál orden minimiza bloqueos**
*Análisis*:
1. **R-U-C2 DTO refactor BREAKING** primero — bloquea Test Launcher (D-E.1) y Catalog snippets.
2. **R-U-C1 KitforgeRoot prefab + KitforgeThemeBinder** — bloquea Setup wizard (D-B.2).
3. **Hub shell + 5 tabs base** — esqueleto.
4. **Setup tab (wizard 3-step)** — primer feature visible.
5. **Catalog tab + thumbnails** — segundo feature, R-U-C2 ya done.
6. **Theme tab** — tercer feature, presets premise ya done.
7. **Test tab** — cuarto feature, Force scenarios annotations en stubs en paralelo.
8. **Communication** — toasts + badge + audit checks (R-C3 Bloque 1) en paralelo.
9. **R-U-C3 HUDSimple + R-U-C7 SafeAreaFitter + R-U-C5 multi-font** — Runtime work, paralelo.
10. **Final triple gate + tag**.

**[D-G.3] Riesgos top 3 (probabilidad × impacto)**
1. **R-U-C2 DTO refactor 17 archivos** — risk: BREAKING migration en consumer-imported samples bajo `Assets/Samples/`. Mitigation: scripted migration tool + clear MIGRATION block en CHANGELOG. **Impact alto, prob media**.
2. **D-D.1 Theme live preview latency Path B** — risk: 100-200ms re-render perceptible degrada UX P2 "show don't tell". Mitigation: Path A fallback ready si testing real revela problema. **Impact medio, prob baja**.
3. **D-A.2 Auto-open on import timing** — risk: domain reload race condition → Hub no abre primer importar → P3 onboarding violado. Mitigation: delayCall + exhaustive testing en fresh project install. **Impact medio, prob media**.

---

### Disposition Bloque 4 — viabilidad técnica

#### GO sin reservas (24/30 findings)
A.1, A.2, A.3, A.4, A.5, B.1, B.2, B.4, C.1, C.2, C.3, D.2, D.3, D.4, E.1, E.2, F.1, F.2, F.3, F.4, F.5, G.1, G.2, G.3.

#### RE-WORK plan original (4 findings)
- **D-E.3** LogError actionable: aceptar limitación Console + complement con toast (b)+(c).
- **D-E.4** ShowNotification: re-lock D4 a "ShowNotification text-only + Hub auto-foreground" (Pillar P5 mantenido a costa de cero buttons in-toast).
- **B.3** Build Everything orchestration: añadir try/catch + retry-from-step.
- **D.1** Theme live preview: SPLIT — Path B en v1.0.0-rc, Path A reservado v1.1.0.

#### DEFER post-RC (1 finding)
- **C.4** Custom popup auto-discovery API → v1.1.0 (D3 LOCKED).

#### Premises críticos non-Hub (cubiertos en disposition Bloque 2 R-U-C1/2/3/5/7)
- KitforgeRoot prefab + KitforgeThemeBinder
- DTO refactor BREAKING 17 archivos
- HUDSimple
- Theme presets move + multi-font
- SafeAreaFitter

---

### Plan aterrizaje M5/RC — sub-milestones M5.1 a M5.7

> v1.0.0-rc total scope: **~67h = ~10-11 sesiones**. Sub-divido para que cada M5.X cierre con audit verde + commit atómico + sin tag intermedio (single tag at M5.7).

| # | Sub-milestone | Scope | Sesiones | Premise / depend |
|---|---|---|---|---|
| **M5.1** | **Premises Runtime** — KitforgeRoot.prefab + KitforgeThemeBinder + DTO refactor BREAKING + multi-font Theme + Theme presets move a Runtime | ~16h | ~2-3 | None — first |
| **M5.2** | **Hub shell + Setup tab** — UIToolkit window + 5-tab sidebar + state model + auto-open + Setup wizard 3-step + idempotency + Build Everything | ~18h | ~3 | M5.1 (KitforgeRoot prefab needed for Setup Step 2) |
| **M5.3** | **Catalog Browser tab** — visual grid + thumbnails (PrefabSnapshotCapture) + drag-to-scene + side panel preview/snippet | ~9h | ~1.5 | M5.1 (DTO refactor for snippets) |
| **M5.4** | **Theme Studio tab** — live preview (Path B) + slot categorías + presets dropdown + Capture All button + ThemeConsistencyCheck audit | ~6h | ~1 | M5.1 (Theme presets in Runtime) |
| **M5.5** | **Test tab + Communication + audit checks** — Popup Test Launcher + DTO mock-fill + Force scenarios + LogError actionable + ShowNotification + health badge + UIServicesRefsCheck + ThemeReskinDeltaCheck audit checks | ~12h | ~2 | M5.1 (DTO refactor for mock-fill) |
| **M5.6** | **HUDSimple + SafeAreaFitter + Help tab + Cheat-sheet doc + Tools menu jerarquía + legacy soft-deprecate** | ~10h | ~1.5 | M5.2 (Hub shell ready for Help tab) |
| **M5.7** | **Final triple gate + RC tag** — verify-all-docs/samples/tests fresh compile + ritual `Documentation~/QAReports/_README.md` (Clear All → Regenerate + Audit → green → copy `_Summary.md`) + tag `v1.0.0-rc` | ~3h | ~0.5 | All M5.X done |

**Total**: 7 sub-milestones · ~74h · ~12 sesiones (incluye buffer 10% sobre 67h estimate).

**Original M5/RC roadmap estimate**: 1-2 sesiones. **Revised post-Bloque 4**: 12 sesiones. Diferencia +10 sesiones explicada por:
- Hub Editor window (no estaba en plan original).
- DTO refactor BREAKING (no estaba en plan original).
- HUDSimple + SafeAreaFitter Runtime work (no estaban en plan original).
- 4 premises Runtime ahora bloqueantes pre-tag.

### Decisión scope M5/RC — confirmar al user

**Opción A** — full scope (12 sesiones, todos los P0 lockados): v1.0.0-rc ships Hub + Runtime simples + safe-area + DTO refactor. Asset Store-quality identidad UX.

**Opción B** — split en v1.0.0-rc + v1.0.1-rc Hub-light: v1.0.0-rc ships solo M5.1 + M5.6 + M5.7 (premises Runtime + HUDSimple + safe-area + tag). Hub a v1.1.0. Reduce a ~4 sesiones pero v1.0.0-rc no tiene "single front door" — P1 violado a tag.

**Opción C** — Hub-only minimal MVP en v1.0.0-rc: solo M5.2 (Hub shell + Setup tab) + premises críticos. Catalog/Theme/Test tabs vacíos placeholder. ~5 sesiones. Identidad parcial.

**Recomendación**: **Opción A**. Razones:
- Bloque 3 estableció Hub como deliverable central. Sin ella, v1.0.0-rc se siente incompleto.
- Re-tag a v1.1.0 para feature obvia (Hub completa) confunde a buyers internos del studio.
- 12 sesiones es estimate generoso (incluye buffer); probablemente cierra en 9-10.
- Trade-off real = más tiempo pre-tag vs identidad completa al tag. Identidad completa gana.

**Bloque 4 cerrado**.
