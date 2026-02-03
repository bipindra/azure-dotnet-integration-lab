using Azure.Security.KeyVault.Secrets;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthKeyVault;

/// <summary>
/// Service for interacting with Azure Key Vault.
/// </summary>
public class KeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;
    private readonly KeyVaultOptions _options;

    public KeyVaultService(
        SecretClient secretClient,
        IOptions<KeyVaultOptions> options,
        ILogger<KeyVaultService> logger)
    {
        _secretClient = secretClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Retrieves a secret from Key Vault.
    /// </summary>
    public async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving secret '{SecretName}' from Key Vault", secretName);

            var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Successfully retrieved secret '{SecretName}' (version: {Version})",
                secretName,
                secret.Value.Properties.Version);

            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret '{SecretName}'", secretName);
            throw;
        }
    }

    /// <summary>
    /// Sets a secret in Key Vault.
    /// </summary>
    public async Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting secret '{SecretName}' in Key Vault", secretName);

            await _secretClient.SetSecretAsync(secretName, secretValue, cancellationToken);

            _logger.LogInformation("Successfully set secret '{SecretName}'", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret '{SecretName}'", secretName);
            throw;
        }
    }

    /// <summary>
    /// Lists all secret names in the vault.
    /// </summary>
    public async Task<List<string>> ListSecretsAsync(CancellationToken cancellationToken = default)
    {
        var secretNames = new List<string>();

        try
        {
            _logger.LogInformation("Listing secrets in Key Vault");

            await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync(cancellationToken))
            {
                secretNames.Add(secretProperties.Name);
            }

            _logger.LogInformation("Found {Count} secrets in Key Vault", secretNames.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list secrets");
            throw;
        }

        return secretNames;
    }
}
