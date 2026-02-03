# 10-ApiManagement-APIM

## Purpose

This project demonstrates **Azure API Management (APIM)** integration. It showcases:

- Importing APIs into APIM
- JWT validation policies
- Rate limiting
- Request/response transformation
- Caching policies

## Architecture

```mermaid
graph LR
    A[Client] -->|Request| B[API Management]
    B -->|Policy| C[Backend API]
    C -->|Response| B
    B -->|Response| A
    
    style A fill:#0078d4
    style B fill:#0078d4
    style C fill:#0078d4
```

## Implementation TODO

1. **Backend API**
   - Complete Minimal API implementation
   - Ensure OpenAPI/Swagger is available

2. **APIM Configuration**
   - Create APIM instance (Consumption tier if possible)
   - Import API from backend
   - Configure policies (JWT, rate limit, caching)

3. **Bicep Infrastructure**
   - Create APIM instance
   - Configure API import
   - Set up policies

## Prerequisites

- .NET 9 SDK
- Azure CLI

## Cost Considerations

- APIM Consumption: **Pay per API call** (~$3.50 per million calls)
- APIM Developer: **~$50/month** (fixed)
- For development: **Use Consumption tier** for cost savings

## Next Steps

- Complete backend API
- Create APIM instance
- Import API
- Configure policies
- Test through APIM
