using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEd.CoreAdmin
{
    public class CoreAdminAuthAttribute : TypeFilterAttribute
    {
        public CoreAdminAuthAttribute() : base(typeof(CoreAdminAuthFilter)) { }
    }

    public class CoreAdminAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly IWebHostEnvironment environment;
        private readonly IList<CoreAdminOptions> coreAdminOptions;
        private readonly IServiceProvider serviceProvider;

        public CoreAdminAuthFilter(IWebHostEnvironment environment, IEnumerable<CoreAdminOptions> securityOptions, IServiceProvider serviceProvider)
        {
            this.environment = environment;
            this.coreAdminOptions = securityOptions.ToList();
            this.serviceProvider = serviceProvider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            bool failedSecurityCheck = true;

            // Permit the Development-only bypass only when explicitly opted in.
            // Missing security configuration is denied by default in every environment.
            if (environment.EnvironmentName == "Development"
                && coreAdminOptions.Any(o => o.AllowUnauthenticatedAccessInDevelopment)
                && !coreAdminOptions.Any(o => o.IsSecuritySet))
            {
                failedSecurityCheck = false;
            }
            else
            {
                foreach (var options in coreAdminOptions)
                {
                    if (options.RestrictToRoles != null && options.RestrictToRoles.Any())
                    {
                        foreach (var role in options.RestrictToRoles)
                        {
                            if (context.HttpContext.User.IsInRole(role))
                            {
                                failedSecurityCheck = false;
                            }
                        }
                    }

                    if (options.CustomAuthorisationMethod != null)
                    {
                        if (await options.CustomAuthorisationMethod())
                        {
                            failedSecurityCheck = false;
                        }
                    }

                    if (options.CustomAuthorisationMethodWithServiceProvider != null)
                    {
                        if (await options.CustomAuthorisationMethodWithServiceProvider(serviceProvider))
                        {
                            failedSecurityCheck = false;
                        }
                    }
                }
            }

            if (failedSecurityCheck) context.Result = new UnauthorizedResult();

        }
    }
}
