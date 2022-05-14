using NonFactors.Mvc.Grid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin
{
    public class CoreAdminOptions
    {
        public string[] RestrictToRoles { get; set; }
        public Func<Task<bool>> CustomAuthorisationMethod { get; set; }
        public bool IsSecuritySet => (RestrictToRoles != null && RestrictToRoles.Length > 0) || CustomAuthorisationMethod != null || CustomAuthorisationMethodWithServiceProvider != null;

        public string CdnPath { get; set; }
        public Func<IServiceProvider, Task<bool>> CustomAuthorisationMethodWithServiceProvider { get; set; }
        public IEnumerable<Type> IgnoreEntityTypes { get; set; } = new List<Type>();
        public Dictionary<Int32, string> PageSizes { get; set; }
        public bool ShowPageSizes { get; set; }
        public GridFilterMode FilterMode { get; set; }
        public string Title { get; set; } = "Core Admin";

        public CoreAdminOptions()
        {
            PageSizes = new Dictionary<Int32, String> { { 0, "All" }, { 10, "10" }, { 20, "20" } };
            ShowPageSizes = true;
            FilterMode = GridFilterMode.Header;
        }
    }
}
