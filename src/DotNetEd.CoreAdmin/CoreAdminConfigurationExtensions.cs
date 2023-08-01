using DotNetEd.CoreAdmin;
using DotNetEd.CoreAdmin.Provider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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

		public static void AddCoreAdmin(this IServiceCollection services, CoreAdminOptions options)
		{
			if (options.FirebaseApiKey != null)
			{
				CoreAdminProvider.Instance.FirebaseApiKey = options.FirebaseApiKey;
			}

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
			foreach (var option in options)
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

		private static void AddLocalisation(IServiceCollection services)
		{
			services.AddSingleton<IStringLocalizer<JsonLocalizer>, JsonLocalizer>();
			services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
		}

		private static void FindDbContexts(IServiceCollection services, CoreAdminOptions options)
		{
			CoreAdminTree coreAdminTree = new CoreAdminTree();

			// remove previously discovered db contexts
			var servicesToRemove = services.Where(s => s.ServiceType == typeof(DiscoveredDbSetEntityType)).ToList();
			foreach (var serviceToRemove in servicesToRemove)
			{
				services.Remove(serviceToRemove);
			}

			var discoveredServices = new List<DiscoveredDbSetEntityType>();
			//foreach (var service in services.ToList())
			foreach (var serviceType in services.Where(s => s.ImplementationType != null).Select(s => s.ImplementationType))
			{
				if (serviceType.IsSubclassOf(typeof(DbContext)) &&
					!discoveredServices.Any(x => x.DbContextType == serviceType))
				{
					Dictionary<string, string> databaseName2ConnectionStrings = new Dictionary<string, string>();
					if (options.Context2ConnectionStrings != null && options.Context2ConnectionStrings.TryGetValue(serviceType.Name, out List<string> connectionStrings))
					{
						foreach (var connectionString in connectionStrings)
						{
							SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
							databaseName2ConnectionStrings.Add(builder.InitialCatalog, connectionString);
						}
					}

					List<PropertyInfo> dbSetProperties = new List<PropertyInfo>();

					foreach (var dbSetProperty in serviceType.GetProperties())
					{
						if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && !options.IgnoreEntityTypes.Contains(dbSetProperty.PropertyType.GenericTypeArguments.First()))
						{
							dbSetProperties.Add(dbSetProperty);
						}
					}

					if (databaseName2ConnectionStrings.Any())
					{
						foreach (var databaseName2ConnectionString in databaseName2ConnectionStrings)
						{
							var databaseName = databaseName2ConnectionString.Key;
							var connectionString = databaseName2ConnectionString.Value;
							foreach (var dbSetProperty in dbSetProperties)
							{
								discoveredServices.Add(new DiscoveredDbSetEntityType()
								{
									DbContextType = serviceType,
									DbSetType = dbSetProperty.PropertyType,
									UnderlyingType = dbSetProperty.PropertyType.GenericTypeArguments.First(),
									Name = databaseName + " - " + dbSetProperty.Name,
									ConnectionString = connectionString
								});
							}

							coreAdminTree.Db2Tables.Add(databaseName, dbSetProperties.Select(x => x.Name).ToList());
						}
					}
					else
					{
						foreach (var dbSetProperty in dbSetProperties)
						{
							discoveredServices.Add(new DiscoveredDbSetEntityType()
							{
								DbContextType = serviceType,
								DbSetType = dbSetProperty.PropertyType,
								UnderlyingType = dbSetProperty.PropertyType.GenericTypeArguments.First(),
								Name = dbSetProperty.Name,
								ConnectionString = null
							});
						}
					}
				}
			}

			foreach (var service in discoveredServices)
			{
				services.AddTransient(_ => service);
			}
			services.AddTransient(_ => coreAdminTree);
		}
	}
}
