# 06-Streaming-EventHubs

## Purpose

This project demonstrates **Azure Event Hubs** for high-throughput event streaming. It showcases:

- Producing events in batches
- Consuming events with checkpointing
- Using blob storage for checkpoint storage
- Partition key routing

## Architecture

```mermaid
graph LR
    A[Producer] -->|Events| B[Event Hub]
    B -->|Consume| C[Consumer]
    C -->|Checkpoints| D[Blob Storage]
    
    style A fill:#0078d4
    style B fill:#0078d4
    style C fill:#0078d4
    style D fill:#0078d4
```

## Implementation TODO

1. **Producer**
   - Create EventHubProducerClient
   - Send batches using EventDataBatch
   - Include partition keys

2. **Consumer**
   - Create EventProcessorClient
   - Configure blob storage for checkpoints
   - Process events with checkpointing
   - Handle partition initialization and closing

3. **Bicep Infrastructure**
   - Create Event Hubs namespace
   - Create Event Hub
   - Create storage account for checkpoints
   - Configure RBAC

## Prerequisites

- .NET 9 SDK
- Azure CLI

## Cost Considerations

- Event Hubs Basic: **~$0.05 per million events**
- Storage for checkpoints: **Minimal cost**
- For development: **< $1/month**

## Next Steps

- Implement producer and consumer
- Add error handling
- Implement custom partition processing
- Add metrics and monitoring
