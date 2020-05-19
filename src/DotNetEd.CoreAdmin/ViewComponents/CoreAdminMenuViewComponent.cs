using DotNetEd.CoreAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin.ViewComponents
{
    public class CoreAdminMenuViewComponent : ViewComponent
    {
        private readonly IEnumerable<DiscoveredDbContextType> dbContexts;

        public CoreAdminMenuViewComponent(IEnumerable<DiscoveredDbContextType> dbContexts)
        {
            this.dbContexts = dbContexts;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new MenuViewModel();

            foreach(var dbContext in dbContexts)
            {
                viewModel.DbContextNames.Add(dbContext.Type.Name);
            }

            return View(viewModel);
        }
    }
}
