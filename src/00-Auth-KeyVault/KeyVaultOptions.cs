namespace AuthKeyVault;

/// <summary>
/// Configuration options for Azure Key Vault.
/// </summary>
public class KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    /// <summary>
    /// The URI of the Key Vault (e.g., https://kv-ailab-xxxxx.vault.azure.net/).
    /// </summary>
    public string VaultUri { get; set; } = string.Empty;
}
