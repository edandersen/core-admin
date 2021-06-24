using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin
{
    public class CoreAdminOptions
    {
        public string[] RestrictToRoles { get; internal set; }
        public Func<Task<bool>> CustomAuthorisationMethod { get; internal set; }
        public bool IsSecuritySet => (RestrictToRoles != null && RestrictToRoles.Length > 0) || CustomAuthorisationMethod != null;

        public string CdnPath { get; internal set; }
    }
}
