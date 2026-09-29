using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class TemporadaController : Controller
    {
         private readonly ApplicationDbContext _context;

        public TemporadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Temporada
        public async Task<IActionResult> Index()
        {
            var list = await _context.Temporada.ToListAsync();
            return View(list);
        }

        // GET: /Temporada/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var temporada = await _context.Temporada.FindAsync(id.Value);
            if (temporada == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(temporada);
        }

        // ADD: /Temporada/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /Temporada/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("TemporadaNombre,Comentarios")] Temporada temporada)
        {
            if (!string.IsNullOrWhiteSpace(temporada.TemporadaNombre))
                temporada.TemporadaNombre = temporada.TemporadaNombre.Trim();

            await ValidarNombreDuplicado(temporada.TemporadaNombre, idActual: 0);

            if (!ModelState.IsValid)
                return View(temporada);

            _context.Add(temporada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Temporada/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var temporada = await _context.Temporada.FindAsync(id.Value);
            if (temporada == null)
                return NotFound();

            return View(temporada);
        }

         // POST: /Temporada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TemporadaId,TemporadaNombre,Comentarios")] Temporada temporada)
        {
            if (id != temporada.TemporadaId)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(temporada.TemporadaNombre))
                temporada.TemporadaNombre = temporada.TemporadaNombre.Trim();

            await ValidarNombreDuplicado(temporada.TemporadaNombre, idActual: id);

            if (!ModelState.IsValid)
                return View(temporada);

            try
            {
                _context.Update(temporada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemporadaExists(temporada.TemporadaId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Temporada/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var temporada = await _context.Temporada.FindAsync(id.Value);
            if (temporada == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(temporada);
        }

        // POST: /Temporada/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var temporada = await _context.Temporada.FindAsync(id);
            if (temporada == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(temporada);
            }

            _context.Temporada.Remove(temporada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TemporadaExists(int id)
        {
            return _context.Temporada.Any(e => e.TemporadaId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int temporadaId)
        {
            return new Dictionary<string, int>
            {
                ["Jornadas"] = await _context.Jornada.CountAsync(j => j.TemporadaId == temporadaId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var temporadas = await _context.Temporada
                .Where(t => t.TemporadaId != idActual)
                .ToListAsync();

            var existe = temporadas.Exists(t =>
                string.Equals(t.TemporadaNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("TemporadaNombre", "Ya existe una temporada con ese nombre.");
        }
    }
}
