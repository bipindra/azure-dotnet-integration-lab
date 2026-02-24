using Azure.Messaging.ServiceBus;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessagingServiceBus.Receiver;

/// <summary>
/// Service for receiving and processing messages from Service Bus.
/// </summary>
public class MessageReceiver
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<MessageReceiver> _logger;
    private readonly ServiceBusOptions _options;
    private ServiceBusProcessor? _processor;

    public MessageReceiver(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> options,
        ILogger<MessageReceiver> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Starts processing messages from the queue.
    /// </summary>
    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1, // Process one message at a time for demo
            AutoCompleteMessages = false, // Manual completion for better control
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        };

        _processor = _serviceBusClient.CreateProcessor(_options.QueueName, processorOptions);

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("Starting message processor for queue '{QueueName}'", _options.QueueName);
        await _processor.StartProcessingAsync(cancellationToken);
    }

    /// <summary>
    /// Stops processing messages.
    /// </summary>
    public async Task StopProcessingAsync(CancellationToken cancellationToken = default)
    {
        if (_processor != null)
        {
            _logger.LogInformation("Stopping message processor");
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
            _processor = null;
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var messageId = args.Message.MessageId;
            var body = args.Message.Body.ToString();

            _logger.LogInformation(
                "Processing message {MessageId}: {Body}",
                messageId,
                body);

            // Simulate message processing
            await Task.Delay(1000, args.CancellationToken);

            // Check for dead-letter scenarios (example: messages with "error" in body)
            if (body.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Message {MessageId} contains 'error', will be dead-lettered", messageId);
                await args.DeadLetterMessageAsync(args.Message, "Message contains error keyword", cancellationToken: args.CancellationToken);
                return;
            }

            // Complete the message
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            _logger.LogInformation("Successfully processed message {MessageId}", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", args.Message.MessageId);

            // Abandon the message so it can be retried
            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Error in message processor: {ErrorSource}", args.ErrorSource);

        return Task.CompletedTask;
    }
}
