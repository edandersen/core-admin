using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetEd.CoreAdmin.Controllers
{
    [Route("coreadmin")]
    [CoreAdminAuth]
    public class CoreAdminHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
