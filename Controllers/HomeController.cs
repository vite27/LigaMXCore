using Microsoft.AspNetCore.Mvc;

namespace LigaMXCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}