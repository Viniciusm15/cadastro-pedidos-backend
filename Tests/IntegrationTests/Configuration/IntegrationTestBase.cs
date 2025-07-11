using Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Tests.IntegrationTests.Configuration
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        protected readonly HttpClient _client;
        protected readonly CustomWebApplicationFactory _factory;
        private ApplicationDbContext _dbContext;
        private readonly ILogger<IntegrationTestBase> _logger;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();
        }

        public async Task InitializeAsync()
        {
            await ClearDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            if (_dbContext != null)
            {
                await _dbContext.DisposeAsync();
            }
        }

        private async Task ClearDatabaseAsync()
        {
            _dbContext.ChangeTracker.Clear();

            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

                var tableNames = _dbContext.Model.GetEntityTypes()
                    .Select(t => t.GetTableName())
                    .Distinct()
                    .ToList();

                foreach (var tableName in tableNames)
                {
                    try
                    {
                        await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]");
                        _logger.LogDebug("Tabela {TableName} limpa com sucesso", tableName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao limpar tabela {TableName}", tableName);
                    }
                }

                await _dbContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a limpeza do banco de dados");
                throw;
            }
        }

        protected internal static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}. Response: {content}");
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result is null ? throw new JsonException($"Deserialization returned null for content: {content}") : result;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize: {content}", ex);
            }
        }
    }
}
