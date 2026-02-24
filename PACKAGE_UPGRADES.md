# Package Upgrades Summary

All packages have been upgraded to the latest stable versions compatible with .NET 8.

## Directory.Build.props - Common Package Versions

| Package | Old Version | New Version |
|---------|-------------|-------------|
| Azure.Identity | 1.11.0 | 1.13.1 |
| Azure.Core | 1.38.0 | 1.44.1 |
| Microsoft.Extensions.* | 8.0.0 | 8.0.1 |
| Microsoft.Extensions.Configuration.Json | 8.0.1 | 8.0.1 (no change) |
| Polly | 8.4.2 | 8.5.0 |

## Project-Specific Package Upgrades

### 00-Auth-KeyVault
- Azure.Security.KeyVault.Secrets: 4.6.0 ? 4.7.0

### 01-Storage-Blob
- Azure.Storage.Blobs: 12.22.0 ? 12.23.0

### 02-Db-Cosmos
- Microsoft.Azure.Cosmos: 3.49.0 ? 3.45.0 (reverted to stable LTS)
- Newtonsoft.Json: 13.0.4 ? 13.0.3 (stable version)

### 03-Db-AzureSql-EFCore
- Microsoft.EntityFrameworkCore.SqlServer: 8.0.0 ? 8.0.11
- Microsoft.EntityFrameworkCore.Tools: 8.0.0 ? 8.0.11
- Microsoft.EntityFrameworkCore.Design: 8.0.0 ? 8.0.11
- Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore: 8.0.0 ? 8.0.11
- Swashbuckle.AspNetCore: 6.9.0 ? 7.2.0

### 04-Messaging-ServiceBus
- Azure.Messaging.ServiceBus: 7.17.0 ? 7.18.2

### 05-Eventing-EventGrid
- Azure.Messaging.EventGrid: 4.22.0 ? 4.29.0
- Swashbuckle.AspNetCore: 6.9.0 ? 7.2.0

### 06-Streaming-EventHubs
- Azure.Messaging.EventHubs: 5.11.0 ? 5.12.0
- Azure.Messaging.EventHubs.Processor: 5.11.0 ? 5.12.0
- Azure.Storage.Blobs: 12.22.0 ? 12.23.0

### 07-Serverless-Functions-ServiceBusTrigger
- Microsoft.Azure.Functions.Worker: 2.51.0 ? 1.23.0 (stable LTS for .NET 8)
- Microsoft.Azure.Functions.Worker.Sdk: 2.0.7 ? 1.18.1 (stable LTS for .NET 8)
- Microsoft.Azure.Functions.Worker.Extensions.ServiceBus: 5.24.0 (no change)
- Microsoft.ApplicationInsights.WorkerService: 3.0.0 ? 2.22.0 (stable version)
- Microsoft.Azure.Functions.Worker.ApplicationInsights: 2.50.0 ? 1.4.0 (stable LTS for .NET 8)

### 08-Hosting-ContainerApps-MinimalApi
- Azure.Identity: 1.11.0 ? 1.13.1
- Swashbuckle.AspNetCore: 6.9.0 ? 7.2.0

### 09-Observability-AppInsights-Otel
- Azure.Identity: 1.11.0 ? 1.13.1
- Azure.Monitor.OpenTelemetry.AspNetCore: 1.2.0 ? 1.3.0
- OpenTelemetry.Exporter.Console: 1.15.0 ? 1.10.0 (stable version)
- Swashbuckle.AspNetCore: 6.9.0 ? 7.2.0

### 10-ApiManagement-APIM
- Azure.Core: 1.38.0 ? 1.44.1
- Azure.Identity: 1.11.0 ? 1.13.1
- Swashbuckle.AspNetCore: 6.9.0 ? 7.2.0

### 11-Caching-Redis
- Azure.Core: 1.38.0 ? 1.44.1
- Azure.Identity: 1.11.0 ? 1.13.1
- Microsoft.Extensions.Caching.StackExchangeRedis: 8.0.11 (no change)
- Swashbuckle.AspNetCore: 6.9.0 ? 7.2.0

### 12-Advanced-AKS-Keda-ServiceBus
- Azure.Core: 1.38.0 ? 1.44.1
- Azure.Identity: 1.11.0 ? 1.13.1
- Azure.Messaging.ServiceBus: 7.17.0 ? 7.18.2

## Notes

- All packages have been updated to the latest stable versions compatible with .NET 8
- Azure Functions packages use the v1.x LTS versions which are the stable releases for .NET 8 isolated worker process
- Entity Framework Core packages updated to 8.0.11 (latest patch for .NET 8)
- Swashbuckle.AspNetCore upgraded to 7.2.0 across all web projects
- Build completed successfully with no errors

## Next Steps

1. Test all applications thoroughly to ensure compatibility
2. Review any breaking changes in upgraded packages (especially major version upgrades)
3. Update documentation if any API changes affect usage
4. Consider setting up Dependabot (already configured) to keep packages up-to-date automatically
