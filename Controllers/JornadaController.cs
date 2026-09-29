using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class JornadaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JornadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Jornada
        public async Task<IActionResult> Index()
        {
            var list = await _context.Jornada
                .Include(j => j.Temporada)
                .Include(j => j.EstatusJornada)
                .ToListAsync();
            return View(list);
        }

        // GET: /Jornada/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var jornada = await _context.Jornada
                .Include(j => j.Temporada)
                .Include(j => j.EstatusJornada)
                .FirstOrDefaultAsync(j => j.JornadaId == id.Value);
            if (jornada == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(jornada);
        }

        // ADD: /Jornada/Add
        public IActionResult Add()
        {
            ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre");
            ViewData["EstatusJornadaId"] = new SelectList(_context.EstatusJornada, "EstatusJornadaId", "EstatusJornadaNombre");
            return View();
        }

        // ADD: /Jornada/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Orden,JornadaNombre,TemporadaId,EstatusJornadaId")] Jornada jornada)
        {
            ModelState.Remove("Temporada");
            ModelState.Remove("EstatusJornada");

            if (!string.IsNullOrWhiteSpace(jornada.JornadaNombre))
                jornada.JornadaNombre = jornada.JornadaNombre.Trim();

            await ValidarOrdenDuplicado(jornada.TemporadaId, jornada.Orden, idActual: 0);

            if (!ModelState.IsValid)
            {
                ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre", jornada.TemporadaId);
                ViewData["EstatusJornadaId"] = new SelectList(_context.EstatusJornada, "EstatusJornadaId", "EstatusJornadaNombre", jornada.EstatusJornadaId);
                return View(jornada);
            }

            _context.Add(jornada);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Jornada/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var jornada = await _context.Jornada.FindAsync(id.Value);
            if (jornada == null)
                return NotFound();

            ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre", jornada.TemporadaId);
            ViewData["EstatusJornadaId"] = new SelectList(_context.EstatusJornada, "EstatusJornadaId", "EstatusJornadaNombre", jornada.EstatusJornadaId);
            return View(jornada);
        }

        // POST: /Jornada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JornadaId,Orden,JornadaNombre,TemporadaId,EstatusJornadaId")] Jornada jornada)
        {
            if (id != jornada.JornadaId)
                return NotFound();

            ModelState.Remove("Temporada");
            ModelState.Remove("EstatusJornada");

            if (!string.IsNullOrWhiteSpace(jornada.JornadaNombre))
                jornada.JornadaNombre = jornada.JornadaNombre.Trim();

            await ValidarOrdenDuplicado(jornada.TemporadaId, jornada.Orden, idActual: id);

            if (!ModelState.IsValid)
            {
                ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre", jornada.TemporadaId);
                ViewData["EstatusJornadaId"] = new SelectList(_context.EstatusJornada, "EstatusJornadaId", "EstatusJornadaNombre", jornada.EstatusJornadaId);
                return View(jornada);
            }

            try
            {
                _context.Update(jornada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JornadaExists(jornada.JornadaId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Jornada/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var jornada = await _context.Jornada
                .Include(j => j.Temporada)
                .Include(j => j.EstatusJornada)
                .FirstOrDefaultAsync(j => j.JornadaId == id.Value);
            if (jornada == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(jornada);
        }

        // POST: /Jornada/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jornada = await _context.Jornada
                .Include(j => j.Temporada)
                .Include(j => j.EstatusJornada)
                .FirstOrDefaultAsync(j => j.JornadaId == id);
            if (jornada == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(jornada);
            }

            _context.Jornada.Remove(jornada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool JornadaExists(int id)
        {
            return _context.Jornada.Any(j => j.JornadaId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int jornadaId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPartidos"] = await _context.JornadaPartidos.CountAsync(jp => jp.JornadaId == jornadaId),
                ["JornadaPronosticos"] = await _context.JornadaPronosticos.CountAsync(jp => jp.JornadaId == jornadaId)
            };
        }

        private async Task ValidarOrdenDuplicado(int temporadaId, int orden, int idActual)
        {
            var existe = await _context.Jornada.AnyAsync(j =>
                j.JornadaId != idActual &&
                j.TemporadaId == temporadaId &&
                j.Orden == orden);

            if (existe)
                ModelState.AddModelError("Orden", "Ya existe una jornada con ese orden en esta temporada.");
        }
    }
}
