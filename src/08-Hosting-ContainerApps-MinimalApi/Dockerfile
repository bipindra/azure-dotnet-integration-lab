FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/08-Hosting-ContainerApps-MinimalApi/08-Hosting-ContainerApps-MinimalApi.csproj", "src/08-Hosting-ContainerApps-MinimalApi/"]
COPY ["src/Common/Common.csproj", "src/Common/"]
RUN dotnet restore "src/08-Hosting-ContainerApps-MinimalApi/08-Hosting-ContainerApps-MinimalApi.csproj"
COPY . .
WORKDIR "/src/src/08-Hosting-ContainerApps-MinimalApi"
RUN dotnet build "08-Hosting-ContainerApps-MinimalApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "08-Hosting-ContainerApps-MinimalApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "08-Hosting-ContainerApps-MinimalApi.dll"]
