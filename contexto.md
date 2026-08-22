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
