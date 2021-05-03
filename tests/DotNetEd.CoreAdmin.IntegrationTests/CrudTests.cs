using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetEd.CoreAdmin.IntegrationTests.TestApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetEd.CoreAdmin.IntegrationTests
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
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "id", idGuid.ToString() }, { "dbSetName", "testentities" } };

            var response = await client.PostAsync("/coreadmindata/DeleteEntityPost", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();

            // check to see if the item is deleted from DB context
            Assert.False(await dbContext.TestEntities.AnyAsync(test => test.Id == idGuid));

        }

        [Fact]
        public async Task CreateHappyPath()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameGuidString }, { "Id", nameGuidString } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();

            
            Assert.True(await dbContext.TestEntities.AnyAsync(test => test.Name == nameGuidString));

        }
        
        [Fact]
        public async Task UpdateHappyPath()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();
            dbContext.TestEntities.Add(new TestApp.Entities.TestEntity() { Id = idGuid, Name = nameGuidString});
            await dbContext.SaveChangesAsync();

            var updatedNameGuid = Guid.NewGuid().ToString();

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Do the post to update the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", updatedNameGuid }, { "Id", idGuid.ToString() } };

            var response = await client.PostAsync("/coreadmindata/editentityPost/" + idGuid.ToString() + "?dbSetName=TestEntities", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();

            // check to see if the item is updated from DB context
            var foundEntity = dbContext.TestEntities.First(e => e.Id == idGuid);
            dbContext.Entry(foundEntity).Reload();
            Assert.True(dbContext.TestEntities.First(e => e.Id == idGuid).Name == updatedNameGuid);

        }
    }
}