using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEd.CoreAdmin.Controllers
{
	[Authorize(Policy = CookieAuthenticationDefaults.AuthenticationScheme)]
	public class CoreAdminController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
