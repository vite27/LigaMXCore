# -*- coding: utf-8 -*-
"""
Descarga el logo de cada equipo de la Liga MX desde Wikipedia en inglés (imagen
principal del infobox del artículo del club, que es su escudo, vía la API de MediaWiki) a wwwroot/img/equipos/ y genera
Scripts/equipos_logos.sql con los UPDATE de Equipo.EquipoLogo.

Solo LEE liga.db (para resolver los ids reales) - no escribe en la base de datos.
Las miniaturas se piden de 64 px de ancho (se muestran a 32 px en pantalla, con
margen para pantallas de alta densidad).

Uso: ejecutar desde la raíz del repo con `python Scripts/gen_logos_equipos.py`.
"""
import json
import os
import re
import sqlite3
import sys
import unicodedata
import urllib.parse
import urllib.request

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DB_PATH = os.path.join(BASE_DIR, "liga.db")
OUT_DIR = os.path.join(BASE_DIR, "wwwroot", "img", "equipos")
OUT_SQL = os.path.join(BASE_DIR, "Scripts", "equipos_logos.sql")
API = "https://en.wikipedia.org/w/api.php"
UA = {"User-Agent": "LigaMXCore-logos/1.0 (proyecto de quiniela interna; mirov27@gmail.com)"}
ANCHO = 64

# Equipo (nombre en liga.db) -> título del artículo en en.wikipedia.org
ARTICULOS = {
    "América": "Club América",
    "Atlante": "Atlante F.C.",
    "Atlas": "Atlas F.C.",
    "Atlético San Luis": "Atlético San Luis",
    "Cruz Azul": "Cruz Azul",
    "Deportivo Toluca": "Deportivo Toluca F.C.",
    "FC Juárez": "FC Juárez",
    "Guadalajara": "C.D. Guadalajara",
    "León": "Club León",
    "Mazatlán FC": "Mazatlán F.C.",
    "Monterrey": "C.F. Monterrey",
    "Necaxa": "Club Necaxa",
    "Pachuca": "C.F. Pachuca",
    "Puebla": "Club Puebla",
    "Pumas UNAM": "Club Universidad Nacional",
    "Querétaro": "Querétaro F.C.",
    "Santos Laguna": "Santos Laguna",
    "Tigres UANL": "Tigres UANL",
    "Tijuana": "Club Tijuana",
}


def slug(nombre):
    s = unicodedata.normalize("NFD", nombre)
    s = "".join(c for c in s if unicodedata.category(c) != "Mn")
    return "-".join("".join(c.lower() if c.isalnum() else " " for c in s).split())


def get(url):
    req = urllib.request.Request(url, headers=UA)
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read()


def api(**params):
    params.update({"format": "json", "redirects": 1})
    return json.loads(get(API + "?" + urllib.parse.urlencode(params)))


def archivo_logo(titulo):
    """Nombre del archivo del escudo según el parámetro logo/image del infobox."""
    data = api(action="query", prop="revisions", rvprop="content", rvslots="main", titles=titulo)
    for page in data["query"]["pages"].values():
        if "missing" in page:
            return None
        wikitext = page["revisions"][0]["slots"]["main"]["*"]
        m = re.search(r"\|\s*(?:logo|crest|image)\s*=\s*(?:\[\[)?(?:File:|Image:)?([^\n|\]]+?\.(?:svg|png|jpg|jpeg|gif))",
                      wikitext[:8000], re.I)
        return m.group(1).strip() if m else None
    return None


def miniatura(titulo):
    archivo = archivo_logo(titulo)
    if not archivo:
        return None
    data = api(action="query", prop="imageinfo", iiprop="url", iiurlwidth=ANCHO, titles="File:" + archivo)
    for page in data["query"]["pages"].values():
        info = page.get("imageinfo")
        return (info[0].get("thumburl") or info[0]["url"]) if info else None
    return None


def main():
    con = sqlite3.connect("file:" + DB_PATH.replace("\\", "/") + "?mode=ro", uri=True)
    equipos = dict((n, i) for i, n in con.execute("SELECT EquipoId, Equipo FROM Equipo"))
    con.close()

    faltan = [n for n in equipos if n not in ARTICULOS]
    sobran = [n for n in ARTICULOS if n not in equipos]
    if faltan or sobran:
        print("Equipos sin artículo:", faltan, "| artículos sin equipo:", sobran)
        sys.exit(1)

    os.makedirs(OUT_DIR, exist_ok=True)
    sentencias, errores = [], []
    for nombre, eid in sorted(equipos.items(), key=lambda x: x[1]):
        try:
            url = miniatura(ARTICULOS[nombre])
            if not url:
                errores.append(f"{nombre}: el artículo no tiene imagen principal")
                continue
            ext = os.path.splitext(urllib.parse.urlparse(url).path)[1].lower() or ".png"
            archivo = slug(nombre) + ext
            with open(os.path.join(OUT_DIR, archivo), "wb") as f:
                f.write(get(url))
            ruta = "/img/equipos/" + archivo
            sentencias.append(f"UPDATE Equipo SET EquipoLogo = '{ruta}' WHERE EquipoId = {eid};  -- {nombre}")
            print(f"OK  {nombre:20} <- {url}")
        except Exception as ex:  # noqa: BLE001
            errores.append(f"{nombre}: {ex}")

    if errores:
        print("\nERRORES:")
        for e in errores:
            print(" -", e)
        sys.exit(1)

    with open(OUT_SQL, "w", encoding="utf-8", newline="\n") as f:
        f.write("-- Generado por Scripts/gen_logos_equipos.py. Respaldar liga.db antes de aplicar.\n")
        f.write("BEGIN TRANSACTION;\n" + "\n".join(sentencias) + "\nCOMMIT;\n")
    print(f"\n{len(sentencias)} logos descargados; SQL en {OUT_SQL}")


if __name__ == "__main__":
    main()
