# Fase 0 — Línea base de What the Hell? 3D

Fecha de auditoría: 2026-08-21

Estado: completada como auditoría documental/estática. El playtest interactivo de Godot queda pendiente porque el ejecutable `godot`/`godot4` no está disponible en este entorno.

## 1. Fuentes revisadas

- `project.godot`, `export_presets.cfg` y `README.md`.
- Escenas activas en `scenes/menu/`, `scenes/cinematics/`, `scenes/levels/`, `scenes/player/`, `scenes/enemies/` y `scenes/ui/`.
- `scripts/game/game_state.gd`, `campaign_level.gd`, `level_config.gd`, `player_controller.gd`, `enemy_base.gd`, `adventure_camera.gd`, `main_menu.gd`, `intro_story_3d.gd`, `hud.gd` y `pause_menu.gd`.
- `resources/levels/level_01.tres`, `level_02.tres` y `level_03.tres`.
- `ASSET_SOURCES.md` y `THIRD_PARTY_NOTICES.md`, además de los archivos de licencia existentes.
- `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` y `Assets/InputSystem_Actions.inputactions` del proyecto Unity.

## 2. Flujo activo de referencia

La campaña que debe migrarse es:

```text
Main menu
  ├─ Nueva partida → Intro cinematográfica → Nivel 1
  ├─ Continuar → escena/checkpoint guardado
  ├─ Selección de capítulo → Nivel 1, 2 o 3
  └─ Créditos / salir

Nivel 1 → Nivel 2 → Nivel 3 → Victoria
```

Escenas que forman parte del build activo de Godot:

- `scenes/menu/main_menu.tscn`
- `scenes/cinematics/intro/intro_story_3d.tscn`
- `scenes/levels/campaign_level_01.tscn`
- `scenes/levels/campaign_level_02.tscn`
- `scenes/levels/campaign_level_03.tscn`
- `scenes/levels/victory.tscn`

Fuera del flujo inicial:

- Tres escenas en `scenes/legacy_levels/`.
- `scenes/levels/prototype_25d.tscn` y `scenes/player/player_25d.tscn`.
- `scenes/tests/LevelMetricsTest.tscn`, que queda como referencia de validación.

## 3. Controles y acciones de referencia

| Acción | Binding por defecto | Observación |
|---|---|---|
| `move_forward` | W | Movimiento relativo a la cámara |
| `move_backward` | S | Movimiento relativo a la cámara |
| `move_left` | A | Movimiento relativo a la cámara |
| `move_right` | D | Movimiento relativo a la cámara |
| `jump` | Espacio | Salto doble, coyote time y jump buffer |
| `sprint` | Shift | Aumenta la velocidad y puede activar sprint-jump roll |
| `attack` | Click izquierdo | Ataque de espada y cola de combo |
| `guard` | Click derecho | Guardia; también inicia ventana de parry |
| `dodge` | Q | Esquiva con consumo de stamina |
| `lock_on` | Click central | Activa/desactiva objetivo cercano |
| `pause` | Escape | Pausa o navegación atrás en menús |
| `interact` | E | Se usa principalmente en contenido legacy; no es requisito de la campaña activa según README |

El action asset de Unity actualmente contiene acciones de plantilla como `Move`, `Look`, `Attack`, `Interact`, `Jump` y `Sprint`, pero todavía debe alinearse con los nombres y bindings de Godot, especialmente `Guard`, `Dodge`, `LockOn` y `Pause`.

## 4. Parámetros jugables que deben conservarse

### Jugador

- Salud máxima: 100.
- Velocidad normal: 5.5; sprint: 8.0.
- Salto: 7.4; segundo salto: 6.9; máximo de saltos: 2.
- Coyote time: 0.12 s; jump buffer: 0.14 s.
- Daño de espada: 35; alcance: 2.8; cooldown: 0.26 s.
- Reinicio de combo: 0.8 s.
- Dodge: velocidad 9.8, duración 0.34 s, cooldown 0.48 s.
- Stamina máxima: 100; reducción de daño en guardia: 78 %.
- Ventana de parry: 0.18 s; alcance inicial de lock-on: 18.

Estos valores son la línea base del controlador principal, no una garantía de que todas las variantes legacy usen el mismo comportamiento.

### Enemigo base

- Salud máxima: 100.
- Velocidad de patrulla: 2.8; persecución: 4.0.
- Distancia de patrulla: 5; detección: 14; pérdida de interés: 20.
- Daño: 12; distancia de ataque: 1.9.
- Cooldown de ataque: 1.15 s; wind-up: 0.28 s; recuperación: 0.24 s.
- Leash: 18; postura máxima: 100.

La bruja utiliza el comportamiento de proyectil cuando su variante de escena lo configura; los valores finales por variante deben comprobarse durante la migración de prefabs.

### Cámara

- Distancia inicial: 8.5; altura: 3.2; shoulder offset: 0.65.
- Distancia máxima: 18; seguimiento: 10; sensibilidad de mouse: 0.003.
- Look-ahead: 1.2; FOV dinámico: 4; shake por impacto: 0.08.
- Distancia/altura de combate: 5.4 / 2.35.
- Lock-on se rompe a 20 unidades.

## 5. Reglas de progresión y guardado

- `GameState` es el estado global persistente.
- Nueva partida limpia llave, checkpoints, coleccionables y estado de nivel, guarda y abre la intro.
- Continuar carga `user://save_game.cfg`; sin guardado, inicia una partida nueva.
- Seleccionar nivel reinicia el progreso específico del nivel seleccionado.
- Un nivel de campaña solo puede finalizarse cuando `key_collected` es verdadero.
- Cada nivel tiene dos checkpoints configurados; solo avanza el checkpoint si su índice es mayor al actual.
- Al finalizar, el destino es el siguiente nivel; el último va a `victory.tscn`.
- El guardado conserva escena/checkpoint, nivel actual, siguiente escena, coleccionables, llave, posición de respawn y `level_finished`.
- La compatibilidad con el archivo de guardado de Godot no es requisito de la primera entrega; Unity debe definir un formato propio versionado.

## 6. Inventario de niveles

| Nivel | Tema / objetivo | Inicio → meta | Contenido crítico |
|---|---|---|---|
| 1 | Bosque — llave y bandera | `(-28, 2.2, 0)` → `(90, 4.2, 0)`; corredor 18 | 1 plataforma móvil, 2 plataformas que caen, 2 zonas de spikes, 2 checkpoints, 4 enemigos: 2 goblins y 2 zombies |
| 2 | Minas — 2 checkpoints, llave y salida | `(-29, 2.2, 0)` → `(111, 6.7, 0)`; corredor 16 | Lava, 2 plataformas móviles, 2 que caen, escaleras, spikes, 2 checkpoints, 4 enemigos: 2 goblins, 1 zombie y 1 bruja |
| 3 | Castillo — guardianes, llave y salida | `(-30, 2.2, 0)` → `(119, 9.3, 0)`; corredor 15 | 2 plataformas móviles, 2 que caen, 2 tramos de escaleras, spikes y sierra, 2 checkpoints, 6 enemigos: 3 zombies, 1 goblin y 2 brujas |

Todos los niveles tienen llave y corazón configurados, líneas/arcos de monedas y una caché bonus; sus layout, decoración y luces se generan/conectan desde `campaign_level.gd` y los datos de `LevelConfig`.

## 7. Assets, procedencia y licencias

- Se reutilizarán exclusivamente los assets ya presentes en el repositorio Godot, salvo autorización posterior.
- Modelos principales: caballero, goblins, zombies, bruja, edificios medievales, naturaleza, rocas, plataformas y peligros.
- Animaciones del caballero marcadas como suministradas por el usuario desde Mixamo: muerte, ataques de espada y sprint-jump roll.
- Audio existente: música, ambiente, efectos y diálogos de la intro.
- Formatos detectados: GLTF/GLB, FBX, OBJ, PNG/JPG, MP3, TTF y shader Godot.
- Licencias/notices encontrados: `assets/licenses/`, `THIRD_PARTY_NOTICES.md` y `scenes/SBS - Planet Surface Backgrounds 2 - 512x384/License.txt`.
- Antes de publicar o vender, se debe verificar que cada pack permita la distribución prevista.
- No se deben incluir `.godot`, builds, caches, fuentes de paquetes de terceros no autorizadas ni archivos `.blend`/`.unitypackage` innecesarios.

## 8. Matriz de paridad inicial

| ID | Sistema | Evidencia Godot | Objetivo Unity | Estado al cierre de Fase 0 |
|---|---|---|---|---|
| F0-01 | Arranque | `project.godot` apunta al menú | Menú como escena inicial de Unity | Línea base capturada |
| F0-02 | Flujo | Export preset contiene 6 escenas activas | Mismo flujo de campaña | Línea base capturada |
| F0-03 | Input | 11 acciones documentadas | Input System con equivalencia de bindings | Pendiente de implementación |
| F0-04 | Jugador | Parámetros y mecánicas documentados | Prefab con paridad de movimiento/combate | Pendiente de implementación |
| F0-05 | Cámara | Seguimiento, combate, zoom y lock-on documentados | Cámara equivalente | Pendiente de implementación |
| F0-06 | Enemigos | 3 familias y comportamiento base documentados | Prefabs, IA y navegación equivalentes | Pendiente de implementación |
| F0-07 | Progresión | Llave, checkpoints, escenas y guardado documentados | Servicio persistente y save versionado | Pendiente de implementación |
| F0-08 | Niveles | Layout y objetivos de los 3 `LevelConfig` documentados | Datos Unity y builders equivalentes | Pendiente de implementación |
| F0-09 | Presentación | Menú, intro, HUD, pausa, audio y victoria identificados | UGUI/Timeline/AudioMixer equivalentes | Pendiente de implementación |
| F0-10 | Alcance | Activo, legacy y 2.5D separados | Solo campaña activa en primera entrega | Decisión confirmada |
| F0-11 | Licencias | Fuentes y notices localizados | Migración sin assets externos no aprobados | Decisión confirmada |

## 9. Pruebas manuales pendientes cuando haya runtime Godot

Estas pruebas no se declaran ejecutadas por falta del ejecutable local:

1. Abrir el menú y verificar música, botones, capítulos, créditos y salida.
2. Iniciar nueva partida, omitir/no omitir intro y llegar al nivel 1.
3. Verificar movimiento, salto doble, sprint, ataque, guardia, parry, dodge y lock-on.
4. Recibir daño, morir y reaparecer en el checkpoint correcto.
5. Obtener la llave, intentar salir sin llave y completar el nivel con llave.
6. Completar la transición nivel 1 → 2 → 3 → victoria.
7. Guardar, cerrar/reabrir y continuar desde la escena/checkpoint esperado.
8. Confirmar que legacy y prototipo no se incorporan accidentalmente al flujo activo.

## 10. Resultado de la fase cero

- Línea base documental creada.
- Controles, parámetros críticos, progresión, niveles y flujo activo registrados.
- Matriz de paridad preparada para las siguientes fases.
- Alcance de primera entrega confirmado: campaña 3D activa.
- Procedencia y licencias de assets identificadas.
- Limitación registrada: falta playtest interactivo del proyecto Godot en este entorno.
