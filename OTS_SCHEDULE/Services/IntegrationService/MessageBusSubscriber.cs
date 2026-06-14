using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OTS_SCHEDULE.DTO;
using OTS_SCHEDULE.Interface;
using OTS_SCHEDULE.Dtos;

namespace OTS_SCHEDULE.Services.IntegrationService
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory; // 1. Add the field
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;

        // 2. Inject IServiceScopeFactory into the constructor
        public MessageBusSubscriber(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                _queueName = _channel.QueueDeclare().QueueName;

                _channel.QueueBind(queue: _queueName,
                                 exchange: "trigger",
                                 routingKey: "");

                Console.WriteLine("--> Category Service is listening on the Message Bus...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to Message Bus: {ex.Message}");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            // 3. Add the 'async' keyword to this lambda so you can use 'await' inside!
            consumer.Received += async (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Envelope Received!");

                var body = ea.Body.ToArray();
                var notificationMessage = Encoding.UTF8.GetString(body);

                var envelope = JsonSerializer.Deserialize<IntegrationRabbitMqHandlerObj>(notificationMessage);

                if (envelope != null)
                {
                    Console.WriteLine($"--> Routing Code: {envelope.IntegrationMapCode}");
                    Console.WriteLine($"--> Transaction No: {envelope.TrxNo}");

                    switch (envelope.IntegrationMapCode)
                    {
                        case "ORDER_INITIATED":
                            Console.WriteLine($"--> Routing to: ProcessNewOrder (Trx: {envelope.TrxNo})");

                            var orderData = JsonSerializer.Deserialize<OrderInitiatedEventObj>(envelope.Payload);

                            if (orderData != null)
                            {
                                // 4. We use the injected _scopeFactory here!
                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var ScheduleService = scope.ServiceProvider.GetRequiredService<ISchdlTrService>();

                                    await ScheduleService.CreateSchedule(orderData);
                                }
                            }
                            break;
                        case "PAYMENT_SUCCESS":
                            Console.WriteLine($"--> Routing to: ProcessPaymentSuccess (Trx: {envelope.TrxNo})");

                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var ScheduleService = scope.ServiceProvider.GetRequiredService<ISchdlTrService>();

                                    await ScheduleService.UpdateScheduleStatus(envelope.TrxNo, "EXECUTED");
                                }
                            
                            break;
                        case "ORDER_CANCELLED":
                            Console.WriteLine($"--> Routing to: ProcessOrderCancelled (Trx: {envelope.TrxNo})");

                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var ScheduleService = scope.ServiceProvider.GetRequiredService<ISchdlTrService>();

                                    await ScheduleService.UpdateScheduleStatus(envelope.TrxNo, "CANCELLED");
                                }
                            
                            break;

                        default:
                            Console.WriteLine($"--> Ignored message with Routing Code: {envelope.IntegrationMapCode}");
                            break;
                    
                    }
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
            base.Dispose();
        }
    }
}