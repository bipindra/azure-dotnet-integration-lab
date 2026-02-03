namespace DbCosmos;

/// <summary>
/// Configuration options for Azure Cosmos DB.
/// </summary>
public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";

    /// <summary>
    /// The Cosmos DB account endpoint URI.
    /// </summary>
    public string AccountEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// The database name.
    /// </summary>
    public string DatabaseName { get; set; } = "demo-db";

    /// <summary>
    /// The container name.
    /// </summary>
    public string ContainerName { get; set; } = "items";
}
