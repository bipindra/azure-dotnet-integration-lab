# 05-Eventing-EventGrid

## Purpose

This project demonstrates **Azure Event Grid** event handling. It showcases:

- Receiving Event Grid events via webhook
- Handling validation handshake
- Processing blob storage events
- Structured logging of events

## Architecture

```mermaid
graph LR
    A[Blob Storage] -->|Event| B[Event Grid Topic]
    B -->|Webhook| C[Minimal API Receiver]
    C -->|Process| D[Event Handler]
    
    style A fill:#0078d4
    style B fill:#0078d4
    style C fill:#0078d4
    style D fill:#0078d4
```

## Implementation TODO

1. **Event Grid Validation Endpoint**
   - Handle GET requests with `aeg-sas-token` header
   - Return validation code from query string

2. **Event Processing Endpoint**
   - Handle POST requests with Event Grid events
   - Deserialize EventGridEvent objects
   - Process blob storage events (BlobCreated, BlobDeleted)
   - Log event details

3. **Bicep Infrastructure**
   - Create Event Grid System Topic (for Blob Storage)
   - Configure event subscription
   - Set webhook endpoint URL

## Prerequisites

- .NET 9 SDK
- Azure CLI
- ngrok or public endpoint for webhook (local dev)

## Setup

1. Deploy infrastructure (see `infra/` folder)
2. Configure webhook endpoint (use ngrok for local dev)
3. Implement event receiver endpoint
4. Test with blob uploads

## Cost Considerations

- Event Grid: **First 100,000 operations/month free**, then $0.60 per million
- For development: **Typically free**

## Next Steps

- Implement the TODO items above
- Add event filtering
- Implement event batching
- Add dead-letter handling
