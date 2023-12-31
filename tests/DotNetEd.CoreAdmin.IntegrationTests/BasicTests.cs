using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using DotNetEd.CoreAdmin.IntegrationTests.TestApp;


namespace DotNetEd.CoreAdmin.IntegrationTests
{
    public class BasicTests : IClassFixture<TestAppFixture>
    {
        private readonly TestAppFixture _fixture;

        public BasicTests(TestAppFixture fixture)
        {
            _fixture = fixture;
        }

        static void ConfigureTestServices(IServiceCollection services) { }

        [Fact]
        public async Task ShowsTestEntitiesOnScreenOnAdminScreen()
        {
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
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
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                dbContext.TestEntities.Add(new TestApp.Entities.TestEntity() { Id = idGuid, Name = nameGuidString });
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await client.GetAsync("/coreadmindata/index/testentities");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains(idGuid.ToString(), content);
            Assert.Contains(nameGuidString, content);
        }
    }
}
