# UI-GAME-street-Larpers
Juego de lacetania de chills, con los Femboys

Sandbox para desarrollar y probar el **HUD** de un juego de lucha (estilo Street Fighter) en **Unity 6**,
de forma independiente al juego real y preparado para integrarse después en él.

- Arquitectura y decisiones: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- Módulo portable del HUD: `Assets/StreetLarpersHUD/`
- Herramientas de testing (no se migran): `Assets/Sandbox/`

## Primer arranque (solo la primera vez, lo hace una persona del equipo)

El repositorio es la **raíz del proyecto Unity**, pero `ProjectSettings/` y `Packages/` todavía no existen
(no se pueden generar fuera del editor). Para crearlos:

1. Unity Hub → **New project** → editor **Unity 6 (6000.x LTS)** → plantilla **Universal 2D**
   (el HUD es independiente del pipeline; si el equipo usa otra plantilla, usad la misma que el juego real).
   Nombre y ubicación: cualquiera **fuera** del repositorio, p. ej. `HUDTemp`.
2. Cuando abra, cierra Unity.
3. Copia desde `HUDTemp/` a la raíz del repo las carpetas `Packages/` y `ProjectSettings/`
   y el contenido de `HUDTemp/Assets/` dentro de `Assets/` (no copies `Library/`, `Temp/`, `Logs/`...).
4. Unity Hub → **Add → Add project from disk** → selecciona la carpeta del repo y ábrelo.
5. Comprueba en *Edit ▸ Project Settings ▸ Player ▸ Active Input Handling* que está en
   **Input System Package (New)** o **Both** (las plantillas de Unity 6 ya lo traen así).
6. Haz commit de `Packages/`, `ProjectSettings/` y **todos los `.meta`** que haya generado Unity.

El resto del equipo solo tiene que clonar y abrir la carpeta desde Unity Hub.

## Probar el HUD

1. Menú **Street Larpers ▸ Sandbox ▸ Build HUD Sandbox Scene** → genera `Assets/Sandbox/Scenes/HUDSandbox.unity`.
2. Pulsa **Play**.

| Tecla | Acción |
|-------|--------|
| `1` / `2` / `3` | P1: daño / curar / KO |
| `8` / `9` / `0` | P2: daño / curar / KO |
| `Shift` + daño | Daño fuerte |
| `R` | Reiniciar la vida de ambos |

También puedes usar el menú contextual (⋮) del componente `SimulatedFighter` en el Inspector durante Play.
