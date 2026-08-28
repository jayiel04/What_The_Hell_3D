# Normas de Desarrollo

## Reglas Generales

1. **Verificación visual obligatoria**: Después de cada cambio visual (UI, sprites, fuentes, colores, layouts), debo generar una captura o preview del resultado y mostrárselo al usuario antes de continuar.

2. **Sin interfaces fuera de pantalla**: Ningún elemento UI debe salirse del Canvas o de la pantalla. Verificar que todos los `RectTransform` tengan `anchors` y `sizeDelta` correctos para la resolución de referencia (1280x720).

3. **Coherencia con lo pedido**: El resultado debe ser fiel a lo que el usuario solicitó. Si hay ambigüedad, preguntar antes de implementar.

4. **No romper funcionalidad existente**: Al modificar escenas, scripts o assets, verificar que nada anterior se haya roto. Probar la cadena completa de uso.

5. **Rutas consistentes**: Todas las rutas en scripts, assets serializados y metadatos deben apuntar a la ubicación real de los archivos. Nunca hardcodear rutas incorrectas.

## Reglas de Persistencia

6. **Registro en contexto.md**: Después de cada iteración exitosa (cambio completo y verificado), actualizar `contexto.md` con:
   - Qué se hizo
   - Archivos modificados/creados
   - Estado actual del proyecto

7. **No asumir GUIDs**: Siempre verificar que los GUIDs en archivos serializados coincidan con los metadatos reales antes de modificar escenas.

8. **Respaldar antes de destruir**: Si un cambio es destructivo (sobrescribir imágenes, eliminar objetos de escena), asegurarme de que el usuario está de acuerdo antes de proceder.

## Reglas de Código

9. **Seguir convenciones existentes**: Mimicar el estilo de código del proyecto (nomenclatura, patrones, estructura).

10. **No agregar comentarios innecesarios**: Solo comentarios si el usuario lo pide explícitamente.

11. **Verificar compilación**: Después de modificar scripts C#, ejecutar verificación de compilación si es posible.

## Reglas de Integración de Assets

12. **Verificar metadatos**: Al mover o crear assets, asegurar que los `.meta` tengan configuración correcta (sprite import settings, texture type, etc.).

13. **No sobrescribir sin confirmar**: Si un asset ya existe, preguntar antes de sobrescribirlo.

14. **Probar en runtime**: Verificar que los assets se cargan correctamente en el motor, no solo que existen en disco.
