# Centralized Package Management Summary

## What was done

All common NuGet package references have been moved to a centralized `Directory.Build.props` file located at `src/Directory.Build.props`.

## Benefits

1. **Single source of truth** - Package versions are defined once and reused across all projects
2. **Easier maintenance** - Update versions in one place
3. **Consistency** - All projects use the same versions of common packages
4. **Cleaner project files** - Individual `.csproj` files are much simpler

## Structure

### Directory.Build.props

The file defines:
- **Common properties**: TargetFramework, ImplicitUsings, Nullable, LangVersion
- **Version variables**: Centralized version numbers for commonly used packages
- **Conditional package references**: Automatically adds common packages to console/worker apps (not libraries or web apps)
- **File copy rules**: Automatically copies appsettings.json files to output directory

### Project categorization

Projects now use simple markers to control behavior:

- `<IsCommonLibrary>true</IsCommonLibrary>` - For the Common library (no common packages added)
- `<IsWebProject>true</IsWebProject>` - For Web/API projects (ASP.NET manages DI/Config)
- `<IsAzureFunctions>true</IsAzureFunctions>` - For Azure Functions projects
- Default (no marker) - Console/Worker apps get full common packages

## Updated Projects

All 14 projects in the solution have been simplified:

### Console/Worker Projects (get common packages automatically)
- 00-Auth-KeyVault
- 01-Storage-Blob
- 02-Db-Cosmos
- 04-Messaging-ServiceBus
- 06-Streaming-EventHubs
- 12-Advanced-AKS-Keda-ServiceBus (Worker)

### Web Projects (marked with `<IsWebProject>true</IsWebProject>`)
- 03-Db-AzureSql-EFCore
- 05-Eventing-EventGrid
- 08-Hosting-ContainerApps-MinimalApi
- 09-Observability-AppInsights-Otel
- 10-ApiManagement-APIM
- 11-Caching-Redis

### Functions Project (marked with `<IsAzureFunctions>true</IsAzureFunctions>`)
- 07-Serverless-Functions-ServiceBusTrigger

### Library Project (marked with `<IsCommonLibrary>true</IsCommonLibrary>`)
- Common

## Common Package Versions

The following package versions are centralized:

```xml
<AzureIdentityVersion>1.11.0</AzureIdentityVersion>
<AzureCoreVersion>1.38.0</AzureCoreVersion>
<MicrosoftExtensionsVersion>8.0.0</MicrosoftExtensionsVersion>
<MicrosoftExtensionsAbstractionsVersion>8.0.2</MicrosoftExtensionsAbstractionsVersion>
<MicrosoftExtensionsConfigurationVersion>8.0.0</MicrosoftExtensionsConfigurationVersion>
<MicrosoftExtensionsConfigurationJsonVersion>8.0.1</MicrosoftExtensionsConfigurationJsonVersion>
<MicrosoftExtensionsDIVersion>8.0.1</MicrosoftExtensionsDIVersion>
<MicrosoftExtensionsHostingVersion>8.0.1</MicrosoftExtensionsHostingVersion>
<MicrosoftExtensionsLoggingVersion>8.0.1</MicrosoftExtensionsLoggingVersion>
<MicrosoftExtensionsOptionsVersion>8.0.2</MicrosoftExtensionsOptionsVersion>
<PollyVersion>8.4.2</PollyVersion>
```

## Example: Before and After

### Before (01-Storage-Blob.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>12</LangVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Storage.Blobs" Version="12.22.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Common\Common.csproj" />
  </ItemGroup>
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

### After (01-Storage-Blob.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Storage.Blobs" Version="12.22.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

**Result**: 65% reduction in code, all common properties and packages inherited from `Directory.Build.props`

## How to Update Package Versions

To update a common package version across all projects:

1. Open `src/Directory.Build.props`
2. Update the version property (e.g., `<MicrosoftExtensionsLoggingVersion>8.0.2</MicrosoftExtensionsLoggingVersion>`)
3. Save the file
4. All projects automatically use the new version

## Build Status

? Build successful - All projects compile correctly with the new structure.
