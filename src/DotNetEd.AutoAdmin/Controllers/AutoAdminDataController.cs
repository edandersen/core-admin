using DotNetEd.AutoAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEd.AutoAdmin.Controllers
{
    [Route("autoadmin/data")]
    public class AutoAdminDataController : Controller
    {
        private readonly IEnumerable<DiscoveredDbContextType> dbContexts;

        public AutoAdminDataController(IEnumerable<DiscoveredDbContextType> dbContexts)
        {
            this.dbContexts = dbContexts;
        }

        [Route("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var viewModel = new DataListViewModel();

            foreach (var dbContext in dbContexts)
            {
                foreach (var dbSetProperty in dbContext.Type.GetProperties())
                {
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && dbSetProperty.Name.ToLowerInvariant() == id.ToLowerInvariant())
                    {
                        viewModel.EntityType = dbSetProperty.PropertyType.GetGenericArguments().First();
                        viewModel.DbSetProperty = dbSetProperty;

                        var dbContextObject = this.HttpContext.RequestServices.GetRequiredService(dbContext.Type);

                        var dbSetValue = dbSetProperty.GetValue(dbContextObject);

                        viewModel.Data = (IEnumerable<object>)dbSetValue;
                    }
                }
            }

            return View(viewModel);
        }
    }
}
