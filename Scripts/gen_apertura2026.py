# -*- coding: utf-8 -*-
"""
Genera el SQL de carga de la temporada Apertura 2026 de la Liga MX: Temporada,
equipo Atlante (nuevo en el catálogo), sus Partido (local/visita contra los demás
equipos), 17 Jornada y 153 JornadaPartido.

Fuente: Wikipedia, "Torneo Apertura 2026 (México)", sección "Torneo regular" (vía la
API de MediaWiki), consultada el 2026-10-09. A esa fecha estaban jugadas las
jornadas 1-10 (la 7 con 2 partidos aplazados), la 11 era la siguiente y la 12-17
estaban por jugarse. Si se vuelve a cargar con resultados más recientes, actualizar
PARTIDOS (golLocal/golVisita = None para partidos no jugados).

Solo LEE liga.db (para resolver ids existentes); NO escribe en la base. Escribe
Scripts/apertura2026.sql. Los ids de registros nuevos (temporada, jornadas, Atlante,
partidos nuevos) se resuelven con subconsultas dentro del propio SQL, así que el
script no depende de los ids autogenerados.

Partido jugado: marcador real, estatus "Finalizado" y tipo de resultado calculado.
Partido no jugado: 0-0, "Pendiente" y "Empate" (decisión de producto, igual que Add).
Jornada: "Finalizada" si todos sus partidos están jugados; si no, "Pendiente".

Uso: desde la raíz del repo, `python Scripts/gen_apertura2026.py`.
"""
import os
import sqlite3
import sys

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DB_PATH = os.path.join(BASE_DIR, "liga.db")
OUT_SQL = os.path.join(BASE_DIR, "Scripts", "apertura2026.sql")

TEMPORADA = "Apertura 2026"
COMENTARIOS = "61er torneo corto"
ATLANTE = ("Atlante", "Los Potros de Hierro", "Ciudad de México")  # equipo, alias, municipio

# (jornada, local, golLocal, golVisita, visita, estadio_fuente)
# J3 América-Santos Laguna: la celda de estadio viene desplazada en la fuente
# ("2 de agosto"); la URL del informe arbitral dice estadio-banorte.
PARTIDOS = [
    (1, 'Necaxa', 2, 1, 'Atlante', 'Victoria'),
    (1, 'Tijuana', 3, 1, 'Tigres UANL', 'Caliente'),
    (1, 'Atlético de San Luis', 2, 3, 'Cruz Azul', 'Libertad Financiera'),
    (1, 'León', 2, 3, 'Atlas', 'León'),
    (1, 'Juárez', 0, 1, 'Puebla', 'Olímpico Benito Juárez'),
    (1, 'Pumas UNAM', 0, 3, 'Pachuca', 'Olímpico Universitario'),
    (1, 'Monterrey', 3, 2, 'Santos Laguna', 'BBVA'),
    (1, 'Guadalajara', 0, 2, 'Toluca', 'Akron'),
    (1, 'Querétaro', 0, 1, 'América', 'Corregidora'),
    (2, 'Cruz Azul', 2, 1, 'Puebla', 'Banorte'),
    (2, 'Toluca', 1, 2, 'Pumas UNAM', 'Nemesio Díez'),
    (2, 'Atlante', 1, 1, 'América', 'Banorte'),
    (2, 'Tijuana', 1, 0, 'León', 'Caliente'),
    (2, 'Guadalajara', 1, 0, 'Juárez', 'Akron'),
    (2, 'Santos Laguna', 0, 1, 'Atlas', 'Corona'),
    (2, 'Tigres UANL', 2, 2, 'Atlético de San Luis', 'Universitario'),
    (2, 'Necaxa', 2, 1, 'Monterrey', 'Victoria'),
    (2, 'Pachuca', 1, 2, 'Querétaro', 'Hidalgo'),
    (3, 'Puebla', 1, 1, 'Guadalajara', 'Cuauhtémoc'),
    (3, 'Atlético de San Luis', 0, 0, 'Tijuana', 'Libertad Financiera'),
    (3, 'Juárez', 1, 5, 'Pumas UNAM', 'Olímpico Benito Juárez'),
    (3, 'Querétaro', 3, 2, 'Tigres UANL', 'Corregidora'),
    (3, 'Atlas', 0, 2, 'Monterrey', 'Jalisco'),
    (3, 'León', 1, 0, 'Pachuca', 'León'),
    (3, 'Cruz Azul', 2, 3, 'Atlante', 'Banorte'),
    (3, 'América', 3, 0, 'Santos Laguna', 'Banorte'),
    (3, 'Toluca', 3, 1, 'Necaxa', 'Nemesio Díez'),
    (4, 'Atlante', 0, 0, 'Toluca', 'Banorte'),
    (4, 'Monterrey', 6, 1, 'Juárez', 'BBVA'),
    (4, 'Atlas', 2, 1, 'Tigres UANL', 'Jalisco'),
    (4, 'Pumas UNAM', 0, 0, 'Querétaro', 'Olímpico Universitario'),
    (4, 'América', 3, 0, 'Atlético de San Luis', 'Banorte'),
    (4, 'Santos Laguna', 0, 1, 'Guadalajara', 'Corona'),
    (4, 'Tijuana', 2, 1, 'Cruz Azul', 'Caliente'),
    (4, 'Necaxa', 1, 2, 'León', 'Victoria'),
    (4, 'Pachuca', 2, 3, 'Puebla', 'Hidalgo'),
    (5, 'León', 2, 0, 'Monterrey', 'León'),
    (5, 'Tigres UANL', 2, 0, 'Atlante', 'Universitario'),
    (5, 'Juárez', 1, 2, 'América', 'Olímpico Benito Juárez'),
    (5, 'Querétaro', 1, 2, 'Toluca', 'Corregidora'),
    (5, 'Puebla', 3, 2, 'Santos Laguna', 'Cuauhtémoc'),
    (5, 'Guadalajara', 5, 2, 'Tijuana', 'Akron'),
    (5, 'Cruz Azul', 0, 2, 'Atlas', 'Banorte'),
    (5, 'Atlético de San Luis', 1, 1, 'Pachuca', 'Libertad Financiera'),
    (5, 'Pumas UNAM', 1, 1, 'Necaxa', 'Olimpico Universitario'),
    (6, 'Necaxa', 1, 3, 'Cruz Azul', 'Victoria'),
    (6, 'Atlante', 1, 1, 'León', 'Banorte'),
    (6, 'Tijuana', 2, 0, 'Pumas UNAM', 'Caliente'),
    (6, 'Atlas', 1, 3, 'Querétaro', 'Jalisco'),
    (6, 'Pachuca', 1, 1, 'Guadalajara', 'Hidalgo'),
    (6, 'América', 2, 0, 'Puebla', 'Banorte'),
    (6, 'Santos Laguna', 0, 0, 'Tigres UANL', 'Corona'),
    (6, 'Toluca', 4, 0, 'Juárez', 'Nemesio Díez'),
    (6, 'Monterrey', 1, 3, 'Atlético de San Luis', 'BBVA'),
    (7, 'Juárez', 0, 2, 'Pachuca', 'Olímpico Benito Juárez'),
    (7, 'Atlético de San Luis', 0, 3, 'Guadalajara', 'Libertad Financiera'),
    (7, 'Tigres UANL', 1, 1, 'Necaxa', 'Universitario'),
    (7, 'Atlas', 1, 1, 'Atlante', 'Jalisco'),
    (7, 'Cruz Azul', 1, 0, 'Santos Laguna', 'Banorte'),
    (7, 'Pumas UNAM', 3, 1, 'León', 'Olímpico Universitario'),
    (7, 'Puebla', 0, 1, 'Toluca', 'Cuauhtémoc'),
    (7, 'América', None, None, 'Tijuana', 'Banorte'),
    (7, 'Querétaro', None, None, 'Monterrey', 'Corregidora'),
    (8, 'Necaxa', 0, 1, 'Puebla', 'Victoria'),
    (8, 'Atlante', 0, 3, 'Pachuca', 'Banorte'),
    (8, 'Tijuana', 0, 1, 'Querétaro', 'Caliente'),
    (8, 'Toluca', 5, 2, 'Atlas', 'Nemesio Díez'),
    (8, 'Monterrey', 0, 0, 'Tigres UANL', 'BBVA'),
    (8, 'Cruz Azul', 4, 3, 'América', 'Banorte'),
    (8, 'Santos Laguna', 2, 1, 'Juárez', 'Corona'),
    (8, 'Guadalajara', 3, 0, 'Pumas UNAM', 'Akron'),
    (8, 'León', 2, 0, 'Atlético de San Luis', 'León'),
    (9, 'Puebla', 1, 1, 'Atlante', 'Cuauhtémoc'),
    (9, 'Juárez', 2, 0, 'Tigres UANL', 'Olímpico Benito Juárez'),
    (9, 'Atlas', 1, 1, 'Pumas UNAM', 'Jalisco'),
    (9, 'Atlético de San Luis', 3, 1, 'Necaxa', 'Libertad Financiera'),
    (9, 'Monterrey', 1, 0, 'Cruz Azul', 'BBVA'),
    (9, 'América', 2, 2, 'Guadalajara', 'Banorte'),
    (9, 'Pachuca', 2, 2, 'Tijuana', 'Hidalgo'),
    (9, 'Toluca', 2, 3, 'Santos Laguna', 'Nemesio Díez'),
    (9, 'Querétaro', 1, 1, 'León', 'Corregidora'),
    (10, 'Atlante', 4, 2, 'Monterrey', 'Banorte'),
    (10, 'Tijuana', 2, 3, 'Atlas', 'Caliente'),
    (10, 'Cruz Azul', 3, 3, 'Toluca', 'Banorte'),
    (10, 'Guadalajara', 0, 2, 'Querétaro', 'Akron'),
    (10, 'Santos Laguna', 1, 0, 'Pachuca', 'Corona'),
    (10, 'Tigres UANL', 1, 0, 'Puebla', 'Universitario'),
    (10, 'Pumas UNAM', 2, 3, 'Atlético de San Luis', 'Olímpico Universitario'),
    (10, 'León', 2, 1, 'Juárez', 'León'),
    (10, 'Necaxa', 2, 4, 'América', 'Victoria'),
    (11, 'Puebla', None, None, 'León', 'Cuauhtémoc'),
    (11, 'Tigres UANL', None, None, 'Toluca', 'Universitario'),
    (11, 'Juárez', None, None, 'Tijuana', 'Olímpico Benito Juárez'),
    (11, 'Querétaro', None, None, 'Atlante', 'Corregidora'),
    (11, 'Atlas', None, None, 'Guadalajara', 'Jalisco'),
    (11, 'América', None, None, 'Monterrey', 'Banorte'),
    (11, 'Atlético de San Luis', None, None, 'Santos Laguna', 'Libertad Financiera'),
    (11, 'Pachuca', None, None, 'Necaxa', 'Hidalgo'),
    (11, 'Pumas UNAM', None, None, 'Cruz Azul', 'Olímpico Universitario'),
    (12, 'Necaxa', None, None, 'Atlas', 'Victoria'),
    (12, 'Atlante', None, None, 'Pumas UNAM', 'Banorte'),
    (12, 'Tijuana', None, None, 'Puebla', 'Caliente'),
    (12, 'Guadalajara', None, None, 'Tigres UANL', 'Akron'),
    (12, 'Santos Laguna', None, None, 'Querétaro', 'Corona'),
    (12, 'León', None, None, 'América', 'León'),
    (12, 'Toluca', None, None, 'Atlético de San Luis', 'Nemesio Díez'),
    (12, 'Cruz Azul', None, None, 'Juárez', 'Banorte'),
    (12, 'Monterrey', None, None, 'Pachuca', 'BBVA'),
    (13, 'Atlético de San Luis', None, None, 'Querétaro', 'Libertad Financiera'),
    (13, 'Juárez', None, None, 'Atlante', 'Olímpico Benito Juárez'),
    (13, 'Tigres UANL', None, None, 'León', 'Universitario'),
    (13, 'Guadalajara', None, None, 'Necaxa', 'Akron'),
    (13, 'Puebla', None, None, 'Monterrey', 'Cuauhtémoc'),
    (13, 'Atlas', None, None, 'América', 'Jalisco'),
    (13, 'Toluca', None, None, 'Tijuana', 'Nemesio Díez'),
    (13, 'Pachuca', None, None, 'Cruz Azul', 'Hidalgo'),
    (13, 'Santos Laguna', None, None, 'Pumas UNAM', 'Corona'),
    (14, 'Necaxa', None, None, 'Juárez', 'Victoria'),
    (14, 'Atlante', None, None, 'Atlético de San Luis', 'Banorte'),
    (14, 'León', None, None, 'Toluca', 'León'),
    (14, 'Monterrey', None, None, 'Guadalajara', 'BBVA'),
    (14, 'Pumas UNAM', None, None, 'Tigres UANL', 'Olímpico Universitario'),
    (14, 'Atlas', None, None, 'Puebla', 'Jalisco'),
    (14, 'América', None, None, 'Pachuca', 'Banorte'),
    (14, 'Querétaro', None, None, 'Cruz Azul', 'Corregidora'),
    (14, 'Tijuana', None, None, 'Santos Laguna', 'Caliente'),
    (15, 'Atlético de San Luis', None, None, 'Atlas', 'Libertad Financiera'),
    (15, 'Juárez', None, None, 'Querétaro', 'Olímpico Benito Juárez'),
    (15, 'Puebla', None, None, 'Pumas UNAM', 'Cuauhtémoc'),
    (15, 'Pachuca', None, None, 'Tigres UANL', 'Hidalgo'),
    (15, 'Guadalajara', None, None, 'Atlante', 'Akron'),
    (15, 'Monterrey', None, None, 'Tijuana', 'BBVA'),
    (15, 'América', None, None, 'Toluca', 'Banorte'),
    (15, 'Santos Laguna', None, None, 'Necaxa', 'Corona'),
    (15, 'Cruz Azul', None, None, 'León', 'Banorte'),
    (16, 'Atlético de San Luis', None, None, 'Juárez', 'Libertad Financiera'),
    (16, 'Necaxa', None, None, 'Tijuana', 'Victoria'),
    (16, 'Atlante', None, None, 'Santos Laguna', 'Banorte'),
    (16, 'Atlas', None, None, 'Pachuca', 'Jalisco'),
    (16, 'Tigres UANL', None, None, 'Cruz Azul', 'Universitario'),
    (16, 'Toluca', None, None, 'Monterrey', 'Nemesio Díez'),
    (16, 'Pumas UNAM', None, None, 'América', 'Olímpico Universitario'),
    (16, 'Querétaro', None, None, 'Puebla', 'Corregidora'),
    (16, 'León', None, None, 'Guadalajara', 'León'),
    (17, 'Puebla', None, None, 'Atlético de San Luis', 'Cuauhtémoc'),
    (17, 'Juárez', None, None, 'Atlas', 'Olímpico Benito Juárez'),
    (17, 'Tijuana', None, None, 'Atlante', 'Caliente'),
    (17, 'Santos Laguna', None, None, 'León', 'Corona'),
    (17, 'Pachuca', None, None, 'Toluca', 'Hidalgo'),
    (17, 'Pumas UNAM', None, None, 'Monterrey', 'Olímpico Universitario'),
    (17, 'Tigres UANL', None, None, 'América', 'Universitario'),
    (17, 'Guadalajara', None, None, 'Cruz Azul', 'Akron'),
    (17, 'Querétaro', None, None, 'Necaxa', 'Corregidora'),
]

EQUIPO_MAP = {
    "Atlético de San Luis": "Atlético San Luis",
    "Toluca": "Deportivo Toluca",
    "Juárez": "FC Juárez",
}

ESTADIO_MAP = {
    "Akron": "Estadio Akron", "BBVA": "Estadio BBVA", "Banorte": "Estadio Banorte",
    "Caliente": "Estadio Caliente", "Corona": "Estadio TSM Corona",
    "Corregidora": "Estadio La Corregidora", "Cuauhtémoc": "Estadio Cuauhtémoc",
    "Hidalgo": "Estadio Hidalgo", "Jalisco": "Estadio Jalisco", "León": "Estadio León",
    "Libertad Financiera": "Estadio Libertad Financiera", "Nemesio Díez": "Estadio Nemesio Díez",
    "Olimpico Universitario": "Estadio Olímpico Universitario",
    "Olímpico Universitario": "Estadio Olímpico Universitario",
    "Olímpico Benito Juárez": "Estadio Olímpico Benito Juárez",
    "Universitario": "Estadio Universitario", "Victoria": "Estadio Victoria",
}


def q(s):
    return "'" + s.replace("'", "''") + "'"


def nombre(e):
    return EQUIPO_MAP.get(e, e)


def main():
    con = sqlite3.connect("file:" + DB_PATH.replace("\\", "/") + "?mode=ro", uri=True)
    equipo_id = {n: i for i, n in con.execute("SELECT EquipoId, Equipo FROM Equipo")}
    estadio_id = {n: i for i, n in con.execute("SELECT EstadioId, Estadio FROM Estadio")}
    estatus_partido = {n: i for i, n in con.execute("SELECT EstatusPartidoId, EstatusPartido FROM EstatusPartido")}
    estatus_jornada = {n: i for i, n in con.execute("SELECT EstatusJornadaId, EstatusJornada FROM EstatusJornada")}
    tipo = {n: i for i, n in con.execute("SELECT TipoResultadoId, TipoResultado FROM TipoResultado")}
    municipio = {n: i for i, n in con.execute("SELECT MunicipioId, Municipio FROM Municipio")}
    ya_existe_temporada = con.execute("SELECT 1 FROM Temporada WHERE Temporada = ?", (TEMPORADA,)).fetchone()
    ya_existe_atlante = ATLANTE[0] in equipo_id
    con.close()

    errores = []
    if ya_existe_temporada:
        errores.append(f"La temporada {TEMPORADA!r} ya existe en liga.db; este script no es re-ejecutable.")
    if ATLANTE[2] not in municipio:
        errores.append(f"Municipio {ATLANTE[2]!r} no existe")

    equipos_fixture = {nombre(p[1]) for p in PARTIDOS} | {nombre(p[4]) for p in PARTIDOS}
    for e in equipos_fixture:
        if e != ATLANTE[0] and e not in equipo_id:
            errores.append(f"Equipo sin resolver: {e!r}")
    for p in PARTIDOS:
        if p[5] not in ESTADIO_MAP or ESTADIO_MAP[p[5]] not in estadio_id:
            errores.append(f"J{p[0]}: estadio sin resolver: {p[5]!r}")

    if len(PARTIDOS) != 153 or any(sum(1 for p in PARTIDOS if p[0] == j) != 9 for j in range(1, 18)):
        errores.append("Se esperaban 17 jornadas x 9 partidos")
    pares = [(nombre(p[1]), nombre(p[4])) for p in PARTIDOS]
    if len(set(pares)) != len(pares):
        errores.append("Hay pares local/visitante repetidos en el calendario")
    for e in equipos_fixture:
        n = sum(1 for a, b in pares if e in (a, b))
        if n != 17:
            errores.append(f"{e}: {n} partidos (se esperaban 17)")
    for j in range(1, 18):
        eqs = [x for p in PARTIDOS if p[0] == j for x in (nombre(p[1]), nombre(p[4]))]
        if len(set(eqs)) != 18:
            errores.append(f"J{j}: un equipo aparece dos veces en la jornada")
    if errores:
        print("ERRORES:")
        for e in errores:
            print(" -", e)
        sys.exit(1)

    sql = ["-- Generado por Scripts/gen_apertura2026.py. Respaldar liga.db antes de aplicar.",
           "BEGIN TRANSACTION;",
           f"INSERT INTO Temporada (Temporada, Comentarios) VALUES ({q(TEMPORADA)}, {q(COMENTARIOS)});"]
    sel_temp = f"(SELECT TemporadaId FROM Temporada WHERE Temporada = {q(TEMPORADA)})"

    # Atlante (equipo nuevo) y sus Partido contra el resto de equipos del catálogo
    if not ya_existe_atlante:
        sql.append(f"INSERT INTO Equipo (Equipo, Alias, MunicipioId, EquipoLogo) VALUES "
                   f"({q(ATLANTE[0])}, {q(ATLANTE[1])}, {municipio[ATLANTE[2]]}, NULL);")
    sel_atl = f"(SELECT EquipoId FROM Equipo WHERE Equipo = {q(ATLANTE[0])})"
    for otro, oid in sorted(equipo_id.items(), key=lambda x: x[1]):
        if otro == ATLANTE[0]:
            continue
        sql.append(f"INSERT INTO Partido (EquipoLocalId, EquipoVisitaId) VALUES ({sel_atl}, {oid});  -- Atlante vs {otro}")
        sql.append(f"INSERT INTO Partido (EquipoLocalId, EquipoVisitaId) VALUES ({oid}, {sel_atl});  -- {otro} vs Atlante")

    def sel_eq(n):
        return sel_atl if n == ATLANTE[0] else str(equipo_id[n])

    # Jornadas
    finalizada = {}
    for j in range(1, 18):
        todas = all(p[2] is not None for p in PARTIDOS if p[0] == j)
        finalizada[j] = todas
        est = estatus_jornada["Finalizada" if todas else "Pendiente"]
        sql.append(f"INSERT INTO Jornada (Orden, Jornada, TemporadaId, EstatusJornadaId) "
                   f"VALUES ({j}, {q('Jornada %d' % j)}, {sel_temp}, {est});")

    # JornadaPartido
    jugados = 0
    for j, loc, gl, gv, vis, est in PARTIDOS:
        l, v = nombre(loc), nombre(vis)
        sel_jor = f"(SELECT JornadaId FROM Jornada WHERE TemporadaId = {sel_temp} AND Orden = {j})"
        sel_par = f"(SELECT PartidoId FROM Partido WHERE EquipoLocalId = {sel_eq(l)} AND EquipoVisitaId = {sel_eq(v)})"
        if gl is None:
            gl = gv = 0
            ep, tr = estatus_partido["Pendiente"], tipo["Empate"]
        else:
            jugados += 1
            ep = estatus_partido["Finalizado"]
            tr = tipo["Victoria Local"] if gl > gv else tipo["Victoria Visita"] if gl < gv else tipo["Empate"]
        sql.append(f"INSERT INTO JornadaPartido (JornadaId, PartidoId, EstadioId, GolLocal, GolVisita, EstatusPartidoId, TipoResultadoId) "
                   f"VALUES ({sel_jor}, {sel_par}, {estadio_id[ESTADIO_MAP[est]]}, {gl}, {gv}, {ep}, {tr});  -- J{j} {l} vs {v}")
    sql.append("COMMIT;")

    with open(OUT_SQL, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(sql) + "\n")
    print(f"OK: {len(PARTIDOS)} partidos ({jugados} jugados, {len(PARTIDOS) - jugados} pendientes); "
          f"jornadas finalizadas: {[j for j, f_ in finalizada.items() if f_]}")
    print("SQL en", OUT_SQL)


if __name__ == "__main__":
    main()
