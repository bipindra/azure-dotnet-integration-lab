using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace StorageBlob;

/// <summary>
/// Service for interacting with Azure Blob Storage.
/// </summary>
public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly StorageOptions _options;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<StorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Ensures the container exists, creating it if necessary.
    /// </summary>
    public async Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            
            var exists = await containerClient.ExistsAsync(cancellationToken);
            if (!exists.Value)
            {
                _logger.LogInformation("Creating container '{ContainerName}'", _options.ContainerName);
                await containerClient.CreateIfNotExistsAsync(
                    PublicAccessType.None,
                    cancellationToken: cancellationToken);
                _logger.LogInformation("Container '{ContainerName}' created", _options.ContainerName);
            }
            else
            {
                _logger.LogInformation("Container '{ContainerName}' already exists", _options.ContainerName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure container exists");
            throw;
        }
    }

    /// <summary>
    /// Uploads a blob with text content.
    /// </summary>
    public async Task<string> UploadBlobAsync(
        string blobName,
        string content,
        string? contentType = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uploading blob '{BlobName}'", blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders();
            if (!string.IsNullOrEmpty(contentType))
            {
                blobHttpHeaders.ContentType = contentType;
            }

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var contentBytes = Encoding.UTF8.GetBytes(content);
            using var stream = new MemoryStream(contentBytes);

            await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

            _logger.LogInformation(
                "Successfully uploaded blob '{BlobName}' ({Size} bytes)",
                blobName,
                contentBytes.Length);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload blob '{BlobName}'", blobName);
            throw;
        }
    }

    /// <summary>
    /// Downloads a blob and returns its content as a string.
    /// </summary>
    public async Task<string> DownloadBlobAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Downloading blob '{BlobName}'", blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadContentAsync(cancellationToken);
            var content = response.Value.Content.ToString();

            _logger.LogInformation(
                "Successfully downloaded blob '{BlobName}' ({Size} bytes)",
                blobName,
                response.Value.ContentLength);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob '{BlobName}'", blobName);
            throw;
        }
    }

    /// <summary>
    /// Lists all blobs in the container.
    /// </summary>
    public async Task<List<BlobItem>> ListBlobsAsync(CancellationToken cancellationToken = default)
    {
        var blobs = new List<BlobItem>();

        try
        {
            _logger.LogInformation("Listing blobs in container '{ContainerName}'", _options.ContainerName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

            await foreach (var blobItem in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                blobs.Add(blobItem);
            }

            _logger.LogInformation("Found {Count} blobs", blobs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list blobs");
            throw;
        }

        return blobs;
    }

    /// <summary>
    /// Sets metadata on a blob.
    /// </summary>
    public async Task SetBlobMetadataAsync(
        string blobName,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting metadata on blob '{BlobName}'", blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully set metadata on blob '{BlobName}'", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set metadata on blob '{BlobName}'", blobName);
            throw;
        }
    }

    /// <summary>
    /// Generates a SAS URL for a blob with read permissions.
    /// </summary>
    public async Task<string> GenerateSasUrlAsync(
        string blobName,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating SAS URL for blob '{BlobName}'", blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new FileNotFoundException($"Blob '{blobName}' does not exist");
            }

            // Generate SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _options.ContainerName,
                BlobName = blobName,
                Resource = "b", // blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiryDuration)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Note: In production, you'd use a user delegation key or account key
            // For this demo, we'll use the account key approach (requires Storage Account Key Operator role)
            // For managed identity, consider using BlobServiceClient.GetUserDelegationKeyAsync
            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation(
                "Generated SAS URL for blob '{BlobName}' (expires in {Duration})",
                blobName,
                expiryDuration);

            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SAS URL for blob '{BlobName}'", blobName);
            throw;
        }
    }
}
