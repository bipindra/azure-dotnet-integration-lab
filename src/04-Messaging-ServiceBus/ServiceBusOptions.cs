namespace MessagingServiceBus;

/// <summary>
/// Configuration options for Azure Service Bus.
/// </summary>
public class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    /// <summary>
    /// The Service Bus namespace (e.g., sb-ailab-xxxxx.servicebus.windows.net).
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// The queue name.
    /// </summary>
    public string QueueName { get; set; } = "demo-queue";
}
