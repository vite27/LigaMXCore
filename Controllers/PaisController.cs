using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class PaisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Pais
        public async Task<IActionResult> Index()
        {
            var list = await _context.Pais.ToListAsync();
            return View(list);
        }

        // GET: /Pais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pais = await _context.Pais.FindAsync(id.Value);
            if (pais == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(pais);
        }

        // ADD: /Pais/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /Pais/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("PaisNombre")] Pais pais)
        {
            if (!string.IsNullOrWhiteSpace(pais.PaisNombre))
                pais.PaisNombre = pais.PaisNombre.Trim();

            await ValidarNombreDuplicado(pais.PaisNombre, idActual: 0);

            if (!ModelState.IsValid)
                return View(pais);

            _context.Add(pais);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Pais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pais = await _context.Pais.FindAsync(id.Value);
            if (pais == null)
                return NotFound();

            return View(pais);
        }

        // POST: /Pais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaisId,PaisNombre")] Pais pais)
        {
            if (id != pais.PaisId)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(pais.PaisNombre))
                pais.PaisNombre = pais.PaisNombre.Trim();

            await ValidarNombreDuplicado(pais.PaisNombre, idActual: id);

            if (!ModelState.IsValid)
                return View(pais);

            try
            {
                _context.Update(pais);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaisExists(pais.PaisId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Pais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var pais = await _context.Pais.FindAsync(id.Value);
            if (pais == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(pais);
        }

        // POST: /Pais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pais = await _context.Pais.FindAsync(id);
            if (pais == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(pais);
            }

            _context.Pais.Remove(pais);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PaisExists(int id)
        {
            return _context.Pais.Any(e => e.PaisId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int paisId)
        {
            return new Dictionary<string, int>
            {
                ["Estados"] = await _context.Estados.CountAsync(e => e.PaisId == paisId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var paises = await _context.Pais
                .Where(p => p.PaisId != idActual)
                .ToListAsync();

            var existe = paises.Exists(p =>
                string.Equals(p.PaisNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("PaisNombre", "Ya existe un país con ese nombre.");
        }
    }
}
