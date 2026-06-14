using OTS_INV.DTO;

namespace OTS_INV.Interface
{

    public interface IMessageBusClient
    {

        void PublishMessage(IntegrationRabbitMqHandlerObj messageEnvelope);
    }
}