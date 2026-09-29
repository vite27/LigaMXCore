using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class EstatusJornadaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstatusJornadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /EstatusJornada
        public async Task<IActionResult> Index()
        {
            var list = await _context.EstatusJornada.ToListAsync();
            return View(list);
        }

        // GET: /EstatusJornada/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusJornada = await _context.EstatusJornada.FindAsync(id.Value);
            if (estatusJornada == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estatusJornada);
        }

        // ADD: /EstatusJornada/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /EstatusJornada/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstatusJornadaNombre")] EstatusJornada estatusJornada)
        {
            if (!string.IsNullOrWhiteSpace(estatusJornada.EstatusJornadaNombre))
                estatusJornada.EstatusJornadaNombre = estatusJornada.EstatusJornadaNombre.Trim();

            await ValidarNombreDuplicado(estatusJornada.EstatusJornadaNombre, idActual: 0);

            if (!ModelState.IsValid)
                return View(estatusJornada);

            _context.Add(estatusJornada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /EstatusJornada/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusJornada = await _context.EstatusJornada.FindAsync(id.Value);
            if (estatusJornada == null)
                return NotFound();

            return View(estatusJornada);
        }

        // POST: /EstatusJornada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstatusJornadaId,EstatusJornadaNombre")] EstatusJornada estatusJornada)
        {
            if (id != estatusJornada.EstatusJornadaId)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(estatusJornada.EstatusJornadaNombre))
                estatusJornada.EstatusJornadaNombre = estatusJornada.EstatusJornadaNombre.Trim();

            await ValidarNombreDuplicado(estatusJornada.EstatusJornadaNombre, idActual: id);

            if (!ModelState.IsValid)
                return View(estatusJornada);

            try
            {
                _context.Update(estatusJornada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstatusJornadaExists(estatusJornada.EstatusJornadaId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /EstatusJornada/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusJornada = await _context.EstatusJornada.FindAsync(id.Value);
            if (estatusJornada == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estatusJornada);
        }

        // POST: /EstatusJornada/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estatusJornada = await _context.EstatusJornada.FindAsync(id);
            if (estatusJornada == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(estatusJornada);
            }

            _context.EstatusJornada.Remove(estatusJornada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EstatusJornadaExists(int id)
        {
            return _context.EstatusJornada.Any(e => e.EstatusJornadaId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int estatusJornadaId)
        {
            return new Dictionary<string, int>
            {
                ["Jornadas"] = await _context.Jornada.CountAsync(j => j.EstatusJornadaId == estatusJornadaId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var estatus = await _context.EstatusJornada
                .Where(e => e.EstatusJornadaId != idActual)
                .ToListAsync();

            var existe = estatus.Exists(e =>
                string.Equals(e.EstatusJornadaNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("EstatusJornadaNombre", "Ya existe un estatus de jornada con ese nombre.");
        }
    }
}
