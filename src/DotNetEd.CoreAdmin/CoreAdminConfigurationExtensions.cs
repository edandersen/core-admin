using DotNetEd.CoreAdmin;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CoreAdminConfigurationExtensions
    {
        public static void AddCoreAdmin(this IServiceCollection services, Func<Task<bool>> customAuthorisationMethod)
        {
            FindDbContexts(services);

            if (customAuthorisationMethod != null)
            {
                var coreAdminSecurityOptions = new CoreAdminOptions();
                coreAdminSecurityOptions.CustomAuthorisationMethod = customAuthorisationMethod;
                services.AddSingleton(coreAdminSecurityOptions);
            }

            services.AddControllersWithViews();
        }

        public static void AddCoreAdmin(this IServiceCollection services, params string[] restrictToRoles)
        {
            FindDbContexts(services);

            if (restrictToRoles != null && restrictToRoles.Any())
            {
                var coreAdminSecurityOptions = new CoreAdminOptions();
                coreAdminSecurityOptions.RestrictToRoles = restrictToRoles;
                services.AddSingleton(coreAdminSecurityOptions);
            }

            services.AddControllersWithViews();

        }

        public static void UseCoreAdminCustomUrl(this IApplicationBuilder app, string customUrlPrefix)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "coreadminroute",
                    pattern: customUrlPrefix + "/{controller=CoreAdmin}/{action=Index}/{id?}");

            });
        }

        private static void FindDbContexts(IServiceCollection services)
        {
            foreach (var service in services.ToList())
            {
                if (service.ImplementationType == null)
                    continue;
                if (service.ImplementationType.IsSubclassOf(typeof(DbContext)))
                {
                    services.AddTransient(services => new DiscoveredDbContextType() { Type = service.ImplementationType });
                }
            }
        }
    }
}
