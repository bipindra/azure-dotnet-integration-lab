# Azure .NET Integration Lab

A comprehensive collection of .NET 9 projects demonstrating Azure services end-to-end, including infrastructure (Bicep), application code, configuration, and documentation.

## 🎯 Goals

- **One Solution**: Single Visual Studio solution (`AzureIntegrationLab.sln`) with all projects
- **Independent Projects**: Each project is runnable locally and deployable to Azure
- **Modern Stack**: .NET 9, C# 12+, Azure SDKs, Managed Identity
- **Production-Ready Patterns**: DI, Options pattern, structured logging, retries
- **Cost-Conscious**: Smallest SKUs, free tiers where possible, explicit teardown

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Subscription](https://azure.microsoft.com/free/)
- [PowerShell 7+](https://aka.ms/powershell) or Bash
- [Git](https://git-scm.com/)

## 🏗️ Repository Structure

```
azure-dotnet-integration-lab/
├── src/
│   ├── Common/                          # Shared utilities
│   ├── 00-Auth-KeyVault/                # Entra ID + Key Vault
│   ├── 01-Storage-Blob/                 # Blob Storage
│   ├── 02-Db-Cosmos/                     # Cosmos DB
│   ├── 03-Db-AzureSql-EFCore/            # Azure SQL + EF Core
│   ├── 04-Messaging-ServiceBus/          # Service Bus
│   ├── 05-Eventing-EventGrid/            # Event Grid (scaffolded)
│   ├── 06-Streaming-EventHubs/           # Event Hubs (scaffolded)
│   ├── 07-Serverless-Functions-ServiceBusTrigger/  # Azure Functions (scaffolded)
│   ├── 08-Hosting-ContainerApps-MinimalApi/        # Container Apps (scaffolded)
│   ├── 09-Observability-AppInsights-Otel/          # App Insights + OTel (scaffolded)
│   ├── 10-ApiManagement-APIM/           # API Management (scaffolded)
│   ├── 11-Caching-Redis/                 # Redis Cache (scaffolded)
│   └── 12-Advanced-AKS-Keda-ServiceBus/  # AKS + KEDA (scaffolded, expensive!)
├── infra/                                # Shared infrastructure modules
├── .github/workflows/                    # CI/CD pipelines
└── AzureIntegrationLab.sln              # Visual Studio solution
```

## 📚 Projects

| # | Project | Status | Description | Cost/Month |
|---|---------|--------|-------------|------------|
| 00 | **Auth-KeyVault** | ✅ Complete | Entra ID authentication + Key Vault secrets | $0 |
| 01 | **Storage-Blob** | ✅ Complete | Blob upload/download/list/SAS | < $1 |
| 02 | **Db-Cosmos** | ✅ Complete | Cosmos DB CRUD with partition keys | < $1 |
| 03 | **Db-AzureSql-EFCore** | ✅ Complete | Azure SQL + EF Core + Migrations | ~$5 |
| 04 | **Messaging-ServiceBus** | ✅ Complete | Service Bus sender/receiver | < $1 |
| 05 | **Eventing-EventGrid** | 🚧 Scaffolded | Event Grid webhook receiver | Free |
| 06 | **Streaming-EventHubs** | 🚧 Scaffolded | Event Hubs producer/consumer | < $1 |
| 07 | **Serverless-Functions** | 🚧 Scaffolded | Azure Functions + Service Bus | Free |
| 08 | **ContainerApps** | 🚧 Scaffolded | Container Apps deployment | < $5 |
| 09 | **Observability-Otel** | 🚧 Scaffolded | App Insights + OpenTelemetry | < $5 |
| 10 | **ApiManagement-APIM** | 🚧 Scaffolded | API Management policies | ~$3-50 |
| 11 | **Caching-Redis** | 🚧 Scaffolded | Redis cache-aside pattern | ~$15 |
| 12 | **AKS-Keda** | 🚧 Scaffolded | AKS + KEDA autoscaling | ⚠️ ~$100+ |

**Legend:**
- ✅ Complete: Fully implemented with code, infra, and docs
- 🚧 Scaffolded: Structure and README ready, implementation TODOs

## 🚀 Quick Start

### 1. Clone and Build

```bash
git clone <repo-url>
cd azure-dotnet-integration-lab
dotnet restore
dotnet build
```

### 2. Authenticate with Azure

```bash
az login
az account show  # Verify your subscription
```

### 3. Deploy a Project

Each project has its own deployment script. For example:

```bash
# Deploy project 00 (Key Vault)
cd src/00-Auth-KeyVault/infra
./deploy.sh dev  # or deploy.ps1 on Windows

# Configure and run
cd ../..
dotnet run --project src/00-Auth-KeyVault
```

### 4. Teardown

```bash
cd src/00-Auth-KeyVault/infra
./deploy.sh dev destroy
```

## 📖 Project-Specific Documentation

Each project includes a detailed README with:
- Architecture diagram (Mermaid)
- Prerequisites
- Setup steps
- Run instructions
- Troubleshooting
- Cost considerations

Navigate to `src/<project-name>/README.md` for project-specific documentation.

## 🛠️ Common Patterns

All projects follow consistent patterns:

### Authentication
- **Local**: `DefaultAzureCredential` → Azure CLI login
- **Deployed**: Managed Identity (automatic)

### Configuration
- `appsettings.json` for defaults
- Environment variables for overrides
- `dotnet user-secrets` for local secrets

### Logging
- Structured logging with `ILogger<T>`
- Correlation IDs
- Log levels: Information, Warning, Error

### Retry Policies
- Exponential backoff
- Configurable retry counts
- Transient error handling

### Infrastructure
- Bicep templates for IaC
- RBAC for access control
- Consistent naming: `rg-ailab-<env>`, `kv-ailab-<rand>`, etc.

## 🔧 Development Workflow

1. **Choose a project** from the table above
2. **Read the project README** for specific instructions
3. **Deploy infrastructure** using the project's `infra/` scripts
4. **Configure the app** (appsettings.json or env vars)
5. **Run locally** with `dotnet run`
6. **Test and verify** functionality
7. **Teardown resources** when done

## 💰 Cost Management

- **Free Tier**: Used where available (Key Vault, Event Grid, Functions)
- **Smallest SKUs**: Basic/Standard tiers for development
- **Serverless**: Cosmos DB serverless, Functions Consumption
- **Explicit Teardown**: Always destroy resources after testing
- **Cost Warnings**: Project 12 (AKS) has significant costs

**Estimated total cost for all projects (if all deployed simultaneously): ~$30-50/month**

**Recommended**: Deploy one project at a time, test, then teardown before moving to the next.

## 🤝 Contributing

This is a learning lab. Feel free to:
- Implement the scaffolded projects (05-12)
- Improve existing implementations
- Add new Azure service examples
- Fix bugs or improve documentation

## 📝 License

This project is provided as-is for educational purposes.

## 🔗 Resources

- [Azure .NET SDK Documentation](https://docs.microsoft.com/dotnet/azure/)
- [Azure Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep/)
- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/core/)
- [Azure Architecture Center](https://docs.microsoft.com/azure/architecture/)

## ⚠️ Important Notes

1. **Always teardown resources** after testing to avoid unexpected costs
2. **Project 12 (AKS)** is expensive (~$100+/month) - only deploy if needed
3. **Use separate resource groups** per environment (dev, staging, prod)
4. **Review costs** regularly in Azure Portal
5. **Follow security best practices** - don't commit secrets

## 🐛 Troubleshooting

### Common Issues

**Authentication Errors**
- Ensure `az login` is successful
- Verify RBAC role assignments
- Check subscription permissions

**Deployment Failures**
- Verify resource name uniqueness
- Check quota limits
- Review Bicep template parameters

**Connection Errors**
- Verify firewall rules
- Check network connectivity
- Ensure managed identity is configured

See individual project READMEs for specific troubleshooting steps.

## 📞 Support

For issues or questions:
1. Check the project-specific README
2. Review Azure documentation
3. Check Azure service status
4. Open an issue in the repository

---

**Happy Learning! 🚀**
