using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class JornadaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JornadaController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ADD: /Jornada/Add
        public IActionResult Add()
        {
            ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre");
            return View();
        }

        // ADD: /Jornada/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Orden,JornadaNombre,TemporadaId")] Jornada jornada)
        {
            ModelState.Remove("Temporada");
            if (!ModelState.IsValid)
            {
                ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre");
                return View(jornada);
            }

            _context.Add(jornada);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Jornada/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var jornada = await _context.Jornada.FindAsync(id.Value);
            if (jornada == null)
                return NotFound();

            ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre");
            return View(jornada);
        }

        // POST: /Jornada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JornadaId,Orden,JornadaNombre,TemporadaId")] Jornada jornada)
        {
            if (id != jornada.JornadaId)
                return NotFound();

            ModelState.Remove("Temporada");
            if (!ModelState.IsValid)
            {
                ViewData["TemporadaId"] = new SelectList(_context.Temporada, "TemporadaId", "TemporadaNombre");
                return View(jornada);
            }

            try
            {
                _context.Update(jornada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JornadaExists(jornada.JornadaId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool JornadaExists(int id)
        {
            return _context.Jornada.Any(j => j.JornadaId == id);
        }

        // GET: /Jornada
        public async Task<IActionResult> Index()
        {
            var list = await _context.Jornada
                .Include(j => j.Temporada)
                .ToListAsync();
            return View(list);
        }                                                
    }

}