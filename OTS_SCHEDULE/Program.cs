using Microsoft.EntityFrameworkCore;
using Asp.Versioning; // Required for modern .NET API Versioning
using OTS_SCHEDULE.Data;
using OTS_SCHEDULE.Interface;
using OTS_SCHEDULE.Services;
// Make sure to add the using statements for your specific RabbitMQ folders!
using OTS_SCHEDULE.Services.IntegrationService;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. API VERSIONING CONFIGURATION (AdIns Style)
// ----------------------------------------------------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ----------------------------------------------------
// 2. STANDARD WEB SERVICES & MAPPING
// ----------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ----------------------------------------------------
// 3. DATABASE & REPOSITORIES
// ----------------------------------------------------
builder.Services.AddDbContext<SchdlDbContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<ISchdlTrService, SchdlTrService>();

// ----------------------------------------------------
// 4. MICROSERVICE INFRASTRUCTURE (RABBITMQ & WORKERS)
// ----------------------------------------------------
// The Megaphone: Allows controllers/services to publish to RabbitMQ
builder.Services.AddSingleton<IMessageBusClient, RabbitMQClient>();

// The Ears: Listens continuously for ORDER_INITIATED and PAYMENT_SUCCESS
builder.Services.AddHostedService<MessageBusSubscriber>();

// The Alarm Clock: Wakes up every 10 seconds to expire abandoned orders
builder.Services.AddHostedService<ExpirationWorker>();




var app = builder.Build();

// ----------------------------------------------------
// 5. DATABASE MIGRATION & SEEDING BLOCK
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SchdlDbContext>();
    var isProd = app.Environment.IsProduction();

    PrepDb.SeedData(context, isProd);
}

// ----------------------------------------------------
// 6. HTTP REQUEST PIPELINE
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers(); 

app.Run();