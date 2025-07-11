using DotNet.Testcontainers.Builders;
using Infra.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Tests.IntegrationTests.Configuration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Pass123")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .WithAutoRemove(true)
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(_dbContainer.GetConnectionString());
                    options.EnableSensitiveDataLogging();
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }
    }
}
