# Troubleshooting Guide 

Common issues and solutions for the Azure Integration Lab projects.

## Authentication Issues

### Error: "DefaultAzureCredential failed to retrieve a token"

**Symptoms:**
- Application fails to authenticate
- Error message about token retrieval

**Solutions:**
1. Ensure Azure CLI is installed and you're logged in:
   ```bash
   az login
   az account show
   ```

2. Verify your account has the correct RBAC roles:
   ```bash
   az role assignment list --assignee $(az account show --query user.name -o tsv)
   ```

3. Check that the resource exists and is accessible

4. For deployed resources, verify managed identity is enabled

### Error: "Access denied" or "Forbidden"

**Symptoms:**
- Operations fail with 403 Forbidden
- RBAC errors

**Solutions:**
1. Verify role assignment:
   ```bash
   az role assignment list \
     --scope /subscriptions/{sub-id}/resourceGroups/{rg}/providers/{resource-type}/{resource-name}
   ```

2. Assign the required role manually:
   ```bash
   az role assignment create \
     --role "<Role Name>" \
     --assignee $(az account show --query user.name -o tsv) \
     --scope "<Resource Scope>"
   ```

3. Wait a few minutes for role propagation

## Configuration Issues

### Error: "Configuration value not found"

**Symptoms:**
- App fails to start
- Missing configuration errors

**Solutions:**
1. Check `appsettings.json` exists and has required values
2. Verify environment variables are set correctly
3. Check user secrets:
   ```bash
   dotnet user-secrets list
   ```

4. Ensure configuration key names match (case-sensitive)

### Connection String Issues

**Symptoms:**
- Cannot connect to Azure services
- Connection timeout errors

**Solutions:**
1. Verify connection strings/endpoints are correct
2. Check firewall rules (SQL, Storage, etc.)
3. Ensure network connectivity
4. For SQL: Add your IP to firewall rules

## Deployment Issues

### Bicep Deployment Failures

**Symptoms:**
- `az deployment group create` fails
- Resource creation errors

**Solutions:**
1. Check resource name uniqueness (some resources require globally unique names)
2. Verify subscription quotas:
   ```bash
   az vm list-usage --location eastus
   ```

3. Check for existing resources with same name
4. Review Bicep template syntax
5. Validate template:
   ```bash
   az deployment group validate --template-file main.bicep --parameters ...
   ```

### Resource Name Conflicts

**Symptoms:**
- "Resource name already exists" errors

**Solutions:**
1. Use different environment names (dev, dev2, etc.)
2. Delete existing resources first
3. Use different random suffixes

## Network/Firewall Issues

### Cannot Connect to SQL Database

**Symptoms:**
- Connection timeout
- Firewall errors

**Solutions:**
1. Add your IP to SQL Server firewall:
   ```bash
   az sql server firewall-rule create \
     --resource-group <rg> \
     --server <server> \
     --name AllowMyIP \
     --start-ip-address <your-ip> \
     --end-ip-address <your-ip>
   ```

2. Allow Azure services (if needed)
3. Check network security groups (if using VNet)

### Storage Account Access Denied

**Symptoms:**
- Cannot access blobs
- 403 errors

**Solutions:**
1. Verify RBAC role assignment (Storage Blob Data Contributor)
2. Check firewall rules (if enabled)
3. Ensure managed identity is configured (for deployed apps)

## Service-Specific Issues

### Key Vault

**Issue:** Cannot read secrets
- Verify "Key Vault Secrets User" role
- Check vault access policies (if using access policies instead of RBAC)
- Ensure secret exists

### Cosmos DB

**Issue:** High RU consumption
- Use serverless mode for development
- Optimize queries
- Check partition key strategy

**Issue:** Connection timeout
- Verify endpoint URL
- Check firewall rules
- Ensure account is accessible

### Service Bus

**Issue:** Messages not received
- Verify receiver is running
- Check queue name matches
- Review dead-letter queue
- Check message TTL

### Azure Functions

**Issue:** Function not triggering
- Verify trigger configuration
- Check connection strings
- Review function logs
- Ensure function is running

## Cost Management

### Unexpected Charges

**Symptoms:**
- Higher than expected Azure bills

**Solutions:**
1. Always teardown resources after testing:
   ```bash
   ./deploy.sh <project> <env> destroy
   ```

2. Use Azure Cost Management to track spending
3. Set up budget alerts
4. Review resource usage in Azure Portal

### Resource Not Deleted

**Symptoms:**
- Resources still exist after teardown

**Solutions:**
1. Check for dependencies (child resources)
2. Delete resources manually if needed:
   ```bash
   az group delete --name <rg> --yes --no-wait
   ```

3. Verify deletion completed:
   ```bash
   az group exists --name <rg>
   ```

## Performance Issues

### Slow Operations

**Symptoms:**
- Long response times
- Timeout errors

**Solutions:**
1. Check service tier (Basic vs Standard)
2. Review retry policies
3. Optimize queries/operations
4. Check network latency
5. Review service quotas

## Getting Help

1. **Check Project README**: Each project has specific troubleshooting
2. **Azure Documentation**: [docs.microsoft.com/azure](https://docs.microsoft.com/azure)
3. **Azure Status**: [status.azure.com](https://status.azure.com)
4. **Stack Overflow**: Tag with `azure` and specific service
5. **GitHub Issues**: Open an issue in the repository

## Common Commands

```bash
# Check Azure login
az account show

# List resource groups
az group list

# Check role assignments
az role assignment list --assignee <your-email>

# View deployment outputs
az deployment group show --resource-group <rg> --name <deployment-name> --query properties.outputs

# Check resource status
az resource list --resource-group <rg>

# View logs (for deployed apps)
az webapp log tail --name <app-name> --resource-group <rg>
```
