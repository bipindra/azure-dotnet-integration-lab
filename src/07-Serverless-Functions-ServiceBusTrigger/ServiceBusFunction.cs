// TODO: Implement Service Bus triggered function
// Example structure:
//
// [Function("ProcessServiceBusMessage")]
// public async Task Run(
//     [ServiceBusTrigger("demo-queue", Connection = "ServiceBusConnection")] string message,
//     FunctionContext context)
// {
//     var logger = context.GetLogger("ServiceBusFunction");
//     logger.LogInformation("Processing message: {Message}", message);
//     
//     // Process message
//     // Emit custom metrics
//     // Handle errors
// }

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServerlessFunctionsServiceBusTrigger;

public class ServiceBusFunction
{
    private readonly ILogger<ServiceBusFunction> _logger;

    public ServiceBusFunction(ILogger<ServiceBusFunction> logger)
    {
        _logger = logger;
    }

    // TODO: Add ServiceBusTrigger function
    // [Function("ProcessServiceBusMessage")]
    // public async Task Run([ServiceBusTrigger(...)] string message, FunctionContext context)
}
