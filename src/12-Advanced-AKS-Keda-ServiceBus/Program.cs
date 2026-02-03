// TODO: Implement AKS worker with KEDA scaling
// - Worker service that consumes Service Bus
// - Configured for KEDA autoscaling
// - Handle scale-up and scale-down
//
// NOTE: This is an advanced/expensive scenario
// - Requires AKS cluster
// - Requires KEDA installation
// - Higher costs than other projects

using Common;

var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();
logger.LogApplicationStartup("12-Advanced-AKS-Keda-ServiceBus", "1.0.0");

logger.LogWarning("TODO: Implement AKS worker with KEDA Service Bus scaler");
logger.LogWarning("WARNING: This project requires AKS cluster and has higher costs");
logger.LogInformation("See README.md for implementation guidance and cost warnings");
