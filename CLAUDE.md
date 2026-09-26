# LigaMXCore

Quiniela/pronósticos de fútbol (Liga MX). Migración desde app legacy **QuinielaLigaMX**
(ASP.NET clásico, Identity 2.2.1 + OWIN, EF6 EDMX, SQL Server LocalDB) hacia
ASP.NET Core moderno.

## Stack

- **.NET 8**, ASP.NET Core MVC (Controllers + Razor Views), sin Web API.
- **EF Core 8** con **SQLite** como proveedor activo (`Data Source=identity.db`,
  ver `Program.cs`). `Microsoft.EntityFrameworkCore.SqlServer` sigue en el
  `.csproj` pero **no se usa** (residuo de la migración).
- **ASP.NET Core Identity** (`AddDefaultIdentity<ApplicationUser>`) con
  `PasswordHasherCompatibilityMode.IdentityV2` para preservar hashes del sistema legacy.
- Frontend: Bootstrap + jQuery vía LibMan, un solo script propio
  (`wwwroot/Scripts/ligaMxCore.js`).
- Sin capa de servicios/repositorios: los controladores usan `ApplicationDbContext`
  directamente.

## Estructura

```
Controllers/   14 controladores CRUD por catálogo (patrón Index/Add/Edit)
Models/        17 entidades EF (POCO, [Table]/[ForeignKey])
Data/          ApplicationDbContext (Fluent API + DataAnnotations)
Migrations/    1 migración inicial ("InitialForSqlite")
Views/         Vistas Razor paralelas a cada controlador
Pages/Shared/  _Layout.cshtml (compartido por todas las vistas MVC e Identity UI)
wwwroot/       Bootstrap, jQuery, ligaMxCore.js
```

## Modelo de dominio

- Catálogos: `Pais → Estado → Municipio`, `Equipo`, `Estadio`,
  `Temporada → Jornada`, `EstatusJornada`, `EstatusPartido`, `TipoResultado`,
  `Participante`, `Usuario` (tabla legacy paralela a Identity, no usada para login).
- Núcleo de negocio:
  - `Partido` (catálogo local/visita) → `JornadaPartido` (partido programado en
    una jornada, con marcador real `GolLocal`/`GolVisita`, `EstatusPartidoId`,
    `TipoResultadoId`).
  - `JornadaPronostico` (cabecera: participante pronostica una jornada) →
    `JornadaPronosticoDetalle` (pronóstico por partido: goles, `Puntos` nullable,
    `TipoResultadoId`).

## Flujos ya implementados

- CRUD completo (Index/Add/Edit, sin Delete/Details) para la mayoría de catálogos.
- CRUD parcial (solo Index/Edit, **sin Add**) en `Temporada`, `TipoResultado`, `Usuario`.
- Captura de marcadores por jornada (`JornadaPartidoController`): grid editable +
  guardado masivo vía AJAX (`UpdateScores`) — flujo más reciente y activo.
- Identity registrado pero **sin páginas propias ni enlaces** de login/logout en el layout.

## Flujos pendientes (lo más importante a considerar en cualquier tarea)

1. **Captura de pronósticos del participante** — no existe controlador/vista para
   `JornadaPronostico`/`JornadaPronosticoDetalle`. Es el corazón funcional faltante.
2. **Cálculo de puntos** — no hay lógica que compare resultado real
   (`JornadaPartido`) vs. pronóstico (`JornadaPronosticoDetalle`) para asignar `Puntos`.
3. **Tabla de posiciones/ranking** de participantes — no existe.
4. **Vínculo Usuario Identity ↔ Participante** — sin relación en el modelo.
5. **Autorización** — ningún controlador tiene `[Authorize]`; todo es público.
6. Sin acciones **Delete/Details** en ningún controlador.
7. Sin lógica de cierre automático de `EstatusJornada`.

## Notas / inconsistencias conocidas

- El README menciona una utilidad `tools/SqliteRunner` (scripts `sqlite_create.sql`,
  `sqlite_fill_base.sql`) que **no existe** en el repo actual — documentación desactualizada.
- No modificar sin verificar primero si estas piezas fueron reincorporadas.
