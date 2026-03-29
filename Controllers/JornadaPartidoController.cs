using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaMXCore.Data;
using LigaMXCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    }
}