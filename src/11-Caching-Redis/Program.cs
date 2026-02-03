// TODO: Implement Redis caching with cache-aside pattern
// - Configure Redis connection
// - Implement cache-aside pattern
// - Add cache endpoints
// - Handle cache misses

using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Configure Redis
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
// });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO: Add cache endpoints
app.MapGet("/", () => "Redis Caching Demo");
app.MapGet("/api/cache/{key}", (string key) => $"Get from cache: {key}");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogApplicationStartup("11-Caching-Redis", "1.0.0");
logger.LogWarning("TODO: Configure Redis and implement cache-aside pattern");

app.Run();
