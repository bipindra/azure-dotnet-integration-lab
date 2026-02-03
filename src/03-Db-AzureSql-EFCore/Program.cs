using Common;
using DbAzureSqlEFCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure SQL options
builder.Services.Configure<DbAzureSqlEFCore.AzureSqlOptions>(
    builder.Configuration.GetSection(DbAzureSqlEFCore.AzureSqlOptions.SectionName));

var sqlOptions = builder.Configuration.GetSection(DbAzureSqlEFCore.AzureSqlOptions.SectionName)
    .Get<DbAzureSqlEFCore.AzureSqlOptions>() ?? new DbAzureSqlEFCore.AzureSqlOptions();

// Build connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    // Build connection string from options
    if (string.IsNullOrWhiteSpace(sqlOptions.ServerName))
    {
        throw new InvalidOperationException(
            "AzureSql:ServerName or ConnectionStrings:DefaultConnection must be configured. " +
            "Set it in appsettings.json or via environment variable AZURESQL__SERVERNAME");
    }

    // For managed identity, use: Server=...;Database=...;Authentication=Active Directory Default
    // For SQL auth, use: Server=...;Database=...;User Id=...;Password=...
    // This demo uses Active Directory Default (managed identity)
    connectionString = $"Server={sqlOptions.ServerName};Database={sqlOptions.DatabaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False";
}

// Configure EF Core with Azure SQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and migrations are applied
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogApplicationStartup("03-Db-AzureSql-EFCore", "1.0.0");

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to apply database migrations");
    // Don't throw - allow app to start so user can fix the issue
}

app.Run();
