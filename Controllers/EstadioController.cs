using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LigaMXCore.Controllers
{
    public class EstadioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADD: /Estadio/Add
        public IActionResult Add()
        {
            ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre");
            return View();
        }

        // ADD: /Estadio/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("EstadioNombre,Alias,Direccion,CodigoPostal,MunicipioId")] Estadio estadio)
        {
            ModelState.Remove("Municipio");
            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre");
                return View(estadio);
            }

            _context.Add(estadio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Estadio
        public async Task<IActionResult> Index()
        {
            var list = await _context.Estadios.Include(e => e.Municipio).ToListAsync();
            return View(list);
        }

        // GET: /Estadio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var estadio = await _context.Estadios.FindAsync(id.Value);
            if (estadio == null)
                return NotFound();

            ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", estadio.MunicipioId);
            return View(estadio);
        }

        // POST: /Estadio/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EstadioId,EstadioNombre,Alias,Direccion,CodigoPostal,MunicipioId")] Estadio estadio)
        {
            if (id != estadio.EstadioId)
                return NotFound();

            ModelState.Remove("Municipio");
            if (!ModelState.IsValid)
            {
                ViewData["MunicipioId"] = new SelectList(_context.Municipios, "MunicipioId", "MunicipioNombre", estadio.MunicipioId);
                return View(estadio);
            }

            try
            {
                _context.Update(estadio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstadioExists(estadio.EstadioId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EstadioExists(int id)
        {
            return _context.Estadios.Any(e => e.EstadioId == id);
        }
    }
}