using Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbCosmos;

/// <summary>
/// Service for interacting with Azure Cosmos DB.
/// </summary>
public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<CosmosDbService> _logger;
    private readonly CosmosDbOptions _options;
    private Container? _container;

    public CosmosDbService(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Ensures the database and container exist, creating them if necessary.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing Cosmos DB database and container");

            // Create database if it doesn't exist
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                _options.DatabaseName,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Database '{DatabaseName}' ready",
                databaseResponse.Database.Id);

            // Create container if it doesn't exist
            var containerProperties = new ContainerProperties
            {
                Id = _options.ContainerName,
                PartitionKeyPath = "/partitionKey"
            };

            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                containerProperties,
                throughput: 400, // Minimum throughput (400 RU/s)
                cancellationToken: cancellationToken);

            _container = containerResponse.Container;

            _logger.LogInformation(
                "Container '{ContainerName}' ready (partition key: /partitionKey)",
                _container.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Cosmos DB");
            throw;
        }
    }

    /// <summary>
    /// Creates a new item in the container.
    /// </summary>
    public async Task<Item> CreateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container not initialized. Call InitializeAsync first.");
        }

        try
        {
            _logger.LogInformation("Creating item '{ItemId}' in partition '{PartitionKey}'", item.Id, item.PartitionKey);

            var response = await _container.CreateItemAsync(
                item,
                new PartitionKey(item.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully created item '{ItemId}' (RU cost: {RequestCharge})",
                item.Id,
                response.RequestCharge);

            item.ETag = response.ETag;
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create item '{ItemId}'", item.Id);
            throw;
        }
    }

    /// <summary>
    /// Reads an item by ID and partition key.
    /// </summary>
    public async Task<Item?> ReadItemAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container not initialized. Call InitializeAsync first.");
        }

        try
        {
            _logger.LogInformation("Reading item '{ItemId}' from partition '{PartitionKey}'", id, partitionKey);

            var response = await _container.ReadItemAsync<Item>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully read item '{ItemId}' (RU cost: {RequestCharge})",
                id,
                response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Item '{ItemId}' not found", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read item '{ItemId}'", id);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing item.
    /// </summary>
    public async Task<Item> UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container not initialized. Call InitializeAsync first.");
        }

        try
        {
            _logger.LogInformation("Updating item '{ItemId}' in partition '{PartitionKey}'", item.Id, item.PartitionKey);

            var response = await _container.ReplaceItemAsync(
                item,
                item.Id,
                new PartitionKey(item.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully updated item '{ItemId}' (RU cost: {RequestCharge})",
                item.Id,
                response.RequestCharge);

            item.ETag = response.ETag;
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update item '{ItemId}'", item.Id);
            throw;
        }
    }

    /// <summary>
    /// Deletes an item by ID and partition key.
    /// </summary>
    public async Task DeleteItemAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container not initialized. Call InitializeAsync first.");
        }

        try
        {
            _logger.LogInformation("Deleting item '{ItemId}' from partition '{PartitionKey}'", id, partitionKey);

            var response = await _container.DeleteItemAsync<Item>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully deleted item '{ItemId}' (RU cost: {RequestCharge})",
                id,
                response.RequestCharge);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Item '{ItemId}' not found (already deleted?)", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete item '{ItemId}'", id);
            throw;
        }
    }

    /// <summary>
    /// Queries items by partition key.
    /// </summary>
    public async Task<List<Item>> QueryItemsAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container not initialized. Call InitializeAsync first.");
        }

        var items = new List<Item>();

        try
        {
            _logger.LogInformation("Querying items in partition '{PartitionKey}'", partitionKey);

            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey")
                .WithParameter("@partitionKey", partitionKey);

            var queryIterator = _container.GetItemQueryIterator<Item>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });

            double totalRequestCharge = 0;

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync(cancellationToken);
                totalRequestCharge += response.RequestCharge;

                foreach (var item in response)
                {
                    items.Add(item);
                }
            }

            _logger.LogInformation(
                "Query returned {Count} items (total RU cost: {RequestCharge})",
                items.Count,
                totalRequestCharge);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query items");
            throw;
        }
    }
}
