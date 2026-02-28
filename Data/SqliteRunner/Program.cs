using Microsoft.Data.Sqlite;
using System;
using System.IO;

var baseDir = Directory.GetCurrentDirectory();
var dataDir = Path.GetFullPath(Path.Combine(baseDir, "Data"));
var dbPath = Path.GetFullPath(Path.Combine(dataDir, "liga.db"));
var createPath = Path.GetFullPath(Path.Combine(dataDir, "sqlite_create.sql"));
var fillPath = Path.GetFullPath(Path.Combine(dataDir, "sqlite_fill_base.sql"));

Console.WriteLine($"DB: {dbPath}");
Console.WriteLine($"Schema: {createPath}");
Console.WriteLine($"Data: {fillPath}");

if (!File.Exists(createPath))
{
    Console.Error.WriteLine("No se encuentra sqlite_create.sql");
    return 1;
}
if (!File.Exists(fillPath))
{
    Console.Error.WriteLine("No se encuentra sqlite_fill_base.sql");
    return 1;
}

var createSql = File.ReadAllText(createPath);
var fillSql = File.ReadAllText(fillPath);

bool runChecksOnly = args.Length > 0 && args[0].Equals("check", StringComparison.OrdinalIgnoreCase);

try
{
    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "PRAGMA foreign_keys = ON;";
    cmd.ExecuteNonQuery();

    if (!runChecksOnly)
    {
        // Si existe una BD previa, eliminarla para asegurar ejecución limpia
        try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }

        // Reabrir conexión tras eliminar
        conn.Close();
        using var conn2 = new SqliteConnection($"Data Source={dbPath}");
        conn2.Open();
        using var cmd2 = conn2.CreateCommand();
        cmd2.CommandText = "PRAGMA foreign_keys = ON;";
        cmd2.ExecuteNonQuery();

        // Ejecutar script de esquema
        cmd2.CommandText = createSql;
        cmd2.ExecuteNonQuery();

        // Ejecutar script de datos (el archivo incluye BEGIN/COMMIT)
        cmd2.CommandText = fillSql;
        cmd2.ExecuteNonQuery();

        // Reassign cmd to new open connection for checks below
        cmd2.CommandText = @"SELECT (SELECT COUNT(*) FROM Usuario) as Usuarios,
                                     (SELECT COUNT(*) FROM Participante) as Participantes,
                                     (SELECT COUNT(*) FROM Pais) as Paises,
                                     (SELECT COUNT(*) FROM Estado) as Estados,
                                     (SELECT COUNT(*) FROM Municipio) as Municipios,
                                     (SELECT COUNT(*) FROM Equipo) as Equipos;";

        using var reader2 = cmd2.ExecuteReader();
        if (reader2.Read())
        {
            Console.WriteLine($"Usuarios: {reader2.GetInt32(0)}");
            Console.WriteLine($"Participantes: {reader2.GetInt32(1)}");
            Console.WriteLine($"Paises: {reader2.GetInt32(2)}");
            Console.WriteLine($"Estados: {reader2.GetInt32(3)}");
            Console.WriteLine($"Municipios: {reader2.GetInt32(4)}");
            Console.WriteLine($"Equipos: {reader2.GetInt32(5)}");
        }

        conn2.Close();
    }

    // Ahora ejecutar comprobaciones adicionales sobre la BD (existente o recién creada)
    cmd.CommandText = "PRAGMA integrity_check;";
    var integrity = cmd.ExecuteScalar()?.ToString();
    Console.WriteLine($"PRAGMA integrity_check: {integrity}");

    cmd.CommandText = "PRAGMA foreign_key_check;";
    using (var fkReader = cmd.ExecuteReader())
    {
        bool anyFk = false;
        while (fkReader.Read())
        {
            anyFk = true;
            Console.WriteLine($"FK violation: table={fkReader.GetString(0)} rowid={fkReader.GetInt32(1)} parent={fkReader.GetString(2)}");
        }
        if (!anyFk) Console.WriteLine("PRAGMA foreign_key_check: no violations");
    }

    // Listar conteos por tabla dinámicamente
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
    var tables = new System.Collections.Generic.List<string>();
    using (var tReader = cmd.ExecuteReader())
    {
        while (tReader.Read()) tables.Add(tReader.GetString(0));
    }
    foreach (var t in tables)
    {
        cmd.CommandText = $"SELECT COUNT(*) FROM \"{t}\";";
        var cnt = cmd.ExecuteScalar();
        Console.WriteLine($"{t}: {cnt}");

        // Comprobar columnas NOT NULL con valores NULL
        cmd.CommandText = $"PRAGMA table_info(\"{t}\");";
        var cols = new System.Collections.Generic.List<(string name, int notnull)>();
        using (var cReader = cmd.ExecuteReader())
        {
            while (cReader.Read())
            {
                cols.Add((cReader.GetString(1), cReader.GetInt32(3)));
            }
        }
        foreach (var col in cols)
        {
            if (col.notnull == 1)
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM \"{t}\" WHERE \"{col.name}\" IS NULL;";
                var nulls = Convert.ToInt32(cmd.ExecuteScalar());
                if (nulls > 0) Console.WriteLine($"WARNING: Table {t} column {col.name} has {nulls} NULL(s) but is NOT NULL");
            }
        }

        // Mostrar hasta 3 filas de ejemplo
        cmd.CommandText = $"SELECT * FROM \"{t}\" LIMIT 3;";
        using (var sReader = cmd.ExecuteReader())
        {
            var colsCount = sReader.FieldCount;
            int row = 0;
            while (sReader.Read())
            {
                row++; var values = new System.Text.StringBuilder();
                for (int i = 0; i < colsCount; i++) values.Append($"{sReader.GetName(i)}={sReader.GetValue(i)}; ");
                Console.WriteLine($"Sample {t} row {row}: {values}");
            }
        }
    }

    conn.Close();
    Console.WriteLine("Comprobaciones completadas con éxito.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error al ejecutar scripts: {ex.Message}");
    // Intentar eliminar la BD parcialmente creada
    try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
    return 1;
}