# PowerShell deployment script for 03-Db-AzureSql-EFCore
param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$Action = "deploy" # deploy or destroy
)

$ErrorActionPreference = "Stop"

$projectName = "03-Db-AzureSql-EFCore"
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

# Generate secure password
$sqlPassword = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 16 | ForEach-Object {[char]$_})

# Deploy Bicep template
Write-Host "Deploying Bicep template..." -ForegroundColor Cyan
Write-Host "Note: SQL Server creation may take a few minutes..." -ForegroundColor Yellow
$deploymentName = "deploy-$projectName-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

az deployment group create `
    --resource-group $resourceGroupName `
    --name $deploymentName `
    --template-file $bicepFile `
    --parameters projectName=$projectName environment=$Environment location=$Location sqlAdminPassword=$sqlPassword

if ($LASTEXITCODE -ne 0) {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit 1
}

# Get outputs
Write-Host "`n=== Deployment Outputs ===" -ForegroundColor Green
$outputs = az deployment group show --resource-group $resourceGroupName --name $deploymentName --query properties.outputs -o json | ConvertFrom-Json

Write-Host "SQL Server Name: $($outputs.sqlServerName.value)" -ForegroundColor Green
Write-Host "SQL Server FQDN: $($outputs.sqlServerFqdn.value)" -ForegroundColor Green
Write-Host "Database Name: $($outputs.databaseName.value)" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Add your IP to the firewall (see README for instructions)" -ForegroundColor White
Write-Host "2. Set the SQL Server name in appsettings.json or environment variable:" -ForegroundColor White
Write-Host "   AZURESQL__SERVERNAME=$($outputs.sqlServerFqdn.value)" -ForegroundColor Gray
Write-Host "   AZURESQL__DATABASENAME=$($outputs.databaseName.value)" -ForegroundColor Gray
Write-Host "3. Run the application: cd src/03-Db-AzureSql-EFCore && dotnet run" -ForegroundColor White
