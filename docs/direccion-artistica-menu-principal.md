# Dirección artística y menú principal

Documento de referencia para todo el equipo. Proyecto en **Unity 6 (6000.0.84f1) + URP**.

Referencias de partida:

- **Ref. A**: foto real. Sala común en semisótano del instituto, con techo bajo y vigas vistas, regletas fluorescentes de un tubo, baldosa cerámica blanca, pared de fieltro gris con carteles, banco verde, papeleras de reciclaje, armarios de madera al fondo, pilares chapados en piedra, mesas de patas verdes, sillas naranjas y negras, pizarra blanca con ruedas, cristalera hacia un aula, radiador, cajas de cartón y una franja de baldosa gris oscura en primer plano.
- **Ref. B**: concept que explora la atmósfera. La usamos solo como estudio de oscuridad, contraste y suelo reflectante.

> **Frase guía:** *"Este es un instituto completamente normal, pero algo está profundamente mal y lleva demasiado tiempo vacío."*

---

## 0. Lectura crítica de la Referencia B

| Funciona y lo conservamos | No cumple nuestras reglas y lo descartamos |
|---|---|
| Oscuridad general con zonas de luz bien separadas | **Luz roja en el aula sin ninguna fuente visible** (incumple la regla de iluminación) |
| Suelo con algo de brillo que refleja los fluorescentes | **Sangre** en el suelo |
| Mobiliario y distribución reconocibles de la foto | Grafitis de caritas "XX", que son un tópico del género y parecen de otra franquicia |
| Folios por el suelo (objetos fuera de lugar, en poca cantidad) | Título en tiza sobre una pared de **fieltro** (ese material no admite tiza) |
| Etiqueta de versión con clip (buena idea diegética) | Menú en 2D superpuesto sin relación física con el espacio |
| | Selección con un `>` y un cambio de fondo (convencional, sin identidad) |
| | Plafón cuadrado pixelado, cuando en la foto son **regletas lineales** |

---

## A. Dirección artística general — "Recreo Eterno"

**Concepto:** el instituto se quedó parado un jueves cualquiera **a las 11:29, un minuto antes de que sonara el timbre**. Todo sigue igual de ordenado y de cotidiano, pero nadie ha vuelto.

Pilares:

1. **Normalidad excesiva.** Los objetos están donde deberían, o *casi* donde deberían: una silla de más, un folio sobre una mesa, una mochila sola.
2. **Low-poly honesto.** Las formas son simples y legibles, con un único bisel en las aristas que reciben luz. No intentamos parecer AAA ni imitar una consola concreta.
3. **La luz cuenta la historia.** Cada mancha de luz tiene un origen visible. La oscuridad es la zona donde "falta" un fluorescente.
4. **Desaturado, con tres acentos de color.** El conjunto es frío y grisáceo. Solo hay tres acentos: el **verde** de la señal de salida, el **naranja** de las sillas y el **azul** de la papelera. Son colores que ya aparecen en la foto.
5. **Un solo glifo de interacción**, el *Aro Horario* (apartado D), que aparece en todo el juego.

**Prohibido:** sangre, grafitis satánicos o de caritas, ruina estructural, cadenas, telarañas por todas partes y luces de colores sin fuente.
**Permitido con moderación:** pintura desconchada en zócalos, manchas de humedad en el techo, un tubo fundido, cinta de embalar vieja, cajas apiladas, polvo.

---

## B. Diseño del menú principal — "El Tablón"

### Posición del jugador

Reproducimos la **posición de la foto**. El jugador está de pie, con los ojos a **1,65 m**, justo en el **umbral entre la baldosa gris oscura del pasillo y la baldosa blanca de la sala**. Ese cambio de suelo funciona como frontera narrativa: *todavía no has entrado*.

- La cámara es física, con un FOV vertical de **60–65°**. Corregimos el ojo de pez de la foto, que con un FOV tan alto resultaría artificial.
- Hay un *idle sway* mínimo: respiración de ±0,5 cm y ±0,2° con ruido Perlin. Basta para notar que hay alguien ahí sin marear.

### Planta esquemática (vista cenital)

```
   PARED FONDO:  [EXIT]  [armarios madera con cinta]   [puerta]  [TV 11:29]
   ┌──────────────────────────────────────────────────────────────────┐
   │ (extintor)                                                      │
 F │                              ▓pilar   [pizarra]   ▓pilar         │
 I │         F1 ═══          F2 ═══        F3 ═══         ║ CRISTALERA ║
 E │  banco                   mesa+sillas   mesa+sillas   ║ AULA (F7,  ║
 L │  verde                                               ║ proyector) ║
 T │  ▢▢ papeleras    F4 ═══           F5 ═══                         │
 R │                                                   radiador        │
 O │ [TABLÓN-MENÚ]                                         [cajas]    │
   │═══════════ umbral (baldosa blanca / baldosa gris) ═══════════════│
   │                       ● JUGADOR (1,65 m)                F6 ═══   │
   └──────────────────────────────────────────────────────────────────┘
```

### Dónde vive el menú

La **pared de fieltro gris de la izquierda** es un tablón de anuncios real (Ref. A) y queda a 1–1,5 m del jugador, así que se lee bien incluso con el render a baja resolución.

- **Título:** letras de **cartulina recortada** grapadas al fieltro, como las rosas de la foto. Una letra está torcida y otra se ha caído al suelo, bajo el tablón. Es 100 % diegético y 100 % de instituto.
- **Opciones:** cada opción es un **folio A5 impreso** sujeto con una chincheta. Los folios son ligeramente distintos entre sí (rotación de ±2°, uno con cinta adhesiva).
- **Versión:** un *post-it* con clip en la esquina del tablón (`v0.1.0`), siguiendo la idea de Ref. B.
- **Fuente de luz del menú:** el fluorescente **F1**, que está sobre el tablón y funciona *casi* bien (ver G). Por eso esa zona es la más iluminada de la pantalla.

```
        ┌─ fieltro gris ───────────────────────────┐
        │   R E C R E O   E T E R N O   (cartulina) │
        │                                           │
        │   ◯╮┌───────────┐    ┌──────────────┐     │
        │  ╭─┼│  JUGAR    │    │ horario 11:29│     │  <- cartel "decorado"
        │  │ ╰└───────────┘╯   └──────────────┘     │     (sección 14)
        │   ╰─────────────╯                         │
        │     ┌────────────────┐                    │
        │     │ PARTIDA PRIVADA│                    │
        │     └────────────────┘                    │
        │     ┌──────────┐  ┌──────────┐            │
        │     │ AJUSTES  │  │ CRÉDITOS │            │
        │     └──────────┘  └──────────┘            │
        │                         ┌─────┐ post-it   │
        │      SALIR (ver abajo)  │0.1.0│           │
        └───────────────────────────────────────────┘
```

### Cada opción tiene un destino físico

Al confirmar una opción, la "cabeza" del jugador **gira o da unos pasos** (blend de Cinemachine de 0,8–1,2 s con ease-in-out) hacia un lugar real de la sala. Nunca se abre un panel flotante.

| Opción | Folio en el tablón | Destino físico al confirmar |
|---|---|---|
| **JUGAR** | "INSCRIPCIÓN — ACTIVIDAD DE TARDE" | La cámara gira hacia la **cristalera del aula** (F7 + proyector). El *matchmaking* se muestra como la lista de asistencia proyectada en la pantalla del aula. |
| **PARTIDA PRIVADA** | "RESERVA DE AULA" | Misma cristalera. En el proyector aparece un código de sala, escrito como "Aula 0·1·2·9". |
| **AJUSTES** | "NORMAS DEL CENTRO" | La cámara baja hacia la **pizarra blanca con ruedas**. Los ajustes aparecen "escritos con rotulador" y los *sliders* son líneas con un imán. |
| **CRÉDITOS** | "ORLA / LISTA DE CLASE" | Los nombres del equipo aparecen como una **lista de clase** en el tablón, en el folio junto al horario. |
| **SALIR** | Sin folio. Es la **señal verde de SALIDA** del fondo. | Aparece en el tablón como una pegatina verde pequeña. Al confirmar, la cámara mira la señal, el fluorescente F2 se apaga y el juego se cierra. |

**Mínimo viable:** si no hay tiempo para los destinos, todo puede vivir en el tablón y cada submenú se resuelve con folios nuevos que se clavan encima. Los destinos se añaden después sin cambiar la estructura.

---

## C. Sistema de selección por capas

Cada opción es un **prefab `MenuOption`** con capas independientes. Cada capa es un `GameObject` hijo propio, con su propio componente animable.

| # | Capa | Objeto en Unity | Estado normal | Estado seleccionado |
|---|---|---|---|---|
| 0 | Hitbox | `BoxCollider` invisible | — | — |
| 1 | **Base**: el folio | Quad con el material `M_Papel` y una ligera curvatura en la malla | Pegado al fieltro | Se **despega 1–1,5 cm** (z) y escala ×1,03 |
| 2 | **Texto** | TextMeshPro 3D (`TextMeshPro`, no `TextMeshProUGUI`) | Negro tinta al 85 % | Tinta al 100 %, con *jitter* de brillo de ±3 % |
| 3 | **Marco** | Cuatro trozos de cinta en las esquinas o una chincheta | Estático | La chincheta rota 5° y la cinta se levanta |
| 4 | **Aro Horario** (indicador principal) | Malla de anillo con el material `M_Glifo` | Oculto | Visible y envolviendo el folio |
| 5 | **Aguja** (indicador de movimiento) | *Sprite* o malla hija del aro | — | Hace un "tic" al llegar a su sitio (ver D) |
| 6 | **Luz** | Multiplicador de la emisión del folio + **un único** `Light` local del tablón | Base | +15 % de intensidad (la fuente es F1, ver G) |
| 7 | **Decoración** | Número de índice "01", "02"… y una marca de rotulador | Gris | Aparece un subrayado a mano |

**Reglas de animación (apartado 10):**

- La animación va por código, con *damping* exponencial (`Mathf.SmoothDamp` o `1 - exp(-k·dt)`) y **`Time.unscaledDeltaTime`**. Si el equipo lo prefiere, se puede usar DOTween (versión gratuita).
- No se usa `Animator` para la UI del menú: es más difícil de mantener y no aporta nada aquí.

| Capa | Movimiento constante | Movimiento en una transición |
|---|---|---|
| Folio | Ninguno | Se despega en 0,15 s |
| Aro | Rotación de **2–3°/s** (muy lenta) | Viaja hasta la nueva opción en 0,25 s con *overshoot* leve |
| Aguja | Ninguno | **Tic** en el último frame del viaje (un salto de 6°, como un segundero) |
| Luz | Hereda el parpadeo de F1 | +15 % en 0,2 s |
| Texto | Brillo de ±3 % con ruido lento | — |

**Sonido:** un "tic" de reloj de pared, grabado de verdad y en mono, cuando el aro llega a su sitio. Al confirmar suena una chincheta arrancada del corcho o fieltro.

---

## D. El elemento que rodea al botón: el **Aro Horario**

Es un **anillo fino formado por 12 marcas**, como la esfera de un reloj de pared escolar, **con una de las marcas ausente** y **una aguja corta** dentro.

```
              ·   ·   ·
          ·               ·
        ·                   ·        12 marcas
        ·        ╲          ·        - 1 hueco fijo (la posición del "11")
          ·        ╲      ·          aguja interior: siempre señala 11:29
              ·       ·
```

- **El hueco está siempre en la posición del 11** y la **aguja descansa apuntando a las 11:29**. Es la hora congelada del juego (apartado 14).
- El trazo del aro es **ligeramente irregular**, como dibujado con plantilla y rotulador. Se consigue con una textura de 128 px aplicada sobre una malla de anillo.
- El color es **blanco tiza cálido** (`#E8E4D8`), en el material `M_Glifo`. Es *unlit* y **no emite luz en la escena**. Es el único elemento no diegético del juego, así que tiene prohibido iluminar nada; así no rompe la regla de iluminación.
- Para cubrir distintos tamaños, el aro **no se escala**. Se reparten de nuevo las 12 marcas sobre el contorno del objeto: círculo para objetos compactos y **"estadio" (rectángulo redondeado)** para folios y puertas.

---

## E. Cómo hacerlo único y reconocible

1. **La forma es la firma.** "Aro con un hueco y una aguja" se reconoce aunque se vea pequeño, borroso o de reojo.
2. **El tic.** Siempre suena el mismo sonido de segundero. El oído aprende el glifo antes que el ojo.
3. **Conexión con el mundo.** Los **relojes del instituto tienen la misma esfera, también sin el 11**, y marcan las 11:29. Al principio parece una coincidencia de diseño. Más tarde el jugador cae en la cuenta.
4. **Nunca se usa para otra cosa.** No sirve de decoración, ni de logo, ni de cursor genérico. Solo significa *"puedes interactuar"*.
5. **Logo del juego.** El título puede incorporar el aro (una "O" con el hueco), así que la marca y la mecánica son el mismo símbolo.

---

## F. Integración del aro en el gameplay

Un único prefab, `AroHorario`, en *screen-space* proyectado sobre el objeto o en *world-space* según el caso, con estados:

| Estado | Visual | Uso |
|---|---|---|
| **Latente** | Solo 3–4 marcas tenues (20 % de opacidad) | El objeto está a menos de 4 m y es interactivo, pero no lo estás mirando |
| **Enfocado** | El aro completo se cierra con un tic | Lo estás mirando y está a tu alcance (≤ 2 m) |
| **Mantener** | **La aguja barre las marcas como un segundero** hasta completar la vuelta | Acciones de mantener pulsado: abrir taquillas, forzar puertas |
| **Bloqueado** | La aguja **vuelve atrás** a las 11:29 y el aro tiembla 2 px | Necesitas una llave u otro jugador |
| **Compañero** | Una marca extra de color del jugador (paleta de 4 colores desaturados) | En co-op, ves qué está mirando o manteniendo cada compañero |
| **Peligro / evento** | El aro aparece **sin que lo pidas** y la aguja gira al revés | Un recurso narrativo raro (máximo 2–3 veces en toda la partida) |

Ejemplos: puertas, taquillas, máquina expendedora, ordenadores, interruptores del cuadro eléctrico, selección de personaje en el *lobby* (el aro rodea al compañero) y menús de pausa (otro tablón).

**Implementación:** una interfaz `IInteractable` con `GetInteractionBounds()`. El `AroHorario` lee esos *bounds*, proyecta las cuatro esquinas a pantalla y reparte las 12 marcas. El mismo código sirve para el menú y para el juego.

---

## G. Sistema de iluminación

### Regla de oro

**Cada luz de la escena tiene un objeto que la produce.** Ese objeto tiene un material emisivo cuya emisión está sincronizada con la intensidad de la luz. Si el `Light` se apaga, el tubo se apaga.

### Paleta de fuentes

| Fuente | Color | Justificación en la escena | Tipo en Unity |
|---|---|---|---|
| Fluorescente | 4300 K + un toque de verde (`#E6F2E8`) | Las regletas de la foto | Baked o Realtime (ver abajo) |
| Señal de SALIDA | Verde `#3BD16F`, intensidad baja | Pictograma sobre la puerta del fondo (en la foto) | Mixed, pequeño rango |
| Proyector del aula | Blanco frío `#CFDDF5` | Pantalla del aula tras la cristalera (en la foto hay una pantalla amarilla) | Spot realtime con *cookie* |
| TV de pared | Azul verdoso tenue, mostrando "11:29" | La TV negra de la pared del fondo (en la foto) | Point de rango muy corto + emisivo |
| Pulsador de incendios | **Rojo**, LED diminuto | El pulsador rojo de la foto | Solo emisivo, sin `Light` |
| Luz de emergencia | Blanco, débil | Bloque autónomo junto a la salida (se añade) | Baked |

**El rojo solo existe en LEDs.** Si alguna vez queremos una zona roja (un evento), tiene que haber un objeto rojo que la produzca, como una alarma activada o una baliza.

### Asignación de los fluorescentes de la sala (patrones de la sección 4)

| ID | Posición | Estado | Comportamiento |
|---|---|---|---|
| F1 | Sobre el tablón (menú) | **A: normal**, con microvariación | 100 % ± 2 % de ruido lento. Es la luz principal del menú |
| F2 | Centro, sobre las mesas | **B: parpadeo lento** | Cada 6–14 s baja al 30 % durante 0,3–0,8 s |
| F3 | Cerca de la pizarra | **C: parcialmente apagado** | Los extremos del tubo emiten al 100 % y el centro al 0 %, con un zumbido |
| F4 | Lado del banco | **D: irregular** | Ráfagas de 2–5 cortes rápidos y luego minutos de calma |
| F5 | Junto al radiador | **E: apagado** | Crea la zona oscura que da contraste |
| F6 | Pasillo, detrás del jugador | **A: normal** | Ilumina la espalda del jugador y recorta su silueta. No se ve en el plano |
| F7 | Dentro del aula | **A: normal**, más cálido | El "otro lugar" iluminado: un aula que no debería tener la luz encendida |

**Parpadeo no sincronizado.** Cada luz lleva un componente `FluorescentFlicker` con:

- semilla aleatoria propia;
- modo (`Steady`, `SlowDip`, `Burst`, `Dead`, `Partial`);
- curva de intensidad;
- intervalo mínimo y máximo;
- referencia al `Renderer` del tubo, para sincronizar `_EmissionColor` mediante una instancia de material.

Se evalúa en `Update`, no en `Animator`. Al encenderse, un fluorescente real primero da un destello y luego se estabiliza (*overshoot* del 110 %), lo que lo hace sentir físico.

**Truco del timbre:** cada 60 s, todos los fluorescentes que funcionan bajan un 5 % durante un único frame **al mismo tiempo**. Nadie lo detecta conscientemente, pero se repite por todo el instituto (apartado 14).

### Configuración técnica (Unity 6 URP)

- **Renderer:** URP con **Forward+**, para tener muchas luces locales sin un límite de 8 por objeto.
- **GI:**
  - Los fluorescentes estables (A) son *Mixed* (Baked Indirect) y aportan el rebote horneado. Se hornea con el **Progressive GPU Lightmapper**.
  - Las luces que parpadean son *Realtime* y no proyectan sombras, salvo F2 si la cámara lo justifica.
  - **Adaptive Probe Volumes** iluminan los objetos dinámicos: jugadores, folios y sillas que se mueven.
- **Sombras:** como máximo 2–3 luces realtime con sombra visibles a la vez. Usar *soft shadows* en calidad baja; su borde algo duro ayuda al look.
- **Ambient:** el *Environment Lighting* casi a negro (`#0B0D10`). La oscuridad tiene que ser real; no compensarla con luz ambiental.
- **Rendering Layers:** el folio seleccionado y el tablón están en una capa que solo recibe F1 y la luz local del menú. Así el realce del botón no ilumina la sala.

---

## H. Reglas de materiales

1. **Un shader para todo:** `URP/Lit`, o un único Shader Graph maestro `SG_Instituto` si más adelante hace falta algo global, como el polvo. **Prohibido crear un shader por modelo.**
2. **Paleta del instituto:** una textura atlas de **32 colores** (`T_Paleta_Instituto`, 256×256). Los props sencillos (sillas, mesas, papeleras, radiador) **solo mapean sus UV a parches de la paleta**. Así todos los modelos comparten colores por construcción, con independencia de quién los haga.
3. **Texturas de detalle** (baldosa, fieltro, madera, cartón):
   - máximo **512×512**, la mayoría de 256×256;
   - filtro **Bilinear**, sin *Anisotropic*;
   - compresión estándar.
4. **Densidad de texel:**
   - **128 px/m** en arquitectura;
   - **256 px/m** en props que se ven de cerca (folios, carteles, tablón).
5. **Smoothness** (el metallic es 0 salvo que se indique):

   | Material | Smoothness | Metallic |
   |---|---|---|
   | Pintura de pared, fieltro, cartón, madera | 0,05 – 0,2 | 0 |
   | Plástico de sillas y papeleras | 0,35 | 0 |
   | **Baldosa blanca** | **0,55** (refleja los fluorescentes, como en Ref. B) | 0 |
   | Patas de las mesas, radiador | 0,4 | 0,6 |
   | Cristal | Transparente, con *smoothness* 0,9 | 0 |

6. **Sin normal maps** por defecto. Solo se permiten en baldosa, piedra de pilares y fieltro, a 256 px.
7. **Deterioro** mediante **URP Decal Projector** (renderer feature *Decal*) con un atlas compartido `T_Decals_Desgaste`: humedad, roces, cinta, marcas de silla y folios. Nada de pintar suciedad en cada textura.
8. **Saturación** de los albedos entre el **40 y el 70 %**. Los tres acentos (verde, naranja y azul) pueden llegar al 70 %. Nada al 100 %.
9. **Prohibido:** marcas reales (las cajas "Mi" pasan a tener un logo inventado), fotos de personas y texto copiado de carteles reales.

---

## I. Reglas de post-processing

Un **único Volume Profile global**, `VP_Instituto_Base`. Las zonas especiales (aula, sótano) usan Volumes locales que **solo modifican** 1–2 parámetros.

| Efecto | Dónde | Valor inicial |
|---|---|---|
| **SSAO** | Renderer Feature *Screen Space Ambient Occlusion* | Intensity 1,2 · Radius 0,35 · Direct Lighting Strength 0,35 · Samples Medium · Downsample sí |
| **Tonemapping** | Volume | **Neutral**. ACES satura demasiado los tonos medios con luz fría |
| **Color Adjustments** | Volume | Post Exposure 0 · **Contrast +18** · **Saturation −25** · Color Filter `#F2F4EE` |
| **Shadows / Midtones / Highlights** | Volume | Sombras desplazadas a verde azulado (−0,05 en rojo), luces neutras |
| **Bloom** | Volume | Threshold 1,0 · Intensity 0,35 · Scatter 0,55. Solo deben brillar los tubos y la señal |
| **Film Grain** | Volume | Type *Thin2* · Intensity 0,12 · Response 0,8 |
| **Chromatic Aberration** | Volume | **0,04** como máximo |
| **Vignette** | Volume | 0,22 · Smoothness 0,4 |
| **Resolución retro** | URP Asset | **Render Scale 0,6** + Upscaling Filter **Nearest-Neighbor**. Es la seña low-poly principal y además ahorra GPU |
| *(Opcional)* Dither / posterizado | Renderer Feature *Full Screen Pass* + Shader Graph *Fullscreen* | 6 bits por canal con dither Bayer 4×4. Empezar desactivado |

**Prueba de legibilidad:** con el Volume activo, el jugador tiene que poder leer el texto de los folios desde 1,5 m. Si no se lee, se baja el *grain* o se sube el Render Scale. Nunca se compensa aumentando la exposición.

**Accesibilidad:** en Ajustes, opciones para desactivar *grain*, aberración y parpadeo intenso (aviso de fotosensibilidad), y un *slider* de brillo que solo toca el *Post Exposure*.

---

## J. Receta para modelos futuros

```
MODELO SIMPLE → PALETA / ATLAS → URP/Lit → LUZ CON FUENTE → SSAO → SOMBRAS → VOLUME GLOBAL → LOOK DEL JUEGO
```

Checklist para cualquier modelo:

- [ ] **Escala:** 1 unidad = 1 m. Pivote en la base y centrado. Eje +Z hacia delante.
- [ ] **Presupuesto de triángulos:**

  | Tipo de objeto | Triángulos |
  |---|---|
  | Prop pequeño (papelera, extintor) | 150–400 |
  | Silla | 300–600 |
  | Mesa | 200–500 |
  | Taquilla o armario | 400–900 |
  | Máquina expendedora | 1500–3000 |
  | Personaje | 3000–6000 |

- [ ] **Biseles:** un único segmento de bisel en las aristas que reciben luz. Normales duras por encima de 45°.
- [ ] **Materiales:** uno por prop siempre que se pueda, o dos como máximo (paleta + detalle).
- [ ] **UV:** UV0 para la textura o la paleta. **UV1 para lightmap** si es estático. También se puede marcar *Generate Lightmap UVs* en el importador.
- [ ] **Densidad de texel** según H.4.
- [ ] **Interactividad:** si es interactivo, implementa `IInteractable` y define unos *bounds* para el Aro Horario.
- [ ] **Luz:** si emite luz, lleva su componente de luz y su emisivo sincronizado (G).
- [ ] **Validación:** se comprueba en la **escena de validación** `Scene_LookDev`, con tres luces fijas, el Volume global, una silla, una taquilla y una persona de referencia de 1,70 m. Si el modelo "desentona" ahí, se corrige el modelo, nunca el Volume.
- [ ] **Nombres:** `SM_Silla_Naranja_01`, `M_Silla_Naranja`, `T_Silla_Naranja_D`.

---

## K. Cómo saber inmediatamente dónde mirar

1. **Jerarquía de luminancia 10 : 3 : 1.** El tablón (folio seleccionado) es lo más brillante. La cristalera del aula es el segundo punto (≈30 %). El resto de la sala queda en torno al 10 %.
2. **Oscuridad junto al foco.** F5 está apagado y el tramo entre el jugador y el tablón queda más oscuro, así que el folio se recorta.
3. **Las líneas de fuga** del techo, las regletas y las juntas de las baldosas convergen hacia el fondo. El tablón está en primer plano a la izquierda (tercio izquierdo) y equilibra la composición con la cristalera (tercio derecho).
4. **Calor contra frío.** El folio blanco cálido, bajo un fluorescente frío, destaca sobre el fieltro gris.
5. **Movimiento controlado.** El único movimiento continuo cerca del foco es el aro (2–3°/s). Los parpadeos fuertes (F4) quedan **en la periferia** y son raros, para que no roben la mirada al menú.
6. **Profundidad de campo:** no se usa. Es innecesaria y cara con un render a baja resolución.

---

## L. Qué conservar de la fotografía

- **La posición de cámara** de la foto: el umbral gris/blanco, mirando en diagonal hacia el fondo.
- **El techo bajo con vigas** y las **regletas fluorescentes lineales**, que son la firma de la sala.
- La **baldosa blanca grande** y la **franja de baldosa gris** del pasillo.
- La **pared de fieltro gris** como tablón del menú y las **letras de cartulina**.
- El **banco verde**, las **papeleras de reciclaje** (azul y amarilla), los **pilares de piedra**, las **mesas de patas verdes**, las **sillas naranjas y negras**, la **pizarra blanca con ruedas**, el **radiador**, los **armarios de madera con cinta** y las **cajas apiladas**.
- El **extintor**, la **señal de salida**, el **pulsador de incendios** y la **TV de pared**: todos son fuentes de luz o de color justificadas.
- La **cristalera hacia el aula**, que es el segundo punto focal.

## M. Qué modificar o eliminar

- **Eliminar a todas las personas** del aula. Es una cuestión de privacidad y además el vacío es el terror.
- **Eliminar marcas y textos reales:** "Mi / mirplay.com", carteles con logos institucionales y nombres. Se sustituyen por inventados.
- **Corregir la distorsión** de ojo de pez: la cámara del juego tiene un FOV de unos 62°.
- **Reducir los carteles a 6–8 legibles**, uno de ellos el **horario con fecha** (apartado 14).
- **Añadir:**
  - 1 o 2 relojes de pared (sin el 11, marcando 11:29);
  - una mochila olvidada;
  - una silla girada hacia la pared;
  - folios sobre una mesa (no esparcidos por todo el suelo);
  - una luz de emergencia.
- **Oscurecer:** se apagan las ventanas que den al exterior y se baja la iluminación ambiental. La foto está sobreexpuesta y es completamente diurna.
- **No añadir:** sangre, grafitis, destrozos ni nada de lo descartado en el apartado 0.

---

## Elemento identitario: "atrapado a las 11:29"

Debe ser sutil, con la progresión *primero coincidencia, luego patrón*:

- Los **relojes** marcan siempre las 11:29 y les falta el 11, igual que al Aro Horario.
- La **TV de pared y la pantalla del proyector** muestran "11:29" con aspecto de *standby*.
- El **horario** del tablón tiene la misma fecha en todas las zonas, *jueves 14 de marzo*, con la franja 11:00–11:30 marcada "RECREO" y tachada a boli.
- **Siempre hay una mochila azul** en la misma posición relativa: junto a la tercera silla de la segunda mesa de cada sala.
- **Pulso del timbre:** la bajada sincronizada del 5 % cada 60 s (G).
- **Un mismo folio** ("CIRCULAR 11/29") aparece en tablones de zonas distintas.

---

## N. Implementación viable para un equipo pequeño

| Parte | Coste | Notas |
|---|---|---|
| Escena del menú con *blockout* a partir de la foto | Bajo | ProBuilder (paquete oficial) para muros, techo y vigas |
| Volume global y SSAO | Muy bajo | Un perfil compartido. Se configura una vez |
| Render Scale 0,6 + Nearest-Neighbor | Muy bajo | Un parámetro del URP Asset |
| `FluorescentFlicker` | Bajo | Unas 80 líneas de C#, reutilizable en todo el juego |
| Folios con TextMeshPro 3D, capas y animación por código | Bajo–medio | Un prefab `MenuOption` y un `MenuController` con la navegación |
| Aro Horario (malla + 12 marcas + aguja) | Medio | Un prefab y un script que sirven tanto en el menú como en el juego |
| Blends de cámara entre destinos (Cinemachine 3) | Bajo | Una `CinemachineCamera` por destino, cambiando prioridades |
| Paleta atlas y Decal Projector | Bajo | Decisión de pipeline más que de código |
| Lightmaps + APV | Medio | Exige disciplina: objetos estáticos marcados y UV2 |
| Input (teclado, ratón y mando) | Bajo | **Input System** con *actions* `Navigate`, `Submit` y `Cancel` |

## O. Demasiado costoso (evitar o aplazar)

- Volumétricos o niebla volumétrica real. Si hace falta niebla, se usa la *Fog* lineal de Unity y algún *quad* de polvo con alpha.
- **SSR**, ray tracing, GI en tiempo real o cambiar a **HDRP**.
- Un shader distinto por objeto, o un *toon shader* propio con *outlines*.
- Texturas fotográficas de alta resolución, *photoscan* o normal maps en todo.
- Simulación física de folios o tela en el menú. Se resuelven con animación procedural sencilla: un seno sobre unos pocos vértices o un `Transform`.
- Partículas complejas y destrucción.
- UI Toolkit en *world space*. En 6000.0 aún no está disponible de forma estable; para el menú diegético usamos **TextMeshPro 3D / UGUI World Space Canvas**.
- Más de unas 6 luces realtime con sombras visibles a la vez.

---

## Estado de implementación

El código de este documento vive en `Assets/StreetLarpersInstituto/` (ver su `README.md`):

- [x] `InstitutoClock` (11:29, fecha, pulso del timbre) y `FrozenClock`.
- [x] `FluorescentFlicker` con los modos A–E y la emisión sincronizada con la luz.
- [x] `AroHorario` con sus estados (Hidden, Latent, Focused, Hold, Locked, Danger), el tic y la forma estadio/círculo.
- [x] `MenuOption` por capas, `MenuController` (teclado, mando y ratón) y `MenuCameraRig`.
- [x] Builder de la escena del menú a partir de la foto, materiales, Volume Profile y Lighting Settings.
- [ ] Submenús reales: lobby en el proyector, ajustes en la pizarra y lista de clase.
- [ ] Adaptador de gameplay `IInteractable` → `AroHorario` en screen-space.
- [ ] Sonidos grabados (tic de reloj, chincheta) y la fuente tipográfica definitiva.

## Próximos pasos sugeridos

1. Blockout de la sala en ProBuilder, con la cámara a 1,65 m en el umbral.
2. Configurar el URP Asset, el renderer (SSAO, Decals, Forward+) y `VP_Instituto_Base`.
3. Preparar `Scene_LookDev` con la silla naranja como primer modelo de prueba de la receta (J).
4. Crear los prefabs `FluorescentFlicker`, `MenuOption` y `AroHorario`.
5. Validar la legibilidad del tablón con el render a 0,6.
