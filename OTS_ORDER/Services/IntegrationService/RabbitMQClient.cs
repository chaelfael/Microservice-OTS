using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using OTS_ORDER.Interface;
using OTS_ORDER.Dtos;

namespace OTS_ORDER.Services
{
    public class RabbitMQClient : IMessageBusClient, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        // REMOVED: private readonly IModel _channel; <-- This was the cause of the deadlock!

        public RabbitMQClient(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // 1. Manually build the connection using appsettings.json
            var factory = new ConnectionFactory() 
            { 
                HostName = _configuration["RabbitMQHost"], 
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try 
            {
                // 2. Open the TCP Connection ONCE when the app starts (This is thread-safe)
                _connection = factory.CreateConnection();

                // We only need a temporary channel here just to declare the exchange
                using var channel = _connection.CreateModel();
                channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                
                Console.WriteLine("--> Connected to Message Bus");
            } 
            catch (Exception ex) 
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public void PublishMessage(IntegrationRabbitMqHandlerObj messageEnvelope)
        {
            var message = JsonSerializer.Serialize(messageEnvelope);

            if (_connection != null && _connection.IsOpen) 
            {
                // THE FIX: Open a fresh, thread-safe channel for every single request
                using var channel = _connection.CreateModel();
                
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "trigger", 
                                     routingKey: "", 
                                     basicProperties: null, 
                                     body: body);
                                     
                // We don't log success here during load tests to save CPU I/O!
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connection is closed, not sending.");
            }
        }

        // Clean up connections when the application shuts down
        public void Dispose()
        {
            Console.WriteLine("--> Disposing Message Bus connection...");
            if (_connection != null && _connection.IsOpen)
            {
                // We no longer have a global channel to close, just the connection
                _connection.Close();
            }
        }
    }
}