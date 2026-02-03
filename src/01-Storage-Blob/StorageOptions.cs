namespace StorageBlob;

/// <summary>
/// Configuration options for Azure Storage.
/// </summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// The storage account name (e.g., stailabxxxxx).
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// The container name for blob operations.
    /// </summary>
    public string ContainerName { get; set; } = "demo-container";
}
