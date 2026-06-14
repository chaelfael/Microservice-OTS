using OTS_ORDER.Dtos;

namespace OTS_ORDER.Interface
{

    public interface IMessageBusClient
    {

        void PublishMessage(IntegrationRabbitMqHandlerObj messageEnvelope);
    }
}