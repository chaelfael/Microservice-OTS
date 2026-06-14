using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using OTS_INV.Data;
using RabbitMQ.Client;
using OTS_INV.Services.IntegrationService;
using OTS_INV.Services;
using OTS_INV.Interface;
using StackExchange.Redis; // <-- ADDED for Redis

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); 

// ----------------------------------------------------
// 1. API VERSIONING CONFIGURATION
// ----------------------------------------------------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();

// ----------------------------------------------------
// 2. REDIS GATEKEEPER CONFIGURATION
// ----------------------------------------------------
// Connect to Redis using the string from docker-compose (redis:6379,defaultDatabase=0)
var redisConnectionString = builder.Configuration["RedisConnection"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<ITicketCacheService, TicketCacheService>();

// ----------------------------------------------------
// 3. DATABASE & BUSINESS SERVICES
// ----------------------------------------------------
builder.Services.AddDbContext<InvDbContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<ITicketTrService, TicketTrService>(); 

// ----------------------------------------------------
// 4. MESSAGE BUS (RABBITMQ) REGISTRATION
// ----------------------------------------------------
builder.Services.AddHostedService<MessageBusSubscriber>();
builder.Services.AddSingleton<IMessageBusClient, RabbitMQClient>();

var app = builder.Build();

// ----------------------------------------------------
// DATABASE MIGRATION & SEEDING BLOCK
// ----------------------------------------------------
// ----------------------------------------------------
// DATABASE MIGRATION, SEEDING & CACHE WARMING
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<InvDbContext>();
    var cacheService = services.GetRequiredService<ITicketCacheService>(); // NEW: Get the Redis service
    var isProd = app.Environment.IsProduction();

    // 1. Run Migrations & Seed DB
    PrepDb.SeedData(context, isProd);

    // 2. The Cache Warmer: Auto-load Redis based on actual DB chairs
    try 
    {
        Console.WriteLine("--> Warming up Redis Cache...");
        
        // Group all available tickets by their Class Code (e.g., VIP, REGULAR)
        var availableStocks = context.Tickets
            .Where(t => t.Status == "AVAILABLE")
            .GroupBy(t => t.TicketClass.TicketClassCode)
            .Select(g => new { TicketClassCode = g.Key, AvailableCount = g.Count() })
            .ToList();

        foreach (var stock in availableStocks)
        {
            // Push each class into Redis
            // Note: We use .GetAwaiter().GetResult() because we are inside a synchronous scope block
            cacheService.InitializeTicketsAsync(stock.TicketClassCode, stock.AvailableCount).GetAwaiter().GetResult();
            
            Console.WriteLine($"--> [Gatekeeper Armed] Loaded {stock.AvailableCount} tickets for {stock.TicketClassCode}.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Cache Warming Failed: {ex.Message}");
    }
}

// ----------------------------------------------------
// STARTUP CONNECTION LOGGING
// ----------------------------------------------------
Console.WriteLine($"--> Redis Connection: {redisConnectionString}");
Console.WriteLine($"--> RabbitMQ Host: {builder.Configuration["RabbitMQHost"]}");

// ----------------------------------------------------
// HTTP PIPELINE
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers(); 

app.Run();