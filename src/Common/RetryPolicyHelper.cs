using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Common;

/// <summary>
/// Helper for creating retry policies with exponential backoff.
/// </summary>
public static class RetryPolicyHelper
{
    /// <summary>
    /// Creates an async retry policy with exponential backoff.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <param name="logger">Optional logger for retry events</param>
    /// <returns>Async retry policy</returns>
    public static AsyncRetryPolicy CreateAsyncRetryPolicy(int maxRetries = 3, ILogger? logger = null)
    {
        return Policy
            .Handle<Exception>(ex =>
            {
                // Log transient exceptions
                logger?.LogWarning(ex, "Transient error occurred, will retry: {Message}", ex.Message);
                return true; // Retry on any exception for simplicity
            })
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogInformation(
                        "Retry attempt {RetryCount} after {Delay} seconds",
                        retryCount,
                        timespan.TotalSeconds);
                });
    }

    /// <summary>
    /// Creates a sync retry policy with exponential backoff.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <param name="logger">Optional logger for retry events</param>
    /// <returns>Sync retry policy</returns>
    public static RetryPolicy CreateRetryPolicy(int maxRetries = 3, ILogger? logger = null)
    {
        return Policy
            .Handle<Exception>(ex =>
            {
                logger?.LogWarning(ex, "Transient error occurred, will retry: {Message}", ex.Message);
                return true;
            })
            .WaitAndRetry(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogInformation(
                        "Retry attempt {RetryCount} after {Delay} seconds",
                        retryCount,
                        timespan.TotalSeconds);
                });
    }
}
