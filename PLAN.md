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
| **Estado** | `Municipio.EstadoId` |
| **Municipio** | `Equipo.MunicipioId`, `Estadio.MunicipioId` |
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

### ⏳ Pendiente (todos los demás)

Todos los catálogos restantes tienen `Index` y, salvo `Temporada`/`TipoResultado`/`Usuario`, también `Add`/`Edit`. A ninguno se le ha aplicado `Details`, `Delete` ni validaciones de dato.

## Plan de trabajo futuro, por fases

Orden pensado de "raíz" (menos dependencias que resolver) hacia "hoja" (más
dependencias), para poder probar cada `Delete` con casos reales sencillos antes de
llegar a los catálogos más enredados.

### Fase 1 — Catálogos raíz sin Add todavía (prioridad alta: completar el CRUD básico primero)

| Catálogo | Falta | Validaciones sugeridas |
|---|---|---|
| **Temporada** | `Add`, `Details`, `Delete` | `TemporadaNombre`: requerido, longitud 3–50, sin caracteres especiales salvo espacios/guion (ej. "2026-2027"); permitir dígitos aquí (a diferencia de País) porque los nombres de temporada son años. `Comentarios`: opcional, longitud máx. 500, sin restricción de caracteres (es texto libre). Duplicidad por nombre. |
| **TipoResultado** | `Add`, `Details`, `Delete` | `TipoResultadoNombre`: requerido, longitud 3–30, solo letras/espacios. Duplicidad por nombre. |
| **Usuario** | `Add`, `Details`, `Delete` | Catálogo legacy paralelo a Identity — antes de invertir, decidir con el usuario si se completa o se marca obsoleto. Si se completa: `UsuarioNombre` requerido y único; ocultar `Password` en texto plano en `Index`/`Details` (hoy se muestra sin enmascarar). |

### Fase 2 — Catálogos raíz con CRUD ya completo (agregar solo Details/Delete)

| Catálogo | Validaciones sugeridas para Add/Edit (hoy sin `DataAnnotations`) |
|---|---|
| **EstatusPartido** | `EstatusPartidoNombre`: requerido, longitud 3–30, solo letras/espacios. Duplicidad. |
| **EstatusJornada** | Mismo criterio que EstatusPartido — condicionado a resolver primero el hallazgo de la tabla anterior (catálogo huérfano). |
| **Participante** | `Nombres`, `ApellidoPaterno`, `ApellidoMaterno`: requeridos, longitud 2–50 cada uno, solo letras/acentos/espacios (nombres de persona, no llevan números ni símbolos). Sin duplicidad estricta de nombre completo (dos personas pueden compartir nombre); posible validación suave de "nombre completo ya existe, ¿continuar?" en vez de bloqueo duro. |

### Fase 3 — Catálogos con dependencia de un nivel

| Catálogo | Depende de | Validaciones sugeridas |
|---|---|---|
| **Estado** | Pais | `EstadoNombre`: requerido (ya existe), agregar longitud 3–50 y regex letras/acentos/espacios. Duplicidad de nombre **dentro del mismo país** (dos países distintos sí pueden tener un estado homónimo). |
| **Municipio** | Estado | `MunicipioNombre`: requerido, longitud 3–80, letras/acentos/espacios. Duplicidad dentro del mismo Estado. |

### Fase 4 — Catálogos con dependencia de dos niveles

| Catálogo | Depende de | Validaciones sugeridas |
|---|---|---|
| **Equipo** | Municipio (→ Estado → Pais) | `EquipoNombre`: requerido, longitud 3–60. `Alias`: opcional, longitud 2–20, sin caracteres especiales. `EquipoLogo`: si es URL/ruta, validar formato. Duplicidad de `EquipoNombre` global (nombres de equipo de Liga MX son únicos). |
| **Estadio** | Municipio | `EstadioNombre`: ya tiene `[Required]`, agregar longitud 3–80. `CodigoPostal`: si se conserva, `[RegularExpression(@"^\d{5}$")]` para México. Duplicidad de nombre. |

### Fase 5 — Núcleo de calendario

| Catálogo | Depende de | Notas |
|---|---|---|
| **Partido** | Equipo (local/visita) | Ya validado indirectamente por la regeneración del calendario (306 registros, sin duplicados). Falta: impedir `EquipoLocalId == EquipoVisitaId` en `Add`/`Edit` (un equipo no puede jugar contra sí mismo) — agregar validación cruzada en el controlador. `Details`/`Delete` deben mostrar conteo de `JornadaPartido` asociados. |
| **Jornada** | Temporada | `JornadaNombre`: requerido, longitud 3–50. `Orden`: entero positivo, único dentro de la misma Temporada (no puede haber dos "Jornada 5" en la misma temporada). |

### Fase 6 — Fixture (el más delicado, requiere decisión de producto antes de tocar código)

| Catálogo | Pendiente |
|---|---|
| **JornadaPartido** | No es catálogo puro. Falta definir: ¿cómo se pobla? (¿selección manual de partidos del catálogo `Partido` por Jornada, o generación automática?). Su `Delete` no debería borrar el `Partido` del catálogo, solo la asignación a la Jornada. Requiere alinear con el plan de negocio pendiente (captura de pronósticos) antes de invertir tiempo aquí. |

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
