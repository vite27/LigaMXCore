using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class EstatusPartidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstatusPartidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADD: /EstatusPartido/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /EstatusPartido/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstatusPartidoNombre")] EstatusPartido estatusPartido)
        {
            if (!ModelState.IsValid)
                return View(estatusPartido);

            _context.Add(estatusPartido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /EstatusPartido
        public async Task<IActionResult> Index()
        {
            var list = await _context.EstatusPartido.ToListAsync();
            return View(list);
        }

        // GET: /EstatusPartido/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusPartido = await _context.EstatusPartido.FindAsync(id.Value);
            if (estatusPartido == null)
                return NotFound();

            return View(estatusPartido);
        }

        // POST: /EstatusPartido/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstatusPartidoId,EstatusPartidoNombre")] EstatusPartido estatusPartido)
        {
            if (id != estatusPartido.EstatusPartidoId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(estatusPartido);

            try
            {
                _context.Update(estatusPartido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstatusPartidoExists(estatusPartido.EstatusPartidoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EstatusPartidoExists(int id)
        {
            return _context.EstatusPartido.Any(e => e.EstatusPartidoId == id);
        }
    }
}