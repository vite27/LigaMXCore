using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class PartidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Partido
        public async Task<IActionResult> Index(int? equipoLocalId, int? equipoVisitaId)
        {
            var query = _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisita)
                .AsQueryable();

            if (equipoLocalId.HasValue && equipoVisitaId.HasValue && equipoLocalId == equipoVisitaId)
            {
                ViewData["ErrorFiltro"] = "El equipo local y el equipo visita no pueden ser el mismo. No se aplicó el filtro.";
            }
            else
            {
                if (equipoLocalId.HasValue)
                    query = query.Where(p => p.EquipoLocalId == equipoLocalId.Value);

                if (equipoVisitaId.HasValue)
                    query = query.Where(p => p.EquipoVisitaId == equipoVisitaId.Value);
            }

            ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", equipoLocalId);
            ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", equipoVisitaId);

            var list = await query.ToListAsync();
            return View(list);
        }

        // GET: /Partido/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisita)
                .FirstOrDefaultAsync(p => p.PartidoId == id.Value);
            if (partido == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(partido);
        }

        // ADD: /Partido/Add
        public IActionResult Add()
        {
            ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre");
            ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre");
            return View();
        }

        // ADD: /Partido/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EquipoLocalId,EquipoVisitaId")] Partido partido)
        {
            ModelState.Remove("EquipoLocal");
            ModelState.Remove("EquipoVisita");

            ValidarEquiposDistintos(partido.EquipoLocalId, partido.EquipoVisitaId);
            await ValidarParidoDuplicado(partido.EquipoLocalId, partido.EquipoVisitaId, idActual: 0);

            if (!ModelState.IsValid)
            {
                ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoLocalId);
                ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoVisitaId);
                return View(partido);
            }

            _context.Add(partido);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Partido/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var partido = await _context.Partidos.FindAsync(id.Value);
            if (partido == null)
                return NotFound();

            ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoLocalId);
            ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoVisitaId);
            return View(partido);
        }

        // POST: /Partido/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PartidoId,EquipoLocalId,EquipoVisitaId")] Partido partido)
        {
            if (id != partido.PartidoId)
                return NotFound();

            ModelState.Remove("EquipoLocal");
            ModelState.Remove("EquipoVisita");

            ValidarEquiposDistintos(partido.EquipoLocalId, partido.EquipoVisitaId);
            await ValidarParidoDuplicado(partido.EquipoLocalId, partido.EquipoVisitaId, idActual: id);

            if (!ModelState.IsValid)
            {
                ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoLocalId);
                ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoVisitaId);
                return View(partido);
            }

            try
            {
                _context.Update(partido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartidoExists(partido.PartidoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Partido/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisita)
                .FirstOrDefaultAsync(p => p.PartidoId == id.Value);
            if (partido == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(partido);
        }

        // POST: /Partido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisita)
                .FirstOrDefaultAsync(p => p.PartidoId == id);
            if (partido == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(partido);
            }

            _context.Partidos.Remove(partido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PartidoExists(int id)
        {
            return _context.Partidos.Any(p => p.PartidoId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int partidoId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPartidos"] = await _context.JornadaPartidos.CountAsync(jp => jp.PartidoId == partidoId)
            };
        }

        private void ValidarEquiposDistintos(int equipoLocalId, int equipoVisitaId)
        {
            if (equipoLocalId != 0 && equipoLocalId == equipoVisitaId)
                ModelState.AddModelError("EquipoVisitaId", "El equipo visitante debe ser distinto del equipo local.");
        }

        private async Task ValidarParidoDuplicado(int equipoLocalId, int equipoVisitaId, int idActual)
        {
            if (equipoLocalId == 0 || equipoVisitaId == 0)
                return;

            var existe = await _context.Partidos.AnyAsync(p =>
                p.PartidoId != idActual &&
                p.EquipoLocalId == equipoLocalId &&
                p.EquipoVisitaId == equipoVisitaId);

            if (existe)
                ModelState.AddModelError(string.Empty, "Ya existe un partido con ese equipo local y visitante.");
        }
    }
}
