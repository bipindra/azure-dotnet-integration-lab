// TODO: Implement Event Grid event receiver
// - Handle Event Grid validation handshake (GET requests with validation code)
// - Accept and process events (POST requests with event data)
// - Support blob storage events (BlobCreated, BlobDeleted, etc.)
// - Log received events with structured logging
// - Return appropriate HTTP status codes

using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogApplicationStartup("05-Eventing-EventGrid", "1.0.0");

logger.LogWarning("TODO: Implement Event Grid event receiver endpoint");
logger.LogInformation("See README.md for implementation guidance");

app.Run();
