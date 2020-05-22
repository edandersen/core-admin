using DotNetEd.AutoAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEd.AutoAdmin.ViewComponents
{
    public class AutoAdminMenuViewComponent : ViewComponent
    {
        private readonly IEnumerable<DiscoveredDbContextType> dbContexts;

        public AutoAdminMenuViewComponent(IEnumerable<DiscoveredDbContextType> dbContexts)
        {
            this.dbContexts = dbContexts;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new MenuViewModel();

            foreach(var dbContext in dbContexts)
            {
                viewModel.DbContextNames.Add(dbContext.Type.Name);

                foreach(var dbSetProperty in dbContext.Type.GetProperties())
                {
                    // looking for DbSet<Entity>
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet"))
                    {
                        viewModel.DbSetNames.Add(dbSetProperty.Name);
                    }    
                }
            }

            return View(viewModel);
        }
    }
}
