using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class MunicipioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MunicipioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Municipio
        public async Task<IActionResult> Index()
        {
            var list = await _context.Municipios.Include(m => m.Estado).ToListAsync();
            return View(list);
        }

        // GET: /Municipio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var municipio = await _context.Municipios.Include(m => m.Estado).FirstOrDefaultAsync(m => m.MunicipioId == id.Value);
            if (municipio == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(municipio);
        }

        // GET: /Municipio/Add
        public IActionResult Add()
        {
            ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "EstadoNombre");
            return View();
        }

        // POST: /Municipio/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("MunicipioNombre,EstadoId")] Municipio municipio)
        {
            ModelState.Remove("Estado");

            if (!string.IsNullOrWhiteSpace(municipio.MunicipioNombre))
                municipio.MunicipioNombre = municipio.MunicipioNombre.Trim();

            await ValidarNombreDuplicado(municipio.MunicipioNombre, municipio.EstadoId, idActual: 0);

            if (!ModelState.IsValid)
            {
                ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "EstadoNombre", municipio.EstadoId);
                return View(municipio);
            }

            _context.Add(municipio);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Municipio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var municipio = await _context.Municipios.FindAsync(id.Value);
            if (municipio == null)
                return NotFound();

            ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "EstadoNombre", municipio.EstadoId);
            return View(municipio);
        }

        // POST: /Municipio/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MunicipioId,MunicipioNombre,EstadoId")] Municipio municipio)
        {
            if (id != municipio.MunicipioId)
                return NotFound();

            ModelState.Remove("Estado");

            if (!string.IsNullOrWhiteSpace(municipio.MunicipioNombre))
                municipio.MunicipioNombre = municipio.MunicipioNombre.Trim();

            await ValidarNombreDuplicado(municipio.MunicipioNombre, municipio.EstadoId, idActual: id);

            if (!ModelState.IsValid)
            {
                ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "EstadoNombre", municipio.EstadoId);
                return View(municipio);
            }

            try
            {
                _context.Update(municipio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MunicipioExists(municipio.MunicipioId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Municipio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var municipio = await _context.Municipios.Include(m => m.Estado).FirstOrDefaultAsync(m => m.MunicipioId == id.Value);
            if (municipio == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(municipio);
        }

        // POST: /Municipio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var municipio = await _context.Municipios.FindAsync(id);
            if (municipio == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(municipio);
            }

            _context.Municipios.Remove(municipio);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool MunicipioExists(int id)
        {
            return _context.Municipios.Any(e => e.MunicipioId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int municipioId)
        {
            return new Dictionary<string, int>
            {
                ["Equipos"] = await _context.Equipos.CountAsync(e => e.MunicipioId == municipioId),
                ["Estadios"] = await _context.Estadios.CountAsync(e => e.MunicipioId == municipioId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int estadoId, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var municipios = await _context.Municipios
                .Where(m => m.EstadoId == estadoId && m.MunicipioId != idActual)
                .ToListAsync();

            var existe = municipios.Exists(m =>
                string.Equals(m.MunicipioNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("MunicipioNombre", "Ya existe un municipio con ese nombre en el estado seleccionado.");
        }
    }
}
