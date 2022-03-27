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
        public async Task Create_ChildEntityWithNonNullableFK_HappyPath()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Do the post to create the child item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameGuidString }, { "Id", nameGuidString } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/testchildentities?dbSetName=testchildentities", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();

            Assert.True(await dbContext.TestChildEntities.AnyAsync(test => test.Name == nameGuidString));

            var parentId = Guid.NewGuid();

            data = new Dictionary<string, string>() { { "ParentId", parentId.ToString() }, { "ChildId", idGuid.ToString() }, { "dbSetName", "testparententities" } };

            response = await client.PostAsync("/coreadmindata/CreateEntityPost/testparententities", new FormUrlEncodedContent(data));
            
            response.EnsureSuccessStatusCode();

            Assert.True(await dbContext.TestParentEntities.AnyAsync(test => test.ChildId == idGuid));

        }

        [Fact]
        public async Task CreateTestEntityWithValidationError_TooLongName()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var idGuid = Guid.NewGuid();
            var nameString = new string('*', 5000); // Name should have MaxLength(100)

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameString }, { "Id", idGuid.ToString() } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();

            // make sure it has not written to DB
            Assert.False(await dbContext.TestEntities.AnyAsync(test => test.Name == nameString));

        }

        [Fact]
        public async Task CreateHappyPath_WithAutogeneratedKey()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            // var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();

            // Arrange
            var client = _factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameGuidString } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/TestAutogeneratedKeyEntities?dbSetName=TestAutogeneratedKeyEntities", new FormUrlEncodedContent(data));
            response.EnsureSuccessStatusCode();


            Assert.True(await dbContext.TestAutogeneratedKeyEntities.AnyAsync(test => test.Name == nameGuidString));

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