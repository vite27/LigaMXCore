using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class EstatusPartidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstatusPartidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /EstatusPartido
        public async Task<IActionResult> Index()
        {
            var list = await _context.EstatusPartido.ToListAsync();
            return View(list);
        }

        // GET: /EstatusPartido/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusPartido = await _context.EstatusPartido.FindAsync(id.Value);
            if (estatusPartido == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estatusPartido);
        }

        // ADD: /EstatusPartido/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /EstatusPartido/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstatusPartidoNombre")] EstatusPartido estatusPartido)
        {
            if (!string.IsNullOrWhiteSpace(estatusPartido.EstatusPartidoNombre))
                estatusPartido.EstatusPartidoNombre = estatusPartido.EstatusPartidoNombre.Trim();

            await ValidarNombreDuplicado(estatusPartido.EstatusPartidoNombre, idActual: 0);

            if (!ModelState.IsValid)
                return View(estatusPartido);

            _context.Add(estatusPartido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /EstatusPartido/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusPartido = await _context.EstatusPartido.FindAsync(id.Value);
            if (estatusPartido == null)
                return NotFound();

            return View(estatusPartido);
        }

        // POST: /EstatusPartido/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstatusPartidoId,EstatusPartidoNombre")] EstatusPartido estatusPartido)
        {
            if (id != estatusPartido.EstatusPartidoId)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(estatusPartido.EstatusPartidoNombre))
                estatusPartido.EstatusPartidoNombre = estatusPartido.EstatusPartidoNombre.Trim();

            await ValidarNombreDuplicado(estatusPartido.EstatusPartidoNombre, idActual: id);

            if (!ModelState.IsValid)
                return View(estatusPartido);

            try
            {
                _context.Update(estatusPartido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstatusPartidoExists(estatusPartido.EstatusPartidoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /EstatusPartido/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusPartido = await _context.EstatusPartido.FindAsync(id.Value);
            if (estatusPartido == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estatusPartido);
        }

        // POST: /EstatusPartido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estatusPartido = await _context.EstatusPartido.FindAsync(id);
            if (estatusPartido == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(estatusPartido);
            }

            _context.EstatusPartido.Remove(estatusPartido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EstatusPartidoExists(int id)
        {
            return _context.EstatusPartido.Any(e => e.EstatusPartidoId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int estatusPartidoId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPartidos"] = await _context.JornadaPartidos.CountAsync(jp => jp.EstatusPartidoId == estatusPartidoId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var estatus = await _context.EstatusPartido
                .Where(e => e.EstatusPartidoId != idActual)
                .ToListAsync();

            var existe = estatus.Exists(e =>
                string.Equals(e.EstatusPartidoNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("EstatusPartidoNombre", "Ya existe un estatus de partido con ese nombre.");
        }
    }
}
