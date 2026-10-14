using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    // Pantalla única de pronósticos por participante: Temporada -> Jornada -> Participante
    // -> Buscar. Permite ver, guardar/modificar y limpiar los pronósticos de UNA jornada de
    // UN participante. No es un CRUD clásico: la cabecera (JornadaPronostico) se crea y se
    // borra sola según haya o no detalles. Estrategia completa en PLAN.md.
    public class PronosticoController : Controller
    {
        // Cierre de captura POR PARTIDO: solo se pronostica/modifica/limpia mientras el
        // estatus del partido sea "Pendiente". Se compara por nombre del catálogo
        // EstatusPartido (misma deuda conocida que el resto de reglas por nombre).
        private const string EstatusAbierto = "Pendiente";

        // Tope razonable de goles por pronóstico (evita basura tipo 99999).
        private const int GolesMaximos = 30;

        private readonly ApplicationDbContext _context;

        public PronosticoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Pronostico
        // Igual que JornadaPartido/Index: no carga nada hasta que se presiona "Buscar" y se
        // exigen las tres selecciones (Temporada, Jornada y Participante).
        public async Task<IActionResult> Index(int? temporadaId, int? jornadaId, int? participanteId)
        {
            var huboBusqueda = Request.QueryString.HasValue;

            Jornada? jornadaSel = null;
            if (temporadaId.HasValue && jornadaId.HasValue)
                jornadaSel = await _context.Jornada
                    .Include(j => j.EstatusJornada)
                    .FirstOrDefaultAsync(j => j.JornadaId == jornadaId.Value && j.TemporadaId == temporadaId.Value);

            Participante? participanteSel = participanteId.HasValue
                ? await _context.Participante.FindAsync(participanteId.Value)
                : null;

            var vm = new PronosticoGridViewModel();
            if (jornadaSel != null && participanteSel != null)
            {
                vm.JornadaNombre = jornadaSel.JornadaNombre;
                vm.ParticipanteNombre = NombreCompleto(participanteSel);

                var partidos = await _context.JornadaPartidos
                    .Include(jp => jp.Partido).ThenInclude(p => p.EquipoLocal)
                    .Include(jp => jp.Partido).ThenInclude(p => p.EquipoVisita)
                    .Include(jp => jp.EstatusPartido)
                    .Where(jp => jp.JornadaId == jornadaSel.JornadaId)
                    .OrderBy(jp => jp.JornadaPartidoId)
                    .ToListAsync();

                var detalles = await _context.JornadaPronosticoDetalles
                    .Where(d => d.JornadaPronostico.JornadaId == jornadaSel.JornadaId
                             && d.JornadaPronostico.ParticipanteId == participanteSel.ParticipanteId)
                    .ToDictionaryAsync(d => d.JornadaPartidoId);

                // LEFT JOIN: los partidos sin pronóstico salen con los inputs vacíos (no se
                // pre-crean filas 0-0).
                foreach (var jp in partidos)
                {
                    detalles.TryGetValue(jp.JornadaPartidoId, out var det);
                    vm.Filas.Add(new PronosticoFila
                    {
                        Partido = jp,
                        Detalle = det,
                        Editable = EstaAbierto(jp)
                    });
                }
            }

            var jornadas = temporadaId.HasValue
                ? await _context.Jornada.Where(j => j.TemporadaId == temporadaId.Value).OrderBy(j => j.Orden).ToListAsync()
                : new List<Jornada>();
            var participantes = (await _context.Participante
                .OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoPaterno)
                .ToListAsync())
                .Select(p => new { p.ParticipanteId, Nombre = NombreCompleto(p) });

            ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre", temporadaId);
            ViewData["JornadaId"] = new SelectList(jornadas, "JornadaId", "JornadaNombre", jornadaId);
            ViewData["ParticipanteId"] = new SelectList(participantes, "ParticipanteId", "Nombre", participanteId);
            ViewData["BusquedaRealizada"] = huboBusqueda;
            ViewData["FaltaSeleccion"] = huboBusqueda && (jornadaSel == null || participanteSel == null);
            ViewData["TemporadaId_Sel"] = temporadaId;
            ViewData["JornadaId_Sel"] = jornadaSel?.JornadaId;
            ViewData["ParticipanteId_Sel"] = participanteSel?.ParticipanteId;

            return View(vm);
        }

        // POST: /Pronostico/Guardar
        // Upsert por lote en una sola transacción (un SaveChanges). Por cada fila ENVIADA:
        //  - ambos goles vacíos: se elimina el pronóstico existente de ese partido (si hay);
        //  - un solo gol vacío o inválido: se rechaza el lote completo;
        //  - ambos goles válidos: se inserta/actualiza el detalle. El resultado se deriva
        //    SIEMPRE de los goles en el servidor. NO se toca Puntos (solo lectura aquí).
        // Las filas de partidos que ya no están "Pendiente" se ignoran (cierre por partido):
        // la vista ni siquiera las renderiza como inputs.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(int temporadaId, int jornadaId, int participanteId, List<PronosticoFilaForm>? filas)
        {
            var volver = new { temporadaId, jornadaId, participanteId };

            var jornada = await _context.Jornada.FirstOrDefaultAsync(j => j.JornadaId == jornadaId && j.TemporadaId == temporadaId);
            var participante = await _context.Participante.FindAsync(participanteId);
            if (jornada == null || participante == null)
                return NotFound();

            filas ??= new List<PronosticoFilaForm>();

            var tipos = await _context.TipoResultado.ToDictionaryAsync(t => t.TipoResultadoNombre, t => t.TipoResultadoId);
            if (!tipos.ContainsKey("Victoria Local") || !tipos.ContainsKey("Victoria Visita") || !tipos.ContainsKey("Empate"))
            {
                TempData["Error"] = "Faltan tipos de resultado en el catálogo (Victoria Local / Victoria Visita / Empate).";
                return RedirectToAction(nameof(Index), volver);
            }

            // Solo se aceptan partidos de ESTA jornada (nunca se confía en los ids del cliente).
            var partidos = await _context.JornadaPartidos
                .Include(jp => jp.EstatusPartido)
                .Where(jp => jp.JornadaId == jornadaId)
                .ToDictionaryAsync(jp => jp.JornadaPartidoId);

            var cabecera = await _context.JornadaPronosticos
                .Include(c => c.JornadaPronosticoDetalles)
                .FirstOrDefaultAsync(c => c.JornadaId == jornadaId && c.ParticipanteId == participanteId);
            var existentes = cabecera?.JornadaPronosticoDetalles.ToDictionary(d => d.JornadaPartidoId)
                ?? new Dictionary<int, JornadaPronosticoDetalle>();

            var aGuardar = new List<(JornadaPartido jp, int gl, int gv)>();
            var aEliminar = new List<JornadaPronosticoDetalle>();
            var vistos = new HashSet<int>();

            foreach (var fila in filas)
            {
                if (!partidos.TryGetValue(fila.JornadaPartidoId, out var jp))
                {
                    TempData["Error"] = "Se envió un partido que no pertenece a la jornada seleccionada.";
                    return RedirectToAction(nameof(Index), volver);
                }
                if (!vistos.Add(fila.JornadaPartidoId))
                {
                    TempData["Error"] = "Petición inválida: partido repetido.";
                    return RedirectToAction(nameof(Index), volver);
                }
                if (!EstaAbierto(jp))
                    continue; // cierre de captura por partido

                var txtL = fila.GolLocal?.Trim();
                var txtV = fila.GolVisita?.Trim();
                var vacioL = string.IsNullOrEmpty(txtL);
                var vacioV = string.IsNullOrEmpty(txtV);

                if (vacioL && vacioV)
                {
                    if (existentes.TryGetValue(jp.JornadaPartidoId, out var previo))
                        aEliminar.Add(previo);
                    continue;
                }

                if (!TryParseGoles(txtL, out var gl) || !TryParseGoles(txtV, out var gv))
                {
                    TempData["Error"] = $"Los goles deben ser números enteros entre 0 y {GolesMaximos} y capturarse completos (local y visita) o dejarse ambos vacíos. No se guardó nada.";
                    return RedirectToAction(nameof(Index), volver);
                }

                aGuardar.Add((jp, gl, gv));
            }

            int cambios = 0;
            foreach (var (jp, gl, gv) in aGuardar)
            {
                var tipo = CalcularTipoResultado(gl, gv, tipos);
                if (existentes.TryGetValue(jp.JornadaPartidoId, out var det))
                {
                    if (det.GolLocal == gl && det.GolVisita == gv && det.TipoResultadoId == tipo)
                        continue; // sin cambios
                    det.GolLocal = gl;
                    det.GolVisita = gv;
                    det.TipoResultadoId = tipo;
                    cambios++;
                }
                else
                {
                    if (cabecera == null)
                    {
                        cabecera = new JornadaPronostico { JornadaId = jornadaId, ParticipanteId = participanteId };
                        _context.JornadaPronosticos.Add(cabecera);
                    }
                    _context.JornadaPronosticoDetalles.Add(new JornadaPronosticoDetalle
                    {
                        JornadaPronostico = cabecera,
                        JornadaPartidoId = jp.JornadaPartidoId,
                        GolLocal = gl,
                        GolVisita = gv,
                        TipoResultadoId = tipo
                    });
                    cambios++;
                }
            }

            if (aEliminar.Count > 0)
            {
                _context.JornadaPronosticoDetalles.RemoveRange(aEliminar);
                cambios += aEliminar.Count;
            }

            // Si tras el lote la cabecera existente se queda sin detalles, se borra.
            var restantes = existentes.Count - aEliminar.Count + aGuardar.Count(a => !existentes.ContainsKey(a.jp.JornadaPartidoId));
            if (cabecera != null && cabecera.JornadaPronosticoId != 0 && restantes == 0)
                _context.JornadaPronosticos.Remove(cabecera);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se pudo guardar (posible captura simultánea del mismo participante). Vuelve a intentarlo.";
                return RedirectToAction(nameof(Index), volver);
            }

            TempData["Mensaje"] = cambios == 0
                ? "No hubo cambios que guardar."
                : $"Pronósticos de {NombreCompleto(participante)} actualizados ({cambios} cambio{(cambios == 1 ? "" : "s")}).";
            return RedirectToAction(nameof(Index), volver);
        }

        // POST: /Pronostico/Limpiar
        // Borra los pronósticos de los partidos todavía abiertos (Pendiente) de ese
        // participante en la jornada; los de partidos ya iniciados/finalizados se conservan
        // (cierre por partido). Si la cabecera se queda sin detalles, también se borra.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Limpiar(int temporadaId, int jornadaId, int participanteId)
        {
            var volver = new { temporadaId, jornadaId, participanteId };

            var jornada = await _context.Jornada.FirstOrDefaultAsync(j => j.JornadaId == jornadaId && j.TemporadaId == temporadaId);
            var participante = await _context.Participante.FindAsync(participanteId);
            if (jornada == null || participante == null)
                return NotFound();

            var cabecera = await _context.JornadaPronosticos
                .Include(c => c.JornadaPronosticoDetalles).ThenInclude(d => d.JornadaPartido).ThenInclude(jp => jp.EstatusPartido)
                .FirstOrDefaultAsync(c => c.JornadaId == jornadaId && c.ParticipanteId == participanteId);
            if (cabecera == null || cabecera.JornadaPronosticoDetalles.Count == 0)
            {
                TempData["Mensaje"] = "No había pronósticos que limpiar.";
                return RedirectToAction(nameof(Index), volver);
            }

            var borrables = cabecera.JornadaPronosticoDetalles.Where(d => EstaAbierto(d.JornadaPartido)).ToList();
            var conservados = cabecera.JornadaPronosticoDetalles.Count - borrables.Count;

            _context.JornadaPronosticoDetalles.RemoveRange(borrables);
            if (conservados == 0)
                _context.JornadaPronosticos.Remove(cabecera);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = borrables.Count == 0
                ? "Ningún pronóstico se puede limpiar: todos sus partidos ya no están Pendientes."
                : $"Se limpiaron {borrables.Count} pronóstico{(borrables.Count == 1 ? "" : "s")}"
                  + (conservados > 0 ? $"; {conservados} se conservaron porque su partido ya no está Pendiente." : ".");
            return RedirectToAction(nameof(Index), volver);
        }

        private static bool EstaAbierto(JornadaPartido jp)
        {
            return string.Equals(jp.EstatusPartido?.EstatusPartidoNombre?.Trim(), EstatusAbierto, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseGoles(string? texto, out int goles)
        {
            goles = 0;
            return !string.IsNullOrEmpty(texto)
                && texto.All(char.IsAsciiDigit)
                && int.TryParse(texto, out goles)
                && goles <= GolesMaximos;
        }

        // Mismo criterio que JornadaPartidoController: el resultado se deriva de los goles.
        private static int CalcularTipoResultado(int golLocal, int golVisita, Dictionary<string, int> tipos)
        {
            return golLocal > golVisita ? tipos["Victoria Local"]
                : golLocal < golVisita ? tipos["Victoria Visita"]
                : tipos["Empate"];
        }

        private static string NombreCompleto(Participante p)
        {
            return $"{p.Nombres} {p.ApellidoPaterno} {p.ApellidoMaterno}".Trim();
        }
    }

    public class PronosticoGridViewModel
    {
        public string? JornadaNombre { get; set; }
        public string? ParticipanteNombre { get; set; }
        public List<PronosticoFila> Filas { get; set; } = new List<PronosticoFila>();
    }

    public class PronosticoFila
    {
        public JornadaPartido Partido { get; set; } = null!;
        public JornadaPronosticoDetalle? Detalle { get; set; }
        public bool Editable { get; set; }
    }

    public class PronosticoFilaForm
    {
        public int JornadaPartidoId { get; set; }
        public string? GolLocal { get; set; }
        public string? GolVisita { get; set; }
    }
}
