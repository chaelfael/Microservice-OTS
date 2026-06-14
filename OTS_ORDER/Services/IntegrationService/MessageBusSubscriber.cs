using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OTS_ORDER.Dtos;
using OTS_ORDER.Interface;

namespace OTS_ORDER.Services.IntegrationService
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
            if (_channel == null)
    {
        Console.WriteLine("--> Cannot listen: RabbitMQ Channel is null. Check connection logs.");
        return Task.CompletedTask;
    }

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
                        case "AFTER_ORDER_INITIATED":
                            Console.WriteLine($"--> Routing to: ProcessAfterOrder (Trx: {envelope.TrxNo})");

                            var orderData = JsonSerializer.Deserialize<AfterOrderInitiatedEventObj>(envelope.Payload);

                            if (orderData != null)
                            {
                                // 4. We use the injected _scopeFactory here!
                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderTrService>();

                                    await orderService.AddOrderDetails(orderData, envelope.TrxNo);
                                }
                            }
                            break;
                        case "ORDER_EXPIRED":
                            Console.WriteLine($"--> Routing to: ProcessNewOrder (Trx: {envelope.TrxNo})");

                            var expireData = JsonSerializer.Deserialize<ExpireCancelOrderPayload>(envelope.Payload);

                            if (expireData != null)
                            {
                                // 4. We use the injected _scopeFactory here!
                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderTrService>();

                                    await orderService.ExpireOrder(expireData);
                                }
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