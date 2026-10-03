# Plan de completitud CRUD — Catálogos LigaMXCore

Este documento registra el trabajo ya realizado sobre los catálogos CRUD y el plan
para el resto, siguiendo el mismo patrón. Se originó al detectar que la mayoría de
los catálogos solo implementan `Index`/`Add`/`Edit`, sin `Delete` ni `Details`, y sin
validaciones de datos más allá de lo mínimo.

## Patrón establecido (implementado en País, a replicar en el resto)

1. **`Details`**: vista de solo lectura + tabla "Catálogos dependientes" con
   **conteo por catálogo** (`CountAsync`), nunca el listado completo de registros
   dependientes — para que catálogos con dependencias amplias (ej. `JornadaPartido`)
   no carguen miles de filas solo para mostrarlas.
2. **`Delete` (GET)**: si hay dependientes (`suma de conteos > 0`), la vista bloquea
   la acción (no muestra formulario ni token) y solo permite "Cancelar". Si no hay
   dependientes, muestra confirmación normal.
3. **`Delete` (POST/`DeleteConfirmed`)**: **repite la validación de dependientes en
   servidor** (nunca confiar solo en la UI) antes de intentar `SaveChangesAsync()`.
4. **Método privado `ObtenerDependencias(id)`**: devuelve `Dictionary<string,int>`
   con una entrada por catálogo dependiente. Es el único lugar que hay que tocar al
   agregar un nuevo tipo de dependencia.
5. **Validaciones de dato en el modelo** (`DataAnnotations`):
   - `[Required]`
   - `[StringLength(max, MinimumLength = min)]`
   - `[RegularExpression]` restringiendo a letras/acentos/espacios y puntuación básica
     según el campo (ver detalle por catálogo abajo).
6. **Duplicidad de nombres**: no se resuelve con `DataAnnotations` (requiere consultar
   la base). Se agrega un método privado `ValidarNombreDuplicado` en el controlador,
   comparación *case-insensitive* + `Trim()`, excluyendo el propio registro en `Edit`.
7. **Verificación siempre en runtime**, no solo compilación: build + levantar la app +
   probar cada acción contra datos reales (incluyendo el caso bloqueado y el caso
   permitido), y probar el `POST` de `Delete` directamente con un token de otra
   página para confirmar que el bloqueo no depende solo de la UI. Usar registros de
   prueba (crear/borrar) en vez de tocar datos reales cuando sea posible.

## Mapa de dependencias entre catálogos

Construido a partir de las relaciones declaradas en `ApplicationDbContext` y las FK
de cada modelo. Cada fila indica qué catálogo(s) hay que consultar en
`ObtenerDependencias` antes de permitir borrar.

| Catálogo | Bloqueado para Delete por... |
|---|---|
| **Pais** | `Estado.PaisId` ✅ *(implementado)* |
| **Estado** | `Municipio.EstadoId` ✅ *(implementado)* |
| **Municipio** | `Equipo.MunicipioId`, `Estadio.MunicipioId` ✅ *(implementado)* |
| **Equipo** | `Partido.EquipoLocalId`, `Partido.EquipoVisitaId` (ambas direcciones) |
| **Estadio** | `JornadaPartido.EstadioId` |
| **Partido** | `JornadaPartido.PartidoId` |
| **Temporada** | `Jornada.TemporadaId` |
| **Jornada** | `JornadaPartido.JornadaId`, `JornadaPronostico.JornadaId` |
| **EstatusPartido** | `JornadaPartido.EstatusPartidoId` |
| **TipoResultado** | `JornadaPartido.TipoResultadoId`, `JornadaPronosticoDetalle.TipoResultadoId` |
| **Participante** | `JornadaPronostico.ParticipanteId` |
| **EstatusJornada** | *(ninguno — ver hallazgo abajo)* |
| **Usuario** | *(ninguno — catálogo legacy, standalone)* |

**Hallazgo importante**: `EstatusJornada` no está referenciado por ninguna FK en todo
el modelo (`Jornada` no tiene `EstatusJornadaId`). Es un catálogo huérfano — antes de
invertir en su CRUD completo, vale la pena confirmar si se planea conectarlo a
`Jornada` o si debe descartarse.

## Estado actual

### ✅ Completado

| Catálogo | Index | Add | Edit | Details | Delete | Validaciones (longitud/caracteres/duplicidad) |
|---|---|---|---|---|---|---|
| **Pais** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Estado** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad acotada por país)* |
| **Municipio** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad acotada por estado; catálogo construido desde cero — no existía)* |
| **Temporada** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad global de nombre; permite dígitos/guion)* |
| **TipoResultado** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad global de nombre; solo letras/espacios)* |
| **EstatusPartido** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad global de nombre; solo letras/espacios)* |
| **EstatusJornada** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(ya no es huérfano — vinculado a `Jornada.EstatusJornadaId`)* |
| **Participante** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(sin duplicidad de nombre completo, por diseño)* |
| **Equipo** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad global de nombre; validaciones de Alias/Logo ajustadas a datos reales)* |
| **Estadio** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(duplicidad global de nombre; CP de 5 dígitos)* |
| **Partido** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(equipo local ≠ visitante; sin pares local/visitante duplicados)* |
| **Jornada** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(Orden único dentro de la misma Temporada)* |
| **JornadaPartido** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ *(partido único en todo el fixture; equipo sin duplicarse en la misma jornada)* |

Estado y Municipio se implementaron en paralelo con dos agentes independientes
(cada uno en su propio git worktree), fusionados sin conflictos porque no
comparten archivos (salvo una línea de navegación que Municipio agregó en
`Views/Home/Index.cshtml`). Ambos fueron verificados en runtime contra los
datos reales (`liga.db`), incluyendo el caso de bloqueo forzando un `POST`
directo con un token antiforgery tomado de otra página.

Temporada y TipoResultado (Fase 1, sin Usuario) se completaron siguiendo el
mismo patrón que Pais (catálogo raíz sin FK propia). Verificados en runtime
contra `liga.db`: creación y borrado de registros de prueba, bloqueo de
`Delete` en `Temporada` id=1 (tiene `Jornada` dependiente) confirmado también
por `POST` directo con token de otra página, y validaciones de longitud,
regex y duplicidad probadas para ambos catálogos. `TipoResultado` no tiene
todavía `JornadaPartido`/`JornadaPronosticoDetalle` en la base (captura de
partidos aún no implementada), por lo que su bloqueo por dependencias no
pudo probarse con datos reales — la lógica de conteo es idéntica en
estructura a la ya validada en Estado/Municipio/Temporada.

Fase 2 (EstatusPartido, EstatusJornada, Participante) verificada en runtime
contra `liga.db`: creación/borrado de registros de prueba, validaciones de
longitud/regex/duplicidad, y el caso más importante — `EstatusJornada` id=1
("Pendiente", 17 jornadas asociadas) bloqueado tanto en el GET como en un
`POST` directo con token de otra página, mientras que id=2 ("En Curso", sin
jornadas) permite el borrado con confirmación normal.

Fase 4 (Equipo, Estadio) verificada en runtime contra `liga.db`. Las
validaciones sugeridas originalmente en el plan para `Alias` (longitud 2–20,
sin caracteres especiales) no coincidían con los datos reales — los alias de
equipos/estadios llegan a 42 caracteres y usan "/" (ej. "Las Chivas / El
Rebaño Sagrado", "Estadio Azteca / El Coloso de Santa Úrsula`) — se ajustó a
longitud 2–50 permitiendo letras/números/espacios/`/.,'-`. Confirmado que
todos los registros reales (incluyendo esos alias largos) siguen pasando la
validación tras un `Edit` sin cambios. Bloqueo de `Delete` probado con caso
real: `Equipo` id=1 (América, 17 `Partido` como local y 17 como visita)
bloqueado; `Estadio` no tiene aún `JornadaPartido` en la base así que ese
bloqueo no se pudo probar con datos reales (mismo caso que TipoResultado/
EstatusPartido en fases previas). Se corrigieron dos bugs cosméticos
preexistentes de copy-paste: título "Editar Estado" en `Estadio/Edit.cshtml`
y "Editar País" en `EstatusJornada/Edit.cshtml`.

`Partido` (parte de Fase 5, completado por separado) verificado en runtime:
los 306 registros reales (18 equipos × 17 rivales, round-robin completo local/
visita) se confirmó que no tienen auto-enfrentamientos ni pares duplicados
antes de aplicar las validaciones nuevas. Se agregó validación cruzada
`EquipoLocalId != EquipoVisitaId` y de pareja duplicada (ningún otro `Partido`
con el mismo local+visitante) en el controlador, ya que `DataAnnotations` no
puede comparar dos propiedades del mismo modelo sin una lógica adicional.
Como el calendario ya está saturado (los 306 pares posibles ya existen), la
ruta de éxito de `Add` se probó con un ciclo borrar→volver a crear sobre un
registro real (`Necaxa vs Monterrey`, `PartidoId` 306), confirmando que
SQLite reutiliza el mismo id y el estado queda idéntico al original.
`Details`/`Delete` cuentan `JornadaPartido.PartidoId` — aún en cero, mismo
caso que otros catálogos a la espera de la captura de partidos.

`Jornada` (parte de Fase 5) verificado en runtime contra las 17 jornadas
reales (todas en Temporada 1, sin duplicados de `Orden`). Se agregó
`[Range(1, int.MaxValue)]` a `Orden` y validación de duplicidad compuesta
`(TemporadaId, Orden)` en el controlador (no se puede resolver solo con
`DataAnnotations`). Probado: orden duplicado bloqueado, orden negativo
bloqueado por `Range`, nombre corto bloqueado por `StringLength`, y ciclo
completo Add→Details→Delete con un registro de prueba. `JornadaPartido` y
`JornadaPronostico` siguen en cero, así que el bloqueo real de `Delete` por
dependientes no se pudo probar con datos reales — misma limitación que en
`Partido`/`Estadio`.

**`JornadaPartido` (Fase 6) completado.** Hallazgo antes de empezar: la vista
`Index.cshtml` ya tenía enlaces a `Add`/`Edit`, pero esas acciones **no
existían** en el controlador (solo `Index` y `UpdateScores`) — CLAUDE.md
documentaba mal este catálogo como "Index/Add/Edit sin Details/Delete"; en
realidad no tenía ni Add ni Edit reales. Se construyó el CRUD completo desde
cero. Decisión de producto tomada con el usuario: un `JornadaPartido` nuevo
(partido programado pero no jugado) se crea con `GolLocal=0`, `GolVisita=0` y
`TipoResultadoId` = "Empate" por defecto (no se agregó un valor "Pendiente"
al catálogo `TipoResultado` ni se hizo la columna nullable). El marcador y el
resultado real se siguen capturando exclusivamente por el flujo existente
`UpdateScores` (grid de `Index`), no por el formulario `Add`/`Edit` — que solo
programa Jornada/Partido/Estadio/Estatus.

Validaciones agregadas en el controlador (no posibles con `DataAnnotations`
puro):
- Un `Partido` del catálogo (306 posibles) solo puede quedar programado una
  vez en **todo** el fixture (no solo dentro de la misma Jornada) — el
  dropdown de `Partido` en Add/Edit además excluye los ya programados.
- Ningún equipo puede tener dos partidos en la misma Jornada (se valida
  cruzando `EquipoLocalId`/`EquipoVisitaId` de los `Partido` ya asignados a
  esa Jornada).

**Cuidado con `_context.Update()` en Edit**: como el formulario de Edit no
incluye `GolLocal`/`GolVisita`/`TipoResultadoId` en el `[Bind]`, usar
`_context.Update(objetoBindeado)` habría sobrescrito esos campos con sus
valores CLR por defecto (`null`/`null`/`0`) al guardar. Se evitó cargando la
entidad rastreada con `FindAsync` y modificando solo los 4 campos del
formulario antes de `SaveChangesAsync` — mismo patrón que ya usaba
`UpdateScores`. Verificado explícitamente en runtime: se editó Jornada/
Estadio/Estatus de un registro y el marcador 0-0 se mantuvo intacto.

Probado en runtime con un registro de prueba (creado y eliminado al
finalizar, `liga.db` queda en 0 filas de `JornadaPartido` como al inicio):
ciclo Add→Details→Edit→Delete completo, bloqueo de partido duplicado en todo
el fixture, bloqueo de equipo duplicado en la misma jornada, y que el
dropdown de `Partido` excluye correctamente los ya usados.

### Carga de datos reales — Clausura 2026 (fase regular completa)

Se cargaron los 153 partidos reales de la fase regular del Clausura 2026
(17 jornadas × 9 partidos) en `JornadaPartido`, consultados de Wikipedia
(`Anexo:Torneo Clausura 2026 (México) - Fase regular`, vía la API de
MediaWiki sección por sección — el fetch normal de la página se truncaba
antes de la Jornada 4).

- **Script generador**: `Scripts/gen_jornadapartido.py` — contiene los 153
  partidos (jornada, local, marcador, visita, estadio) como datos hardcoded,
  resuelve los ids reales contra `liga.db` (`Equipo`, `Estadio`, `Jornada`,
  `Partido`, `EstatusPartido`, `TipoResultado`) **sin escribir nada**, valida
  que las 153 filas resuelvan sin ambigüedad (sin partidos repetidos, sin
  nombres sin mapear) y escribe el resultado en
  `Scripts/clausura2026_jornadapartido.sql`.
- **Mapeo de nombres**: los nombres de equipos/estadios de la fuente no
  coinciden literalmente con el catálogo (ej. "Atlético de San Luis" →
  "Atlético San Luis", "Toluca" → "Deportivo Toluca", "Juárez" →
  "FC Juárez", "Mazatlán" → "Mazatlán FC") — ver diccionarios `EQUIPO_MAP`/
  `ESTADIO_MAP` en el script.
- **Caso especial de estadio**: "Estadio Ciudad de los Deportes" (sede real y
  alterna de América mientras se remodelaba su estadio habitual para el
  Mundial 2026) no existe en nuestro catálogo de 17 estadios (para 18
  equipos) — se mapeó al único estadio de América en el catálogo
  (`Estadio Banorte`).
- **Estadio compartido real**: los datos muestran que Cruz Azul y Puebla
  comparten sede ("Estadio Cuauhtémoc") durante este torneo — es correcto
  tal cual, `EstadioId` es un campo por partido en `JornadaPartido`, no una
  propiedad fija del equipo, así que no requirió ningún ajuste de esquema.
- **Resultado real, no el default de partido no jugado**: a diferencia del
  flujo `Add` normal (marcador 0-0 / "Empate" para partidos futuros, ver
  arriba), esta carga usa el marcador y `TipoResultadoId` **reales** de cada
  partido (calculado por comparación de goles) y `EstatusPartidoId` =
  "Finalizado" para las 153 filas.
- **Verificado en runtime** tras aplicar el script: 9 partidos por jornada en
  las 17 jornadas, cada uno de los 18 equipos con exactamente 17 apariciones
  (local + visita) en todo el torneo, distribución de resultados 69
  victorias de local / 45 de visita / 39 empates, y una revisión visual en
  `/JornadaPartido/Details/1` confirmando que coincide con el resultado real
  (Mazatlán 1-2 Juárez, Estadio El Encanto).
- Respaldo de `liga.db` antes de aplicar el script:
  `liga.db.bak-20260928174923-preJornadaPartido`.

### ⏳ Pendiente

- **Usuario**: catálogo legacy paralelo a Identity — pendiente decidir con el
  usuario si se completa su CRUD o se marca obsoleto (ver Fase 1 arriba).

## Plan de trabajo futuro, por fases

Orden pensado de "raíz" (menos dependencias que resolver) hacia "hoja" (más
dependencias), para poder probar cada `Delete` con casos reales sencillos antes de
llegar a los catálogos más enredados.

### Fase 1 — Catálogos raíz sin Add todavía ✅ Completada (excepto Usuario)

| Catálogo | Falta | Validaciones sugeridas |
|---|---|---|
| **Temporada** | ✅ `Add`, `Details`, `Delete` | `TemporadaNombre`: requerido, longitud 3–50, sin caracteres especiales salvo espacios/guion (ej. "2026-2027"); permite dígitos aquí (a diferencia de País) porque los nombres de temporada son años. `Comentarios`: opcional, longitud máx. 500, sin restricción de caracteres (es texto libre). Duplicidad global por nombre. |
| **TipoResultado** | ✅ `Add`, `Details`, `Delete` | `TipoResultadoNombre`: requerido, longitud 3–30, solo letras/espacios. Duplicidad global por nombre. |
| **Usuario** | `Add`, `Details`, `Delete` — **pendiente, aplazado a fase próxima por decisión del usuario** | Catálogo legacy paralelo a Identity — antes de invertir, decidir con el usuario si se completa o se marca obsoleto. Si se completa: `UsuarioNombre` requerido y único; ocultar `Password` en texto plano en `Index`/`Details` (hoy se muestra sin enmascarar). |

### Fase 2 — Catálogos raíz con CRUD ya completo ✅ Completada

| Catálogo | Validaciones aplicadas |
|---|---|
| **EstatusPartido** | `EstatusPartidoNombre`: requerido, longitud 3–30, solo letras/espacios. Duplicidad global. `Details`/`Delete` agregados (dependiente: `JornadaPartido.EstatusPartidoId`). |
| **EstatusJornada** | Mismo criterio que EstatusPartido. **Se resolvió el hallazgo de catálogo huérfano**: se agregó `EstatusJornadaId` a `Jornada` (FK real a nivel SQLite, ver nota de esquema abajo), con backfill a "Pendiente" (id=1) para las 17 jornadas existentes. `Details`/`Delete` ahora validan dependientes reales (`Jornada.EstatusJornadaId`), verificado con datos reales: id=1 (con jornadas) bloqueado, id=2 (sin jornadas) permitido. `JornadaController` (Add/Edit/Index) actualizado con el nuevo dropdown/columna. |
| **Participante** | `Nombres`, `ApellidoPaterno`, `ApellidoMaterno`: requeridos, longitud 2–50 cada uno, solo letras/acentos/espacios. Sin duplicidad estricta de nombre completo (dos personas pueden compartir nombre), tal como estaba previsto. `Details`/`Delete` agregados (dependiente: `JornadaPronostico.ParticipanteId`). |

**Nota de esquema — `liga.db` no se gestiona vía EF Core migrations**: se detectó que
`liga.db` no tiene tabla `__EFMigrationsHistory` (las tablas de negocio se crearon con
SQL directo, fuera de EF; solo `identity.db` usa migraciones, y ahí solo viven las
tablas de Identity). `dotnet ef migrations add` generado para este cambio intentaba
recrear *todo* el esquema desde cero — se descartó (`dotnet ef migrations remove`) sin
aplicarlo. El cambio real (agregar `EstatusJornadaId` a `Jornada` con `FOREIGN KEY`) se
aplicó recreando la tabla directamente por SQL (SQLite no soporta `ADD COLUMN` con FK),
con respaldo previo de `liga.db` (`liga.db.bak-20260928164255`). **Cualquier cambio de
esquema futuro sobre catálogos de negocio debe seguir este mismo patrón manual, no
`dotnet ef database update`**, hasta que se decida formalizar las migraciones para
`liga.db`.

### Fase 3 — Catálogos con dependencia de un nivel ✅ Completada

| Catálogo | Depende de | Validaciones aplicadas |
|---|---|---|
| **Estado** | Pais | `EstadoNombre`: requerido, longitud 3–100, regex letras/acentos/espacios. Duplicidad de nombre **dentro del mismo país**. |
| **Municipio** | Estado | `MunicipioNombre`: requerido, longitud 3–80, letras/acentos/espacios. Duplicidad dentro del mismo Estado. Catálogo construido desde cero (no tenía controlador ni vistas). |

### Fase 4 — Catálogos con dependencia de dos niveles ✅ Completada

| Catálogo | Depende de | Validaciones aplicadas |
|---|---|---|
| **Equipo** | Municipio (→ Estado → Pais) | `EquipoNombre`: requerido, longitud 3–60. `Alias`: opcional, longitud 2–50 (ajustado de la sugerencia original 2–20 tras revisar datos reales: hasta 42 caracteres, incluye "/"), letras/números/espacios/`/.,'-`. `EquipoLogo`: opcional, `StringLength(255)` (todos los registros reales están en `NULL` hoy). Duplicidad de `EquipoNombre` global. `Details`/`Delete` cuentan `Partido` como local y como visita por separado. |
| **Estadio** | Municipio | `EstadioNombre`: longitud 3–80. `Alias`: mismo criterio que Equipo (hasta 42 caracteres reales). `Direccion`: `StringLength(200)`. `CodigoPostal`: `[RegularExpression(@"^\d{5}$")]`, verificado que los 17 registros reales ya cumplen el formato. Duplicidad de nombre. `Details`/`Delete` cuentan `JornadaPartido` asociados (aún en cero, no hay captura de partidos). |

### Fase 5 — Núcleo de calendario ✅ Completada

| Catálogo | Depende de | Validaciones aplicadas |
|---|---|---|
| **Partido** | Equipo (local/visita) | `Details`/`Delete` cuentan `JornadaPartido.PartidoId`. Validación cruzada `EquipoLocalId != EquipoVisitaId` y de pareja local/visitante duplicada, ambas en el controlador (no se puede con `DataAnnotations` puro). Verificado con los 306 registros reales del calendario completo — ver detalle en "Estado actual" arriba. |
| **Jornada** | Temporada, EstatusJornada | `JornadaNombre`: requerido, longitud 3–50. `Orden`: `[Range(1, int.MaxValue)]`, único dentro de la misma Temporada (validación compuesta en el controlador). `Details`/`Delete` cuentan `JornadaPartido`/`JornadaPronostico` asociados. Verificado con las 17 jornadas reales — ver detalle en "Estado actual" arriba. |

### Fase 6 — Fixture ✅ Completada

| Catálogo | Validaciones aplicadas |
|---|---|
| **JornadaPartido** | Se resolvió la pregunta abierta de "¿cómo se puebla?": selección manual de `Partido`/`Jornada`/`Estadio`/`EstatusPartido` vía dropdowns en `Add`/`Edit` (no generación automática). Decisión de producto sobre partidos no jugados: `GolLocal=0`, `GolVisita=0`, `TipoResultadoId`="Empate" por defecto, sin cambios de esquema. Validado que un `Partido` solo puede programarse una vez en todo el fixture y que un equipo no puede tener dos partidos en la misma Jornada. `Delete` solo quita la asignación a la Jornada (no toca el catálogo `Partido`), cuenta `JornadaPronosticoDetalle.JornadaPartidoId` como dependiente. Ver detalle completo, incluyendo el cuidado con `_context.Update()` para no sobrescribir el marcador, en "Estado actual" arriba. |

## Consideraciones transversales para todas las fases

- **Índices `UNIQUE` a nivel de base de datos**: hoy ningún catálogo los tiene; toda
  la protección contra duplicados vive en el código del controlador. Recomendado
  agregarlos como respaldo, pero no depender solo de ellos (SQLite no da mensajes de
  error amigables para mostrarlos en la UI).
- **Mensajes de error en español, consistentes** con el estilo ya usado en País
  (`"El nombre del país es obligatorio."`, `"Ya existe un país con ese nombre."`).
- **No usar `Include()` para contar dependientes** — siempre `CountAsync`/`AnyAsync`
  filtrando por FK, para que el patrón siga funcionando cuando el volumen de datos
  crezca (ej. cientos de `JornadaPartido`).
- **Probar en runtime cada catálogo** antes de darlo por terminado, no solo verificar
  que compila — el caso de la codificación de acentos en la prueba de País mostró que
  hay diferencias reales entre "compila bien" y "funciona bien".

## Ajustes de UI — Index simplificado (columnas ocultas)

A petición del usuario, se simplificaron las columnas mostradas en `Index` de todos
los catálogos con listado. La regla general adoptada:

- **El Id nunca se muestra en `Index`** en ningún catálogo (sí sigue disponible en
  `Details`, y los enlaces de Editar/Eliminar/Detalles lo siguen usando internamente
  vía `asp-route-id`, que no depende de que la columna sea visible).
- Además del Id, se ocultaron columnas específicas en tres catálogos, moviéndolas
  exclusivamente a `Details` (que ya las mostraba desde que se construyó, sin
  cambios ahí):
  - **Estadio**: se ocultan `Dirección`, `CP` y `Municipio`. `Index` solo muestra
    Nombre y Alias.
  - **Equipo**: se ocultan `Municipio` y `Alias`. `Index` solo muestra Nombre y Logo.
  - **Jornada**: se ocultan `Orden` y `Estatus`. `Index` solo muestra Temporada y
    Jornada.

Catálogos afectados por la ocultación de Id (los 13 con listado propio — `Home` no
es catálogo y `JornadaPartido` no mostraba un Id visible, solo lo usaba como
`data-id` del `<tr>` para el flujo `UpdateScores`, así que no requirió cambio):
`Pais`, `Estado`, `Municipio`, `Temporada`, `TipoResultado`, `EstatusPartido`,
`EstatusJornada`, `Participante`, `Equipo`, `Estadio`, `Partido`, `Jornada`,
`Usuario`.

Verificado en runtime: las 14 vistas `Index` cargan sin error (200), los
encabezados y las filas de datos muestran exactamente las columnas esperadas para
cada catálogo, los botones de acción siguen funcionando, y `Details` conserva
todos los campos removidos de `Index` en Estadio/Equipo/Jornada.

## Mejora de UX — Filtros de listado en catálogos con volumen (Fase A)

Motivación: varios catálogos están pensados para crecer bastante más allá de
los datos actuales (ej. `Municipio` con ~2,500 municipios reales en México,
`JornadaPartido` acumulando temporada tras temporada) y hoy su `Index` es una
lista plana sin forma de acotarla. Se evaluaron librerías externas (Tom Select,
Choices.js, Select2, DataTables, X.PagedList.Mvc.Core — todas MIT/gratuitas y
compatibles con LibMan vía cdnjs) para fases posteriores (B/C: combos con
búsqueda, paginación), pero **la Fase A se implementa sin agregar ninguna
dependencia nueva**: combos nativos de Bootstrap + handlers puntuales en
`ligaMxCore.js`, consistente con el resto del proyecto.

Alcance de la Fase A, aplicado a **5 catálogos**: `Estado`, `Municipio`,
`Partido`, `Jornada` y `JornadaPartido`. Sin paginación todavía (Fase B, cuando
el volumen de datos lo justifique).

### Patrón general (aplica a los 5 catálogos)

1. **Controlador — soporte de filtros en `Index`**: agregar parámetros
   opcionales por query string (uno por cada FK/campo de texto relevante del
   catálogo — ver tabla abajo) y aplicar `Where` condicionales (solo cuando el
   parámetro venga informado) sobre el query existente. El texto libre usa
   `Contains` case-insensitive + `Trim()`, mismo criterio que ya usan los
   métodos `ValidarNombreDuplicado`.
2. **Combos de filtro**: cada uno se puebla con un `SelectList` de todas las
   opciones más "Todos", marcando como seleccionado el valor recibido por
   query string (para que el formulario no se "resetee" visualmente al
   filtrar).
3. **Cascada solo donde hay jerarquía real entre dos combos de filtro** (ver
   columna "Cascada" en la tabla): un endpoint `GET` nuevo que devuelve JSON
   `[{ id, nombre }]` con las opciones del combo hijo dado el combo padre, más
   un handler en `ligaMxCore.js` que lo consume al cambiar el combo padre. Sin
   `[HttpPost]` ni antiforgery (solo lectura). Sin cascada entre combos que son
   ejes independientes (ej. Temporada y Estatus en Jornada).
4. **Vista `Index`**: `<form method="get" asp-action="Index">` arriba de la
   tabla existente, con los combos/input de texto del catálogo, botón
   "Filtrar" y enlace "Limpiar filtros" (`asp-action="Index"` sin query
   string). Sin auto-submit al cambiar combos (consistente con que el resto
   del proyecto no usa auto-submit en ningún formulario). La estructura de la
   tabla de resultados no cambia (respeta "Ajustes de UI" ya aplicado).
5. **Casos borde**: combinación de filtros sin jerarquía real entre sí (ej.
   `estadoId` que no pertenece al `paisId` filtrado, si se edita el query
   string a mano) no requiere manejo especial — el `Where` compuesto
   simplemente no devuelve filas. Texto vacío/solo espacios no aplica filtro.
6. **Verificación en runtime por catálogo**, sin dejar datos de prueba (es
   funcionalidad de solo lectura sobre datos ya existentes): sin filtros (lista
   completa actual), cada filtro por separado, combinaciones, cascada (si
   aplica), "Limpiar filtros", caso sin resultados, y que los enlaces
   Detalles/Editar/Eliminar sigan funcionando sobre filas filtradas.
7. **Documentación al finalizar cada catálogo**: marcar su fila como
   completada en la tabla de abajo con el resultado de la verificación, y — si
   el patrón de cascada se reutiliza — dejar la convención anotada en
   "Convenciones de UI" de `CLAUDE.md`.

### ⏳ Filtros por catálogo

| Catálogo | Filtros | Cascada | Endpoint nuevo | Notas |
|---|---|---|---|---|
| **Estado** ✅ | País (combo) + texto (`EstadoNombre`) | No | Ninguno | Un solo nivel de dependencia (`Estado.PaisId`); no hay un tercer combo debajo que necesite acotarse. |
| **Municipio** ✅ | País → Estado (combo, en cascada) + texto (`MunicipioNombre`) | Sí | `GET /Municipio/EstadosPorPais/{paisId}` | El caso ya detallado originalmente: repuebla el combo Estado según el País elegido. |
| **Partido** ✅ | Equipo Local (combo) + Equipo Visita (combo) | No | Ninguno | Ambos combos usan el mismo catálogo `Equipo` pero son ejes independientes (no uno depende del otro), así que se filtran por separado y se combinan con AND. |
| **Jornada** ✅ | Temporada (combo) + Estatus (combo) + texto (`JornadaNombre`) | No | Ninguno | `Temporada` y `EstatusJornada` son ejes independientes entre sí — sin jerarquía, sin cascada. |
| **JornadaPartido** ✅ (parcial) | Temporada → Jornada (combo, en cascada) + Estadio (combo) + Estatus de partido (combo) | Sí | `GET /JornadaPartido/JornadasPorTemporada/{temporadaId}` | El más complejo de los cinco. **Primera iteración completada** (Temporada + cascada a Jornada + comportamiento "sin carga inicial"); Estadio/Estatus de partido quedan pendientes para una siguiente iteración. |

Cada fila se implementa y verifica de forma independiente (mismo criterio que
las fases anteriores del plan), actualizando esta tabla conforme se completen.

**Estado — completado y verificado en runtime.** `EstadoController.Index` ahora
acepta `paisId`/`nombre` opcionales por query string y aplica `Where`
condicionales sobre `Estados.Include(Pais)`; el combo País usa un `SelectList`
con el valor seleccionado marcado, y el input de texto conserva lo escrito tras
el submit. Vista actualizada con el formulario de filtros (sin auto-submit) y
"Limpiar filtros". Probado contra los datos reales (33 estados: 32 de México,
1 de Estados Unidos): sin filtro (33 filas), filtro por país (32 y 1
respectivamente), filtro por texto ("Nuevo" → 1 fila, "Nuevo León"),
combinación país+texto, y caso sin resultados (0 filas, sin error). Confirmado
que el combo y el input reflejan la selección activa después de filtrar, y que
los enlaces Detalles/Editar/Eliminar siguen funcionando sobre las filas
filtradas. No requirió datos de prueba adicionales (funcionalidad de solo
lectura sobre datos ya existentes).

**Bug encontrado y corregido tras pruebas adicionales de búsqueda**: el filtro
de texto usaba `EstadoNombre.Contains(nombre)`, que el proveedor de SQLite de
EF Core traduce a la función `instr()` — **sensible a mayúsculas/minúsculas**
(a diferencia de `LIKE`, que SQLite trata sin distinguir mayúsculas para
caracteres ASCII). Buscar "Jalisco" funcionaba pero "jalisco"/"JALISCO"/
"duran"/"nuevo" (en minúsculas) no encontraban nada. Corregido usando
`EF.Functions.Like(e.EstadoNombre, "%" + nombre.Trim() + "%")`, que sí se
traduce a `LIKE` y es insensible a mayúsculas para ASCII. Reverificado: los 9
casos de prueba (incluyendo mayúsculas/minúsculas mezcladas, substring parcial
y espacios extra) devuelven el resultado esperado.

**Búsqueda insensible a acentos — resuelta.** SQLite no normaliza diacríticos
de forma nativa (`LIKE` solo resuelve mayúsculas/minúsculas ASCII), así que se
agregó una normalización explícita para el alfabeto español acentuado
(`á/é/í/ó/ú/ü/ñ` y sus mayúsculas — el mismo conjunto ya usado en el
`[RegularExpression]` del modelo `Estado`):
- **Columna (`EstadoNombre`)**: se le aplica una cadena de `.Replace()` por
  cada carácter acentuado (mapeando a su equivalente sin acento en minúscula)
  directamente en la expresión LINQ del `Where` — EF Core traduce cada
  `.Replace()` a la función SQL `replace()`, así que el filtro sigue
  resolviéndose 100% en SQLite, sin traer registros a memoria.
- **Término de búsqueda**: se normaliza en C# con el método privado
  `QuitarAcentos` (mismo mapeo, aplicado antes de construir el patrón `LIKE`).
- Verificado en runtime con los 33 estados reales: "Nuevo Leon" (sin tilde),
  "nuevo leon", y "Nuevo León" devuelven la misma fila; "mexico"/"México"/
  "MEXICO" devuelven las 2 filas correctas (México y Ciudad de México);
  "michoacan"/"Michoacán", "queretaro"/"Querétaro", "san luis potosi"/"San
  Luis Potosí" y "jalisco"/"JALISCO" funcionan igual con o sin acento/mayúscula;
  combinado con el filtro de país (`paisId=1&nombre=mexico` → 2,
  `paisId=2&nombre=mexico` → 0) también correcto; caso sin resultados sigue en
  0 filas sin error.

**Municipio — completado y verificado en runtime.** `MunicipioController.Index`
acepta `paisId`/`estadoId`/`nombre` opcionales por query string sobre
`Municipios.Include(Estado)`: `paisId` filtra vía `m.Estado.PaisId` (join
implícito, sin tabla puente visible), `estadoId` filtra directo por FK, y
`nombre` reutiliza el mismo patrón de `EstadoController` (`EF.Functions.Like`
+ cadena de `.Replace()` para insensibilidad a acentos/mayúsculas, con
`QuitarAcentos` duplicado como método privado — mismo criterio que Estado, sin
extraerlo a una clase compartida para no tocar un controlador que no estaba en
el alcance de esta tarea). El combo Estado del filtro se puebla ya acotado al
`paisId` seleccionado en cada render del servidor (para que al enviar el
formulario con "Filtrar" no se muestren estados de otro país), y además se
repuebla sin recargar la página vía cascada cliente-servidor.

**Endpoint de cascada — bug encontrado y corregido antes de verificar.**
`GET /Municipio/EstadosPorPais/{paisId}` inicialmente no tenía atributo de
ruta explícito, así que caía en la ruta convencional del proyecto
(`{controller}/{action}/{id?}` en `Program.cs`) — como el parámetro se llama
`paisId` y no `id`, nunca se bindeaba desde la URL y el endpoint devolvía
siempre los 33 estados completos sin filtrar, para cualquier país. Corregido
agregando `[HttpGet("Municipio/EstadosPorPais/{paisId}")]` explícito.
Reverificado: `paisId=1` (México) devuelve sus 32 estados sin incluir
"Alabama", `paisId=2` (Estados Unidos) devuelve solo "Alabama", y `paisId=0`
(caso "Todos los países" del combo padre) devuelve los 33. El handler de
cascada en `ligaMxCore.js` usa ids exclusivos de esta vista (`muniPaisId`/
`muniEstadoId`, distintos de los `paisId`/`estadoId` usados como `name` para
el query string) para no interferir con el combo sin cascada de
`Estado/Index` ni con los campos del formulario `Add`/`Edit` de Municipio
(que usan el id generado por el tag helper, `EstadoId` con mayúscula) — al
cambiar de país conserva el Estado seleccionado solo si sigue perteneciendo
al nuevo país, igual que el comportamiento esperado al enviar el formulario.

Verificado en runtime contra los 16 municipios reales (sin datos de prueba
adicionales, funcionalidad de solo lectura): sin filtros (16 filas), filtro
por `estadoId=14` (Jalisco → Zapopan y Guadalajara), filtro por `paisId=2`
(Estados Unidos → 0 filas, ningún municipio cargado ahí todavía), texto
insensible a acentos/mayúsculas (`leon` → León, `JUAREZ` → Ciudad Juárez,
`torreon` → Torreón, `queretaro` → Querétaro), combinación `paisId=1&nombre=
mexico` → Ciudad de México únicamente (no incluye Toluca, cuyo *Estado* se
llama México pero cuyo nombre de municipio no coincide con el texto buscado,
confirmando que el filtro de texto es sobre `MunicipioNombre` y no arrastra
coincidencias del nombre del Estado), caso sin resultados (`nombre=xyzxyz` →
0 filas sin error), combo Estado del filtro mostrando únicamente las 32
opciones de México con Jalisco marcado como seleccionado tras filtrar por
`paisId=1&estadoId=14`, y enlaces Detalles/Editar/Eliminar funcionando sobre
las filas filtradas.

**Partido — completado y verificado en runtime.** Se mantuvo el diseño
original del plan (dos combos independientes, `Equipo Local` y `Equipo
Visita`, combinados con AND vía `Index(int? equipoLocalId, int?
equipoVisitaId)`), con un acotamiento decidido con el usuario antes de
implementar — se evaluó y descartó una alternativa (un solo combo de Equipo
+ checkboxes de rol) por la ambigüedad de UX que introducía (desmarcar un
checkbox no tiene efecto en un filtro por GET sin el patrón de "hidden
companion" de MVC, y quedaba sin definir qué pasa con ambos checkboxes
desmarcados):
- Si solo se selecciona un combo, filtra únicamente por ese lado (local o
  visita), el otro queda sin restricción.
- Si no se selecciona ningún valor, se cargan todos los partidos (sin
  filtro), igual que el resto de catálogos de la Fase A.
- **Si se selecciona el mismo equipo en ambos combos, no se aplica ningún
  filtro** (se cargan todos los partidos, igual que sin selección) y se
  muestra una alerta: "El equipo local y el equipo visita no pueden ser el
  mismo. No se aplicó el filtro." — consistente con la validación ya
  existente en `Add`/`Edit` (`EquipoLocalId != EquipoVisitaId`), que hace que
  ningún partido real tenga esa combinación.

Verificado en runtime contra los 306 partidos reales (307 filas de `<tr>`
contando el encabezado): sin filtros (307), solo `equipoLocalId=1` (América
de local → 17 filas, uno por cada rival), solo `equipoVisitaId=1` (América de
visita → 17 filas), combinación `equipoLocalId=1&equipoVisitaId=2` (América
vs Atlas → 1 fila), caso bloqueado `equipoLocalId=1&equipoVisitaId=1` (alerta
mostrada, 307 filas sin filtrar), combos reflejando la selección activa tras
filtrar, y enlaces Detalles/Editar/Eliminar funcionando sobre la fila
filtrada.

**Jornada — completado y verificado en runtime.** `JornadaController.Index`
acepta `temporadaId`/`estatusJornadaId`/`nombre` opcionales por query string,
combinados con AND, sin cascada (ejes independientes, igual que estaba
planeado). El filtro de texto reutiliza el mismo patrón de `Estado`/
`Municipio` (`EF.Functions.Like` + `QuitarAcentos`) aunque `JornadaNombre` no
tiene `[RegularExpression]` con acentos en el modelo — se aplicó igual por
consistencia con el resto de catálogos y porque no tiene costo adicional.
Carga inicial sin filtros muestra todas las jornadas (comportamiento pedido
explícitamente por el usuario antes de implementar), y el filtro por
Temporada acota a las de esa temporada únicamente.

Verificado en runtime contra las 17 jornadas reales (todas en la temporada
"Clausura 2026", id=1, con `EstatusJornadaId`=1 "Pendiente" tras el backfill
de Fase 2) y la temporada "60vo. torneo corto" (id=2, recién creada, sin
jornadas): sin filtros (17 filas), `temporadaId=1` (17), `temporadaId=2` (0),
`estatusJornadaId=1` (17), `estatusJornadaId=2` "En Curso" (0), texto
`JORNADA 2` en mayúsculas (1 fila, confirma insensibilidad a mayúsculas),
combinación `temporadaId=1&estatusJornadaId=1` (17), combinación sin
jerarquía real `temporadaId=2&estatusJornadaId=1` (0, sin error), caso sin
resultados `nombre=xyzxyz` (0), combos reflejando la selección activa tras
filtrar, y enlaces Detalles/Editar/Eliminar funcionando sobre filas
filtradas.

### JornadaPartido — primera iteración completada y verificada en runtime

Implementado tal cual quedó acordado con el usuario tras resolver los puntos
abiertos del diseño original (dejado íntegro más abajo como referencia):

- **Corrección de la condición de carrera antes de continuar** (pedido
  explícito del usuario): se generalizó la lógica de cascada de `Municipio` en
  una función reutilizable `initCascada(selectPadreId, selectHijoId, urlBase,
  textoTodos)` en `ligaMxCore.js`, que ahora usa un número de secuencia local
  por cascada — cada petición AJAX se etiqueta con un contador incremental al
  dispararse, y en el `success` se descarta cualquier respuesta cuyo número no
  coincida con la última petición disparada (sin importar el orden en que
  lleguen las respuestas). `Municipio` (País→Estado) se migró a esta función
  compartida y `JornadaPartido` (Temporada→Jornada) la usa desde el día uno.
  Verificado con la misma prueba de estrés que había reproducido el bug
  original (cambiar el combo padre dos veces rápido, sin esperar la respuesta
  de la primera llamada): **0 inconsistencias en 8 repeticiones para
  Municipio y 8 para JornadaPartido** (antes del fix, Municipio fallaba 2 de
  5 veces).
- **Sin carga inicial**: `Index(int? temporadaId, int? jornadaId)` solo
  consulta `JornadaPartidos` cuando `temporadaId` tiene valor; si no, devuelve
  lista vacía sin tocar la base.
- **"Buscar" sin Temporada no devuelve resultados** (confirmado por el
  usuario): como no hay forma de distinguir "nunca se buscó" de "se buscó sin
  elegir Temporada" solo mirando que el modelo esté vacío, se usa
  `Request.QueryString.HasValue` para saber si la petición vino de un submit
  del formulario (aunque haya ido con los combos en blanco) vs. la primera
  carga de la página (sin query string en absoluto) — guardado en
  `ViewData["BusquedaRealizada"]`.
- **Mensaje de alerta cuando se buscó y no hay resultados** (pedido explícito
  del usuario): `alert-warning` "No se encontraron partidos para los filtros
  seleccionados." cuando `BusquedaRealizada == true` y el modelo viene vacío.
  Antes de la primera búsqueda se muestra en cambio un `alert-info`
  ("Selecciona una Temporada y presiona 'Buscar' para ver el fixture.") en
  vez de alerta, para no confundir "todavía no buscaste" con "buscaste y no
  hay nada".
- **El combo Jornada de la cascada muestra todas las jornadas de la
  temporada elegida**, tengan o no partidos ya programados (pedido explícito
  del usuario: "ese catálogo servirá para capturar los resultados o
  visualizarlos") — mismo criterio en el pre-poblado del servidor y en el
  endpoint `GET /JornadaPartido/JornadasPorTemporada/{temporadaId}` (con ruta
  explícita para evitar el mismo bug de binding ya encontrado en
  `Municipio`). `temporadaId <= 0` devuelve `[]` (no tiene sentido listar
  jornadas de "todas las temporadas" cuando el diseño exige elegir una).
- Los botones "Nuevo" y "Actualizar" se mantienen siempre visibles (incluso
  sin resultados) — "Nuevo" no depende del filtro, y "Actualizar" simplemente
  no tiene filas `.match_score` sobre las que iterar cuando no hay resultados
  (sin cambios necesarios en `updateScores()`).

Verificado en runtime contra los datos reales (153 `JornadaPartido` de
Clausura 2026, id=1; "60vo. torneo corto" id=2 sigue vacía): carga inicial sin
query string (0 filas, `alert-info`, sin tabla), `Buscar` con combos en blanco
— query string presente pero vacío (0 filas, `alert-warning`), `temporadaId=1`
(153 filas, sin alerta), `temporadaId=2` (0 filas, `alert-warning`),
`JornadasPorTemporada/1` (17), `JornadasPorTemporada/2` (0),
`JornadasPorTemporada/0` (0, no "todas"), combinación
`temporadaId=1&jornadaId=1` (9 filas, una jornada completa), combos
reflejando la selección activa tras filtrar, enlaces Detalles/Editar/Eliminar
funcionando sobre filas filtradas, y botones "Nuevo"/"Actualizar" presentes
incluso con 0 resultados.

Pendiente para una siguiente iteración: filtros de Estadio y Estatus de
partido (ver fila de la tabla de arriba).

### JornadaPartido — notas de diseño originales (referencia histórica, ya resueltas arriba)

Alcance definido con el usuario, deliberadamente más chico que la fila completa
de la tabla de arriba (Estadio/Estatus quedan para después):

1. **Filtro de Temporada**: combo sobre `_context.Temporada`, igual que el
   resto de catálogos de la Fase A.
2. **Filtro en cascada Temporada → Jornada**: al elegir una Temporada, el
   combo Jornada se acota a las jornadas de esa temporada — mismo patrón ya
   validado en `Municipio` (País → Estado): combo hijo pre-poblado en el
   servidor según el padre seleccionado + repoblado por AJAX sin recargar vía
   un endpoint nuevo `GET /JornadaPartido/JornadasPorTemporada/{temporadaId}`.
3. **Sin carga inicial**: a diferencia de `Estado`/`Municipio`/`Partido`/
   `Jornada` (que muestran todo el catálogo sin filtros), `JornadaPartido/Index`
   **no debe mostrar ninguna fila hasta que el usuario aplique "Buscar"**. Es
   una excepción deliberada a la convención ya documentada en `CLAUDE.md`
   ("casos borde... no requiere manejo especial", pensada para catálogos que
   sí cargan todo por defecto) — justificada porque este es el único catálogo
   cuyo `Index` además es un grid *editable* (`UpdateScores`): cargar todas
   las jornadas de todas las temporadas por defecto significaría renderizar
   cientos de inputs editables de golpe, algo que solo va a empeorar con cada
   temporada nueva que se cargue.

#### Mejoras / decisiones a confirmar antes de implementar

- **Mensaje de estado vacío**: hay que distinguir en la vista dos casos que
  hoy lucirían igual (tabla sin filas) — "todavía no se ha buscado nada" vs.
  "se buscó y no hay resultados". Sugerido: un `ViewData["BusquedaRealizada"]`
  (o equivalente) que el controlador setea cuando `temporadaId.HasValue`, para
  mostrar un mensaje distinto en cada caso ("Selecciona una Temporada y
  presiona Buscar" vs. "Sin partidos para los filtros seleccionados").
- **¿Qué hace "Buscar" sin ninguna Temporada elegida?** Con el diseño actual
  (no cargar nada hasta buscar), lo más consistente es que "Buscar" sin
  Temporada se comporte igual que la carga inicial (sigue vacío, con el
  mensaje de "selecciona una Temporada"), en vez de cargar todo el catálogo
  como hacen los demás filtros del proyecto cuando no se selecciona nada. Vale
  la pena confirmarlo explícitamente con el usuario porque es lo opuesto a la
  convención usada en el resto de la Fase A.
- **Alcance del combo Jornada en la cascada**: mostrar todas las jornadas de
  la temporada elegida (tengan o no partidos programados todavía), igual
  criterio que ya usa `CargarDropdowns` en `Add`/`Edit` de este mismo
  controlador — así filtrar por una jornada vacía muestra "0 resultados" en
  vez de ocultar la opción del combo.
- **Interacción con el flujo `UpdateScores`**: el botón "Actualizar" y el JS
  (`updateScores()` en `ligaMxCore.js`) iteran sobre las filas `.match_score`
  presentes en el DOM al momento del click, sin importar su origen — como el
  filtro es un `GET` de formulario normal (recarga de página, no AJAX/SPA),
  debería seguir funcionando sin cambios sobre las filas que resulten del
  filtro. Confirmar en runtime al implementar, no asumirlo solo por lectura
  de código.

#### Dificultad encontrada: replicar la cascada de Municipio replicaría también su bug

La verificación con `/verify` de la cascada País→Estado de `Municipio`
encontró una **condición de carrera real y reproducible** (2 de 5 intentos
controlados): si el usuario cambia el combo padre dos veces seguido sin
esperar a que la primera llamada AJAX resuelva, las respuestas de
`/Municipio/EstadosPorPais/{paisId}` pueden resolver en orden distinto al que
se dispararon, y la que llega *última* pisa a la que llega *primera* — el
combo hijo queda mostrando datos de una selección anterior, sin ningún error
visible. El endpoint nuevo que planea esta Acción 2
(`/JornadaPartido/JornadasPorTemporada/{temporadaId}`) usaría exactamente el
mismo patrón, así que replicarlo tal cual reproduciría el mismo bug por
tercera vez (ya está también planeado para reutilizarse eventualmente en
cualquier otro catálogo con cascada). **Recomendación: corregir esto al
implementar** esta cascada (no en `Municipio` retroactivamente, a menos que
el usuario lo pida aparte) agregando un token/contador de petición en
`ligaMxCore.js` que descarte en el `success` cualquier respuesta que no
corresponda a la última petición disparada, o usando `xhr.abort()` sobre la
petición anterior antes de lanzar una nueva.

#### Otros puntos a tener en cuenta (ya conocidos de catálogos anteriores)

- El endpoint `JornadasPorTemporada/{temporadaId}` necesita ruta explícita
  (`[HttpGet("JornadaPartido/JornadasPorTemporada/{temporadaId}")]`) — la ruta
  convencional del proyecto es `{controller}/{action}/{id?}`, y como el
  parámetro no se llama `id`, sin el atributo el filtro se ignoraría
  silenciosamente (mismo bug ya encontrado y corregido en `Municipio`).
- Los combos del filtro necesitan `id` HTML exclusivos de esta vista (ej.
  `jpTemporadaId`/`jpJornadaId`) para no chocar con otros combos del mismo
  nombre en otras páginas que comparten `ligaMxCore.js`.
- **Limitación para probar en runtime**: hoy solo la Temporada "Clausura 2026"
  (id=1) tiene datos reales (153 filas); la segunda Temporada ("60vo. torneo
  corto", id=2) está vacía. Se puede probar "Temporada con datos" vs.
  "Temporada vacía → sin resultados", pero no hay una segunda temporada con
  partidos para probar que el filtro realmente *excluye* filas de otra
  temporada (mismo tipo de limitación ya documentada para otros catálogos que
  aún no tenían datos cruzados al momento de implementarse).
