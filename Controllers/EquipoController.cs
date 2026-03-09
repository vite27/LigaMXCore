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
            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre");
                return View(equipo);
            }

            _context.Add(equipo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Equipo
        public async Task<IActionResult> Index()
        {
            var list = await _context.Equipos.Include(e => e.Municipio).ToListAsync();
            return View(list);
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

        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.EquipoId == id);
        }
    }
}