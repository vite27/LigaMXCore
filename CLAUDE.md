# LigaMXCore

Quiniela/pronósticos de fútbol (Liga MX). Migración desde app legacy **QuinielaLigaMX**
(ASP.NET clásico, Identity 2.2.1 + OWIN, EF6 EDMX, SQL Server LocalDB) hacia
ASP.NET Core moderno.

## Stack

- **.NET 8**, ASP.NET Core MVC (Controllers + Razor Views), sin Web API.
- **EF Core 8** con **SQLite**, pero **dos archivos de base de datos separados**:
  - `identity.db`: solo tablas de Identity (`AspNetUsers`, etc.). Se gestiona con
    EF Core Migrations (`Migrations/`, una sola migración `InitialForSqlite`) y es
    lo que apunta `DefaultConnection` en `appsettings.json` / `Program.cs`.
  - `liga.db`: todas las tablas de negocio (`Pais`, `Estado`, `Temporada`, `Jornada`,
    etc.). **No tiene tabla `__EFMigrationsHistory` y nunca se gestionó vía EF
    migrations** — se creó y se sigue modificando con SQL directo. El motivo por el
    que el mismo `ApplicationDbContext` termina apuntando aquí en runtime es que
    `ApplicationDbContext.OnConfiguring` tiene un `UseSqlite("Data Source=liga.db")`
    incondicional que sobreescribe el `DefaultConnection` inyectado por DI.
  - **Importante para cualquier cambio de esquema futuro**: `dotnet ef migrations add`
    contra este proyecto genera una migración que intenta recrear *todo* el esquema de
    negocio desde cero (porque EF no tiene historial de `liga.db`) — **no ejecutar
    `dotnet ef database update`**. Los cambios de esquema sobre catálogos de negocio se
    aplican con SQL directo sobre `liga.db` (recrear tabla si hace falta agregar una FK,
    ya que SQLite no soporta `ALTER TABLE ADD COLUMN` con `FOREIGN KEY`), respaldando
    antes el archivo (`cp liga.db liga.db.bak-<timestamp>`). Ver PLAN.md para un ejemplo
    real (columna `Jornada.EstatusJornadaId`).
- `Microsoft.EntityFrameworkCore.SqlServer` sigue en el `.csproj` pero **no se usa**
  (residuo de la migración).
- **ASP.NET Core Identity** (`AddDefaultIdentity<ApplicationUser>`) con
  `PasswordHasherCompatibilityMode.IdentityV2` para preservar hashes del sistema legacy.
- Frontend: Bootstrap + jQuery vía LibMan, un solo script propio
  (`wwwroot/Scripts/ligaMxCore.js`).
- Sin capa de servicios/repositorios: los controladores usan `ApplicationDbContext`
  directamente.

## Estructura

```
Controllers/   15 controladores CRUD por catálogo
Models/        17 entidades EF (POCO, [Table]/[ForeignKey])
Data/          ApplicationDbContext (Fluent API + DataAnnotations)
Migrations/    1 migración inicial ("InitialForSqlite") — solo cubre identity.db
Views/         Vistas Razor paralelas a cada controlador
Pages/Shared/  _Layout.cshtml (compartido por todas las vistas MVC e Identity UI)
wwwroot/       Bootstrap, jQuery, ligaMxCore.js
Scripts/       Scripts de carga de datos para liga.db (ej. `gen_jornadapartido.py` +
               el .sql que genera, con los 153 partidos reales del Clausura 2026).
               Convención para futuras cargas de datos de negocio: generador en
               Python que resuelve ids reales contra liga.db (solo lectura) y
               escribe un .sql con INSERTs, nunca escribe la DB directamente.
PLAN.md        Plan de completitud CRUD por catálogo — fuente de verdad de qué
               falta y qué validaciones se aplicaron a cada uno. Consultar antes
               de tocar cualquier catálogo.
```

## Modelo de dominio

- Catálogos: `Pais → Estado → Municipio`, `Equipo`, `Estadio`,
  `Temporada → Jornada`, `EstatusJornada`, `EstatusPartido`, `TipoResultado`,
  `Participante`, `Usuario` (tabla legacy paralela a Identity, no usada para login).
  `Jornada.EstatusJornadaId` es FK real a `EstatusJornada` (agregado en Fase 2 del
  plan; antes `EstatusJornada` era un catálogo huérfano sin ninguna FK apuntándole).
- Núcleo de negocio:
  - `Partido` (catálogo local/visita) → `JornadaPartido` (partido programado en
    una jornada, con marcador real `GolLocal`/`GolVisita`, `EstatusPartidoId`,
    `TipoResultadoId`).
  - `JornadaPronostico` (cabecera: participante pronostica una jornada) →
    `JornadaPronosticoDetalle` (pronóstico por partido: goles, `Puntos` nullable,
    `TipoResultadoId`).

## Flujos ya implementados

- CRUD completo (Index/Add/Edit/Details/Delete + validaciones DataAnnotations +
  duplicidad) en: `Pais`, `Estado`, `Municipio`, `Temporada`, `TipoResultado`,
  `EstatusPartido`, `EstatusJornada`, `Participante`, `Equipo`, `Estadio`, `Partido`,
  `Jornada`, `JornadaPartido`. El patrón (incluyendo bloqueo de `Delete` por
  dependientes, con conteo vía `CountAsync` en vez de `Include`) está documentado en
  detalle en `PLAN.md`.
  - `Partido` valida en el controlador (no con `DataAnnotations`, que no compara dos
    propiedades) que `EquipoLocalId != EquipoVisitaId` y que no exista ya otro
    partido con el mismo par local/visitante.
  - `Jornada` valida de forma análoga que `Orden` sea único dentro de la misma
    `TemporadaId`.
  - `JornadaPartido` (la tabla de fixture) se puebla por selección manual vía
    dropdowns (no generación automática) y valida que un `Partido` solo se use una
    vez en todo el fixture y que un equipo no tenga dos partidos en la misma
    `Jornada`. Un partido no jugado se crea con `GolLocal=0`/`GolVisita=0`/
    `TipoResultadoId`="Empate" (decisión de producto, sin cambio de esquema). El
    marcador real se sigue capturando solo vía `UpdateScores` (grid de `Index`),
    nunca desde `Add`/`Edit`. **Si tocas el `Edit` de `JornadaPartido`**: no uses
    `_context.Update()` sobre el objeto bindeado del formulario — como
    `GolLocal`/`GolVisita`/`TipoResultadoId` no están en el `[Bind]`, eso los
    sobrescribiría con sus valores CLR por defecto. Carga la entidad con
    `FindAsync` y modifica solo los campos del formulario (mismo patrón que
    `UpdateScores`).
  - `JornadaPartido` tiene datos reales cargados: las 153 filas de la fase
    regular completa del Clausura 2026 (17 jornadas × 9 partidos, resultados
    y estadios reales, `EstatusPartidoId`="Finalizado"). Cargados vía
    `Scripts/clausura2026_jornadapartido.sql` — ver `PLAN.md` para el detalle
    de cómo se generó y las decisiones de mapeo (nombres de equipos/estadios,
    caso especial de "Estadio Ciudad de los Deportes").
- CRUD parcial (solo Index/Edit, **sin Add/Details/Delete**) en `Usuario` — catálogo
  legacy paralelo a Identity, pendiente decidir si se completa o se marca obsoleto.
- Captura de marcadores por jornada (`JornadaPartidoController.UpdateScores`): grid
  editable en `Index` + guardado masivo vía AJAX — flujo más reciente y activo,
  independiente del CRUD de asignación de partidos (`Add`/`Edit`/`Details`/`Delete`).
- Identity registrado pero **sin páginas propias ni enlaces** de login/logout en el layout.

## Filtros de listado (Fase A — en progreso)

Varios catálogos están pensados para crecer mucho más allá de los datos
actuales (ej. `Municipio` hacia los ~2,500 municipios reales de México), así
que se está agregando un filtro de búsqueda a su `Index`, catálogo por
catálogo, sin agregar ninguna dependencia nueva (combos nativos de Bootstrap +
query string + handlers puntuales en `ligaMxCore.js` cuando hace falta
cascada). Alcance total: `Estado`, `Municipio`, `Partido`, `Jornada`,
`JornadaPartido` — el desglose de qué filtros aplica a cada uno (y cuáles
llevan cascada) está en `PLAN.md`, sección "Mejora de UX — Filtros de listado
en catálogos con volumen (Fase A)".

- **Estado ✅ completado**: filtro por País (combo) + texto (`EstadoNombre`),
  combinables, vía query string (`Index(int? paisId, string? nombre)`), sin
  auto-submit. Los demás catálogos de la lista siguen pendientes.

**Dos gotchas de EF Core + SQLite descubiertos al implementar la búsqueda por
texto en Estado, aplicables a cualquier filtro de texto futuro:**

1. **No uses `.Contains()` para buscar texto.** El proveedor de SQLite de EF
   Core traduce `string.Contains()` a la función SQL `instr()`, que es
   **sensible a mayúsculas/minúsculas** (a diferencia de `LIKE`, que SQLite
   trata sin distinguir mayúsculas para ASCII). Usa
   `EF.Functions.Like(columna, "%" + termino + "%")` en su lugar.
2. **SQLite no normaliza acentos de forma nativa** — ni `LIKE` ni `instr()`
   igualan "Leon" con "León". Para catálogos con nombres en español
   (`Estado`, `Municipio`, `Equipo`, `Estadio`, etc.) hay que normalizar
   ambos lados manualmente con el mismo alfabeto acentuado que ya usan los
   `[RegularExpression]` de los modelos (`á/é/í/ó/ú/ü/ñ` + mayúsculas):
   - **Columna**: una cadena de `.Replace("á","a")...` directamente en la
     expresión LINQ del `Where` — EF Core traduce cada `.Replace()` a la
     función SQL `replace()`, así que el filtro se sigue resolviendo 100% en
     la base de datos (sin cargar todo a memoria, algo que sí importa para
     catálogos que van a crecer como `Municipio`).
   - **Término de búsqueda**: se normaliza en C# antes de construir el patrón
     (ver `QuitarAcentos` en `EstadoController` como referencia a replicar en
     los demás controladores).

## Convenciones de UI

- **Ningún `Index` muestra la columna Id** en ningún catálogo (se sigue mostrando en
  `Details`; los enlaces de Editar/Eliminar/Detalles siguen usando el id internamente
  vía `asp-route-id`, que no depende de que haya una columna visible). Al crear la
  vista `Index` de un catálogo nuevo, seguir este mismo patrón.
- **Estadio**: `Index` solo muestra Nombre y Alias (Dirección/CP/Municipio solo en
  `Details`).
- **Equipo**: `Index` solo muestra Nombre y Logo (Alias/Municipio solo en `Details`).
- **Jornada**: `Index` solo muestra Temporada y Jornada (Orden/Estatus solo en
  `Details`).

## Flujos pendientes (lo más importante a considerar en cualquier tarea)

1. **Captura de pronósticos del participante** — no existe controlador/vista para
   `JornadaPronostico`/`JornadaPronosticoDetalle`. Es el corazón funcional faltante.
2. **Cálculo de puntos** — no hay lógica que compare resultado real
   (`JornadaPartido`) vs. pronóstico (`JornadaPronosticoDetalle`) para asignar `Puntos`.
3. **Tabla de posiciones/ranking** de participantes — no existe.
4. **Vínculo Usuario Identity ↔ Participante** — sin relación en el modelo.
5. **Autorización** — ningún controlador tiene `[Authorize]`; todo es público.
6. **Delete/Details** faltan en `Usuario` (catálogo legacy, pendiente decisión —
   ver PLAN.md Fase 1).
7. Sin lógica de cierre automático de `EstatusJornada` (el campo ya existe en
   `Jornada`, pero nada lo actualiza automáticamente).

## Notas / inconsistencias conocidas

- El README menciona una utilidad `tools/SqliteRunner` (scripts `sqlite_create.sql`,
  `sqlite_fill_base.sql`) que **no existe** en el repo actual — documentación desactualizada.
  Es probablemente la herramienta con la que originalmente se creó el esquema de
  `liga.db` fuera de EF migrations (ver nota en Stack arriba).
- No modificar sin verificar primero si estas piezas fueron reincorporadas.
