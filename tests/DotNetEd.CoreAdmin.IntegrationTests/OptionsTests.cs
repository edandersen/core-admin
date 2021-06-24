using DotNetEd.CoreAdmin.IntegrationTests.TestApp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DotNetEd.CoreAdmin.IntegrationTests
{
    public class OptionsTests : IClassFixture<IntegrationTestsWebHostFactory<IntegrationTestStartup>>
    {
        private readonly IntegrationTestsWebHostFactory<IntegrationTestStartup> _factory;

        public OptionsTests(IntegrationTestsWebHostFactory<IntegrationTestStartup> factory)
        {
            _factory = factory;
        }

        static void ConfigureTestServices(IServiceCollection services) { }

        [Fact]
        public async Task ReturnsBuiltInAssetPathWhenCdnOptionsNotSet()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

            var pageContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("<link rel=\"stylesheet\" href=\"/_content/CoreAdmin/css/bootstrap.min.css\" />", pageContent);
        }

        [Fact]
        public async Task ReturnsCdnAssetPathWhenCdnOptionsSet()
        {
            var cdnPath = "https://wow-an-amazing-cdn.com/assets";

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
                builder.Configure(
                        app =>
                        {
                            app.UseRouting();
                            app.UseCoreAdminCdn(cdnPath);
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllerRoute(
                                    name: "default",
                                    pattern: "{controller=Home}/{action=Index}/{id?}");

                            });
                        }
                    );
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

            var pageContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("<link rel=\"stylesheet\" href=\"" + cdnPath + "/css/bootstrap.min.css\" />", pageContent);
        }


    }
}
