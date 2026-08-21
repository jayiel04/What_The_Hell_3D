# Fase 2 — Inventario e importación de assets

## Resultado

Se copiaron al proyecto Unity los assets existentes de Godot, sin descargar contenido externo y sin copiar archivos `.import` generados por Godot.

Destinos principales:

- `Art/Source/`: modelos, texturas, animaciones y fuentes visuales.
- `Audio/Source/`: música, ambiente, diálogos y SFX.
- `Audio/Source/legacy_sounds/`: sonidos de la carpeta `what the hell (sonidos)`.
- `UI/Source/`: fuente TTF.
- `Settings/Licenses/`: notices y licencias de los packs.
- `Settings/SourceNotices/`: `ASSET_SOURCES.md` y `THIRD_PARTY_NOTICES.md`.
- `Art/Source/Root/`: assets sueltos del root de Godot usados por el proyecto.
- `Art/Source/GodotWater/`: shader y recursos de agua de Godot como referencia de conversión.

## Inventario copiado

- 55 FBX.
- 43 MP3.
- 42 PNG.
- 31 OBJ y 28 MTL.
- 20 GLTF.
- 7 GLB.
- 8 JPG.
- 1 TTF.
- 1 shader Godot y 2 recursos `.tres` conservados como referencia.

El tamaño aproximado del contenido copiado es 261 MB de arte, 44 MB de audio y menos de 100 KB de notices/licencias.

## Compatibilidad y riesgos identificados

### Importación nativa esperada

- FBX: soportado por el importador de Unity; requiere revisar escala, rig y clips.
- OBJ/MTL: soportado con revisión manual de materiales y normales.
- PNG/JPG: soportado; albedo en sRGB, normales y máscaras en espacio lineal.
- MP3: soportado; el formato de compresión y los loops se definirán por categoría de audio.
- TTF: soportado; debe convertirse en Font Asset cuando se configure UI.

### Conversión pendiente

- GLTF/GLB: el proyecto Unity no tiene glTFast en `Packages/manifest.json`; los archivos quedaron copiados como fuente, pero su importación visual requiere añadir/verificar un importer compatible o convertirlos a FBX.
- `Water.gdshader`: es shader de Godot y no se puede usar directamente en URP; se conserva como referencia para Shader Graph/HLSL.
- `.tres`: son recursos de Godot; los `LevelConfig` fueron convertidos a assets Unity independientes en la fase 3, mientras que los recursos de agua quedan como referencia.
- Materiales generados por Godot: no se consideran equivalentes a materiales URP hasta revisión visual.

## Regla de importación para la siguiente revisión en Unity

1. Abrir el proyecto con Unity 6000.5.6f1 y dejar que genere `.meta`.
2. Verificar que no haya errores de importación antes de crear prefabs.
3. Revisar primero `Knight_Male`, un enemigo, una plataforma y una textura normal.
4. Congelar escala/orientación de modelos antes de crear escenas.
5. Separar materiales reutilizables de materiales específicos de cada asset.
6. Validar clips y nombres de huesos antes de conectar animaciones de combate.

La copia de fuentes está completada; la importación y validación dentro del editor Unity permanece pendiente porque Unity no está disponible por CLI en este entorno.
