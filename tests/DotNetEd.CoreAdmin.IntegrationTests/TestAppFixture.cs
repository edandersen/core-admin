using System;
using System.Threading.Tasks;

using DotNetEd.CoreAdmin.IntegrationTests.TestApp;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;


namespace DotNetEd.CoreAdmin.IntegrationTests;

public sealed class TestAppFixture : IAsyncLifetime
{
    public IntegrationTestsWebHostFactory Factory { get; } = new();

    public Task InitializeAsync()
    {
        // Create a scope to obtain a reference to the database
        // context (ApplicationDbContext).
        using (var scope = Factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<IntegrationTestDbContext>();
            var logger = scopedServices
                .GetRequiredService<ILogger<IntegrationTestsWebHostFactory>>();

            // Ensure the database is created.
            db.Database.EnsureCreated();

            try
            {
                // Seed the database with test data.
                // Utilities.InitializeDbForTests(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                    "database with test messages. Error: {Message}", ex.Message);
            }
        }

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}
