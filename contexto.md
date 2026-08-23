# Contexto del proyecto: What the Hell? 3D

## Objetivo general

Migrar el proyecto jugable de Godot 4.7 a Unity 6000.5.6f1 con URP, conservando la jugabilidad, progresión, flujo de campaña, assets existentes e intención visual.

El flujo objetivo es:

```text
Menú principal → Nueva partida → Cinemática de introducción
→ Nivel 1: Bosque → Nivel 2: Minas → Nivel 3: Castillo → Victoria
```

La primera entrega se limita a la campaña 3D activa. Los niveles legacy, el prototipo 2.5D y las mejoras de diseño no presentes en Godot quedan fuera hasta alcanzar paridad funcional.

## Proyectos y versiones

- Fuente: `/home/javier/Documentos/Juegos/Godot Projects/What_The_Hell_3D-main`.
- Destino: `/home/javier/Documentos/Juegos/Unity Projects/What_The_Hell_3D`.
- Godot fuente: 4.7.
- Unity destino: 6000.5.6f1.
- Render pipeline: URP.
- Paquetes relevantes ya disponibles: Input System, AI Navigation, Timeline y UGUI.

## Estado inicial

El proyecto Godot contiene el juego funcional, con menú, cinemática, tres niveles, jugador, combate, enemigos, cámara, UI, guardado, audio y generación procedural de niveles.

El proyecto Unity comenzó como una plantilla URP con `SampleScene`. No se modificó el proyecto Godot durante la migración.

## Cambios realizados

### Fase 0 — Línea base

- Se documentó el flujo activo y las escenas incluidas en el build de Godot.
- Se registraron controles, parámetros críticos del jugador, enemigos y cámara.
- Se documentaron reglas de llave, checkpoints, guardado, reinicio y transición entre niveles.
- Se registraron los objetivos, posiciones y composición de los tres niveles.
- Se separó el contenido activo del contenido legacy y del prototipo 2.5D.
- Se localizaron licencias, notices y procedencia de los assets.
- Se creó la matriz de paridad Godot → Unity.
- Se creó [phase_0_baseline.md](phase_0_baseline.md).
- Se dejó registrado que el ejecutable de Godot no está disponible en el entorno, por lo que el playtest interactivo queda pendiente.

### Fase 1 — Preparación de Unity

- Se creó `Assets/WhatTheHell3D/` con carpetas para arte, audio, datos, materiales, prefabs, escenas, scripts, settings, UI, VFX y herramientas.
- Se añadió la documentación de estructura en `Assets/WhatTheHell3D/README.md`.
- Se cambió el nombre del producto a `What the Hell? 3D`.
- Se configuró la resolución objetivo a 1280×720, igual que la referencia Godot.
- Se añadieron tags para jugador, enemigos, pickups, hazards, interactables, checkpoints, objetivos y proyectiles.
- Se añadieron layers de gameplay para jugador, enemigos, suelo, hazards, pickups, interactables, proyectiles, hitboxes, checkpoints, objetivo y colisión de cámara.
- Se confirmó el uso del nuevo Input System (`activeInputHandler: 1`) y la gravedad 3D de Unity.
- Se añadieron las acciones `Guard`, `Dodge`, `LockOn` y `Pause` con bindings equivalentes a Godot:
  - Click derecho: guardia.
  - Q: dodge.
  - Click central: lock-on.
  - Escape: pausa.
- `SampleScene` se mantiene temporalmente como bootstrap de Build Settings hasta crear la escena de menú migrada.

### Fase 2 — Assets y licencias

- Se copiaron los assets existentes de Godot a Unity, sin descargar assets externos.
- Se copiaron aproximadamente 261 MB de arte y 44 MB de audio.
- Se organizaron modelos, texturas, animaciones y fuentes en `Art/Source`.
- Se organizaron música, ambiente, diálogos y SFX en `Audio/Source`.
- Se conservaron los sonidos de `what the hell (sonidos)` en `Audio/Source/legacy_sounds`.
- Se copiaron licencias a `Settings/Licenses`.
- Se copiaron `ASSET_SOURCES.md` y `THIRD_PARTY_NOTICES.md` a `Settings/SourceNotices`.
- Se conservaron los recursos de agua Godot como referencia en `Art/Source/GodotWater`.
- No se copiaron archivos `.import` ni `.uid` generados por Godot.
- Se documentó que GLTF/GLB requiere un importer compatible o conversión a FBX.
- Se documentó que `Water.gdshader` debe rehacerse para URP mediante Shader Graph o HLSL.
- Se creó [Phase2_AssetImport.md](Assets/WhatTheHell3D/Settings/Phase2_AssetImport.md).

### Fase 3 — Datos y servicios comunes

- Se creó `CampaignLevelConfig` como `ScriptableObject` tipado.
- Se crearon los assets:
  - `CampaignLevel01_Forest.asset`.
  - `CampaignLevel02_Mines.asset`.
  - `CampaignLevel03_Castle.asset`.
- Se trasladaron a esos assets posiciones, tamaños, colores, plataformas, escaleras, hazards, pickups, checkpoints, enemigos y metas de los tres niveles.
- Se creó `CampaignSceneCatalog` y su asset con las rutas previstas para menú, intro, niveles y victoria.
- Se creó `CampaignProgressData` con esquema de guardado versionado.
- Se creó `JsonCampaignProgressStore` para guardar en `Application.persistentDataPath`.
- Se definieron contratos comunes: `IDamageable`, `IInteractable`, `ICheckpoint`, `ICampaignCollectible` e `ICampaignGoal`.
- Se creó [Phase3_DataModel.md](Assets/WhatTheHell3D/Settings/Phase3_DataModel.md).

### Fases 4–7 — Runtime, escenas y presentación

- Se creó `CampaignRuntimeState` como servicio persistente para nueva partida, continuar, selección directa, checkpoints, reinicio y transición a victoria.
- Se creó `InputReader` para conectar el action asset del Input System con el runtime y ofrecer fallback de teclado/ratón.
- Se creó `PlayerController` con `CharacterController`, movimiento 3D, salto doble, coyote time, jump buffer, sprint, stamina, ataque, guardia, dodge, interacción y reaparición.
- Se creó `HealthComponent` con daño, curación, guardia con reducción, invulnerabilidad durante dodge y evento de muerte.
- Se creó `CameraController` con seguimiento, zoom, colisión, look-ahead y lock-on al enemigo más cercano.
- Se creó `EnemyController` para goblins, zombies y brujas con patrulla, detección, persecución, leash, ataque y muerte.
- Se crearon componentes runtime para plataformas móviles, plataformas que caen, hazards, kill zone, pickups, bonus caches, checkpoints y meta.
- Se creó `CampaignLevelRuntime` como coordinador de los tres niveles; recibe referencias serializadas y configura estado, iluminación, fog, jugador, enemigos, cámara y objetivo sin construir contenido de escena.
- Se crearon las escenas `MainMenu`, `Intro`, `CampaignLevel01`, `CampaignLevel02`, `CampaignLevel03` y `Victory`, todas con `SceneBootstrap` y referencias serializadas al catálogo/datos.
- Se actualizó `EditorBuildSettings.asset` para iniciar en `MainMenu` y contener el flujo activo completo.
- Se crearon controladores runtime de menú, intro con subtítulos/skip, HUD, pausa, victoria y punto de integración de audio.
- Se creó [RuntimeImplementation.md](Assets/WhatTheHell3D/Settings/RuntimeImplementation.md) con inventario, comprobaciones estáticas y pruebas pendientes.

### Conversión a escenas authoradas en Unity Editor

- Se modificó `SceneBootstrap` para obtener `MenuSceneController`, `IntroSceneController`, `CampaignLevelRuntime` y `VictorySceneController` ya presentes en la escena, sin añadir componentes mediante código.
- `CampaignLevelRuntime` dejó de construir GameObjects, primitivas, colliders, luces, cámara, enemigos, pickups, UI y audio; ahora solo coordina referencias serializadas y configura el estado de campaña.
- `CampaignLevel01.unity`, `CampaignLevel02.unity` y `CampaignLevel03.unity` fueron reconstruidas con objetos serializados de plataforma, jugador, cámara, luces, hazards, pickups, checkpoints, enemigos, meta y componentes de comportamiento.
- Se añadieron también triggers estáticos de caída de plataformas y una kill zone fuera de límites.
- `MovingPlatformRuntime`, `FallingPlatformRuntime` y `CampaignAudioDirector` fueron ajustados para trabajar con referencias/componentes existentes en escena.
- La auditoría encontró 81, 83 y 91 GameObjects serializados en los niveles 1, 2 y 3 respectivamente.
- Se crearon seis materiales URP Lit persistentes en `Assets/WhatTheHell3D/Materials` y se asignaron a los 231 `MeshRenderer` de los tres niveles, con colores diferenciados para terreno, jugador, enemigos, pickups, peligros y objetivo.
- La geometría actual usa meshes primitivos integrados como representación temporal; los materiales de color son de blockout y todavía deben sustituirse por materiales y modelos finales importados.

## Archivos principales de la migración

- [plan.md](plan.md): plan, objetivos, fases, riesgos y registro de avance.
- [phase_0_baseline.md](phase_0_baseline.md): línea base de Godot y matriz de paridad.
- [CampaignLevelConfig.cs](Assets/WhatTheHell3D/Scripts/Runtime/Data/CampaignLevelConfig.cs): modelo de datos de niveles.
- [CampaignSceneCatalog.cs](Assets/WhatTheHell3D/Scripts/Runtime/SceneManagement/CampaignSceneCatalog.cs): catálogo de escenas.
- [CampaignProgressData.cs](Assets/WhatTheHell3D/Scripts/Runtime/Services/CampaignProgressData.cs): estado serializable de campaña.
- [JsonCampaignProgressStore.cs](Assets/WhatTheHell3D/Scripts/Runtime/Services/JsonCampaignProgressStore.cs): almacenamiento JSON.
- [GameplayContracts.cs](Assets/WhatTheHell3D/Scripts/Runtime/Contracts/GameplayContracts.cs): contratos de gameplay.
- [CampaignLevelRuntime.cs](Assets/WhatTheHell3D/Scripts/Runtime/Gameplay/CampaignLevelRuntime.cs): coordinador de referencias y estado de los niveles.
- [PlayerController.cs](Assets/WhatTheHell3D/Scripts/Runtime/PlayerController.cs): movimiento y combate base del jugador.
- [RuntimeImplementation.md](Assets/WhatTheHell3D/Settings/RuntimeImplementation.md): resumen de fases 4–8 y validaciones.

## Pendientes y limitaciones actuales

- Unity Editor 6000.5.6f1 estuvo disponible durante la verificación del 2026-08-22; el proyecto importó, deserializó y ejecutó Play Mode sin errores rojos de C#.
- GLTF/GLB todavía necesita glTFast, otro importer compatible o conversión a FBX.
- Los rigs, huesos, clips y sockets de espada aún no han sido validados en Unity.
- Agua, lava, fog y VFX todavía no han sido convertidos visualmente; los objetos sólidos ya tienen materiales URP Lit de blockout.
- Las escenas Unity del menú, intro, niveles y victoria ya existen como escenas bootstrap y están registradas en Build Settings.
- `SampleScene` se conserva fuera del flujo activo como escena de plantilla, pero ya no es la escena inicial del build.
- El servicio de guardado ya está conectado al flujo de escenas, gameplay, pickups, checkpoints, HUD y pausa.
- Las escenas ya contienen objetos serializados, primitivas temporales y materiales URP de blockout; los modelos, prefabs reutilizables, rigs, animaciones y materiales fuente todavía no están vinculados.
- Las seis escenas se abrieron en Unity Editor. Las tres campañas se revisaron en Scene View con jerarquía, MeshFilter, MeshRenderer, materiales, cámara y luces visibles; además se ejecutaron en Play Mode.
- La intro actual usa coroutine e IMGUI para subtítulos; Timeline, fade final, voces y AudioMixer todavía deben integrarse.
- La IA inicial usa locomoción propia, no NavMesh; proyectiles de bruja, parry, combo, knockback, wind-up/recovery y VFX finales siguen pendientes.
- La UI actual es IMGUI runtime para validar el flujo; debe sustituirse o consolidarse en Canvas/UGUI durante el acabado de presentación.
- La compatibilidad con el archivo de guardado de Godot no forma parte de la primera entrega.
- La build jugable completa sigue bloqueada por importer GLTF/GLB, UI UGUI, NavMesh, Timeline/audio final, animaciones y rendimiento; la compilación y la ejecución base sí fueron validadas en Editor.

### Corrección de errores de compilación y metadatos

- Se identificaron cuatro archivos `.meta` con GUIDs de 31 caracteres; Unity los ignoraba y provocaba errores secundarios de `ICampaignProgressStore`, `IDamageable`, `IInteractable` y `DamageInfo`.
- Se corrigieron los GUIDs de `JsonCampaignProgressStore.cs.meta`, `GameplayContracts.cs.meta`, `WorldObjects.cs.meta` e `Intro.unity.meta` a 32 caracteres hexadecimales.
- Se actualizó el GUID de `Intro.unity` en `ProjectSettings/EditorBuildSettings.asset`.
- No se eliminó `Library`, no se regeneraron metadatos globalmente y no se modificó código C#.
- La comprobación automática confirmó 371 GUIDs válidos, longitud de 32 caracteres y ausencia de duplicados. Unity salió de Safe Mode, importó las escenas y Console mostró 0 errores del proyecto; quedó 1 advertencia externa de Account API.

## Verificación visual y estado actual — 2026-08-22

- `MainMenu`: cámara persistente creada y guardada desde Unity Editor; Play Mode muestra el menú sin el aviso “No cameras rendering”.
- `Intro`: cámara persistente creada y guardada desde Unity Editor; Play Mode muestra subtítulos y el botón `Saltar`.
- `CampaignLevel01`, `CampaignLevel02` y `CampaignLevel03`: jerarquía authorada visible en Scene View; jugador, plataformas, enemigos/pickups/hazards, cámaras, luces y materiales URP Lit de blockout visibles en Play Mode con HUD.
- `Victory`: cámara persistente creada y guardada desde Unity Editor; Play Mode muestra victoria y el botón de retorno al menú funciona.
- Evidencias: `/tmp/unity-mainmenu-play-camera-fixed.png`, `/tmp/unity-intro-play2.png`, `/tmp/unity-campaign01-play-final.png`, `/tmp/unity-campaign02-play.png`, `/tmp/unity-campaign03-play.png`, `/tmp/unity-campaign03-pause2.png`, `/tmp/unity-victory-play.png` y `/tmp/unity-victory-return-menu.png`.
- La UI todavía usa IMGUI runtime: no se declara cerrada la condición de Canvas/UGUI hasta authorar Canvas, textos y botones serializados desde Unity Editor.

## Pendientes bloqueantes

- Importer compatible para GLTF/GLB, revisión de escala/rig/animaciones y sustitución de primitivas por modelos fuente.
- NavMesh y pruebas reales de completar/reiniciar cada nivel.
- Canvas/UGUI para menú, HUD, pausa y subtítulos; Timeline, voces, AudioMixer y clips de audio.
- Combate/IA avanzada, VFX finales, build de prueba y métricas de rendimiento.

## Cierre de fases 4–8 — 2026-08-22 (sesión de finalización)

### UI: IMGUI → Canvas/UGUI authorado

- Las seis escenas tienen Canvas serializados con `CanvasScaler` 1280×720, `GraphicRaycaster` y `EventSystem` con `InputSystemUIInputModule`.
- Menú: título, subtítulo, botones Nueva partida / Continuar / Nivel 1–3 e hint de controles, cableados a `MenuSceneController`.
- HUD por nivel: título y objetivo, barra de salud con `Image.Filled`, monedas, llave, checkpoint y hint.
- Pausa: panel serializado con Continuar / Reiniciar checkpoint / Volver al menú.
- Intro: 13 líneas originales de Godot con voces individuales (`s1_line1.mp3` … `s7_line2.mp3`), fade final y botón Saltar.
- Victoria: título, cuerpo, narración (`victory_narration.mp3`) y retorno al menú.

### Gameplay ampliado

- `PlayerController`: combo de hasta 3 golpes con multiplicadores, parry con ventana configurable que refleja daño, knockback recibido con resistencia, hooks de audio/VFX (`attackClip`, `parryClip`, `hurtClip`, `attackVfx`, `parryVfx`) y socket de espada para el modelo final.
- `EnemyController`: máquina de estados Patrol/Chase/WindUp/Strike/Recover/Stunned con telegraph visual, hit-stun, knockback aplicado al jugador y lanzamiento de proyectil en brujas mediante `WitchProjectile.prefab`.
- `WitchProjectileRuntime`: proyectil con daño, velocidad, vida limitada y destrucción contra jugador o geometría.

### Datos, escenas y servicios

- `WhatTheHell3D.Runtime.asmdef` creado; scripts divididos en un archivo por clase (corrige la resolución de componentes multi-clase serializados).
- `CampaignLevelSceneBuilder` (Editor): reconstruye los tres niveles desde los `CampaignLevelConfig` como escenas authoradas reproducibles; `CampaignAuthoringTools.AuthorAll` configura UI, audio y NavMesh.
- NavMesh horneado por nivel como asset externo (`Assets/WhatTheHell3D/NavMesh/*.asset`) evitando serialización binaria de las escenas.
- AudioMixer `WhatTheHellMixer.mixer` (Master/Music/Ambience/SFX) creado desde Editor; música por nivel (level 1–3.mp3), ambiente, SFX de combate y narración de victoria vinculados.

### Verificación

- Suite `CampaignFlowTests` (Play Mode): **9/9 superadas** — carga completa L01–L03, daño/muerte/reaparición del jugador, muerte de enemigos, proyectil de bruja, pickups, checkpoints, meta condicionada por llave y ciclo JSON del guardado.
- Validación de assets: glTFast 6.2.0 importa los 6 personajes (meshes + rigs detectados) y las 8 animaciones del caballero con sus clips.
- Validaciones estáticas: 395 GUIDs válidos sin duplicados, sin `.import`/`.uid`, Build Settings en orden, 379 referencias de script sin rotas, escenas en YAML texto.
- Build de prueba Linux64: `Builds/Test/WhatTheHell3D.x86_64` (232 MB, 0 errores) con arranque verificado.

### Limitaciones conocidas restantes

- Los personajes siguen siendo primitivas de blockout: conectar modelos GLTF rigueados + Animator + socket de espada queda como trabajo de presentación.
- Agua/lava/VFX finales requieren Shader Graph/materiales URP definitivos.
- Timeline opcional: la intro usa coroutine funcional equivalente.
- Métricas de rendimiento formales y playtest manual comparativo contra Godot pendientes de sesión interactiva.

## Malla del personaje principal visible — 2026-08-22 (sesión complementaria)

### Hallazgo
La malla del caballero **sí se había transferido de Godot a Unity**: `Knight_Male.gltf` (exportado vía Mixamo/Blender a glTF 2.0) estaba en `Assets/WhatTheHell3D/Art/Source/characters/player/`. El problema de "no se ve el personaje principal" era que el jugador en escena solo tenía `CharacterController` + `HealthComponent` + `PlayerController`, **sin ninguna malla** (el enemigo sí tenía una cápsula de placeholder, el jugador no).

### Solución (carga en tiempo de ejecución, sin dependencias del editor)
- El glTF es autocontenido (buffer embebido en base64, sin `.bin` ni texturas externas), así que se copió a `Assets/StreamingAssets/Characters/Knight_Male.gltf` para que `Application.streamingAssetsPath` lo resuelva tanto en editor como en build.
- `WhatTheHell3D.Runtime.asmdef` ahora referencia `glTFast`.
- `PlayerController` carga el modelo de forma asíncrona (`GltfImport.LoadFile` + `InstantiateMainSceneAsync`) y lo padre al jugador como hijo `KnightModel`, con `modelScale` y `modelYawOffsetDegrees` expuestos para ajuste fino de orientación/escala.
- La animación usa el componente `Animation` (legacy, válido en build) con los **17 clips embebidos del propio glTF** (Death, Defeat, Idle, Jump, PickUp, Punch, RecieveHit, Roll, Run, Run_Carry, Shoot_OneHanded, SitDown, StandUp, SwordSlash, Victory, Walk, Walk_Carry). `UpdateAnimation()` elige el clip según estado: muerte → `Death`, esquiva → `Roll`, ataque → `SwordSlash`, daño → `RecieveHit`, en aire → `Jump`, y locomoción → `Walk`/`Run`/`Idle` (en bucle).
- Si la carga falla, se crea una cápsula de respaldo (`PlayerCapsule`) para que el jugador nunca quede invisible.
- Se mantiene el marcador de posición de la cápsula deshabilitado cuando el modelo real carga.

### Verificación
- Compilación: 0 errores C#.
- Suite Play Mode: **10/10** (nuevo test `Level01_JugadorMuestraMallaDelCaballero` afirma `SkinnedMeshRenderer` con malla de vértices > 0 en el hijo `KnightModel`).
- Build Linux64 (234 MB, 0 errores); `Knight_Male.gltf` confirmado dentro de `WhatTheHell3D_Data/StreamingAssets/Characters/`.

### Pendiente (mismo patrón aplicable a enemigos)
- Equipar la espada en el hueso de la mano (`Fist.R`) vía el `swordSocket` existente.
- Aplicar el mismo flujo glTFast a goblins, zombies y bruja (`Assets/WhatTheHell3D/Art/Source/characters/enemies/`) para reemplazar sus cápsulas.
- Retargetear a un avatar Humanoid si se quieren mezclar clips de distintas fuentes; actualmente se usan los clips nativos del esqueleto del caballero (Generic/legacy), que animan fielmente sin retargeting.

## Mallas de enemigos visibles — 2026-08-22 (continuación)

### Mismo patrón que el jugador, extendido a enemigos
- 5 glTF copiados a `Assets/StreamingAssets/Characters/Enemies/` (Goblin_Male/Female, Zombie_Male/Female, Witch — mismas 17 animaciones Mixamo que Knight).
- `EnemyController` (en `WhatTheHell3D.Runtime`, ya referencia `glTFast`) carga en `Start()` el modelo según `CampaignEnemyKind.kind`:
  - Goblin/Zombie eligen variante M/F por hash de posición (`|x|*7.3+|z|*3.7`) para variedad sin cambiar el enum.
  - Witch → `Witch.gltf`.
  - `characterModelPath` expuesto para override manual si se desea forzar una variante.
- Carga idéntica a jugador: `GltfImport.LoadFile` + `InstantiateMainSceneAsync` bajo hijo `<Kind>Model`, `Animation` legacy con `playAutomatically=false`, clips con `wrapMode` (Loop para Idle/Walk/Run, Once para resto), `CrossFade` por estado.
- Animación mapeada a `EnemyState`: Patrol/Chase → Walk/Run/Idle, WindUp → Punch / `Shoot_OneHanded` (bruja), Strike → SwordSlash / `Shoot_OneHanded`, Stunned → RecieveHit, Recover → Idle, Death → Death (en `OnDied`). Tint de estados (`SetTint`) y `OnDied` gris ahora incluyen el modelo (refresca `GetComponentsInChildren<Renderer>` tras la carga y oculta la cápsula `EnemyVisual`).
- Armadura del jugador: inicio de carga movido de `Awake` a `Start` para que `Configure` fije `kind` antes de elegir el glTF (evita condición de carrera en spawns dinámicos).

### Verificación
- 11/11 Play Mode (`Level01_JugadorMuestraMallaDelCaballero` + `Enemigos_MuestranMallaSegunKind` con 300 frames de espera para las 6 cargas concurrentes).
- Build Linux64 244 MB, 0 errores, 6 glTF en `StreamingAssets/Characters/` + `Enemies/` confirmados.
- Enemigo sigue con colisionador `CharacterController` (la malla es solo visual, no afecta colisión).

### Ajustes visuales pendientes (si se ven girados/pequeños)
- `modelYawOffsetDegrees` y `modelScale` expuestos en `EnemyController` y `PlayerController` por instancia.
- Equipar armas en hueso `Fist.R` vía socket (pendiente de retarget humanoide si se quieren mezclar animaciones de distintas fuentes).

## Doble música en nivel 1 corregida — 2026-08-22

### Causa (paridad Godot)
- En Godot `campaign_level_audio.gd` (`_setup_audio`) solo reproduce `LEVEL_MUSIC[level_id-1]` (`level 1/2/3.mp3`) por nivel; `ambiente 1.mp3` es la música del **menú** (`main_menu.gd: _setup_music`), no del nivel.
- En Unity `CampaignAuthoringTools.AuthorAudio` asignaba `director.ambientClip = ambiente 1.mp3` **y** `musicClip = level X.mp3` para cada `CampaignLevel`, por lo que en `CampaignAudioDirector.Start()` sonaban dos pistas a la vez (dos canciones).

### Parche
- `Assets/WhatTheHell3D/Tools/Editor/CampaignAuthoringTools.cs:373` → `ambientClip = null` para niveles (comentado para evitar regresión).
- `Assets/WhatTheHell3D/Scenes/CampaignLevel*.unity` parcheadas directamente (`ambientClip: {fileID: 0}`) para no requerir re-horneo de NavMesh.
- `Assets/WhatTheHell3D/Scripts/Runtime/Audio/CampaignAudioDirector.cs` reforzado: patrón singleton `activeDirector` con `OnEnable`/`OnDisable`/`StopAllAudio()` y `Start()` detiene cualquier director previo y garantiza `ambientSource.Stop()` si `ambientClip==null`; evita solapamiento por escena aditiva o `DontDestroyOnLoad` residual.

### Verificación
- Suite PlayMode 12/12 (nuevo `Nivel01_SoloUnaPistaMusical` comprueba `ambientClip==null`, `musicClip!=null`, `ambientSource.isPlaying==false` y `musicSource.isPlaying==true`).
- Build Linux64 238 MB, 0 errores.

## Fase 9 — Cinemática de intro 3D — 2026-08-22

### Referencia Godot replicada
`scenes/cinematics/intro/intro_story_3d.tscn` (653 nodos) + `scripts/cinematics/intro_story_3d.gd`. Se extrajeron del `.tscn`: posiciones de las 28 tumbas, 39 árboles y 24 módulos de muralla, transformaciones de Gate/Castle/Statue, volúmenes en dB de los 5 AudioStreamPlayer y constantes del timeline.

### Runtime nuevo
- `Scripts/Runtime/Cinematics/IntroCutsceneDirector.cs`: timeline de 7 escenas con coroutines y lerp manual (sin Timeline package). Constantes exactas de Godot: cámara inicial (0,3.5,7)→wake (0,2.2,4.5) en 6 s; jugador escala 0.5; despertar z 4.2→0 en 2.8 s (velocidad de animación Walk proporcional = walkSpeed/2.8); tour por (1.8,2,-1.5)/(-1.8,1.6,-0.5)/(2.4,2,-4) a 2.2 s c/u; caminata larga z→−16.5 en 12.6 s con cámara siguiendo a 4.5 m (los tweens continúan dentro de la S6 como en Godot); glow portal 0→2.5/2.5 s, castillo 0→3.0/3.5 s; fades de subtítulo 0.6 s con hold = longitud real del clip de voz.
- Skip: Enter/Esc (Input System) o botón «Enter para omitir» → detiene todo y carga `CampaignLevel01.unity` inmediatamente (equivalente a `_finish_intro(skip=true)`).
- Carga del caballero vía glTFast desde `StreamingAssets/Characters/Knight_Male.gltf`, Animation legacy con Idle/Walk en bucle.
- `SceneBootstrap` ahora configura `IntroCutsceneDirector` (el viejo `IntroSceneController` fue eliminado junto con su script).

### Builder editor nuevo
- `Tools/Editor/IntroSceneAuthoring.cs` (`WhatTheHell3D > Autoría > Autoría de intro 3D`, idempotente): piso de adoquín (textura cobblestone), backdrop de colinas (URP/Unlit), estatua (statue.obj), puerta z=−18 (pilares+arco+Glow), castillo z=−28 (Keep=city_house ×2, Towers/Rears=small_building, muro base, CastleLight), cementerio (28 GraveStone_*.fbx), bosque (39 Tree1–4.obj), muralla (24 WallBricks.fbx), MoonLight direccional, MainCamera+AudioListener, 5 AudioSources (wind/bell/breath/footsteps/voces) y UI UGUI. Materiales persistidos como assets en `Materials/Intro/*.mat`. `CampaignAuthoringTools.AuthorAll` lo invoca.

### Verificación
- Suite PlayMode **14/14**: `Intro_Cutscene_TimelineAvanzaYModeloCarga` (15 líneas, luces presentes, modelo glTF cargado, viento+campana en bucle, luna se anima 0→≤0.65 con polling ≤40 s porque la S1 dura ~10 s, cámara se mueve, escala 0.5) y `Intro_Skip_CargaNivel01`.
- Compilación C#: 0 errores. Validación estática: 0 fallos (378 refs de script, GUIDs, Build Settings, YAML).
- Build Linux64: 254 MB, 0 errores.

### Desviaciones conocidas respecto a Godot
- El escenario usa primitivas/materiales planos para pilares/arco/muro base (en Godot eran cajas escaladas equivalentes); los modelos OBJ/FBX sí son los originales.
- FOV de cámara 70 (Godot usaba el default ~75 sin especificar en la intro).
- Volumen lineal aproximado para wind (Godot 0 dB): 0.8 en Unity para evitar saturación.

### Corrección post-playtest (pantalla negra sin cambios de plano)
Dos causas encontradas y corregidas en `IntroSceneAuthoring.cs`:
1. **Cámara mirando al vacío**: en Unity el forward de la cámara es +Z, pero todo el escenario (estatua z=−7, puerta z=−18, castillo z=−28) está en −Z; Godot mira hacia −Z por defecto. Fix: `MainCamera` con rotación Y=180°.
2. **Fondo opaco del canvas**: `EnsureCanvas("UICanvas", negro)` creaba una Image de fondo a pantalla completa que seguía negra aunque el fade subiera. Fix: canvas sin fondo (`null`) y se elimina cualquier hijo `Background` residual.
Se añadieron aserciones anti-regresión al test `Intro_Cutscene_TimelineAvanzaYModeloCarga`: `camera.forward.z < −0.9` y `fadeImage.alpha ≤ 0.95` tras la S2. Re-verificado: suite 14/14, build 254 MB 0 errores.
