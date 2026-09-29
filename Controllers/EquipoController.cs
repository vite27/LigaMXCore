using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class EquipoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Equipo
        public async Task<IActionResult> Index()
        {
            var list = await _context.Equipos.Include(e => e.Municipio).ToListAsync();
            return View(list);
        }

        // GET: /Equipo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var equipo = await _context.Equipos.Include(e => e.Municipio).FirstOrDefaultAsync(e => e.EquipoId == id.Value);
            if (equipo == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(equipo);
        }

        // ADD: /Equipo/Add
        public IActionResult Add()
        {
            ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre");
            return View();
        }

        // ADD: /Equipo/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EquipoNombre,Alias,MunicipioId,EquipoLogo")] Equipo equipo)
        {
            ModelState.Remove("Municipio");

            if (!string.IsNullOrWhiteSpace(equipo.EquipoNombre))
                equipo.EquipoNombre = equipo.EquipoNombre.Trim();
            if (!string.IsNullOrWhiteSpace(equipo.Alias))
                equipo.Alias = equipo.Alias.Trim();

            await ValidarNombreDuplicado(equipo.EquipoNombre, idActual: 0);

            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", equipo.MunicipioId);
                return View(equipo);
            }

            _context.Add(equipo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Equipo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var equipo = await _context.Equipos.FindAsync(id.Value);
            if (equipo == null)
                return NotFound();

            ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", equipo.MunicipioId);
            return View(equipo);
        }

        // POST: /Equipo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipoId,EquipoNombre,Alias,MunicipioId,EquipoLogo")] Equipo equipo)
        {
            if (id != equipo.EquipoId)
                return NotFound();

            ModelState.Remove("Municipio");

            if (!string.IsNullOrWhiteSpace(equipo.EquipoNombre))
                equipo.EquipoNombre = equipo.EquipoNombre.Trim();
            if (!string.IsNullOrWhiteSpace(equipo.Alias))
                equipo.Alias = equipo.Alias.Trim();

            await ValidarNombreDuplicado(equipo.EquipoNombre, idActual: id);

            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", equipo.MunicipioId);
                return View(equipo);
            }

            try
            {
                _context.Update(equipo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipoExists(equipo.EquipoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Equipo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var equipo = await _context.Equipos.Include(e => e.Municipio).FirstOrDefaultAsync(e => e.EquipoId == id.Value);
            if (equipo == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(equipo);
        }

        // POST: /Equipo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipo = await _context.Equipos.Include(e => e.Municipio).FirstOrDefaultAsync(e => e.EquipoId == id);
            if (equipo == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(equipo);
            }

            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.EquipoId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int equipoId)
        {
            return new Dictionary<string, int>
            {
                ["Partidos (como local)"] = await _context.Partidos.CountAsync(p => p.EquipoLocalId == equipoId),
                ["Partidos (como visita)"] = await _context.Partidos.CountAsync(p => p.EquipoVisitaId == equipoId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var equipos = await _context.Equipos
                .Where(e => e.EquipoId != idActual)
                .ToListAsync();

            var existe = equipos.Exists(e =>
                string.Equals(e.EquipoNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("EquipoNombre", "Ya existe un equipo con ese nombre.");
        }
    }
}
