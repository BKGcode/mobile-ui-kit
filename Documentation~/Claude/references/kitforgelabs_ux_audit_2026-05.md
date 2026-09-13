---
name: kitforgelabs_ux_audit_2026-05
description: M3c UX audit del kit (pre v1.0.0-rc) — INDEX + Methodology + Buyer profile + Bloque 2 (as user game designer hype-casual, 50 casos REPLAN). Split 2026-06-12 (was 1307 lines): Bloque 3 (UX specialist + Hub design + Decisiones LOCKED) → kitforgelabs_ux_audit_2026-05_bloque3.md; Bloque 4 (as dev + plan aterrizaje M5.X) → kitforgelabs_ux_audit_2026-05_bloque4.md. SUPERSEDED 1-6 plan + Reframe → kitforgelabs_ux_audit_2026-05_archive.md.
type: project
status: active
---

# KF MobileUIKit — M3c UX Audit (2026-05)

> **Split 2026-06-12** (monolith >800 líneas). Este archivo = INDEX + Methodology + Buyer profile + **Bloque 2**.
> - **Bloque 3** (UX specialist · tool design · 50 casos · Hub design · Decisiones LOCKED) → `kitforgelabs_ux_audit_2026-05_bloque3.md`
> - **Bloque 4** (as dev · técnica · plan aterrizaje M5.X) → `kitforgelabs_ux_audit_2026-05_bloque4.md`
> - **SUPERSEDED 1-6 + Reframe** → `kitforgelabs_ux_audit_2026-05_archive.md`

> Audit puro: zero código, zero tag. Output = findings priorizados que alimentan disposition en M5/RC. Veredicto global "user-friendly 1000%" se emite al cierre del Bloque 6.

## Methodology

- **Roles** (4): `as user` (buyer hybrid-casual studio prototyping speed) · `as ta` (technical artist, art pipeline) · `as pm` (product manager, Asset Store viability) · `as qa` (tester / automated).
- **Preguntas operativas** (6): ver sección siguiente.
- **Casos**: 10 por pregunta por rol = 40 findings/bloque, 240 totales.
- **Estrategia** (locked 2026-05-09): B — bloque por pregunta. Disposition al cierre de cada bloque.
- **Veredicto global**: emitido tras Bloque 6, no incremental.
- **Disposition tags**: P0 (apply pre-RC) · P1 (defer post-1.0) · P2 (drop).
- **Finding ID**: `<Role>.<Question>.<Index>` — `U1.1`, `T1.5`, `P1.7`, `Q1.10`.

## Preguntas operativas

1. ¿Está todo centralizado para el usuario?
2. ¿Es fácil para el usuario elegir qué prefabs cargar?
3. ¿Es fácil para el usuario crear una escena y cargar todos los prefabs que necesita?
4. ¿Es fácil para el usuario "conectar" los prefabs y que la información fluya sin problema?
5. ¿Es fácil para el usuario modificar un prefab?
6. ¿Tenemos una tool para que el usuario personalice la creación de prefabs y escenas?

**Veredicto global (Q7)**: "¿Esta tool es user-friendly 1000%?" — emitido tras Bloque 6.

## Buyer profile (LOCKED — basis for all "as user" findings)

> Studio hybrid-casual mobile, 1-5 devs. Descarga el kit para **prototipar rápido** evitando reinventar UI mobile típica. Quiere "plug & play" entre sistemas para enfocarse en gameplay. NO quiere leer 24 specs ni configurar 4 MonoBehaviours antes de ver Reward popup en pantalla.

---

> **Archive note (2026-05-14):** Bloque 1 (original 4-roles audit), Reframe Bloque 1, and the SUPERSEDED Bloques 2-6 original-plan marker have been moved to [`kitforgelabs_ux_audit_2026-05_archive.md`](./kitforgelabs_ux_audit_2026-05_archive.md) to keep this file focused on the active audit (Bloque 2 REPLAN onwards). Audit history preserved verbatim in the archive.

---

## Bloque 2 (REPLAN 2026-05-09) — User audit deep dive: Game designer hype-casual

### Persona LOCKED — "el game designer bestia"

**Quién**: game designer junior-mid en hype-casual studio. 1-3 años experiencia. Conoce Unity GameObject + Component + drag-drop Inspector + Awake/Start/Update. **NO conoce**: DI, VContainer, asmdef, abstracciones, dependency injection, reflection. No es programmer.

**Qué hace**: 1 prototipo jugable cada semana. Brief Lunes 9am → showcase Viernes 11am. Trabaja paralelo con artist + 1 programmer (small team de 3). Iteration speed > best practices. Si algo no funciona en 5 min: pasa al siguiente kit o pregunta al programmer (que está ocupado).

**Su semana**:

| Día | Hora | Bloque |
|---|---|---|
| **L** | 9:00-10:00 | Brief, decisión gameplay, scope |
| **L** | 10:00-13:00 | **Setup proyecto + UI base** ← aquí muere o vive el kit |
| **L** | 14:00-18:00 | Gameplay loop básico |
| **M-Mi** | 9-18 | Gameplay + features |
| **J** | 9-13 | Polish + tweaks UI |
| **J** | 14-18 | Build + test device |
| **V** | 9-12 | Showcase studio |
| **V** | 13-18 | Decision next sprint, doc transition |

**Solo 3 horas Lunes** para UI base. Si tarda 4+ → entra al martes con UI inacabada → pierde día gameplay → llega Viernes con prototipo flojo. Si tarda 1 hora → 2 horas extra de gameplay polish.

**UI típica que necesita cada prototipo**: loading + main menu + HUD (coins/gems/energy/timer) + pause + game over + level complete + reward + shop + daily login + settings + tutorial + not-enough-currency + confirm + toast. **Match exacto con el catálogo del kit** — coincidencia perfecta.

**Lo que el kit DEBE darle el lunes mañana**:
1. Drop 1 prefab → escena con UI base lista (4 managers + theme wired).
2. Disparar cualquier popup en 1 línea código.
3. Mostrar HUD de un `int _coins` simple sin implementar 7 métodos de un service.
4. Cambiar 1 color del Theme → ver impacto en todo sin abrir 5 escenas.
5. Reusar todo en el siguiente proto la semana que viene.

**Anti-patrón que el kit NO debe forzar**:
- Leer 24 specs de Documentation~/Specs/Catalog/ antes de mostrar GameOver.
- Implementar IEconomyService, IProgressionService, ITimeService (21 métodos combinados) para mostrar 3 números en HUD.
- Wirear 4 GameObjects con 4 components y 3 theme refs cada lunes.
- Encontrar `SetTheme runtime API` en CHANGELOG `[Unreleased]` block.

---

### 50 findings as user — game designer bestia

> Format: `[U-X.N]` **Título** — *contexto temporal*. Síntoma + impacto en tiempo. **Fix**: solución concreta. **[Priority]**

#### Sección A — Lunes 9-10am: setup proyecto + escena (10 casos)

**[U-A.1] Bootstrap Defaults dialog técnico**
*Lunes 9:15am*. Click `Tools → Kitforge → UI Kit → Bootstrap Defaults`. Dialog dice "Assign this Theme to your UIManager / PopupManager / ToastManager". Game designer no sabe qué son. Cierra dialog perdido.
**Fix**: dialog redirige a "Setup → Add Scene Root" (1 next-step), no instrucciones técnicas.
**[P0]**

**[U-A.2] No KitforgeRoot prefab — 4 GameObjects manuales**
*Lunes 9:25am*. Crea escena nueva. Lee Quickstart README → debe crear `Canvas` + 2 RectTransform roots (`ScreenRoot`, `PopupRoot`) + 4 MonoBehaviours (UIManager, PopupManager, ToastManager, UIServices). 25 minutos jugando con Inspector + drag-drop. Cada prototipo igual.
**Fix**: `Runtime/Bootstrap/KitforgeRoot.prefab` drop → 4 managers + Canvas + roots wired automáticamente.
**[P0]**

**[U-A.3] Theme se asigna 3 veces — la mitad de las veces olvida una**
*Lunes 9:40am*. Drag `UIThemeConfig_Default` a `UIManager._themeConfig`. Y a `PopupManager._themeConfig`. Y a `ToastManager._themeConfig`. Olvida ToastManager. 10 minutos después al disparar Toast: visual roto. Debug 15 min hasta encontrar la causa.
**Fix**: KitforgeThemeBinder MonoBehaviour single-field distribuye Theme a los 3 managers en Awake.
**[P0]**

**[U-A.4] No "Add UI Kit to my scene" menu**
*Lunes 9:50am*. Tras leer Quickstart README, busca menú: "Add UI Kit to scene" o "Setup scene". No existe. Concluye que el kit no tiene helper de escena → setup manual obligatorio.
**Fix**: `Tools → Kitforge → UI Kit → Setup → Add Scene Root` instancia KitforgeRoot.prefab en active scene + ping prefab.
**[P0]**

**[U-A.5] Sin project template Unity con kit pre-wired**
*Lunes 9:00am*. Cada prototipo arranca desde Unity → New Project → 0 boilerplate. Cada lunes hace setup desde cero. 5 prototipos/mes × 30min/setup = 2.5 horas wasted.
**Fix**: ship `Project Template` (folder `ProjectTemplates~/KitforgeMobileGame/`) que Unity Hub puede usar al crear proyecto. Pre-wired: kit instalado vía manifest.json + escena base + KitforgeRoot prefab + Theme default.
**[P1]**

**[U-A.6] 9 entries Tools menu sin jerarquía → no sabe por dónde empezar**
*Lunes 9:05am*. Abre Tools → Kitforge → UI Kit. Ve: Audit, Audit (Run Headless), Bootstrap Defaults, Build Group A/B/C/D/E Sample, Build M4.1 — Theme Presets. 9 entries flat. ¿Qué hago primero? Click "Build Group A Sample" (parece accionable). Spam de prefabs. Confusión.
**Fix**: jerarquía Setup / Build Individual / Theme / Audit / Developer. Setup contiene los 3 lunes-essential entries.
**[P0]**

**[U-A.7] No scene template (.unity preset) "MobileGameBoot.unity"**
*Lunes 10:00am*. Game designer quiere "1 escena con loading + main menu + HUD pre-cableado para empezar gameplay". Tiene que componerla manualmente desde Group E demo + samples. 30 min.
**Fix**: ship `Samples~/SceneTemplates/MobileGameBoot.unity` + `MobileGameLevel.unity`. Drop, Play, ya tienes loading + mainmenu + HUD.
**[P0]**

**[U-A.8] Audit window primer click expone tabs/options dev-internal**
*Lunes 9:30am exploratorio*. Click `Audit`. Ventana con tabs (Reports / Settings / Targets) + options (MirrorReportsToAssets, Snapshots, etc.). Game designer no es dev. No usa audit. Pero el tool aparece en `Setup` para él en M4.X de igual manera.
**Fix**: Audit movido a `Tools → Kitforge → UI Kit → Developer → Audit` (oculto detrás de submenu Developer). Game designer no lo ve por accidente.
**[P1]**

**[U-A.9] Sample Quickstart no enseña catalog usage**
*Lunes 10:30am*. Importa `Quickstart` sample. README dice cómo Push/Show un screen/popup genérico. Game designer espera ejemplo `popupManager.Show<RewardPopup>(new RewardPopupData(...))`. No está.
**Fix**: Quickstart sample añade 2 ContextMenu nuevos: `Show RewardPopup demo` + `Show GameOverPopup demo` con DTO populated minimal. Game designer ve el patrón canonical.
**[P0]**

**[U-A.10] Sin "Hello World popup" 1-click**
*Lunes 11:00am*. Quiere validar que el kit funciona end-to-end en 30s. Tendría que: import sample + run builder + abrir scene + Play + ContextMenu. Mínimo 5 min. Si falla algo: 30 min debug.
**Fix**: `Tools → Kitforge → UI Kit → Hello World` instancia KitforgeRoot + dispara ConfirmPopup demo en active scene (entra Play mode automáticamente). 1 click → popup en pantalla.
**[P0]**

#### Sección B — L-Mi: disparar popups en gameplay (10 casos)

**[U-B.1] No tutorial "cómo disparar GameOverPopup" en docs principales**
*Martes 10am*. Player muere → quiere mostrar GameOverPopup. Lee README → no menciona catalog usage. Lee `Documentation~/Specs/Catalog/GameOverPopup.md` (24 specs). 5 min lectura. Para 1 popup.
**Fix**: `Documentation~/CHEATSHEET.md` 1-page: 17 popups × 1 línea cada uno con snippet `popupManager.Show<X>(new XData(...))` y campos DTO esenciales.
**[P0]**

**[U-B.2] Cada popup necesita DTO clase**
*Martes 10:30am*. Para GameOverPopup necesita instanciar `new GameOverPopupData(...)`. Constructor con 8 params. Game designer no sabe los nombres ni el orden. Abre IDE → autocomplete. Pero los valores válidos para `ContinueMode` enum tampoco los sabe.
**Fix**: DTOs con constructor sin params + `[SerializeField]` fields visibles en Inspector. Game designer asigna valores en Inspector + usa `popupManager.Show<GameOverPopup>(_gameOverData)`. Snippet DTO en cheat-sheet.
**[P1]**

**[U-B.3] 24 specs en Documentation~/Specs/Catalog/ → game designer no lee**
*Martes 11am*. Quiere ver lista de popups disponibles. Abre Documentation~ → 24 .md files con nombres de componente. Spec por popup ≈ 200-400 líneas. Lectura imposible para game designer con 3 horas/UI.
**Fix**: 1-page `CHEATSHEET.md` (mismo que U-B.1) reemplaza 24 specs como entry point. Specs siguen existiendo para programmer del studio.
**[P0]**

**[U-B.4] Popup queue: 2x GameOver = 2 en queue → user esperaba replace**
*Martes 14:00*. Player muere 2 veces seguidas (bug de gameplay). 2 GameOverPopups en queue. Player ve 1, dismiss, ve otro. Confunde. Game designer no sabe cómo cambiar a "replace" (descartar duplicados).
**Fix**: PopupManager API añade `Show<T>(data, ShowMode.Replace)`. Cheat-sheet documenta: por defecto = queue, opcional Replace.
**[P1]**

**[U-B.5] Stack popups con Back button: comportamiento por defecto OK pero no doc**
*Miércoles 9am*. Abre Pause → dentro Settings → back button. ¿Cierra solo Settings o ambos? Game designer no sabe. Test manual.
**Fix**: cheat-sheet 1 línea: "Back button cierra el popup top de la stack. Apila popups con `Show<T>` consecutivos."
**[P1]**

**[U-B.6] Sin "Popup Test Launcher" window → Editor floating menu**
*Miércoles 11am*. Quiere ver cómo se ve LevelCompletePopup con stars=3 score=10000. Tendría que: editar DTO + Play scene + trigger. Sin floating debug menu en Editor Play mode "click button → show popup with mock data".
**Fix**: `Tools → Kitforge → UI Kit → Popup Test Launcher` window: list de los 17 popups + DTO mock-fill UI + button "Show now" (requiere Play mode + KitforgeRoot en escena).
**[P0]**

**[U-B.7] Sin gallery prefabs "browse 17 popups, drag to scene"**
*Lunes 11:30am*. Quiere ver visualmente qué popups hay. Solo encuentra prefabs post-Build A+B+C+D+E en 5 carpetas distintas. No hay overview.
**Fix**: post-`Build Everything`, generar `Catalog Browser` window: grid de thumbnails (snapshot per prefab) + drag-to-scene. Reusa `PrefabSnapshotCapture`.
**[P0]**

**[U-B.8] LevelComplete DTO oculto: stars / score / bestScore no obvio**
*Miércoles 13:00*. Llama `Show<LevelCompletePopup>(new LevelCompletePopupData(?))`. Constructor con 5 params: `stars`, `score`, `bestScore`, `levelLabel`, `mode`. Sin Inspector preview. Sin doc compacto.
**Fix**: cubierto por cheat-sheet (U-B.1) + DTO Inspector-friendly (U-B.2).
**[P1]**

**[U-B.9] TutorialPopup necesita TutorialStep[] array**
*Jueves 10am*. Quiere mostrar 3-step tutorial. DTO requiere `TutorialStep[]`. Game designer no entiende "array of structs". Abre spec → 4 min lectura. Crea array hardcoded en `Awake`.
**Fix**: TutorialPopupData fields visibles en Inspector + `[Serializable] TutorialStep[]` que Unity expone. Game designer compone steps en Inspector. Snippet en cheat-sheet.
**[P1]**

**[U-B.10] ConfirmPopup OnConfirmed event subscription no obvio**
*Jueves 11am*. `popupManager.Show<ConfirmPopup>(data)`. ¿Cómo sé cuando user confirma? Necesita subscribir a evento. ¿En la propia popup instance? ¿En PopupManager? Spec dice "events emit DTO ride-along". Game designer no entiende.
**Fix**: cheat-sheet snippet:
```csharp
var popup = popupManager.Show<ConfirmPopup>(data);
popup.OnConfirmed += () => { /* tu código */ };
```
+ cheat-sheet documenta el patrón canonical para los 17 popups.
**[P0]**

#### Sección C — L-V: HUD + servicios (10 casos)

**[U-C.1] HUDCoins necesita IEconomyService — 7 métodos a implementar**
*Lunes 12:30*. Quiere mostrar coins. Encuentra HUDCurrency parameterized. Builder Group B asume IEconomyService stub. Game designer NO quiere implementar 7 métodos para mostrar `int`.
**Fix**: `HUDSimple` prefab + script: `[SerializeField] int _value` setable runtime via `SetValue(int)`. Sin IEconomyService. Para gameplay simple. Coexiste con HUDCurrency (que requiere service para games complejos).
**[P0]**

**[U-C.2] Sin "simple HUD" sin servicios**
*Lunes 12:45*. Cubierto por U-C.1.
**Fix**: ver U-C.1.
**[P0]** (mismo fix que U-C.1)

**[U-C.3] HUDEnergy necesita IProgressionService**
*Martes 11:00*. Quiere energy bar 5/5. Spec → IProgressionService.GetEnergyRegenState() + 6 métodos más. Game designer abandona, hardcodea Image fillAmount manual.
**Fix**: `HUDEnergySimple` con `[SerializeField] int _current, _max, _regenSeconds`. Auto-tick local. Sin servicio. Para games con energy gameplay simple.
**[P0]**

**[U-C.4] HUDTimer necesita ITimeService**
*Martes 14:00*. Quiere countdown 60s gameplay. Spec → ITimeService + TimerMode enum. Game designer hace `Time.time` manual.
**Fix**: `HUDTimerSimple` con `[SerializeField] float _duration` + `StartCountdown()` API directa. Sin servicio.
**[P0]**

**[U-C.5] SettingsPopup necesita IPlayerDataService — PlayerPrefsPlayerDataService default escondido**
*Jueves 10am*. SettingsPopup se abre, music/sfx sliders. ¿Persisten? Spec dice "via IPlayerDataService". Default impl `PlayerPrefsPlayerDataService` existe en Runtime ✅ pero game designer no sabe que existe.
**Fix**: KitforgeRoot prefab pre-wirea `PlayerPrefsPlayerDataService` por defecto. Game designer no toca. Settings persisten "out of the box".
**[P0]**

**[U-C.6] InMemory stubs (8 archivos en Samples~) → user importa pero no entiende**
*Lunes 12:00*. Importa Group B sample. Trae `InMemoryEconomyService` + `InMemoryShopDataProvider` + `InMemoryAdsService`. Game designer no entiende qué son ni si los tiene que reemplazar.
**Fix**: stubs renombrados con prefijo `Stub` y comentario doc-string en clase: `"// Replace with your real economy service. This stub gives 1000 coins on start."`. Cheat-sheet 1 línea: "Stubs in samples = throwaway. Replace when your gameplay needs real economy."
**[P1]**

**[U-C.7] UIServices container 8 ref slots Inspector**
*Lunes 12:15*. UIServices tiene 8 SerializeField MonoBehaviour refs. Game designer arrastra los stubs uno a uno. Olvida `_audioRouterRef` → ToastManager intenta sound → null → silent fail.
**Fix**: Audit `UIServicesRefsCheck` (ya en R-C3 P0). + UIServices Inspector custom editor highlight slots null en rojo + warning Validate-button en Inspector.
**[P0]**

**[U-C.8] GameWiring sample con VContainer = scope-overkill**
*Lunes 12:30 exploratorio*. Ve sample "Game Wiring (VContainer)". Importa por curiosidad. LifetimeScope + 8 stubs + asmdef gated `KFMUI_HAS_VCONTAINER`. No entiende. Asusta.
**Fix**: Sample displayName añade prefijo `(advanced)`. Description dice claro: "For studios already using VContainer DI. Skip if you don't know what VContainer is." Game designer no abre.
**[P1]**

**[U-C.9] Cómo conectar HUDCoins a su `int _gameCoins` simple → no doc**
*Martes 9:30am*. Quiere HUDCoins display que reaccione a `_player.Coins` int field. Sin economy service, sin events. Cómo? No-doc.
**Fix**: cubierto por U-C.1 (HUDSimple + SetValue API).
**[P0]**

**[U-C.10] No "events-only mode" HUD**
*Martes 9:45*. Quiere `Coins.Changed += value => hud.SetValue(value)`. Sin servicios. Sin DI. Sin abstracción.
**Fix**: cubierto por U-C.1 + cheat-sheet snippet "Events-only HUD mode".
**[P0]**

#### Sección D — L-V: visual customization (10 casos)

**[U-D.1] Theme 16 ColorSlots: ¿cuál es "color principal"?**
*Lunes 14:00*. Quiere cambiar tono del juego. Abre Theme.asset. 16 slots: PrimaryColor, SecondaryColor, AccentColor, BackgroundColor, BackgroundLight, BackgroundDark, TextColor, TextOnPrimary, TextOnAccent, MutedColor, TertiaryColor, SuccessColor, WarningColor, FailureColor, ... Confunde. ¿Cuál es el "el color del juego"?
**Fix**: `UIThemeConfigEditor` agrupa slots por categoría con [Header] foldouts: "Brand colors (3 main)" + "Backgrounds (3)" + "Text (3)" + "Status (3)" + "Other (4)". Tooltips por slot: "PrimaryColor: main brand color, used in primary buttons + active accents."
**[P0]**

**[U-D.2] Theme 8 SpriteSlots: ¿cuál es "fondo de botón"?**
*Lunes 14:30*. Quiere botón con sprite custom del juego. 8 sprite slots: IconEnergy, IconClock, StarFilledSprite, StarEmptySprite, etc. No "ButtonBackground". Concluye que botones NO usan sprite via Theme → modifica prefab manualmente.
**Fix**: documentar en UIThemeConfigEditor tooltips qué slot usa qué prefab. + Audit `ThemeSlotUsageCheck` que escanea callsites. + cheat-sheet: 1-line "Theme slot → consumed by".
**[P1]**

**[U-D.3] Cambiar slot → Play 5 demo scenes para ver impacto**
*Lunes 15:00*. Cambia PrimaryColor → quiere ver impacto. Sin live preview. Debe abrir 5 demo scenes + Play cada una. 30 min. Cada theme iteration.
**Fix**: `Tools → Theme → Capture All Snapshots` (R-C2 ya P0). 1 click → 17 thumbnails de prefabs reskined.
**[P0]** (ya P0 en Bloque 1 reframe)

**[U-D.4] Theme 1 font slot → game designer quiere title + body**
*Lunes 15:30*. Quiere title font cinemático + body font legible. Theme tiene 1 `_fontAsset`. Asigna title → todos los textos en ese font (incluye body). Mal.
**Fix**: añadir `_titleFont` + `_bodyFont` + `_labelFont` (3 slots). Catalog prefabs ThemedText puede consumir el que corresponda via enum FontSlot.
**[P0]**

**[U-D.5] Custom button shape: ¿modificar prefab o Theme?**
*Lunes 16:00*. Quiere botón redondeado. ¿Modifica el sprite del prefab o cambia Theme.ButtonSprite? Confusión. Acaba modificando el prefab → su modificación se pierde la próxima vez que regenera con builder.
**Fix**: cheat-sheet sección "Customizing visual style": "Sprite global del juego → Theme. Sprite override en 1 popup específico → modifica prefab variant, no el original. Builder NO sobrescribe variants."
**[P0]**

**[U-D.6] SetTheme runtime API existe (M4.X) pero no documentado**
*Martes 9am*. Quiere "Dark Mode" toggle in-game. SetTheme existe en UIManager / PopupManager / ToastManager. Game designer no sabe. Hardcodea cambios manual + bug.
**Fix**: cheat-sheet snippet "Switch theme at runtime: `uiManager.SetTheme(theme)` + same on PopupManager + ToastManager. Or use KitforgeThemeBinder.SetTheme(theme) for single call."
**[P0]**

**[U-D.7] Theme A juego 1, Theme B juego 2 → reasignar 3 veces cada proto**
*Semana 2 lunes*. Nuevo proto. Copia Theme.asset del proto anterior. Reasigna 3 veces (UIManager+PopupManager+ToastManager). Cada lunes.
**Fix**: cubierto por R-C1 (KitforgeThemeBinder single-field).
**[P0]** (ya P0)

**[U-D.8] No Theme palette generator — 16 colores click-by-click**
*Lunes 15:00*. Studio le da palette JSON con 16 colores. Tiene que asignar uno por uno en Inspector. 5 min × 3 themes (Default + 2 custom) = 15 min/proto.
**Fix**: `UIThemeConfigEditor` añade botón "Import from JSON / paste hex codes" → 16 fields auto-fill.
**[P1]**

**[U-D.9] Theme presets Default/Casual/Premium escondidos en sample M4.1**
*Lunes 14:15*. Quiere "tono casual brillante". Existe Theme_Casual ✅ pero solo si importa M4.1 sample. Game designer no sabe que existe.
**Fix**: Theme presets shipped en `Runtime/Theme/Presets/Theme_Casual.asset` + `Theme_Premium.asset` (movidos del sample al Runtime). KitforgeRoot prefab default Theme = Default; user cambia Inspector dropdown a Casual/Premium si quiere.
**[P0]**

**[U-D.10] No "Reskin all → screenshot" tool**
*Lunes 16:30*. Cubierto por U-D.3 (Capture All Snapshots = mismo concept).
**Fix**: ver U-D.3.
**[P0]**

#### Sección E — Iteración + multi-proto + edge cases (10 casos)

**[U-E.1] Semana 2 nuevo proto — setup repeats**
*Semana 2, Lunes 9am*. Nuevo proto. Repite TODO: 4 GameObjects + Theme 3x + 8 service stubs. 30 min wasted otra vez.
**Fix**: cubierto por R-C1 (KitforgeRoot.prefab + Add Scene Root) + U-A.5 (Project Template).
**[P0]**

**[U-E.2] Reuso Theme entre proyectos → manual copy**
*Semana 3*. Tiene Theme custom de proto 1 que quiere reusar en proto 3. Manual copy del .asset entre `Assets/Settings/UI/`. No hay export.
**Fix**: `Tools → Theme → Export Package` exporta Theme.asset + sprites + fonts referenciados como `.unitypackage`. Import en otro proyecto. Quick win.
**[P1]**

**[U-E.3] Reuso prefabs custom (popup estilizado proto1 → proto2)**
*Semana 3*. Tiene RewardPopup variant con sprites del juego 1. Quiere reusar en juego 3 (similar art style). Manual copy.
**Fix**: cheat-sheet sección "Sharing prefab variants across projects". Quick doc, no code.
**[P2]** (game designer hace copy paste sin docs).

**[U-E.4] No "force day N daily login" debug button**
*Miércoles 11am*. Quiere validar DailyLogin day 5 visual. Tiene que setear `LastClaimUtc = today - 5 days` en el stub manualmente. Reflection o code edit.
**Fix**: `InMemoryProgressionService` Inspector adds `[ContextMenu("Force Day N")]` con int field `_forcedDay` + button. Click → setea state.
**[P0]**

**[U-E.5] No "force not enough coins" debug button**
*Jueves 14:00*. Quiere ver NotEnoughCurrencyPopup. Tiene que setear `_coins = 0` en stub. Edit manual.
**Fix**: `InMemoryEconomyService` Inspector adds `[ContextMenu("Set Coins to 0 / 1000 / Max")]` quick presets.
**[P0]**

**[U-E.6] No floating debug menu in Editor Play mode**
*Jueves 15:00*. Quiere disparar cualquier popup sin tocar código. Sin floating menu in Play mode "click button → show popup with mock data".
**Fix**: cubierto por U-B.6 (Popup Test Launcher window). Mismo fix.
**[P0]**

**[U-E.7] Cambiar DTO + Play + click + ver: 30s × 16 iters = 8min**
*Jueves 16:00*. Iterando GameOverPopup texto: edita DTO en MonoBehaviour del demo → Play → click trigger → ver. 30s/iter. Si itera 16 veces (palabras + tamaño + color) = 8 min wasted.
**Fix**: cubierto por U-B.6 (Popup Test Launcher con mock-fill UI live, sin Play mode loop).
**[P0]**

**[U-E.8] Crash NullReference: no validación pre-Play**
*Lunes 17:00*. Olvida wirear UIServices.Economy. Click "Show ShopPopup" → `NullReferenceException`. 20 min debug.
**Fix**: `UIServicesRefsCheck` audit (R-C3 P0). + UIManager validation OnEnable que LogError actionable: "[ShopPopup] IEconomyService not registered. Wire it on UIServices before opening this popup."
**[P0]**

**[U-E.9] Game designer en Unity 2022 (proyecto legacy) → kit requiere 6000.1**
*Estudio con proyectos legacy*. Game designer asignado a un legacy project Unity 2022. Importa kit → errores de compilación obscuros.
**Fix**: `package.json` `unity: 6000.1` ✅ ya está. Pero error message si Unity < 6000.1 debería ser claro: añadir Editor scripted check que muestre dialog "Kitforge UI Kit requires Unity 6000.1 or newer. You are on 2022.X. Aborting import." Si Unity lo permite (script ejecuta antes del compile fail). Sino: `Documentation~/COMPATIBILITY.md` 1 línea.
**[P2]** (legacy fuera de scope hype-casual workflow primary).

**[U-E.10] Build Android: safe-area not handled**
*Jueves 17:00*. Build Android. Test device iPhone 13 (notch) o Galaxy S22 (cutout). UI clipped en notch. Non-goal #7 dice "no automatic safe-area handling". Game designer hype-casual NO va a leer Non-goals, espera safe-area built-in en mobile UI kit.
**Fix**: revaluar Non-goal #7 — ship `SafeAreaFitter` MonoBehaviour minimal en Runtime (no full safe-area framework, solo aplica `Screen.safeArea` a un RectTransform en OnEnable). KitforgeRoot Canvas wrap RectTransform con SafeAreaFitter aplicado por defecto. Game designer en notch device → UI dentro de safe-area sin tocar nada. **Esto es el caso uso #1 de un mobile UI kit en 2026** — Non-goal #7 contradice el pitch "mobile" del kit.
**[P0]**

---

### Convergencias post-50

7 clusters accionables emergen del análisis de 50 casos:

| ID | Cluster | Findings | Dispo |
|---|---|---|---|
| **R-U-C1** | **Drop & play setup** — KitforgeRoot.prefab + KitforgeThemeBinder + Tools → Setup → Initialize / Build Everything / Add Scene Root + Hello World 1-click + Project Template Unity Hub + scene templates `MobileGameBoot.unity` / `MobileGameLevel.unity` + Tools menu jerarquía | A.1, A.2, A.3, A.4, A.5, A.6, A.7, A.10, B.7, D.7, E.1 | **P0** |
| **R-U-C2** | **Catalog discoverability + cheat-sheet + ~~DTO Inspector-friendly~~** — `Documentation~/CHEATSHEET.md` 1-page con 17 popups + ~~DTOs Inspector-friendly~~ + cheat-sheet snippets event subscription + Catalog Browser window post-Build. **⚠️ DTO sub-item INVALIDATED 2026-05-09 (M5.1 post-discovery)** — empirical audit de los 13 DTOs encontró que ya están en formato target (`[Serializable]` + public fields + parameterless ctor + sensible defaults); grep `new \w+PopupData\(` retornó 0 hits con args positional. Refactor cancelado en M5.1. Cheat-sheet + Catalog Browser snippets sub-items siguen válidos (M5.2 / M5.3). | A.9, B.1, B.2, B.3, B.5, B.8, B.9, B.10, D.5, D.6 | **P0** (parcial — DTO refactor sub-item RESOLVED) |
| **R-U-C3** | **Simple HUD + simple services** — `HUDSimple` / `HUDEnergySimple` / `HUDTimerSimple` prefabs + scripts + KitforgeRoot pre-wirea PlayerPrefsPlayerDataService default + service stubs renombrados Stub* + GameWiring sample marked (advanced) | C.1, C.2, C.3, C.4, C.5, C.8, C.9, C.10 | **P0** |
| **R-U-C4** | **Popup Test Launcher window + Inspector debug helpers** — floating window con 17 popups + DTO mock-fill UI + button "Show now" en Play mode + ContextMenu helpers en stubs (Force Day N, Force Coins, etc.) | B.6, E.4, E.5, E.6, E.7 | **P0** |
| **R-U-C5** | **Theme polish for hype-casual** — UIThemeConfigEditor categorías [Header] foldouts + tooltips + capture all snapshots tool + multi-font (3 slots) + theme presets en Runtime (no sample) + SetTheme cheat-sheet | D.1, D.2, D.3, D.4, D.6, D.9, D.10 | **P0** |
| **R-U-C6** | **Validation pre-Play + actionable errors + popup queue config** — UIServicesRefsCheck audit (ya en Bloque 1 R-C3) + Inspector custom editor UIServices red highlight + ShowMode.Replace API + LogError actionable format | B.4, C.7, E.8 | **P0** |
| **R-U-C7** | **Mobile reality: safe-area built-in** — `SafeAreaFitter` MonoBehaviour en Runtime + KitforgeRoot Canvas pre-wirea + revaluación Non-goal #7 | E.10 | **P0** |
| **R-U-C8** | **Polish post-RC P1** — Project Template (E.1 split), Theme JSON import, custom-stub renaming, GameWiring sample renaming, font slot expansion, theme export package, Audit window oculto submenu Developer | A.5, A.8, C.6, C.8, D.8, E.2 | **P1** |
| **R-U-C9** | **DROP / out of scope hype-casual primary workflow** | E.3 (prefab variant copy docs), E.9 (Unity 2022 compat) | **P2** |

### Disposition Bloque 2 (REPLAN) — workshop-drawer test post-50

#### P0 — los 7 P0 que importan para "monta UI en minutos"

**1. R-U-C1 — Drop & play setup**
   - `Runtime/Bootstrap/KitforgeRoot.prefab` + `KitforgeThemeBinder.cs`
   - 3 nuevos menu items: `Tools → Setup → Initialize Project / Build Everything / Add Scene Root`
   - `Tools → Hello World` (instancia + dispara ConfirmPopup demo)
   - 2 scene templates: `Samples~/SceneTemplates/MobileGameBoot.unity` + `MobileGameLevel.unity`
   - Tools menu jerarquía (Setup / Build Individual / Theme / Audit / Developer)
   - **Fixes**: A.1, A.2, A.3, A.4, A.6, A.7, A.10, B.7, D.7, E.1

**2. R-U-C2 — Catalog discoverability + cheat-sheet**
   - `Documentation~/CHEATSHEET.md` 1-page con 17 popups: 1 línea snippet por popup (`Show<X>(new XData(...))`) + 1 línea event subscription pattern
   - DTOs refactor a `[Serializable]` con [SerializeField] fields visibles en Inspector + constructor `()`
   - `Catalog Browser` window post-`Build Everything`: grid thumbnails + drag-to-scene
   - Quickstart sample añade 2 ContextMenu nuevos: `Show RewardPopup demo` + `Show GameOverPopup demo`
   - **Fixes**: A.9, B.1, B.2, B.3, B.5, B.8, B.9, B.10, D.5, D.6

**3. R-U-C3 — Simple HUD + simple services**
   - 3 nuevos prefabs/scripts: `HUDSimple.cs` (`SetValue(int)`), `HUDEnergySimple.cs` (`_current/_max/_regenSeconds` Inspector), `HUDTimerSimple.cs` (`StartCountdown(float)`)
   - KitforgeRoot prefab pre-wirea `PlayerPrefsPlayerDataService` default
   - In-memory stubs renombrados: `InMemoryEconomyService` → `StubEconomy_InMemory` + clase doc-string explica
   - GameWiring sample displayName → `Game Wiring (advanced — VContainer)`
   - **Fixes**: C.1, C.2, C.3, C.4, C.5, C.8, C.9, C.10

**4. R-U-C4 — Popup Test Launcher + debug helpers**
   - `Tools → Kitforge → UI Kit → Popup Test Launcher` Editor window
   - Inspector ContextMenu helpers: `InMemoryProgressionService.Force Day N`, `InMemoryEconomyService.Set Coins (0/1000/Max)`, `InMemoryTimeService.Skip Hour/Day`, etc.
   - **Fixes**: B.6, E.4, E.5, E.6, E.7

**5. R-U-C5 — Theme polish hype-casual**
   - `UIThemeConfigEditor` categorías [Header] foldouts + tooltips por slot
   - `Tools → Theme → Capture All Snapshots` (ya en Bloque 1 R-C2)
   - Multi-font: añadir `_titleFont` + `_bodyFont` + `_labelFont` (3 slots) + ThemedText FontSlot enum
   - Theme presets movidos a `Runtime/Theme/Presets/Theme_Casual.asset` + `Theme_Premium.asset`
   - SetTheme cheat-sheet snippet
   - **Fixes**: D.1, D.2, D.3, D.4, D.6, D.9, D.10

**6. R-U-C6 — Validation pre-Play + actionable errors**
   - `UIServicesRefsCheck` audit (ya en Bloque 1 R-C3)
   - UIServices Inspector custom editor: red highlight slots null + Validate button visible
   - PopupManager API añade `Show<T>(data, ShowMode.Replace)`
   - LogError format actionable: `"[<Popup>] I<Service> not registered on UIServices. Wire it before opening. See CHEATSHEET.md § Service binding."`
   - **Fixes**: B.4, C.7, E.8

**7. R-U-C7 — Safe-area built-in (revaluar Non-goal #7)**
   - `Runtime/Mobile/SafeAreaFitter.cs` MonoBehaviour minimal: aplica `Screen.safeArea` a RectTransform en OnEnable + OnRectTransformDimensionsChange
   - KitforgeRoot prefab Canvas envuelve RectTransform con SafeAreaFitter aplicado por defecto
   - README Non-goal #7 actualizado: `~~No automatic safe-area handling~~ — Safe-area handled by built-in `SafeAreaFitter`. For advanced device-specific handling beyond `Screen.safeArea`, use a dedicated package.`
   - **Fix**: E.10. **Decision**: Non-goal #7 inversión — el kit es mobile-first; no shippear safe-area = pitfall en cada notch device en 2026.

#### P1 — polish post-tag (no bloquea v1.0.0-rc)

**8. R-U-C8 — Polish post-RC**
- `ProjectTemplates~/KitforgeMobileGame/` Unity Hub template (E.1 split)
- Theme JSON palette import (D.8)
- Stub naming polish (C.6) — si no entra en R-U-C3
- Audit window movido a submenu Developer (A.8)
- Theme export package menu (E.2)
- **Fixes**: A.5, A.8, C.6, D.8, E.2

#### P2 — drop / out of scope

- E.3 — Reuso prefab variants entre proyectos. Game designer copy-paste sin doc; no problema real.
- E.9 — Unity 2022 backward-compat. Hype-casual studio en 2026 está en Unity 6+. No relevante.

---

### Métricas Bloque 2 (REPLAN)

- **50 findings** as user, perfil game designer hype-casual.
- **Workshop-drawer test aplicado desde generación** — sin findings store/demos/buyer-evaluator.
- **9 clusters** identificados (R-U-C1 a R-U-C9).
- **7 P0** (R-U-C1 a R-U-C7) + **1 P1 cluster** (R-U-C8) + **2 P2 drops**.
- **Findings nuevos vs Bloque 1 reframed** que aparecen sólo cuando aplicas el lente bestia game designer:
  - **Hello World 1-click** (A.10) — onboard 30s vs 5min.
  - **Cheat-sheet** (B.1, B.3) — reemplaza 24 specs como entry point.
  - **DTO Inspector-friendly** (B.2, B.8, B.9) — game designer compone DTO sin código.
  - **Popup Test Launcher window** (B.6, E.6, E.7) — itera sin Play loop.
  - **HUDSimple sin servicios** (C.1-C.4, C.9, C.10) — el game designer no quiere implementar 7 métodos.
  - **Catalog Browser window** (B.7) — drag thumbnails to scene.
  - **Inspector debug helpers en stubs** (E.4, E.5) — Force Day N, Force Coins.
  - **UIThemeConfigEditor categorías + tooltips** (D.1, D.2) — 16 colors agrupados.
  - **Multi-font slots** (D.4) — title + body + label.
  - **Theme presets en Runtime** (D.9) — no escondidos en sample.
  - **SafeAreaFitter built-in** (E.10) — invierte Non-goal #7. **Mobile reality 2026**.
  - **Scene templates** (A.7) — `.unity` preset listo.

### Decisiones críticas que requieren confirmación user

1. **R-U-C7 SafeAreaFitter**: invierte Non-goal #7. ¿Confirmas que safe-area built-in entra en M5/RC? Es un cambio de scope explícito vs el README actual.
2. **R-U-C3 HUDSimple variants**: ¿shipping junto a HUDCurrency (coexisten) o reemplazo? Reco coexisten — HUDCurrency para games con economy real, HUDSimple para hype-casual prototipos.
3. ~~**R-U-C2 DTO refactor**: ¿breaking? Refactorizar DTOs de constructor-style a Inspector-style toca 17 archivos. Reco hacerlo en M5/RC pre-tag (último BREAKING aceptable; v1.0.0-rc es la chance final).~~ **RESOLVED 2026-05-09 — false finding, INVALIDATED en M5.1.** Empirical audit de los 13 DTOs encontró que ya están en formato target. No refactor needed; decisión cerrada sin acción. Lección: "Inherited framing requires empirical re-verification" — el framing del audit no se verificó contra el código actual antes de lockear como P0.
4. **R-U-C5 Theme multi-font**: añadir `_titleFont` + `_bodyFont` + `_labelFont` rompe ThemedText backing-field rule. Migration sweep ~40 callsites. Reco P0 pero capability-gate evaluation antes de codear.

**Bloque 2 (REPLAN) cerrado**. Continúa con Bloque 3 (UX audit) abajo.

---

