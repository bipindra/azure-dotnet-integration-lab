// TODO: Implement OpenTelemetry with Application Insights
// - Configure OpenTelemetry
// - Add traces, metrics, and logs
// - Show correlation IDs
// - Demonstrate dependency tracking

using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Configure OpenTelemetry
// builder.Services.AddOpenTelemetry()
//     .WithTracing(...)
//     .WithMetrics(...)
//     .UseAzureMonitor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO: Add endpoints that demonstrate observability
app.MapGet("/", () => "OpenTelemetry Demo");
app.MapGet("/api/trace", () => "Trace endpoint");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogApplicationStartup("09-Observability-AppInsights-Otel", "1.0.0");
logger.LogWarning("TODO: Configure OpenTelemetry and Application Insights");

app.Run();
