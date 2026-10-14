using System.Collections.Concurrent;
using Testcontainers.PostgreSql;

namespace AutoLuki.IntegrationTests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private readonly ConcurrentDictionary<string, ApiFactory> _factories = new();

    public string ConnectionString(string database) =>
        new Npgsql.NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = database }.ConnectionString;

    public ApiFactory GetFactory(string database, IDictionary<string, string?>? extraSettings = null) =>
        _factories.GetOrAdd(database, db => new ApiFactory(this, db, extraSettings));

    public Task InitializeAsync() => _container.StartAsync();

    public async Task DisposeAsync()
    {
        foreach (var factory in _factories.Values)
        {
            await factory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Integration";
}
