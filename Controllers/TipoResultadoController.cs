using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class TipoResultadoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoResultadoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TipoResultado
        public async Task<IActionResult> Index()
        {
            var list = await _context.TipoResultado.ToListAsync();
            return View(list);
        }

        // GET: /TipoResultado/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var tipoResultado = await _context.TipoResultado.FindAsync(id.Value);
            if (tipoResultado == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(tipoResultado);
        }

        // ADD: /TipoResultado/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /TipoResultado/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("TipoResultadoNombre")] TipoResultado tipoResultado)
        {
            if (!string.IsNullOrWhiteSpace(tipoResultado.TipoResultadoNombre))
                tipoResultado.TipoResultadoNombre = tipoResultado.TipoResultadoNombre.Trim();

            await ValidarNombreDuplicado(tipoResultado.TipoResultadoNombre, idActual: 0);

            if (!ModelState.IsValid)
                return View(tipoResultado);

            _context.Add(tipoResultado);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /TipoResultado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var tipoResultado = await _context.TipoResultado.FindAsync(id.Value);
            if (tipoResultado == null)
                return NotFound();

            return View(tipoResultado);
        }

        // POST: /TipoResultado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TipoResultadoId,TipoResultadoNombre")] TipoResultado tipoResultado)
        {
            if (id != tipoResultado.TipoResultadoId)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(tipoResultado.TipoResultadoNombre))
                tipoResultado.TipoResultadoNombre = tipoResultado.TipoResultadoNombre.Trim();

            await ValidarNombreDuplicado(tipoResultado.TipoResultadoNombre, idActual: id);

            if (!ModelState.IsValid)
                return View(tipoResultado);

            try
            {
                _context.Update(tipoResultado);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoResultadoExists(tipoResultado.TipoResultadoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /TipoResultado/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var tipoResultado = await _context.TipoResultado.FindAsync(id.Value);
            if (tipoResultado == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(tipoResultado);
        }

        // POST: /TipoResultado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoResultado = await _context.TipoResultado.FindAsync(id);
            if (tipoResultado == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(tipoResultado);
            }

            _context.TipoResultado.Remove(tipoResultado);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TipoResultadoExists(int id)
        {
            return _context.TipoResultado.Any(e => e.TipoResultadoId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int tipoResultadoId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPartidos"] = await _context.JornadaPartidos.CountAsync(jp => jp.TipoResultadoId == tipoResultadoId),
                ["JornadaPronosticoDetalles"] = await _context.JornadaPronosticoDetalles.CountAsync(jpd => jpd.TipoResultadoId == tipoResultadoId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var tiposResultado = await _context.TipoResultado
                .Where(t => t.TipoResultadoId != idActual)
                .ToListAsync();

            var existe = tiposResultado.Exists(t =>
                string.Equals(t.TipoResultadoNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("TipoResultadoNombre", "Ya existe un tipo de resultado con ese nombre.");
        }
    }
}
