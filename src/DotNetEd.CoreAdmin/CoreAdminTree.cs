using System.Collections.Generic;

namespace DotNetEd.CoreAdmin
{
	public class CoreAdminTree
	{
		public CoreAdminTree()
		{
			Db2Tables = new Dictionary<string, List<string>>();
		}

		public Dictionary<string, List<string>> Db2Tables { get; set; }
	}
}
