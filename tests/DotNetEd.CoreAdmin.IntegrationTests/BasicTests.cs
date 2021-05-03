using DotNetEd.CoreAdmin.IntegrationTests.TestApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DotNetEd.CoreAdmin.IntegrationTests
{
    public class BasicTests : IClassFixture<IntegrationTestsWebHostFactory<IntegrationTestStartup>>
    {
        private readonly IntegrationTestsWebHostFactory<IntegrationTestStartup> _factory;

        public BasicTests(IntegrationTestsWebHostFactory<IntegrationTestStartup> factory)
        {
            _factory = factory;
        }

        static void ConfigureTestServices(IServiceCollection services) { }

        [Fact]
        public async Task ShowsTestEntitiesOnScreenOnAdminScreen()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmin");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("TestEntities", content);

        }

        [Fact]
        public async Task ShowDataInDbSetOnScreen()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();
            dbContext.TestEntities.Add(new TestApp.Entities.TestEntity() { Id = idGuid, Name = nameGuidString});
            await dbContext.SaveChangesAsync();

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/coreadmindata/index/testentities");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains(idGuid.ToString(), content);
            Assert.Contains(nameGuidString, content);
        }
    }
}
