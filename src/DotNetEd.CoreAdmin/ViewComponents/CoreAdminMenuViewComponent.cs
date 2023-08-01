using DotNetEd.CoreAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DotNetEd.CoreAdmin.ViewComponents
{
	public class CoreAdminMenuViewComponent : ViewComponent
	{
		private readonly CoreAdminTree coreAdminTree;

		public CoreAdminMenuViewComponent(CoreAdminTree coreAdminTree)
		{
			this.coreAdminTree = coreAdminTree;
		}

		public IViewComponentResult Invoke()
		{
			var viewModel = new MenuViewModel();

			viewModel.Db2Tables = coreAdminTree.Db2Tables;

			return View(viewModel);
		}
	}
}
