using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OTS_SCHEDULE.Data; 
using OTS_SCHEDULE.DTO; 
using OTS_SCHEDULE.Interface; 

namespace OTS_SCHEDULE.Services
{
    // Inheriting from BackgroundService makes this run continuously
    public class ExpirationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpirationWorker> _logger;

        // We inject the ScopeFactory just like we did in the RabbitMQ Listener!
        public ExpirationWorker(IServiceScopeFactory scopeFactory, ILogger<ExpirationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("--> Expiration Worker Started. Polling every 10 seconds...");

            // This loop runs forever until the Docker container shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                try 
                {
                    // Attempt to process orders
                    await ProcessExpiredOrders();
                }
                catch (Exception ex)
                {
                    // THE FIX: Catch the DB timeout so the worker doesn't completely die!
                    // If the DB is overwhelmed by the k6 load test, we just wait for the next tick.
                    _logger.LogWarning($"--> [Worker Warning] Database overloaded or timed out: {ex.Message}");
                }
                
                // Pause for 10 seconds before checking again (perfect for your load test)
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessExpiredOrders()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                // Safely open the database and grab the megaphone
                var context = scope.ServiceProvider.GetRequiredService<SchdlDbContext>();
                var messageBusClient = scope.ServiceProvider.GetRequiredService<IMessageBusClient>();

                // 1. THE QUERY: Find tasks that are waiting, where the time has passed!
                var expiredTasks = await context.ScheduleTasks
                    .Where(s => s.Status == "WAITING_EXECUTION" && s.ExecuteAt <= DateTime.UtcNow)
                    .ToListAsync();

                if (expiredTasks.Any())
                {
                    _logger.LogInformation($"--> Found {expiredTasks.Count} expired orders. Processing...");

                    foreach (var task in expiredTasks)
                    {
                        // 2. Lock it down so we don't double-process it
                        task.Status = "EXPIRED";

                        // 3. Pack the envelope
                        var payload = new { OrderNo = task.OrderNo }; // Simple payload

                        var envelope = new IntegrationRabbitMqHandlerObj
                        {
                            SystemFrom = "OTS_SCHEDULE",
                            IntegrationMapCode = "ORDER_EXPIRED",
                            TrxNo = task.OrderNo,
                            Payload = JsonSerializer.Serialize(payload)
                        };

                        // 4. Shout to RabbitMQ!
                        messageBusClient.PublishMessage(envelope);
                        
                        _logger.LogInformation($"--> Published ORDER_EXPIRED for Trx: {task.OrderNo}");
                    }

                    // 5. Save all the status updates to the database at once
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}