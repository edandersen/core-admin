using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetEd.AutoAdmin.IntegrationTests.TestApp;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetEd.AutoAdmin.IntegrationTests
{
    public class CrudTests : IClassFixture<IntegrationTestsWebHostFactory<IntegrationTestStartup>>
    {
        private readonly IntegrationTestsWebHostFactory<IntegrationTestStartup> _factory;

        public CrudTests(IntegrationTestsWebHostFactory<IntegrationTestStartup> factory)
        {
            _factory = factory;
        }

         static void ConfigureTestServices(IServiceCollection services) { }

        [Fact]
        public async Task DeleteHappyPath()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();
            dbContext.TestEntities.Add(new TestApp.Entities.TestEntity() { Id = idGuid, Name = nameGuidString});
            await dbContext.SaveChangesAsync();

            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
                    builder.ConfigureTestServices(ConfigureTestServices)).CreateClient();

            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "id", idGuid.ToString() }, { "dbSetName", "testentities" } };

            var response = await client.PostAsync("/autoadmin/data/deleteentity", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();

            // check to see if the item is deleted from DB context
            Assert.False(await dbContext.TestEntities.AnyAsync(test => test.Id == idGuid));

        }
    }
}