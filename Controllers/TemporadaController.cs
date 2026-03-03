using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class TemporadaController : Controller
    {
         private readonly ApplicationDbContext _context;

        public TemporadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Temporada
        public async Task<IActionResult> Index()
        {
            var list = await _context.Temporada.ToListAsync();
            return View(list);
        }

        // GET: /Temporada/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var temporada = await _context.Temporada.FindAsync(id.Value);
            if (temporada == null)
                return NotFound();

            return View(temporada);
        }
        
         // POST: /Temporada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TemporadaId,TemporadaNombre,Comentarios")] Temporada temporada)
        {
            if (id != temporada.TemporadaId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(temporada);

            try
            {
                _context.Update(temporada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemporadaExists(temporada.TemporadaId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TemporadaExists(int id)
        {
            return _context.Temporada.Any(e => e.TemporadaId == id);
        }
    }
}