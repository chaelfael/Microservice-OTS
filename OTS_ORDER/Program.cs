using Microsoft.EntityFrameworkCore;
using Asp.Versioning; 
using OTS_ORDER.Data;
using OTS_ORDER.Interface;
using OTS_ORDER.Services;
using OTS_ORDER.Services.IntegrationService;
using StackExchange.Redis;
using System.Text.Json.Serialization;

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
// 2. DATABASE & BUSINESS SERVICES
// ----------------------------------------------------
builder.Services.AddDbContext<OrderDbContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<IOrderTrService, OrderTrService>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IOrderTrObService, OrderTrObService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ----------------------------------------------------
// 3. MESSAGE BUS (RABBITMQ) REGISTRATION
// ----------------------------------------------------
builder.Services.AddHostedService<MessageBusSubscriber>();
builder.Services.AddSingleton<IMessageBusClient, RabbitMQClient>();

// ----------------------------------------------------
// 4. REDIS CACHE CONFIGURATION (Logical Database 1)
// ----------------------------------------------------
// Reserved for future Order-specific caching (like rate limiting).
// The Flash Sale countdown happens in the Inventory Service's Redis (Database 0).
var redisConnectionString = builder.Configuration["RedisConnection"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// ----------------------------------------------------
// 5. INTERNAL MICROSERVICE CLIENTS (The Fast Lane)
// ----------------------------------------------------
// This wires up the HttpClient injected into your OrderTrService
var inventoryUrl = builder.Configuration["InventoryApiUrl"] ?? "http://ots-inv:80";

builder.Services.AddHttpClient("InventoryClient", client =>
{
    // Bypasses Nginx to talk directly inside the Docker network
    client.BaseAddress = new Uri("http://ots-inv:80"); 
    client.Timeout = TimeSpan.FromSeconds(5); 
});
var app = builder.Build();

// ----------------------------------------------------
// DATABASE MIGRATION & SEEDING BLOCK
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<OrderDbContext>();
    var isProd = app.Environment.IsProduction();

    PrepDb.SeedData(context, isProd);
}

// ----------------------------------------------------
// HTTP PIPELINE
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers(); 

app.Run();