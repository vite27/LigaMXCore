using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class JornadaPartidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JornadaPartidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /JornadaPartido
        // No muestra ningún registro hasta que el usuario elige una Temporada y
        // presiona "Buscar" (único catálogo de la Fase A con este comportamiento):
        // su Index es además el grid editable de captura de marcadores
        // (UpdateScores), así que cargar todo el historial de temporadas por
        // defecto renderizaría cientos de inputs editables sin que el usuario lo
        // haya pedido. Sin Temporada seleccionada, no se devuelve nada (decisión
        // de producto explícita, a diferencia del resto de catálogos donde "sin
        // filtro" significa "mostrar todo").
        public async Task<IActionResult> Index(int? temporadaId, int? jornadaId)
        {
            var huboBusqueda = Request.QueryString.HasValue;
            var list = new List<JornadaPartido>();

            // Se exige Temporada Y Jornada: el grid muestra una sola jornada a la vez
            // (su nombre va como encabezado, no como columna repetida en cada fila).
            Jornada? jornadaSel = null;
            if (temporadaId.HasValue && jornadaId.HasValue)
                jornadaSel = await _context.Jornada.Include(j => j.EstatusJornada).FirstOrDefaultAsync(j => j.JornadaId == jornadaId.Value && j.TemporadaId == temporadaId.Value);

            if (jornadaSel != null)
            {
                var query = _context.JornadaPartidos
                    .Include(j => j.Jornada)
                    .Include(j => j.Partido)
                        .ThenInclude(p => p.EquipoLocal)
                    .Include(j => j.Partido)
                        .ThenInclude(p => p.EquipoVisita)
                    .Include(j => j.Estadio)
                    .Include(j => j.EstatusPartido)
                    .Include(j => j.TipoResultado)
                    .Where(j => j.JornadaId == jornadaSel.JornadaId);

                list = await query.ToListAsync();
            }

            var jornadas = temporadaId.HasValue
                ? await _context.Jornada.Where(j => j.TemporadaId == temporadaId.Value).OrderBy(j => j.Orden).ToListAsync()
                : new List<Jornada>();

            ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre", temporadaId);
            ViewData["JornadaId"] = new SelectList(jornadas, "JornadaId", "JornadaNombre", jornadaId);
            ViewData["BusquedaRealizada"] = huboBusqueda;
            ViewData["JornadaNombre"] = jornadaSel?.JornadaNombre;
            ViewData["FaltaJornada"] = huboBusqueda && jornadaSel == null;
            ViewData["EstatusJornadaNombre"] = jornadaSel?.EstatusJornada.EstatusJornadaNombre;
            ViewData["MarcadoresEditables"] = jornadaSel != null && JornadaPermiteCaptura(jornadaSel);

            return View(list);
        }

        // GET: /JornadaPartido/JornadasPorTemporada/5
        // Usado por el combo en cascada del filtro de Index (ver ligaMxCore.js).
        // Devuelve TODAS las jornadas de la temporada (tengan o no partidos ya
        // programados), porque este catálogo se usa tanto para capturar el
        // fixture como para visualizar/editar resultados ya cargados. Sin
        // antiforgery porque es solo lectura. temporadaId <= 0 devuelve vacío
        // (no tiene sentido listar jornadas sin una temporada elegida).
        [HttpGet("JornadaPartido/JornadasPorTemporada/{temporadaId}")]
        public async Task<IActionResult> JornadasPorTemporada(int temporadaId)
        {
            if (temporadaId <= 0)
                return Json(new List<object>());

            var jornadas = await _context.Jornada
                .Where(j => j.TemporadaId == temporadaId)
                .OrderBy(j => j.Orden)
                .Select(j => new { id = j.JornadaId, nombre = j.JornadaNombre })
                .ToListAsync();

            return Json(jornadas);
        }

        // POST: /JornadaPartido/UpdateScores
        [HttpPost]
        public JsonResult UpdateScores([FromBody] List<JornadaPartido> item)
        {
            try
            {
                var tipos = _context.TipoResultado.ToDictionary(t => t.TipoResultadoNombre, t => t.TipoResultadoId);
                if (!tipos.ContainsKey("Victoria Local") || !tipos.ContainsKey("Victoria Visita") || !tipos.ContainsKey("Empate"))
                    return Json(new { success = false, message = "Faltan tipos de resultado en el catálogo (Victoria Local / Victoria Visita / Empate)." });

                if (item == null || item.Count == 0)
                    return Json(new { success = false, message = "No hay marcadores para actualizar." });

                // Se carga todo el lote (con el estatus de su jornada) en una sola consulta y se
                // valida completo ANTES de modificar nada: si alguna fila pertenece a una jornada
                // que no permite captura, se rechaza el lote entero (nunca guardado parcial).
                var ids = item.Select(i => i.JornadaPartidoId).Distinct().ToList();
                var partidosDb = _context.JornadaPartidos
                    .Include(j => j.Jornada).ThenInclude(j => j.EstatusJornada)
                    .Where(j => ids.Contains(j.JornadaPartidoId))
                    .ToDictionary(j => j.JornadaPartidoId);

                var bloqueada = partidosDb.Values.FirstOrDefault(j => !JornadaPermiteCaptura(j.Jornada));
                if (bloqueada != null)
                    return Json(new { success = false, message = $"La {bloqueada.Jornada.JornadaNombre} tiene estatus {bloqueada.Jornada.EstatusJornada.EstatusJornadaNombre}; sus marcadores no se pueden modificar." });

                foreach (var partidoEnviado in item)
                {
                    if (partidoEnviado.GolLocal is null or < 0 || partidoEnviado.GolVisita is null or < 0)
                        return Json(new { success = false, message = "Los goles deben ser números enteros mayores o iguales a 0." });

                    partidosDb.TryGetValue(partidoEnviado.JornadaPartidoId, out var partidoDb);

                    if (partidoDb != null)
                    {
                        partidoDb.GolLocal = partidoEnviado.GolLocal;
                        partidoDb.GolVisita = partidoEnviado.GolVisita;

                        // El resultado no lo captura el usuario (ni se confía en lo que mande el
                        // cliente): se deriva siempre de los goles, porque alimentará el cálculo
                        // de puntos. El estatus es de solo lectura en este grid, tampoco se toma
                        // del cliente.
                        partidoDb.TipoResultadoId = CalcularTipoResultado(partidoDb.GolLocal!.Value, partidoDb.GolVisita!.Value, tipos);

                        _context.Entry(partidoDb).State = EntityState.Modified;
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, message = $"{item.Count} marcadores actualizados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar: " + ex.Message });
            }
        }

        // POST: /JornadaPartido/CambiarEstatus
        // Botón de estatus de la fila: "iniciar" (Pendiente -> En Curso) y "finalizar"
        // (En Curso -> Finalizado). Cualquier otra transición (incluido Pausado, ignorado por
        // ahora) se rechaza, igual que un cambio sobre una jornada que no permite captura
        // (ej. Finalizada). Si la petición trae marcador (la fila tiene textboxes) se guarda
        // en el mismo paso y el resultado se recalcula de los goles, para no iniciar/finalizar
        // con un marcador escrito pero sin guardar. Responde con la fila renderizada
        // (_FilaPartido) para que el cliente reemplace solo esa fila; los errores devuelven
        // texto plano con un código HTTP 4xx.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstatus([FromBody] CambiarEstatusRequest req)
        {
            if (req == null)
                return BadRequest("Petición inválida.");

            var jp = await _context.JornadaPartidos
                .Include(j => j.Jornada).ThenInclude(j => j.EstatusJornada)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoVisita)
                .Include(j => j.Estadio)
                .Include(j => j.EstatusPartido)
                .FirstOrDefaultAsync(j => j.JornadaPartidoId == req.JornadaPartidoId);
            if (jp == null)
                return NotFound("El partido no existe.");

            if (!JornadaPermiteCaptura(jp.Jornada))
                return Conflict($"La {jp.Jornada.JornadaNombre} tiene estatus {jp.Jornada.EstatusJornada.EstatusJornadaNombre}; no se puede cambiar el estatus de sus partidos.");

            string estatusActual = "Pendiente", estatusNuevo = "En Curso";
            switch (req.Accion?.Trim().ToLowerInvariant())
            {
                case "iniciar":
                    break;
                case "finalizar":
                    (estatusActual, estatusNuevo) = ("En Curso", "Finalizado");
                    break;
                default:
                    return BadRequest("Acción no válida.");
            }

            if (!string.Equals(jp.EstatusPartido.EstatusPartidoNombre?.Trim(), estatusActual, StringComparison.OrdinalIgnoreCase))
                return Conflict($"El partido está {jp.EstatusPartido.EstatusPartidoNombre}; solo se puede cambiar a {estatusNuevo} desde {estatusActual}. Actualiza la página.");

            var nuevo = await _context.EstatusPartido.FirstOrDefaultAsync(e => e.EstatusPartidoNombre == estatusNuevo);
            if (nuevo == null)
                return StatusCode(500, $"No se encontró el estatus '{estatusNuevo}' en el catálogo.");

            if (req.GolLocal != null || req.GolVisita != null)
            {
                if (req.GolLocal is null or < 0 || req.GolVisita is null or < 0)
                    return BadRequest("Los goles deben ser números enteros mayores o iguales a 0.");

                var tipos = await _context.TipoResultado.ToDictionaryAsync(t => t.TipoResultadoNombre, t => t.TipoResultadoId);
                if (!tipos.ContainsKey("Victoria Local") || !tipos.ContainsKey("Victoria Visita") || !tipos.ContainsKey("Empate"))
                    return StatusCode(500, "Faltan tipos de resultado en el catálogo (Victoria Local / Victoria Visita / Empate).");

                jp.GolLocal = req.GolLocal;
                jp.GolVisita = req.GolVisita;
                jp.TipoResultadoId = CalcularTipoResultado(req.GolLocal.Value, req.GolVisita.Value, tipos);
            }

            jp.EstatusPartidoId = nuevo.EstatusPartidoId;
            jp.EstatusPartido = nuevo;
            await _context.SaveChangesAsync();

            ViewData["EstatusJornadaNombre"] = jp.Jornada.EstatusJornada.EstatusJornadaNombre;
            ViewData["MarcadoresEditables"] = true; // JornadaPermiteCaptura ya se validó arriba
            return PartialView("_FilaPartido", jp);
        }

        // GET: /JornadaPartido/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var jornadaPartido = await _context.JornadaPartidos
                .Include(j => j.Jornada).ThenInclude(j => j.Temporada)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoVisita)
                .Include(j => j.Estadio)
                .Include(j => j.EstatusPartido)
                .Include(j => j.TipoResultado)
                .FirstOrDefaultAsync(j => j.JornadaPartidoId == id.Value);
            if (jornadaPartido == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(jornadaPartido);
        }

        // ADD: /JornadaPartido/Add
        public async Task<IActionResult> Add()
        {
            var pendiente = await _context.EstatusPartido.FirstOrDefaultAsync(e => e.EstatusPartidoNombre == "Pendiente");
            await CargarDropdowns(idActualJornadaPartido: 0, estatusIdSel: pendiente?.EstatusPartidoId);
            return View();
        }

        // ADD: /JornadaPartido/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("JornadaId,PartidoId,EstadioId,EstatusPartidoId")] JornadaPartido jornadaPartido)
        {
            ModelState.Remove("Jornada");
            ModelState.Remove("Partido");
            ModelState.Remove("Estadio");
            ModelState.Remove("EstatusPartido");
            ModelState.Remove("TipoResultado");

            await ValidarPartidoYaProgramado(jornadaPartido.JornadaId, jornadaPartido.PartidoId, idActual: 0);
            await ValidarEquiposNoDuplicadosEnJornada(jornadaPartido.JornadaId, jornadaPartido.PartidoId, idActual: 0);

            var empate = await _context.TipoResultado.FirstOrDefaultAsync(t => t.TipoResultadoNombre == "Empate");
            if (empate == null)
                ModelState.AddModelError(string.Empty, "No se encontró el tipo de resultado 'Empate' en el catálogo; no se puede crear el partido.");

            if (!ModelState.IsValid)
            {
                await CargarDropdowns(0, jornadaPartido.JornadaId, jornadaPartido.PartidoId, jornadaPartido.EstadioId, jornadaPartido.EstatusPartidoId);
                return View(jornadaPartido);
            }

            // Partido aún no jugado: marcador 0-0 y tipo de resultado "Empate" por defecto.
            jornadaPartido.GolLocal = 0;
            jornadaPartido.GolVisita = 0;
            jornadaPartido.TipoResultadoId = empate!.TipoResultadoId;

            _context.Add(jornadaPartido);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /JornadaPartido/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var jornadaPartido = await _context.JornadaPartidos.FindAsync(id.Value);
            if (jornadaPartido == null)
                return NotFound();

            await CargarDropdowns(jornadaPartido.JornadaPartidoId, jornadaPartido.JornadaId, jornadaPartido.PartidoId, jornadaPartido.EstadioId, jornadaPartido.EstatusPartidoId);
            return View(jornadaPartido);
        }

        // POST: /JornadaPartido/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JornadaPartidoId,JornadaId,PartidoId,EstadioId,EstatusPartidoId")] JornadaPartido form)
        {
            if (id != form.JornadaPartidoId)
                return NotFound();

            var jornadaPartido = await _context.JornadaPartidos.FindAsync(id);
            if (jornadaPartido == null)
                return NotFound();

            ModelState.Remove("Jornada");
            ModelState.Remove("Partido");
            ModelState.Remove("Estadio");
            ModelState.Remove("EstatusPartido");
            ModelState.Remove("TipoResultado");

            await ValidarPartidoYaProgramado(form.JornadaId, form.PartidoId, idActual: id);
            await ValidarEquiposNoDuplicadosEnJornada(form.JornadaId, form.PartidoId, idActual: id);

            if (!ModelState.IsValid)
            {
                await CargarDropdowns(id, form.JornadaId, form.PartidoId, form.EstadioId, form.EstatusPartidoId);
                return View(form);
            }

            // Solo se reprograma jornada/partido/estadio/estatus; el marcador y el tipo de
            // resultado los administra el flujo de captura (UpdateScores), no este formulario.
            jornadaPartido.JornadaId = form.JornadaId;
            jornadaPartido.PartidoId = form.PartidoId;
            jornadaPartido.EstadioId = form.EstadioId;
            jornadaPartido.EstatusPartidoId = form.EstatusPartidoId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JornadaPartidoExists(id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /JornadaPartido/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var jornadaPartido = await _context.JornadaPartidos
                .Include(j => j.Jornada).ThenInclude(j => j.EstatusJornada)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoVisita)
                .Include(j => j.Estadio)
                .Include(j => j.EstatusPartido)
                .Include(j => j.TipoResultado)
                .FirstOrDefaultAsync(j => j.JornadaPartidoId == id.Value);
            if (jornadaPartido == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);
            ViewData["MotivoBloqueoEstatus"] = MotivoBloqueoEliminar(jornadaPartido);

            return View(jornadaPartido);
        }

        // POST: /JornadaPartido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jornadaPartido = await _context.JornadaPartidos
                .Include(j => j.Jornada).ThenInclude(j => j.EstatusJornada)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoVisita)
                .Include(j => j.EstatusPartido)
                .FirstOrDefaultAsync(j => j.JornadaPartidoId == id);
            if (jornadaPartido == null)
                return NotFound();

            var motivoBloqueo = MotivoBloqueoEliminar(jornadaPartido);
            if (motivoBloqueo != null)
            {
                ViewData["Dependencias"] = await ObtenerDependencias(id);
                ViewData["MotivoBloqueoEstatus"] = motivoBloqueo;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: " + motivoBloqueo);
                return View(jornadaPartido);
            }

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(jornadaPartido);
            }

            _context.JornadaPartidos.Remove(jornadaPartido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Los marcadores solo se capturan mientras la jornada esté Pendiente, En Curso o
        // Pausada; en cualquier otro estatus (ej. Finalizada) el grid es de solo lectura.
        // Se compara por NOMBRE del catálogo EstatusJornada. MEJORA POSIBLE: esta regla (y
        // "Pendiente" en EstaPendiente, y los tipos de resultado en UpdateScores) depende de
        // nombres de catálogo, frágiles si alguien los renombra; una alternativa más robusta
        // es una columna/clave estable en el catálogo (ej. PermiteCaptura o un código fijo) —
        // aplicable a todos los catálogos que hoy se evalúan por nombre.
        private static readonly string[] EstatusJornadaConCaptura = { "Pendiente", "En Curso", "Pausada" };

        private static bool JornadaPermiteCaptura(Jornada jornada)
        {
            var nombre = jornada.EstatusJornada?.EstatusJornadaNombre?.Trim();
            return EstatusJornadaConCaptura.Any(e => string.Equals(e, nombre, StringComparison.OrdinalIgnoreCase));
        }

        // Solo un partido en estatus "Pendiente" (aún no iniciado) puede eliminarse.
        // Se compara por nombre del catálogo EstatusPartido (los ids no son estables entre
        // bases); si alguien renombra "Pendiente", esta regla hay que actualizarla aquí.
        // Resultado derivado de los goles (nunca capturado por el usuario ni tomado del cliente).
        private static int CalcularTipoResultado(int golLocal, int golVisita, Dictionary<string, int> tipos)
        {
            return golLocal > golVisita ? tipos["Victoria Local"]
                : golLocal < golVisita ? tipos["Victoria Visita"]
                : tipos["Empate"];
        }

        // Motivo por el que NO se puede eliminar un partido de la jornada, o null si se puede.
        // Reglas: la jornada Finalizada bloquea cualquier eliminación, y el partido solo se
        // elimina si está Pendiente (un partido Finalizado, En Curso o Pausado no). Requiere
        // cargar Jornada.EstatusJornada y EstatusPartido. Comparación por nombre de catálogo
        // (ver nota de mejora arriba). La vista Index repite el cálculo para deshabilitar el botón.
        private static string? MotivoBloqueoEliminar(JornadaPartido jp)
        {
            if (string.Equals(jp.Jornada.EstatusJornada.EstatusJornadaNombre?.Trim(), "Finalizada", StringComparison.OrdinalIgnoreCase))
                return "la jornada tiene estatus Finalizada.";
            if (!EstaPendiente(jp))
                return $"el partido tiene estatus {jp.EstatusPartido.EstatusPartidoNombre}; solo se pueden eliminar partidos Pendientes.";
            return null;
        }

        private static bool EstaPendiente(JornadaPartido jp)
        {
            return string.Equals(jp.EstatusPartido?.EstatusPartidoNombre?.Trim(), "Pendiente", StringComparison.OrdinalIgnoreCase);
        }

        private bool JornadaPartidoExists(int id)
        {
            return _context.JornadaPartidos.Any(j => j.JornadaPartidoId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int jornadaPartidoId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPronosticoDetalles"] = await _context.JornadaPronosticoDetalles.CountAsync(d => d.JornadaPartidoId == jornadaPartidoId)
            };
        }

        // Un partido del catálogo (par local/visitante) solo puede quedar programado una vez
        // dentro de la MISMA TEMPORADA: el mismo par sí se repite entre temporadas (ej. Clausura
        // y Apertura 2026), así que la regla ya no es global sobre todo el fixture.
        private async Task ValidarPartidoYaProgramado(int jornadaId, int partidoId, int idActual)
        {
            var temporadaId = await _context.Jornada
                .Where(j => j.JornadaId == jornadaId)
                .Select(j => (int?)j.TemporadaId)
                .FirstOrDefaultAsync();
            if (temporadaId == null)
                return;

            var existe = await _context.JornadaPartidos
                .AnyAsync(jp => jp.JornadaPartidoId != idActual && jp.PartidoId == partidoId && jp.Jornada.TemporadaId == temporadaId);

            if (existe)
                ModelState.AddModelError("PartidoId", "Este partido ya está programado en otra jornada de la misma temporada.");
        }

        // Ningún equipo puede tener dos partidos programados en la misma jornada.
        private async Task ValidarEquiposNoDuplicadosEnJornada(int jornadaId, int partidoId, int idActual)
        {
            var partido = await _context.Partidos.FindAsync(partidoId);
            if (partido == null)
                return;

            var equiposEnJornada = await _context.JornadaPartidos
                .Where(jp => jp.JornadaId == jornadaId && jp.JornadaPartidoId != idActual)
                .Select(jp => new { jp.Partido.EquipoLocalId, jp.Partido.EquipoVisitaId })
                .ToListAsync();

            var conflicto = equiposEnJornada.Exists(e =>
                e.EquipoLocalId == partido.EquipoLocalId || e.EquipoVisitaId == partido.EquipoLocalId ||
                e.EquipoLocalId == partido.EquipoVisitaId || e.EquipoVisitaId == partido.EquipoVisitaId);

            if (conflicto)
                ModelState.AddModelError("PartidoId", "Uno de los equipos ya tiene un partido programado en esta jornada.");
        }

        private async Task CargarDropdowns(int idActualJornadaPartido, int? jornadaIdSel = null, int? partidoIdSel = null, int? estadioIdSel = null, int? estatusIdSel = null)
        {
            var jornadas = await _context.Jornada
                .Include(j => j.Temporada)
                .OrderBy(j => j.TemporadaId).ThenBy(j => j.Orden)
                .Select(j => new { j.JornadaId, Descripcion = j.Temporada.TemporadaNombre + " - " + j.JornadaNombre })
                .ToListAsync();
            ViewData["JornadaId"] = new SelectList(jornadas, "JornadaId", "Descripcion", jornadaIdSel);

            // Se excluyen los partidos ya programados en la temporada de la jornada seleccionada
            // (en Edit). En Add todavía no hay jornada elegida, así que se listan todos y la
            // validación del POST (ValidarPartidoYaProgramado) rechaza los repetidos.
            var usados = new List<int>();
            if (jornadaIdSel.HasValue)
            {
                var temporadaSel = await _context.Jornada
                    .Where(j => j.JornadaId == jornadaIdSel.Value)
                    .Select(j => (int?)j.TemporadaId)
                    .FirstOrDefaultAsync();
                if (temporadaSel != null)
                    usados = await _context.JornadaPartidos
                        .Where(jp => jp.JornadaPartidoId != idActualJornadaPartido && jp.Jornada.TemporadaId == temporadaSel)
                        .Select(jp => jp.PartidoId)
                        .ToListAsync();
            }

            var partidos = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisita)
                .Where(p => !usados.Contains(p.PartidoId))
                .OrderBy(p => p.EquipoLocal.EquipoNombre)
                .Select(p => new { p.PartidoId, Descripcion = p.EquipoLocal.EquipoNombre + " vs " + p.EquipoVisita.EquipoNombre })
                .ToListAsync();
            ViewData["PartidoId"] = new SelectList(partidos, "PartidoId", "Descripcion", partidoIdSel);

            ViewData["EstadioId"] = new SelectList(_context.Estadios, "EstadioId", "EstadioNombre", estadioIdSel);
            ViewData["EstatusPartidoId"] = new SelectList(_context.EstatusPartido, "EstatusPartidoId", "EstatusPartidoNombre", estatusIdSel);
        }
    }

    public class CambiarEstatusRequest
    {
        public int JornadaPartidoId { get; set; }
        public string? Accion { get; set; }
        public int? GolLocal { get; set; }
        public int? GolVisita { get; set; }
    }
}
