using DotNetEd.CoreAdmin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CoreAdminConfigurationExtensions
    {
        [Obsolete("Use app.UseCoreAdminCustomAuth()")]
        public static void AddCoreAdmin(this IServiceCollection services, Func<Task<bool>> customAuthorisationMethod)
        {
            var coreAdminOptions = new CoreAdminOptions();

            FindDbContexts(services, coreAdminOptions);

            if (customAuthorisationMethod != null)
            {
                coreAdminOptions.CustomAuthorisationMethod = customAuthorisationMethod;
            }

            services.AddSingleton(coreAdminOptions);

            AddLocalisation(services);

            services.AddControllersWithViews();
        }

        private static void AddLocalisation(IServiceCollection services)
        {
            services.AddSingleton<IStringLocalizer<JsonLocalizer>, JsonLocalizer>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void AddCoreAdmin(this IServiceCollection services, CoreAdminOptions options)
        {
            FindDbContexts(services, options);

            services.AddSingleton(options);

            services.AddControllersWithViews();

            AddLocalisation(services);
        }

        public static void AddCoreAdmin(this IServiceCollection services, params string[] restrictToRoles)
        {

            var coreAdminOptions = new CoreAdminOptions();

            FindDbContexts(services, coreAdminOptions);

            if (restrictToRoles != null && restrictToRoles.Any())
            {
                coreAdminOptions.RestrictToRoles = restrictToRoles;
            }

            services.AddSingleton(coreAdminOptions);

            AddLocalisation(services);

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

        public static void UseCoreAdminCustomTitle(this IApplicationBuilder app, string customTitle)
        {
            var options = app.ApplicationServices.GetServices<CoreAdminOptions>();
            foreach (var option in options)
            {
                option.Title = customTitle;
            }
        }

        public static void UseCoreAdminCustomAuth(this IApplicationBuilder app, Func<IServiceProvider, Task<bool>> customAuthFunction)
        {
            var options = app.ApplicationServices.GetServices<CoreAdminOptions>();
            foreach (var option in options)
            {
                option.CustomAuthorisationMethodWithServiceProvider = customAuthFunction;
            }
        }

        private static void FindDbContexts(IServiceCollection services, CoreAdminOptions options)
        {
            // remove previously discovered db contexts
            var servicesToRemove = services.Where(s => s.ServiceType == typeof(DiscoveredDbSetEntityType)).ToList();
            foreach(var serviceToRemove in servicesToRemove)
            {
                services.Remove(serviceToRemove);
            }

            var discoveredServices = new List<DiscoveredDbSetEntityType>();

            var dbContextImplementations = services
                .Where(x => x.Lifetime is ServiceLifetime.Scoped && x.ServiceType.IsSubclassOf(typeof(DbContext)))
                .ToList();

            foreach (var dbContextImplementation in dbContextImplementations)
            {
                foreach (var dbSetProperty in dbContextImplementation.ServiceType.GetProperties())
                {
                    // looking for DbSet<Entity>
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet"))
                    {
                        if (!options.IgnoreEntityTypes.Contains(dbSetProperty.PropertyType.GenericTypeArguments.First()))
                        {
                            discoveredServices.Add(new DiscoveredDbSetEntityType {
                                DbContextType = dbContextImplementation.ServiceType,
                                DbSetType = dbSetProperty.PropertyType,
                                UnderlyingType = dbSetProperty.PropertyType.GenericTypeArguments.First(), Name = dbSetProperty.Name });
                        }
                    }
                }
            }

            foreach (var service in discoveredServices)
            {
                services.AddTransient(_ => service);
            }
        }
    }
}
