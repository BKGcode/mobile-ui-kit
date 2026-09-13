---
name: kitforgelabs_phase_ux_review
description: KF MobileUIKit current development phase — UX review for game designer hype-casual workflow. Establishes the workshop-drawer test as acceptance gate. Read at any KF MobileUIKit session start.
type: project
---

# KF MobileUIKit — Fase de revisión UX (LOCKED 2026-05-09)

**Why:** Phase context governs every M5/RC decision. Mid-M5/RC sessions inherit work from prior sessions (audit, refactors, premise specs); without the phase context loaded, recommendations drift toward technical completeness or evaluator-facing polish, both of which are out of scope until the workshop-drawer is organized.

**How to apply:** at the start of any KF MobileUIKit session, the test below is the acceptance gate for any proposal. Apply it BEFORE technical evaluation, not after. If a finding/feature/refactor fails the workshop-drawer test, classify it `defer post-phase` regardless of how technically sound it is. The phase ends when the user explicitly opens the next scope (Asset Store / buyer-facing / marketing).

**Sibling memory:** [`feedback_audit_priority_tool_first.md`](./feedback_audit_priority_tool_first.md) — the workshop-drawer test rule itself (operative test). This file is the broader phase context; the test file is the filter.

---

## Contexto de la fase

El kit está técnicamente resuelto. Los 17 popups, los 3 HUDs, los 5 builders, el contrato de theme y la pipeline de audit funcionan: los tests pasan, la arquitectura aguanta y los contratos están definidos. Pero al abrirlo por primera vez, el usuario no se encuentra con una herramienta — se encuentra con un cajón de viejo taller lleno de tornillos, llaves inglesas y reglas. Hay valor dentro, pero no sabe por dónde empezar a sacarlo.

Esta fase no añade features. Reorganiza lo que ya existe para que sea útil.

## En quién pensar

El protagonista es un game designer junior-mid de un studio hype-casual que tiene que sacar un prototipo jugable cada semana. No es programador. Conoce GameObject, Component y drag-drop. No conoce inyección de dependencias, ni asmdefs, ni el patrón interface-vs-implementación. Su lunes 9am llega con un brief y su viernes 11am es demo. Dispone de unas tres horas el lunes para montar la UI base; si tarda más, entra a martes con UI inacabada y pierde un día de gameplay.

Cuando este usuario abre el kit, no quiere leer veinticuatro specs ni implementar siete métodos de un IEconomyService para enseñar el contador de monedas en pantalla. Quiere arrastrar un prefab a la escena, ver el HUD funcionar, y empezar a programar el loop de juego.

## El cambio de mentalidad

Hasta ahora el trabajo se ha medido en términos técnicos: ¿pasa los tests, está el contrato bien definido, escala la arquitectura? Esas preguntas siguen siendo válidas pero ya no son las preguntas centrales. La pregunta central de esta fase es otra: ¿el usuario consigue resolver lo que vino a hacer sin esfuerzo? Si el contrato es elegante pero el usuario no encuentra dónde tirar del hilo, el contrato pierde valor en el flujo real. Si los tests pasan pero el setup le come treinta minutos del lunes, el kit no sirve para su workflow, por mucho que sea correcto en abstracto.

La fase se mide, por tanto, en minutos de fricción eliminados y en clicks que dejan de ser necesarios, no en líneas de código ni en cobertura de tests añadida.

## Dónde poner el foco

El foco está en la experiencia del usuario que ya tiene el kit instalado y quiere usarlo en su trabajo diario. No en el evaluador externo que decide si comprarlo, no en el Asset Store, no en las screenshots de marketing, no en la documentación buyer-facing que alguien lee antes de instalar. Todo eso es importante pero llega después, en una fase distinta que abriremos cuando el cajón esté organizado.

Los principios que gobiernan las decisiones de esta fase son siete: una sola puerta de entrada (single front door), enseñar antes que explicar (show don't tell), exponer lo simple por defecto y lo avanzado bajo click (progressive disclosure), garantizar que cada error y cada estado vacío termina con un siguiente paso obvio (no dead-ends), reutilizar la sensación de Unity nativa (Unity-native, no widgets exóticos), hacer que el comportamiento por defecto coincida con lo que el usuario espera (predictable defaults), y conseguir que la herramienta recuerde dónde dejaste el trabajo entre sesiones (persistent context). Estos principios no son aspiraciones — son criterios de aceptación. Cualquier propuesta que viole alguno necesita justificación explícita o se descarta.

## El test del cajón de taller

Cualquier idea, finding, feature o cambio que aparezca durante esta fase pasa por una pregunta única antes de incorporarse al plan: ¿esto organiza el cajón del usuario que ya está dentro, o pinta el letrero de la tienda para atraer a alguien de fuera? Si organiza el cajón — si reduce clicks, elimina ambigüedad, facilita iteración, hace el lunes más corto, predispone a que el usuario vuelva el martes sin re-aprender — entra. Si pinta el letrero — si vende mejor el kit a alguien que aún no lo ha instalado, si mejora la primera impresión de un evaluador, si optimiza la conversion de la store — sale, y se queda fuera hasta que abramos explícitamente esa otra fase.

Este test no es opcional ni negociable. Es el filtro de prioridad que distingue trabajo de esta fase de trabajo de la siguiente. El kit ya tiene contenido suficiente; lo que necesita es que ese contenido sea encontrable, predecible y reutilizable en el workflow real del game designer al que va dirigido.

---

## Phase exit criteria

The phase ends when ALL of these are true:

1. The 7 P0 clusters R-U-C1 to R-U-C7 from M3c Bloque 2 audit are landed (KitforgeRoot prefab + DTO Inspector-friendly + HUDSimple + Popup Test Launcher + Theme polish + Validation pre-Play + SafeAreaFitter).
2. The 6 P0 Hub features F-UX-1 to F-UX-6 from M3c Bloque 3 are landed (Hub window + Setup wizard + Catalog Browser + Theme Studio + Test Launcher + Communication actionable).
3. The user explicitly says "now we open the next scope" (Asset Store / buyer-facing / marketing).

Until all three: workshop-drawer test stays as the active filter. Asset Store findings, buyer journey simulations, hero shots, store-page text, marketing pitch optimization → automatically `defer post-phase`.

## Sibling artifacts

- M3c audit findings: `kitforgelabs_ux_audit_2026-05.md` (130 findings, 7 Pillars, 7 Hub features, 5 Premises Runtime, 4 Decisions LOCKED)
- M5/RC plan: `kitforgelabs_mobile_ui_kit_roadmap.md` § Path to v1.0.0-rc + § M3c summary (sub-divided M5.1-M5.7)
- Workshop-drawer test rule: `feedback_audit_priority_tool_first.md`
- Single-persona audit methodology: `feedback_single_persona_audit.md`
