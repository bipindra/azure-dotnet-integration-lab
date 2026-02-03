using Newtonsoft.Json;

namespace DbCosmos;

/// <summary>
/// Sample item entity for Cosmos DB.
/// </summary>
public class Item
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("_etag")]
    public string? ETag { get; set; }
}
