using System.Collections.Generic;

namespace DotNetEd.CoreAdmin.ViewModels
{
	public class MenuViewModel
	{
		public Dictionary<string, List<string>> Db2Tables { get; set; } = new Dictionary<string, List<string>>();
	}
}
