Eres Claude Code y estás en mi PC con Windows: puedes leer y modificar archivos y ejecutar comandos en PowerShell. Soy estudiante y no domino Unity, así que explícame en español y en pasos cortos lo que necesites que haga yo (clics en ventanas). Todo lo que puedas hacer tú por comandos, hazlo tú.

## Contexto

Estamos haciendo un juego cooperativo de terror low-poly ambientado en un instituto español. Nombre provisional: "Semestre Final". En otra sesión se escribió el código del MENÚ PRINCIPAL para Unity 6 + URP, pero nunca se ha llegado a ejecutar en Unity.

- **Código del menú:** carpeta `SemestreFinal` + archivo `SemestreFinal.meta`. Están en `%USERPROFILE%\Unity\SemestreFinal\`.
  - También están en GitHub: repositorio `senor-j/UI-GAME-street-Larpers`, rama `claude/gifted-fermi-aejxuj`, carpeta `Assets/SemestreFinal/`.
- **Documentación:** `docs/direccion-artistica-menu-principal.md`, `docs/guia-desde-cero.md` y `Assets/SemestreFinal/README.md` (en ese repositorio/rama). Léela antes de tocar nada.
- **Proyecto de Unity actual:** `%USERPROFILE%\Downloads\Pos tenemos 2 freddys\Semestre Final`.
  - Creado con Unity **6000.6.3f1** (Unity 6.6) y plantilla URP.
  - Ya se copió `SemestreFinal` en su `Assets` y se importó TMP Essential Resources.
  - **Problema:** el menú "Semestre Final" no aparece en la barra de Unity. Seguramente hay errores de compilación que nadie ha visto.
- **Versión que usa el equipo:** **Unity 6000.0.84f1**. La tengo descargada/instalada en algún sitio del PC, pero NO está añadida a Unity Hub y no la encuentro.
- **Cuidado con las rutas:** mi carpeta de usuario es `C:\Users\Señor J` (con "ñ" y espacio). En PowerShell usa siempre `$env:USERPROFILE` en vez de escribir la ruta. Al principio ya tuvimos un `DirectoryNotFoundException` por una ruta demasiado larga.

## Limpieza pendiente (hazla primero)

Un comando anterior copió por error `SemestreFinal` y `SemestreFinal.meta` dentro de `C:\Windows\System32`.

1. Comprueba si siguen ahí.
2. Si están, comprueba que la carpeta solo contiene `Editor`, `Runtime`, `Shaders`, `README.md` y `.meta`.
3. Enséñame lo que vas a borrar y, cuando te lo confirme, borra SOLO esos dos elementos.

No toques nada más de System32.

## Tareas, en este orden

### 1. Encontrar Unity 6000.0.84f1 y añadirlo a Unity Hub

1. Busca `Unity.exe` cuya ruta contenga `6000.0.84f1`. Mira en:
   - `C:\Program Files\Unity*`
   - `C:\Program Files\Unity\Hub\Editor\`
   - Descargas
   - otros discos
2. Si lo que encuentras es solo el instalador (`UnitySetup64-6000.0.84f1.exe` o similar), dímelo: hay que instalarlo.
3. Añádelo a Unity Hub. Puedes probar con la CLI de Hub:
   `& "C:\Program Files\Unity Hub\Unity Hub.exe" -- --headless editors --add "<ruta a Unity.exe>"`
   Si no funciona, guíame para hacerlo a mano: Hub ▸ Installs ▸ Locate.
4. Comprueba que Hub lo lista.

### 2. Dejar el proyecto en la versión del equipo, en una ruta corta y sin "ñ"

- No conviene "bajar" de versión un proyecto ya abierto con 6000.6. Lo más limpio es un proyecto nuevo en **`C:\Unity\SemestreFinal`**, con 6000.0.84f1 y la plantilla **Universal 3D** (URP).
- La carpeta `C:\Unity\SemestreFinal` ya existe, pero está vacía o no es un proyecto. Revísala antes de usarla.
- Si no puedes crear el proyecto con plantilla por línea de comandos, guíame para crearlo en Hub y luego sigue tú.
- Después:
  1. Copia `SemestreFinal` + `SemestreFinal.meta` en su `Assets`.
  2. Asegúrate de que está el paquete **Input System** y de que *Active Input Handling* es "Input System Package (New)" o "Both".
  3. Importa TMP Essential Resources. Por código: `AssetDatabase.ImportPackage` con el `.unitypackage` de TMP Essential Resources que trae el paquete `com.unity.ugui`, o guíame (Window ▸ TextMeshPro ▸ Import TMP Essential Resources).

### 3. Arreglar los errores de compilación

1. Ejecuta Unity en batch para compilar y leer el log:
   `& "<Unity.exe>" -batchmode -quit -projectPath "C:\Unity\SemestreFinal" -logFile "C:\Unity\compile.log"`
2. Busca los `error CS` y corrígelos en `Assets/SemestreFinal`.
   - Ese código solo se comprobó contra imitaciones de la API de Unity, así que puede haber firmas o nombres de API que no coinciden con Unity 6000.0.
3. Repite hasta que no quede ningún error.
4. No cambies el diseño. Solo arregla lo necesario.

### 4. Generar y abrir la escena del menú

El código añade un menú de editor con:

- **Semestre Final ▸ Apply Render Settings**: `SemestreFinal.Editor.InstitutoRenderSetup.Apply`. Pone Render Scale 0.6 y escalado Nearest-Neighbor.
- **Semestre Final ▸ Build Main Menu Scene**: `SemestreFinal.Editor.MainMenuSceneBuilder.Build`. Genera `Assets/SemestreFinal/Scenes/MainMenu.unity`.

Puedes lanzarlos con `-executeMethod`. Ojo: usan `EditorUtility.DisplayDialog`. Si en batch eso molesta, añade métodos alternativos sin diálogo para batch.

Hay además un paso manual: añadir los Renderer Features **Screen Space Ambient Occlusion** y **Decal** al Universal Renderer (`Assets/Settings/PC_Renderer`). Hazlo por código de forma segura o guíame. Los valores están en la documentación.

Luego abre Unity con el proyecto para que yo lo vea y le dé a Play.

### 5. Revisar y ajustar el menú visualmente

1. Para ver cómo queda sin depender de mí, crea un script de editor que renderice la cámara de la escena (`PlayerHead/Main Camera`) a un PNG. Ejecútalo en batch con gráficos (sin `-nographics`) y mira las imágenes.
2. Ajusta lo que haga falta:
   - intensidades de luz;
   - legibilidad de los folios del tablón con Render Scale 0.6;
   - tamaño del Aro Horario;
   - encuadre de la cámara;
   - que la pizarra no tape la cristalera al elegir JUGAR.
3. Respeta las reglas de `docs/direccion-artistica-menu-principal.md`:
   - toda luz tiene una fuente visible;
   - nada de sangre;
   - colores desaturados;
   - la hora congelada es 11:29.
4. Enséñame capturas del antes y el después y explícame qué has cambiado.

## Reglas

- Antes de borrar o sobrescribir cualquier cosa, enséñamelo y espera a que te lo confirme.
- No toques nada fuera de la carpeta del proyecto, `C:\Unity` y `%USERPROFILE%\Unity`, salvo la limpieza de System32 descrita arriba.
- Si algo no funciona, dime el error tal cual y qué vas a probar después.
- Al terminar, dame un resumen corto y la lista de archivos que has cambiado. Si tengo git configurado, proponme hacer commit en la rama `claude/gifted-fermi-aejxuj`; no hagas push sin preguntarme.
