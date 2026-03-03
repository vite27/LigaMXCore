using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class EstatusJornadaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstatusJornadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADD: /EstatusJornada/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /EstatusJornada/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstatusJornadaNombre")] EstatusJornada estatusJornada)
        {
            if (!ModelState.IsValid)
                return View(estatusJornada);

            _context.Add(estatusJornada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /EstatusJornada
        public async Task<IActionResult> Index()
        {
            var list = await _context.EstatusJornada.ToListAsync();
            return View(list);
        }

        // GET: /EstatusJornada/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusJornada = await _context.EstatusJornada.FindAsync(id.Value);
            if (estatusJornada == null)
                return NotFound();

            return View(estatusJornada);
        }

        // POST: /EstatusJornada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstatusJornadaId,EstatusJornadaNombre")] EstatusJornada estatusJornada)
        {
            if (id != estatusJornada.EstatusJornadaId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(estatusJornada);

            try
            {
                _context.Update(estatusJornada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstatusJornadaExists(estatusJornada.EstatusJornadaId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EstatusJornadaExists(int id)
        {
            return _context.EstatusJornada.Any(e => e.EstatusJornadaId == id);
        }
    }
}