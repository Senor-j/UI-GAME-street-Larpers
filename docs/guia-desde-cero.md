# Guía desde cero: probar el menú principal en Unity

Tiempo estimado: unos 20 minutos la primera vez. Versión: **Unity 6000.0.84f1**.

## 1. Descargar el código

1. Abre el repositorio en GitHub: `senor-j/UI-GAME-street-Larpers`.
2. Arriba a la izquierda, en el botón de rama (pone `main`), elige **`claude/gifted-fermi-aejxuj`**.
3. Pulsa el botón verde **Code ▸ Download ZIP** y descomprímelo en cualquier sitio.
   - Si usáis GitHub Desktop, también vale: *Clone* y cambiar a esa rama.

Dentro del ZIP te interesa la carpeta **`Assets/SemestreFinal`**.

## 2. Crear el proyecto de Unity

> Si ya tenéis un proyecto con URP, saltad al paso 3 y usad ese.

1. Abre **Unity Hub ▸ Projects ▸ New project**.
2. Arriba, en *Editor Version*, elige **6000.0.84f1**.
3. Elige la plantilla **Universal 3D**, que es la de URP. **No** elijas "3D (Built-In Render Pipeline)".
4. Ponle un nombre (por ejemplo `SemestreFinal`) y pulsa **Create project**.

## 3. Comprobar el Input System

1. En Unity: **Window ▸ Package Manager**. En *In Project* debe aparecer **Input System**.
   - Si no aparece: *Unity Registry* ▸ busca "Input System" ▸ **Install**.
   - Si Unity pregunta si quieres activar el nuevo sistema de entrada, responde **Yes**. El editor se reinicia.
2. Comprueba **Edit ▸ Project Settings ▸ Player ▸ Other Settings ▸ Active Input Handling**. Tiene que poner **Input System Package (New)** o **Both**.

## 4. Copiar los archivos del juego

1. Con Unity abierto, ve al explorador de archivos del sistema (Explorador de Windows o Finder).
2. Copia la carpeta **`SemestreFinal`** del ZIP (la que está dentro de `Assets/`) y pégala dentro de la carpeta **`Assets`** de tu proyecto. Copia también el archivo `SemestreFinal.meta`.
3. Vuelve a Unity y espera a que termine de compilar (rueda abajo a la derecha).
4. Abre **Window ▸ General ▸ Console**. No debe haber **errores en rojo**. Si los hay, copia el texto y mándamelo.

Si todo ha ido bien, en la barra de menús de arriba aparece un menú nuevo: **Semestre Final**.

## 5. Importar las fuentes de TextMeshPro

**Window ▸ TextMeshPro ▸ Import TMP Essential Resources ▸ Import.**

Sin este paso, los textos de los folios salen invisibles.

## 6. Ajustes de render (aspecto retro)

**Semestre Final ▸ Apply Render Settings ▸ Aplicar.**

Baja la resolución interna al 60 % con escalado pixelado.

## 7. Ambient Occlusion y Decals (a mano, una vez)

1. En la ventana **Project**, abre la carpeta `Assets/Settings`. Selecciona **`PC_Renderer`** (el de tipo *Universal Renderer Data*).
2. En el **Inspector**, abajo del todo, pulsa **Add Renderer Feature ▸ Screen Space Ambient Occlusion**. Si ya existe, solo ajústalo:

   | Parámetro | Valor |
   |---|---|
   | Intensity | 1.2 |
   | Radius | 0.35 |
   | Direct Lighting Strength | 0.35 |
   | Samples | Medium |
   | Downsample | activado |

3. Pulsa otra vez **Add Renderer Feature ▸ Decal**.
4. Repite los pasos 1–3 con `Mobile_Renderer` si queréis la misma imagen en calidad baja.

## 8. Generar la escena del menú

**Semestre Final ▸ Build Main Menu Scene.**

Crea y abre `Assets/SemestreFinal/Scenes/MainMenu.unity`, junto con sus materiales y ajustes. Si la escena ya existe, pregunta antes de sobrescribirla.

## 9. Probar

1. En la pestaña **Game**, pon la proporción en **16:9** (desplegable de arriba a la izquierda de la vista).
2. Pulsa **Play** (▶).

| Acción | Teclado | Mando | Ratón |
|---|---|---|---|
| Moverse por el menú | ↑ / ↓ o W / S | Stick o cruceta | Pasar por encima |
| Confirmar | Enter o Espacio | A / Cruz | Clic |
| Volver al tablón | Escape | B / Círculo | Clic derecho |

**SALIR** detiene el modo Play.

## 10. (Opcional) Hornear la luz

**Window ▸ Rendering ▸ Lighting ▸ Generate Lighting.** Tarda unos minutos y mejora el rebote de luz de los fluorescentes estables.

## 11. Grabar un vídeo

- **Opción rápida:** Win + G (Xbox Game Bar en Windows) o OBS.
- **Desde Unity:**
  1. Package Manager ▸ *Unity Registry* ▸ **Recorder** ▸ Install.
  2. **Window ▸ General ▸ Recorder ▸ Recorder Window ▸ Add Recorder ▸ Movie**.
  3. Pulsa **Start Recording**. Graba solo mientras está en Play y deja el vídeo en la carpeta `Recordings` del proyecto.

## Problemas típicos

| Síntoma | Causa | Solución |
|---|---|---|
| No aparece el menú "Semestre Final" | Errores de compilación | Mira la Console y mándame los errores en rojo |
| Todo sale **rosa** | El proyecto no usa URP | Crea el proyecto con la plantilla *Universal 3D* (paso 2) |
| Los folios no tienen texto | Faltan los recursos de TMP | Paso 5 y luego vuelve a hacer el paso 8 |
| Las teclas no hacen nada | Input System desactivado | Paso 3, *Active Input Handling* |
| Error de `Unity.InputSystem` en Console | Falta el paquete | Instala *Input System* (paso 3) |
| Se ve demasiado oscuro o demasiado claro | Las intensidades son una primera estimación | Toca la *Intensity* de las luces en `Iluminacion` (jerarquía) o el *Post Exposure* en `VP_Instituto_Base` |
