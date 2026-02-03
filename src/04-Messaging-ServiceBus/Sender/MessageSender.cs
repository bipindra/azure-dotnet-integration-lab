using Azure.Messaging.ServiceBus;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessagingServiceBus.Sender;

/// <summary>
/// Service for sending messages to Service Bus.
/// </summary>
public class MessageSender
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<MessageSender> _logger;
    private readonly ServiceBusOptions _options;

    public MessageSender(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> options,
        ILogger<MessageSender> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Sends a single message to the queue.
    /// </summary>
    public async Task SendMessageAsync(string messageBody, Dictionary<string, string>? properties = null, CancellationToken cancellationToken = default)
    {
        await using var sender = _serviceBusClient.CreateSender(_options.QueueName);

        try
        {
            var message = new ServiceBusMessage(messageBody);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    message.ApplicationProperties[prop.Key] = prop.Value;
                }
            }

            message.ApplicationProperties["SentAt"] = DateTime.UtcNow.ToString("O");
            message.MessageId = Guid.NewGuid().ToString();

            _logger.LogInformation("Sending message to queue '{QueueName}': {MessageId}", _options.QueueName, message.MessageId);

            await sender.SendMessageAsync(message, cancellationToken);

            _logger.LogInformation("Successfully sent message {MessageId}", message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            throw;
        }
    }

    /// <summary>
    /// Sends a batch of messages to the queue.
    /// </summary>
    public async Task SendBatchAsync(IEnumerable<string> messageBodies, CancellationToken cancellationToken = default)
    {
        await using var sender = _serviceBusClient.CreateSender(_options.QueueName);

        try
        {
            _logger.LogInformation("Sending batch of {Count} messages to queue '{QueueName}'", messageBodies.Count(), _options.QueueName);

            using var messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);

            foreach (var body in messageBodies)
            {
                var message = new ServiceBusMessage(body)
                {
                    MessageId = Guid.NewGuid().ToString()
                };
                message.ApplicationProperties["SentAt"] = DateTime.UtcNow.ToString("O");

                if (!messageBatch.TryAddMessage(message))
                {
                    // Batch is full, send it and create a new one
                    await sender.SendMessagesAsync(messageBatch, cancellationToken);
                    using var newBatch = await sender.CreateMessageBatchAsync(cancellationToken);
                    newBatch.TryAddMessage(message);
                }
            }

            // Send any remaining messages
            if (messageBatch.Count > 0)
            {
                await sender.SendMessagesAsync(messageBatch, cancellationToken);
            }

            _logger.LogInformation("Successfully sent batch of messages");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message batch");
            throw;
        }
    }
}
