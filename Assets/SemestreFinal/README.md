# SemestreFinal — menú principal y sistema visual

Implementación de `docs/direccion-artistica-menu-principal.md` para **Unity 6 (6000.0.x) + URP**.

## Requisitos del proyecto

| Paquete | Uso |
|---|---|
| Universal RP (17.x) | Render, Volume, post-processing |
| Input System | Teclado, ratón y mando en el menú |
| uGUI 2.x (incluye TextMeshPro) | Texto 3D de los folios. Importar **TMP Essential Resources** una vez |

## Cómo probarlo

Guía detallada paso a paso, desde crear el proyecto: `docs/guia-desde-cero.md`.


1. Copiar `Assets/SemestreFinal/` (con sus `.meta`) al proyecto.
2. `Semestre Final ▸ Apply Render Settings`: Render Scale 0,6 + Nearest-Neighbor en todos los URP Assets.
3. Añadir a mano al *Universal Renderer*: **Screen Space Ambient Occlusion** y **Decal** (la consola indica los valores si faltan).
4. `Semestre Final ▸ Build Main Menu Scene` → genera `Scenes/MainMenu.unity`, materiales, `VP_Instituto_Base` y `LS_Instituto`.
5. Opcional: `Window ▸ Rendering ▸ Lighting ▸ Generate Lighting` (lightmaps de las luces estables).
6. Play.

**Controles:** ↑/↓ o W/S, stick o cruceta para moverse por las opciones. Enter, Espacio, botón Sur o clic para confirmar. Escape, Retroceso, botón Este o clic derecho para volver al tablón.

## Piezas

| Script | Qué hace | ¿Se reutiliza en gameplay? |
|---|---|---|
| `Core/InstitutoClock` | Hora congelada (11:29), fecha y "pulso del timbre" común a todas las luces | Sí, es la fuente de verdad |
| `Core/FrozenClock` | Pone cualquier reloj o pantalla a las 11:29 | Sí |
| `Lighting/FluorescentFlicker` | Modos A–E (Steady, SlowDip, Partial, Burst, Dead). Sincroniza el `Light` con la emisión del tubo y usa una semilla por tubo | Sí, todos los fluorescentes |
| `Interaction/AroHorario` | El glifo de interacción: 12 marcas sin el "11" y una aguja a las 11:29. Estados Hidden / Latent / Focused / Hold / Locked / Danger. Se adapta a círculo o estadio | Sí, es el lenguaje de interacción |
| `Menu/MenuOption` | Una opción por capas: folio, texto, chincheta, luz (aclarado del papel) y subrayado | No (solo menús) |
| `Menu/MenuController` | Navegación (teclado, mando, ratón), mueve el aro, confirma y gira la cámara al destino | No |
| `Menu/MenuCameraRig` | Cabeza del jugador: transiciones suaves y respiración mínima | No |
| `Shaders/Glifo.shader` | Unlit, transparente, color por vértice; no ilumina la escena | Sí |
| `Editor/MainMenuSceneBuilder` | Blockout de la sala de la foto con todas las referencias asignadas | — |
| `Editor/InstitutoPalette` | Materiales URP/Lit con las reglas de color y smoothness | Sí, materiales base |

## Integrar el menú con el juego

`MenuController.OptionConfirmed` (C#) y `MenuOption.onConfirmed` (UnityEvent) avisan de la opción elegida. Por ejemplo, se pueden enganchar ahí la carga del lobby en JUGAR y el panel de ajustes en AJUSTES. El menú solo resuelve SALIR: mira la señal de salida, apaga F2 y cierra el juego. `MenuController.ReturnHome()` devuelve la cámara al tablón desde cualquier submenú.

## Notas

- Todo lo generado es **blockout con primitivas**. Los modelos finales sustituyen a las cajas conservando nombre y posición.
- El tubo de cada fluorescente instancia su material en runtime para poder parpadear solo. Son pocos objetos, así que el coste es asumible.
- `InstitutoRenderSetup` no añade SSAO ni Decals por código, porque la API de renderer features no es pública y se arriesgaría a corromper el asset. Solo avisa si faltan.
