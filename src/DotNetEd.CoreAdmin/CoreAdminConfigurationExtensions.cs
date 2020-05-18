using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetEd.CoreAdmin
{
    public static class CoreAdminConfigurationExtensions
    {
        public static void AddCoreAdmin(this IServiceCollection services)
        {

        }

        public static void UseCoreAdmin(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "coreadmin",
                    pattern: "/admin/{controller=CoreAdminHome}/{action=Index}/{id?}");
            });
        }
    }
}
