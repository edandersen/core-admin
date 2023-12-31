using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using DotNetEd.CoreAdmin.IntegrationTests.TestApp;


namespace DotNetEd.CoreAdmin.IntegrationTests
{
    public class CrudTests : IClassFixture<TestAppFixture>
    {
        private readonly TestAppFixture _fixture;

        public CrudTests(TestAppFixture fixture)
        {
            _fixture = fixture;
        }

         static void ConfigureTestServices(IServiceCollection services) { }

        [Fact]
        public async Task DeleteHappyPath()
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
            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "id", idGuid.ToString() }, { "dbSetName", "testentities" } };

            var response = await client.PostAsync("/coreadmindata/DeleteEntityPost", new FormUrlEncodedContent(data));

            // Assert
            response.EnsureSuccessStatusCode();

            // check to see if the item is deleted from DB context
            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                Assert.False(await dbContext.TestEntities.AnyAsync(test => test.Id == idGuid));
            }
        }

        [Fact]
        public async Task CreateHappyPath()
        {
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var nameGuidString = Guid.NewGuid().ToString();

            // Act
            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameGuidString }, { "Id", nameGuidString } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities", new FormUrlEncodedContent(data));

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                Assert.True(await dbContext.TestEntities.AnyAsync(test => test.Name == nameGuidString));
            }
        }

        [Fact]
        public async Task Create_ChildEntityWithNonNullableFK_HappyPath()
        {
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();


            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();

            // Act
            // Do the post to create the child item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameGuidString }, { "Id", nameGuidString } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/testchildentities?dbSetName=testchildentities", new FormUrlEncodedContent(data));

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                Assert.True(await dbContext.TestChildEntities.AnyAsync(test => test.Name == nameGuidString));
            }

            var parentId = Guid.NewGuid();

            data = new Dictionary<string, string>() { { "ParentId", parentId.ToString() }, { "ChildId", idGuid.ToString() }, { "dbSetName", "testparententities" } };

            response = await client.PostAsync("/coreadmindata/CreateEntityPost/testparententities", new FormUrlEncodedContent(data));

            response.EnsureSuccessStatusCode();

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                Assert.True(await dbContext.TestParentEntities.AnyAsync(test => test.ChildId == idGuid));
            }
        }

        [Fact]
        public async Task CreateTestEntityWithValidationError_TooLongName()
        {
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();


            var idGuid = Guid.NewGuid();
            var nameString = new string('*', 5000); // Name should have MaxLength(100)

            // Act
            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameString }, { "Id", idGuid.ToString() } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities", new FormUrlEncodedContent(data));

            // Assert
            response.EnsureSuccessStatusCode();

            // make sure it has not written to DB
            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                Assert.False(await dbContext.TestEntities.AnyAsync(test => test.Name == nameString));
            }
        }

        [Fact]
        public async Task CreateHappyPath_WithAutogeneratedKey()
        {
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var nameGuidString = Guid.NewGuid().ToString();

            // Act
            // Do the post to delete the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", nameGuidString } };

            var response = await client.PostAsync("/coreadmindata/CreateEntityPost/TestAutogeneratedKeyEntities?dbSetName=TestAutogeneratedKeyEntities", new FormUrlEncodedContent(data));

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                Assert.True(await dbContext.TestAutogeneratedKeyEntities.AnyAsync(test => test.Name == nameGuidString));
            }
        }

        [Fact]
        public async Task UpdateHappyPath()
        {
            // Arrange
            var client = _fixture.Factory.WithWebHostBuilder(builder => {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var idGuid = Guid.NewGuid();
            var nameGuidString = Guid.NewGuid().ToString();
            var updatedNameGuid = Guid.NewGuid().ToString();

            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                dbContext.TestEntities.Add(new TestApp.Entities.TestEntity() { Id = idGuid, Name = nameGuidString});
                await dbContext.SaveChangesAsync();
            }

            // Act
            // Do the post to update the item
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            var data = new Dictionary<string, string>() { { "Name", updatedNameGuid }, { "Id", idGuid.ToString() } };

            var response = await client.PostAsync("/coreadmindata/editentityPost/" + idGuid.ToString() + "?dbSetName=TestEntities", new FormUrlEncodedContent(data));

            // Assert
            response.EnsureSuccessStatusCode();

            // check to see if the item is updated from DB context
            using (var scope = _fixture.Factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationTestDbContext>();
                var foundEntity = dbContext.TestEntities.First(e => e.Id == idGuid);
                dbContext.Entry(foundEntity).Reload();
                Assert.True(dbContext.TestEntities.First(e => e.Id == idGuid).Name == updatedNameGuid);
            }
        }
    }
}
