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
        private readonly IEnumerable<DiscoveredDbSetEntityType> dbSetEntities;

        public CoreAdminMenuViewComponent(IEnumerable<DiscoveredDbSetEntityType> dbContexts)
        {
            this.dbSetEntities = dbContexts;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new MenuViewModel();

            foreach(var dbSetEntity in dbSetEntities)
            {
                viewModel.DbContextNames.Add(dbSetEntity.DbContextType.Name);
                viewModel.DbSetNames.Add(dbSetEntity.Name);
            }    

            return View(viewModel);
        }
    }
}
