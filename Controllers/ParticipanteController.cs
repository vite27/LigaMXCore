using System.Collections.Generic;
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

        // GET: /Participante
        public async Task<IActionResult> Index()
        {
            var list = await _context.Participante.ToListAsync();
            return View(list);
        }

        // GET: /Participante/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var participante = await _context.Participante.FindAsync(id.Value);
            if (participante == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(participante);
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
            if (!string.IsNullOrWhiteSpace(participante.Nombres))
                participante.Nombres = participante.Nombres.Trim();
            if (!string.IsNullOrWhiteSpace(participante.ApellidoPaterno))
                participante.ApellidoPaterno = participante.ApellidoPaterno.Trim();
            if (!string.IsNullOrWhiteSpace(participante.ApellidoMaterno))
                participante.ApellidoMaterno = participante.ApellidoMaterno.Trim();

            if (!ModelState.IsValid)
                return View(participante);

            _context.Add(participante);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

            if (!string.IsNullOrWhiteSpace(participante.Nombres))
                participante.Nombres = participante.Nombres.Trim();
            if (!string.IsNullOrWhiteSpace(participante.ApellidoPaterno))
                participante.ApellidoPaterno = participante.ApellidoPaterno.Trim();
            if (!string.IsNullOrWhiteSpace(participante.ApellidoMaterno))
                participante.ApellidoMaterno = participante.ApellidoMaterno.Trim();

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

        // GET: /Participante/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var participante = await _context.Participante.FindAsync(id.Value);
            if (participante == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(participante);
        }

        // POST: /Participante/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participante = await _context.Participante.FindAsync(id);
            if (participante == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(participante);
            }

            _context.Participante.Remove(participante);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ParticipanteExists(int id)
        {
            return _context.Participante.Any(e => e.ParticipanteId == id);
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int participanteId)
        {
            return new Dictionary<string, int>
            {
                ["JornadaPronosticos"] = await _context.JornadaPronosticos.CountAsync(jp => jp.ParticipanteId == participanteId)
            };
        }
    }
}
