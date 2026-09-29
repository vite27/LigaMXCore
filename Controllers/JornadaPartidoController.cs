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
        public async Task<IActionResult> Index()
        {
            var list = await _context.JornadaPartidos
                .Include(j => j.Jornada)
                .Include(j => j.Partido)
                    .ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido)
                    .ThenInclude(p => p.EquipoVisita)
                .Include(j => j.Estadio)
                .Include(j => j.EstatusPartido)
                .Include(j => j.TipoResultado)
                .ToListAsync();
            return View(list);
        }

        // POST: /JornadaPartido/UpdateScores
        [HttpPost]
        public JsonResult UpdateScores([FromBody] List<JornadaPartido> item)
        {
            try
            {
                foreach (var partidoEnviado in item)
                {
                    var partidoDb = _context.JornadaPartidos.Find(partidoEnviado.JornadaPartidoId);

                    if (partidoDb != null)
                    {
                        partidoDb.GolLocal = partidoEnviado.GolLocal;
                        partidoDb.GolVisita = partidoEnviado.GolVisita;
                        partidoDb.EstatusPartidoId = partidoEnviado.EstatusPartidoId;

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

            await ValidarPartidoYaProgramado(jornadaPartido.PartidoId, idActual: 0);
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

            await ValidarPartidoYaProgramado(form.PartidoId, idActual: id);
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
                .Include(j => j.Jornada)
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

        // POST: /JornadaPartido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jornadaPartido = await _context.JornadaPartidos
                .Include(j => j.Jornada)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido).ThenInclude(p => p.EquipoVisita)
                .FirstOrDefaultAsync(j => j.JornadaPartidoId == id);
            if (jornadaPartido == null)
                return NotFound();

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

        // Un partido del catálogo solo puede quedar programado una vez en todo el fixture.
        private async Task ValidarPartidoYaProgramado(int partidoId, int idActual)
        {
            var existe = await _context.JornadaPartidos
                .AnyAsync(jp => jp.JornadaPartidoId != idActual && jp.PartidoId == partidoId);

            if (existe)
                ModelState.AddModelError("PartidoId", "Este partido ya está programado en otra jornada.");
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

            var usados = await _context.JornadaPartidos
                .Where(jp => jp.JornadaPartidoId != idActualJornadaPartido)
                .Select(jp => jp.PartidoId)
                .ToListAsync();

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
}
