# -*- coding: utf-8 -*-
"""
Genera el script SQL de carga para JornadaPartido, temporada Clausura 2026,
a partir de los resultados reales de la fase regular (17 jornadas, 153 partidos)
consultados en Wikipedia (Anexo:Torneo Clausura 2026 (Mexico) - Fase regular).

Solo LEE liga.db (para resolver ids reales) - no escribe en la base de datos.
Produce Scripts/clausura2026_jornadapartido.sql con los INSERT statements.

Uso: ejecutar desde la raíz del repo con `python Scripts/gen_jornadapartido.py`.
"""
import os
import sqlite3
import sys

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DB_PATH = os.path.join(BASE_DIR, "liga.db")
OUT_SQL = os.path.join(BASE_DIR, "Scripts", "clausura2026_jornadapartido.sql")

# (jornada, local, golLocal, golVisita, visita, estadio_fuente)
PARTIDOS = [
    # Jornada 1
    (1, "Mazatlán", 1, 2, "Juárez", "El Encanto"),
    (1, "Atlas", 1, 0, "Puebla", "Jalisco"),
    (1, "Tijuana", 0, 0, "América", "Caliente"),
    (1, "Guadalajara", 2, 0, "Pachuca", "Akron"),
    (1, "León", 2, 1, "Cruz Azul", "León"),
    (1, "Santos Laguna", 1, 3, "Necaxa", "Corona"),
    (1, "Monterrey", 0, 1, "Toluca", "BBVA"),
    (1, "Pumas UNAM", 1, 1, "Querétaro", "Olímpico Universitario"),
    (1, "Atlético de San Luis", 1, 2, "Tigres UANL", "Libertad Financiera"),
    # Jornada 2
    (2, "Puebla", 2, 1, "Mazatlán", "Cuauhtémoc"),
    (2, "Necaxa", 0, 2, "Monterrey", "Victoria"),
    (2, "Pachuca", 2, 1, "León", "Hidalgo"),
    (2, "Juárez", 0, 1, "Guadalajara", "Olímpico Benito Juárez"),
    (2, "Cruz Azul", 2, 0, "Atlas", "Cuauhtémoc"),
    (2, "Querétaro", 1, 2, "Tijuana", "Corregidora"),
    (2, "América", 0, 2, "Atlético de San Luis", "Ciudad de los Deportes"),
    (2, "Tigres UANL", 0, 1, "Pumas UNAM", "Universitario"),
    (2, "Toluca", 3, 1, "Santos Laguna", "Nemesio Díez"),
    # Jornada 3
    (3, "Mazatlán", 1, 5, "Monterrey", "El Encanto"),
    (3, "Necaxa", 0, 1, "Atlas", "Victoria"),
    (3, "Guadalajara", 2, 1, "Querétaro", "Akron"),
    (3, "Tigres UANL", 0, 0, "Toluca", "Universitario"),
    (3, "Cruz Azul", 1, 0, "Puebla", "Cuauhtémoc"),
    (3, "Tijuana", 1, 1, "Atlético de San Luis", "Caliente"),
    (3, "Pumas UNAM", 1, 1, "León", "Olímpico Universitario"),
    (3, "Santos Laguna", 2, 2, "Juárez", "Corona"),
    (3, "Pachuca", 0, 0, "América", "Hidalgo"),
    # Jornada 4
    (4, "Puebla", 0, 0, "Toluca", "Cuauhtémoc"),
    (4, "Pumas UNAM", 4, 0, "Santos Laguna", "Olímpico Universitario"),
    (4, "Juárez", 3, 4, "Cruz Azul", "Olímpico Benito Juárez"),
    (4, "América", 2, 0, "Necaxa", "Ciudad de los Deportes"),
    (4, "Atlético de San Luis", 2, 3, "Guadalajara", "Libertad Financiera"),
    (4, "Atlas", 1, 0, "Mazatlán", "Jalisco"),
    (4, "Monterrey", 2, 2, "Tijuana", "BBVA"),
    (4, "León", 1, 2, "Tigres UANL", "León"),
    (4, "Querétaro", 0, 0, "Pachuca", "Corregidora"),
    # Jornada 5
    (5, "Tigres UANL", 5, 1, "Santos Laguna", "Universitario"),
    (5, "Necaxa", 4, 1, "Atlético de San Luis", "Victoria"),
    (5, "Tijuana", 0, 0, "Puebla", "Caliente"),
    (5, "Mazatlán", 1, 2, "Guadalajara", "El Encanto"),
    (5, "Querétaro", 2, 0, "León", "Corregidora"),
    (5, "Toluca", 1, 1, "Cruz Azul", "Nemesio Díez"),
    (5, "Atlas", 2, 2, "Pumas UNAM", "Jalisco"),
    (5, "Pachuca", 2, 0, "Juárez", "Hidalgo"),
    (5, "América", 1, 0, "Monterrey", "Ciudad de los Deportes"),
    # Jornada 6
    (6, "Puebla", 2, 3, "Pumas UNAM", "Cuauhtémoc"),
    (6, "Toluca", 1, 0, "Tijuana", "Nemesio Díez"),
    (6, "Atlético de San Luis", 3, 0, "Querétaro", "Libertad Financiera"),
    (6, "Pachuca", 3, 1, "Atlas", "Hidalgo"),
    (6, "Monterrey", 1, 0, "León", "BBVA"),
    (6, "Juárez", 1, 2, "Necaxa", "Olímpico Benito Juárez"),
    (6, "Guadalajara", 1, 0, "América", "Akron"),
    (6, "Cruz Azul", 2, 1, "Tigres UANL", "Cuauhtémoc"),
    (6, "Santos Laguna", 1, 2, "Mazatlán", "Corona"),
    # Jornada 7
    (7, "Tigres UANL", 1, 2, "Pachuca", "Universitario"),
    (7, "Puebla", 0, 4, "América", "Cuauhtémoc"),
    (7, "Atlas", 3, 2, "Atlético de San Luis", "Jalisco"),
    (7, "León", 2, 1, "Santos Laguna", "León"),
    (7, "Necaxa", 0, 3, "Toluca", "Victoria"),
    (7, "Cruz Azul", 2, 1, "Guadalajara", "Cuauhtémoc"),
    (7, "Tijuana", 1, 1, "Mazatlán", "Caliente"),
    (7, "Pumas UNAM", 2, 0, "Monterrey", "Olímpico Universitario"),
    (7, "Querétaro", 1, 1, "Juárez", "Corregidora"),
    # Jornada 8
    (8, "Querétaro", 2, 2, "Santos Laguna", "Corregidora"),
    (8, "Mazatlán", 1, 0, "Pachuca", "El Encanto"),
    (8, "Juárez", 3, 1, "Atlas", "Olímpico Benito Juárez"),
    (8, "Tijuana", 1, 1, "Pumas UNAM", "Caliente"),
    (8, "Atlético de San Luis", 0, 1, "Puebla", "Libertad Financiera"),
    (8, "Toluca", 2, 0, "Guadalajara", "Nemesio Díez"),
    (8, "Monterrey", 0, 2, "Cruz Azul", "BBVA"),
    (8, "León", 2, 1, "Necaxa", "León"),
    (8, "América", 1, 4, "Tigres UANL", "Ciudad de los Deportes"),
    # Jornada 9
    (9, "Pachuca", 2, 1, "Necaxa", "Hidalgo"),
    (9, "Santos Laguna", 1, 2, "Cruz Azul", "Corona"),
    (9, "Atlético de San Luis", 4, 1, "Mazatlán", "Libertad Financiera"),
    (9, "Pumas UNAM", 2, 3, "Toluca", "Olímpico Universitario"),
    (9, "Monterrey", 4, 0, "Querétaro", "BBVA"),
    (9, "Puebla", 3, 1, "Tigres UANL", "Cuauhtémoc"),
    (9, "Atlas", 2, 1, "Tijuana", "Jalisco"),
    (9, "América", 1, 2, "Juárez", "Ciudad de los Deportes"),
    (9, "Guadalajara", 5, 0, "León", "Akron"),
    # Jornada 10
    (10, "Mazatlán", 4, 2, "León", "El Encanto"),
    (10, "Necaxa", 0, 1, "Pumas UNAM", "Victoria"),
    (10, "Cruz Azul", 3, 0, "Atlético de San Luis", "Cuauhtémoc"),
    (10, "Querétaro", 1, 2, "América", "Corregidora"),
    (10, "Atlas", 1, 2, "Guadalajara", "Jalisco"),
    (10, "Pachuca", 2, 1, "Puebla", "Hidalgo"),
    (10, "Tigres UANL", 1, 0, "Monterrey", "Universitario"),
    (10, "Toluca", 3, 1, "Juárez", "Nemesio Díez"),
    (10, "Tijuana", 1, 2, "Santos Laguna", "Caliente"),
    # Jornada 11
    (11, "Puebla", 0, 0, "Necaxa", "Cuauhtémoc"),
    (11, "Juárez", 2, 2, "Monterrey", "Olímpico Benito Juárez"),
    (11, "Tigres UANL", 0, 0, "Querétaro", "Universitario"),
    (11, "Atlético de San Luis", 1, 1, "Pachuca", "Libertad Financiera"),
    (11, "Guadalajara", 3, 0, "Santos Laguna", "Akron"),
    (11, "León", 0, 3, "Tijuana", "León"),
    (11, "Toluca", 1, 1, "Atlas", "Nemesio Díez"),
    (11, "Pumas UNAM", 2, 2, "Cruz Azul", "Olímpico Universitario"),
    (11, "América", 2, 0, "Mazatlán", "Ciudad de los Deportes"),
    # Jornada 12
    (12, "Necaxa", 3, 0, "Tijuana", "Victoria"),
    (12, "Mazatlán", 1, 1, "Cruz Azul", "El Encanto"),
    (12, "Atlas", 0, 0, "Querétaro", "Jalisco"),
    (12, "Atlético de San Luis", 1, 2, "León", "Libertad Financiera"),
    (12, "Monterrey", 2, 3, "Guadalajara", "BBVA"),
    (12, "Pumas UNAM", 1, 0, "América", "Olímpico Universitario"),
    (12, "Santos Laguna", 2, 1, "Puebla", "Corona"),
    (12, "Pachuca", 1, 1, "Toluca", "Hidalgo"),
    (12, "Juárez", 2, 1, "Tigres UANL", "Olímpico Benito Juárez"),
    # Jornada 13
    (13, "Puebla", 1, 1, "Juárez", "Cuauhtémoc"),
    (13, "Necaxa", 2, 1, "Mazatlán", "Victoria"),
    (13, "Tijuana", 1, 0, "Tigres UANL", "Caliente"),
    (13, "Monterrey", 1, 2, "Atlético de San Luis", "BBVA"),
    (13, "Querétaro", 1, 0, "Toluca", "Corregidora"),
    (13, "Cruz Azul", 1, 2, "Pachuca", "Cuauhtémoc"),
    (13, "León", 2, 0, "Atlas", "León"),
    (13, "Santos Laguna", 1, 1, "América", "Corona"),
    (13, "Guadalajara", 2, 2, "Pumas UNAM", "Akron"),
    # Jornada 14
    (14, "Puebla", 0, 1, "León", "Cuauhtémoc"),
    (14, "Juárez", 1, 2, "Tijuana", "Olímpico Benito Juárez"),
    (14, "Querétaro", 3, 1, "Necaxa", "Corregidora"),
    (14, "Tigres UANL", 4, 1, "Guadalajara", "Universitario"),
    (14, "Atlas", 0, 0, "Monterrey", "Jalisco"),
    (14, "Pachuca", 4, 2, "Santos Laguna", "Hidalgo"),
    (14, "América", 1, 1, "Cruz Azul", "Banorte"),
    (14, "Pumas UNAM", 3, 1, "Mazatlán", "Olímpico Universitario"),
    (14, "Toluca", 1, 1, "Atlético de San Luis", "Nemesio Díez"),
    # Jornada 15
    (15, "Atlético de San Luis", 0, 2, "Pumas UNAM", "Libertad Financiera"),
    (15, "Mazatlán", 1, 1, "Querétaro", "El Encanto"),
    (15, "Necaxa", 1, 1, "Tigres UANL", "Victoria"),
    (15, "Cruz Azul", 1, 1, "Tijuana", "Cuauhtémoc"),
    (15, "Monterrey", 1, 3, "Pachuca", "BBVA"),
    (15, "Guadalajara", 5, 0, "Puebla", "Akron"),
    (15, "León", 3, 1, "Juárez", "León"),
    (15, "América", 2, 1, "Toluca", "Banorte"),
    (15, "Santos Laguna", 0, 1, "Atlas", "Corona"),
    # Jornada 16
    (16, "Querétaro", 1, 1, "Cruz Azul", "Corregidora"),
    (16, "Pumas UNAM", 4, 2, "Juárez", "Olímpico Universitario"),
    (16, "Monterrey", 2, 1, "Puebla", "BBVA"),
    (16, "León", 2, 3, "América", "León"),
    (16, "Atlas", 0, 0, "Tigres UANL", "Jalisco"),
    (16, "Atlético de San Luis", 2, 0, "Santos Laguna", "Libertad Financiera"),
    (16, "Mazatlán", 4, 3, "Toluca", "El Encanto"),
    (16, "Tijuana", 3, 1, "Pachuca", "Caliente"),
    (16, "Necaxa", 0, 0, "Guadalajara", "Victoria"),
    # Jornada 17
    (17, "Puebla", 1, 2, "Querétaro", "Cuauhtémoc"),
    (17, "Pachuca", 0, 2, "Pumas UNAM", "Hidalgo"),
    (17, "Tigres UANL", 5, 1, "Mazatlán", "Universitario"),
    (17, "Toluca", 4, 1, "León", "Nemesio Díez"),
    (17, "Guadalajara", 0, 0, "Tijuana", "Akron"),
    (17, "Juárez", 2, 1, "Atlético de San Luis", "Olímpico Benito Juárez"),
    (17, "América", 0, 1, "Atlas", "Banorte"),
    (17, "Santos Laguna", 3, 0, "Monterrey", "Corona"),
    (17, "Cruz Azul", 4, 1, "Necaxa", "Banorte"),
]

EQUIPO_MAP = {
    "América": "América",
    "Atlas": "Atlas",
    "Atlético de San Luis": "Atlético San Luis",
    "Cruz Azul": "Cruz Azul",
    "Toluca": "Deportivo Toluca",
    "Juárez": "FC Juárez",
    "Guadalajara": "Guadalajara",
    "León": "León",
    "Mazatlán": "Mazatlán FC",
    "Monterrey": "Monterrey",
    "Necaxa": "Necaxa",
    "Pachuca": "Pachuca",
    "Puebla": "Puebla",
    "Pumas UNAM": "Pumas UNAM",
    "Querétaro": "Querétaro",
    "Santos Laguna": "Santos Laguna",
    "Tigres UANL": "Tigres UANL",
    "Tijuana": "Tijuana",
}

ESTADIO_MAP = {
    "El Encanto": "Estadio El Encanto",
    "Jalisco": "Estadio Jalisco",
    "Caliente": "Estadio Caliente",
    "Akron": "Estadio Akron",
    "Akrón": "Estadio Akron",
    "León": "Estadio León",
    "Corona": "Estadio TSM Corona",
    "BBVA": "Estadio BBVA",
    "Olímpico Universitario": "Estadio Olímpico Universitario",
    "Alfonso Lastras Ramírez": "Estadio Libertad Financiera",
    "Libertad Financiera": "Estadio Libertad Financiera",
    "Cuauhtémoc": "Estadio Cuauhtémoc",
    "Victoria": "Estadio Victoria",
    "Hidalgo": "Estadio Hidalgo",
    "Olímpico Benito Juárez": "Estadio Olímpico Benito Juárez",
    "Corregidora": "Estadio La Corregidora",
    # Estadio Ciudad de los Deportes no existe en nuestro catálogo (17 filas para
    # 18 equipos): fue la sede alterna real de América mientras se remodelaba su
    # estadio habitual; se mapea al único estadio de América en el catálogo.
    "Ciudad de los Deportes": "Estadio Banorte",
    "Universitario": "Estadio Universitario",
    "Nemesio Díez": "Estadio Nemesio Díez",
    "Banorte": "Estadio Banorte",
}


def main():
    con = sqlite3.connect(DB_PATH)
    cur = con.cursor()

    cur.execute("SELECT EquipoId, Equipo FROM Equipo")
    equipo_id = {nombre: eid for eid, nombre in cur.fetchall()}

    cur.execute("SELECT EstadioId, Estadio FROM Estadio")
    estadio_id = {nombre: eid for eid, nombre in cur.fetchall()}

    cur.execute("SELECT JornadaId, Orden FROM Jornada WHERE TemporadaId = "
                "(SELECT TemporadaId FROM Temporada WHERE Temporada = 'Clausura 2026')")
    jornada_id = {orden: jid for jid, orden in cur.fetchall()}

    cur.execute("SELECT PartidoId, EquipoLocalId, EquipoVisitaId FROM Partido")
    partido_id = {(loc, vis): pid for pid, loc, vis in cur.fetchall()}

    cur.execute("SELECT EstatusPartidoId FROM EstatusPartido WHERE EstatusPartido = 'Finalizado'")
    estatus_finalizado = cur.fetchone()[0]

    cur.execute("SELECT TipoResultadoId FROM TipoResultado WHERE TipoResultado = ?", ("Victoria Local",))
    tr_local = cur.fetchone()[0]
    cur.execute("SELECT TipoResultadoId FROM TipoResultado WHERE TipoResultado = ?", ("Victoria Visita",))
    tr_visita = cur.fetchone()[0]
    cur.execute("SELECT TipoResultadoId FROM TipoResultado WHERE TipoResultado = ?", ("Empate",))
    tr_empate = cur.fetchone()[0]

    errores = []
    filas = []
    partidos_usados = set()

    for jornada, local, gl, gv, visita, estadio_src in PARTIDOS:
        eq_local = EQUIPO_MAP.get(local)
        eq_visita = EQUIPO_MAP.get(visita)
        est_nombre = ESTADIO_MAP.get(estadio_src)

        if eq_local is None or eq_local not in equipo_id:
            errores.append(f"J{jornada}: equipo local no resuelto: {local!r}")
            continue
        if eq_visita is None or eq_visita not in equipo_id:
            errores.append(f"J{jornada}: equipo visita no resuelto: {visita!r}")
            continue
        if est_nombre is None or est_nombre not in estadio_id:
            errores.append(f"J{jornada}: estadio no resuelto: {estadio_src!r}")
            continue
        if jornada not in jornada_id:
            errores.append(f"Jornada {jornada} no existe en Temporada 'Clausura 2026'")
            continue

        loc_id = equipo_id[eq_local]
        vis_id = equipo_id[eq_visita]
        pid = partido_id.get((loc_id, vis_id))
        if pid is None:
            errores.append(f"J{jornada}: no existe Partido con local={eq_local}({loc_id}) "
                            f"visita={eq_visita}({vis_id})")
            continue
        if pid in partidos_usados:
            errores.append(f"J{jornada}: PartidoId {pid} ({eq_local} vs {eq_visita}) duplicado en el dataset")
            continue
        partidos_usados.add(pid)

        if gl > gv:
            tr = tr_local
        elif gv > gl:
            tr = tr_visita
        else:
            tr = tr_empate

        filas.append((jornada_id[jornada], pid, estadio_id[est_nombre], gl, gv, estatus_finalizado, tr))

    print(f"Partidos procesados: {len(PARTIDOS)}")
    print(f"Filas resueltas correctamente: {len(filas)}")
    print(f"Errores: {len(errores)}")
    for e in errores:
        print(" - " + e)

    if errores:
        print("\nSe encontraron errores; no se genera el script SQL hasta corregirlos.")
        sys.exit(1)

    if len(filas) != 153:
        print(f"\nADVERTENCIA: se esperaban 153 filas y se generaron {len(filas)}.")

    with open(OUT_SQL, "w", encoding="utf-8") as f:
        f.write("-- Carga de JornadaPartido para la Temporada 'Clausura 2026' (fase regular,\n")
        f.write("-- 17 jornadas, 153 partidos), generado a partir de resultados reales.\n")
        f.write("-- Fuente: Wikipedia, Anexo:Torneo Clausura 2026 (Mexico) - Fase regular.\n")
        f.write("-- Generado por Scripts/gen_jornadapartido.py (no ejecutar dos veces sin\n")
        f.write("-- verificar duplicados: JornadaPartido no tiene UNIQUE a nivel de BD).\n\n")
        f.write("BEGIN TRANSACTION;\n\n")
        for jid, pid, eid, gl, gv, est, tr in filas:
            gl_sql = "NULL" if gl is None else str(gl)
            gv_sql = "NULL" if gv is None else str(gv)
            f.write(
                "INSERT INTO JornadaPartido (JornadaId, PartidoId, EstadioId, GolLocal, GolVisita, "
                "EstatusPartidoId, TipoResultadoId) VALUES "
                f"({jid}, {pid}, {eid}, {gl_sql}, {gv_sql}, {est}, {tr});\n"
            )
        f.write("\nCOMMIT;\n")

    print(f"\nScript SQL generado: {OUT_SQL}")

    con.close()


if __name__ == "__main__":
    main()
