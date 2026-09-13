---
name: kitforgelabs_ux_audit_2026-05_archive
description: ARCHIVE of superseded content from kitforgelabs_ux_audit_2026-05 — Bloque 1 (4-roles original plan, cleared lente correction) + Reframe Bloque 1 (workshop-drawer test) + SUPERSEDED Bloques 2-6 original plan marker. Kept as audit history; current audit content lives in kitforgelabs_ux_audit_2026-05.md.
type: project
status: superseded
superseded_by: kitforgelabs_ux_audit_2026-05
---

# KF MobileUIKit — M3c UX Audit (2026-05) — ARCHIVE

> Archive of superseded content from the original 4-roles × 6-preguntas plan. Replan happened mid-Bloque 1 on 2026-05-09; Bloque 1 preserved as audit history. Current audit (Bloque 2 REPLAN onwards) lives in `kitforgelabs_ux_audit_2026-05.md`.

## Bloque 1 — Pregunta 1: ¿Está todo centralizado para el usuario?

### Estado fáctico (research 2026-05-09)

| Aspecto | Estado | Notas |
|---|---|---|
| Tools menu root | ✅ Single namespace `Tools → Kitforge → UI Kit → ...` | 9 entries flat |
| Audit window | ✅ Unificado post M4.X (`Editor/Audit/`) | 7 `IUIKitCheck` pluggable |
| Theme asset | ✅ 1 `UIThemeConfig` SO | 16 ColorSlots + 8 SpriteSlots + Font + Audio |
| Theme assignment | ❌ 3 veces (UIManager + PopupManager + ToastManager) | Arch decision #11 |
| Master "Build All" | ❌ Inexistente | 5 builders separados (A/B/C/D/E) + M4.1 |
| Master demo scene | ❌ `Catalog_All_Demo/` no shipped | Roadmap línea 67 Diferidos pre-RC |
| Setup wizard | ❌ Bootstrap Defaults solo crea SOs (10 presets + 1 theme) | NO crea managers en escena |
| Scene root prefab | ❌ No "KitforgeRoot.prefab" preconfigurado | Buyer monta 4 MonoBehaviours a mano |
| QUICKSTART.md | ❌ Inexistente | Quickstart vive en README, solo cubre Phase 1 |
| MIGRATION.md | ❌ Inexistente | Migrations dispersas en CHANGELOG entries |
| Sample dependencies | ❌ Implícitas | E→C, M4.1→B+C, solo en `package.json` descriptions |
| Documentation~ structure | ❌ Mix buyer/dev | 24 specs sin "buyer index" |
| Sample count | 8 entries en `package.json` | Quickstart + 5 catalog + M4.1 + GameWiring |
| MonoBehaviours scene | 4 obligatorios | UIManager + PopupManager + ToastManager + UIServices |

---

### `as user` — 10 findings (Q1, centralización)

**[U1.1] No existe "Build Everything" — buyer click 5 menu items para ver el catálogo completo**
- **Síntoma**: Tools menu expone `Build Group A/B/C/D/E Sample` + `Build M4.1 — Theme Presets` por separado. Para ver los 17 prefabs hay que ejecutar 6 menu items en orden correcto (A→B→C→D→M4.1→E por GUID lifecycle de M4.1).
- **Repro**: import 6 samples → recordar el orden → 6 clicks → revisar 6 carpetas Assets/Catalog_Group*_Demo/.
- **Impacto buyer**: pitch "Wire it in five minutes" se rompe en el primer minuto. Buyer evaluador hace 1 click, ve 4 prefabs, asume que eso es todo el kit.
- **Fix sugerido**: añadir `Tools → Kitforge → UI Kit → Build Everything` que orqueste A→B→C→D→M4.1→E (mismo flujo que `Regenerate + Audit` interno) + final dialog "17 prefabs + 6 demo scenes generadas".
- **Priority**: **P0**.

**[U1.2] No existe master demo scene — buyer no ve "todo el kit en una pantalla"**
- **Síntoma**: 6 demo scenes separadas (una por grupo + theme presets). No hay `Samples~/Catalog_All_Demo/` con un menú visual que dispare cualquier popup/HUD/screen.
- **Repro**: import N samples → abrir N escenas → Play cada una → 6 contextos mentales distintos.
- **Impacto buyer**: Asset Store hero screenshot imposible (no hay scene que muestre "el kit"); evaluación previa-compra fragmentada.
- **Fix sugerido**: crear `Samples~/Catalog_All_Demo/` con `MasterDemoHost.cs` (UI Toolkit window or UGUI grid de buttons) que dispare cada elemento del catalog. Listado en roadmap línea 67 como Diferidos M4 — promover a P0 pre-RC.
- **Priority**: **P0**.

**[U1.3] Quickstart README cubre solo Phase 1 (UIManager + PopupManager) — el catálogo queda fuera**
- **Síntoma**: README § "Quickstart (5 steps)" termina con "Push Quickstart Screen / Show Quickstart Popup". Buyer no aprende cómo lanzar `RewardPopup.Show(data)` desde el Quickstart.
- **Repro**: buyer abre Package Manager → Documentation tab (sale README) → sigue 5 steps → no ve nada del catalog.
- **Impacto buyer**: 5-min onboarding promete "ship it", entrega "wire framework you don't need". Catalog (la propuesta de valor diferencial) queda escondido en sample READMEs.
- **Fix sugerido**: Quickstart README expone 7 steps: 1-5 actuales + 6) "Build Everything" + 7) "Show RewardPopup desde tu host" con snippet `popupManager.Show<RewardPopup>(new RewardPopupData(...))`.
- **Priority**: **P0**.

**[U1.4] Tools menu sin separadores — Audit, Bootstrap, Builders y M4.1 conviven en mismo nivel**
- **Síntoma**: 9 entries flat sin agrupación (`Audit`, `Audit (Run Headless)`, `Bootstrap Defaults`, `Build Group A Sample`, ..., `Build M4.1 — Theme Presets`).
- **Repro**: abrir Tools menu, scan visual.
- **Impacto buyer**: ruido visual; el buyer no sabe qué entry pulsar primero. "Audit (Run Headless)" expuesto a buyer = entry interno developer.
- **Fix sugerido**: jerarquizar `Tools → Kitforge → UI Kit → Setup` (Bootstrap + Build Everything + Setup Scene Wizard) · `Build Individual Group` (5 entries) · `Theme Presets` (M4.1) · `Audit` (window) · `Developer` (Audit Headless, oculto behind `[MenuItem(... priority = 1000)]` o `validate` que requiera scripting define).
- **Priority**: **P1**.

**[U1.5] 4 MonoBehaviours obligatorios en escena — buyer monta a mano sin prefab "KitforgeRoot"**
- **Síntoma**: cada escena viva del buyer requiere `UIManager` + `PopupManager` + `ToastManager` + `UIServices`. Bootstrap Defaults no toca escena.
- **Repro**: buyer crea escena nueva → debe leer Quickstart README → crear 4 GameObjects → asignar 4 components → asignar Theme 3 veces.
- **Impacto buyer**: friction de setup escena ≈ 5-10 min por escena, vs "drop a prefab and play".
- **Fix sugerido**: shippear `Runtime/Bootstrap/KitforgeRoot.prefab` con los 4 MonoBehaviours + Theme slot vacío. Buyer arrastra el prefab + asigna Theme una vez (variant prefab override) → escena lista.
- **Priority**: **P0**.

**[U1.6] Bootstrap Defaults dialog termina con instrucción técnica que el buyer no entiende**
- **Síntoma**: tras "Bootstrap Defaults" sale dialog `"...Assign this Theme to your UIManager / PopupManager / ToastManager."`. Buyer recién importado no sabe qué son esos managers ni dónde están.
- **Repro**: Tools → Bootstrap Defaults → leer dialog → confusión.
- **Impacto buyer**: setup-wizard se siente incompleto. Buyer cierra dialog y queda perdido.
- **Fix sugerido**: dialog redirige a "Build Everything" o a "Setup Scene Wizard" como next-step explícito. Si KitforgeRoot.prefab existe (U1.5), dialog puede decir "Drop `Runtime/Bootstrap/KitforgeRoot.prefab` en tu escena — el Theme se autodetecta".
- **Priority**: **P1**.

**[U1.7] 8 samples en Package Manager sin guidance "qué importar primero"**
- **Síntoma**: lista samples = Quickstart + Group A + B + C + D + E + M4.1 + GameWiring. Sin orden visible. Buyer hace click en cualquiera.
- **Repro**: Package Manager → Samples → 8 entries → orden alfabético/registro, no didáctico.
- **Impacto buyer**: M4.1 importado primero → "builder aborta porque Group B+C no presentes" → primera impresión: "kit roto".
- **Fix sugerido**: prefijar displayName con orden (`1. Quickstart`, `2. Catalog — Group A`, ..., `7. Theme Presets`, `8. Game Wiring (advanced)`). O reducir samples = `Quickstart` + `Catalog (all groups + theme presets)` único + `Game Wiring (VContainer)`.
- **Priority**: **P0**.

**[U1.8] Sample dependency graph (E→C, M4.1→B+C) implícito, solo en descriptions**
- **Síntoma**: `package.json` description menciona "Requires Group B + Group C samples". Buyer scanning Package Manager UI lee primero displayName, descripción truncada. Builder aborta con LogError post-import.
- **Repro**: import M4.1 sin Group B/C → click "Build M4.1" → LogError "Group B not present".
- **Impacto buyer**: "kit cripto", primera-impresión negativa.
- **Fix sugerido**: declarar dependencias en displayName: `"Catalog — M4.1 — Theme Presets (requires Groups B+C)"`. O builder pre-flight: si dependencia falta → DisplayDialog "Importa Group B+C primero — ¿abrir Package Manager?".
- **Priority**: **P0**.

**[U1.9] No existe buyer-facing index en Documentation~/ — 24 spec files mezclan buyer + dev**
- **Síntoma**: `Documentation~/Specs/Catalog/` contiene 13 popup specs + 4 HUD/screen specs + 2 service specs + 2 DELTA files (Group C/E pre-flight, dev-internal) + CATALOG.md master.
- **Repro**: buyer abre `Documentation~/` (vía git clone o explorador) → no ve QUICKSTART, no ve MIGRATION, no ve INDEX. Ve 24 .md files con nombres-de-componente.
- **Impacto buyer**: docs orientados-dev, no orientados-buyer. Buyer no encuentra "how do I X" guides.
- **Fix sugerido**: añadir `Documentation~/QUICKSTART.md` (buyer-facing onboarding 5-min) + `Documentation~/MIGRATION.md` (cumulative version-to-version) + `Documentation~/README.md` (índice: "Buyer? read QUICKSTART. Migrating? read MIGRATION. Spec details? read Specs/CATALOG.md."). Mover DELTA files a `Documentation~/Specs/_dev/`.
- **Priority**: **P0**.

**[U1.10] CHANGELOG `[Unreleased]` block expone work-in-progress a buyer en Package Manager UI**
- **Síntoma**: Package Manager renderiza CHANGELOG. `[Unreleased]` block contiene M4.X cluster intermedio (audit unification, text-theming sweep, ritual evidence) pre-tag.
- **Repro**: Package Manager → Kitforge UI Kit → Changelog tab → primer bloque `[Unreleased]` con detalles internos.
- **Impacto buyer**: ruido + percepción "kit en construcción", contradice pitch "ship it" del Asset Store.
- **Fix sugerido**: pre-tag ritual extends a "purgar `[Unreleased]` content → consolidar al tag block". Mantener `[Unreleased]` solo cuando hay cambios POST-tag visibles a buyer.
- **Priority**: **P1**.

---

### `as ta` — 10 findings (Q1, centralización TA-pipeline)

**[T1.1] Theme se asigna 3 veces (UIManager + PopupManager + ToastManager) — arch decision #11 lo acepta como friction conocida**
- **Síntoma**: cada manager tiene su propio `_themeConfig` field. Buyer arrastra el mismo asset 3 veces. Si TA cambia el Theme asignado a UIManager pero olvida PopupManager → escena con 2 themes mixed.
- **Repro**: scene con KitforgeRoot wired manual → drag Theme_Casual a UIManager → Play → screens reskinned, popups con Theme_Default.
- **Impacto buyer**: arch decision #11 reconoce el problema ("skin it once" se rompe en setup). Validador no flaggea theme drift.
- **Fix sugerido**: KitforgeRoot.prefab (U1.5) debería tener un `KitforgeThemeBinder` MonoBehaviour que distribuya el Theme a los 3 managers en Awake (single Inspector field). Audit Check `ThemeConsistencyCheck` que compare `UIManager._themeConfig == PopupManager._themeConfig == ToastManager._themeConfig`.
- **Priority**: **P0**.

**[T1.2] UIThemeConfig 16 ColorSlots + 8 SpriteSlots + Font + Audio — sin "preview all skins at once"**
- **Síntoma**: `UIThemeConfigEditor` muestra preview color por slot (single value). No hay vista de "todo el theme rendered en un mock prefab".
- **Repro**: TA modifica `_warningColor` en Inspector → no ve impacto hasta abrir escena Group C demo + Play + disparar HUDEnergy regen countdown.
- **Impacto buyer**: TA itera "edit → Play → check → edit" en bucle, pierde 30s por iteración × 16 slots = 8 min por sesión de coloring.
- **Fix sugerido**: `UIThemeConfigEditor` añade preview thumbnail de un mock popup (card + buttons + texts) renderizado con los slots actuales. Misma idea que Bootstrap Defaults dialog, pero como CustomEditor preview.
- **Priority**: **P1**.

**[T1.3] TA cambia un slot del Theme → debe abrir 6 demo scenes + Play cada una para validar reskin**
- **Síntoma**: no existe "Reskin all & screenshot" tool. TA cambia `Theme.PrimaryColor` → debe abrir Group A demo + Play, Group B demo + Play, ..., 6 escenas, validar visualmente.
- **Repro**: edit theme → 6 abre/Play/cierra ciclos.
- **Impacto buyer**: TA workflow lentísimo. La promesa "skin it once" cierra el lazo si validar también es one-click.
- **Fix sugerido**: `Tools → Kitforge → UI Kit → Theme → Capture All Snapshots` ejecuta `PrefabSnapshotCapture` (ya existe en `Editor/Audit/Snapshots/`) sobre los 17 prefabs con el theme activo + abre folder con thumbnails.
- **Priority**: **P1**.

**[T1.4] No Theme palette generator — TA importa colores de Figma → asigna 16 colores manualmente**
- **Síntoma**: TA recibe palette de UI/UX (16 colores) → abre UIThemeConfig → asigna slot por slot click-by-click.
- **Repro**: 16 click-asignaciones en Inspector por theme.
- **Impacto buyer**: TA workflow tedioso. Si studio tiene 3 themes (Default/Casual/Premium) = 48 click-asignaciones.
- **Fix sugerido**: Theme generator: pega JSON/CSV con 16 colores → genera UIThemeConfig.asset. O integration con design tokens (Figma Tokens / Style Dictionary). P1 mínimo, P2 si scope es Asset Store.
- **Priority**: **P1**.

**[T1.5] Theme presets (Default/Casual/Premium) escondidos en sample M4.1 — no en menu raíz "Theme → Switch"**
- **Síntoma**: para usar Theme_Casual hay que importar M4.1 sample → Build M4.1 → asignar manualmente al UIManager. No hay `Tools → Kitforge → UI Kit → Theme → Switch to Casual` que reasigne en runtime.
- **Repro**: TA quiere ver Casual → 4 pasos.
- **Impacto buyer**: feature "skin it once" subutilizado por friction de switch.
- **Fix sugerido**: si Catalog_M4_ThemePresets generó los assets → menu entries `Theme → Apply Default/Casual/Premium` que setean el Theme en el KitforgeRoot.prefab del scene activo (asumiendo U1.5 fix). M4.X ya añadió SetTheme runtime API + dropdown E2E — promover a menu entry editor.
- **Priority**: **P1**.

**[T1.6] ThemedImage/ThemedText backing-field rule documentada solo en audit + memoria interna**
- **Síntoma**: `ThemedFieldsWiredCheck` lockea la regla. Pero docs buyer-facing (README, CATALOG.md) no explican "AddThemedImage debe wirear `_image` SerializedProperty".
- **Repro**: TA añade ThemedImage manual → audit falla → busca en docs cómo arreglar → encuentra spec interno.
- **Impacto buyer**: regla queda como gotcha. Si buyer extiende kit con custom popup → tropieza.
- **Fix sugerido**: docs `Documentation~/CONTRIBUTING.md` o sección en CATALOG.md "Adding ThemedImage/ThemedText to your popup" con snippet patrón canonical.
- **Priority**: **P1**.

**[T1.7] 10 UIAnimPresets en `Assets/Settings/UIAnimPresets/` sin UI para swap global**
- **Síntoma**: Bootstrap Defaults crea 10 presets. Theme tiene `_defaultAnimPreset` slot. Si TA quiere cambiar de Playful a Snappy globally → edita Theme manualmente, no hay `Theme → Animation → Switch Preset` menu.
- **Repro**: TA toca `Theme._defaultAnimPreset` → Play.
- **Impacto buyer**: feature animation styles (10 presets) está infrautilizada por descubribilidad cero.
- **Fix sugerido**: `Tools → Kitforge → UI Kit → Animation → Switch Preset → [Snappy / Bouncy / Playful / ...]` cambia el `_defaultAnimPreset` del theme activo. Inspector preview tween en `UIAnimPreset` SO.
- **Priority**: **P2**.

**[T1.8] No Inspector tool "qué prefabs usan esta sprite slot"**
- **Síntoma**: TA cambia `Theme.IconEnergy` sprite → quiere ver los prefabs que la consumen → no hay buscador. Audit `_Summary.md` lista prefabs pero por status (pass/fail), no por slot consumption.
- **Repro**: TA inspecciona Theme → ningún botón "Find usages".
- **Impacto buyer**: TA workflow = grep manual en Editor o leer 24 specs.
- **Fix sugerido**: `UIThemeConfigEditor` añade botón "Find Usages" por slot que busca prefabs con `ThemedImage` consuming ese slot (SerializedObject scan, similar al audit). P2 mínimo.
- **Priority**: **P2**.

**[T1.9] No Theme A/B comparator inline — preview dual Default vs Casual side-by-side**
- **Síntoma**: TA quiere comparar dos themes lado a lado → debe abrir 2 instancias Editor o Play 2 veces.
- **Repro**: edit theme A, Play, screenshot, edit theme B, Play, screenshot, comparar mentalmente.
- **Impacto buyer**: validación de reskin = manual + propenso a errores.
- **Fix sugerido**: `Tools → Theme → Compare A vs B → [select 2 themes] → window con 2 columns rendered prefabs`. Reusa `PrefabSnapshotCapture`.
- **Priority**: **P2**.

**[T1.10] 8 sprite slots declarados en Theme + ningún doc "qué prefab usa qué sprite"**
- **Síntoma**: SpriteSlots = `IconEnergy`, `IconClock`, `StarFilledSprite`, `StarEmptySprite`, etc. Sin tabla "IconEnergy → consumed by HUDEnergy.IconImage + HUDEnergyDemo header".
- **Repro**: TA mira Theme, ve 8 slots, no sabe qué impacto tiene tocar cada uno.
- **Impacto buyer**: TA toca un slot defensivamente → bug visual no detectado hasta QA.
- **Fix sugerido**: tabla "Theme Slot → Consumers" generada automáticamente por audit + dumped en `Documentation~/QAReports/_ThemeSlotsUsage.md`. P2.
- **Priority**: **P2**.

---

### `as pm` — 10 findings (Q1, centralización buyer journey & Asset Store)

**[P1.1] Pitch "Wire it in five minutes" vs realidad ≈ 30 min onboarding**
- **Síntoma**: buyer real journey = import package + Bootstrap Defaults (1 min) + import 6 samples (3 min) + run 6 builders (3 min) + montar 4 MonoBehaviours en escena (10 min) + asignar Theme 3 veces (2 min) + leer 8 sample READMEs (10 min) = 30 min mínimo.
- **Repro**: stopwatch + buyer fresh-import.
- **Impacto buyer**: pitch creates expectations → 30 min reality → first review "claim is misleading".
- **Fix sugerido**: o cumplir el pitch (Setup wizard + Build Everything + KitforgeRoot prefab → 5 min real) o ajustar pitch a "Wire your first popup in 5 minutes; configure the catalog in 30".
- **Priority**: **P0**.

**[P1.2] Sin master demo scene, no hay hero screenshot — Asset Store store page castrada**
- **Síntoma**: store page necesita 1-2 screenshots que muestren "todo el catálogo" para conversion. Sin `Catalog_All_Demo` no hay scene posible.
- **Repro**: PM intenta producir hero shot → no hay scene → toca componer composite manual de 6 demo scenes.
- **Impacto buyer**: conversion ratio store-page baja (industry data: hero shot es el #1 drive de click-through).
- **Fix sugerido**: master demo scene (U1.2) es prerequisito de store page. P0 pre-RC.
- **Priority**: **P0**.

**[P1.3] package.json description mezcla "router + theme" + "catalog" — confusión sobre proposición de valor**
- **Síntoma**: description = "Opinionated UGUI router, popup queue, theme contract AND a catalog of mid-core mobile UI elements (Confirm/Pause/Tutorial/Toast in v0.4; Shop/Reward/Settings/DailyLogin/GameOver/LevelComplete/HUD/Screens shipping group-by-group)".
- **Repro**: buyer scan en Asset Store → "es framework? es kit prefabs? las dos?".
- **Impacto buyer**: confusión = baja conversion. Doozy resuelve con pitch unificado "the UI manager Unity is missing".
- **Fix sugerido**: pre-RC, lock description final centrada en propuesta diferencial: "15 mid-core mobile UI prefabs, plug-and-play. Router + theme + popup queue included." Reordenar para que el buyer entienda primero el valor (catálogo) y después la infra (framework).
- **Priority**: **P0**.

**[P1.4] 8 samples vs Doozy 1-sample — sobrecarga cognitiva en Package Manager UI**
- **Síntoma**: Doozy ships 1 master sample. Kitforge ships 8 (Quickstart + 5 group + M4.1 + GameWiring). Buyer evaluador no sabe cuál importar.
- **Repro**: Package Manager → Samples list → scroll de 8 entries.
- **Impacto buyer**: choice paralysis. Buyer importa Quickstart, no ve catálogo, asume eso es todo.
- **Fix sugerido**: consolidar a 3 samples: `Quickstart` (Phase 1 framework) + `Catalog (full)` (todos los groups + theme presets en una unidad) + `Game Wiring (VContainer)` (advanced). Builders pueden seguir siendo per-group internamente, samples-as-package se reducen.
- **Priority**: **P0**.

**[P1.5] README "Status" tabla expone process interno (alpha-by-alpha tags + BREAKING entries)**
- **Síntoma**: README § Status muestra tabla de Group 0/A/B/C/D/E con tags `v0.3.0-alpha`, `v0.4.0-alpha BREAKING`, etc. Expone delivery interno.
- **Repro**: buyer abre README → tabla 7 rows con alpha tags.
- **Impacto buyer**: percepción "kit experimental, no production-ready". Pitch de Asset Store apunta a producción; tabla la contradice.
- **Fix sugerido**: pre-RC, simplify Status section a 3 rows: "Framework ✅ stable" + "Catalog ✅ 15 elements shipped" + "Themes ✅ 3 presets included". Mover delivery internals a CHANGELOG.
- **Priority**: **P0**.

**[P1.6] No "What's in the box" 1-pager — README mixea Phase 1 done criteria + Architecture decisions + Quickstart**
- **Síntoma**: README estructura = Status + Non-goals + Install + Quickstart + Phase 1 done + Architecture decisions + License + Changelog. Buyer scrollea esperando "qué incluye el kit" y se encuentra "Phase 1 done criteria" tipo dev-changelog.
- **Repro**: lectura README de arriba abajo.
- **Impacto buyer**: orientación buyer-flow rota. Phase 1 done criteria pertenece a internal docs.
- **Fix sugerido**: README pre-RC = Pitch (1 line) + What's in the box (15 prefabs + 3 themes + framework) + Quickstart (5 steps real) + Install + Non-goals + Architecture decisions (collapsible) + License + Changelog. Mover Phase 1 done criteria a Documentation~/INTERNAL.md.
- **Priority**: **P0**.

**[P1.7] Sample dependency graph implícito — buyer importa M4.1 sin B+C → builder LogError**
- **Síntoma**: M4.1 sample requiere Group B + Group C builders previos. Si buyer no los tiene → `Build M4.1` aborta con LogError.
- **Repro**: import M4.1 only → click Build → LogError → confusión.
- **Impacto buyer**: primera-impresión "kit roto", aunque mensaje sea claro.
- **Fix sugerido**: ver U1.7 + U1.8 (numerado prefijo + builder pre-flight DisplayDialog).
- **Priority**: **P0** (mismo fix que U1.7+U1.8).

**[P1.8] "Bootstrap Defaults" = nombre técnico — buyer espera "First-time setup" o "Setup wizard"**
- **Síntoma**: entry name `Bootstrap Defaults` resuena para devs (bootstrap = arranque); buyer hybrid-casual studio entiende "setup wizard" o "initialize project".
- **Repro**: Tools menu → "Bootstrap Defaults" → "¿qué hace?".
- **Impacto buyer**: descubribilidad baja. Buyer prueba `Build Group A Sample` antes (parece más accionable).
- **Fix sugerido**: rename a `First-time Setup` o `Setup Wizard`. O combinar con U1.1: `Setup → Initialize Project` (Bootstrap Defaults) + `Setup → Build Everything` + `Setup → Add Scene Root` (KitforgeRoot.prefab drop).
- **Priority**: **P1**.

**[P1.9] `Documentation~/QAReports/` package-internal evidence visible si buyer hace git clone**
- **Síntoma**: `Documentation~/` no se procesa por Unity (tilde suffix), pero está commiteado. Buyer que clona repo desde GitHub ve `QAReports/_Summary.md` + `_README.md` (developer-facing).
- **Repro**: clone repo → ls Documentation~ → ver QAReports.
- **Impacto buyer**: ruido para buyer evaluator. Si llega vía git URL `#v0.9.0-alpha` (UPM), Documentation~ NO se importa al consumer project (solo accesible en repo browser); no es visible en Editor.
- **Fix sugerido**: en realidad scope-cap — no es problema crítico: Package Manager UI no expone `Documentation~/QAReports/`. PM dejar como está. Marcar **drop P2**.
- **Priority**: **P2**.

**[P1.10] Asset Store keywords (`ui`, `ugui`, `mobile`, `router`, `theme`, `popup`, `menu`, `kitforge`) sin keywords de catalog**
- **Síntoma**: package.json keywords excluye `prefabs`, `kit`, `template`, `ready-to-use`, `hybrid-casual`. Buyer searches "popup prefab unity" → no hit.
- **Repro**: search Asset Store con queries reales de buyer hybrid-casual.
- **Impacto buyer**: discoverability cero en search engine de Asset Store.
- **Fix sugerido**: añadir keywords pre-RC: `prefab`, `template`, `kit`, `hybrid-casual`, `ready-to-use`, `mid-core`. Out of scope técnico audit, pero centralización-de-marketing aplica.
- **Priority**: **P1**.

---

### `as qa` — 10 findings (Q1, centralización testing & verification)

**[Q1.1] 5 builders separados = 5 entry points para verificación regression manual**
- **Síntoma**: cada builder genera prefabs + scene independientemente. QA debe ejecutar 5 menu items + abrir 5 scenes + Play 5 veces para validar regression post-cambio.
- **Repro**: PR cambia `CatalogGroupBuilderShared.CreateThemedText` → QA debe re-Build A+B+C+D+E + abrir 5 demos.
- **Impacto buyer**: regression catch lento. Pre-tag ritual `Documentation~/QAReports/_README.md` ya tiene "Regenerate + Audit" one-click ✅, pero QA manual seguía 5 entries hasta M4.X. Confirmar que el audit window cubre eso ahora.
- **Fix sugerido**: ✅ ya cubierto post-M4.X (audit window `Regenerate + Audit` toolbar). Confirmar que documentation refleja "Regenerate + Audit substitutes manual 5-build ritual".
- **Priority**: **P2** (cubierto, solo doc).

**[Q1.2] Audit cubre 23/23 catalog + scenes pero NO "buyer journey" (import sample + Play + dismiss popup)**
- **Síntoma**: audit verifica integridad estructural (refs wired, themed counts, missing scripts). NO simula buyer fresh-import → Build → Play → click button → popup shown → dismissed.
- **Repro**: PR rompe `PopupManager.Show` runtime → audit pasa ✅, pero Play falla → QA solo lo detecta manual.
- **Impacto buyer**: regression runtime no covered. M4 done criteria menciona "buyer fresh-import smoke test" como manual checklist.
- **Fix sugerido**: PlayMode test "BuyerJourneySmokeTest": load Group A demo scene + EnterPlayMode + PopupManager.Show<ConfirmPopup> + assert `popup.gameObject.activeInHierarchy` + dismiss + assert dismissed. 1 test por grupo = 5 PlayMode tests cubre regression runtime.
- **Priority**: **P0**.

**[Q1.3] UIServices.Validate ContextMenu = manual-only QA gate, no automatizado**
- **Síntoma**: `UIServices.Validate` es ContextMenu method que loggea "Missing: N/8". QA debe right-click cada UIServices instance manualmente.
- **Repro**: 6 demo scenes × right-click UIServices × Validate × leer Console.
- **Impacto buyer**: regression "service ref nulled" no detectada hasta runtime.
- **Fix sugerido**: añadir audit check `UIServicesRefsCheck` (igual que `UIKitManagerCheck` existente) que escanee scenes y flag UIServices con refs null. Promote ContextMenu a audit-coverage.
- **Priority**: **P1**.

**[Q1.4] Buyer fresh-import smoke test = manual checklist en M4 done criteria, sin mecanización**
- **Síntoma**: M4 done criteria línea 638 lista "buyer fresh-import smoke test passes" como entry. Sin script + sin checklist file + sin CI hook.
- **Repro**: ¿quién corre el smoke test antes de tag? ¿con qué pasos?
- **Impacto buyer**: regression "fresh-import flow rota" no detectada hasta buyer real reporta.
- **Fix sugerido**: `Documentation~/QAReports/SMOKE_TEST.md` con checklist (1. install via UPM URL fresh project / 2. Bootstrap Defaults / 3. Import 8 samples / 4. Build Everything / 5. Play each demo / 6. zero LogError). Posibilidad de promote a CI script (`Tools → Audit Headless` ya existe, smoke test es complementario).
- **Priority**: **P0**.

**[Q1.5] No CI hook ejecuta `Audit (Run Headless)` automáticamente en PR**
- **Síntoma**: entry `MenuItem("Tools/Kitforge/UI Kit/Audit (Run Headless)")` existe pero `Documentation~/QAReports/_README.md` no documenta cómo invocarlo desde GitHub Actions / Unity CLI batch mode.
- **Repro**: PR → CI → no audit ejecutado → reviewer manual.
- **Impacto buyer**: regression escapa hasta merge.
- **Fix sugerido**: doc en `_README.md` o `Documentation~/CI.md`: snippet GitHub Actions `Unity -batchmode -executeMethod KitforgeLabs.MobileUIKit.Editor.Audit.Headless.UIKitAuditHeadless.RunHeadless -nographics -quit`. Exit 0/1/2 ya implementado per CHANGELOG.
- **Priority**: **P1**.

**[Q1.6] Visual snapshot diff Casual↔Premium hash≠ check pendiente — Diferidos línea 53**
- **Síntoma**: snapshots SHA256-hashed se generan, pero sin check "comparar hashes Default vs Casual debe ser ≠ (porque colors difieren)". Si reskin se rompe (theme no aplica) → hashes iguales → audit pasa pero theme contract roto.
- **Repro**: reskin code regression → snapshots iguales → audit verde mentirosa.
- **Impacto buyer**: contract "skin it once" silently roto.
- **Fix sugerido**: añadir audit check `ThemeReskinDeltaCheck` que valide hash(Theme_Default snapshot) ≠ hash(Theme_Casual snapshot) por prefab.
- **Priority**: **P1**.

**[Q1.7] `OnUpdate-workaround-M3-sweep` comment anchors en HUDEnergy/HUDTimer — verificar limpieza post-M3a**
- **Síntoma**: M3a en v0.8.0-alpha cerró el OnUpdate infra dispatch fix + sweep workaround. Comment anchors `// OnUpdate-workaround-M3-sweep` pueden persistir si sweep no fue exhaustivo.
- **Repro**: grep `OnUpdate-workaround-M3-sweep` → si hay hits, el workaround sigue presente.
- **Impacto buyer**: dead code shipping al buyer.
- **Fix sugerido**: grep + verificar limpieza pre-RC. Si quedan anchors, eliminar.
- **Priority**: **P1** (verificación rápida).

**[Q1.8] Tools menu expone `Audit` y `Audit (Run Headless)` — buyer no entiende diferencia**
- **Síntoma**: 2 entries lado a lado. Headless es para CI batch mode, no para buyer manual. Buyer puede ejecutarlo accidentally → output va a stdout, no a Console.
- **Repro**: Tools menu → click "Audit (Run Headless)" en Editor manual.
- **Impacto buyer**: confusión + UX rota.
- **Fix sugerido**: ocultar `Audit (Run Headless)` con `[MenuItem(... priority = 1000)]` o moverlo a `Tools → Kitforge → UI Kit → Developer → Audit (CI Headless)` separado.
- **Priority**: **P1**.

**[Q1.9] 292 EditMode tests cover catalog + framework, NO Editor tools (builders, audit checks)**
- **Síntoma**: cobertura de tests = runtime contracts. Builders y audit checks (Editor namespace) no tienen tests propios.
- **Repro**: PR rompe `CatalogGroupBuilderShared.CreateThemedText` lógica → audit corre ok porque builder no se ejecuta en audit; manual run requerido.
- **Impacto buyer**: regression Editor-only escapa hasta buyer real importa.
- **Fix sugerido**: añadir EditMode tests para builders (smoke: `Build Group A` no throws, output prefabs existen) + audit checks (unit per check con prefab fixture). Scope ≈ 10-15 tests adicionales.
- **Priority**: **P1**.

**[Q1.10] No PlayMode integration test "press button → popup shows → dismiss → next popup"**
- **Síntoma**: 292 tests son EditMode (binding contracts, queue logic, type resolution). PlayMode = 0.
- **Repro**: regression "PopupManager.Show no llama OnShow" → EditMode pasa, runtime falla.
- **Impacto buyer**: dynamic UI flow no covered.
- **Fix sugerido**: PlayMode test suite minimal: 1 test por grupo cargando demo scene + EnterPlayMode + ContextMenu trigger via reflection + assert popup state. Solapa con Q1.2 (consolidar como un "BuyerJourneyPlayMode" suite).
- **Priority**: **P0** (junto con Q1.2).

---

### Convergencias cross-rol (Bloque 1)

Findings flagged por ≥2 roles, ranked por convergencia:

| ID convergence | Tema | Roles convergentes | P0/P1/P2 |
|---|---|---|---|
| **C1** | **Setup wizard / Build Everything / KitforgeRoot prefab — falta entry centralizada** | U1.1 + U1.5 + U1.6 + P1.1 + P1.8 (5 findings, 2 roles) | **P0** |
| **C2** | **Master demo scene `Catalog_All_Demo` missing — hero shot imposible, evaluación fragmentada** | U1.2 + P1.2 (2 findings, 2 roles) | **P0** |
| **C3** | **Buyer-facing docs scattered — sin QUICKSTART unified, sin Documentation~ index** | U1.3 + U1.9 + P1.5 + P1.6 (4 findings, 2 roles) | **P0** |
| **C4** | **Sample dependency graph implícito (E→C, M4.1→B+C) — friction en first-import** | U1.7 + U1.8 + P1.7 (3 findings, 2 roles) | **P0** |
| **C5** | **Theme assigned 3 veces — arch decision #11 known, no resolved** | T1.1 (1 role mainly, pero arch-acknowledged) | **P0** |
| **C6** | **Buyer journey not covered by tests — fresh-import + PlayMode integration missing** | Q1.2 + Q1.4 + Q1.10 (3 findings, 1 role internamente) | **P0** |
| **C7** | **Theme reskin tools insuficientes (snapshot all + palette gen + A/B compare + theme switch menu)** | T1.3 + T1.4 + T1.5 + T1.9 + Q1.6 (5 findings, 2 roles) | **P1** |
| **C8** | **Tools menu sin agrupación / jerarquía — Audit Headless visible a buyer** | U1.4 + Q1.8 (2 findings, 2 roles) | **P1** |
| **C9** | **Pitch / package.json description / keywords / sample naming — Asset Store store-page polish** | P1.3 + P1.4 + P1.10 (3 findings, 1 role) | **P0** |
| **C10** | **CHANGELOG `[Unreleased]` exposed a buyer + arch decisions / Phase 1 done criteria mixed** | U1.10 + P1.5 + P1.6 (3 findings, 2 roles) | **P0** |

---

### Disposition Bloque 1 (P0/P1/P2) — SUPERSEDED 2026-05-09

> **SUPERSEDED**. Original disposition (below) leaned ~50% Asset Store / demos / buyer-docs framing. User correction 2026-05-09 reframed audit per workshop-drawer test (`feedback_audit_priority_tool_first.md`): tool reliability + prefab practicality + reorganization come FIRST. Store/demos/docs LAST until user explicitly opens that scope. **Use the "REFRAME 2026-05-09" section below this for actionable disposition.** Original kept as audit history.

#### P0 — apply pre-RC (M5/RC-tag work) [SUPERSEDED]

1. **C1 — Setup wizard + Build Everything + KitforgeRoot prefab** (U1.1 + U1.5 + U1.6 + P1.1 + P1.8)
   - Tools menu añade `Setup → Initialize Project` (rename de Bootstrap Defaults) + `Setup → Build Everything` (orquesta A→B→C→D→M4.1→E) + `Setup → Add Scene Root` (drop `KitforgeRoot.prefab`).
   - `Runtime/Bootstrap/KitforgeRoot.prefab`: GameObject con UIManager + PopupManager + ToastManager + UIServices + `KitforgeThemeBinder` MonoBehaviour que distribuye Theme single-field a los 3 managers.
   - Bootstrap Defaults dialog termina pingueando KitforgeRoot prefab + dialog "Drop this prefab in your scene; Theme is auto-detected".

2. **C2 — Master demo scene `Catalog_All_Demo`** (U1.2 + P1.2)
   - `Samples~/Catalog_All_Demo/` con `MasterDemoHost.cs` (UGUI grid de buttons + ContextMenu triggers per element).
   - `package.json` samples[] añade entry `Catalog — Master Demo (all elements)`.
   - Hero screenshot derivable de esta scene.

3. **C3 — Buyer-facing docs unified** (U1.3 + U1.9 + P1.5 + P1.6)
   - `Documentation~/QUICKSTART.md` (5-step buyer onboarding real).
   - `Documentation~/MIGRATION.md` (cumulative version-to-version).
   - `Documentation~/README.md` (índice buyer).
   - README pre-RC simplifica: Pitch + What's in the box + Quickstart + Install + Non-goals + collapsible Architecture decisions + License + Changelog.
   - DELTA files (`CATALOG_GroupC_DELTA.md`, `CATALOG_GroupE_DELTA.md`) movidos a `Documentation~/Specs/_dev/`.

4. **C4 — Sample dependency graph explicit** (U1.7 + U1.8 + P1.7)
   - `package.json` samples[] displayName prefijado con orden numérico.
   - Builders pre-flight DisplayDialog si dependencia falta.
   - O consolidar a 3 samples: Quickstart + Catalog (all) + Game Wiring (advanced). Decisión = trade-off; ver discussion abajo.

5. **C5 — Theme assigned 3 veces resolved** (T1.1)
   - `KitforgeThemeBinder` MonoBehaviour (parte de KitforgeRoot prefab) distribuye Theme single-field a los 3 managers.
   - Audit `ThemeConsistencyCheck` flag drift entre 3 theme refs.

6. **C6 — Buyer journey covered by PlayMode tests** (Q1.2 + Q1.4 + Q1.10)
   - PlayMode test suite `BuyerJourneyTests` con 5 tests (1 por grupo): load demo scene + EnterPlayMode + PopupManager.Show + assert visible + dismiss + assert dismissed.
   - `Documentation~/QAReports/SMOKE_TEST.md` checklist manual complementario.

7. **C9 — Pitch + description + keywords pre-RC freeze** (P1.3 + P1.4 + P1.10)
   - `package.json` description final centrada en valor (catálogo) primero, infra después.
   - keywords añade `prefab`, `template`, `kit`, `hybrid-casual`, `mid-core`.
   - Sample consolidation = decisión paralela (3 vs 8).

8. **C10 — CHANGELOG `[Unreleased]` purge + README simplify** (U1.10 + P1.5 + P1.6)
   - Pre-tag ritual: consolidar `[Unreleased]` content al tag block.
   - README mover Phase 1 done criteria a `Documentation~/INTERNAL.md`.

#### P1 — defer post-1.0 (post-RC `v1.1.0-alpha` o `v1.2.0`)

- **C7 — Theme reskin tools polish** (T1.3 + T1.4 + T1.5 + T1.9 + Q1.6)
  - `Tools → Theme → Capture All Snapshots` · `Theme → Switch Preset` menu · A/B comparator · palette generator · `ThemeReskinDeltaCheck` audit.
- **C8 — Tools menu jerarquía** (U1.4 + Q1.8)
  - Submenús `Setup` / `Build Individual Group` / `Theme` / `Audit` / `Developer` (Headless oculto).
- **Q1.3 — `UIServicesRefsCheck` audit promote** del ContextMenu a audit-coverage.
- **Q1.5 — CI doc `Documentation~/CI.md`** con GitHub Actions snippet.
- **Q1.7 — Verificación grep `OnUpdate-workaround-M3-sweep`** pre-RC (cheap, podría P0).
- **Q1.9 — EditMode tests builders + audit checks** (~10-15 tests adicionales).
- **T1.2 — UIThemeConfigEditor preview thumbnail mock popup** (TA workflow polish).
- **T1.6 — `Documentation~/CONTRIBUTING.md`** o sección "Adding ThemedImage/ThemedText".

#### P2 — drop / deferred indef

- **T1.7 — Animation preset switch menu** (descubribilidad baja, valor marginal).
- **T1.8 — Theme slot "Find Usages"** (audit ya cubre indirect).
- **T1.10 — Theme slots usage table autogenerada** (over-engineering, philosophy §5).
- **P1.9 — Documentation~/QAReports buyer-visible** (no problema real, scope-cap).
- **Q1.1 — 5 builders separadas regression friction** (cubierto post-M4.X via audit one-click).

---

### Trade-off pendiente decision (Bloque 1 → M5/RC kickoff)

**TR1 — Sample consolidation: 8 entries vs 3 entries en `package.json` samples[]**

Pros 8: granularidad, buyer importa solo lo que necesita, samples chicos = import rápido por sample.
Pros 3: choice paralysis eliminada (P1.4), one-click "import the catalog", store-page más limpio.
Recomendación PM: **3 samples** (Quickstart + Catalog full + GameWiring), pero LOCKED solo si buyer real testing confirma el trade-off. Decisión M5/RC kickoff.

**TR2 — Builders per group vs unified `Build Everything` only**

Pros separados: dev/QA workflow granular (rebuild un grupo después de cambio).
Pros unified: buyer no ve 5 entries en menu.
Recomendación: **mantener builders per group para dev/QA + añadir `Build Everything` como entry buyer-facing**. Per-group builders movidos a submenu `Build Individual Group` (C8).

---

### Métricas Bloque 1 — SUPERSEDED por Reframe 2026-05-09

> See "Reframe Bloque 1" section below.

---

## Reframe Bloque 1 — 2026-05-09 (workshop-drawer test applied)

> Applied lens: **¿esto sirve al usuario que ya usa el kit, o al evaluador externo / buyer?**. First wins. Findings re-classified per `feedback_audit_priority_tool_first.md`. Store/demos/docs findings auto-demote to P2/drop until user explicitly opens that scope.

### Triage de findings originales por workshop-drawer test

| Finding | Reframe | Razón |
|---|---|---|
| **U1.1** Build Everything menu | **P0 keep** | Reorganiza el cajón: 6 clicks → 1. |
| **U1.2** Master demo scene | **P2 drop** | Es demo. Lo último de lo último. |
| **U1.3** Quickstart README catalog | **P2 drop** | Es buyer-facing docs. |
| **U1.4** Tools menu sin separadores | **P0 keep** | Organización del cajón. |
| **U1.5** 4 MonoBehaviours, no KitforgeRoot prefab | **P0 keep** | Practicidad de prefab — drop & play. |
| **U1.6** Bootstrap Defaults dialog técnico | **P1 keep** | Editor UX que sirve al user, no al evaluator. |
| **U1.7** 8 samples sin guidance orden | **P2 drop** | Buyer-evaluation framing. |
| **U1.8** Sample dependencies implícitas | **P1 keep** | Friction de workflow real al usar el kit. |
| **U1.9** No buyer-facing index Documentation~ | **P2 drop** | Es docs buyer-facing. |
| **U1.10** CHANGELOG `[Unreleased]` exposed | **P2 drop** | Buyer-facing en Package Manager UI. |
| **T1.1** Theme 3 veces | **P0 keep** | Friction técnica diaria. |
| **T1.2** UIThemeConfigEditor no preview "all skins" | **P1 keep** | TA workflow polish. |
| **T1.3** Cambiar slot → 6 demo scenes Play | **P0 PROMOTED** | Daily TA workflow, friction grande. Cierra con snapshot tool. |
| **T1.4** No theme palette generator | **P1 keep** | Mejora workflow TA. |
| **T1.5** Theme presets escondidos en sample | **P1 keep** | Descubribilidad de tool. |
| **T1.6** ThemedImage rule no docs | **P2 drop** | Es docs (CONTRIBUTING). |
| **T1.7** 10 anim presets sin UI swap global | **P1 keep** | Descubribilidad de tool. |
| **T1.8** No "find usages" theme slot | **P1 keep** | TA workflow daily. |
| **T1.9** No A/B compare themes | **P2 drop** | Nice-to-have, no organiza el cajón. |
| **T1.10** No doc theme slots usage | **P2 drop** | Es docs. |
| **P1.1** Pitch 5min vs 30min real | **DROP** | Pitch = marketing. Friction técnica subyacente cubierta por R-C1. |
| **P1.2** Master demo + hero shot | **P2 drop** | Store. |
| **P1.3** package.json description confusa | **P2 drop** | Store. |
| **P1.4** 8 samples vs Doozy 1 | **P2 drop** | Buyer choice paralysis = buyer-facing. |
| **P1.5** README Status tabla expone alpha | **P2 drop** | Buyer-facing. |
| **P1.6** No "what's in the box" | **P2 drop** | Buyer-facing docs. |
| **P1.7** Sample dependencies LogError | **CUBIERTO U1.8** | Mismo tema. |
| **P1.8** Bootstrap Defaults nombre técnico | **CUBIERTO U1.6** | Mismo tema. |
| **P1.9** Documentation~/QAReports buyer | **P2 drop** | No problema real, scope-cap. |
| **P1.10** Asset Store keywords | **P2 drop** | Store. |
| **Q1.1** 5 builders separados regression | **P2 drop** | Cubierto post-M4.X (audit one-click). |
| **Q1.2** Audit no buyer journey | **P0 keep** (reframed) | Tool reliability "funciona como reloj"; PlayMode integration tests. NO buyer journey framing. |
| **Q1.3** UIServicesRefsCheck audit | **P0 PROMOTED** | Audit-coverage gap real, validation reliability. |
| **Q1.4** Smoke test manual | **P1 keep** | Reliability del developer del kit. |
| **Q1.5** CI hook headless audit | **P1 keep** | Reliability infra. |
| **Q1.6** Visual snapshot diff Casual↔Premium | **P0 PROMOTED** | Regression-catch tool reliability. |
| **Q1.7** OnUpdate-workaround anchors verificar | **P0 keep** | Hygiene cheap. |
| **Q1.8** Audit y Audit Headless lado a lado | **P1 keep** | Organización menu (cubierto en R-C1 / U1.4). |
| **Q1.9** No tests builders + audit checks | **P1 keep** | Reliability tool. |
| **Q1.10** No PlayMode integration tests | **P0 keep** | Consolida con Q1.2. |

### Convergencias post-reframe

| ID | Tema | Findings convergentes | Dispo |
|---|---|---|---|
| **R-C1** | **Reorganización del cajón** — `KitforgeRoot.prefab` + `KitforgeThemeBinder` + `Tools → Setup → Initialize / Build Everything / Add Scene Root` + Tools menu jerarquía (Setup / Build Individual / Theme / Audit / Developer) | U1.1 + U1.4 + U1.5 + T1.1 (+ Q1.8) | **P0** |
| **R-C2** | **Theme reskin tool reliability** — `Tools → Theme → Capture All Snapshots` (reusa `PrefabSnapshotCapture`) + `ThemeConsistencyCheck` audit (theme drift entre 3 managers, redundante post-R-C1 pero defense-in-depth) | T1.3 + T1.1 (parcial) | **P0** |
| **R-C3** | **Audit checks gap closure** — `UIServicesRefsCheck` (Q1.3) + `ThemeReskinDeltaCheck` snapshot hash≠ (Q1.6) + `OnUpdate-workaround-M3-sweep` anchor cleanup grep (Q1.7) | Q1.3 + Q1.6 + Q1.7 | **P0** |
| **R-C4** | **PlayMode integration tests** — 5 tests (1 por grupo): load demo + EnterPlayMode + Show popup + assert visible + dismiss + assert dismissed | Q1.2 + Q1.10 | **P0** |
| **R-C5** | **TA daily workflow tools** — palette gen (T1.4) + theme switch menu (T1.5) + anim preset switch (T1.7) + find usages (T1.8) + UIThemeConfigEditor preview thumbnail (T1.2) | T1.2 + T1.4 + T1.5 + T1.7 + T1.8 | **P1** |
| **R-C6** | **Editor UX wording** — Bootstrap Defaults dialog rephrased post-R-C1 (U1.6) + sample dependency pre-flight DisplayDialog (U1.8) | U1.6 + U1.8 | **P1** |
| **R-C7** | **Editor tests + CI infra reliability** — Smoke test mecanizable (Q1.4) + CI doc headless (Q1.5) + EditMode tests builders/audit checks (Q1.9) | Q1.4 + Q1.5 + Q1.9 | **P1** |

### Disposition Bloque 1 — REFRAMED 2026-05-09

#### P0 — REORGANIZACIÓN DEL CAJÓN (apply pre-RC, este es el verdadero scope M5/RC)

**1. R-C1 — KitforgeRoot prefab + Build Everything + Tools menu jerarquía**
   - **`Runtime/Bootstrap/KitforgeRoot.prefab`** — GameObject con UIManager + PopupManager + ToastManager + UIServices + nuevo `KitforgeThemeBinder` MonoBehaviour. Theme se asigna en UN solo Inspector field (KitforgeThemeBinder) que distribuye a los 3 managers en Awake.
   - **`Tools → Kitforge → UI Kit → Setup → Initialize Project`** (rename de Bootstrap Defaults) — sin cambios funcionales, solo renaming.
   - **`Tools → Kitforge → UI Kit → Setup → Build Everything`** (nuevo) — orquesta A→B→C→D→M4.1→E (mismo flujo que `Regenerate + Audit` del audit window pero invocado desde menu raíz). Final dialog "17 prefabs + 6 scenes generadas".
   - **`Tools → Kitforge → UI Kit → Setup → Add Scene Root`** (nuevo) — drop `KitforgeRoot.prefab` en el active scene.
   - **Tools menu jerarquía**:
     ```
     Tools/Kitforge/UI Kit/
       Setup/
         Initialize Project
         Build Everything
         Add Scene Root
       Build Individual Group/
         Group A · B · C · D · E
         M4.1 — Theme Presets
       Audit
       Developer/
         Audit (Run Headless)
     ```
   - Mata: U1.1, U1.4, U1.5, T1.1, Q1.8.

**2. R-C2 — Theme snapshot capture + ThemeConsistencyCheck audit**
   - **`Tools → Kitforge → UI Kit → Theme → Capture All Snapshots`** (nuevo) — ejecuta `PrefabSnapshotCapture` (ya existe en `Editor/Audit/Snapshots/`) sobre los 17 prefabs con el theme activo. Output a `Library/UIKitAudit/Snapshots/<themeName>/`. Click → carpeta abierta automáticamente. TA cambia un slot → 1 click → 17 thumbnails.
   - **`ThemeConsistencyCheck`** audit check — flag drift si `UIManager._themeConfig != PopupManager._themeConfig != ToastManager._themeConfig` en demo scenes. Defense-in-depth post-KitforgeThemeBinder.
   - Mata: T1.3, T1.1 (parcial).

**3. R-C3 — Audit checks gap closure (3 nuevos checks + 1 hygiene)**
   - **`UIServicesRefsCheck`** — escanea scenes y flag `UIServices` con refs MonoBehaviour null en alguno de los 8 service slots.
   - **`ThemeReskinDeltaCheck`** — valida `hash(snapshot Theme_Default) ≠ hash(snapshot Theme_Casual) ≠ hash(snapshot Theme_Premium)` por cada prefab. Si hashes iguales → reskin contract roto.
   - **Hygiene grep** — verificar `OnUpdate-workaround-M3-sweep` anchors limpiados post-M3a sweep. Si quedan, eliminar.
   - Mata: Q1.3, Q1.6, Q1.7.

**4. R-C4 — PlayMode integration tests**
   - `Tests/PlayMode/BuyerJourneyTests.cs` (rename interno: `KitFlowSmokeTests.cs` para no contaminar con marketing-speak): 5 tests, 1 por grupo. Cada test: load demo scene + EnterPlayMode + invoke ContextMenu trigger via reflection (o exponer test seam) + assert popup `gameObject.activeInHierarchy == true` + dismiss + assert dismissed.
   - Tool reliability — ya teníamos 292 EditMode, faltaba el "presiono botón → popup aparece → dismiss → siguiente popup".
   - Mata: Q1.2, Q1.10.

#### P1 — POLISH POST-REORGANIZACIÓN (post-RC, no bloqueante)

**5. R-C5 — TA daily workflow tools**
   - `Tools → Theme → Switch → [Default / Casual / Premium]` (T1.5)
   - `Tools → Animation → Switch Preset → [10 presets]` (T1.7)
   - `UIThemeConfigEditor` añade preview thumbnail mock popup (T1.2)
   - `UIThemeConfigEditor` botón "Find Usages" por slot (T1.8)
   - Theme palette generator (paste JSON colores → genera Theme.asset) (T1.4)

**6. R-C6 — Editor UX wording**
   - Bootstrap Defaults dialog rephrased: "Now drop `Setup → Add Scene Root` o el `KitforgeRoot.prefab` a tu escena" (U1.6)
   - Sample builders pre-flight DisplayDialog: si dependencia falta → "Group B+C requeridos. ¿Importarlos ahora? [Open Package Manager]" (U1.8)

**7. R-C7 — Editor tests + CI infra**
   - EditMode tests para builders + audit checks (~10-15 tests) (Q1.9)
   - `Documentation~/QAReports/SMOKE_TEST.md` checklist developer-facing (Q1.4)
   - `Documentation~/QAReports/CI.md` snippet GitHub Actions invocando headless audit (Q1.5)

#### P2 / DROP — fuera de scope hasta user lo abra

Todos los findings que pasaron el filtro store/demos/docs:
- **Store / Asset Store / pitch / hero shot / keywords / CHANGELOG buyer**: U1.10, P1.2, P1.3, P1.4, P1.5, P1.10, P1.9.
- **Buyer-facing docs**: U1.3, U1.9, T1.6, T1.10, P1.6.
- **Demo scenes / master demo**: U1.2.
- **Buyer journey / first-impression / fresh-import como buyer experience**: U1.7.
- **Cubiertos / consolidados**: P1.1 (cubierto R-C1), P1.7 (cubierto U1.8/R-C6), P1.8 (cubierto U1.6/R-C6).
- **Cubierto post-M4.X**: Q1.1.
- **Nice-to-have no organiza el cajón**: T1.9.

### Métricas Bloque 1 — REFRAMED

- **40 findings** generados, **40 triados** post-reframe.
- **7 convergencias accionables** post-filtro workshop-drawer (R-C1 a R-C7).
- **4 P0** (R-C1 a R-C4) + **3 P1** (R-C5 a R-C7) + **17 P2/drop** (filtrados por workshop-drawer test) + **3 cubiertos/consolidados**.
- **Findings que requieren código nuevo en M5/RC (P0 only)**:
   - R-C1: `KitforgeRoot.prefab` + `KitforgeThemeBinder.cs` + 3 menu items nuevos + Tools menu jerarquía (rename + reorganize 9 entries).
   - R-C2: 1 menu item nuevo (`Capture All Snapshots`) + 1 audit check nuevo (`ThemeConsistencyCheck`).
   - R-C3: 2 audit checks nuevos (`UIServicesRefsCheck` + `ThemeReskinDeltaCheck`) + 1 hygiene grep.
   - R-C4: 5 PlayMode tests + posibles test seams en demos.
- **Findings que NO requieren tag M5/RC** (P1 work post-tag): R-C5 + R-C6 + R-C7.

### Trade-offs reframed

- **TR1 (sample consolidation 8→3)**: **DROPPED**. Era buyer choice paralysis. No relevante hasta user abra scope store.
- **TR2 (builders per-group + Build Everything)**: **RESOLVED en R-C1**. Per-group movidos a submenu `Build Individual Group`, `Build Everything` añadido. Ambos coexisten.

**Bloque 1 cerrado y reframed**. Próximo bloque: **Pregunta 2 — ¿Es fácil para el usuario elegir qué prefabs cargar?** — aplicará workshop-drawer test desde la generación de findings, no solo en disposition.

---

## Bloques 2-6 (original plan) — SUPERSEDED 2026-05-09 (REPLAN)

> Original 4-roles × 6-preguntas plan superseded. User decidió 2026-05-09 mid-Bloque 1 que el audit se centra en **un solo perfil bestia**: game designer hype-casual con 1 prototipo/semana. Bloque 1 preservado como audit history (cleared lente correction). Nuevo único bloque profundo abajo.

---
