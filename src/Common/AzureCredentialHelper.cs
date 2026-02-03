using Azure.Core;
using Azure.Identity;

namespace Common;

/// <summary>
/// Helper for creating Azure credentials using DefaultAzureCredential.
/// Supports local development (Azure CLI, Visual Studio, etc.) and managed identity when deployed.
/// </summary>
public static class AzureCredentialHelper
{
    /// <summary>
    /// Creates a DefaultAzureCredential instance with appropriate options.
    /// </summary>
    public static TokenCredential CreateCredential()
    {
        var options = new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = false,
            ExcludeManagedIdentityCredential = false,
            ExcludeSharedTokenCacheCredential = false,
            ExcludeVisualStudioCredential = false,
            ExcludeVisualStudioCodeCredential = false,
            ExcludeAzureCliCredential = false,
            ExcludeAzurePowerShellCredential = false,
            ExcludeInteractiveBrowserCredential = false
        };

        return new DefaultAzureCredential(options);
    }
}
