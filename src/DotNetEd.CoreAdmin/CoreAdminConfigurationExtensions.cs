using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNetEd.CoreAdmin
{
    public static class CoreAdminConfigurationExtensions
    {
        public static void AddCoreAdmin(this IServiceCollection services)
        {
            foreach(var service in services.ToList())
            {
                if (service.ImplementationType?.BaseType == typeof(DbContext))
                {
                    services.AddTransient(services => new DiscoveredDbContextType() { Type = service.ImplementationType }) ;
                }
            }
        }

        public static void UseCoreAdmin(this IApplicationBuilder app)
        {
           // var dbContexts = app.ApplicationServices.

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "coreadmin",
                    pattern: "/admin/{controller=CoreAdminHome}/{action=Index}/{id?}");
            });
        }
    }
}
