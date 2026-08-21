# WhatTheHell3D — estructura del proyecto

Esta carpeta contiene el contenido de la migración del proyecto Godot. Los assets y escenas de la plantilla de Unity se mantienen fuera hasta que sean reemplazados o reutilizados conscientemente.

## Carpetas

- `Art/`: modelos, texturas, rigs y fuentes visuales importados.
- `Audio/`: música, ambiente, diálogos y efectos de sonido.
- `Data/`: `ScriptableObject` y datos serializados de campaña.
- `Materials/`: materiales y variantes URP.
- `Prefabs/`: jugador, enemigos, interactables, hazards, plataformas y VFX reutilizables.
- `Scenes/`: menú, introducción, niveles, victoria y escenas de pruebas.
- `Scripts/`: gameplay, servicios, UI, cámara, IA y herramientas runtime.
- `Settings/`: perfiles, capas, configuración de importación y assets de proyecto específicos.
- `UI/`: fuentes, sprites, prefabs y layouts de UGUI.
- `VFX/`: partículas, feedback de combate, portales y efectos ambientales.
- `Tools/`: herramientas de editor y validación de migración.

## Estado de migración

- Fase 2: fuentes de arte, audio, licencias y notices copiadas; importación visual pendiente de validación en Unity.
- Fase 3: datos de los tres niveles, catálogo de escenas, esquema de progreso y contratos comunes preparados y conectados al runtime.
- Fases 4–7: runtime base de jugador, cámara, enemigos, objetos interactivos, constructor de niveles, menú, intro, HUD, pausa y victoria implementado.
- Build Settings: el flujo activo inicia en `Scenes/MainMenu.unity` y contiene intro, tres niveles y victoria.
- Fase 8: validaciones estáticas ejecutadas; Play Mode, importación visual, build y rendimiento requieren Unity Editor.

La lista completa de componentes y pruebas pendientes está en `Settings/RuntimeImplementation.md` y el estado global en `plan.md`/`contexto.md`.

## Convenciones iniciales

- PascalCase para assets y tipos (`CampaignLevelConfig`, `PlayerController`).
- Cada prefab y `ScriptableObject` debe tener una responsabilidad clara.
- Las escenas de campaña no deben depender de `SampleScene`.
- Los assets de Godot se importan desde la fuente original y se validan antes de crear prefabs.
- Unity debe regenerar los `.meta` de estas carpetas al abrir el proyecto; no se deben crear GUID manuales fuera del editor.
