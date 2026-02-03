using Microsoft.Extensions.Logging;

namespace Common;

/// <summary>
/// Extension methods for structured logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs application startup information.
    /// </summary>
    public static void LogApplicationStartup(this ILogger logger, string applicationName, string version)
    {
        logger.LogInformation(
            "Starting {ApplicationName} v{Version}",
            applicationName,
            version);
    }

    /// <summary>
    /// Logs Azure resource connection information (without secrets).
    /// </summary>
    public static void LogAzureResourceConnection(this ILogger logger, string resourceType, string resourceName)
    {
        logger.LogInformation(
            "Connecting to {ResourceType}: {ResourceName}",
            resourceType,
            resourceName);
    }
}
