using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class TipoResultadoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoResultadoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TipoResultado
        public async Task<IActionResult> Index()
        {
            var list = await _context.TipoResultado.ToListAsync();
            return View(list);
        }

        // GET: /TipoResultado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estatusJornada = await _context.TipoResultado.FindAsync(id.Value);
            if (estatusJornada == null)
                return NotFound();

            return View(estatusJornada);
        }

        // POST: /EstatusJornada/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TipoResultadoId,TipoResultadoNombre")] TipoResultado tipoResultado)
        {
            if (id != tipoResultado.TipoResultadoId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(tipoResultado);

            try
            {
                _context.Update(tipoResultado);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoResultadoExists(tipoResultado.TipoResultadoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TipoResultadoExists(int id)
        {
            return _context.TipoResultado.Any(e => e.TipoResultadoId == id);
        }
    }
}