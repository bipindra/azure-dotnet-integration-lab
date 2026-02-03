# PowerShell deployment script for 02-Db-Cosmos
param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$Action = "deploy" # deploy or destroy
)

$ErrorActionPreference = "Stop"

$projectName = "02-Db-Cosmos"
$resourceGroupName = "rg-ailab-$Environment"
$bicepFile = Join-Path $PSScriptRoot "main.bicep"

Write-Host "=== Azure Integration Lab - $projectName ===" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Location: $Location" -ForegroundColor Yellow
Write-Host "Action: $Action" -ForegroundColor Yellow
Write-Host ""

if ($Action -eq "destroy") {
    Write-Host "Destroying resources..." -ForegroundColor Red
    az group delete --name $resourceGroupName --yes --no-wait
    Write-Host "Resource group deletion initiated." -ForegroundColor Green
    exit 0
}

# Check if logged in
$account = az account show 2>$null
if (-not $account) {
    Write-Host "Not logged in to Azure. Please run 'az login'" -ForegroundColor Red
    exit 1
}

# Create resource group if it doesn't exist
Write-Host "Ensuring resource group exists..." -ForegroundColor Cyan
az group create --name $resourceGroupName --location $Location --output none

# Deploy Bicep template
Write-Host "Deploying Bicep template..." -ForegroundColor Cyan
Write-Host "Note: Cosmos DB account creation may take 5-10 minutes..." -ForegroundColor Yellow
$deploymentName = "deploy-$projectName-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

az deployment group create `
    --resource-group $resourceGroupName `
    --name $deploymentName `
    --template-file $bicepFile `
    --parameters projectName=$projectName environment=$Environment location=$Location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit 1
}

# Get outputs
Write-Host "`n=== Deployment Outputs ===" -ForegroundColor Green
$outputs = az deployment group show --resource-group $resourceGroupName --name $deploymentName --query properties.outputs -o json | ConvertFrom-Json

Write-Host "Cosmos DB Account Name: $($outputs.cosmosAccountName.value)" -ForegroundColor Green
Write-Host "Cosmos DB Endpoint: $($outputs.cosmosAccountEndpoint.value)" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Set the Cosmos DB endpoint in appsettings.json or environment variable:" -ForegroundColor White
Write-Host "   COSMOSDB__ACCOUNTENDPOINT=$($outputs.cosmosAccountEndpoint.value)" -ForegroundColor Gray
Write-Host "2. Run the application: cd src/02-Db-Cosmos && dotnet run" -ForegroundColor White
