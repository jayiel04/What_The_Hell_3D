# Contexto del Proyecto - What The Hell? 3D

## Estado Actual

### Última Iteración: Integración de UI de vida y fuente medieval
**Fecha**: 27 Agosto 2026

## Cambios Realizados

### 1. Integración de barra de vida personalizada
- **Imágenes**: `BordeBarraVida.png` (2400x1340) y `BarraVida.png` (2000x1116) integradas en `Assets/WhatTheHell3D/UI/`
- **Escenas modificadas**: CampaignLevel01, CampaignLevel02, CampaignLevel03
- **Configuración UI**:
  - `HealthBarBackground`: sprite BordeBarraVida, preserveAspect=true, size 360x201
  - `HealthBarFill`: sprite BarraVida, anchors al cavity interior del borde (0.123,0.328)-(0.879,0.776)
  - `HudPanel`: 400x340 px
- **Texto eliminado**: HealthLabel ("SALUD" + porcentaje) removido de las 3 escenas

### 2. Fuente MedievalSharp
- **Fuente descargada**: `MedievalSharp-Regular.ttf` de Google Fonts (SIL OFL)
- **Ubicación**: `Assets/WhatTheHell3D/UI/Source/`
- **Archivos C# actualizados**:
  - `UIStyleKit.cs` - DefaultFont → MedievalSharp
  - `CampaignAuthoringTools.cs` - DefaultFont → MedievalSharp
  - `MenuSceneAuthoring.cs` - DefaultFont → MedievalSharp
- **Escenas actualizadas**: Las 6 escenas (MainMenu, Intro, Victory, CampaignLevel01-03) usan MedievalSharp

### 3. Corrección de rutas de escenas
- **Problema**: `CampaignSceneCatalog.asset` y múltiples scripts apuntaban a `Assets/WhatTheHell3D/Scenes/` pero las escenas están en `Assets/Scenes/`
- **Archivos corregidos**:
  - `CampaignSceneCatalog.asset` y `.cs`
  - `CampaignAuthoringTools.cs`, `MenuSceneAuthoring.cs`, `CampaignLevelSceneBuilder.cs`
  - `IntroSceneAuthoring.cs`, `BuildScript.cs`, `SceneScreenshot.cs`, `SceneDiagnostics.cs`
  - `IntroCutsceneDirector.cs`, `CampaignFlowTests.cs`

## Archivos Importantes

| Archivo | Descripción |
|---------|-------------|
| `Assets/WhatTheHell3D/UI/BordeBarraVida.png` | Marco decorativo de barra de vida |
| `Assets/WhatTheHell3D/UI/BarraVida.png` | Relleno de barra de vida |
| `Assets/WhatTheHell3D/UI/Source/MedievalSharp-Regular.ttf` | Fuente principal del juego |
| `Assets/WhatTheHell3D/Data/CampaignSceneCatalog.asset` | Catálogo de escenas |
| `Assets/WhatTheHell3D/Tools/Editor/CampaignAuthoringTools.cs` | Herramienta de autoría de UI |
| `Assets/WhatTheHell3D/Scripts/Runtime/UI/CampaignHudController.cs` | Controlador HUD en runtime |

## Notas Técnicas

- **Resolución de referencia**: 1280x720 (CanvasScaler)
- **Render mode**: ScreenSpaceOverlay
- **Los scripts de autoría son Editor-only**: Se ejecutan desde menús de Unity, no en runtime
- **El GUID de MedievalSharp en escenas**: `a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6`

## Estado de Escenas

| Escena | Estado | Notas |
|--------|--------|-------|
| MainMenu | OK | Botón "NUEVO JUEGO" funcional, fuente MedievalSharp |
| Intro | OK | Fuente MedievalSharp |
| CampaignLevel01 | OK | HUD con barra de vida personalizada |
| CampaignLevel02 | OK | HUD con barra de vida personalizada |
| CampaignLevel03 | OK | HUD con barra de vida personalizada |
| Victory | OK | Fuente MedievalSharp |

## Última Iteración: Corrección de stats y transición de nivel
**Fecha**: 27 Agosto 2026

### Problema reportado
- Las estadísticas del puntaje (monedas, llave, checkpoint) no se actualizaban en el HUD.
- El jugador no transicionaba de nivel al llegar a la meta.

### Causa raíz
- El `Player` (CharacterController + HealthComponent) **no tenía Rigidbody**. En Unity,
  `OnTriggerEnter` solo se dispara si al menos uno de los objetos involucrados tiene un
  Rigidbody. Las pickups, checkpoints y la meta usan colliders `isTrigger=true` y
  `OnTriggerEnter` con `CompareTag("Player")`, por lo que ningún evento se generaba.
- Secundario: `coinsLabel`, `keyLabel`, `checkpointLabel` en `CampaignHudController` estaban
  null (fileID 0) en las 3 escenas de campaña, así que aunque `Progress` cambiara, el HUD no lo reflejaba.

### Cambios realizados
- `PlayerController.cs` (`Awake`): añade un `Rigidbody` cinemático (isKinematic=true,
  useGravity=false) si no existe. Cubre las escenas ya generadas en disco.
- `CampaignLevelSceneBuilder.cs` (`CreatePlayer`): añade el `Rigidbody` al crear el jugador (estado autorado consistente).
- `CampaignLevel01.unity`, `CampaignLevel02.unity`, `CampaignLevel03.unity`: cableados
  `coinsLabel`, `keyLabel`, `checkpointLabel` con los fileID de sus Text components correspondientes.

### Verificación
- Compilación headless de Unity (batchmode) exitosa: sin errores, solo warnings CS0618
  pre-existentes en herramientas Editor (APIs obsoletas, no relacionados con este cambio).
- Flujo confirmado: triggers disparan → `CampaignRuntimeState.Collect/SetCheckpoint` actualiza
  `Progress` → `CampaignHudController.Update` refresca labels → `GoalRuntime.Finish` (requiere
  `keyCollected`) carga la siguiente escena vía `Catalog.GetNextScene`.

## Última Iteración: Espada del jugador + reorganización de multimedia
**Fecha**: 27 Agosto 2026

### Solicitud
- El jugador debe tener una espada (la del proyecto Godot; el `Sword.fbx` ya existe en
  `Assets/WhatTheHell3D/Art/Source/rpg_items/weapons/Sword.fbx` e idéntico al de Godot por md5).
- Mover los archivos de la carpeta `Assets/integrar` a una carpeta de recursos multimedia organizada.

### Cambios: espada
- `CampaignLevelSceneBuilder.cs` (`CreatePlayer`): instancia `Sword.fbx` como hijo "Sword" del jugador
  al reconstruir escenas. También se añadió `EnsureSwordResource()` (MenuItem + ejecutable por batch)
  que crea `Assets/WhatTheHell3D/Resources/Sword.prefab` a partir del FBX.
- `PlayerController.cs`:
  - `Awake` → `EnsureSwordChild()`: si la escena no tiene hijo "Sword", lo crea desde
    `Resources.Load<GameObject>("Sword")` (cubre escenas existentes sin reconstruir).
  - `LoadCharacterModel` → `AttachSwordToHand(modelRoot)`: reparenta "Sword" al hueso de la
    mano derecha (`LowerArm.R`, con candidatos alternos) para que siga la animación del brazo.
    Fallback: lo coloca a la altura del pecho si no encuentra el hueso.

### Cambios: reorganización multimedia
- Creada `Assets/WhatTheHell3D/Media/UI/`.
- Movidos (git mv, preserva GUIDs): `BarraVida.png`, `BordeBarraVida.png`, `TablaPuntaje2.png`
  (+ .meta) desde `Assets/integrar/` a `Assets/WhatTheHell3D/Media/UI/`.
- Eliminada la carpeta `Assets/integrar` (quedó vacía).

### Notas / pendiente de ajuste visual
- **Bug corregido (espada invisible)**: el `Sword.fbx` tiene escala de raíz (100,100,100) porque está
  modelado en cm; la espada mide ~2.3u. El código forzaba `localScale = Vector3.one`, reduciéndola
  ~100x (~5mm) y volviéndola invisible. Se eliminaron esos overrides en `PlayerController.AttachSwordToHand`
  y en `CreatePlayer` (builder); ahora se conserva la escala natural del FBX.
- La posición/rotación local de la espada en el hueso (`localPosition (0,0,0)`, rotación identity)
  es un valor provisional: requiere ajuste visual en el editor (la empuñadura debe coincidir con la mano).
- `Resources/Sword.prefab` se incluye en el build (necesario para cargar la espada en runtime).
- `Assets/WhatTheHell3D/UI/` conserva copias activas de BarraVida/BordeBarraVida usadas por las escenas;
  `Media/UI/` guarda ahora los recursos originales de `integrar`.
