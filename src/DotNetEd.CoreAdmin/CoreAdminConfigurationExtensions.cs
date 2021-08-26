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

            var coreAdminOptions = new CoreAdminOptions();
            
            if (customAuthorisationMethod != null)
            {
                coreAdminOptions.CustomAuthorisationMethod = customAuthorisationMethod;
            }
           
            services.AddSingleton(coreAdminOptions);
            

            services.AddControllersWithViews();
        }

        public static void AddCoreAdmin(this IServiceCollection services, params string[] restrictToRoles)
        {
            FindDbContexts(services);

            var coreAdminOptions = new CoreAdminOptions();
            
            if (restrictToRoles != null && restrictToRoles.Any())
            {
                coreAdminOptions.RestrictToRoles = restrictToRoles;
            }
            
            services.AddSingleton(coreAdminOptions);

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

        public static void UseCoreAdminCdn(this IApplicationBuilder app, string cdnPath)
        {
            var options = app.ApplicationServices.GetServices<CoreAdminOptions>();
            foreach(var option in options)
            {
                option.CdnPath = cdnPath;
            }
        }

        private static void FindDbContexts(IServiceCollection services)
        {
            List<DiscoveredDbContextType> discoveredServices = new();
            foreach (var service in services.ToList())
            {
                if (service.ImplementationType == null)
                    continue;
                if (service.ImplementationType.IsSubclassOf(typeof(DbContext)) && !discoveredServices.Any(x => x.Type == service.ImplementationType)){
                    discoveredServices.Add(new DiscoveredDbContextType() { Type = service.ImplementationType });
                    services.AddTransient(_ => new DiscoveredDbContextType() { Type = service.ImplementationType });
                }
            }
        }
    }
}
