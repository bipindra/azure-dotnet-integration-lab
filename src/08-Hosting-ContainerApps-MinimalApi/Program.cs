// TODO: Implement Minimal API for Container Apps
// - Create simple endpoints
// - Use managed identity
// - Health check endpoint
// - Ready for containerization

using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO: Add Minimal API endpoints
app.MapGet("/", () => "Hello from Container Apps!");
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.MapHealthChecks("/health");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogApplicationStartup("08-Hosting-ContainerApps-MinimalApi", "1.0.0");

app.Run();
