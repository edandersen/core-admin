using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetEd.AutoAdmin.Controllers
{
    [Route("admin")]
    public class AutoAdminHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
