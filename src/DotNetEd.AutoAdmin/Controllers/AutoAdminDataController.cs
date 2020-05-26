using DotNetEd.AutoAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [Route("list/{id}", Order = 1)]
        [HttpGet]
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

        [HttpPost]
        [Route("deleteentity")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteEntity([FromForm] DataDeleteViewModel viewModel)
        {
            foreach (var dbContext in dbContexts)
            {
                foreach (var dbSetProperty in dbContext.Type.GetProperties())
                {
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && dbSetProperty.Name.ToLowerInvariant() == viewModel.DbSetName.ToLowerInvariant())
                    {
                        var dbContextObject = (DbContext)this.HttpContext.RequestServices.GetRequiredService(dbContext.Type);
                        var dbSetValue = dbSetProperty.GetValue(dbContextObject);

                        var primaryKey = dbContextObject.Model.FindEntityType(dbSetProperty.PropertyType.GetGenericArguments()[0]).FindPrimaryKey();
                        var clrType = primaryKey.Properties[0].ClrType;

                        object convertedPrimaryKey = viewModel.Id;
                        if (clrType == typeof(Guid))
                        {
                            convertedPrimaryKey = Guid.Parse(viewModel.Id);
                        } 
                        else if(clrType == typeof(int))
                        {
                            convertedPrimaryKey = int.Parse(viewModel.Id);
                        } 
                        else if (clrType == typeof(Int64))
                        {
                            convertedPrimaryKey = Int64.Parse(viewModel.Id);
                        }

                        var entityToDelete = dbSetValue.GetType().InvokeMember("Find", BindingFlags.InvokeMethod, null, dbSetValue, args: new object[] { convertedPrimaryKey });
                        dbSetValue.GetType().InvokeMember("Remove", BindingFlags.InvokeMethod, null, dbSetValue, args: new object[] {entityToDelete});

                        await dbContextObject.SaveChangesAsync();
                        
                    }
                }
            }

            return Content("OK");

        }
    }
}
