# LigaMXCore

Aplicación de ejemplo ASP.NET Core con Identity y EF Core empleando SQLite.

## Objetivo del repositorio
Este proyecto forma parte de un portafolio personal. Está diseñado para ser fácil de clonar y ejecutar sin configuraciones adicionales. SQLite se usa como base de datos por defecto para minimizar dependencias.

## Configuración y ejecución
1. Asegúrate de tener instalado .NET 8 (SDK y runtime).
2. Clona el repositorio.
3. Desde la carpeta `LigaMXCore` ejecuta:
   ```powershell
   dotnet tool install --global dotnet-ef --version 8.*
   dotnet ef database update
   dotnet run
   ```
   Esto creará `identity.db` en el directorio y aplicará la migración inicial de Identity.

- El archivo `identity.db` está ignorado por Git (`.gitignore`).
- Si deseas borrar y regenerar la base de datos:
  ```powershell
  rm identity.db
  dotnet ef database update
  ```

## Base de datos y migraciones
El contexto `ApplicationDbContext` utiliza Identity con tablas `AspNet*`. Las migraciones residen en `Migrations/` y fueron creadas para SQLite.

### Cambiar a otro proveedor
Modificar `Program.cs` para usar `UseSqlServer`/`UseNpgsql`, cambiar cadena de conexión en `appsettings.json` y recrear migraciones si es necesario.

## Datos de ejemplo
Actualmente no se incluyen datos de ejemplo. Puedes agregar un seeding en `ApplicationDbContext` o ejecutar tus propios scripts.

## Notas
El archivo `README_MIGRATE.md` contiene notas históricas de la migración desde la aplicación original.

---

¡Disfruta explorando el proyecto!

## Utilidad SqliteRunner

Hay una utilidad en `tools/SqliteRunner` que crea y valida la base de datos SQLite (`Data/liga.db`) usando los scripts `Data/sqlite_create.sql` y `Data/sqlite_fill_base.sql`.

- Para ejecutar la utilidad desde la raíz del repositorio:

```powershell
dotnet run --project tools/SqliteRunner/SqliteRunner.csproj
```

Para ver solo las comprobaciones sin recrear la base:

```powershell
dotnet run --project tools/SqliteRunner/SqliteRunner.csproj -- check
```

Consulta `tools/SqliteRunner/README.md` para más detalles.
