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

        // ADD: /Pais/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /Pais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("PaisNombre")] Pais pais)
        {
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

        private bool PaisExists(int id)
        {
            return _context.Pais.Any(e => e.PaisId == id);
        }
    }
}
