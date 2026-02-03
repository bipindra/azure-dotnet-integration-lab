namespace DbAzureSqlEFCore;

/// <summary>
/// Configuration options for Azure SQL Database.
/// </summary>
public class AzureSqlOptions
{
    public const string SectionName = "AzureSql";

    /// <summary>
    /// The Azure SQL server name (e.g., sql-ailab-xxxxx.database.windows.net).
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// The database name.
    /// </summary>
    public string DatabaseName { get; set; } = "demo-db";
}
