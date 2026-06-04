using Microsoft.AspNetCore.Mvc;

namespace HMS.PL.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}