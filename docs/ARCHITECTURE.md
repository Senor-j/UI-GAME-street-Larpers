# Arquitectura del HUD

Prioridades: **modularidad > facilidad de integración > mantenibilidad > testing > complejidad**.

Pregunta guía para cada decisión: *¿podremos sacar este sistema del sandbox e integrarlo en el juego real
sin reconstruirlo?*

## Capas

```
 Juego (real o simulado)          Contrato (HUD)            HUD
 ───────────────────────          ──────────────            ───
 SimulatedFighter  ──implementa──▶ IHealthSource ◀──escucha── HealthBarBinder ──▶ HealthBarView
 (PlayerHealth real en el futuro)                                                   │
                                                                     HealthBarStyle ┘ (aspecto)
```

| Pieza | Responsabilidad | ¿Qué NO hace? |
|-------|-----------------|---------------|
| **Contrato** (`IHealthSource`, `HealthChange`) | Define qué datos necesita el HUD. | No contiene lógica de juego. |
| **Binder** (`HealthBarBinder`) | Se suscribe a una fuente y pasa los datos a la vista. | No dibuja nada ni calcula daño. |
| **Vista** (`HealthBarView`) | Dibuja un valor 0–1. | No sabe de quién es la vida. |
| **Estilo** (`HealthBarStyle`, ScriptableObject) | Colores/umbrales editables sin código. | No contiene referencias de escena. |
| **Sandbox** (`SimulatedFighter`, `HudDebugKeyboard`, builder) | Simular el juego y provocar situaciones. | **No se migra** al juego real. |

La vista y el binder están separados a propósito: la vista se puede controlar desde cualquier sitio
(p. ej. una pantalla de selección de personaje) y el binder es el único punto que conoce "el juego".

## Integración futura en el juego real

1. Copiar `Assets/StreetLarpersHUD/` completo (con sus `.meta`).
2. En el componente de vida del equipo, implementar `IHealthSource` (2 propiedades + 1 evento),
   **o** crear un adaptador pequeño si no queremos tocar su código.
3. Asignar ese componente al `HealthBarBinder` (Inspector) o llamar a `binder.Bind(fuente)` desde código
   si los personajes se instancian en runtime.

`Assets/Sandbox/` no se copia. Los assembly definitions garantizan la dirección de dependencias:
`StreetLarpers.HUD` **no puede** referenciar al sandbox (el compilador lo impediría).

## Decisiones tomadas

- **uGUI (Canvas) para el HUD**, no UI Toolkit. En Unity 6 ambos sirven para runtime, pero uGUI es más directo
  para un HUD de juego con sprites, 9-slice, materiales/shaders, partículas y Animator/Timeline, que es lo
  que probablemente pediremos en feedback visual. UI Toolkit sigue siendo buena opción para menús o
  herramientas de debug más adelante.
- **Interfaces C# + eventos** para comunicar juego → HUD. Alternativas consideradas:
  - *ScriptableObject event channels*: muy desacoplado, pero añade assets por cada dato y cuesta más depurar.
  - *Event bus estático*: sencillo, pero global y difícil de controlar con 2 jugadores.
  - Interfaces: explícitas, fáciles de implementar en el código del equipo y fáciles de testear.
- **Relleno por anchors** en lugar de `Image.fillAmount`: funciona con o sin sprite y con sprites 9-slice.
- **Estilo en ScriptableObject**: cambiar colores/umbrales = editar un asset; cambiar forma/sprites = editar el
  prefab o la jerarquía de la vista. La lógica no cambia.
- **Escena generada por código** (solo sandbox): evita errores de referencias a mano y permite regenerarla.
- **Input System** (paquete por defecto en Unity 6) para los atajos de debug.

## Estructura de carpetas

```
Assets/
├─ StreetLarpersHUD/          ← MÓDULO PORTABLE (se migra)
│  ├─ Runtime/                  StreetLarpers.HUD.asmdef
│  │  ├─ Core/                  Contratos compartidos (IHealthSource, ...)
│  │  ├─ Health/                Barra de vida
│  │  ├─ Timer/                 (siguiente paso)
│  │  └─ ...                    Una carpeta por elemento del HUD
│  ├─ Styles/                   Assets de estilo (ScriptableObjects)
│  ├─ Prefabs/                  (cuando la estructura visual se estabilice)
│  ├─ Art/  Audio/  Fonts/      (cuando existan recursos)
│  └─ Tests/                    (tests EditMode de la lógica pura, más adelante)
└─ Sandbox/                   ← SOLO TESTING (no se migra)
   ├─ Runtime/                  StreetLarpers.HUD.Sandbox.asmdef
   │  ├─ Simulation/            Datos/jugadores ficticios
   │  └─ Debugging/             Atajos y paneles de debug
   ├─ Editor/                   StreetLarpers.HUD.Sandbox.Editor.asmdef (builder de escena)
   └─ Scenes/
```

Las carpetas marcadas "(más adelante)" se crean cuando haga falta, no antes.

## Hoja de ruta

1. ✅ Proyecto, carpetas, arquitectura base, barra de vida mínima, testing por teclado.
2. Temporizador (`IMatchTimerSource` → `TimerView`) + pausa/reanudar.
3. Información de jugadores (nombre, retrato) con TextMeshPro.
4. Barra de vida "pendiente" (el trozo que se vacía con retraso) y feedback de daño.
5. Estados de combate (ROUND 1, FIGHT, KO, TIME, victorias por ronda).
6. Panel de debug en pantalla.
7. Sonidos del HUD.
8. Prefabs + temas completos.
9. Animaciones (si se deciden).
10. Preparar la integración.
