using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Nodes;

namespace LigaMXCore.Controllers
{
    
    public class JornadaPartidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JornadaPartidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Jornada
        public async Task<IActionResult> Index()
        {
            var list = await _context.JornadaPartidos
                .Include(j => j.Jornada)
                .Include(j => j.Partido)
                    .ThenInclude(p => p.EquipoLocal)
                .Include(j => j.Partido)
                    .ThenInclude(p => p.EquipoVisita)
                .Include(j => j.Estadio)
                .Include(j => j.EstatusPartido)
                .Include(j => j.TipoResultado)
                .ToListAsync();
            return View(list);
        } 

        // POST: /Jornada/Add
        [HttpPost]
        public JsonResult UpdateScores([FromBody] List<JornadaPartido> item) 
        {
            try 
            {
                foreach (var partidoEnviado in item)
                {
                    var partidoDb = _context.JornadaPartidos.Find(partidoEnviado.JornadaPartidoId);

                    if (partidoDb != null)
                    {
                        partidoDb.GolLocal = partidoEnviado.GolLocal;
                        partidoDb.GolVisita = partidoEnviado.GolVisita;
                        partidoDb.EstatusPartidoId = partidoEnviado.EstatusPartidoId;
                
                        _context.Entry(partidoDb).State = EntityState.Modified;
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, message = $"{item.Count} marcadores actualizados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar: " + ex.Message });
            }

        }

    }
}