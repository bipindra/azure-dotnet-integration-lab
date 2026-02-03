// TODO: Create Minimal API for APIM import
// - Simple API endpoints
// - Ready for APIM import
// - Document with Swagger/OpenAPI

using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO: Add API endpoints for APIM
app.MapGet("/", () => "API Management Demo");
app.MapGet("/api/values", () => new[] { "value1", "value2" });

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogApplicationStartup("10-ApiManagement-APIM", "1.0.0");
logger.LogWarning("TODO: Implement API endpoints and configure APIM policies");

app.Run();
