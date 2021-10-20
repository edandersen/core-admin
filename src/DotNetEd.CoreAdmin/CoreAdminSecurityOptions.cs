using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
