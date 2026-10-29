using System;
using DotNetEd.CoreAdmin;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        static void ConfigureTestServices(IServiceCollection services)
        {
            // CRUD tests explicitly opt into the Development-only bypass.
            services.AddCoreAdmin(new CoreAdminOptions
            {
                AllowUnauthenticatedAccessInDevelopment = true
            });
        }

        private static readonly Regex AntiforgeryTokenRegex = new Regex(
            "<input\\b(?=[^>]*\\bname\\s*=\\s*[\"']__RequestVerificationToken[\"'])(?=[^>]*\\bvalue\\s*=\\s*[\"']([^\"']+)[\"'])[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static async Task<HttpResponseMessage> PostWithAntiforgeryTokenAsync(
            HttpClient client,
            string formUrl,
            string postUrl,
            IDictionary<string, string> formData)
        {
            using var formResponse = await client.GetAsync(formUrl);
            formResponse.EnsureSuccessStatusCode();

            var html = await formResponse.Content.ReadAsStringAsync();
            var tokenMatch = AntiforgeryTokenRegex.Match(html);
            Assert.True(tokenMatch.Success, $"CSRF token was not found in form: {formUrl}");

            var data = new Dictionary<string, string>(formData)
            {
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(tokenMatch.Groups[1].Value)
            };

            // WebApplicationFactory's client keeps the antiforgery cookie between GET and POST.
            return await client.PostAsync(postUrl, new FormUrlEncodedContent(data));
        }

        [Fact]
        public async Task CreateWithoutAntiforgeryToken_IsRejectedAndDoesNotCreateEntity()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var name = Guid.NewGuid().ToString();
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var data = new Dictionary<string, string> { { "Name", name }, { "Id", Guid.NewGuid().ToString() } };
            var response = await client.PostAsync(
                "/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities",
                new FormUrlEncodedContent(data));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(await dbContext.TestEntities.AnyAsync(entity => entity.Name == name));
        }

        [Fact]
        public async Task EditWithoutAntiforgeryToken_IsRejectedAndDoesNotUpdateEntity()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var entity = new TestApp.Entities.TestEntity { Id = Guid.NewGuid(), Name = "Original" };
            dbContext.TestEntities.Add(entity);
            await dbContext.SaveChangesAsync();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var data = new Dictionary<string, string>
            {
                { "Name", "Changed" },
                { "Id", entity.Id.ToString() },
                { "dbSetName", "TestEntities" }
            };
            var response = await client.PostAsync(
                $"/coreadmindata/EditEntityPost/{entity.Id}?dbSetName=TestEntities",
                new FormUrlEncodedContent(data));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await dbContext.Entry(entity).ReloadAsync();
            Assert.Equal("Original", entity.Name);
        }

        [Fact]
        public async Task DeleteWithoutAntiforgeryToken_IsRejectedAndDoesNotDeleteEntity()
        {
            var dbContext = _factory.Services.GetService<IntegrationTestDbContext>();
            var entity = new TestApp.Entities.TestEntity { Id = Guid.NewGuid(), Name = "Must remain" };
            dbContext.TestEntities.Add(entity);
            await dbContext.SaveChangesAsync();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(ConfigureTestServices);
            }).CreateClient();

            var data = new Dictionary<string, string>
            {
                { "Id", entity.Id.ToString() },
                { "DbSetName", "testentities" }
            };
            var response = await client.PostAsync(
                "/coreadmindata/DeleteEntityPost",
                new FormUrlEncodedContent(data));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(await dbContext.TestEntities.AnyAsync(item => item.Id == entity.Id));
        }

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

            var response = await PostWithAntiforgeryTokenAsync(
                client,
                $"/coreadmindata/DeleteEntity/{idGuid}?dbSetName=testentities",
                "/coreadmindata/DeleteEntityPost",
                data);
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

            var response = await PostWithAntiforgeryTokenAsync(
                client,
                "/coreadmindata/Create/testentities",
                "/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities",
                data);
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

            var response = await PostWithAntiforgeryTokenAsync(
                client,
                "/coreadmindata/Create/testchildentities",
                "/coreadmindata/CreateEntityPost/testchildentities?dbSetName=testchildentities",
                data);
            response.EnsureSuccessStatusCode();

            Assert.True(await dbContext.TestChildEntities.AnyAsync(test => test.Name == nameGuidString));

            var parentId = Guid.NewGuid();

            data = new Dictionary<string, string>() { { "ParentId", parentId.ToString() }, { "ChildId", idGuid.ToString() }, { "dbSetName", "testparententities" } };

            response = await PostWithAntiforgeryTokenAsync(
                client,
                "/coreadmindata/Create/testparententities",
                "/coreadmindata/CreateEntityPost/testparententities",
                data);
            
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

            var response = await PostWithAntiforgeryTokenAsync(
                client,
                "/coreadmindata/Create/testentities",
                "/coreadmindata/CreateEntityPost/testentities?dbSetName=testentities",
                data);
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

            var response = await PostWithAntiforgeryTokenAsync(
                client,
                "/coreadmindata/Create/TestAutogeneratedKeyEntities",
                "/coreadmindata/CreateEntityPost/TestAutogeneratedKeyEntities?dbSetName=TestAutogeneratedKeyEntities",
                data);
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

            var response = await PostWithAntiforgeryTokenAsync(
                client,
                $"/coreadmindata/EditEntity/{idGuid}?dbSetName=TestEntities",
                $"/coreadmindata/editentityPost/{idGuid}?dbSetName=TestEntities",
                data);
            response.EnsureSuccessStatusCode();

            // check to see if the item is updated from DB context
            var foundEntity = dbContext.TestEntities.First(e => e.Id == idGuid);
            dbContext.Entry(foundEntity).Reload();
            Assert.True(dbContext.TestEntities.First(e => e.Id == idGuid).Name == updatedNameGuid);

        }
    }
}