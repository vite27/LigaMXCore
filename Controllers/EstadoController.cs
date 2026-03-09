using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class EstadoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADD: /Estado/Add
        public IActionResult Add()
        {
            ViewData["PaisId"] = new SelectList(_context.Pais, "PaisId", "PaisNombre");
            return View();
        }

        // ADD: /Estado/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstadoNombre,PaisId")] Estado estado)
        {
            ModelState.Remove("Pais");
            if (!ModelState.IsValid)
            {
                ViewData["PaisId"] = new SelectList(_context.Pais, "PaisId", "PaisNombre");
                return View(estado);
            }

            _context.Add(estado);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Estado
        public async Task<IActionResult> Index()
        {
            var list = await _context.Estados.Include(e => e.Pais).ToListAsync();
            return View(list);
        }

        // GET: /Estado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estado = await _context.Estados.FindAsync(id.Value);
            if (estado == null)
                return NotFound();

            ViewData["PaisId"] = new SelectList(_context.Pais, "PaisId", "PaisNombre", estado.PaisId);
            return View(estado);
        }

        // POST: /Estado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstadoId,EstadoNombre,PaisId")] Estado estado)
        {
            if (id != estado.EstadoId)
                return NotFound();

            ModelState.Remove("Pais");
            if (!ModelState.IsValid)
            {
                ViewData["PaisId"] = new SelectList(_context.Pais, "PaisId", "PaisNombre", estado.PaisId);
                return View(estado);
            }

            try
            {
                _context.Update(estado);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstadoExists(estado.EstadoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EstadoExists(int id)
        {
            return _context.Estados.Any(e => e.EstadoId == id);
        }
    }
}