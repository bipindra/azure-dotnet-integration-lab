using Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DbCosmos;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Configure Cosmos DB options
                services.Configure<CosmosDbOptions>(
                    configuration.GetSection(CosmosDbOptions.SectionName));

                var cosmosOptions = configuration.GetSection(CosmosDbOptions.SectionName)
                    .Get<CosmosDbOptions>() ?? new CosmosDbOptions();

                if (string.IsNullOrWhiteSpace(cosmosOptions.AccountEndpoint))
                {
                    throw new InvalidOperationException(
                        "CosmosDb:AccountEndpoint must be configured. " +
                        "Set it in appsettings.json or via environment variable COSMOSDB__ACCOUNTENDPOINT");
                }

                // Create CosmosClient with DefaultAzureCredential
                var credential = AzureCredentialHelper.CreateCredential();
                var cosmosClientOptions = new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Direct
                };

                var cosmosClient = new CosmosClient(
                    cosmosOptions.AccountEndpoint,
                    credential,
                    cosmosClientOptions);

                services.AddSingleton(cosmosClient);
                services.AddScoped<CosmosDbService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogApplicationStartup("02-Db-Cosmos", "1.0.0");

        try
        {
            var cosmosService = host.Services.GetRequiredService<CosmosDbService>();
            var cosmosOptions = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>().Value;

            logger.LogAzureResourceConnection("Azure Cosmos DB", cosmosOptions.AccountEndpoint);

            // Initialize database and container
            logger.LogInformation("=== Initializing Database and Container ===");
            await cosmosService.InitializeAsync();

            // Create items
            logger.LogInformation("\n=== Creating Items ===");
            const string partitionKey = "demo-partition";

            var item1 = new Item
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = partitionKey,
                Name = "Sample Item 1",
                Description = "This is the first sample item",
                CreatedAt = DateTime.UtcNow
            };

            var item2 = new Item
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = partitionKey,
                Name = "Sample Item 2",
                Description = "This is the second sample item",
                CreatedAt = DateTime.UtcNow
            };

            await cosmosService.CreateItemAsync(item1);
            await cosmosService.CreateItemAsync(item2);

            // Read an item
            logger.LogInformation("\n=== Reading Item ===");
            var readItem = await cosmosService.ReadItemAsync(item1.Id, partitionKey);
            if (readItem != null)
            {
                logger.LogInformation(
                    "Read item: {Name} - {Description} (Created: {CreatedAt})",
                    readItem.Name,
                    readItem.Description,
                    readItem.CreatedAt);
            }

            // Update an item
            logger.LogInformation("\n=== Updating Item ===");
            item1.Description = "Updated description";
            await cosmosService.UpdateItemAsync(item1);
            logger.LogInformation("Item updated successfully");

            // Query items
            logger.LogInformation("\n=== Querying Items ===");
            var items = await cosmosService.QueryItemsAsync(partitionKey);
            logger.LogInformation("Found {Count} items in partition '{PartitionKey}'", items.Count, partitionKey);
            foreach (var item in items)
            {
                logger.LogInformation("  - {Name}: {Description}", item.Name, item.Description);
            }

            // Delete an item
            logger.LogInformation("\n=== Deleting Item ===");
            await cosmosService.DeleteItemAsync(item2.Id, partitionKey);
            logger.LogInformation("Item deleted successfully");

            // Verify deletion
            logger.LogInformation("\n=== Verifying Deletion ===");
            var deletedItem = await cosmosService.ReadItemAsync(item2.Id, partitionKey);
            if (deletedItem == null)
            {
                logger.LogInformation("Item successfully deleted (not found)");
            }

            logger.LogInformation("\n=== Demo Complete ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application error occurred");
            Environment.ExitCode = 1;
        }
    }
}
