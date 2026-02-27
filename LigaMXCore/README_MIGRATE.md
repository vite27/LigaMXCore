Proyecto prototipo ASP.NET Core (net6.0) creado como punto de partida para migración.

> **Nota**: La documentación principal de este repositorio ahora se encuentra en `README.md`.  
> Este archivo conserva apuntes históricos sobre la migración.


Siguientes pasos:
- Migrar autenticación (OWIN/Identity -> ASP.NET Core Identity).
- Migrar persistencia (EF6 EDMX -> EF Core o redesign de modelos).
- Migrar vistas y helpers que dependan de System.Web.
- Reemplazar Bundling/Minification por LibMan/webpack o similares.

Identity (lo que añadí):
- Proyecto configurado para usar `Microsoft.AspNetCore.Identity` con EF Core (SQLite) como prototipo.
- Archivos añadidos: `Models/ApplicationUser.cs`, `Data/ApplicationDbContext.cs`.
- Para crear la base de datos y tablas de Identity desde la carpeta `LigaMXCore`:

```powershell
dotnet tool install --global dotnet-ef --version 6.*
dotnet ef migrations add InitialIdentity -p LigaMXCore -s LigaMXCore
dotnet ef database update -p LigaMXCore -s LigaMXCore
```

Nota: la migración real puede requerir mapear usuarios/roles existentes de la aplicación original (Identity 2.2.1 + OWIN). Esto implica exportar datos y rehacer contraseñas si es necesario o validar hashes compatibles.
