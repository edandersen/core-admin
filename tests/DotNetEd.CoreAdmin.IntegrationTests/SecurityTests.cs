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
    public class SecurityTests : IClassFixture<IntegrationTestsWebHostFactory<IntegrationTestStartup>>
    {
        private readonly IntegrationTestsWebHostFactory<IntegrationTestStartup> _factory;

        public SecurityTests(IntegrationTestsWebHostFactory<IntegrationTestStartup> factory)
        {
            _factory = factory;
        }

        static void ConfigureTestServices(IServiceCollection services) { }

        static void ConfigureTestServicesWithSecurity(IServiceCollection services) { 
            services.AddCoreAdmin("TestRole"); 
        }

        [Obsolete]
        static void ConfigureTestServicesWithSecurityAuthMethod(IServiceCollection services, Func<Task<bool>> authMethod)
        {
            services.AddCoreAdmin(authMethod);
        }

        static void ConfigureTestServicesWithSecurityAndAlternativeTestRole(IServiceCollection services)
        {
            services.AddCoreAdmin("TestRole2");
        }

        [Fact]
        public async Task ShowsWarningMessageInDevelopmentModeWhenNoSecuritySet()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { 
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices); }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("You are running in Development mode.", content);

        }

        [Fact]
        public async Task DoesNotShowWarningMessageInDevelopmentModeWhenSecurityIsSet()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServicesWithSecurity);
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());

            var content = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain("You are running in Development mode.", content);

        }

        [Fact]
        public async Task ReturnsUnauthorisedInDevelopmentModeWhenSecurityIsSetAndCheckFails()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServicesWithSecurityAndAlternativeTestRole);
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ReturnsUnauthorizedWhenInProductionButNoSecuritySet()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
                    builder.ConfigureTestServices(ConfigureTestServices)).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SuccessStatusCodeAndUsesRoleWhenInProductionAndRoleCheckSet()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
                    builder.ConfigureTestServices(ConfigureTestServicesWithSecurity)).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmindata/index/testentities");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

        }

        [Fact]
        [Obsolete]
        public async Task SuccessStatusCodeAndCallsCustomAuthMethodInProductionAndAuthMethodCheckSet()
        {
            bool authCalled = false;

            var authMethod = new Func<Task<bool>>(() => { authCalled = true; return Task.FromResult(true); });

            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
                    builder.ConfigureTestServices(services => 
                    ConfigureTestServicesWithSecurityAuthMethod(services, authMethod))).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmindata/index/testentities");

            // Assert
            Assert.True(authCalled);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        [Obsolete]
        public async Task ReturnsUnauthorizedWhenInProductionCustomAuthMethodFails()
        {
            bool authCalled = false;

            var authMethod = new Func<Task<bool>>(() => { authCalled = true; return Task.FromResult(false); });

            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
                   builder.ConfigureTestServices(services =>
                    ConfigureTestServicesWithSecurityAuthMethod(services, authMethod))).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmindata/index/testentities");

            // Assert
            Assert.True(authCalled);
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ReturnsUnauthorizedWhenInProductionNewCustomAuthMethodFails()
        {
            bool authCalled = false;

            var authMethod = new Func<IServiceProvider, Task<bool>>((serviceProvider) => { authCalled = true; return Task.FromResult(false); });

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Production");
                builder.ConfigureTestServices(ConfigureTestServices);
                builder.Configure(
                        app =>
                        {
                            app.UseRouting();
                            app.UseCoreAdminCustomAuth(authMethod);
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
            var response = await client.GetAsync("/coreadmindata/index/testentities");

            // Assert
            Assert.True(authCalled);
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }
    }
}
