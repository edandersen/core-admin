using DotNetEd.CoreAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public IViewComponentResult Invoke()
        {
            var viewModel = new MenuViewModel();

            foreach(var dbSetEntity in dbSetEntities)
            {
                viewModel.DbContextNames.Add(dbSetEntity.DbContextType.Name);
                viewModel.DbSetNames.Add(dbSetEntity.Name);

                var d = dbSetEntity.Name;
                if (Attribute.IsDefined(dbSetEntity.UnderlyingType, typeof(DisplayAttribute)))
                {
                    var displayAttribute = dbSetEntity.UnderlyingType.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
                    if (!String.IsNullOrEmpty(displayAttribute.Name))
                        d = displayAttribute.Name;
                }
                viewModel.DbDisplayNames.Add(d);
            }    

            return View(viewModel);
        }
    }
}
