# Fase 3 — Datos, escenas y servicios comunes

## Resultado

La configuración de los niveles de Godot se trasladó a un modelo serializable de Unity sin depender de `.tres` ni de diccionarios de GDScript.

## Archivos creados

- `Scripts/Runtime/Data/CampaignLevelConfig.cs`: `ScriptableObject` y tipos serializables para layout, entorno, cámara, plataformas, escaleras, peligros, pickups, checkpoints y enemigos.
- `Data/CampaignLevel01_Forest.asset`.
- `Data/CampaignLevel02_Mines.asset`.
- `Data/CampaignLevel03_Castle.asset`.
- `Scripts/Runtime/SceneManagement/CampaignSceneCatalog.cs`: registro de menú, intro, niveles y victoria.
- `Data/CampaignSceneCatalog.asset`: rutas previstas de las escenas activas.
- `Scripts/Runtime/Services/CampaignProgressData.cs`: esquema versionado de progreso.
- `Scripts/Runtime/Services/JsonCampaignProgressStore.cs`: contrato y almacenamiento JSON en `Application.persistentDataPath`.
- `Scripts/Runtime/Contracts/GameplayContracts.cs`: contratos de daño, interacción, checkpoints, coleccionables y objetivo.

## Equivalencias aplicadas

| Godot | Unity |
|---|---|
| `LevelConfig.level_id` | `CampaignLevelConfig.levelId` |
| `Vector3`/`Color` | `UnityEngine.Vector3`/`UnityEngine.Color` |
| Diccionario de plataforma | `CampaignPlatformPlacement` |
| Diccionario de enemigo | `CampaignEnemyPlacement` + `CampaignEnemyKind` |
| `kind = spikes/saw` | `CampaignHazardKind` |
| `kind = coin/heart/key` | `CampaignPickupKind` |
| `GameState` campaign fields | `CampaignProgressData` |
| `ConfigFile` en `user://` | JSON versionado en `Application.persistentDataPath` |
| Rutas de escenas `res://` | `CampaignSceneCatalog` con rutas `Assets/...` |

Los tres assets conservan posiciones, tamaños, colores, temas, objetivos, checkpoints, enemigos y objetivo final de sus fuentes Godot. Los valores de enum son intencionales: `Goblin/Zombie/Witch`, `Spikes/Saw/Lava` y `Coin/Heart/Key` mantienen un orden estable para la serialización.

## Estado de validación

- El modelo y los assets están escritos con nombres de campos compatibles.
- Los `.meta` de los scripts y `ScriptableObject` fueron preparados para mantener referencias estables.
- Las seis escenas Unity de las rutas del catálogo ya existen como escenas bootstrap y están registradas en `EditorBuildSettings.asset`.
- El proyecto no se ha abierto en el editor Unity dentro de este entorno, por lo que queda pendiente verificar compilación, deserialización de los assets y referencias serializadas.
- El servicio de guardado ya está conectado al `CampaignRuntimeState`, gameplay, checkpoints, pickups, HUD y pausa; la verificación interactiva queda pendiente.
