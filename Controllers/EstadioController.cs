using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class EstadioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Estadio
        public async Task<IActionResult> Index()
        {
            var list = await _context.Estadios.Include(e => e.Municipio).ToListAsync();
            return View(list);
        }

        // GET: /Estadio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var estadio = await _context.Estadios.Include(e => e.Municipio).FirstOrDefaultAsync(e => e.EstadioId == id.Value);
            if (estadio == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estadio);
        }

        // ADD: /Estadio/Add
        public IActionResult Add()
        {
            ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre");
            return View();
        }

        // ADD: /Estadio/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstadioNombre,Alias,Direccion,CodigoPostal,MunicipioId")] Estadio estadio)
        {
            ModelState.Remove("Municipio");

            if (!string.IsNullOrWhiteSpace(estadio.EstadioNombre))
                estadio.EstadioNombre = estadio.EstadioNombre.Trim();
            if (!string.IsNullOrWhiteSpace(estadio.Alias))
                estadio.Alias = estadio.Alias.Trim();

            await ValidarNombreDuplicado(estadio.EstadioNombre, idActual: 0);

            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", estadio.MunicipioId);
                return View(estadio);
            }

            _context.Add(estadio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Estadio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estadio = await _context.Estadios.FindAsync(id.Value);
            if (estadio == null)
                return NotFound();

            ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", estadio.MunicipioId);
            return View(estadio);
        }

        // POST: /Estadio/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstadioId,EstadioNombre,Alias,Direccion,CodigoPostal,MunicipioId")] Estadio estadio)
        {
            if (id != estadio.EstadioId)
                return NotFound();

            ModelState.Remove("Municipio");

            if (!string.IsNullOrWhiteSpace(estadio.EstadioNombre))
                estadio.EstadioNombre = estadio.EstadioNombre.Trim();
            if (!string.IsNullOrWhiteSpace(estadio.Alias))
                estadio.Alias = estadio.Alias.Trim();

            await ValidarNombreDuplicado(estadio.EstadioNombre, idActual: id);

            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", estadio.MunicipioId);
                return View(estadio);
            }

            try
            {
                _context.Update(estadio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstadioExists(estadio.EstadioId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Estadio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var estadio = await _context.Estadios.Include(e => e.Municipio).FirstOrDefaultAsync(e => e.EstadioId == id.Value);
            if (estadio == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estadio);
        }

        // POST: /Estadio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadio = await _context.Estadios.Include(e => e.Municipio).FirstOrDefaultAsync(e => e.EstadioId == id);
            if (estadio == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(estadio);
            }

            _context.Estadios.Remove(estadio);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EstadioExists(int id)
        {
            return _context.Estadios.Any(e => e.EstadioId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int estadioId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPartidos"] = await _context.JornadaPartidos.CountAsync(jp => jp.EstadioId == estadioId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var estadios = await _context.Estadios
                .Where(e => e.EstadioId != idActual)
                .ToListAsync();

            var existe = estadios.Exists(e =>
                string.Equals(e.EstadioNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("EstadioNombre", "Ya existe un estadio con ese nombre.");
        }
    }
}
