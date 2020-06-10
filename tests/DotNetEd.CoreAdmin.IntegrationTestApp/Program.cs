using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEd.CoreAdmin.IntegrationTests.TestApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetEd.CoreAdmin.IntegrationTestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<IntegrationTestStartup>();
                });
    }
}
