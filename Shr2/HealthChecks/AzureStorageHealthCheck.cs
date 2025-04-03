using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Shr2.Interfaces;

namespace Shr2.HealthChecks
{
    public class AzureStorageHealthCheck : IHealthCheck
    {
        private readonly IConfig _config;
        private readonly ILogger<AzureStorageHealthCheck>? _logger;

        public AzureStorageHealthCheck(
            IConfig config,
            ILogger<AzureStorageHealthCheck>? logger = null)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Checking Azure Storage health");
                
                var connectionString = _config.GetConfig().StorageConnectionString;
                var tableServiceClient = new TableServiceClient(connectionString);
                
                // Try to get service properties to verify connectivity
                var properties = await tableServiceClient.GetPropertiesAsync(cancellationToken);
                
                _logger?.LogInformation("Azure Storage health check passed");
                return HealthCheckResult.Healthy("Azure Storage is accessible");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Azure Storage health check failed");
                return HealthCheckResult.Unhealthy("Azure Storage is not accessible", ex);
            }
        }
    }
}
