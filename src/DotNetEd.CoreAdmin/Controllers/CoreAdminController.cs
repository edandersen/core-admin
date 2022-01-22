using Microsoft.AspNetCore.Mvc;

namespace DotNetEd.CoreAdmin.Controllers
{
    [CoreAdminAuth]
    public class CoreAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
