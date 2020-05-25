using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNetEd.AutoAdmin
{
    public static class AutoAdminConfigurationExtensions
    {
        public static void AddAutoAdmin(this IServiceCollection services)
        {
            foreach(var service in services.ToList())
            {
                if (service.ImplementationType?.BaseType == typeof(DbContext))
                {
                    services.AddTransient(services => new DiscoveredDbContextType() { Type = service.ImplementationType }) ;
                }
            }
        }
    }
}
