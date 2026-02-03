# PowerShell deployment script for 00-Auth-KeyVault
param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$Action = "deploy" # deploy or destroy
)

$ErrorActionPreference = "Stop"

$projectName = "00-Auth-KeyVault"
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

Write-Host "Key Vault Name: $($outputs.keyVaultName.value)" -ForegroundColor Green
Write-Host "Key Vault URI: $($outputs.keyVaultUri.value)" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Set the Key Vault URI in appsettings.json or environment variable:" -ForegroundColor White
Write-Host "   KEYVAULT__VAULTURI=$($outputs.keyVaultUri.value)" -ForegroundColor Gray
Write-Host "2. Run the application: cd src/00-Auth-KeyVault && dotnet run" -ForegroundColor White
