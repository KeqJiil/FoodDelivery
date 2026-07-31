using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;

namespace FoodDelivery.IntegrationTest.Infrastructure;

public class DatabaseResetter(string connectionString)
{
    private static readonly string[] Schemas =
        ["ordering", "restaurants", "order_requests", "payments", "deliveries", "saga"];

    private Respawner _respawner = null!;
    
    public async Task InitializeAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = Schemas,
            TablesToIgnore = Schemas.Select(schema => new Table(schema, "__EFMigrationsHistory")).ToArray()
        });
    }

    public async Task ResetAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}