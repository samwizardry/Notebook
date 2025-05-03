using Microsoft.Extensions.Diagnostics.HealthChecks;

using RottenRest.Application.Data;

namespace RottenRest.Web.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    public const string Name = "Database";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory,
        ILogger<DatabaseHealthCheck> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database is unhealthy");
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}
