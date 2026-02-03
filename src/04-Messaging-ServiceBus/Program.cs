using Azure.Messaging.ServiceBus;
using Common;
using MessagingServiceBus.Receiver;
using MessagingServiceBus.Sender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessagingServiceBus;

class Program
{
    static async Task Main(string[] args)
    {
        // Determine mode: sender or receiver
        var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "sender";

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Configure Service Bus options
                services.Configure<ServiceBusOptions>(
                    configuration.GetSection(ServiceBusOptions.SectionName));

                var serviceBusOptions = configuration.GetSection(ServiceBusOptions.SectionName)
                    .Get<ServiceBusOptions>() ?? new ServiceBusOptions();

                if (string.IsNullOrWhiteSpace(serviceBusOptions.Namespace))
                {
                    throw new InvalidOperationException(
                        "ServiceBus:Namespace must be configured. " +
                        "Set it in appsettings.json or via environment variable SERVICEBUS__NAMESPACE");
                }

                // Create ServiceBusClient with DefaultAzureCredential
                var credential = AzureCredentialHelper.CreateCredential();
                var fullyQualifiedNamespace = serviceBusOptions.Namespace.EndsWith(".servicebus.windows.net")
                    ? serviceBusOptions.Namespace
                    : $"{serviceBusOptions.Namespace}.servicebus.windows.net";

                var serviceBusClientOptions = new ServiceBusClientOptions
                {
                    RetryOptions = new Azure.Messaging.ServiceBus.ServiceBusRetryOptions
                    {
                        Mode = Azure.Messaging.ServiceBus.ServiceBusRetryMode.Exponential,
                        MaxRetries = 3,
                        Delay = TimeSpan.FromSeconds(1),
                        MaxDelay = TimeSpan.FromSeconds(30)
                    }
                };

                var serviceBusClient = new ServiceBusClient(fullyQualifiedNamespace, credential, serviceBusClientOptions);

                services.AddSingleton(serviceBusClient);

                if (mode == "receiver")
                {
                    services.AddHostedService<MessageReceiverWorker>();
                }
                else
                {
                    services.AddScoped<MessageSender>();
                }
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogApplicationStartup($"04-Messaging-ServiceBus-{mode}", "1.0.0");

        try
        {
            if (mode == "receiver")
            {
                logger.LogInformation("Starting in RECEIVER mode. Press Ctrl+C to stop.");
                await host.RunAsync();
            }
            else
            {
                logger.LogInformation("Starting in SENDER mode.");
                var serviceBusOptions = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
                logger.LogAzureResourceConnection("Azure Service Bus", serviceBusOptions.Namespace);

                var sender = host.Services.GetRequiredService<MessageSender>();

                // Send sample messages
                logger.LogInformation("\n=== Sending Messages ===");

                for (int i = 1; i <= 5; i++)
                {
                    var messageBody = $"Hello from Service Bus! Message #{i}";
                    await sender.SendMessageAsync(
                        messageBody,
                        new Dictionary<string, string>
                        {
                            { "MessageNumber", i.ToString() },
                            { "Source", "SenderApp" }
                        });
                    await Task.Delay(500);
                }

                logger.LogInformation("\n=== Sending Batch ===");
                var batchMessages = new[]
                {
                    "Batch message 1",
                    "Batch message 2",
                    "Batch message 3"
                };
                await sender.SendBatchAsync(batchMessages);

                logger.LogInformation("\n=== Demo Complete ===");
                logger.LogInformation("Start the receiver with: dotnet run receiver");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application error occurred");
            Environment.ExitCode = 1;
        }
    }
}

/// <summary>
/// Background worker for receiving messages.
/// </summary>
public class MessageReceiverWorker : BackgroundService
{
    private readonly MessageReceiver _messageReceiver;
    private readonly ILogger<MessageReceiverWorker> _logger;

    public MessageReceiverWorker(
        MessageReceiver messageReceiver,
        ILogger<MessageReceiverWorker> logger)
    {
        _messageReceiver = messageReceiver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Message receiver worker starting");
        await _messageReceiver.StartProcessingAsync(stoppingToken);

        // Keep running until cancellation
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        await _messageReceiver.StopProcessingAsync(stoppingToken);
        _logger.LogInformation("Message receiver worker stopped");
    }
}
