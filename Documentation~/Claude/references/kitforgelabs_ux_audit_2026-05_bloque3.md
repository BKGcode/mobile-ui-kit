---
name: kitforgelabs_ux_audit_2026-05_bloque3
description: M3c UX audit — Bloque 3 (as ux specialist · tool design from scratch · 50 casos · Hub design) + Decisiones críticas LOCKED 2026-05-09. Split from kitforgelabs_ux_audit_2026-05.md (monolith >800). Index + Methodology + Buyer profile + Bloque 2 live in the index file; Bloque 4 in kitforgelabs_ux_audit_2026-05_bloque4.md.
type: project
status: active
---

## Bloque 3 (2026-05-09) — UX audit: tool design from scratch + 50 casos

### Persona LOCKED — "el especialista UX que diseña la puerta de entrada"

**Quién**: UX specialist senior. Define flows, pillars, interacciones. **No** auditor técnico (eso es `as dev`). Su pregunta de oro: *"¿qué siente el usuario cuando abre la tool por primera vez? ¿qué siente al volver al día siguiente?"*

**Patrón operativo**: lee los 50 findings del Bloque 2 (user game designer hype-casual) + las dudas del user real + diseña respuestas en formato `"si el usuario necesita X, la tool debe poder Y"`. NO discute viabilidad técnica — eso lo aterriza `as dev` después.

**Posición clave**: la tool propia es la puerta de entrada del kit. Si la puerta es hostil, ningún feature dentro importa. El UX specialist diseña **una sola ventana** que sea simple y elegante; el resto del kit se accede a través de ella.

---

### UX Pillars (LOCKED — 7 principios rectores)

Estos pilares gobiernan toda decisión UX del package y la Hub. Conflictos se resuelven a favor del pilar de menor número.

| # | Pilar | Significa | Test rápido |
|---|---|---|---|
| **P1** | **Single front door** | UNA ventana entrada (Kitforge Hub). 9 menu items dispersos = anti-pilar. | "¿Puedo abrir una sola ventana y hacer todo lo del lunes?" |
| **P2** | **Show, don't tell** | Visual previews > texto + specs. Thumbnails + live preview > 24 .md files. | "¿Tengo que LEER para entender, o puedo VER?" |
| **P3** | **Progressive disclosure** | Game designer ve simple por defecto. Programmer expande con click. Audit/Headless oculto en Developer. | "¿El no-coder se asusta al primer abrir?" |
| **P4** | **No dead-ends** | Cada error / estado vacío / dialog termina con next-step button. Nunca "ahora qué". | "¿Hay siempre un siguiente paso obvio?" |
| **P5** | **Unity-native** | Inspector + drag-drop + ContextMenu + IMGUI/UIToolkit standard. NO custom widgets exotic. | "¿Se siente como Unity?" |
| **P6** | **Predictable defaults** | Comportamiento por defecto = lo que un game designer espera. Ej: 2x GameOverPopup → reemplazar (no queue). Theme drop → 3 managers wired. | "¿Lo obvio funciona sin configurar?" |
| **P7** | **Persistent context** | La Hub recuerda donde dejaste. Setup wizard "2 of 3 done". No re-onboarding cada session. | "¿Vuelvo el martes y sigo donde estaba?" |

**Anti-pilares explícitos** (cosas que NO hacemos):
- ❌ Custom Editor windows con look-and-feel propio (rompe P5).
- ❌ Wizards multi-step modal con next/prev/cancel (rompe P3 + P7).
- ❌ Tutorials interactivos que apuntan flecha al botón (rompe P5 + es paternalista).
- ❌ Onboarding video / animated intro al primer abrir (rompe P3 — game designer quiere productividad inmediata).
- ❌ Notifications "Did you know..." in-Editor (ruido).

---

### Kitforge Hub — diseño from scratch (the front door)

**Single Editor window**. Nombre: `Kitforge Hub`. Acceso: `Tools → Kitforge → UI Kit → Hub` + shortcut `Ctrl+Shift+K`. Dockable. Auto-opens al primer importar el package (opt-out checkbox).

#### Layout

```
┌──── Kitforge Hub ─────────────────────────────────────── ⊙ Settings ── ⊗ ─┐
│  Kitforge UI Kit · v1.0.0-rc · ● Healthy (23/23 audit pass · 1 click→fix)  │
├──────────┬──────────────────────────────────────────────────────────────────┤
│  🏗️ Setup │                                                                  │
│  📚 Catalog                  [main tab content area]                         │
│  🎨 Theme                                                                    │
│  🧪 Test                                                                     │
│  ❓ Help                                                                     │
└──────────┴──────────────────────────────────────────────────────────────────┘
```

- **Header**: kit name + version + health badge (green/yellow/red pill, 1-click → audit window if not green).
- **Left sidebar** (60px wide): 5 vertical tab icons. Tooltip on hover.
- **Main area**: tab content. Min size 600×500.
- **Footer hidden** (no constant bar — no wasted real estate).

#### Tab 1: Setup (default tab on first open)

3-step welcome cards. Each card: status icon + title + 1-line description + primary action button. Persistent state (P7).

```
┌──────────────────────────────────────────────────────┐
│ ✓  1. Initialize Project                              │
│    10 anim presets · Theme · KitforgeRoot prefab      │
│    [ Re-initialize ]  ← safe idempotent              │
├──────────────────────────────────────────────────────┤
│ ◐  2. Add Scene Root                                  │
│    Drops KitforgeRoot in current scene.               │
│    [ Add to current scene ]                           │
├──────────────────────────────────────────────────────┤
│ ○  3. Hello World                                     │
│    Press Play and trigger a demo popup.               │
│    [ Run Hello World ]  (requires Play mode)          │
└──────────────────────────────────────────────────────┘

Progress: [██████░░░░] 2 of 3 complete

─────────────────────────────────────────────────────────
Advanced:  [ Build Everything ]  [ Open Audit ]
```

- ✓ done · ◐ in-progress · ○ todo. Visible status (P7).
- "Re-initialize" idempotent (no rompe nada). "Add to current scene" detects KitforgeRoot ya presente → "Already added (ping)" instead of duplicate (P6).
- "Hello World" botón disabled si no Play mode → tooltip "Press Play first".
- "Build Everything" para advanced users que ya saben qué quieren.

#### Tab 2: Catalog (visual prefab browser)

Grid responsive de cards. Cada card = thumbnail + nombre + 1-line tagline.

```
┌────── Search [____________]  Category [All ▼]  Filter [No services ☐] ─────┐
│                                                                              │
│  ┌────────┐  ┌────────┐  ┌────────┐  ┌────────┐                              │
│  │ [img]  │  │ [img]  │  │ [img]  │  │ [img]  │                              │
│  │Confirm │  │ Pause  │  │Tutorial│  │ Toast  │   ← Pure UI                  │
│  │Yes/No  │  │Resume… │  │N steps │  │ Auto…  │                              │
│  └────────┘  └────────┘  └────────┘  └────────┘                              │
│                                                                              │
│  ┌────────┐  ┌────────┐  ┌────────┐  ┌────────┐                              │
│  │Reward  │  │ Shop   │  │NotEnough  │HUDCoins│   ← Currency                 │
│  │+items… │  │Buy IAP │  │watch ad│  │show int│                              │
│  └────────┘  └────────┘  └────────┘  └────────┘                              │
│  ...                                                                         │
└──────────────────────────────────────────────────────────────────────────────┘
```

Click un card → side panel desliza:

```
┌────── RewardPopup ───────────────────────────────┐
│  ┌─────────────────────┐                          │
│  │ ░░░ FULL PREVIEW ░░░│  DTO fields:              │
│  │   "You won!"        │  • Items: RewardItem[]    │
│  │   [coin x100]       │  • OnDismissed: Action    │
│  │   [Tap to claim]    │                           │
│  └─────────────────────┘                          │
│                                                    │
│  Snippet (copy):                                   │
│   popupManager.Show<RewardPopup>(new RewardPopup… │
│   [📋 Copy]                                        │
│                                                    │
│  Service requirements:                             │
│   • IEconomyService (auto-credit on claim)         │
│   ⚠ Or use [HUDSimple] for service-free workflow.  │
│                                                    │
│  [+ Add to current scene]   [Read full spec]       │
└────────────────────────────────────────────────────┘
```

- Search filtra en tiempo real (P2).
- "Filter: No services" oculta popups con dependencies de service → game designer ve solo lo plug&play.
- Add to scene: instancia prefab + register en PopupManager + ping (P6 default behavior).
- "Read full spec" abre `Documentation~/Specs/Catalog/<X>.md` en IDE externo.
- Custom popups del user (`UIModule<TData>` derived) aparecen automáticamente en grid (P7 reactividad).

#### Tab 3: Theme Studio

Live preview + slot picker categorizado.

```
┌────── Theme: [Theme_Default ▼]  [+ New Theme]  [📸 Capture All] ─────────┐
│                                                                            │
│  ┌─── Live preview ────────────────────────────┐                           │
│  │                                              │                           │
│  │       ╔════════════════════════════╗         │                           │
│  │       ║        Are you sure?       ║ ←Title  │                           │
│  │       ║  Quit the current level?   ║ ←Body   │                           │
│  │       ║   ┌────┐    ┌────┐          ║         │                           │
│  │       ║   │ No │    │Yes │          ║ ←Button │                           │
│  │       ║   └────┘    └────┘          ║         │                           │
│  │       ╚════════════════════════════╝         │                           │
│  │                                              │                           │
│  └──────────────────────────────────────────────┘                           │
│                                                                            │
│  ▼ Brand colors                                                            │
│    PrimaryColor      [████████]  ↳ main brand (used in primary buttons)    │
│    SecondaryColor    [████████]  ↳ secondary actions, less prominent        │
│    AccentColor       [████████]  ↳ highlights, badges, NEW BEST            │
│  ▶ Backgrounds (3)                                                         │
│  ▶ Text (3 + 3 fonts)                                                      │
│  ▶ Status colors (3)                                                       │
│  ▶ Sprites (8)                                                             │
│  ▶ Audio + animation (2)                                                   │
└────────────────────────────────────────────────────────────────────────────┘
```

- Live preview re-renders al cambiar slot (no Play mode required).
- Categorías colapsables. Default expandido = Brand colors (P3).
- Tooltips por slot (hover) explican qué usa el slot.
- "Capture All" → 17 thumbnails snapshots → carpeta abierta + Catalog tab thumbnails update.
- "+ New Theme" duplica theme actual + abre rename (workflow re-skin per proto).

#### Tab 4: Test (Popup Test Launcher)

```
┌─── Test Launcher ─ ⏵ Play mode required ─────────────────────────────┐
│                                                                          │
│  ▼ Pure UI                                                               │
│    ConfirmPopup       [DTO mock fields ▼]  [▶ Show]                       │
│    PausePopup         [DTO mock fields ▼]  [▶ Show]                       │
│    TutorialPopup      [DTO mock fields ▼]  [▶ Show]                       │
│    NotificationToast  [DTO mock fields ▼]  [▶ Show]                       │
│                                                                          │
│  ▶ Currency (5 popups)                                                   │
│  ▶ Progression (5 popups)                                                │
│  ▶ Player Data (1 popup)                                                 │
│  ▶ Screens (2 screens)                                                   │
│                                                                          │
│  ─── Force scenarios ──────────────────────────                          │
│  ☐ Day N daily login [ 5 ]    [Apply]                                    │
│  ☐ Coins = [ 0 ]              [Apply]                                    │
│  ☐ Skip [ 1 ] hour            [Apply]                                    │
│  ☐ Energy = full              [Apply]                                    │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

- Play mode required check (P4 — si no Play, big button "Press Play to test"; tab not greyed silently).
- DTO mock fields = collapsible per popup, auto-fills sensible defaults.
- Force scenarios = debug helpers visibles, mismo patrón que Inspector ContextMenu pero centralizado en Hub.

#### Tab 5: Help

```
┌─── Help ────────────────────────────────────────────────┐
│  📋 Cheat-sheet  · 17 popups · 1-line snippets  [Open]   │
│  📦 Samples     · 8 samples available      [Manage…]    │
│  ❓ FAQ          · Common gotchas          [Read]        │
│  📜 Changelog    · v1.0.0-rc latest        [Open]        │
│  🌐 GitHub       · github.com/BKGcode/...   [↗]          │
│  ── About ───────                                        │
│  Kitforge UI Kit v1.0.0-rc · Unity 6000.1+               │
│  Built by Kitforge Labs                                  │
└──────────────────────────────────────────────────────────┘
```

- Cheat-sheet abre embedded markdown viewer (no IDE switch).
- Samples list: estado importado/no + "Import" / "Re-import" botón.
- FAQ resume gotchas comunes.

#### Settings (gear icon top-right)

```
┌─── Settings ───────────────────────────────┐
│  ☑ Auto-open Kitforge Hub on import        │
│  ☐ Auto-open Hub on Editor launch          │
│  ☐ Show advanced (Build Individual Group)   │
│  ☐ Show developer tools (Audit Headless)   │
│  Hub width: [Auto ▼]                        │
└─────────────────────────────────────────────┘
```

---

### 50 findings as UX (formato "si user X → tool Y")

#### Sección A — Pilares + filosofía (10)

**[UX-A.1]** **Falta North Star explícito**
*User need*: entender en 10s qué hace el kit + qué problema resuelve.
*Tool must*: Hub header muestra 1-line tagline ("Mid-core mobile UI in 5 minutes. 17 popups + Theme + flow.") debajo del logo. **[P1, P2]** → **P0**

**[UX-A.2]** **Dispersión: 9 menus + Audit window + UIThemeConfigEditor + samples — sin "single front door"**
*User need*: una sola entrada que organice todo.
*Tool must*: Kitforge Hub = ÚNICO entry point. 9 menu items legacy quedan deprecated o minimizados a Developer submenu. **[P1]** → **P0**

**[UX-A.3]** **Sin onboarding visual al primer importar**
*User need*: saber qué hacer en 30s sin leer.
*Tool must*: Hub auto-opens al primer importar (detect via PackageManager event) y muestra Setup tab con el wizard 3-step. **[P1, P3]** → **P0**

**[UX-A.4]** **Inconsistencia naming: "Bootstrap Defaults" (técnico) vs "Build Group A Sample" (descriptivo) vs "Audit (Run Headless)" (cripto)**
*User need*: nombres que comunican qué hacen al non-coder.
*Tool must*: rename: `Bootstrap Defaults → Initialize Project`, `Build Group X → Build [Catalog Group]`, `Audit (Run Headless) → CI Audit (no UI)`, oculto en Developer. **[P5]** → **P0** (rename = barato, alto impacto).

**[UX-A.5]** **Sin search bar / quick action global en Hub**
*User need*: encontrar "Reward" sin clicks.
*Tool must*: Hub header tiene search bar que filtra en tiempo real popups + theme slots + cheat-sheet entries (single search, multi-target). **[P2]** → **P1** (P0 si scope permite).

**[UX-A.6]** **Sin shortcut keys**
*User need*: abrir Hub sin clicks (Ctrl+Shift+K), abrir Test Launcher sin tabs (Ctrl+Shift+T).
*Tool must*: registrar shortcuts vía `[MenuItem]` priority + Unity 6 ShortcutManager API. Shortcuts visibles en menu entry (`Tools → Kitforge → Hub %#k`). **[P5]** → **P1**

**[UX-A.7]** **Sin "What's new in v0.X" tour cuando user updates kit**
*User need*: entender qué cambió post-update sin leer CHANGELOG.
*Tool must*: Hub detecta version diff vs last-shown → muestra modal "What's new" con 3-5 bullet points + link CHANGELOG. Solo primer abrir post-update. **[P7]** → **P2** (post-RC, value low pre-1.0).

**[UX-A.8]** **Visual style del editor tooling no consistente**
*User need*: tooling que se sienta como una sola pieza.
*Tool must*: USS / IMGUI styles compartidos entre Hub tabs + Audit window + UIThemeConfigEditor. Common header bar pattern, common card pattern, common color palette para badges (green/yellow/red). **[P5]** → **P1**

**[UX-A.9]** **Sin iconos** — wall of text/lists
*User need*: scan visual rápido.
*Tool must*: cada tab tiene icon (🏗️📚🎨🧪❓ vía Unity built-in icons o custom 16×16 pngs). Cada popup en Catalog grid tiene icono identificador (ConfirmPopup = ✓ / RewardPopup = 🎁 / GameOverPopup = ☠ / etc.). **[P2]** → **P1**

**[UX-A.10]** **No telemetry interna de qué features usa el user — para iterar UX post-RC**
*User need*: (kit dev internal) saber qué se usa y qué no para priorizar polish.
*Tool must*: opt-in anonymous telemetry (ON/OFF en Settings, default OFF) que registra qué tabs se abren + qué prefabs se draggean. **[P6]** → **P2** drop (privacy + scope creep, no relevante para v1.0.0-rc).

#### Sección B — Kitforge Hub diseño (10)

**[UX-B.1]** **Diseñar Single Editor Window "Kitforge Hub"**
*User need*: 1 ventana = todo el kit.
*Tool must*: `Editor/Hub/KitforgeHubWindow.cs` UIToolkit. 5 tabs verticales (Setup / Catalog / Theme / Test / Help). Min 600×500, dockable. **[P1]** → **P0** (es el deliverable central).

**[UX-B.2]** **Tabs verticales icon-only sidebar**
*User need*: navegación fría con icons + tooltips.
*Tool must*: sidebar 60px wide, icons 32×32 + tooltip on hover. Active tab = highlighted bg. Click → switch tab content sin re-create. **[P1, P5]** → **P0**

**[UX-B.3]** **Setup tab default on first open (P7 persistent state después)**
*User need*: la Hub auto-positions donde más útil.
*Tool must*: primer abrir = Setup tab. Subsequent abrir = recordar last tab abierto (EditorPrefs persist). Excepto si Setup wizard incompleto → fuerza Setup tab. **[P7]** → **P0**

**[UX-B.4]** **Catalog tab visual grid 17 popups + thumbnails + drag-to-scene**
*User need*: ver qué hay sin leer.
*Tool must*: grid responsive 4-col desktop. Cada card thumbnail (snapshot via PrefabSnapshotCapture) + name + 1-line tagline. Drag thumbnail al Hierarchy → instancia + register PopupManager + ping. **[P2]** → **P0**

**[UX-B.5]** **Theme tab live preview + categorías**
*User need*: cambiar PrimaryColor → ver impacto inmediato en mock popup.
*Tool must*: top-half = live preview render (mock ConfirmPopup con title/body/2 buttons). Bottom-half = slot picker categorizado. Cambio slot → rerender preview en frame. **[P2]** → **P0**

**[UX-B.6]** **Test tab Popup Test Launcher Play-mode-only**
*User need*: probar popups sin pestañar Play scene + Hierarchy + ContextMenu.
*Tool must*: list 17 popups categorizados + DTO mock-fill collapsible + Show button. + Force scenarios panel inferior. **[P1]** → **P0**

**[UX-B.7]** **Help tab — cheat-sheet + samples + FAQ + changelog + about**
*User need*: ayuda sin pestañar IDE / browser.
*Tool must*: embedded markdown viewer renderiza `Documentation~/CHEATSHEET.md`. Lista samples con import button. FAQ inline. Links externos (GitHub / CHANGELOG). **[P1, P2]** → **P0**

**[UX-B.8]** **Hub auto-opens primer importar (opt-out checkbox)**
*User need*: no buscar la tool tras importar package.
*Tool must*: hook `UnityEditor.PackageManager.Events.registeredPackages` detecta install. Abre Hub. Settings → "Auto-open on import" toggle ON por defecto. **[P3]** → **P0**

**[UX-B.9]** **Health badge en header (green / yellow / red)**
*User need*: saber si el kit está sano sin abrir audit.
*Tool must*: header pill: green "● Healthy (23/23)" | yellow "● 2 warnings (1-click→fix)" | red "● 3 fail · click to inspect". Click → Audit window abierto. **[P4]** → **P0**

**[UX-B.10]** **Hub minimizable como Tools menu icon (Unity 6 docking)**
*User need*: docking en layout permanente, no flotante.
*Tool must*: `EditorWindow.Show()` standard + dockable preset. User docka donde quiera (lateral, fondo). EditorPrefs persist layout. **[P5, P7]** → **P0**

#### Sección C — Setup flow wizard 3-step (10)

**[UX-C.1]** **Welcome wizard 3 cards: Initialize / Add Scene Root / Hello World**
*User need*: setup en 90 segundos sin pensar.
*Tool must*: 3 cards verticales con visual checkmark status (✓◐○) + título + 1-line desc + 1 primary button. Sin steppers, sin modals (P5 — Unity-native cards, no wizard widget). **[P3, P4]** → **P0**

**[UX-C.2]** **Visual status pending / in-progress / done**
*User need*: progreso visible.
*Tool must*: cards ordered top-to-bottom. Done → pale green tint + ✓. In-progress (button clicked, async) → amber + spinner. Todo → default. **[P2, P7]** → **P0**

**[UX-C.3]** **Initialize step: muestra qué se crea con preview thumbnails**
*User need*: saber qué assets aparecerán en Project window antes de click.
*Tool must*: card expandible "What's created?" → list "10 anim presets · 1 Theme asset · KitforgeRoot.prefab · Theme presets folder". + 1 thumbnail por asset categoría. **[P2, P4]** → **P1** (nice-to-have, no bloquea fix core).

**[UX-C.4]** **Add Scene Root: si no escena → ofrece create**
*User need*: no quedarse sin next step si abrió Hub sin escena activa.
*Tool must*: detect `EditorSceneManager.GetActiveScene()`. Si null o "Untitled" → button extra "Create new scene + add root". Si existe → "Add to current scene". **[P4, P6]** → **P0**

**[UX-C.5]** **Hello World step: requires Play mode**
*User need*: probar sin recordar "primero Play, luego trigger".
*Tool must*: button "Run Hello World" disabled (greyed) si no Play mode. Tooltip "Press Play first". Si Play → click → instancia ConfirmPopup demo + dialog Editor "Click 'Yes' on the popup → done". **[P4]** → **P0**

**[UX-C.6]** **Si user salta steps: progress 1/3 visible persistente**
*User need*: la Hub recuerda incompleto.
*Tool must*: progress bar `[██████░░░░] 2 of 3 complete`. EditorPrefs persistence. Hub label in Tools menu añade `(Setup incomplete)` si <3/3. **[P7]** → **P1**

**[UX-C.7]** **Si user vuelve después: Hub recuerda donde dejó**
*User need*: continuar sin re-onboarding.
*Tool must*: cubierto por C.6 (EditorPrefs persistence). + last-seen tab + last theme selected. **[P7]** → **P0** (parte de C.6).

**[UX-C.8]** **"Build Everything" button en Setup tab footer (advanced)**
*User need*: power user que ya conoce → atajo a "todo de una vez".
*Tool must*: footer secondary button "Build Everything". Orquesta Initialize + Build A→E + M4.1 + Add Scene Root atómicamente. Confirm dialog "This will overwrite Catalog_Group*_Demo/. Continue? [Y/N]". **[P3]** → **P0**

**[UX-C.9]** **Add Scene Root: si KitforgeRoot ya presente → ping en lugar de duplicate**
*User need*: idempotencia sin sorpresa.
*Tool must*: detect `FindObjectOfType<KitforgeThemeBinder>()`. Si existe → ping + dialog "Already added. Pinged in Hierarchy." En lugar de instanciar duplicate. **[P6]** → **P0**

**[UX-C.10]** **Initialize Project: idempotent (re-run safe)**
*User need*: poder hacer click sin miedo.
*Tool must*: cada AssetDatabase create check existence first. Si Theme.asset existe → log "already initialized, skipping" + ping. + dialog mantiene "All set" message. **[P6]** → **P0**

#### Sección D — Catalog Browser (10)

**[UX-D.1]** **Visual grid 17 popups + 3 HUD prefabs con thumbnails**
*User need*: ver qué hay.
*Tool must*: grid 4-col responsive. Thumbnail = snapshot SHA256 del prefab con Theme actual. Refresh thumbnails post-`Capture All`. **[P2]** → **P0**

**[UX-D.2]** **Click thumbnail → side panel preview + DTO + snippet**
*User need*: detalles sin abrir 3 ventanas.
*Tool must*: click selecciona card → side panel desliza desde derecha (animación 200ms). Panel: full preview big + DTO field list + cheat-sheet snippet con copy button + service requirements + add-to-scene button + read-spec link. **[P2, P3]** → **P0**

**[UX-D.3]** **Drag thumbnail al Hierarchy → instancia + register + ping**
*User need*: workflow Unity-native.
*Tool must*: implementar `DragAndDrop.PrepareStartDrag` con `objectReferences = new[] { prefab }`. On drop: instancia en root active scene + auto-register en PopupManager (si presente) + ping. **[P5, P6]** → **P0**

**[UX-D.4]** **Search bar filtra en tiempo real**
*User need*: encontrar "Reward" sin scroll.
*Tool must*: search bar top input filtra por: name (case-insensitive substring) + tags (Reward, Gameplay, Modal, etc.) + service requirements (HUDSimple no-services). **[P2]** → **P0**

**[UX-D.5]** **Categories sidebar / dropdown filter**
*User need*: ver solo "Pure UI" sin scroll.
*Tool must*: dropdown "Category" en header: All · Pure UI · Currency · Progression · Player Data · Screens · HUD. Selección filtra grid. **[P3]** → **P0**

**[UX-D.6]** **Filter "No service dependencies" toggle**
*User need*: game designer ve solo plug&play (HUDSimple, Confirm, Pause, Tutorial, Toast).
*Tool must*: checkbox header "Filter: plug&play only" → oculta popups con service requirements (Reward/Shop/GameOver/etc.). **[P3, P6]** → **P0**

**[UX-D.7]** **"Add to scene" + "Show in PopupManager" buttons en side panel**
*User need*: 2 acciones distintas: instanciar prefab vs registrar.
*Tool must*: side panel 2 buttons explicit. Add to scene = instancia. Show in PopupManager = registrar prefab en PopupManager registry sin instanciar (si user quiere disparar runtime sin tener prefab en escena). **[P4, P6]** → **P1**

**[UX-D.8]** **Spec link "Read full spec" abre IDE externo**
*User need*: buyer leer spec si quiere detalle.
*Tool must*: button "Read full spec" llama `EditorUtility.OpenWithDefaultApp("Documentation~/Specs/Catalog/X.md")`. **[P5]** → **P1** (opcional, cubierto por cheat-sheet primary).

**[UX-D.9]** **Cheat-sheet snippet copy-to-clipboard**
*User need*: copiar snippet sin re-tipear.
*Tool must*: button 📋 al lado del snippet → `EditorGUIUtility.systemCopyBuffer = snippet`. Tooltip "Copied!" 1s. **[P2, P5]** → **P0**

**[UX-D.10]** **Catalog Browser updates en tiempo real con custom popups del user**
*User need*: extender el catalog sin perder el browser.
*Tool must*: `TypeCache.GetTypesDerivedFrom<UIModuleBase>()` escan en Hub OnEnable. Custom subclasses con `[KitforgeCatalogEntry]` attribute appear con sus thumbnails (auto-generated o user-supplied vía attribute parameter). **[P5, P7]** → **P1** (extension API post-RC).

#### Sección E — Theme Studio + Test Launcher + Communication (10)

**[UX-E.1]** **Theme Studio live preview con mock popup**
*User need*: ver impacto del slot inmediato.
*Tool must*: Theme tab top-half = live render mock ConfirmPopup (Image card + 2 Texts + 2 Buttons). Re-renders al cambiar slot via `EditorApplication.update` + `Repaint()`. **[P2]** → **P0**

**[UX-E.2]** **Slot categorías colapsables: Brand / Backgrounds / Text / Status / Sprites / Audio**
*User need*: 16 colors agrupados.
*Tool must*: 6 foldout sections en Theme tab bottom-half. Default expanded = Brand colors (primer fix slot que toca el game designer). Otros foldouts collapsed. **[P3]** → **P0**

**[UX-E.3]** **Slot picker per-categoría con tooltip explicativo**
*User need*: saber qué hace cada slot sin leer spec.
*Tool must*: cada slot row: label + color picker / sprite picker / font picker + tooltip on hover ("PrimaryColor: main brand color, applied to primary action buttons + active accent borders. Default = #4A90E2."). **[P2]** → **P0**

**[UX-E.4]** **Preset switcher quick: Default / Casual / Premium**
*User need*: comparar 3 themes en 3 clicks.
*Tool must*: dropdown header Theme tab: `Theme: [Theme_Default ▼]`. Selección cambia preview instantáneo. + button "Save as preset" si user customizó actual. **[P2, P3]** → **P0**

**[UX-E.5]** **"Capture All Snapshots" button → 17 thumbnails generated + folder abierto**
*User need*: validar reskin global en 1 click.
*Tool must*: header button 📸 "Capture All". Llama `PrefabSnapshotCapture` sobre los 17 prefabs con Theme actual. Output `Library/UIKitAudit/Snapshots/<themeName>/`. + dialog "17 captured. Open folder?" → `EditorUtility.RevealInFinder`. **[P2]** → **P0**

**[UX-E.6]** **Popup Test Launcher como Test tab**
*User need*: testear sin Hierarchy + ContextMenu.
*Tool must*: cubierto por UX-B.6.
**[P1]** → **P0**

**[UX-E.7]** **Test Launcher DTO mock-fill UI panel**
*User need*: testear con datos diversos sin código.
*Tool must*: cada popup row collapsible → sub-panel con DTO fields editables (Inspector-like). Auto-fill defaults sensibles ("Continue with 3 stars / 10000 score / new best"). User edita → click Show → popup con esos datos. **[P3]** → **P0**

**[UX-E.8]** **Test Launcher "Force scenarios" panel — debug helpers visibles**
*User need*: ver edge cases (Day 7 daily, coins=0, energy full) sin code.
*Tool must*: panel inferior con checkboxes + values: ☐ Day N daily login [N], ☐ Coins=0, ☐ Skip 1 hour, ☐ Energy full, etc. Click Apply → llama ContextMenu helpers stubs. **[P2, P3]** → **P0**

**[UX-E.9]** **Communication: error messages actionable con next-step button**
*User need*: si algo falla → siguiente paso obvio.
*Tool must*: LogError format: `[Kitforge] <problem>. Click here to fix: <action>` con `[Action]` clickeable que abre Hub al tab/setting relevante. Si no clickeable (Console plain text): button suggestion claro. **[P4]** → **P0**

**[UX-E.10]** **Communication: missing wiring → toast in-Editor (no LogError)**
*User need*: no errors confusos al abrir scene sin wiring.
*Tool must*: si UIServices ref null en Awake (Editor mode) → in-Editor toast bottom-right "ShopPopup needs IEconomyService. [Wire it]". Click → Hub > Setup tab + ping UIServices. **[P4, P6]** → **P0**

---

### Convergencias UX → tool features

7 features-driven derivados de los 50 findings. Cada feature está al servicio de >5 findings.

| ID | Feature | Findings | Pillar | Priority |
|---|---|---|---|---|
| **F-UX-1** | **Kitforge Hub Editor window (5 tabs: Setup/Catalog/Theme/Test/Help)** — el deliverable central. Toda la UX colapsa aquí. | UX-A.1, A.2, A.3, B.1-B.10, all 50 implícitamente | P1 single front door | **P0** |
| **F-UX-2** | **Setup wizard 3-step persistente (Initialize / Add Scene Root / Hello World)** | C.1-C.10 | P3 progressive · P4 no dead-ends · P7 persistent | **P0** |
| **F-UX-3** | **Catalog Browser visual grid + drag-to-scene + side panel preview/snippet/DTO** | D.1-D.7, D.9 | P2 show don't tell | **P0** |
| **F-UX-4** | **Theme Studio live preview + categorías + presets switcher + capture all** | E.1-E.5 | P2 show don't tell · P3 progressive | **P0** |
| **F-UX-5** | **Popup Test Launcher Play-mode-only + DTO mock + force scenarios** | B.6, E.6-E.8 | P3 progressive · P4 no dead-ends | **P0** |
| **F-UX-6** | **Communication: actionable LogError + missing-wiring toasts + health badge header** | B.9, E.9, E.10 | P4 no dead-ends | **P0** |
| **F-UX-7** | **Polish post-RC: search global · shortcut keys · USS unified style · iconos · custom popup auto-discovery · "what's new" tour** | A.5, A.6, A.7, A.8, A.9, D.10 | P5 Unity-native · P2 show don't tell | **P1** |

### Disposition Bloque 3 — UX features

#### P0 — features que la Hub DEBE shippear en v1.0.0-rc

1. **F-UX-1 Kitforge Hub** — single Editor window, 5 tabs, dockable, auto-open on import. Es el deliverable central. Sin la Hub, no hay "single front door" → P1 violado → 50 findings re-emergen como hidden bugs UX.
2. **F-UX-2 Setup wizard 3-step persistente** — primer encuentro game designer.
3. **F-UX-3 Catalog Browser** — el corazón "show don't tell". Mata 24 specs como entry point (que pasa a P3 progressive: programmer expande "Read full spec").
4. **F-UX-4 Theme Studio** — Theme tab. Live preview + categorías. Mata 30 min/iteration.
5. **F-UX-5 Popup Test Launcher** — Test tab.
6. **F-UX-6 Communication actionable** — error messages + toasts + health badge. Sin esto, la Hub es bonita pero los errores siguen siendo Console-noise → P4 violado.

#### P1 — polish post-RC

7. **F-UX-7 Polish** — search global, shortcuts, iconos, USS unified, custom popup auto-discovery (`[KitforgeCatalogEntry]` attribute), "what's new" tour. No bloquea v1.0.0-rc; sube la elegancia +20%.

#### P2 — drop / out of scope

- UX-A.10 Telemetry (privacy + scope creep).
- UX-A.7 "What's new" tour (P1 si scope permite, P2 drop si no — value low pre-1.0).

---

### Métricas Bloque 3

- **50 findings as UX** generados.
- **7 UX Pillars** lockeados (P1-P7).
- **7 features-driven** identificadas (F-UX-1 a F-UX-7).
- **6 P0 features** (la Hub completa) + **1 P1 polish** + **2 P2 drops**.
- **Hub design wireframe** sketched (Setup / Catalog / Theme / Test / Help tabs + Settings).

### Mapeo Bloque 2 ↔ Bloque 3 (cómo las features UX cubren findings user)

| Cluster Bloque 2 | Cubierto por Hub feature |
|---|---|
| R-U-C1 Drop & play setup | F-UX-1 Hub + F-UX-2 Setup wizard |
| R-U-C2 Catalog discoverability + cheat-sheet + DTO Inspector-friendly | F-UX-3 Catalog Browser (cheat-sheet vive en Help tab + side panel snippet) |
| R-U-C3 HUDSimple sin servicios | (independent — código nuevo, no UX feature; Catalog filter "no services" lo expone) |
| R-U-C4 Popup Test Launcher | F-UX-5 Test tab |
| R-U-C5 Theme polish | F-UX-4 Theme Studio |
| R-U-C6 Validation pre-Play + actionable errors | F-UX-6 Communication |
| R-U-C7 SafeAreaFitter built-in | (independent — código nuevo, KitforgeRoot wraps Canvas) |

**Conclusión central**: 5 de 7 clusters Bloque 2 colapsan en features de la Hub. Los 2 restantes (HUDSimple + SafeAreaFitter) son código Runtime nuevo, no UX feature de la tool. La Hub es **el** deliverable UX de v1.0.0-rc.

### Decisiones críticas que requieren confirmación user

1. **F-UX-1 Hub scope**: ¿Hub se hace en v1.0.0-rc (P0) o se posterga a v1.1.0? Sin Hub, el resto de fixes son atómicos pero no resuelven "single front door" (P1 violado). **Recomendación: incluir en v1.0.0-rc** — la Hub define la identidad del kit.
2. **F-UX-1 implementación**: UIToolkit (UXML/USS, modern, performance + ergonomic) vs IMGUI (legacy, single-file). **Recomendación: UIToolkit** — alineado con Unity 6 dirección + escalable.
3. **F-UX-3 Catalog auto-discovery**: ¿solo los 17 popups del kit aparecen en Catalog Browser, o también custom popups del user via `[KitforgeCatalogEntry]` attribute? **Recomendación: 17 kit-only en v1.0.0-rc, attribute API en v1.1.0**.
4. **F-UX-6 in-Editor toasts**: hay precedente en Unity? Sí (`SceneView.AddOverlayToActiveView`, `EditorWindow.ShowNotification`). **Recomendación: usar `ShowNotification` API standard, no widget custom**.

**Bloque 3 cerrado**. Continúa con cierre 4 decisiones + Bloque 4 (as dev) abajo.

---

## Decisiones críticas LOCKED 2026-05-09 (post-Bloque 3)

| # | Decisión | LOCKED | Razón resumida |
|---|---|---|---|
| **D1** | F-UX-1 Hub scope: v1.0.0-rc vs v1.1.0 | **v1.0.0-rc** (split por sub-milestones M5.X internos) | Hub define identidad. Sin ella, 50 findings re-emergen como hidden UX bugs. |
| **D2** | F-UX-1 implementación: UIToolkit vs IMGUI | **UIToolkit** | Unity 6 dirección. Performance + USS theming + dockable nativo. Future-proof para live preview + drag-drop + tabs. |
| **D3** | F-UX-3 Catalog auto-discovery | **17 kit-only en v1.0.0-rc; `[KitforgeCatalogEntry]` API en v1.1.0** | Kit-only es atomic. Attribute API requiere thumbnail-API estable; diferir reduce scope sin perder value. |
| **D4** | F-UX-6 in-Editor toasts impl | **`EditorWindow.ShowNotification` API standard** | Unity-native (P5). Free. Custom widget = scope creep sin valor. |

---

