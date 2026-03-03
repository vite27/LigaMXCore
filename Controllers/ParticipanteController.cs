using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;

namespace LigaMXCore.Controllers
{
    public class ParticipanteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParticipanteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADD: /Participante/Add
        public IActionResult Add()
        {
            return View();
        }

        // ADD: /Participante/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Nombres,ApellidoPaterno,ApellidoMaterno")] Participante participante)
        {
            if (!ModelState.IsValid)
                return View(participante);

            _context.Add(participante);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Participante
        public async Task<IActionResult> Index()
        {
            var list = await _context.Participante.ToListAsync();
            return View(list);
        }

        // GET: /Participante/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var participante = await _context.Participante.FindAsync(id.Value);
            if (participante == null)
                return NotFound();

            return View(participante);
        }
        
         // POST: /Participante/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParticipanteId,Nombres,ApellidoPaterno,ApellidoMaterno")] Participante participante)
        {
            if (id != participante.ParticipanteId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(participante);

            try
            {
                _context.Update(participante);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipanteExists(participante.ParticipanteId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ParticipanteExists(int id)
        {
            return _context.Participante.Any(e => e.ParticipanteId == id);
        }
    }

}