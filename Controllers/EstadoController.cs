using System.Collections.Generic;
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

            if (!string.IsNullOrWhiteSpace(estado.EstadoNombre))
                estado.EstadoNombre = estado.EstadoNombre.Trim();

            await ValidarNombreDuplicado(estado.EstadoNombre, estado.PaisId, idActual: 0);

            if (!ModelState.IsValid)
            {
                ViewData["PaisId"] = new SelectList(_context.Pais, "PaisId", "PaisNombre", estado.PaisId);
                return View(estado);
            }

            _context.Add(estado);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Estado
        public async Task<IActionResult> Index(int? paisId, string? nombre)
        {
            var query = _context.Estados.Include(e => e.Pais).AsQueryable();

            if (paisId.HasValue)
                query = query.Where(e => e.PaisId == paisId.Value);

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var patron = "%" + QuitarAcentos(nombre.Trim()) + "%";
                query = query.Where(e => EF.Functions.Like(
                    e.EstadoNombre
                        .Replace("Á", "a").Replace("É", "e").Replace("Í", "i").Replace("Ó", "o").Replace("Ú", "u").Replace("Ü", "u").Replace("Ñ", "n")
                        .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ü", "u").Replace("ñ", "n"),
                    patron));
            }

            ViewData["PaisId"] = new SelectList(_context.Pais, "PaisId", "PaisNombre", paisId);
            ViewData["NombreFiltro"] = nombre;

            var list = await query.ToListAsync();
            return View(list);
        }

        // GET: /Estado/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var estado = await _context.Estados.Include(e => e.Pais).FirstOrDefaultAsync(e => e.EstadoId == id.Value);
            if (estado == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estado);
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

            if (!string.IsNullOrWhiteSpace(estado.EstadoNombre))
                estado.EstadoNombre = estado.EstadoNombre.Trim();

            await ValidarNombreDuplicado(estado.EstadoNombre, estado.PaisId, idActual: id);

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

        // GET: /Estado/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var estado = await _context.Estados.Include(e => e.Pais).FirstOrDefaultAsync(e => e.EstadoId == id.Value);
            if (estado == null)
                return NotFound();

            ViewData["Dependencias"] = await ObtenerDependencias(id.Value);

            return View(estado);
        }

        // POST: /Estado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estado = await _context.Estados.Include(e => e.Pais).FirstOrDefaultAsync(e => e.EstadoId == id);
            if (estado == null)
                return NotFound();

            var dependencias = await ObtenerDependencias(id);
            if (dependencias.Values.Sum() > 0)
            {
                ViewData["Dependencias"] = dependencias;
                ModelState.AddModelError(string.Empty, "No se puede eliminar: existen registros dependientes en los catálogos listados.");
                return View(estado);
            }

            _context.Estados.Remove(estado);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EstadoExists(int id)
        {
            return _context.Estados.Any(e => e.EstadoId == id);
        }

        // Normaliza el término de búsqueda a minúsculas sin acentos, para que coincida
        // con la misma normalización aplicada a EstadoNombre en el filtro de Index
        // (ver el Replace en cadena ahí, que SQLite sí puede traducir a SQL).
        private static string QuitarAcentos(string texto)
        {
            var mapa = new Dictionary<char, char>
            {
                ['á'] = 'a', ['é'] = 'e', ['í'] = 'i', ['ó'] = 'o', ['ú'] = 'u', ['ü'] = 'u', ['ñ'] = 'n'
            };

            var minuscula = texto.ToLowerInvariant();
            var resultado = new System.Text.StringBuilder(minuscula.Length);
            foreach (var c in minuscula)
                resultado.Append(mapa.TryGetValue(c, out var reemplazo) ? reemplazo : c);

            return resultado.ToString();
        }

        // Conteo por catálogo dependiente (sin cargar los registros completos, pensado
        // para catálogos donde la dependencia puede ser muy amplia).
        private async Task<Dictionary<string, int>> ObtenerDependencias(int estadoId)
        {
            return new Dictionary<string, int>
            {
                ["Municipios"] = await _context.Municipios.CountAsync(m => m.EstadoId == estadoId)
            };
        }

        private async Task ValidarNombreDuplicado(string? nombre, int paisId, int idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return;

            var estados = await _context.Estados
                .Where(e => e.PaisId == paisId && e.EstadoId != idActual)
                .ToListAsync();

            var existe = estados.Exists(e =>
                string.Equals(e.EstadoNombre?.Trim(), nombre, System.StringComparison.OrdinalIgnoreCase));

            if (existe)
                ModelState.AddModelError("EstadoNombre", "Ya existe un estado con ese nombre en este país.");
        }
    }
}