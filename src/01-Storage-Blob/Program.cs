using Azure.Storage.Blobs;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StorageBlob;

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

                // Configure Storage options
                services.Configure<StorageOptions>(
                    configuration.GetSection(StorageOptions.SectionName));

                var storageOptions = configuration.GetSection(StorageOptions.SectionName)
                    .Get<StorageOptions>() ?? new StorageOptions();

                if (string.IsNullOrWhiteSpace(storageOptions.AccountName))
                {
                    throw new InvalidOperationException(
                        "Storage:AccountName must be configured. " +
                        "Set it in appsettings.json or via environment variable STORAGE__ACCOUNTNAME");
                }

                // Create BlobServiceClient with DefaultAzureCredential
                var credential = AzureCredentialHelper.CreateCredential();
                var blobServiceUri = new Uri($"https://{storageOptions.AccountName}.blob.core.windows.net");
                var blobServiceClient = new BlobServiceClient(blobServiceUri, credential);

                services.AddSingleton(blobServiceClient);
                services.AddScoped<BlobStorageService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogApplicationStartup("01-Storage-Blob", "1.0.0");

        try
        {
            var blobService = host.Services.GetRequiredService<BlobStorageService>();
            var storageOptions = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>().Value;

            logger.LogAzureResourceConnection("Azure Blob Storage", storageOptions.AccountName);

            // Ensure container exists
            logger.LogInformation("=== Ensuring Container Exists ===");
            await blobService.EnsureContainerExistsAsync();

            // Upload a blob
            const string testBlobName = "test-file.txt";
            const string testContent = "Hello from Azure Blob Storage!\nThis is a test file.";

            logger.LogInformation("\n=== Uploading Blob ===");
            var blobUri = await blobService.UploadBlobAsync(
                testBlobName,
                testContent,
                contentType: "text/plain",
                metadata: new Dictionary<string, string>
                {
                    { "Author", "Azure Integration Lab" },
                    { "Created", DateTime.UtcNow.ToString("O") }
                });

            logger.LogInformation("Blob uploaded to: {Uri}", blobUri);

            // Set additional metadata
            logger.LogInformation("\n=== Setting Blob Metadata ===");
            await blobService.SetBlobMetadataAsync(
                testBlobName,
                new Dictionary<string, string>
                {
                    { "Updated", DateTime.UtcNow.ToString("O") },
                    { "Version", "1.0" }
                });

            // Download the blob
            logger.LogInformation("\n=== Downloading Blob ===");
            var downloadedContent = await blobService.DownloadBlobAsync(testBlobName);
            logger.LogInformation("Downloaded content:\n{Content}", downloadedContent);

            // List all blobs
            logger.LogInformation("\n=== Listing All Blobs ===");
            var blobs = await blobService.ListBlobsAsync();
            foreach (var blob in blobs)
            {
                logger.LogInformation(
                    "  - {Name} ({Size} bytes, ContentType: {ContentType})",
                    blob.Name,
                    blob.Properties.ContentLength ?? 0,
                    blob.Properties.ContentType ?? "unknown");
            }

            // Generate SAS URL (note: requires Storage Account Key Operator role or account key)
            logger.LogInformation("\n=== Generating SAS URL ===");
            try
            {
                var sasUrl = await blobService.GenerateSasUrlAsync(testBlobName, TimeSpan.FromHours(1));
                logger.LogInformation("SAS URL (valid for 1 hour): {SasUrl}", sasUrl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not generate SAS URL. This requires Storage Account Key Operator role or account key access.");
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
