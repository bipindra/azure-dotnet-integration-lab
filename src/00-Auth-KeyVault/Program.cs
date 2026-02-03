using Azure.Security.KeyVault.Secrets;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthKeyVault;

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

                // Configure Key Vault options
                services.Configure<KeyVaultOptions>(
                    configuration.GetSection(KeyVaultOptions.SectionName));

                var keyVaultOptions = configuration.GetSection(KeyVaultOptions.SectionName)
                    .Get<KeyVaultOptions>() ?? new KeyVaultOptions();

                if (string.IsNullOrWhiteSpace(keyVaultOptions.VaultUri))
                {
                    throw new InvalidOperationException(
                        "KeyVault:VaultUri must be configured. " +
                        "Set it in appsettings.json or via environment variable KEYVAULT__VAULTURI");
                }

                // Create SecretClient with DefaultAzureCredential
                var credential = AzureCredentialHelper.CreateCredential();
                var secretClient = new SecretClient(new Uri(keyVaultOptions.VaultUri), credential);

                services.AddSingleton(secretClient);
                services.AddScoped<KeyVaultService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogApplicationStartup("00-Auth-KeyVault", "1.0.0");

        try
        {
            var keyVaultService = host.Services.GetRequiredService<KeyVaultService>();
            var keyVaultOptions = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<KeyVaultOptions>>().Value;

            logger.LogAzureResourceConnection("Azure Key Vault", keyVaultOptions.VaultUri);

            // Demo: Set a test secret
            const string testSecretName = "TestSecret";
            const string testSecretValue = "Hello from Azure Key Vault!";

            logger.LogInformation("=== Setting Test Secret ===");
            await keyVaultService.SetSecretAsync(testSecretName, testSecretValue);

            // Demo: Retrieve the secret
            logger.LogInformation("\n=== Retrieving Test Secret ===");
            var retrievedValue = await keyVaultService.GetSecretAsync(testSecretName);
            logger.LogInformation("Retrieved value: {Value}", retrievedValue);

            // Demo: List all secrets
            logger.LogInformation("\n=== Listing All Secrets ===");
            var secrets = await keyVaultService.ListSecretsAsync();
            foreach (var secretName in secrets)
            {
                logger.LogInformation("  - {SecretName}", secretName);
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
