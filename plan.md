# Plan de migración: What the Hell? 3D

## 1. Propósito

Migrar el proyecto jugable de Godot 4.7 a Unity 6000.5.6f1 con URP, conservando el flujo activo, la jugabilidad, la progresión, los assets existentes y la intención visual del original.

Este documento define objetivos, fases, obstáculos y criterios de aceptación. La fase cero fue de planificación; las fases posteriores ya contienen una implementación base del runtime y escenas de bootstrap, pendiente de validación dentro de Unity Editor.

## 2. Alcance aprobado para la primera migración

### Flujo que debe quedar funcional

1. Menú principal.
2. Nueva partida.
3. Cinemática de introducción con subtítulos, voz, audio ambiente y opción de omitir.
4. Nivel 1: bosque.
5. Nivel 2: minas.
6. Nivel 3: castillo.
7. Victoria y retorno al menú.

### Sistemas incluidos

- Movimiento 3D del jugador: WASD, salto doble, coyote time, jump buffer, sprint y control aéreo.
- Combate: ataque de espada, combo, hitbox, daño, knockback, invulnerabilidad, feedback visual y sonoro.
- Acciones avanzadas: guardia, parry, dodge, stamina, postura y lock-on.
- Cámara de aventura: seguimiento, zoom, colisión, recentrado, look-ahead, FOV dinámico, cámara de combate y shake.
- Enemigos: goblins, zombies, bruja, patrulla, persecución, ataque telegrafiado, proyectil, hit stun, postura y muerte.
- Interactables: llave, checkpoints, portales, salida, pickups, corazones, monedas, peligros, plataformas móviles y plataformas que caen.
- HUD y pausa.
- Estado persistente de campaña, checkpoints y guardado local.
- Audio, música, ambiente, efectos, diálogos y fuentes existentes.
- Efectos visuales de ataques, impactos, pickups, checkpoints, portales y peligros.

### Elementos fuera de la primera entrega

- `scenes/legacy_levels/`: se conserva como referencia, pero no se migra inicialmente.
- `scenes/levels/prototype_25d.tscn` y sus scripts: se documentan, pero no forman parte del flujo 3D activo.
- Mejoras de diseño no presentes en Godot: se evaluarán después de alcanzar paridad funcional.
- Compatibilidad automática con archivos de guardado de Godot: queda como objetivo opcional posterior; el guardado nuevo de Unity sí debe funcionar.

## 3. Estado actual de los proyectos

### Godot, fuente de comportamiento

- Proyecto Godot 4.7.
- Escena de arranque: `scenes/menu/main_menu.tscn`.
- Estado global mediante `scripts/game/game_state.gd` como autoload.
- Niveles activos definidos mediante `resources/levels/level_01.tres`, `level_02.tres` y `level_03.tres`.
- Generación y conexión de gran parte del contenido desde `scripts/game/campaign_level.gd` (1665 líneas).
- Controlador de jugador principal en `scripts/player/player_controller.gd` (787 líneas).
- Menú principal en `scripts/menu/main_menu.gd` (629 líneas).
- Cinemática principal en `scripts/cinematics/intro_story_3d.gd`.
- Assets existentes en formatos GLTF/GLB, FBX, OBJ, texturas, MP3, fuente TTF y shader Godot.

### Unity, destino inicial

- Unity 6000.5.6f1.
- Proyecto URP todavía basado en la plantilla, con `Assets/Scenes/SampleScene.unity`.
- Paquetes ya disponibles y aprovechables: Input System, AI Navigation, Timeline, UGUI y URP.
- El action asset existente contiene acciones de plantilla; todavía hay que alinearlo con `guard`, `dodge`, `lock_on` y `pause` del proyecto Godot.

## 4. Mapeo de conceptos Godot → Unity

| Godot | Unity previsto | Objetivo |
|---|---|---|
| `Node` / `Node3D` | `GameObject` + `MonoBehaviour` | Separar composición, datos y comportamiento |
| `CharacterBody3D` | `CharacterController` o solución cinemática equivalente | Mantener el control preciso del jugador y enemigos |
| `Area3D` / `CollisionShape3D` | `Collider` trigger / `Collider` físico | Reproducir hitboxes, pickups y zonas de daño |
| `PackedScene` | Prefab | Crear unidades reutilizables |
| `.tscn` | Scene de Unity | Separar menú, cinemática, niveles y victoria |
| `Resource` / `.tres` | `ScriptableObject` | Modelar la configuración de cada nivel |
| `AnimationPlayer` / `AnimationTree` | `Animator`, clips y/o Timeline | Animación jugable y cinemáticas |
| Autoload `GameState` | Servicio persistente `DontDestroyOnLoad` | Progresión y cambios de escena |
| `ConfigFile` | JSON/binario versionado en `Application.persistentDataPath` | Guardado local robusto |
| `Tween` | Coroutine, AnimationCurve o DOTween solo si se aprueba | Transiciones, feedback y secuencias |
| `NavigationAgent3D`/patrulla propia | NavMeshSurface + NavMeshAgent o locomoción propia | IA de enemigos |
| `CanvasLayer` / `Control` | Canvas + UGUI | HUD, menú, pausa y subtítulos |
| `StandardMaterial3D` / `gdshader` | Materiales URP / Shader Graph / shader HLSL si hace falta | Adaptar el aspecto visual |

## 5. Fases y objetivos

### Fase 0 — Línea base y control del alcance — completada

- [x] Registrar el flujo jugable de Godot como referencia de comportamiento.
- [x] Documentar controles, velocidades, daños, cooldowns, checkpoints, objetivos y transiciones mediante auditoría estática.
- [x] Crear una matriz de paridad para comparar cada sistema entre Godot y Unity.
- [x] Separar claramente contenido activo, legacy y prototipo 2.5D.
- [x] Confirmar que no se incorporarán assets externos sin autorización y revisar las licencias existentes.
- [x] Registrar la ausencia del ejecutable Godot en el entorno; el playtest interactivo queda pendiente de ejecución manual.

**Salida:** completada con la línea base en [`phase_0_baseline.md`](phase_0_baseline.md). La matriz y las pruebas manuales pendientes están documentadas allí.

### Fase 1 — Preparación de Unity

- [x] Definir la estructura de carpetas de `Assets` para scripts, prefabs, escenas, datos, materiales, audio, UI, VFX y herramientas en `Assets/WhatTheHell3D/`.
- [x] Configurar Build Settings y la escena inicial del flujo activo con `MainMenu`, `Intro`, `CampaignLevel01–03` y `Victory`.
- [x] Configurar la base de URP, resolución objetivo 1280×720, tags, capas de gameplay y gravedad 3D compatible con la línea base.
- [~] Consolidar el uso del Input System: se añadieron `Guard`, `Dodge`, `LockOn` y `Pause` con sus bindings de Godot; las acciones sobrantes de la plantilla aún deben limpiarse y validarse en el editor.
- [x] Mantener el proyecto reproducible en Unity 6000.5.6f1 y conservar los paquetes base de URP, Input System, AI Navigation, Timeline y UGUI.

**Avance actual:** estructura, identidad/resolución, tags/layers, bindings y seis escenas de flujo creadas. La escena inicial de Build Settings ya es `MainMenu`; falta confirmar que Unity importe y compile las escenas.

**Salida:** proyecto Unity limpio y preparado para recibir contenido.

### Fase 2 — Inventario e importación de assets

- [x] Copiar los assets permitidos desde el proyecto Godot a `Assets/WhatTheHell3D/` sin incluir `.import`.
- [~] Preparar la importación de GLTF/GLB, FBX, OBJ, texturas, audio y fuente; la importación visual requiere abrir Unity y GLTF/GLB necesita importer compatible.
- [ ] Validar escala, orientación, normales, tangentes, materiales, compresión y mipmaps dentro del editor Unity.
- [ ] Revisar rig, huesos y clips del caballero, goblins, zombies y bruja.
- [ ] Crear materiales URP equivalentes para superficies, roca, bosque, minas, castillo, agua, lava y efectos emisivos.
- [x] Registrar assets que requieren corrección manual o sustitución en `Settings/Phase2_AssetImport.md`.

**Salida actual:** biblioteca de fuentes copiada y clasificada. La verificación de importación, rigs, materiales y shaders permanece pendiente del editor Unity.

### Fase 3 — Datos, escenas y servicios comunes

- [x] Convertir `LevelConfig` y los tres `.tres` en `ScriptableObject` tipados y versionables.
- [x] Reemplazar los diccionarios de layout por datos serializables para plataformas, peligros, pickups, enemigos, checkpoints y objetivo.
- [x] Diseñar e implementar el esquema versionado y el servicio persistente JSON de estado de campaña.
- [x] Implementar el registro de escenas: menú, intro, niveles 1–3 y victoria como catálogo de rutas.
- [x] Definir contratos comunes para daño, vida, interacción, checkpoint, pickup y objetivo de nivel.
- [x] Crear el servicio persistente `CampaignRuntimeState` para iniciar, continuar, reiniciar y completar niveles.

**Salida actual:** arquitectura de datos y servicios preparada en `Settings/Phase3_DataModel.md`; el catálogo y el estado persistente ya están conectados a las escenas de bootstrap. La compilación/deserialización dentro del editor Unity queda pendiente de verificación.

### Fase 4 — Slice vertical del jugador y un nivel

- [~] Crear el jugador en runtime con `CharacterController`, `HealthComponent` y configuración reutilizable.
- [~] Reproducir movimiento, salto doble, coyote time, jump buffer, sprint, gravedad y control aéreo.
- [~] Reproducir ataque, stamina, guardia con reducción de daño, dodge con invulnerabilidad, lock-on, daño y muerte/reaparición.
- [ ] Conectar animaciones importadas, espada en el hueso de la mano, sonidos de combate y VFX finales.
- [x] Implementar la cámara de aventura con seguimiento, zoom, colisión y lock-on usando los valores de cada nivel.
- [x] Construir un tramo runtime verificable del nivel 1 con suelo, plataformas, enemigos, pickups y checkpoints.

**Salida actual:** slice vertical runtime en `Scripts/Runtime/Gameplay/CampaignLevelRuntime.cs`, `PlayerController.cs` y `CameraController.cs`. La paridad fina y las animaciones requieren Unity Editor/playtest.

### Fase 5 — Enemigos e interacción

- [~] Crear variantes runtime de goblin, zombie y bruja con `EnemyController` y `HealthComponent`.
- [~] Portar patrulla, detección, persecución, leash, ataque y muerte; quedan pendientes proyectiles, wind-up y recovery animados.
- [ ] Configurar NavMesh por nivel y verificar que no se use navegación en zonas donde la locomoción debe ser manual.
- [x] Portar llave, corazones, monedas, checkpoints, salida, spikes, sierras, lava y kill zones.
- [x] Portar plataformas móviles, plataformas que caen, escaleras y puentes mediante componentes runtime.

**Salida actual:** conjunto reutilizable en `Scripts/Runtime/Gameplay/EnemyController.cs` y `WorldObjects.cs`; las variantes visuales de los modelos fuente siguen pendientes de importación/rig.

### Fase 6 — Construcción de los tres niveles

- [x] Reproducir el generador/layout del bosque, minas y castillo a partir de los `ScriptableObject`.
- [x] Mantener posiciones, tamaños, secuencia de retos, enemigos, pickups, checkpoints y objetivos en los datos migrados.
- [ ] Decidir por nivel qué elementos deben ser prefabs, geometría procedural o contenido authorable en escena.
- [~] Configurar iluminación, fog, colores, lava y límites de cámara; agua, decoración y materiales finales requieren conversión URP.
- [ ] Validar en Play Mode que cada nivel pueda reiniciarse y completarse de forma independiente.

**Salida actual:** tres escenas bootstrap que construyen su layout en runtime desde los assets de datos; falta validar Play Mode y sustituir primitivas temporales por assets importados.

### Fase 7 — Menús, HUD, pausa, cinemática y audio

- [x] Portar menú principal, nueva partida, continuar, selección directa de nivel y navegación de botones.
- [x] Portar HUD de objetivo, salud, monedas, llave, checkpoint y controles.
- [x] Portar pausa, reanudar, reiniciar nivel y salir al menú respetando el tiempo pausado.
- [~] Crear la introducción con subtítulos, fade visual y skip; la versión actual usa coroutine y falta reconstruir la secuencia final en Timeline.
- [~] Mantener transición al nivel 1 y líneas de texto; las voces y clips aún no están vinculados.
- [~] Crear `CampaignAudioDirector` y dejar el audio importado disponible; faltan AudioMixer, asignación de música/ambiente/SFX y validación de volumen.

**Salida actual:** flujo de presentación funcional a nivel de scripts en `Scripts/Runtime/UI` y `Scripts/Runtime/Audio`; la integración visual/audio final sigue pendiente de Editor.

### Fase 8 — Paridad, rendimiento y entrega

- [~] Ejecutar validaciones estáticas de JSON, GUIDs, escenas, Build Settings, referencias y ausencia de `.import`/`.uid`.
- [ ] Ejecutar pruebas funcionales de menú, controles, combate, IA, pickups, checkpoints, guardado y victoria en Play Mode.
- [ ] Comparar cada nivel contra la línea base de Godot y corregir diferencias relevantes.
- [ ] Revisar errores de consola, referencias, colliders, capas, animaciones, audio y escenas de build.
- [ ] Medir FPS, memoria, draw calls, luces, partículas, sombras y tiempos de carga.
- [ ] Optimizar import settings, occlusion, LOD, batching, audio y efectos según mediciones.
- [ ] Generar una build de prueba y documentar instalación, controles y limitaciones conocidas.

**Salida actual:** checklist estático y documentación actualizados; la build jugable y las mediciones de rendimiento requieren Unity Editor.

## 6. Obstáculos principales y mitigaciones

| Obstáculo | Riesgo | Mitigación |
|---|---|---|
| GDScript no es portable directamente a C# | Alto | Portar por sistemas y contratos, no traducir línea por línea; validar cada slice |
| `campaign_level.gd` concentra 1665 líneas de generación y wiring | Alto | Separar datos, builders, prefabs y servicios; conservar primero el layout y después refactorizar |
| Las escenas `.tscn` mezclan nodos, recursos y subescenas | Alto | Reconstruir prefabs y escenas Unity con una matriz de referencias y validación de cada dependencia |
| Materiales y `Water.gdshader` son específicos de Godot | Alto | Crear equivalentes URP; aceptar diferencias visuales controladas hasta ajustar el resultado |
| GLTF/FBX/OBJ pueden importar con escala, ejes, rigs o materiales distintos | Alto | Hacer una prueba de importación temprana y congelar reglas de escala, orientación y nombres de huesos |
| Animaciones de Mixamo y sockets de espada dependen de huesos concretos | Alto | Verificar avatar/retarget, clips, root motion y socket de mano antes de completar el combate |
| Física y movimiento de `CharacterBody3D` difieren de Unity | Alto | Medir velocidades y tiempos; no asumir equivalencia entre `move_and_slide` y `CharacterController` |
| IA y navegación en plataformas con saltos o cambios de altura | Medio/alto | Usar NavMesh solo en rutas adecuadas y resolver patrulla/ataques especiales con locomoción controlada |
| Estado global y guardado usan rutas de Godot | Alto | Crear un servicio persistente y un formato de guardado versionado, con pruebas de reinicio y checkpoint |
| Cinemática basada en tweens y `await` | Medio | Rehacerla como Timeline más controladores de secuencia, audio y skip; verificar pausa y transición |
| VFX procedural y feedback generado en runtime | Medio | Crear prefabs VFX reutilizables y validar su rendimiento en cada nivel |
| Proyecto Unity parte de una plantilla casi vacía | Medio | Hacer primero la configuración base y un slice vertical; no intentar importar todo de una vez |
| Licencias y peso de assets | Medio | Mantener `THIRD_PARTY_NOTICES.md`/fuentes equivalentes y excluir caches, builds y archivos generados |

## 7. Criterios globales de aceptación

- [ ] La build inicia en el menú principal y permite comenzar, continuar, seleccionar y reiniciar niveles.
- [ ] La cinemática conserva su secuencia, audio, subtítulos, fade, skip y entrada al nivel 1.
- [ ] Los tres niveles se pueden completar con sus objetivos, llaves, checkpoints, enemigos y salida.
- [ ] El jugador reproduce los controles y mecánicas principales sin bloqueos ni estados imposibles.
- [ ] El guardado sobrevive al cambio de escena y permite continuar desde el checkpoint esperado.
- [ ] Los enemigos reciben y aplican daño correctamente, respetan sus estados y no atraviesan límites no previstos.
- [ ] No existen referencias rotas, errores de consola bloqueantes ni assets externos no autorizados.
- [ ] La build de Unity solo incluye las escenas y dependencias de la campaña activa.
- [ ] Las diferencias visuales menores quedan documentadas; las diferencias de jugabilidad, progresión o estabilidad no se consideran aceptables.

## 8. Orden recomendado de trabajo

1. Línea base y datos.
2. Configuración de Unity e importación de un conjunto pequeño de assets.
3. Jugador + cámara + slice vertical del nivel 1.
4. Enemigos e interacciones.
5. Generación completa de los niveles 1–3.
6. Menús, HUD, pausa, cinemática y audio.
7. Paridad, rendimiento, build y documentación.

La regla de avance es no continuar con el siguiente bloque si el bloque anterior no tiene una prueba reproducible y un criterio de salida verificable.

## 9. Registro de avance

- **Fase 0:** completada mediante auditoría estática el 2026-08-21.
- **Resultado:** alcance, flujo, controles, parámetros críticos, niveles, assets/licencias y matriz de paridad documentados.
- **Pendiente explícito:** playtest interactivo de Godot cuando el ejecutable esté disponible.
- **Fase 1:** implementación base completada; Build Settings apunta al flujo migrado y la verificación de Editor queda pendiente.
- **Fase 2:** en curso; fuentes copiadas, con importación visual y revisión de rigs/materiales pendiente.
- **Fase 3:** base implementada y conectada al runtime; falta validar compilación/deserialización en Editor.
- **Fases 4–7:** implementación runtime base completada en scripts y escenas bootstrap; animaciones, modelos, Timeline, audio final y balance quedan pendientes.
- **Fase 8:** validación estática ejecutada; Play Mode, build y rendimiento pendientes por ausencia de Unity Editor en este entorno.
- **Corrección de compilación/metadatos:** se corrigieron cuatro GUIDs de 31 caracteres en `.meta` y se actualizó la referencia de `Intro.unity` en `EditorBuildSettings.asset`; no se modificó código C# ni `Library`.
- **Contexto:** resumen consolidado en `contexto.md` para reutilizarlo como referencia del proyecto.
- **Siguiente paso:** abrir Unity 6000.5.6f1, dejar que importe/compile, corregir errores de API o serialización y ejecutar la matriz de pruebas de `Settings/RuntimeImplementation.md`.
