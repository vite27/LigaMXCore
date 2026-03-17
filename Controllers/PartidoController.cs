using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class PartidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADD: /Partido/Add
        public IActionResult Add()
        {
            ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre");
            ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre");
            return View();
        }

        // ADD: /Partido/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Fecha,EquipoLocalId,EquipoVisitaId")] Partido partido)
        {
            ModelState.Remove("EquipoLocal");
            ModelState.Remove("EquipoVisita");
            if (!ModelState.IsValid)
            {
                ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoLocalId);
                ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoVisitaId);
                return View(partido);
            }

            _context.Add(partido);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Partido/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var partido = await _context.Partidos.FindAsync(id.Value);
            if (partido == null)
                return NotFound();

            ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoLocalId);
            ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoVisitaId);
            return View(partido);
        }

        // POST: /Partido/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PartidoId,Fecha,EquipoLocalId,EquipoVisitaId")] Partido partido)
        {
            if (id != partido.PartidoId)
                return NotFound();

            ModelState.Remove("EquipoLocal");
            ModelState.Remove("EquipoVisita");
            if (!ModelState.IsValid)
            {
                ViewData["EquipoLocalId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoLocalId);
                ViewData["EquipoVisitaId"] = new SelectList(_context.Equipos, "EquipoId", "EquipoNombre", partido.EquipoVisitaId);
                return View(partido);
            }

            try
            {
                _context.Update(partido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartidoExists(partido.PartidoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PartidoExists(int id)
        {
            return _context.Partidos.Any(p => p.PartidoId == id);
        }        

        // GET: /Partido
        public async Task<IActionResult> Index()
        {
            var list = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisita)
                .ToListAsync();
            return View(list);
        }
    }
}