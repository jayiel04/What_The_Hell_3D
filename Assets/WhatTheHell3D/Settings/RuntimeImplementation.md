# Implementación runtime de las fases 4–8

## Componentes creados

- `Scripts/Runtime/Services/CampaignRuntimeState.cs`: estado persistente, nueva partida, continuar, reinicio, checkpoints y transición de escenas.
- `Scripts/Runtime/Services/InputReader.cs`: lectura del mapa `Player` y fallback de teclado/ratón.
- `Scripts/Runtime/PlayerController.cs`: movimiento cinemático, salto doble, coyote time, jump buffer, sprint, stamina, ataque, guardia, dodge, interacción y reaparición.
- `Scripts/Runtime/CameraController.cs`: seguimiento, zoom, colisión de cámara, look-ahead y lock-on.
- `Scripts/Runtime/Contracts/HealthComponent.cs`: daño, curación, guardia, invulnerabilidad y muerte.
- `Scripts/Runtime/Gameplay/EnemyController.cs`: goblin, zombie y bruja con patrulla, detección, persecución, leash, ataque y muerte.
- `Scripts/Runtime/Gameplay/WorldObjects.cs`: plataformas móviles, plataformas que caen, hazards, pickups, bonus caches, checkpoints y meta.
- `Scripts/Runtime/Gameplay/CampaignLevelRuntime.cs`: constructor runtime para los tres layouts migrados.
- `Scripts/Runtime/UI/`: menú, intro, HUD, pausa y victoria.
- `Scripts/Runtime/Audio/CampaignAudioDirector.cs`: punto de integración para música y ambiente.

## Escenas creadas y flujo

1. `Scenes/MainMenu.unity`
2. `Scenes/Intro.unity`
3. `Scenes/CampaignLevel01.unity`
4. `Scenes/CampaignLevel02.unity`
5. `Scenes/CampaignLevel03.unity`
6. `Scenes/Victory.unity`

Todas usan `SceneBootstrap` y referencias explícitas al catálogo, a los `CampaignLevelConfig` y al `InputActionAsset`.

## Comprobaciones realizadas fuera de Unity Editor

- Los assets de datos se validaron leyendo el cuerpo YAML después del encabezado Unity.
- El action asset sigue siendo JSON válido.
- No quedan `.import`, `.uid` ni temporales dentro de `Assets/WhatTheHell3D`.
- No se detectaron GUIDs duplicados en los `.meta` de la migración.
- Las seis escenas tienen `.meta` y están incluidas en `EditorBuildSettings.asset`.
- El primer Build Settings entry es `MainMenu.unity`.

## Comprobaciones obligatorias en Unity Editor

- Confirmar que el proyecto compila sin errores y que Unity acepta las referencias serializadas.
- Confirmar importación de FBX/OBJ/texturas/audio/fuente y resolver GLTF/GLB.
- Sustituir primitivas temporales por prefabs/modelos importados y verificar escala, materiales, rigs y animaciones.
- Probar menú, intro, pausa, movimiento, doble salto, combate, IA, pickups, checkpoints, guardado y victoria en Play Mode.
- Asignar clips al `CampaignAudioDirector`, crear AudioMixer y reconstruir la cinemática final con Timeline.
- Ejecutar una build desde `MainMenu`, revisar consola y medir rendimiento.
